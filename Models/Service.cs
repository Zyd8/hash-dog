﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using Serilog;

namespace HashDog.Models
{
    public class Service
    {

        private List<Scheduler> _schedulers = new List<Scheduler>();

        private static readonly Service _instance = new Service();

        public static Service Instance
        {
            get { return _instance; }
        }

        private Service() 
        {
            InitDatabase();                 
        }

        public static void InitDatabase()
        {
            using (var context = new Database())
            {
                context.Database.EnsureCreated(); 
            }
        }

        public static bool IsSchedulerRunning()
        {
            return Scheduler.IsRunning;
        }

        public bool OutpostAlreadyExist(string outpostPath)
        {
            using (var context = new Database())
            {             
                var existingOutpost = context.Outposts.FirstOrDefault(o => o.CheckPath == outpostPath);
                if (existingOutpost != null)
                {
                    Log.Information($"Outpost with CheckPath '{outpostPath}' already exists.");
                    return true;
                }
                return false;
            }
        }
        public void CreateOutpost(string outpostPath, HashType hashType, int checkFreqHours)
        {
            using (var context = new Database())
            { 
                if (OutpostAlreadyExist(outpostPath))
                {
                    return;
                }

                var newOutpostEntry = new OutpostEntry
                {
                    CheckPath = outpostPath,
                    HashType = HashUtils.ParseHashTypeToString(hashType),
                    CheckFreqHours = checkFreqHours,
                    LastChecked = DateTime.Now
                };
                context.Outposts.Add(newOutpostEntry);
                context.SaveChanges();

                Run(newOutpostEntry);

                if (Scheduler.IsRunning)
                {
                    Scheduler newScheduler = new Scheduler(newOutpostEntry, this);
                    _schedulers.Add(newScheduler);
                }
            }
        }

        public void RemoveOutpost(OutpostEntry outpost)
        {
            using (var context = new Database())
            {
                var outpostToDelete = context.Outposts.FirstOrDefault(o => o.Id == outpost.Id);
                if (outpostToDelete != null)
                {

                    var schedulerToRemove = _schedulers.FirstOrDefault(s => s.Outpost.Id == outpost.Id);
                    if (schedulerToRemove != null)
                    {
                        schedulerToRemove.Dispose();
                        _schedulers.Remove(schedulerToRemove);
                    }

                    context.Outposts.Remove(outpostToDelete);
                    context.SaveChanges();
                }
            }
        }


        public List<OutpostEntry> ReadOutpost()
        {
            using (var context = new Database())
            {
                var outpost = context.Outposts
                    .Select(o => new OutpostEntry
                    {
                        Id = o.Id,
                        CheckPath = o.CheckPath,
                        HashType = o.HashType,                                           
                        CheckFreqHours= o.CheckFreqHours,
                        LastChecked = o.LastChecked,
                        Files = o.Files
                    })
                    .ToList();

                return outpost;
            }
        }

        public List<FileEntry> ReadOutpostFile(int outpostId)
        {
            using (var context = new Database())
            {
                var files = context.Files
                    .Where(o => o.OutpostEntryId == outpostId).
                    Select(f => new FileEntry
                    {
                        Id = f.Id,
                        OutpostEntryId = outpostId,
                        OutpostEntry = f.OutpostEntry,
                        Path = f.Path,
                        Hash = f.Hash,
                    })
                    .ToList();
         
                return files;
            }
        }

        public List<ArchiveEntry> ReadOutpostFileArchive(int outpostId, int fileId)
        {
            using (var context = new Database())
            {
                var archives = context.Archives
                    .Where(a => a.OutpostEntryId == outpostId && a.FileEntryId == fileId)
                    .Select(a => new ArchiveEntry
                    {
                        Id = a.Id,
                        FileEntryId = a.FileEntryId,
                        FileEntry = a.FileEntry,
                        OutpostEntryId = a.OutpostEntryId,
                        OutpostEntry = a.OutpostEntry,
                        HashBefore = a.HashBefore,
                        HashAfter = a.HashAfter,
                        Timestamp = a.Timestamp,
                        ComparisonResult = a.ComparisonResult
                    })
                    .ToList();

                return archives;
            }
        }

        public List<MismatchArchiveEntry> ReadMismatchArchive()
        {
            List<MismatchArchiveEntry> mismatchEntries = new List<MismatchArchiveEntry>();

            using (var context = new Database())
            {
                var archiveEntries = context.Archives
                    .Where(entry => entry.ComparisonResult == "Mismatch")
                    .ToList();

                foreach (var entry in archiveEntries)
                {
                    var fileEntry = context.Files.FirstOrDefault(fe => fe.Id == entry.FileEntryId);
                    string path = fileEntry?.Path ?? "Unknown"; 

                    var mismatchEntry = new MismatchArchiveEntry
                    {
                        Id = entry.Id,
                        HashBefore = entry.HashBefore,
                        HashAfter = entry.HashAfter,
                        Timestamp = entry.Timestamp,
                        FileEntryId = entry.FileEntryId,
                        OutpostEntryId = entry.OutpostEntryId,
                        Path = path
                    };

                    mismatchEntries.Add(mismatchEntry);
                }
            }

            return mismatchEntries;
            
        }

        public void Run(OutpostEntry outpostEntry)
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

        public void Dispose()
        {
            foreach (var scheduler in _schedulers)
            {
                scheduler.Dispose();
            }

            // avoid duplicates when scheduler is toggled multiple times
            _schedulers = new List<Scheduler>();

            Console.WriteLine("Service disposed");
        }


    }

    public class Scheduler
    {
        public DateTime NextRunTime { get; private set; }
        public OutpostEntry Outpost { get; private set; }
        public Service Service { get; private set; }

        private Timer? _timer;

        public static bool IsRunning;

        public Scheduler(OutpostEntry outpost, Service service)
        {
            IsRunning = true;
            Outpost = outpost;
            Service = service;

            SetNextRunTimeSchedule();
        }

        private void SetNextRunTimeSchedule()
        {     
            NextRunTime = Outpost.LastChecked.AddSeconds(Outpost.CheckFreqHours); 

            TimeSpan interval = NextRunTime - DateTime.Now;
            if (interval < TimeSpan.Zero) interval = TimeSpan.Zero;

            if (_timer == null)
            {
                _timer = new Timer(ScheduleRun, null, interval, Timeout.InfiniteTimeSpan);
            }
            else
            {
                _timer.Change(interval, Timeout.InfiniteTimeSpan);
            }
            Log.Information($"next run time: {NextRunTime}");
        }

        private void ScheduleRun(object? state)
        {
            Service.Run(Outpost);
            SetNextRunTimeSchedule();
        }

        public void Dispose()
        {
            _timer?.Change(Timeout.Infinite, Timeout.Infinite);
            _timer?.Dispose();
            Log.Information("I disposed");
        }
    }

}
