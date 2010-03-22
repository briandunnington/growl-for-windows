using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using Growl.CoreLibrary;

namespace Growl
{
    /// <summary>
    /// Provides methods for getting and storing images in a file-based cache.
    /// </summary>
    /// <remarks>
    /// Each application's resources will be cached in a seperate sub-folder inside of the
    /// cache.
    /// </remarks>
    internal static class ImageCache
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
        /// The physical path to where resources will be saved
        /// </summary>
        private static string cacheFolder;

        /// <summary>
        /// File search pattern filter
        /// </summary>
        private static string filter = String.Format("*{0}", EXTENSION);

        /// <summary>
        /// Type initializer
        /// </summary>
        static ImageCache()
        {
            try
            {
                PurgeCache();
            }
            catch
            {
            }
        }

        /// <summary>
        /// The physical path to the folder where resources will be cached.
        /// </summary>
        /// <value>
        /// string - Ex: c:\temp\
        /// </value>
        public static string CacheFolder
        {
            get
            {
                return cacheFolder;
            }
            set
            {
                if (value != cacheFolder)
                {
                    Growl.CoreLibrary.PathUtility.EnsureDirectoryExists(value);
                    ReadCacheFromDisk(value);
                    cacheFolder = value;
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
        /// <see cref="Image"/> if the resource exists in the cache, <c>null</c> otherwise
        /// </returns>
        public static Image Get(string applicationName, string resourceID)
        {
            Image image = null;
            if (IsCacheConfigured && !String.IsNullOrEmpty(applicationName) && !String.IsNullOrEmpty(resourceID))
            {
                string id = PathUtility.GetSafeFileName(resourceID);
                string filename = GetResourceFilename(applicationName, id);
                if (!String.IsNullOrEmpty(filename))
                {
                    try
                    {
                        image = Growl.CoreLibrary.ImageConverter.ImageFromUrl(filename);
                    }
                    catch
                    {
                    }
                }
            }
            return image;
        }

        /// <summary>
        /// Adds the specified resource to the cache
        /// </summary>
        /// <param name="applicationName">The application that owns the resource</param>
        /// <param name="resource"><see cref="Resource"/> to cache</param>
        /// <returns>The ID of the added resource</returns>
        public static string Add(string applicationName, Resource resource)
        {
            string id = null;
            if (IsCacheConfigured && !String.IsNullOrEmpty(applicationName) && resource != null)
            {
                string path = GetApplicationCacheDirectoryName(applicationName);
                DirectoryInfo appDirectory = CreateAppCacheDirectory(path); // do this every time, in case the underlying filesystem has changed
                if (!cache.ContainsKey(appDirectory.FullName))
                {
                    cache.Add(appDirectory.FullName, new Dictionary<string, string>());
                }

                id = PathUtility.GetSafeFileName(resource.GetKey());

                Dictionary<string, string> appCache = cache[appDirectory.FullName];
                if (appCache.ContainsKey(id)) appCache.Remove(id);

                string filename = String.Format(@"{0}{1}", id, EXTENSION);
                string filepath = PathUtility.Combine(appDirectory.FullName, filename);

                System.Drawing.Image image = (Image)resource;
                using (image)
                {
                    if(image != null) image.Save(filepath);
                }

                if (!appCache.ContainsKey(id))
                    appCache.Add(id, filepath);
            }
            return id;
        }

        /// <summary>
        /// Indicates if the cache is configured for use
        /// </summary>
        /// <value>
        /// <c>true</c> if the cache is enabled and has a valid CacheFolder set,
        /// <c>false otherwise</c>
        /// </value>
        private static bool IsCacheConfigured
        {
            get
            {
                return (!String.IsNullOrEmpty(CacheFolder));
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
            string path = PathUtility.Combine(cacheFolder, safeAppName);
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
