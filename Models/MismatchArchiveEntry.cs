using System;


namespace HashDog.Models
{
    public class MismatchArchiveEntry
    {
        public int Id { get; set; }
        public string Path { get; set; }
        public string HashBefore { get; set; }
        public string HashAfter { get; set; }
        public DateTime Timestamp { get; set; }
        public int FileEntryId { get; set; }
        public int OutpostEntryId { get; set; }
    }
}
