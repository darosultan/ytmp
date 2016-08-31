using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ytmp
{
    class YTConfig
    {
        public YTConfig(string url, double volume, bool repeat, bool shuffle)
        {
            this.url = url;
            this.volume = volume;
            this.repeat = repeat;
            this.shuffle = shuffle;
        }

        public string url { get; set; }
        public double volume { get; set; }
        public bool repeat { get; set; }
        public bool shuffle { get; set; }
    }
}
