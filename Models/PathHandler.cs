using System.IO;
using System.Collections.Generic;

namespace HashDog
{
    public class PathHandler
    {


        public static List<string> GetPathFiles(string path)
        {
            List<string> files = new List<string>();

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
