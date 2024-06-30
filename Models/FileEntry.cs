namespace HashDog.Models
{
    public class FileEntry
    {
        public int Id { get; set; }
        public int OutpostEntryId { get; set; }
        public OutpostEntry OutpostEntry { get; set; }
        public string Path { get; set; }
        public string Hash { get; set; }


    }
}
