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

        private ObservableCollection<FileEntry> _file;
        public ObservableCollection<FileEntry> File
        {
            get => _file;
            set => SetProperty(ref _file, value);
        }

        private ObservableCollection<OutpostEntry> _outpost;
        public ObservableCollection<OutpostEntry> Outpost
        {
            get => _outpost;
            set => SetProperty(ref _outpost, value);
        }

        public List<ArchiveEntry> Archive { get; }

        private readonly Service _instance;

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

        private OutpostEntry _selectedOutpost;
        public OutpostEntry SelectedOutpost
        {
            get { return _selectedOutpost; }
            set
            {
                _selectedOutpost = value;
                if (_selectedOutpost != null)
                {
                    File = new ObservableCollection<FileEntry>(_instance.ReadOutpostFile(_selectedOutpost.Id));
                    Log.Information($"Selected Outpost Id: {_selectedOutpost.Id}");
                }
                RaisePropertyChanged(nameof(SelectedOutpost));
            }

        }

        public MainWindowViewModel()
        {
            _instance = Service.Instance;
            Outpost = new ObservableCollection<OutpostEntry>(_instance.ReadOutpost());

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
