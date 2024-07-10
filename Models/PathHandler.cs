using System.IO;
using System.Collections.Generic;
using System;

namespace HashDog.Models
{
    public class PathHandler
    {

        public static List<string> GetPathFiles(string path)
        {
            List<string> files = new List<string>();

            if (path.StartsWith("file:///"))
            {
                string localPath = new Uri(path).LocalPath;

                if (File.Exists(localPath))
                {
                    files.Add(localPath);
                }
                else
                {
                    files.AddRange(GetFilesRecursive(localPath));
                }
            }

            if (File.Exists(path))
            {
                files.Add(path);
            }
            else
            {
                files.AddRange(GetFilesRecursive(path));
            }

            return files;
        }


        private static List<string> GetFilesRecursive(string directory)
        {
            List<string> fileList = new List<string>();

            fileList.AddRange(Directory.GetFiles(directory));

            string[] subdirectories = Directory.GetDirectories(directory);
            foreach (string subdir in subdirectories)
            {
                fileList.AddRange(GetFilesRecursive(subdir));
            }

            return fileList;
        }
    }
}
