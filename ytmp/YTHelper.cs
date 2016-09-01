using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using VideoLibrary;
using MoreLinq;

namespace ytmp
{
    static class YTHelper
    {
        private static readonly Random rnd = new Random();

        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                int k = rnd.Next(0, n);
                n--;
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static string timeFromSeconds(double curSec, double lenSec)
        {

            int curminutes = Convert.ToInt32(Math.Floor(curSec / 60));
            int curnewsec = Convert.ToInt32(Math.Floor(curSec - (60 * curminutes)));
            string curzerosec;
            if (curnewsec < 10)
                curzerosec = "0" + curnewsec.ToString();
            else
                curzerosec = curnewsec.ToString();
            int lenminutes = Convert.ToInt32(Math.Floor(lenSec / 60));
            int lennewsec = Convert.ToInt32(Math.Floor(lenSec - (60 * lenminutes)));
            string lenzerosec;
            if (lennewsec < 10)
                lenzerosec = "0" + lennewsec.ToString();
            else
                lenzerosec = lennewsec.ToString();

            return curminutes.ToString() + ":" + curzerosec + " / " + lenminutes.ToString() + ":" + lenzerosec;
        }

        public static List<YTSong> gimmeItems(string ytlink)
        {
            List<YTSong> output = new List<YTSong>();
            string listId;

            if (ytlink.Contains("list="))
            {
                listId = ytlink.Substring(ytlink.IndexOf("list=") + 5);
                
                if (listId.Contains('&'))
                    listId=listId.Remove(listId.IndexOf('&'));
                if (listId.Contains('#'))
                    listId=listId.Remove(listId.IndexOf('#'));
                string pagetoken = String.Empty;
                bool b = true;
                while (b)
                {
                    string listRequestUrl = "https://www.googleapis.com/youtube/v3/playlistItems?part=snippet&maxResults=50&key=AIzaSyApTG3kkY0blW9WJFac00uwJp7Rkg31LCY&playlistId=" + listId + "&pageToken=" + pagetoken;
                    var request = (HttpWebRequest)WebRequest.Create(listRequestUrl);
                    try
                    {
                        var response = (HttpWebResponse)request.GetResponse();
                        YTObject listResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<YTObject>(new StreamReader(response.GetResponseStream(), Encoding.UTF8).ReadToEnd());
                        response.Close();
                        if (listResponse.nextPageToken ==null)
                            b = false;
                        else
                            pagetoken = listResponse.nextPageToken;
                        foreach (Item item in listResponse.items)
                        {
                            if(item.snippet.title!="Deleted video" && item.snippet.title != "Private video")
                                output.Add(new YTSong(item.snippet.title, item.snippet.resourceId.videoId));
                        }
                    }
                    catch
                    {
                        return null;
                    }
                }
                return output;
            }
            else if(ytlink.Contains("v="))
            {
                listId = ytlink.Substring(ytlink.IndexOf("v=") + 2);

                if (listId.Contains('&'))
                    listId=listId.Remove(listId.IndexOf('#'));
                if (listId.Contains('#'))
                    listId=listId.Remove(listId.IndexOf('#'));

                string listRequestUrl = "https://www.googleapis.com/youtube/v3/videos?part=snippet&key=AIzaSyApTG3kkY0blW9WJFac00uwJp7Rkg31LCY&id=" + listId;
                var request = (HttpWebRequest)WebRequest.Create(listRequestUrl);
                try
                {
                    var response = (HttpWebResponse)request.GetResponse();
                    YTObject vidResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<YTObject>(new StreamReader(response.GetResponseStream(), Encoding.UTF8).ReadToEnd());
                    response.Close();

                    foreach (Item item in vidResponse.items)
                    {
                        output.Add(new YTSong(item.snippet.title, item.id));
                    }
                    return output;
                }
                catch
                {
                    return null;
                }
            }
            else if(ytlink.Contains("channel/")||ytlink.Contains("user/"))
            {
                string channelId;
                if(ytlink.Contains("user/"))
                {
                    string username = ytlink.Substring(ytlink.IndexOf("user/") + 5);
                    if (username.Contains('/'))
                        username = username.Remove(username.IndexOf('/'));
                    string usernameRequestUrl = "https://www.googleapis.com/youtube/v3/channels?part=id&key=AIzaSyApTG3kkY0blW9WJFac00uwJp7Rkg31LCY&forUsername=" + username;
                    var request = (HttpWebRequest)WebRequest.Create(usernameRequestUrl);
                    try
                    {
                        var response = (HttpWebResponse)request.GetResponse();
                        YTObject userResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<YTObject>(new StreamReader(response.GetResponseStream(), Encoding.UTF8).ReadToEnd());
                        response.Close();
                        if (userResponse.items[0].id != null)
                            channelId = userResponse.items[0].id;
                        else return null;
                    }
                    catch
                    {
                        return null;
                    }
                }
                else
                {
                    channelId = ytlink.Substring(ytlink.IndexOf("channel/") + 8);
                    if (channelId.Contains('/'))
                        channelId=channelId.Remove(channelId.IndexOf('/'));
                }

                string pagetoken = String.Empty;
                bool b = true;
                while (b)
                {
                    string listRequestUrl = "https://www.googleapis.com/youtube/v3/search?part=snippet&maxResults=50&order=date&key=AIzaSyApTG3kkY0blW9WJFac00uwJp7Rkg31LCY&channelId=" + channelId + "&pageToken=" + pagetoken;
                    var request = (HttpWebRequest)WebRequest.Create(listRequestUrl);
                    try
                    {
                        var response = (HttpWebResponse)request.GetResponse();
                        YTChannelObject listResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<YTChannelObject>(new StreamReader(response.GetResponseStream(), Encoding.UTF8).ReadToEnd());
                        response.Close();
                        if (listResponse.nextPageToken == null)
                            b = false;
                        else
                            pagetoken = listResponse.nextPageToken;
                        foreach (ChannelItem item in listResponse.items)
                        {
                            if (item.snippet.title != "Deleted video" && item.snippet.title != "Private video" && item.id.kind=="youtube#video")
                                output.Add(new YTSong(item.snippet.title, item.id.videoId));
                        }
                    }
                    catch
                    {
                        return null;
                    }
                }
                return output;
            }
            else return null;
        }

        public static string createDirectLink(string vidId)
        {
            var yt = YouTube.Default;
            var videos = yt.GetAllVideos("http://www.youtube.com/watch?v=" + vidId).ToList();
            var audios = videos.Where(_ => _.AudioFormat == AudioFormat.Aac && _.AdaptiveKind == AdaptiveKind.Audio).ToList();
            if (audios.Count != 0)
                return audios.MaxBy(_ => _.AudioBitrate).Uri;
            else return null;
        }
    }

    public class PageInfo
    {
        public int totalResults { get; set; }
        public int resultsPerPage { get; set; }
    }

    public class Default
    {
        public string url { get; set; }
        public int width { get; set; }
        public int height { get; set; }
    }

    public class Medium
    {
        public string url { get; set; }
        public int width { get; set; }
        public int height { get; set; }
    }

    public class High
    {
        public string url { get; set; }
        public int width { get; set; }
        public int height { get; set; }
    }

    public class Standard
    {
        public string url { get; set; }
        public int width { get; set; }
        public int height { get; set; }
    }

    public class Maxres
    {
        public string url { get; set; }
        public int width { get; set; }
        public int height { get; set; }
    }

    public class Thumbnails
    {
        public Default @default { get; set; }
        public Medium medium { get; set; }
        public High high { get; set; }
        public Standard standard { get; set; }
        public Maxres maxres { get; set; }
    }

    public class ResourceId
    {
        public string kind { get; set; }
        public string videoId { get; set; }
    }

    public class Snippet
    {
        public string publishedAt { get; set; }
        public string channelId { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public Thumbnails thumbnails { get; set; }
        public string channelTitle { get; set; }
        public string playlistId { get; set; }
        public int position { get; set; }
        public ResourceId resourceId { get; set; }
        public List<string> tags { get; set; }
        public string categoryId { get; set; }
        public string liveBroadcastContent { get; set; }
        public Localized localized { get; set; }
    }

    public class Localized
    {
        public string title { get; set; }
        public string description { get; set; }
    }

    public class Item
    {
        public string kind { get; set; }
        public string etag { get; set; }
        public string id { get; set; }
        public Snippet snippet { get; set; }
    }

    public class ChannelItem
    {
        public string kind { get; set; }
        public string etag { get; set; }
        public Id id { get; set; }
        public Snippet snippet { get; set; }
    }

    public class Id
    {
        public string kind { get; set; }
        public string videoId { get; set; }
        public string playlistId { get; set; }
    }

    public class YTObject
    {
        public string kind { get; set; }
        public string etag { get; set; }
        public string nextPageToken { get; set; }
        public string regionCode { get; set; }
        public PageInfo pageInfo { get; set; }
        public List<Item> items { get; set; }
    }

    public class YTChannelObject
    {
        public string kind { get; set; }
        public string etag { get; set; }
        public string nextPageToken { get; set; }
        public string regionCode { get; set; }
        public PageInfo pageInfo { get; set; }
        public List<ChannelItem> items { get; set; }
    }
}
