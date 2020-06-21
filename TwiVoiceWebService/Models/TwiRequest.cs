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
        //// [JsonProperty(PropertyName = "id")]
        public Guid Id
        {
            get; set;
        }

        //// [JsonProperty(PropertyName = "name")]
        public string Name
        {
            get; set;
        }


        //// [JsonProperty(PropertyName = "request")]
        public TwiRequestBody Request
        {
            get; set;
        }

        //// [JsonProperty(PropertyName = "response")]
        public TwiResponseBody Response
        {
            get; set;
        }
        

    }

    public class TwiRequestBody
    {
        //// [JsonProperty(PropertyName = "is_test")]
        public bool IsTest
        {
            get; set;
        }

        //// [JsonProperty(PropertyName = "output_file_name")]
        public string OutputFileName
        {
            get; set;
        }

        //// [JsonProperty(PropertyName = "input")]
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
        //// [JsonProperty(PropertyName = "result")]
        public int Result
        {
            get; set;
        }

        //// [JsonProperty(PropertyName = "message")]
        public string Message
        {
            get; set;
        }

        //// [JsonProperty(PropertyName = "output_file_name")]
        public string OutputFileName
        {
            get; set;
        }

    }


}
