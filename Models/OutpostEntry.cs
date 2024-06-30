using System;
using System.Collections.Generic;

namespace HashDog.Models
{
    public class OutpostEntry
    {
        public int Id { get; set; }
      
        public string CheckPath { get; set; }
        public string HashType { get; set; }
        public int CheckFreqHours { get; set; }
        public DateTime LastChecked { get; set; }

        public ICollection<FileEntry> Files { get; set; }
    }
}
