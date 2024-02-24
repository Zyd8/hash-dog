using System;
using System.IO;

namespace HashDog;
public class Source
{
    private string? _path;
    private bool _isFile;
    public bool IsFile { 
        get { return _isFile; }
        set { _isFile = value; }
    }

    public string Path { 
        get 
        { 
            if (_path != null)
            {
                return _path;
            } 
            else
            {
                throw new Exception();
            }
        }
        set 
        {
            if (File.Exists(value))
            {
                IsFile = true;
                _path = value;
            }
            else if (Directory.Exists(value))
            {
                IsFile = false;
                _path = value;
            }
            else
            {
                throw new Exception();
            }
        }
    }

    public Source(string path)
    {
        Path = path;
    }

    
}