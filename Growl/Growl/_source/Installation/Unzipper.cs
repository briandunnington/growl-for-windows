using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ICSharpCode.SharpZipLib.Zip;

namespace Growl.Installation
{
    class Unzipper
    {
        public static void UnZipFiles(string zipFilePath, string outputFolder, bool deleteZipFile)
        {
            FileStream fs = File.OpenRead(zipFilePath);
            using (fs)
            {
                ZipInputStream s = new ZipInputStream(fs);
                using (s)
                {
                    ZipEntry theEntry;
                    string tmpEntry = String.Empty;
                    while ((theEntry = s.GetNextEntry()) != null)
                    {
                        string directoryName = outputFolder;
                        if (!Directory.Exists(directoryName))
                        {
                            Directory.CreateDirectory(directoryName);
                            Utility.WriteDebugInfo(String.Format("Unzipper created directory '{0}'.", directoryName));
                        }

                        string fileName = Path.GetFileName(theEntry.Name);
                        if (fileName != String.Empty)
                        {
                            string fullPath = directoryName + "\\" + theEntry.Name;
                            fullPath = fullPath.Replace("\\ ", "\\");
                            string fullDirPath = Path.GetDirectoryName(fullPath);
                            if (!Directory.Exists(fullDirPath)) Directory.CreateDirectory(fullDirPath);
                            FileStream streamWriter = File.Create(fullPath);
                            using (streamWriter)
                            {
                                byte[] data = new byte[theEntry.Size];
                                int size = 0;
                                while (true)
                                {
                                    size = s.Read(data, 0, data.Length);
                                    if (size > 0)
                                    {
                                        streamWriter.Write(data, 0, size);
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (deleteZipFile)
                File.Delete(zipFilePath);
        }

    }
}
