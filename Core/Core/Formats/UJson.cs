using Newtonsoft.Json;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using TwiVoice.Core.Common;
using TwiVoice.Core.USTx;
using static TwiVoice.Core.Formats.Ust;

namespace TwiVoice.Core.Formats
{
    public class UJson
    {
        //// [JsonProperty(PropertyName = "setting")]
        public UJsonSetting Setting
        {
            get; set;
        }

        //// [JsonProperty(PropertyName = "tracks")]
        public List<UJsonTrack> Tracks
        {
            get; set;
        }

        public static UJson Create(string ustFileFullPath, string resamplerFullPath, string singerName)
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

        public void LoadTracksFromUst(string ustFileFullPath)
        {
            var twiConfig = TwiConfig.LoadFromFile();

            string singerFolder = Path.Combine(twiConfig.SingersRootPath, this.Setting.SingerName);
            var uProject = Ust.Load(ustFileFullPath, singerFolder);

            this.Tracks = new List<UJsonTrack>();
            UJsonTrack track = new UJsonTrack();
            track.Id = 0;
            track.Notes = new List<UJsonNote>();

            var voicePart = uProject.Parts[0] as UVoicePart;
            int noteIndex = 0;
            foreach (UNote note in voicePart.Notes)
            {
                UJsonNote tNote = new UJsonNote();
                tNote.Index = noteIndex++;
                tNote.Length = note.DurTick;
                tNote.Lyric = note.Lyric;
                tNote.NoteNum = note.NoteNum;

                tNote.Intensity = (int)note.Expressions["volume"].Data;
                tNote.Velocity = (int)note.Expressions["velocity"].Data;
                tNote.PreUtterance = note.Phonemes[0].Preutter.ToString();

                tNote.VoiceOverlap = note.Phonemes[0].Overlap;

                tNote.Modulation = 0;
                tNote.Moduration = 0;
                tNote.Pbs = string.IsNullOrWhiteSpace(note.Pbs) ? "0" : note.Pbs;
                tNote.Pbw = string.IsNullOrWhiteSpace(note.Pbw) ? "0" : note.Pbw;
                tNote.Pby = string.IsNullOrWhiteSpace(note.Pby) ? "0" : note.Pby;
                tNote.Pbm = string.IsNullOrWhiteSpace(note.Pbm) ? "0" : note.Pbm;

                track.Notes.Add(tNote);
            }

            this.Tracks.Add(track);
        }

        public UProject ToUProject()
        {
            UProject project = new UProject() { Resolution = 480, Saved = false };

            UstVersion version = UstVersion.Early;
            UstBlock currentBlock = UstBlock.None;
            TwiConfig twiConfig = TwiConfig.LoadFromFile();

            project.RegisterExpression(new IntExpression(null, "velocity", "VEL") { Data = 100, Min = 0, Max = 200 });
            project.RegisterExpression(new IntExpression(null, "volume", "VOL") { Data = 100, Min = 0, Max = 200 });
            project.RegisterExpression(new IntExpression(null, "gender", "GEN") { Data = 0, Min = -100, Max = 100 });
            project.RegisterExpression(new IntExpression(null, "lowpass", "LPF") { Data = 0, Min = 0, Max = 100 });
            project.RegisterExpression(new IntExpression(null, "highpass", "HPF") { Data = 0, Min = 0, Max = 100 });
            project.RegisterExpression(new IntExpression(null, "accent", "ACC") { Data = 100, Min = 0, Max = 200 });
            project.RegisterExpression(new IntExpression(null, "decay", "DEC") { Data = 0, Min = 0, Max = 100 });

            // Settings
            project.BPM = this.Setting.Tempo;

            var _track = new UTrack();
            project.Tracks.Add(_track);
            _track.TrackNo = 0;
            UVoicePart part = new UVoicePart() { TrackNo = 0, PosTick = 0 };
            project.Parts.Add(part);
            
            string singerFolder = Path.Combine(twiConfig.SingersRootPath, this.Setting.SingerName);
            var singer = UtauSoundbank.LoadSinger(singerFolder);
            project.Singers.Add(singer);
            project.Tracks[0].Singer = singer;

            
            int currentTick = 0;
            foreach (var note in this.Tracks[0].Notes)
            {
                UNote currentNote = UNoteFromJson(project.CreateNote(), note, version);
                currentNote.PosTick = currentTick;
                if (!currentNote.Lyric.Replace("R", string.Empty).Replace("r", string.Empty).Equals(string.Empty))
                {
                    part.Notes.Add(currentNote);
                }

                currentTick += currentNote.DurTick;
            }

            part.DurTick = currentTick;
            return project;
        }

        private UNote UNoteFromJson(UNote uNote, UJsonNote jsonNote, UstVersion verison)
        {
            uNote.Phonemes[0].Phoneme = uNote.Lyric = jsonNote.Lyric;
            uNote.DurTick = jsonNote.Length;
            uNote.NoteNum = jsonNote.NoteNum;

            uNote.Expressions["velocity"].Data = jsonNote.Velocity;
            uNote.Expressions["volume"].Data = jsonNote.Intensity;

            if (jsonNote.VoiceOverlap > 0)
            {
                uNote.Phonemes[0].Overlap = jsonNote.VoiceOverlap;
            }

            var pbs = jsonNote.Pbs;
            var pbw = jsonNote.Pbw;
            var pby = jsonNote.Pby;
            var pbm = jsonNote.Pbm;

            if (pbs != string.Empty)
            {
                var pts = uNote.PitchBend.Data as List<PitchPoint>;
                pts.Clear();
                // PBS
                if (pbs.Contains(';'))
                {
                    pts.Add(new PitchPoint(double.Parse(pbs.Split(new[] { ';' })[0]), double.Parse(pbs.Split(new[] { ';' })[1])));
                    uNote.PitchBend.SnapFirst = false;
                }
                else
                {
                    pts.Add(new PitchPoint(double.Parse(pbs), 0));
                    uNote.PitchBend.SnapFirst = true;
                }

                double x = pts.First().X;
                if (pbw != string.Empty)
                {
                    string[] w = pbw.Split(new[] { ',' });
                    string[] y = null;
                    if (w.Count() > 1) y = pby.Split(new[] { ',' });
                    for (int i = 0; i < w.Count() - 1; i++)
                    {
                        x += string.IsNullOrEmpty(w[i]) ? 0 : float.Parse(w[i]);
                        pts.Add(new PitchPoint(x, string.IsNullOrEmpty(y[i]) ? 0 : double.Parse(y[i])));
                    }
                    pts.Add(new PitchPoint(x + double.Parse(w[w.Count() - 1]), 0));
                }
                if (pbm != string.Empty)
                {
                    string[] m = pbw.Split(new[] { ',' });
                    for (int i = 0; i < m.Count() - 1; i++)
                    {
                        pts[i].Shape = m[i] == "r" ? PitchPointShape.o :
                                       m[i] == "s" ? PitchPointShape.l :
                                       m[i] == "j" ? PitchPointShape.i : PitchPointShape.io;
                    }
                }
            }

            return uNote;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class UJsonSetting
    {
        //// [JsonProperty(PropertyName = "twi_version")]
        public string Version
        {
            get; set;
        }

        //// [JsonProperty(PropertyName = "tempo")]
        public int Tempo
        {
            get; set;
        }

        //// [JsonProperty(PropertyName = "tracks")]
        public int TrackCount
        {
            get; set;
        }

        //// [JsonProperty(PropertyName = "singer_name")]
        public string SingerName
        {
            get; set;
        }

        //// [JsonProperty(PropertyName = "resampler_file")]
        public string ResamplerFile
        {
            get; set;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class UJsonTrack
    {
        //// [JsonProperty(PropertyName = "track_id")]
        public int Id
        {
            get; set;
        }

        //// [JsonProperty(PropertyName = "notes")]
        public List<UJsonNote> Notes
        {
            get; set;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class UJsonNote
    {
        //// [JsonProperty(PropertyName = "note_index")]
        public int Index
        {
            get; set;
        }


        //// [JsonProperty(PropertyName = "length")]
        public int Length
        {
            get; set;
        }

        //// [JsonProperty(PropertyName = "lyric")]
        public string Lyric
        {
            get; set;
        }

        //// [JsonProperty(PropertyName = "note_num")]
        public int NoteNum
        {
            get; set;
        }

        //// [JsonProperty(PropertyName = "intensity")]
        public int Intensity
        {
            get; set;
        }

        //// [JsonProperty(PropertyName = "velocity")]
        public int Velocity
        {
            get; set;
        }

        //// [JsonProperty(PropertyName = "voice_overlap")]
        public double VoiceOverlap
        {
            get; set;
        }


        //// [JsonProperty(PropertyName = "pre_utterance")]
        public string PreUtterance
        {
            get; set;
        }

        //// [JsonProperty(PropertyName = "modulation")]
        public int Modulation
        {
            get; set;
        }

        //// [JsonProperty(PropertyName = "moduration")]
        public int Moduration
        {
            get; set;
        }

        //// [JsonProperty(PropertyName = "pbs")]
        public string Pbs
        {
            get; set;
        }

        //// [JsonProperty(PropertyName = "pbw")]
        public string Pbw
        {
            get; set;
        }

        //// [JsonProperty(PropertyName = "pby")]
        public string Pby
        {
            get; set;
        }

        //// [JsonProperty(PropertyName = "pbm")]
        public string Pbm
        {
            get; set;
        }
    }
}
