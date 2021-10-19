using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace SolastaKoreanMod
{
    public class GithubRepoClient
    {
        private const string REPO_API_URL = "https://api.github.com/repos/team-waldo/solasta-translation/releases/latest";

        public Release GetLatestRelease()
        {
            return DeserializeWebRequest<Release>(REPO_API_URL);
        }

        public TranslationData DoanloadAsset(Release.Asset asset)
        {
            return DeserializeWebRequest<TranslationData>(asset.browser_download_url);
        }

        private T DeserializeWebRequest<T>(string url)
        {
            try
            {
                WebClient client = new WebClient();
                client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");

                using (Stream data = client.OpenRead(url))
                using (StreamReader sr = new StreamReader(data))
                using (JsonTextReader jtr = new JsonTextReader(sr))
                {
                    return JsonSerializer.CreateDefault().Deserialize<T>(jtr);
                }
            }
            catch (Exception e)
            {
                ModMain.Error(e.StackTrace);
                return default;
            }
        }

        public class Release
        {
            public int id;
            public string tag_name;
            public string name;

            public DateTime created_at;
            public DateTime published_at;

            public List<Asset> assets;

            public class Asset
            {
                public int id;
                public string name;

                public DateTime created_at;
                public DateTime published_at;

                public string browser_download_url;
            }
        }
    }
}
