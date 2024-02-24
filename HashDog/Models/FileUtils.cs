using System.Collections.Generic;
using System.IO;

namespace HashDog;

public class FileUtils
{
    public static List<string> TraverseDirectories(string directoryPath)
    {
        List<string> filePaths = new List<string>();

        foreach (string filePath in Directory.GetFiles(directoryPath))
        {
            filePaths.Add(filePath);
        }

        foreach (string subDirPath in Directory.GetDirectories(directoryPath))
        {
            filePaths.AddRange(TraverseDirectories(subDirPath));
        }

        return filePaths;
    }
}