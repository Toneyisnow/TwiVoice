using Newtonsoft.Json;
using System;
using System.IO;

namespace TwiVoice.Core.Common
{
    public class TwiConfig
    {
        private static string configFileName = "twi_config.json";

        [JsonProperty(PropertyName = "singers_root_path")]
        public string SingersRootPath
        {
            get; set;
        }

        [JsonProperty(PropertyName = "cache_folder_path")]
        public string CacheFolderPath
        {
            get; set;
        }

        [JsonProperty(PropertyName = "resamplers_folder_path")]
        public string ResamplersFolderPath
        {
            get; set;
        }

        [JsonProperty(PropertyName = "output_folder_path")]
        public string OutputFolderPath
        {
            get; set;
        }



        public static TwiConfig LoadFromFile()
        {
            using (StreamReader reader = new StreamReader(configFileName))
            {
                try
                {
                    string content = reader.ReadToEnd();
                    TwiConfig config = JsonConvert.DeserializeObject<TwiConfig>(content);
                    return config;

                }
                catch(Exception ex)
                {
                    Logger.Instance.Error("Reading config file error: " + ex.ToString());
                    return null;
                }
            }
        }


    }
}
