
namespace ytmp
{
    public class YTSong
    {
        public YTSong(string title, string id)
        {
            this.title = title;
            this.id = id;
        }
        public string title { get; set; }
        public string id { get; set; }

        public override string ToString()
        {
            return title;
        }
    }
}
