namespace GuiLabs.FileUtilities
{
    public static class Paths
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

        public static string TrimQuotes(string path)
        {
            if (path.Length > 2 && path[0] == '"' && path[path.Length - 1] == '"')
            {
                path = path.Substring(1, path.Length - 2);
            }

            return path;
        }
    }
}
