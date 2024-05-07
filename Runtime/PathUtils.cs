using System;
using System.IO;

namespace FizzSDK.Utils
{
    public static class PathUtils
    {
        // Source: https://gist.github.com/AlexeyMz/183b3ab2c4dbb0a7de5b
        public static string MakeRelativePath(string absolutePath, string pivotFolder)
        {
            //string folder = Path.IsPathRooted(pivotFolder)
            //    ? pivotFolder : Path.GetFullPath(pivotFolder);
            var folder = pivotFolder;
            var pathUri = new Uri(absolutePath);
            // Folders must end in a slash
            if (!folder.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                folder += Path.DirectorySeparatorChar;
            }
            var folderUri = new Uri(folder);
            var relativeUri = folderUri.MakeRelativeUri(pathUri);
            return Uri.UnescapeDataString(
                relativeUri.ToString().Replace('/', Path.DirectorySeparatorChar));
        }   
    }
}