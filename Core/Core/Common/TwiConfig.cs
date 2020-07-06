using Newtonsoft.Json;
using System;
using System.IO;
using System.Runtime.InteropServices;

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
            using (StreamReader reader = new StreamReader(GetConfigFile()))
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

        static string GetConfigFile()
		{
			bool isWindows = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            bool isMacOS = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
            bool isLinux = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
            
            if (isWindows) {
                return "twi_config.json";
            }

            if (isMacOS) {
                return "twi_config_darwin.json";
            }

            if (isLinux) {
                return "twi_config_linux.json";
            }

            return "twi_config.json";
        }
    }
}
