using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using HashDog.Models;
using Serilog;

namespace HashDog.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
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

        public List<OutpostEntry> Outpost { get; }



        private readonly Service _instance;

        public MainWindowViewModel()
        {
            _instance = Service.Instance;
            Outpost = _instance.ReadOutpost();

        }

        public void OnAddOutpostClick()
        {
            _instance.CreateOutpost(_instance.GetNewOutpostPath());
            _instance.CreateOutpost(@"C:\Users\Zyd\testing\outpost2");
        }
    }
}
