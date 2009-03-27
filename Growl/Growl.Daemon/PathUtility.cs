using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Growl.Daemon
{
    /// <summary>
    /// Provides utilities for working with file and folder paths
    /// </summary>
    public static class PathUtility
    {
        /// <summary>
        /// Gets a folder name consisting of path-safe characters.
        /// </summary>
        /// <param name="name">The string to base the folder name on</param>
        /// <returns><paramref name="name"/> with any invalid characters removed</returns>
        public static string GetSafeFolderName(string name)
        {
            char[] disallowedChars = Path.GetInvalidPathChars();
            return GetSafeName(name, disallowedChars);
        }

        /// <summary>
        /// Gets a file name consisting of filename-safe characters.
        /// </summary>
        /// <param name="name">The string to base the filename on</param>
        /// <returns><paramref name="name"/> with any invalid characters removed</returns>
        public static string GetSafeFileName(string name)
        {
            char[] disallowedChars = Path.GetInvalidFileNameChars();
            return GetSafeName(name, disallowedChars);
        }

        /// <summary>
        /// Removes any <paramref name="disallowedChars"/> in <paramref name="name"/>
        /// and returns the resulting string.
        /// </summary>
        /// <param name="name">The string to base the safe name on</param>
        /// <param name="disallowedChars">array of <see cref="char"/>s to replace</param>
        /// <returns></returns>
        private static string GetSafeName(string name, char[] disallowedChars)
        {
            string safe = name;
            foreach (char disallowed in disallowedChars)
            {
                safe = safe.Replace(disallowed.ToString(), "");
            }
            return safe;
        }

        /// <summary>
        /// Ensures that the given path exists, creating it if necessary.
        /// </summary>
        /// <param name="path">The path to check</param>
        public static void EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        /// <summary>
        /// Combines two portions of a path into one valid path.
        /// </summary>
        /// <param name="path1">The beginning of the path</param>
        /// <param name="path2">The end of the path</param>
        /// <returns>Full path</returns>
        /// <remarks>
        /// If <paramref name="path1"/> does not end in a trailing slash, it 
        /// will be added. <paramref name="path2"/> is not modified (and thus
        /// can be a filename or folder path).
        /// </remarks>
        public static string Combine(string path1, string path2)
        {
            return System.IO.Path.Combine(path1, path2);
        }
    }
}
