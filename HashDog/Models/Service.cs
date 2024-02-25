using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;

namespace HashDog;
public class Service
{
    public Service()
    {
        
        //string sourcePath = Path.Combine(Environment.CurrentDirectory, "test.txt");
        string sourcePath = Path.Combine(Environment.CurrentDirectory, "testfolder");
        Source source = new(sourcePath);

        Database db = new Database(Path.GetFileNameWithoutExtension(source.Path));

        // FIRST RUN
        if (!db.DoesTableExist())
        {
            db.CreateHashDog();

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
                }
                Console.WriteLine(queue.Count);
            }

            db.InsertMetadata(HashType.MD5);

            while (queue.Count > 0)
            {
                string path = queue.Dequeue();
                db.InsertData(path, Hash.GetFileHash(path, HashType.MD5));
            }

            db.FirstRunArchiveCopy(); // Make this atomic 

            Console.WriteLine(queue.Count);
        }
        // SUBSEQUENT RUNS
        else
        {
            Console.WriteLine("Table already exists");
            
            
            Queue<int> queue = new Queue<int>();   

            foreach(int id in db.GetHashDogTableId())
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
            }

            //db.GenerateArchiveComparisonResults();
            // check if a certain file is first run, if so, archive it directly and make it as first run
            // Rename better
            // Do refactorings
        }

        db.Dispose();
    }
}