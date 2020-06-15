using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using TwiVoice.Core.Classes;
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
            ConvertToUJson(args);
            // Console.ReadKey();
        }


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

        static void ConvertToUJson(string[] args)
        {
            Console.WriteLine("Start ConvertToUJson...");
            string ustFileFullPath = args[0];
            string outputFullPath = args[1];
            string resamplerFullPath = args[2];
            string singerPath = args[3];

            UJson uJson = JsonUstConverter.CreateUJson(ustFileFullPath, resamplerFullPath, singerPath);

            string content = JsonConvert.SerializeObject(uJson, Formatting.Indented);

            using(StreamWriter writer = new StreamWriter(outputFullPath))
            {
                writer.Write(content);
            }

            Console.WriteLine("Finished ConvertToUJson.");
        }
    }

}
