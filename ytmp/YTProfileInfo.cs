using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ytmp
{
    class YTProfileInfo
    {
        public YTProfileInfo(string name, string pic)
        {
            Name = name;
            PictureUrl = pic;
        }

        public string Name { get; set; }
        public string PictureUrl { get; set; }
    }
}
