using System;
using System.Collections.Generic;
using System.Linq;

namespace GuiLabs.FileUtilities
{
    public class Arguments
    {
        private string[] args;

        public string Source { get; set; }
        public string Destination { get; set; }
        public string Pattern { get; set; } = "*";
        public bool Nonrecursive { get; set; }

        public bool CopyLeftOnlyFiles { get; private set; } = true;
        public bool UpdateChangedFiles { get; private set; } = true;
        public bool DeleteRightOnlyFiles { get; private set; } = true;
        public bool CopyEmptyDirectories { get; private set; } = true;
        public bool DeleteRightOnlyDirectories { get; private set; } = true;
        public bool DeleteSameFiles { get; private set; }
        public bool DeleteChangedFiles { get; private set; }
        public bool WhatIf { get; set; }
        public bool Quiet { get; set; }
        public bool Help { get; set; }

        public string Error { get; set; } = "";

        public Arguments()
        {
            args = new string[0];
        }

        public Arguments(params string[] args)
        {
            this.args = args;
            Parse();
        }

        private void Parse()
        {
            var switches = new List<string>();
            var paths = new List<string>();

            foreach (var arg in args)
            {
                if (arg.StartsWith("/") || arg.StartsWith("-"))
                {
                    switches.Add(arg.Substring(1).ToLowerInvariant());
                }
                else
                {
                    paths.Add(Paths.TrimQuotes(arg));
                }
            }

            if (switches.Any())
            {
                if (switches.Where(s => s != "q" && s != "whatif").Any())
                {
                    // if any of c, u, d, ds or dc are specified, they're in explicit mode, assume all defaults false
                    CopyLeftOnlyFiles = false;
                    UpdateChangedFiles = false;
                    DeleteRightOnlyFiles = false;
                    CopyEmptyDirectories = false;
                    DeleteRightOnlyDirectories = false;
                }

                foreach (var key in switches)
                {
                    switch (key)
                    {
                        case "c":
                            CopyLeftOnlyFiles = true;
                            break;
                        case "u":
                            UpdateChangedFiles = true;
                            break;
                        case "cu":
                            CopyLeftOnlyFiles = true;
                            UpdateChangedFiles = true;
                            break;
                        case "d":
                            DeleteRightOnlyFiles = true;
                            break;
                        case "cd":
                            CopyLeftOnlyFiles = true;
                            DeleteRightOnlyFiles = true;
                            break;
                        case "cud":
                            CopyLeftOnlyFiles = true;
                            UpdateChangedFiles = true;
                            DeleteRightOnlyFiles = true;
                            break;
                        case "ud":
                            UpdateChangedFiles = true;
                            DeleteRightOnlyFiles = true;
                            break;
                        case "ds":
                            DeleteSameFiles = true;
                            break;
                        case "dc":
                            DeleteChangedFiles = true;
                            break;
                        case "n":
                            Nonrecursive = true;
                            break;
                        case "q":
                            Quiet = true;
                            Log.Quiet = true;
                            break;
                        case "h":
                        case "help":
                        case "?":
                            Help = true;
                            Error = "Help argument cannot be combined with any other arguments";
                            return;
                        case "whatif":
                            WhatIf = true;
                            if (switches.Count == 1)
                            {
                                // if whatif is the only switch, assume the defaults
                                CopyLeftOnlyFiles = true;
                                UpdateChangedFiles = true;
                                DeleteRightOnlyFiles = true;
                                CopyEmptyDirectories = true;
                                DeleteRightOnlyDirectories = true;
                            }

                            break;
                        default:
                            Error += "Unrecognized argument: " + key + Environment.NewLine;
                            return;
                    }
                }
            }

            if (Quiet && WhatIf)
            {
                Error = "-q and -whatif are incompatible. Choose one or the other.";
                return;
            }

            if (DeleteChangedFiles && UpdateChangedFiles)
            {
                Error = "Incompatible options: -u and -dc (can't update and delete changed files at the same time";
                return;
            }

            if (paths.Count < 2)
            {
                Error = "Two paths need to be specified (source and destination)";
                return;
            }

            if (paths.Count > 3)
            {
                Error = "Unable to process extra argument: " + paths[3];
                return;
            }

            if (paths.Count == 3)
            {
                var patterns = paths.Where(p => p.Contains("*") || p.Contains("?")).ToArray();
                if (patterns.Length != 1)
                {
                    Error = $"Expected exactly one file pattern, {patterns.Length} was specified";
                    return;
                }

                if (paths[2] != patterns[0])
                {
                    Error = "File pattern should be specified after Source and Destination arguments";
                    return;
                }

                Pattern = paths[2];
            }

            if (Pattern != "*")
            {
                // when we're not comparing all files, don't synchronize directories
                CopyEmptyDirectories = false;
                DeleteRightOnlyDirectories = false;
            }

            if (!CopyLeftOnlyFiles)
            {
                CopyEmptyDirectories = false;
            }

            if (!DeleteRightOnlyFiles)
            {
                DeleteRightOnlyDirectories = false;
            }

            Source = paths[0];
            Destination = paths[1];
        }
    }
}
