using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using HashDog.Models;
using Serilog;

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
                File = new ObservableCollection<FileEntry>(); 
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
                File = new ObservableCollection<FileEntry>(_instance.ReadOutpostFile(value.Id));
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
                Archive = new ObservableCollection<ArchiveEntry>(_instance.ReadOutpostFileArchive(SelectedOutpost.Id, value.Id));
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
                Archive = new ObservableCollection<ArchiveEntry>(_instance.ReadOutpostFileArchive(value.OutpostEntryId, value.FileEntryId));
                TopSelectedTabIndex = 2;
            }
        }


        public MainWindowViewModel()
        {
            _instance = Service.Instance;
            Outpost = new ObservableCollection<OutpostEntry>(_instance.ReadOutpost());
            MismatchArchive = new ObservableCollection<MismatchArchiveEntry>(_instance.ReadMismatchArchive()); 

        }

        public void OnAddOutpostClick()
        {
            _instance.CreateOutpost(_instance.GetNewOutpostPath());

            _instance.CreateOutpost(@"C:\Users\Zyd\testing\outpost2");
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
