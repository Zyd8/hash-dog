using CommunityToolkit.Mvvm.ComponentModel;
using HashDog.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HashDog.ViewModels
{
    public partial class AddOutpostViewModel : ObservableObject
    {
        private readonly Service _instance;

        private readonly MainWindowViewModel _mainWindowViewModel;

        [ObservableProperty]
        private bool _folderPath_IsVisible;

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


        [ObservableProperty]
        private bool _alert_IsVisible;

        [ObservableProperty]
        private string _alert_Text;

        partial void OnAlert_TextChanged(string value)
        {
            if (value != string.Empty)
            {
                Alert_IsVisible = true;
            }
        }


        public AddOutpostViewModel(MainWindowViewModel mainWindowViewModel)
        {
            _instance = Service.Instance;
            _mainWindowViewModel = mainWindowViewModel;
        }


        public void AddOutpost(AddOutpostView addOutpostView)
        {

            if (FolderPath_Text == null || FolderPath_Text == String.Empty)
            {
                Alert_Text = "Please specify the folder";
                return;
            }

            if (_instance.OutpostAlreadyExist(FolderPath_Text))
            {
                Alert_Text = "Outpost Already Exists";
                return;
            }

            if (CheckFreqHours_Value == null || CheckFreqHours_Value == 0)
            {
                Alert_Text = "Check frequency is not valid";
                return;
            }

            // Does not wait for the Service.Run to finish
            addOutpostView.Close();

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

            _mainWindowViewModel.RefreshDataGrids();
            

        }


    }
}
