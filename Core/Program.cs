using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using TwiVoice.Core;
using TwiVoice.Core.Formats;
using TwiVoice.Core.Render;
using TwiVoice.Core.ResamplerDriver;
using TwiVoice.Core.USTx;

namespace Core
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

            //// WaveUtils.SplitWave("first.wav", new TimeSpan(0, 0, 8), "output.wav");

            string ustFileFullPath = args[0];
            string outputFullPath = args[1];
            string resamplerFullPath = args[2];
            string singerPath = args[3];

            ConvertUstToWave(ustFileFullPath, resamplerFullPath, singerPath, outputFullPath);

            Console.WriteLine("Finished...");

        }

        /// <summary>
        /// 
        /// 
        /// </summary>
        /// <param name="ustFileFullPath"></param>
        /// <param name="resamplerFullPath"></param>
        /// <param name="singerPath"></param>
        /// <param name="output"></param>
        static void ConvertUstToWave(string ustFileFullPath, string resamplerFullPath, string singerPath, string output)
        {

            UProject uproject = Ust.Load(ustFileFullPath, singerPath);


            outputFullPath = output;

            PartManager partManager = new PartManager(uproject);
            partManager.UpdatePart(uproject.Parts[0] as UVoicePart);

            BuildAudio(uproject, resamplerFullPath);

            Console.WriteLine("Press any key...");
            Console.ReadKey();
        }

        private static readonly object lockObject = new object();
        private static MixingSampleProvider masterMix;
        private static List<TrackSampleProvider> trackSources;
        private static int pendingParts;
        private static string outputFullPath = null;


        private static void BuildAudio(UProject project, string resamplerFullPath)
        {
            trackSources = new List<TrackSampleProvider>();
            foreach (UTrack track in project.Tracks)
            {
                trackSources.Add(new TrackSampleProvider() { Volume = DecibelToVolume(track.Volume) });
            }

            pendingParts = project.Parts.Count;
            foreach (UPart part in project.Parts)
            {
                if (part is UWavePart)
                {
                    /*
                    lock (lockObject)
                    {
                        trackSources[part.TrackNo].AddSource(
                            BuildWavePartAudio(part as UWavePart, project),
                            TimeSpan.FromMilliseconds(project.TickToMillisecond(part.PosTick))
                        );
                        pendingParts--;
                    }
                    */
                }
                else
                {
                    var singer = project.Tracks[part.TrackNo].Singer;
                    if (singer != null && singer.Loaded)
                    {
                        System.IO.FileInfo ResamplerFile = new System.IO.FileInfo(resamplerFullPath);
                        IResamplerDriver engine = ResamplerDriver.LoadEngine(ResamplerFile.FullName);
                        BuildVoicePartAudio(part as UVoicePart, project, engine);
                    }
                    else lock (lockObject) { pendingParts--; }
                }
            }

            if (pendingParts == 0) WriteToFile(outputFullPath);
        }


        private static void BuildVoicePartAudio(UVoicePart part, UProject project, IResamplerDriver engine)
        {
            ResamplerInterface ri = new ResamplerInterface();
            ri.ResamplePart(part, project, engine, (o) => { BuildVoicePartDone(o, part, project); });

        }

        private static void BuildVoicePartDone(SequencingSampleProvider source, UPart part, UProject project)
        {
            lock (lockObject)
            {
                if (source != null)
                {
                    trackSources[part.TrackNo].AddSource(
                        source,
                        TimeSpan.FromMilliseconds(project.TickToMillisecond(part.PosTick))
                    );
                }
                pendingParts--;
            }

            if (pendingParts == 0) WriteToFile(outputFullPath);
        }

        private static void WriteToFile(string outputFileFullPath)
        {
            masterMix = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(44100, 1));
            foreach (var source in trackSources)
            {
                //// Log.Debug("source format: " + source.WaveFormat.ToString());
                masterMix.AddMixerInput(source.ToMono());
            }

            WdlResamplingSampleProvider resampedWavFile =
                            new WdlResamplingSampleProvider(masterMix, 44100);

            SampleToWaveProvider16 sampleToWavProvider = new SampleToWaveProvider16(resampedWavFile);

            WaveProviderToWaveStream waveStream = new WaveProviderToWaveStream(sampleToWavProvider);

            WaveFileWriter.CreateWaveFile(outputFileFullPath, waveStream);
        }

        private static float DecibelToVolume(double db)
        {
            return (db == -24) ? 0 : (float)((db < -16) ? MusicMath.DecibelToLinear(db * 2 + 16) : MusicMath.DecibelToLinear(db));
        }

    }

}
