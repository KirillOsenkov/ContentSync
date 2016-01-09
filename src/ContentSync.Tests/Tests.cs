using System.IO;
using System.Reflection;
using Xunit;
using static GuiLabs.FileUtilities.Builder;

namespace GuiLabs.FileUtilities
{
    public class ContentSyncTests
    {
        [Fact]
        public void Test1()
        {
            T
            (
                Folder("A", File("a.txt", "123")),
                Folder("B")
            );
        }

        private void T(Folder source, Folder destination)
        {
            var entrypointDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var root = Path.Combine(entrypointDirectory, nameof(ContentSyncTests));
            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }

            Directory.CreateDirectory(root);

            source.CreateOnDisk(root);
            destination.CreateOnDisk(root);

            Sync.Directories(Path.Combine(root, source.Name), Path.Combine(root, destination.Name));

            var actual = Folder.FromDisk(Path.Combine(root, destination.Name));

            Assert.True(Folder.AreIdentical(source, actual));

            Directory.Delete(root, recursive: true);
        }
    }
}