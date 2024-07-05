using CommunityToolkit.Mvvm.ComponentModel;
using HashDog.Models;
using HashDog.Views;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HashDog.ViewModels
{

    public partial class OutpostFileViewModel : ObservableObject
    {
        private readonly Service _instance = Service.Instance;

        private int _outpostId;

        [ObservableProperty]
        private ObservableCollection<FileEntry> _file;


        [ObservableProperty]
        private FileEntry _selectedOutpostFile;
        partial void OnSelectedOutpostFileChanged(FileEntry value)
        {
            if (value != null)
            {
                var outpostFileArchiveViewModel = new OutpostFileArchiveViewModel()
                {
                    Archive = new ObservableCollection<ArchiveEntry>(_instance.ReadOutpostFileArchive(_outpostId, value.Id))
                };

                var outpostFileArchiveView = new OutpostFileArchiveView(outpostFileArchiveViewModel);
                outpostFileArchiveView.Show();
               
                Log.Information($"Selected Outpost File Id: {value.Id}");
            }
        }

        public OutpostFileViewModel(int outpostId)
        {
            _outpostId = outpostId;   
        }
       
    }
}
