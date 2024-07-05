using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using HashDog.Models;
using HashDog.ViewModels;
using HashDog.Views;
using Serilog;

namespace HashDog.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        
        private readonly Service _instance;


        [ObservableProperty]
        private ObservableCollection<OutpostEntry> _outpost;

        
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
                var outpostFileViewModel = new OutpostFileViewModel(value.Id)
                {
                    File = new ObservableCollection<FileEntry>(_instance.ReadOutpostFile(value.Id))
                };

                var outpostFileView = new OutpostFileView(outpostFileViewModel);
                outpostFileView.Show();


                Log.Information($"Selected Outpost Id: {value.Id}");
            }
        }

        public int GetSelectedOutpostId()
        {
            return SelectedOutpost.Id;
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
    }
}
