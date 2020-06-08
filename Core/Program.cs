using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
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
            Console.WriteLine("Start...");

            string ustFileFullPath = args[0];
            string outputFullPath = args[1];
            string resamplerFullPath = args[2];
            string singerPath = args[3];

            VoiceGenerator generator = new VoiceGenerator(ustFileFullPath, resamplerFullPath, singerPath);
            generator.ConvertUstToWave(outputFullPath);

            Console.WriteLine("Finished.");
            // Console.ReadKey();
        }

    }

}
