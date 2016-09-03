
namespace ytmp
{
    class YTConfig
    {
        public YTConfig(string url, double volume, bool repeat, bool shuffle, YTAuth auth)
        {
            this.url = url;
            this.volume = volume;
            this.repeat = repeat;
            this.shuffle = shuffle;
            this.auth = auth;
        }

        public string url { get; set; }
        public double volume { get; set; }
        public bool repeat { get; set; }
        public bool shuffle { get; set; }
        public YTAuth auth;
    }
}
