using System;
using System.IO;
using System.Collections.Generic;

namespace HashDog;
public class Service
{
    private Source source;
    private Database db;
    public Service()
    {
    
        db = new Database(GetSourcePath());
        source = new Source(GetSourcePath());
        HandleRun();

        DaemonRun();
        db.Dispose();
    }

    private void DaemonRun()
    {
        TimeSpan duration = TimeSpan.FromSeconds(10);
        Daemon daemon = new(duration);
        daemon.Start(HandleRun!);

        Console.ReadKey();

        daemon.Stop();
        
        Console.WriteLine("Daemon stopped.");
    }

    private void HandleRun(object state)
    {
        try
        {
            if (db.IsTableLocked)
            {
                Console.WriteLine($"{db.TablePath} is being used by another hash-dog instance. Try again later.");
            }
            else
            {
                // FIRST RUN
                if (!db.HashDogTableExists())
                {
                    FirstRun();
                }
                // SUBSEQUENT RUNS
                else
                {
                    SubsequentRun();    
                } 
            }
        }
        finally
        {
            db.RemoveLockTablePath();
        }
    }


    private void HandleRun()
    {
        try
        {
            if (db.IsTableLocked)
            {
                Console.WriteLine($"{db.TablePath} is being used by another hash-dog instance. Try again later.");
            }
            else
            {
                // FIRST RUN
                if (!db.HashDogTableExists())
                {
                    FirstRun();
                }
                // SUBSEQUENT RUNS
                else
                {
                    SubsequentRun();    
                } 
            }
        }
        finally
        {
            db.RemoveLockTablePath();
        }
    }

    private void FirstRun()
    {
        db.CreateHashDog();

        Queue<string> queue = new Queue<string>();   

        if (source.IsFile)
        {
            queue.Enqueue(source.Path);
        }
        else
        {
            List<string> filePaths = FileUtils.TraverseDirectories(source.Path);
            foreach (string filePath in filePaths)
            {
                queue.Enqueue(filePath);
            }
        }

        db.InsertMetadata(HashType.MD5);

        while (queue.Count > 0)
        {
            string path = queue.Dequeue();
            int id = db.InsertData(path, Hash.GetFileHash(path, HashType.MD5));
            db.FirstRunArchiveCopy(id);
        }
    }

    private void SubsequentRun()
    {
        List<int> firstRunEntryIds = CheckFirstRunEntries();

        Queue<int> queue = new Queue<int>();   

        foreach(int id in db.GetHashDogTableIds())
        {
            queue.Enqueue(id);
        }

        HashType hashType = db.GetTableHashType();

        while (queue.Count > 0)
        {
            int id = queue.Dequeue();

            if (Path.Exists(db.GetHashDogTableFilepath(id)))
            {
                int archiveId = db.SubsequentRunArchiveCopyBefore(id);
                db.UpdateData(id, Hash.GetFileHash(db.GetHashDogTableFilepath(id), hashType));
                if (!firstRunEntryIds.Contains(id))
                {
                    db.SubsequentRunArchiveCopyAfter(id, archiveId);
                }
                db.HandleArchiveComparisonResult(archiveId);
            }
        }    
    }

    private static string GetSourcePath()
    {
        return Path.Combine(Environment.CurrentDirectory, "testfolder copy");
    }

    private List<int> CheckFirstRunEntries()
    {
        List<int> firstRunEntryIds = new List<int>();
        Queue<string> queue = new Queue<string>();  

        if (source.IsFile && !db.IsFilePathExistInTable(source.Path))
        {
            queue.Enqueue(source.Path);
        }
        else if (!source.IsFile)
        {
            List<string> filePaths = FileUtils.TraverseDirectories(source.Path);
            foreach (string filePath in filePaths)
            {
                if (!db.IsFilePathExistInTable(filePath))
                {
                    queue.Enqueue(filePath);
                }
            }
        }

        while (queue.Count > 0)
        {
            string path = queue.Dequeue();
            int id = db.InsertData(path, Hash.GetFileHash(path, db.GetTableHashType()));
            firstRunEntryIds.Add(id);
        }

        return firstRunEntryIds;
    }
}