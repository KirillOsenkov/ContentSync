using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GuiLabs.FileUtilities
{
    public class Folder
    {
        public string Name { get; }
        public IList<Folder> Folders { get; }
        public IList<File> Files { get; }

        public Folder(string name, IEnumerable<Folder> folders, IEnumerable<File> files)
        {
            Name = name;
            Folders = folders.OrderBy(f => f.Name).ToArray();
            Files = files.OrderBy(f => f.Name).ToArray();
        }

        public void CreateOnDisk(string parentFolder)
        {
            var fullPath = Path.Combine(parentFolder, Name);
            Directory.CreateDirectory(fullPath);
            foreach (var folder in Folders)
            {
                folder.CreateOnDisk(fullPath);
            }

            foreach (var file in Files)
            {
                file.CreateOnDisk(fullPath);
            }
        }

        public static Folder FromDisk(string folderPath)
        {
            var name = Path.GetFileName(folderPath);
            var folders = Directory.GetDirectories(folderPath);
            var files = Directory.GetFiles(folderPath);
            return new Folder(
                name,
                folders.Select(f => FromDisk(f)),
                files.Select(f => new File(
                    Path.GetFileName(f),
                    System.IO.File.ReadAllText(f))));
        }

        /// <summary>
        /// Folders can be considered identical if the names are different.
        /// Equality is content based.
        /// </summary>
        public static bool AreIdentical(Folder left, Folder right)
        {
            if (left == null || right == null)
            {
                return false;
            }

            if (left.Folders.Count != right.Folders.Count)
            {
                return false;
            }

            for (int i = 0; i < left.Folders.Count; i++)
            {
                if (!string.Equals(left.Folders[i].Name, right.Folders[i].Name, System.StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                if (!AreIdentical(left.Folders[i], right.Folders[i]))
                {
                    return false;
                }
            }

            if (left.Files.Count != right.Files.Count)
            {
                return false;
            }

            for (int i = 0; i < left.Files.Count; i++)
            {
                if (!File.AreIdentical(left.Files[i], right.Files[i]))
                {
                    return false;
                }
            }

            return true;
        }
    }

    public class File
    {
        public string Name { get; }
        public string Content { get; }

        public File(string name, string content = "")
        {
            Name = name;
            Content = content;
        }

        public void CreateOnDisk(string folder)
        {
            var filePath = Path.Combine(folder, Name);
            System.IO.File.WriteAllText(filePath, Content ?? "");
        }

        public static bool AreIdentical(File left, File right)
        {
            if (left == null || right == null)
            {
                return false;
            }

            if (!string.Equals(left.Name, right.Name, System.StringComparison.OrdinalIgnoreCase) || left.Content != right.Content)
            {
                return false;
            }

            return true;
        }
    }

    public class Builder
    {
        public static Folder Folder(params object[] items)
        {
            string name = (string)items.Single(o => o is string);
            return new Folder(name, items.OfType<Folder>(), items.OfType<File>());
        }

        public static File File(string name, string content = "")
        {
            return new File(name, content);
        }
    }
}
