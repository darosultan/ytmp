using System.Collections.Generic;

namespace ytmp
{
    public class YTPlaylist
    {
        private bool _shuffle;
        public bool shuffle
        {
            get
            {
                return _shuffle;
            }
            set
            {
                _shuffle = value;
                if (_shuffle) Shuffle();
            }
        }
        public int playIndex { get; set; }
        private List<YTSong> unshuffledList;
        private List<YTSong> shuffledList;

        public YTPlaylist(List<YTSong> list)
        {
            if (list != null)
            {
                unshuffledList = new List<YTSong>(list);
                shuffledList = new List<YTSong>(list);
            }
            else
            {
                unshuffledList = new List<YTSong>();
                shuffledList = new List<YTSong>();
            }
            Shuffle();
            playIndex = 0;
        }

        private void Shuffle()
        {
            shuffledList.Shuffle();
        }

        public int Count()
        {
            return unshuffledList.Count;
        }

        public YTSong Current()
        {
            return this[playIndex];
        }

        public void Remove(YTSong song)
        {
            shuffledList.Remove(song);
            unshuffledList.Remove(song);
        }

        public List<YTSong> GetList()
        {
            if (_shuffle) return shuffledList;
            else return unshuffledList;
        }

        public YTSong this[int index]
        {
            get
            {
                if (index < this.Count())
                {
                    if (_shuffle)
                        return shuffledList[index];
                    else
                        return unshuffledList[index];
                }
                else return null;
            }
        }
    }
}
