using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolastaKoreanMod
{
    public class TranslationData
    {
        [JsonProperty("creation_date")]
        public DateTime CreationDate;

        [JsonProperty("strings")]
        public Dictionary<string, string> Strings;
    }
}
