using System;

namespace HashDog
{
    public class ArchiveEntry
    {
        public int Id { get; set; }
        public int FileEntryId { get; set; }
        public FileEntry FileEntry { get; set; }
        public int OutpostEntryId { get; set; }
        public OutpostEntry OutpostEntry{ get; set; }
        public string HashBefore { get; set; }
        public string HashAfter { get; set; }
        public DateTime Timestamp { get; set; }
        public string ComparisonResult { get; set; }
    }
}
