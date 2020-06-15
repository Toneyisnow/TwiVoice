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
        /// Sample: UtauTest.exe "C:\Users\charl\source\repos\UtauTest\sample\sample.ust" "D:\Temp\ustoutput.wav" "C:\Program Files (x86)\UTAU\resampler.exe" "C:\Program Files (x86)\UTAU\voice\Wan er VCVChinese"
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            GenerateWaveFromUJson(args);
            // Console.ReadKey();
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

            string ustFileFullPath = args[0];
            string outputFullPath = args[1];
            string resamplerFullPath = args[2];
            string singerPath = args[3];

            VoiceGenerator generator = new VoiceGenerator(ustFileFullPath, resamplerFullPath, singerPath);
            generator.ConvertUstToWave(outputFullPath);

            Console.WriteLine("Finished.");
        }

        /// <summary>
        /// Args: 
        ///     "C:\Users\charl\source\repos\UtauTest\sample\sample.ust" 
        ///     "D:\Temp\ust.json" 
        ///     "C:\Users\charl\source\repos\UtauTest\sample\resampler.exe" 
        ///     "C:\Program Files (x86)\UTAU\voice\Wan er VCVChinese"
        /// </summary>
        /// <param name="args"></param>
        static void ConvertToUJson(string[] args)
        {
            Console.WriteLine("Start ConvertToUJson...");
            string ustFileFullPath = args[0];
            string outputFullPath = args[1];
            string resamplerFullPath = args[2];
            string singerPath = args[3];

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

            string jsonFileFullPath = args[0];
            string outputFullPath = args[1];

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

    }

}
