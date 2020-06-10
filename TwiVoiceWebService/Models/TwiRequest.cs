using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TwiVoiceWebService.Models
{
    public class TwiRequest
    {
        public Guid Id
        {
            get; set;
        }

        public string Name
        {
            get; set;
        }

        public int Count
        {
            get; set;
        }

        public bool IsTest
        {
            get; set;
        }

        public string UstFile
        {
            get; set;
        }

        public string VoiceFolder
        {
            get; set;
        }

        public string ResamplerFile
        {
            get; set;
        }

        public string OutputWaveFile
        {
            get; set;
        }

    }
}
