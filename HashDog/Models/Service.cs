using System;
using System.IO;
using System.Linq.Expressions;

namespace HashDog;
public class Service
{
    public Service()
    {
        string sourcePath = Path.Combine(Environment.CurrentDirectory, "test.txt");
        Source source = new(sourcePath);
        Console.WriteLine(source.IsFile);
    }
}