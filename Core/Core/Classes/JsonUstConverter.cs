using System;
using System.Collections.Generic;
using System.Text;
using TwiVoice.Core.Formats;
using TwiVoice.Core.USTx;

namespace TwiVoice.Core.Classes
{
    public class JsonUstConverter
    {
        public static UJson CreateUJson(string ustFileFullPath, string resamplerFullPath, string singerName)
        {
            UJson ujson = new UJson();
            ujson.Setting = new UJsonSetting();

            ujson.Setting.Tempo = 120;
            ujson.Setting.TrackCount = 1;
            ujson.Setting.SingerName = singerName;
            ujson.Setting.ResamplerFile = resamplerFullPath;

            ujson.LoadTracksFromUst(ustFileFullPath);

            return ujson;
        }

        public static UProject ConvertFromJsonToUProject(UJson ujson)
        {
            UProject uProject = new UProject();



            return uProject;
        }


    }
}
