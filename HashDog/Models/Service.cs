using System;
using System.IO;

namespace HashDog;
public class Service
{
    public Service()
    {
        string sourcePath = Path.Combine(Environment.CurrentDirectory, "test.txt");
        Source source = new(sourcePath);
        Console.WriteLine(source.IsFile);

        Console.WriteLine(Hash.GetFileHash(source.Path, HashType.MD5));
    }
}