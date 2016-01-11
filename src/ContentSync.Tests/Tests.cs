using System.IO;
using System.Reflection;
using Xunit;
using static GuiLabs.FileUtilities.Builder;

namespace GuiLabs.FileUtilities
{
    public class ContentSyncTests
    {
        [Fact]
        public void TestSyncBasic()
        {
            T(
                Folder("A",
                    File("a.txt", "123")),
                Folder("B"));
            T(
                Folder("A",
                    File("a.txt", "123")),
                Folder("B",
                    File("a.txt", "123")));
            T(
                Folder("A",
                    File("a.txt", "123")),
                Folder("B",
                    File("a.txt", "124")));
            T(
                Folder("A"),
                Folder("B",
                    File("a.txt", "123")));
            T(
                Folder("A",
                    Folder("A1",
                        File("A1.txt", "A1"),
                        File("A12.txt", "A12")),
                    Folder("A2",
                        File("A2.txt", "A2"))),
                Folder("B",
                    File("a.txt", "123")));
            T(
                Folder("A",
                    Folder("A1")),
                Folder("B"));
            T(
                Folder("A",
                    Folder("A1")),
                Folder("B",
                    Folder("unused1"),
                    Folder("unused2",
                        File("unused.txt", ""))));
            T(
                Folder("A",
                    Folder("A1",
                        Folder("A11"))),
                Folder("B",
                    Folder("B1",
                        Folder("B11"),
                        Folder("B12"))));
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

            var areIdentical = Folder.AreIdentical(source, actual);
            Assert.True(areIdentical);

            Directory.Delete(root, recursive: true);
        }
    }
}