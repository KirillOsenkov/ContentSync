namespace GuiLabs.FileUtilities
{
    class Paths
    {
        public static string AppendSeparatorIfNeeded(string folderPath)
        {
            if (!folderPath.EndsWith("\\"))
            {
                folderPath += "\\";
            }

            return folderPath;
        }

        public static string TrimSeparator(string folderPath)
        {
            if (folderPath.EndsWith("\\"))
            {
                folderPath = folderPath.Substring(0, folderPath.Length - 1);
            }

            return folderPath;
        }
    }
}
