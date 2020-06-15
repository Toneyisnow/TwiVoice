using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using TwiVoice.Core.Common;
using TwiVoice.Core.USTx;

namespace TwiVoice.Core.Formats
{
    public class UJson
    {
        [JsonProperty(PropertyName = "setting")]
        public UJsonSetting Setting
        {
            get; set;
        }

        [JsonProperty(PropertyName = "tracks")]
        public List<UJsonTrack> Tracks
        {
            get; set;
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
                tNote.PreUtterance = note.Phonemes[0].Preutter.ToString();

                tNote.Modulation = 0;
                tNote.Moduration = 0;
                tNote.Pbs = string.IsNullOrWhiteSpace(note.Pbs) ? "0" : note.Pbs;
                tNote.Pbw = string.IsNullOrWhiteSpace(note.Pbw) ? "0" : note.Pbw;

                track.Notes.Add(tNote);
            }

            this.Tracks.Add(track);
        }

        public UProject ToUProject()
        {

            return null;
        }

    }

    /// <summary>
    /// 
    /// </summary>
    public class UJsonSetting
    {
        [JsonProperty(PropertyName = "twi_version")]
        public string Version
        {
            get; set;
        }

        [JsonProperty(PropertyName = "tempo")]
        public int Tempo
        {
            get; set;
        }

        [JsonProperty(PropertyName = "tracks")]
        public int TrackCount
        {
            get; set;
        }

        [JsonProperty(PropertyName = "singer_name")]
        public string SingerName
        {
            get; set;
        }

        [JsonProperty(PropertyName = "resampler_file")]
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
        [JsonProperty(PropertyName = "track_id")]
        public int Id
        {
            get; set;
        }

        [JsonProperty(PropertyName = "notes")]
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
        [JsonProperty(PropertyName = "note_index")]
        public int Index
        {
            get; set;
        }


        [JsonProperty(PropertyName = "length")]
        public int Length
        {
            get; set;
        }

        [JsonProperty(PropertyName = "lyric")]
        public string Lyric
        {
            get; set;
        }

        [JsonProperty(PropertyName = "note_num")]
        public int NoteNum
        {
            get; set;
        }

        [JsonProperty(PropertyName = "intensity")]
        public int Intensity
        {
            get; set;
        }

        [JsonProperty(PropertyName = "pre_utterance")]
        public string PreUtterance
        {
            get; set;
        }

        [JsonProperty(PropertyName = "modulation")]
        public int Modulation
        {
            get; set;
        }

        [JsonProperty(PropertyName = "moduration")]
        public int Moduration
        {
            get; set;
        }

        [JsonProperty(PropertyName = "pbs")]
        public string Pbs
        {
            get; set;
        }

        [JsonProperty(PropertyName = "pbw")]
        public string Pbw
        {
            get; set;
        }
    }
}
