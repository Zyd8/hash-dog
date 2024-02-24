using System;
using System.IO;
using System.Collections.Generic;

namespace HashDog;
public class Service
{
    public Service()
    {
        string sourcePath = Path.Combine(Environment.CurrentDirectory, "testfolder");
        Source source = new(sourcePath);
        Console.WriteLine(source.IsFile);

        Queue<string> queue = new Queue<string>();

        if (source.IsFile)
        {
            queue.Enqueue(source.Path);
            Console.WriteLine(queue.Count);
        }
        else
        {
            List<string> filePaths = FileUtils.TraverseDirectories(source.Path);
            foreach (string filePath in filePaths)
            {
                queue.Enqueue(filePath);
                Console.WriteLine(queue.Count);
            }
        }

        while (queue.Count > 0)
        {
            string path = queue.Dequeue();
            Console.WriteLine(Hash.GetFileHash(path, HashType.SHA512));
        }

        Console.WriteLine(queue.Count);
    }
}