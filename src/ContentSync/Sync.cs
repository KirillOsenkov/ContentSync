using System;
using System.IO;
using System.Linq;
using static GuiLabs.Common.Utilities;

namespace GuiLabs.FileUtilities
{
    public class Sync
    {
        /// <summary>
        /// Assumes source directory exists. destination may or may not exist.
        /// </summary>
        public static void Directories(string source, string destination, Arguments arguments)
        {
            if (!Directory.Exists(destination))
            {
                FileSystem.CreateDirectory(destination, arguments.WhatIf);
            }

            source = Paths.TrimSeparator(source);
            destination = Paths.TrimSeparator(destination);

            var diff = Folders.DiffFolders(
                source,
                destination,
                arguments.Pattern,
                recursive: !arguments.Nonrecursive,
                compareContents:
                    arguments.UpdateChangedFiles ||
                    arguments.DeleteChangedFiles ||
                    arguments.DeleteSameFiles);

            bool changesMade = false;
            int filesFailedToCopy = 0;
            int filesFailedToDelete = 0;
            int foldersFailedToCreate = 0;
            int foldersFailedToDelete = 0;

            if (arguments.CopyLeftOnlyFiles)
            {
                using (Log.MeasureTime("Copying new files"))
                {
                    foreach (var leftOnly in diff.LeftOnlyFiles)
                    {
                        var destinationFilePath = destination + leftOnly;
                        if (!FileSystem.CopyFile(source + leftOnly, destinationFilePath, arguments.WhatIf))
                        {
                            filesFailedToCopy++;
                        }

                        changesMade = true;
                    }
                }
            }

            if (arguments.UpdateChangedFiles)
            {
                using (Log.MeasureTime("Updating changed files"))
                {
                    foreach (var changed in diff.ChangedFiles)
                    {
                        var destinationFilePath = destination + changed;
                        if (!FileSystem.CopyFile(source + changed, destinationFilePath, arguments.WhatIf))
                        {
                            filesFailedToCopy++;
                        }

                        changesMade = true;
                    }
                }
            }
            else if (arguments.DeleteChangedFiles)
            {
                using (Log.MeasureTime("Deleting changed files"))
                {
                    foreach (var changed in diff.ChangedFiles)
                    {
                        var destinationFilePath = destination + changed;
                        if (!FileSystem.DeleteFile(destinationFilePath, arguments.WhatIf))
                        {
                            filesFailedToDelete++;
                        }

                        changesMade = true;
                    }
                }
            }

            if (arguments.DeleteSameFiles)
            {
                using (Log.MeasureTime("Deleting identical files"))
                {
                    foreach (var same in diff.IdenticalFiles)
                    {
                        var destinationFilePath = destination + same;
                        if (!FileSystem.DeleteFile(destinationFilePath, arguments.WhatIf))
                        {
                            filesFailedToDelete++;
                        }

                        changesMade = true;
                    }
                }
            }

            if (arguments.DeleteRightOnlyFiles)
            {
                using (Log.MeasureTime("Deleting extra files"))
                {
                    foreach (var rightOnly in diff.RightOnlyFiles)
                    {
                        var deletedFilePath = destination + rightOnly;
                        if (!FileSystem.DeleteFile(deletedFilePath, arguments.WhatIf))
                        {
                            filesFailedToDelete++;
                        }

                        changesMade = true;
                    }
                }
            }

            int foldersCreated = 0;
            if (arguments.CopyEmptyDirectories)
            {
                using (Log.MeasureTime("Creating folders"))
                {
                    foreach (var leftOnlyFolder in diff.LeftOnlyFolders)
                    {
                        var newFolder = destination + leftOnlyFolder;
                        if (!Directory.Exists(newFolder))
                        {
                            if (!FileSystem.CreateDirectory(newFolder, arguments.WhatIf))
                            {
                                foldersFailedToCreate++;
                            }
                            else
                            {
                                foldersCreated++;
                            }

                            changesMade = true;
                        }
                    }
                }
            }

            int foldersDeleted = 0;
            if (arguments.DeleteRightOnlyDirectories)
            {
                using (Log.MeasureTime("Deleting folders"))
                {
                    foreach (var rightOnlyFolder in diff.RightOnlyFolders)
                    {
                        var deletedFolderPath = destination + rightOnlyFolder;
                        if (Directory.Exists(deletedFolderPath))
                        {
                            if (!FileSystem.DeleteDirectory(deletedFolderPath, arguments.WhatIf))
                            {
                                foldersFailedToDelete++;
                            }
                            else
                            {
                                foldersDeleted++;
                            }

                            changesMade = true;
                        }
                    }
                }
            }

            if (diff.LeftOnlyFiles.Any() && arguments.CopyLeftOnlyFiles)
            {
                var count = diff.LeftOnlyFiles.Count();
                var fileOrFiles = Pluralize("file", count);
                if (arguments.WhatIf)
                {
                    Log.WriteLine($"Would have copied {count} new {fileOrFiles}", ConsoleColor.Green);
                }
                else
                {
                    Log.WriteLine($"{count} new {fileOrFiles} copied", ConsoleColor.Green);
                }
            }

            if (foldersCreated > 0 && arguments.CopyEmptyDirectories)
            {
                var folderOrFolders = Pluralize("folder", foldersCreated);
                if (arguments.WhatIf)
                {
                    Log.WriteLine($"Would have created {foldersCreated} {folderOrFolders}", ConsoleColor.Green);
                }
                else
                {
                    Log.WriteLine($"{foldersCreated} {folderOrFolders} created", ConsoleColor.Green);
                }
            }

            if (diff.ChangedFiles.Any() && arguments.UpdateChangedFiles)
            {
                var count = diff.ChangedFiles.Count();
                var fileOrFiles = Pluralize("file", count);
                if (arguments.WhatIf)
                {
                    Log.WriteLine($"Would have updated {count} changed {fileOrFiles}", ConsoleColor.Yellow);
                }
                else
                {
                    Log.WriteLine($"{count} changed {fileOrFiles} updated", ConsoleColor.Yellow);
                }
            }

            if (diff.ChangedFiles.Any() && arguments.DeleteChangedFiles)
            {
                var count = diff.ChangedFiles.Count();
                var fileOrFiles = Pluralize("file", count);
                if (arguments.WhatIf)
                {
                    Log.WriteLine($"Would have deleted {count} changed {fileOrFiles}", ConsoleColor.Yellow);
                }
                else
                {
                    Log.WriteLine($"{count} changed {fileOrFiles} deleted", ConsoleColor.Yellow);
                }
            }

            if (diff.RightOnlyFiles.Any() && arguments.DeleteRightOnlyFiles)
            {
                var count = diff.RightOnlyFiles.Count();
                var fileOrFiles = Pluralize("file", count);
                if (arguments.WhatIf)
                {
                    Log.WriteLine($"Would have deleted {count} right-only {fileOrFiles}", ConsoleColor.Red);
                }
                else
                {
                    Log.WriteLine($"{count} right-only {fileOrFiles} deleted", ConsoleColor.Red);
                }
            }

            if (foldersDeleted > 0 && arguments.DeleteRightOnlyDirectories)
            {
                var folderOrFolders = Pluralize("folder", foldersDeleted);
                if (arguments.WhatIf)
                {
                    Log.WriteLine($"Would have deleted {foldersDeleted} right-only {folderOrFolders}", ConsoleColor.Red);
                }
                else
                {
                    Log.WriteLine($"{foldersDeleted} right-only {folderOrFolders} deleted", ConsoleColor.Red);
                }
            }

            if (diff.IdenticalFiles.Any())
            {
                var count = diff.IdenticalFiles.Count();
                var fileOrFiles = Pluralize("file", count);
                if (arguments.DeleteSameFiles)
                {
                    if (arguments.WhatIf)
                    {
                        Log.WriteLine($"Would have deleted {count} identical {fileOrFiles} from destination", ConsoleColor.White);
                    }
                    else
                    {
                        Log.WriteLine($"{count} identical {fileOrFiles} deleted from destination", ConsoleColor.White);
                    }
                }
                else
                {
                    Log.WriteLine($"{count} identical {fileOrFiles}", ConsoleColor.White);
                }
            }

            if (filesFailedToCopy > 0)
            {
                Log.WriteLine($"Failed to copy {filesFailedToCopy} {Pluralize("file", filesFailedToCopy)}", ConsoleColor.Red);
            }

            if (filesFailedToDelete > 0)
            {
                Log.WriteLine($"Failed to delete {filesFailedToDelete} {Pluralize("file", filesFailedToDelete)}.", ConsoleColor.Red);
            }

            if (foldersFailedToCreate > 0)
            {
                Log.WriteLine($"Failed to create {foldersFailedToCreate} {Pluralize("folder", foldersFailedToCreate)}.", ConsoleColor.Red);
            }

            if (foldersFailedToDelete > 0)
            {
                Log.WriteLine($"Failed to delete {foldersFailedToDelete} {Pluralize("folder", foldersFailedToDelete)}.", ConsoleColor.Red);
            }

            if (!changesMade)
            {
                if (arguments.WhatIf)
                {
                    Log.WriteLine("Would have made no changes.", ConsoleColor.White);
                }
                else
                {
                    Log.WriteLine("Made no changes.", ConsoleColor.White);
                }
            }

            // if there were no errors, delete the cache of the folder contents. Otherwise
            // chances are they're going to restart the process, so we might needs the cache
            // next time.
            if (filesFailedToCopy == 0 &&
                filesFailedToDelete == 0 &&
                foldersFailedToCreate == 0 &&
                foldersFailedToDelete == 0)
            {
                DirectoryContentsCache.ClearWrittenFilesFromCache();
            }
        }

        /// <summary>
        /// Assumes source exists, destination may or may not exist.
        /// If it exists and is identical bytes to source, nothing is done.
        /// If it exists and is different, it is overwritten.
        /// If it doesn't exist, source is copied.
        /// </summary>
        public static void Files(string source, string destination, Arguments arguments)
        {
            if (File.Exists(destination) && FileUtilities.Files.AreContentsIdentical(source, destination))
            {
                Log.WriteLine("File contents are identical.", ConsoleColor.White);
                return;
            }

            FileSystem.CopyFile(source, destination, arguments.WhatIf);
        }
    }
}