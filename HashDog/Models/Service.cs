using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;

namespace HashDog;
public class Service
{
    private readonly Source source;
    private readonly Database db;
    public Service()
    {
        source = new(GetSourcePath());
        db = new Database(GetSourcePath());
       
        HandleRun();
        TimeSpan duration = TimeSpan.FromSeconds(10);
        Timer timer = new Timer(HandleRun!, null, duration, duration);
        Daemon daemon = new(duration, timer);

        Console.ReadKey();
        daemon.Stop();
        Console.WriteLine("Watchdog stopped.");
        // The Watchdog class is a Daemon not watchdog
        // Add a lock table where it puts the filepath of the currently occupied table to prevent two instances running the same directory

    }

    private void HandleRun()
    {
        // FIRST RUN
        if (!db.DoesTableExist())
        {
            FirstRun();
        }
        // SUBSEQUENT RUNS
        else
        {
            SubsequentRun();    
        } 
    }

    private void HandleRun(object state)
    {
        // FIRST RUN
        if (!db.DoesTableExist())
        {
            FirstRun();
        }
        // SUBSEQUENT RUNS
        else
        {
            SubsequentRun();    
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
            db.InsertData(path, Hash.GetFileHash(path, HashType.MD5));
        }

        foreach (int id in db.GetHashDogTableIds())
        {
            db.FirstRunArchiveCopy(id);
        }
    }

    private void SubsequentRun()
    {
        CheckFirstRun();

        Queue<int> queue = new Queue<int>();   

        foreach(int id in db.GetHashDogTableIds())
        {
            queue.Enqueue(id);
        }

        HashType hashType = db.GetTableHashType();

        while (queue.Count > 0)
        {
            int id = queue.Dequeue();
            int archiveId = db.SubsequentRunArchiveCopyBefore(id);
            db.UpdateData(id, Hash.GetFileHash(db.GetHashDogTableFilepath(id), hashType));
            db.SubsequentRunArchiveCopyAfter(id, archiveId);
            db.HandleArchiveComparisonResult(archiveId);
        }    
    }

    private static string GetSourcePath()
    {
        return Path.Combine(Environment.CurrentDirectory, "testfolder");
    }

    private void CheckFirstRun()
    {
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
            db.InsertData(path, Hash.GetFileHash(path, db.GetTableHashType()));
            db.FirstRunArchiveCopyByPath(path);
        }
    }
}