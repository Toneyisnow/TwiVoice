using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using TwiVoice.Core.Formats;
using TwiVoice.Core.Render;
using TwiVoice.Core.ResamplerDrivers;
using TwiVoice.Core.USTx;

namespace TwiVoice.Core
{
    public class VoiceGenerator
    {
        private readonly object lockObject = new object();

        //private readonly object lockComplete = new object();

        //private Mutex mutex = null;

        private AutoResetEvent waitHandle = null;

        private List<TrackSampleProvider> trackSources;

        private int pendingParts;

        private string outputFullPath = null;

        private string resamplerFullPath = null;

        private UProject uProject = null;

        public VoiceGenerator(string ustFileFullPath, string resamplerFullPath, string singerPath)
        {
            uProject = Ust.Load(ustFileFullPath, singerPath);
            this.resamplerFullPath = resamplerFullPath;
        }

        /// <summary>
        /// 
        /// 
        /// </summary>
        /// <param name="ustFileFullPath"></param>
        /// <param name="resamplerFullPath"></param>
        /// <param name="singerPath"></param>
        /// <param name="output"></param>
        public void ConvertUstToWave(string output)
        {
            Console.WriteLine("Start ConvertUstToWave.");

            outputFullPath = output;

            PartManager partManager = new PartManager(uProject);
            partManager.UpdatePart(uProject.Parts[0] as UVoicePart);

            //bool createdNew;
            //mutex = new Mutex(false, "TestSO27835942", out createdNew);
            waitHandle = new AutoResetEvent(false);

            //Monitor.Enter(lockComplete);
            BuildAudio(uProject, resamplerFullPath);

            //Monitor.Wait(lockComplete);
            waitHandle.WaitOne(10000);

            Console.WriteLine("Complete ConvertUstToWave.");
        }

        private void BuildAudio(UProject project, string resamplerFullPath)
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

            if (pendingParts == 0)
            {
                WriteToFile(outputFullPath);
            }
        }

        private void BuildVoicePartAudio(UVoicePart part, UProject project, IResamplerDriver engine)
        {
            Console.WriteLine("Start BuildVoicePartAudio");
            ResamplerInterface ri = new ResamplerInterface();
            ri.ResamplePart(part, project, engine, (o) => { BuildVoicePartDone(o, part, project); });

        }

        private void BuildVoicePartDone(SequencingSampleProvider source, UPart part, UProject project)
        {
            Console.WriteLine("Start BuildVoicePartAudio");
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

            if (pendingParts == 0)
            {
                WriteToFile(outputFullPath);
            }
        }

        private void WriteToFile(string outputFileFullPath)
        {
            string folder = Directory.GetCurrentDirectory();
            string fullPath = Path.Combine(folder, outputFileFullPath);

            MixingSampleProvider masterMix = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(44100, 1));
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

            //mutex.ReleaseMutex();
            waitHandle.Set();
            
        }

        private static float DecibelToVolume(double db)
        {
            return (db == -24) ? 0 : (float)((db < -16) ? MusicMath.DecibelToLinear(db * 2 + 16) : MusicMath.DecibelToLinear(db));
        }
    }
}
