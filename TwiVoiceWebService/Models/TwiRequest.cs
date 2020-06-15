using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TwiVoice.Core.Formats;

namespace TwiVoiceWebService.Models
{
    public class TwiRequest
    {
        [JsonProperty(PropertyName = "id")]
        public Guid Id
        {
            get; set;
        }

        [JsonProperty(PropertyName = "name")]
        public string Name
        {
            get; set;
        }

        [JsonProperty(PropertyName = "request")]
        public TwiRequestBody RequestBody
        {
            get; set;
        }

        [JsonProperty(PropertyName = "response")]
        public TwiResponseBody ResponseBody
        {
            get; set;
        }

    }

    public class TwiRequestBody
    {
        [JsonProperty(PropertyName = "is_test")]
        public bool IsTest
        {
            get; set;
        }

        [JsonProperty(PropertyName = "output_file_name")]
        public string OutputWaveFile
        {
            get; set;
        }

        [JsonProperty(PropertyName = "input")]
        public UJson Input
        {
            get; set;
        }

    }

    

    /// <summary>
    /// 
    /// </summary>
    public class TwiResponseBody
    {
        [JsonProperty(PropertyName = "result")]
        public int Result
        {
            get; set;
        }

        [JsonProperty(PropertyName = "message")]
        public string Message
        {
            get; set;
        }

        [JsonProperty(PropertyName = "output_file_name")]
        public string WaveFilePath
        {
            get; set;
        }

    }


}
