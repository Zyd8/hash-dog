using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using HashDog.Models;
using Serilog;
using System.Timers;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using Avalonia.Interactivity;
using System;

namespace HashDog.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {

        private readonly Service _instance;

        [ObservableProperty]
        private ObservableCollection<FileEntry> _file;

        [ObservableProperty]
        private ObservableCollection<OutpostEntry> _outpost;

        [ObservableProperty]
        private ObservableCollection<ArchiveEntry> _archive;

        [ObservableProperty]
        private ObservableCollection<MismatchArchiveEntry> _mismatchArchive;

        [ObservableProperty]
        private ObservableCollection<ArchiveEntry> _archiveMismatchRelevant;

        [ObservableProperty]
        private int _topSelectedTabIndex;

        [ObservableProperty]
        private int _bottomSelectedTabIndex;

        [ObservableProperty]
        private bool _isHashDogEnabled;

        [ObservableProperty]
        private string _isHashDogEnabledText = "Enable HashDog";

        partial void OnIsHashDogEnabledChanged(bool value)
        {
            UpdateIsHashDogEnabledText();
            if (value)
            {
                _instance.SetScheduleRunTimer();
                Log.Information("Scheduler started");
            }
            else
            {
                _instance.Dispose();
                Log.Information("Scheduler stopped");
            }
        }

        private void UpdateIsHashDogEnabledText()
        {
            IsHashDogEnabledText = IsHashDogEnabled ? "Disable HashDog" : "Enable HashDog";
        }


        [ObservableProperty]
        private OutpostEntry _selectedOutpost;

        partial void OnSelectedOutpostChanged(OutpostEntry value)
        {
            if (value != null)
            {
                LoadOutpostFile(value);
                Log.Information($"Selected Outpost Id: {value.Id}");

                Archive = new ObservableCollection<ArchiveEntry>();

                TopSelectedTabIndex = 1;
            }
        }


        [ObservableProperty]
        private FileEntry _selectedOutpostFile;
        partial void OnSelectedOutpostFileChanged(FileEntry value)
        {
            if (value != null)
            {
                LoadOutpostFileArchive(value);
                Log.Information($"Selected Outpost File Id: {value.Id}");
                TopSelectedTabIndex = 2;
            }
        }

        [ObservableProperty]
        private MismatchArchiveEntry _selectedMismatchArchive;
        partial void OnSelectedMismatchArchiveChanged(MismatchArchiveEntry value)
        {
            if (value != null)
            {
                LoadArchiveMismatchRelevant(value);

                BottomSelectedTabIndex = 1;
            }
        }


        public MainWindowViewModel()
        {
            _instance = Service.Instance;

            IsHashDogEnabled = Service.IsSchedulerRunning();

            LoadOutpost();
            LoadMismatchArchive();
        }

        public void RefreshDataGrids()
        {             
            LoadOutpost();
            LoadMismatchArchive();
            if (SelectedOutpost != null)
            {
                LoadOutpostFile(SelectedOutpost);
            }
            if (SelectedOutpostFile != null)
            {
                LoadOutpostFileArchive(SelectedOutpostFile);
            }           
        }


        public void LoadOutpost()
        {
            Outpost = new ObservableCollection<OutpostEntry>(_instance.ReadOutpost());
        }

        public void LoadMismatchArchive()
        {
            var mismatchEntries = _instance.ReadMismatchArchive()
                              .OrderByDescending(entry => entry.Timestamp)
                              .ToList();
            MismatchArchive = new ObservableCollection<MismatchArchiveEntry>(mismatchEntries);
        }

        public void LoadOutpostFile(OutpostEntry outpost)
        {
            File = new ObservableCollection<FileEntry>(_instance.ReadOutpostFile(outpost.Id));
        }

        public void LoadOutpostFileArchive(FileEntry file)
        {
            Archive = new ObservableCollection<ArchiveEntry>(_instance.ReadOutpostFileArchive(SelectedOutpost.Id, file.Id));
        }

        public void LoadArchiveMismatchRelevant(MismatchArchiveEntry mismatchArchive)
        {
            ArchiveMismatchRelevant = new ObservableCollection<ArchiveEntry>(_instance.ReadOutpostFileArchive(mismatchArchive.OutpostEntryId, mismatchArchive.FileEntryId));
        }

        public void RemoveOutpost(OutpostEntry outpost)
        {
            _instance.RemoveOutpost(outpost);
            RefreshDataGrids();
        }
    }
}

