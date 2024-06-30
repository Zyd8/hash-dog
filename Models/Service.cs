﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;

namespace HashDog
{
    public class Service
    {

        private List<Scheduler> _schedulers = new List<Scheduler>();

        public Service() 
        {
            InitDatabase();                 
        }

        // temporary for testing only, should use migrations
        public static void InitDatabase()
        {
            using (var context = new Database())
            {
                context.Database.EnsureCreated(); 
            }
        }

        // adds outpost, all needs to first be added to the outpost before proceeding
        public void AddOutpost(string outpostPath)
        {
            using (var context = new Database())
            { 
                // Guards for conflicting outpost checking path
                var existingOutpost = context.Outposts.FirstOrDefault(o => o.CheckPath == outpostPath);
                if (existingOutpost != null)
                {
                    Console.WriteLine($"Outpost with CheckPath '{outpostPath}' already exists.");
                    return; 
                }

                var newOutpostEntry = new OutpostEntry
                {
                    CheckPath = outpostPath,
                    HashType = HashUtils.ParseHashTypeToString(HashType.MD5),
                    CheckFreqHours = 10,
                    LastChecked = DateTime.Now
                };
                context.Outposts.Add(newOutpostEntry);
                context.SaveChanges();
            }
        }

        public void ScheduleRun(OutpostEntry outpostEntry)
        {
            using (var context = new Database())
            {
                List<string> filePaths = PathHandler.GetPathFiles(outpostEntry.CheckPath);

                foreach (var filePath in filePaths)
                {
                    var existingFileEntry = context.Files.FirstOrDefault(o => o.Path == filePath);

                    // up for updates
                    if (existingFileEntry != null)
                    {
                        string oldHash = existingFileEntry.Hash;
                        string newHash = HashUtils.GetFileHash(filePath, HashUtils.ParseHashTypeFromString(outpostEntry.HashType));

                        existingFileEntry.Hash = newHash;
                        context.Files.Update(existingFileEntry);
                        context.SaveChanges();
         

                        var newArchiveEntry = new ArchiveEntry
                        {
                            FileEntryId = existingFileEntry.Id,
                            OutpostEntryId = outpostEntry.Id,
                            HashBefore = oldHash,
                            HashAfter = newHash,
                            Timestamp = DateTime.Now,
                            ComparisonResult = HashUtils.ParseHashComparisonToString(HashUtils.HashCompare(oldHash, newHash)),
                        };
                        context.Archives.Add(newArchiveEntry);
                        context.SaveChanges();

                        // untrack context for next iteration to take over
                        context.ChangeTracker.Clear();
                    }
                    // up for insertion
                    else
                    {
                        var newFileEntry = new FileEntry
                        {
                            OutpostEntryId = outpostEntry.Id,
                            Path = filePath,
                            Hash = HashUtils.GetFileHash(filePath, HashUtils.ParseHashTypeFromString(outpostEntry.HashType)),
                        };
                        context.Files.Add(newFileEntry);
                        context.SaveChanges();
              

                        var newArchiveEntry = new ArchiveEntry
                        {
                            FileEntryId = newFileEntry.Id,
                            OutpostEntryId = outpostEntry.Id,
                            HashBefore = newFileEntry.Hash,
                            HashAfter = "",
                            Timestamp = DateTime.Now,
                            ComparisonResult = HashUtils.ParseHashComparisonToString(ComparisonResult.FirstRun),
                        };
                        context.Archives.Add(newArchiveEntry);
                        context.SaveChanges();

                        // untrack context for next iteration to take over
                        context.ChangeTracker.Clear();
                    }
                }
            }

            using (var context = new Database())
            {
                outpostEntry.LastChecked = DateTime.Now;
                context.Outposts.Update(outpostEntry);
                context.SaveChanges();
            }
        }


        public void SetScheduleRunTimer()
        {
            using (var context = new Database())
            {
                foreach (var outpost in context.Outposts)
                {
                    Scheduler scheduler = new Scheduler(outpost, this);
                    _schedulers.Add(scheduler);
                }
            }
        }

        public string GetNewOutpostPath()
        {
            string path = @"C:\Users\Zyd\testing\outpost1";
            if (path != null)
            {
                return path;
            }
            throw new Exception();
        }

        public void Dispose()
        {
            foreach (var scheduler in _schedulers)
            {
                scheduler.Dispose();
            }
            Console.WriteLine("Service disposed");
        }


    }

    public class Scheduler
    {
        public DateTime NextRunTime { get; private set; }
        public OutpostEntry Outpost { get; private set; }
        public Service Service { get; private set; }

        private Timer? _timer;

        public Scheduler(OutpostEntry outpost, Service service)
        {
            Outpost = outpost;
            Service = service;

            SetNextRunTimeSchedule();
        }

        private void SetNextRunTimeSchedule()
        {
            // Set the next run time
            NextRunTime = Outpost.LastChecked.AddSeconds(Outpost.CheckFreqHours); // Change seconds to hour after testing
            Console.WriteLine($"Next run time {NextRunTime}");

            // Calculate the interval until the next run time
            TimeSpan interval = NextRunTime - DateTime.Now;
            if (interval < TimeSpan.Zero) interval = TimeSpan.Zero;

            // Initialize or change the timer
            if (_timer == null)
            {
                _timer = new Timer(ScheduleRun, null, interval, Timeout.InfiniteTimeSpan);
            }
            else
            {
                _timer.Change(interval, Timeout.InfiniteTimeSpan);
            }
        }

        // Runs on set schedule, checks for untracked files and insert it, or update tracked files
        private void ScheduleRun(object? state)
        {
            Service.ScheduleRun(Outpost);
            SetNextRunTimeSchedule();

        }

        public void Dispose()
        {
            _timer?.Change(Timeout.Infinite, Timeout.Infinite);
            _timer?.Dispose();
        }
    }

}
