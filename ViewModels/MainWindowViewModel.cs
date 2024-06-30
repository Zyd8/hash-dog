using System.Collections.Generic;
using System.Linq;

namespace HashDog.ViewModels
{
    public class MainWindowViewModel
    { 

        public List<string> AnimalList { get; } 

        public MainWindowViewModel()
        {
            AnimalList = new List<string>
            {
                "cat", "camel", "cow", "chameleon", "mouse", "lion", "zebra"
            }.OrderBy(x => x).ToList();
        }
    }
}
