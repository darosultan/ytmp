using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ytmp
{
    class ItemInfo
    {
        ItemInfo() { }
        static public ItemInfo Create(string iinfo)
        {
            if (iinfo.StartsWith("info = "))
            {
                iinfo = iinfo.Substring(7);
                iinfo = iinfo.Remove(iinfo.Length - 1);
                return JsonConvert.DeserializeObject<ItemInfo>(iinfo);
            }
            else return null;
        }
        public string title { get; set; }
        public string image { get; set; }
        public string length { get; set; }
        public string status { get; set; }
        public string progress_speed { get; set; }
        public string progress { get; set; }
        public string ads { get; set; }
        public string pf { get; set; }
        public string pc { get; set; }
        public string h { get; set; }
        public string px { get; set; }
        public string ts_create { get; set; }
        public string r { get; set; }
        public string h2 { get; set; }
    }
}
