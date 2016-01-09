using System.Collections.Generic;
using System.Linq;

namespace GuiLabs.FileUtilities
{
    public class FolderDiffResults
    {
        public IEnumerable<string> LeftOnlyFiles { get; private set; }
        public IEnumerable<string> LeftOnlyFolders { get; private set; }
        public IEnumerable<string> IdenticalFiles { get; private set; }
        public IEnumerable<string> ChangedFiles { get; private set; }
        public IEnumerable<string> RightOnlyFiles { get; private set; }
        public IEnumerable<string> RightOnlyFolders { get; private set; }

        public FolderDiffResults(
            IEnumerable<string> leftOnlyFiles,
            IEnumerable<string> identicalFiles,
            IEnumerable<string> changedFiles,
            IEnumerable<string> rightOnlyFiles,
            IEnumerable<string> leftOnlyFolders,
            IEnumerable<string> rightOnlyFolders)
        {
            LeftOnlyFiles = leftOnlyFiles;
            IdenticalFiles = identicalFiles;
            ChangedFiles = changedFiles;
            RightOnlyFiles = rightOnlyFiles;
            LeftOnlyFolders = leftOnlyFolders;
            RightOnlyFolders = rightOnlyFolders;
        }

        public bool AreFullyIdentical
        {
            get
            {
                return
                    !LeftOnlyFiles.Any() &&
                    !RightOnlyFiles.Any() &&
                    !ChangedFiles.Any() &&
                    !LeftOnlyFolders.Any() &&
                    !RightOnlyFolders.Any();
            }
        }
    }
}
