using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using TwiVoice.Core.Classes;
using TwiVoice.Core.Common;
using TwiVoice.Core.Formats;
using TwiVoice.Core.Render;
using TwiVoice.Core.ResamplerDrivers;
using TwiVoice.Core.USTx;

namespace TwiVoice.Core
{
    public class Program
    {
        /// <summary>
        /// Sample: TwiVoice.Core.exe "C:\Users\charl\source\repos\UtauTest\sample\sample.ust" "D:\Temp\ustoutput.wav" "C:\Program Files (x86)\UTAU\resampler.exe" "C:\Program Files (x86)\UTAU\voice\Wan er VCVChinese"
        /// Commands:
        ///     --usttowav
        ///     --usttojson
        ///     --jsontowav
        ///     --jsontotxt
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            switch(args[0])
            {
                case "--usttowav":
                    GenerateWave(args);
                    return;

                case "--usttojson":
                    ConvertToUJson(args);
                    return;

                case "--jsontowav":
                    GenerateWaveFromUJson(args);
                    return;

                case "--jsontotxt":
                    ExportFromUJson(args); 
                    return;

                default:
                    Console.WriteLine(@"Usage: <command> [parameters]
Commands: 
    --usttowav <ust_file> <output_wav> <resampler_file> <voice_folder>
    --usttojson <ust_file> <output_json> <resampler_file> <voice_folder>
    --jsontowav <json_file> <output_wav>
    --jsontotxt <json_file> <output_txt>
");
                    break;
            }
            
        }

        /// <summary>
        /// Args: 
        ///     "C:\Users\charl\source\repos\UtauTest\sample\sample.ust" 
        ///     "D:\Temp\utauoutput.wav" 
        ///     "C:\Users\charl\source\repos\UtauTest\sample\resampler.exe" 
        ///     "C:\Program Files (x86)\UTAU\voice\Wan er VCVChinese"
        /// </summary>
        /// <param name="args"></param>
        static void GenerateWave(string[] args)
        {
            Console.WriteLine("Start...");

            string ustFileFullPath = args[1];
            string outputFullPath = args[2];
            string resamplerFullPath = args[3];
            string singerPath = args[4];

            VoiceGenerator generator = new VoiceGenerator(ustFileFullPath, resamplerFullPath, singerPath);
            generator.ConvertUstToWave(outputFullPath);

            Console.WriteLine("Finished.");
        }

        /// <summary>
        /// Args: 
        ///     "C:\Users\charl\source\repos\UtauTest\sample\sample.ust" "D:\Temp\ust.json" "C:\Users\charl\source\repos\UtauTest\sample\resampler.exe" "C:\Program Files (x86)\UTAU\voice\Wan er VCVChinese"
        /// </summary>
        /// <param name="args"></param>
        static void ConvertToUJson(string[] args)
        {
            Console.WriteLine("Start ConvertToUJson...");
            string ustFileFullPath = args[1];
            string outputFullPath = args[2];
            string resamplerFullPath = args[3];
            string singerPath = args[4];

            UJson uJson = UJson.Create(ustFileFullPath, resamplerFullPath, singerPath);

            string content = JsonConvert.SerializeObject(uJson, Formatting.Indented);

            using(StreamWriter writer = new StreamWriter(outputFullPath))
            {
                writer.Write(content);
            }

            Console.WriteLine("Finished ConvertToUJson.");
        }

        /// <summary>
        /// Args: 
        ///     "D:\Temp\ust.json" "D:\Temp\json_out.wav"
        /// </summary>
        /// <param name="args"></param>
        static void GenerateWaveFromUJson(string[] args)
        {
            Console.WriteLine("Start...");

            string jsonFileFullPath = args[1];
            string outputFullPath = args[2];

            string jsonContent = string.Empty;
            using (StreamReader reader = new StreamReader(jsonFileFullPath))
            {
                jsonContent = reader.ReadToEnd();
            }

            UJson uJson = JsonConvert.DeserializeObject<UJson>(jsonContent);
            UProject uProject = uJson.ToUProject();

            TwiConfig config = TwiConfig.LoadFromFile();
            string resamplerFullPath = Path.Combine(config.ResamplersFolderPath, uJson.Setting.ResamplerFile);
            VoiceGenerator generator = new VoiceGenerator(uProject, resamplerFullPath);
            generator.ConvertUstToWave(outputFullPath);

            Console.WriteLine("Finished.");
        }

        /// <summary>
        /// Args: 
        ///     "D:\Temp\ust.json" "D:\Temp\json_out.txt"
        /// </summary>
        /// <param name="args"></param>
        static void ExportFromUJson(string[] args)
        {
            Console.WriteLine("Start...");

            string jsonFileFullPath = args[1];
            string outputFullPath = args[2];

            string jsonContent = string.Empty;
            using (StreamReader reader = new StreamReader(jsonFileFullPath))
            {
                jsonContent = reader.ReadToEnd();
            }

            UJson uJson = JsonConvert.DeserializeObject<UJson>(jsonContent);
            UProject uProject = uJson.ToUProject();

            TwiConfig config = TwiConfig.LoadFromFile();
            string resamplerFullPath = Path.Combine(config.ResamplersFolderPath, uJson.Setting.ResamplerFile);
            VoiceGenerator generator = new VoiceGenerator(uProject, resamplerFullPath);
            
            List<UOto> otoList = generator.ListAllOtos();
            using(StreamWriter writer = new StreamWriter(outputFullPath))
            {
                /*
                foreach(UOto oto in otoList)
                {
                    string line = string.Format(@"{0}|{1}|{2}|{3}|{4}|{5}|{6}",
                        oto.Alias,
                        oto.Consonant,
                        oto.Cutoff,
                        oto.File,
                        oto.Offset,
                        oto.Overlap,
                        oto.Preutter);
                }
                */

                writer.Write(JsonConvert.SerializeObject(otoList, Formatting.Indented));
            }

            Console.WriteLine("Finished.");
        }
    }

}
