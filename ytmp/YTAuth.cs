using System;
using System.Text;
using Newtonsoft.Json;
using System.Net;
using System.IO;

namespace ytmp
{
    public class YTAuth
    {
        private string access_token;
        public string Access_token
        {
            get
            {
                DateTime expiration = created.Add(TimeSpan.FromHours(1.0));
                // Access token lasts an hour if its expired we get a new one.
                if (DateTime.Compare(DateTime.Now,expiration)>0)//DateTime.Now.Subtract(created).Hours > 1)
                {
                    Refresh();
                }
                return access_token;
            }
            set { access_token = value; }
        }
        public string refresh_token { get; set; }
       
        public string expires_in { get; set; }
        public DateTime created { get; set; }

        public YTAuth() { }

        public YTAuth(YTAuth auth)
        {
            if (auth != null)
            {
                refresh_token = auth.refresh_token;
                expires_in = auth.expires_in;
                created = auth.created;
                access_token = auth.Access_token;
            }
        }
        /// <summary>
        /// Parse the json response 
        /// //  "{\n  \"access_token\" : \"ya29.kwFUj-la2lATSkrqFlJXBqQjCIZiTg51GYpKt8Me8AJO5JWf0Sx6-0ZWmTpxJjrBrxNS_JzVw969LA\",\n  \"token_type\" : \"Bearer\",\n  \"expires_in\" : 3600,\n  \"refresh_token\" : \"1/ejoPJIyBAhPHRXQ7pHLxJX2VfDBRz29hqS_i5DuC1cQ\"\n}"
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        


        public void Refresh()
        {
            var request = (HttpWebRequest)WebRequest.Create("https://accounts.google.com/o/oauth2/token");
            string postData = string.Format("client_id={0}&client_secret={1}&refresh_token={2}&grant_type=refresh_token", YTHelper.clientId, YTHelper.secret, refresh_token);
            var data = Encoding.ASCII.GetBytes(postData);

            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            var response = (HttpWebResponse)request.GetResponse();
            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
            var refreshResponse = YTHelper.GetAuth(responseString);
            this.access_token = refreshResponse.access_token;
            this.created = DateTime.Now;
        }


        

    }
}
