using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Growl.Connector;
using Growl.CoreLibrary;

namespace Growl.Daemon
{
    /// <summary>
    /// Provides methods for getting and storing binary resources in a file-based cache.
    /// </summary>
    /// <remarks>
    /// Each application's resources will be cached in a seperate sub-folder inside of the
    /// cache.
    /// </remarks>
    internal static class ResourceCache
    {
        /// <summary>
        /// The file extension for resource files
        /// </summary>
        private const string EXTENSION = ".data";

        /// <summary>
        /// Cache pointers
        /// </summary>
        private static Dictionary<string, Dictionary<string, string>> cache;

        /// <summary>
        /// Indicates if the cache is enabled or not
        /// </summary>
        private static bool enabled;

        /// <summary>
        /// The physical path to where resources will be saved
        /// </summary>
        private static string resourceFolder;

        /// <summary>
        /// File search pattern filter
        /// </summary>
        private static string filter = String.Format("*{0}", EXTENSION);

        /// <summary>
        /// Type initializer
        /// </summary>
        static ResourceCache()
        {
            PurgeCache();
        }

        /// <summary>
        /// Indicates if the resource cache is enabled or not
        /// </summary>
        /// <value>
        /// <c>true</c> if resources are cached to disk,
        /// <c>false otherwise</c>
        /// </value>
        /// <remarks>
        /// Caching resources to disk can help speed up the server by allowing it to skip reading
        /// inline binary data that it already has cached.
        /// </remarks>
        public static bool Enabled
        {
            get
            {
                return enabled;
            }
            set
            {
                enabled = value;
            }
        }

        /// <summary>
        /// The physical path to the folder where resources will be cached.
        /// </summary>
        /// <value>
        /// string - Ex: c:\temp\
        /// </value>
        public static string ResourceFolder
        {
            get
            {
                return resourceFolder;
            }
            set
            {
                if (value != resourceFolder)
                {
                    ReadCacheFromDisk(value);
                    resourceFolder = value;
                }
            }
        }

        /// <summary>
        /// Indicates if the specified resource is already cached
        /// </summary>
        /// <param name="applicationName">The application that owns the resource</param>
        /// <param name="resourceID">The resource ID</param>
        /// <returns>
        /// <c>true</c> if the resource is cached,
        /// <c>false</c> otherwise
        /// </returns>
        public static bool IsCached(string applicationName, string resourceID)
        {
            if (IsCacheConfigured)
            {
                string filename = GetResourceFilename(applicationName, resourceID);
                return File.Exists(filename);
            }
            return false;
        }

        /// <summary>
        /// Gets the specified from the cache
        /// </summary>
        /// <param name="applicationName">The application that owns the resource</param>
        /// <param name="resourceID">The resource ID</param>
        /// <returns>
        /// <see cref="BinaryData"/> if the resource exists in the cache, <c>null</c> otherwise
        /// </returns>
        public static BinaryData Get(string applicationName, string resourceID)
        {
            BinaryData binaryData = null;
            if (IsCacheConfigured)
            {
                string filename = GetResourceFilename(applicationName, resourceID);
                if (!String.IsNullOrEmpty(filename))
                {
                    byte[] bytes = ReadFile(filename);
                    if (bytes != null)
                        binaryData = new BinaryData(resourceID, bytes);
                }
            }
            return binaryData;
        }

        /// <summary>
        /// Adds the specified resource to the cache
        /// </summary>
        /// <param name="applicationName">The application that owns the resource</param>
        /// <param name="binaryData"><see cref="BinaryData"/> to cache</param>
        public static void Add(string applicationName, BinaryData binaryData)
        {
            if (IsCacheConfigured)
            {
                string path = GetApplicationCacheDirectoryName(applicationName);
                DirectoryInfo appDirectory = CreateAppCacheDirectory(path); // do this every time, in case the underlying filesystem has changed
                if (!cache.ContainsKey(appDirectory.FullName))
                {
                    cache.Add(appDirectory.FullName, new Dictionary<string, string>());
                }

                Dictionary<string, string> appCache = cache[appDirectory.FullName];
                if (appCache.ContainsKey(binaryData.ID)) appCache.Remove(binaryData.ID);


                string filename = String.Format(@"{0}{1}", binaryData.ID, EXTENSION);
                string filepath = PathUtility.Combine(appDirectory.FullName, filename);
                if (WriteFile(filepath, binaryData.Data))
                    appCache.Add(binaryData.ID, filepath);
            }
        }

        /// <summary>
        /// Indicates if the cache is configured for use
        /// </summary>
        /// <value>
        /// <c>true</c> if the cache is enabled and has a valid ResourceFolder set,
        /// <c>false otherwise</c>
        /// </value>
        private static bool IsCacheConfigured
        {
            get
            {
                return Enabled && !String.IsNullOrEmpty(ResourceFolder);
            }
        }

        /// <summary>
        /// Gets the application-specific cache folder name
        /// </summary>
        /// <param name="applicationName">The application that owns the resources</param>
        /// <returns>string - folder path</returns>
        private static string GetApplicationCacheDirectoryName(string applicationName)
        {
            string safeAppName = PathUtility.GetSafeFolderName(applicationName);
            string path = PathUtility.Combine(resourceFolder, safeAppName);
            return path;
        }

        /// <summary>
        /// Gets the full physical path to a cached resource
        /// </summary>
        /// <param name="applicationName">The application that owns the resource</param>
        /// <param name="resourceID">The resource ID</param>
        /// <returns>string - file path</returns>
        private static string GetResourceFilename(string applicationName, string resourceID)
        {
            string filename = null;
            if (IsCacheConfigured)
            {
                string appCacheDirectory = GetApplicationCacheDirectoryName(applicationName);
                if (cache.ContainsKey(appCacheDirectory))
                {
                    Dictionary<string, string> appCache = cache[appCacheDirectory];
                    if (appCache != null && appCache.ContainsKey(resourceID))
                    {
                        filename = appCache[resourceID];
                    }
                }
            }
            return filename;
        }

        /// <summary>
        /// Ensures that the app-specific cache directory is created
        /// </summary>
        /// <param name="path">The full path to the app-specific cache</param>
        /// <returns><see cref="DirectoryInfo"/></returns>
        private static DirectoryInfo CreateAppCacheDirectory(string path)
        {
            PathUtility.EnsureDirectoryExists(path);
            DirectoryInfo d = new DirectoryInfo(path);
            return d;
        }

        /// <summary>
        /// Reads the contents of a file
        /// </summary>
        /// <param name="path">The path of the file to read</param>
        /// <returns>array of bytes</returns>
        private static byte[] ReadFile(string path)
        {
            byte[] bytes = null;
            try
            {
                bytes = File.ReadAllBytes(path);
            }
            catch
            {
            }
            return bytes;
        }

        /// <summary>
        /// Writes the specified bytes to a file
        /// </summary>
        /// <param name="path">The path of the file to write to</param>
        /// <param name="bytes">The data to write</param>
        /// <returns>
        /// <c>true</c> if the data was successfully written,
        /// <c>false</c> otherwise
        /// </returns>
        private static bool WriteFile(string path, byte[] bytes)
        {
            try
            {
                File.WriteAllBytes(path, bytes);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Purges all cache pointers
        /// </summary>
        /// <remarks>
        /// This method does not delete the underlying files.
        /// </remarks>
        private static void PurgeCache()
        {
            cache = new Dictionary<string, Dictionary<string, string>>();
        }

        /// <summary>
        /// Reads exisiting files into the cache
        /// </summary>
        /// <param name="path">The physical path to the cache</param>
        private static void ReadCacheFromDisk(string path)
        {
            PurgeCache();

            DirectoryInfo cacheDirectory = new DirectoryInfo(path);
            DirectoryInfo[] appDirectories = cacheDirectory.GetDirectories();
            foreach (DirectoryInfo appDirectory in appDirectories)
            {
                cache.Add(appDirectory.FullName, new Dictionary<string, string>());
                Dictionary<string, string> appCache = cache[appDirectory.FullName];

                FileInfo[] files = appDirectory.GetFiles(filter);
                foreach (FileInfo file in files)
                {
                    appCache.Add(Path.GetFileNameWithoutExtension(file.FullName), file.FullName);
                }
            }
        }
    }
}
