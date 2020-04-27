using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;

using MinecraftPlayerViewer.Parser;

using Newtonsoft.Json.Linq;


namespace MinecraftPlayerViewer.Api
{
    class HypixelApi
    {
        private const string apiUrl = "https://api.hypixel.net/friends?key={0}&uuid={1}";
        private const string browserAgent = "Mozilla/5.0 (X11; CrOS x86_64 7520.63.0) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/47.0.2526.106 Safari/537.36";
        public static HypixelApiResult GetFriends(string uuid, string apiKey)
        {
            string buildedApiUrl = string.Format(apiUrl, apiKey, uuid);
            WebClient client = new WebClient();

            client.Headers.Add("user-agent", browserAgent);

            Stream data = null;
            StreamReader reader = null;
            HypixelApiResult result = new HypixelApiResult();
            try
            {
                data = client.OpenRead(buildedApiUrl);
                reader = new StreamReader(data);
                string response = reader.ReadToEnd(); 
                JObject jsonResponse = JObject.Parse(response);


                if (jsonResponse["success"].ToString().Equals(("true")))
                {
                    List<Friend> friends = new List<Friend>();
                    foreach (JToken rec in jsonResponse["records"].ToList<JToken>())
                    {
                        Friend friend = new Friend()
                        {
                            id = rec["id"].ToString(),
                            uuidSender = rec["uuidSender"].ToString(),
                            uuidReceiver = rec["uuidReceiver"].ToString(),
                            started = rec["started"].ToString()
                        };
                        friends.Add(friend);
                    }
                    result.friends = friends;
                }
                else
                {
                    switch (jsonResponse["cause"].ToString())
                    {
                        case "Invalid API key":
                        case "No \"key\" provided":
                            result.result = ApiResultType.INVALIDAPIKEY;
                            break;
                        case "Malformed UUID":
                        case "No \u0027uuid\u0027 field":
                            result.result = ApiResultType.NOUUID;
                            break;
                    }
                }

                return result;
            }
            catch (Exception ignored)
            {
                result.result = ApiResultType.FAILED;
            }
            finally
            {
                if (data != null)
                    data.Close();
                if (reader != null)
                    reader.Close();
            }

            return result;
        }
    }
}
