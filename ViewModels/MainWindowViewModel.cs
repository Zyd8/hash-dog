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
        private int _topSelectedTabIndex;
        partial void OnTopSelectedTabIndexChanged(int value)
        {
            if (value == 0)
            {
                //File = new ObservableCollection<FileEntry>();
                Archive = new ObservableCollection<ArchiveEntry>();
            }
        }

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
                LoadArchiveViaMismatchArchive(value);
                TopSelectedTabIndex = 2;
            }
        }

        [ObservableProperty]
        private string _folderPath_Text = "";
        partial void OnFolderPath_TextChanged(string value)
        {
            if (value != string.Empty)
            {
                FolderPath_IsVisible = true;
            }
        }

        [ObservableProperty]
        private bool _folderPath_IsVisible;

        [ObservableProperty]
        private int? _checkFreqHours_Value = 0;
        partial void OnCheckFreqHours_ValueChanged(int? value)
        {
            if (value > 0 || value < 100000)
            {
                CheckFreqHours_Value = value;
            }
        }

        [ObservableProperty]
        private int _hashType_Index = 0;


        public MainWindowViewModel()
        {
            _instance = Service.Instance;

            IsHashDogEnabled = Service.IsSchedulerRunning();

            LoadOutpost();
            LoadMismatchArchive();
        }

        public void RefreshDataGrids()
        {
            if (IsHashDogEnabled)
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

        public void LoadArchiveViaMismatchArchive(MismatchArchiveEntry mismatchArchive)
        {
            Archive = new ObservableCollection<ArchiveEntry>(_instance.ReadOutpostFileArchive(mismatchArchive.OutpostEntryId, mismatchArchive.FileEntryId));
        }

        public void AddOutpost()
        {

            if (CheckFreqHours_Value == null || CheckFreqHours_Value == 0)
            {
                return;
            }

            if (FolderPath_Text == null || FolderPath_Text == String.Empty)
            { 
                return;
            }

            int checkFreqHours_Value = CheckFreqHours_Value!.Value;

            if (HashType_Index == 0)
            {
                _instance.CreateOutpost(FolderPath_Text, HashType.MD5, checkFreqHours_Value);
            }
            else if (HashType_Index == 1)
            {
                _instance.CreateOutpost(FolderPath_Text, HashType.SHA1, checkFreqHours_Value);
            }
            else if (HashType_Index == 2)
            {
                _instance.CreateOutpost(FolderPath_Text, HashType.SHA256, checkFreqHours_Value);
            }
            else if (HashType_Index == 3)
            {
                _instance.CreateOutpost(FolderPath_Text, HashType.SHA512, checkFreqHours_Value);
            }
            
        }
    }
}

