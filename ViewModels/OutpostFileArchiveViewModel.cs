using CommunityToolkit.Mvvm.ComponentModel;
using HashDog.Models;
using System.Collections.ObjectModel;


namespace HashDog.ViewModels
{
    public partial class OutpostFileArchiveViewModel : ObservableObject
    {

      
        [ObservableProperty]
        private ObservableCollection<ArchiveEntry> _archive;
        

    }
}
