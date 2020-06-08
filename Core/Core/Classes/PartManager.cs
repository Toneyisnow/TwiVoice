using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using TwiVoice.Core.USTx;

namespace TwiVoice.Core
{
    public class PartManager
    {
        class PartContainer
        {
            public UVoicePart Part = null;
        }

        Timer timer;

        UProject _project;
        PartContainer _partContainer;

        public PartManager(UProject project)
        {
            _partContainer = new PartContainer();
            _project = project;

            timer = new Timer(Update, _partContainer, 0, 100);
        }

        private void Update(Object state)
        {
            var partContainer = state as PartContainer;
            if (partContainer.Part == null) return;
            UpdatePart(partContainer.Part);
        }

        public void UpdatePart(UVoicePart part)
        {
            lock (part)
            {
                if (part == null) return;
                CheckOverlappedNotes(part);
                UpdatePhonemeDurTick(part);
                UpdatePhonemeOto(part);
                UpdateOverlapAdjustment(part);
                UpdateEnvelope(part);
                UpdatePitchBend(part);
            }
        }

        private void UpdatePitchBend(UVoicePart part)
        {
            UNote lastNote = null;
            foreach (UNote note in part.Notes)
            {
                if (note.PitchBend.SnapFirst)
                {
                    if (note.Phonemes.Count > 0 && lastNote != null && (note.Phonemes[0].Overlapped || note.PosTick == lastNote.EndTick))
                        note.PitchBend.Points[0].Y = (lastNote.NoteNum - note.NoteNum) * 10;
                    else
                        note.PitchBend.Points[0].Y = 0;
                }
                lastNote = note;
            }
        }

        public void ResnapPitchBend(UVoicePart part)
        {
            UNote lastNote = null;
            foreach (UNote note in part.Notes)
            {
                if (!note.PitchBend.SnapFirst)
                {
                    if (note.Phonemes.Count > 0 && note.Phonemes[0].Overlapped && lastNote != null)
                        if (note.PitchBend.Points[0].Y == (lastNote.NoteNum - note.NoteNum) * 10)
                            note.PitchBend.SnapFirst = true;
                }
                lastNote = note;
            }
        }

        private void UpdateEnvelope(UVoicePart part)
        {
            foreach (UNote note in part.Notes)
            {
                foreach (UPhoneme phoneme in note.Phonemes)
                {
                    phoneme.Envelope.Points[0].X = -phoneme.Preutter;
                    phoneme.Envelope.Points[1].X = phoneme.Envelope.Points[0].X + (phoneme.Overlapped ? phoneme.Overlap : 5);
                    phoneme.Envelope.Points[2].X = Math.Max(0, phoneme.Envelope.Points[1].X);
                    phoneme.Envelope.Points[3].X = _project.TickToMillisecond(phoneme.DurTick) - phoneme.TailIntrude;
                    phoneme.Envelope.Points[4].X = phoneme.Envelope.Points[3].X + phoneme.TailOverlap;

                    phoneme.Envelope.Points[1].Y = (int)phoneme.Parent.Expressions["volume"].Data;
                    phoneme.Envelope.Points[1].X = phoneme.Envelope.Points[0].X + (phoneme.Overlapped ? phoneme.Overlap : 5) * (int)phoneme.Parent.Expressions["accent"].Data / 100.0;
                    phoneme.Envelope.Points[1].Y = (int)phoneme.Parent.Expressions["accent"].Data * (int)phoneme.Parent.Expressions["volume"].Data / 100;
                    phoneme.Envelope.Points[2].Y = (int)phoneme.Parent.Expressions["volume"].Data;
                    phoneme.Envelope.Points[3].Y = (int)phoneme.Parent.Expressions["volume"].Data;
                    phoneme.Envelope.Points[3].X -= (phoneme.Envelope.Points[3].X - phoneme.Envelope.Points[2].X) * (int)phoneme.Parent.Expressions["decay"].Data / 500;
                    phoneme.Envelope.Points[3].Y *= 1.0 - (int)phoneme.Parent.Expressions["decay"].Data / 100.0;
                }
            }
        }

        private void UpdateOverlapAdjustment(UVoicePart part)
        {
            UPhoneme lastPhoneme = null;
            UNote lastNote = null;
            foreach (UNote note in part.Notes)
            {
                foreach (UPhoneme phoneme in note.Phonemes)
                {
                    if (lastPhoneme != null)
                    {
                        int gapTick = phoneme.Parent.PosTick + phoneme.PosTick - lastPhoneme.Parent.PosTick - lastPhoneme.EndTick;
                        double gapMs = _project.TickToMillisecond(gapTick);
                        if (gapMs < phoneme.Preutter)
                        {
                            phoneme.Overlapped = true;
                            double lastDurMs = _project.TickToMillisecond(lastPhoneme.DurTick);
                            double correctionRatio = (lastDurMs + Math.Min(0, gapMs)) / 2 / (phoneme.Preutter - phoneme.Overlap);
                            if (phoneme.Preutter - phoneme.Overlap > gapMs + lastDurMs / 2)
                            {
                                phoneme.OverlapCorrection = true;
                                phoneme.Preutter = gapMs + (phoneme.Preutter - gapMs) * correctionRatio;
                                phoneme.Overlap *= correctionRatio;
                            }
                            else if (phoneme.Preutter > gapMs + lastDurMs)
                            {
                                phoneme.OverlapCorrection = true;
                                phoneme.Overlap *= correctionRatio; 
                                phoneme.Preutter = gapMs + lastDurMs;
                            }
                            else
                                phoneme.OverlapCorrection = false;

                            lastPhoneme.TailIntrude = phoneme.Preutter - gapMs;
                            lastPhoneme.TailOverlap = phoneme.Overlap;

                        }
                        else
                        {
                            phoneme.Overlapped = false;
                            lastPhoneme.TailIntrude = 0;
                            lastPhoneme.TailOverlap = 0;
                        }
                    }
                    else phoneme.Overlapped = false;
                    lastPhoneme = phoneme;
                }
                lastNote = note;
            }
        }

        private void UpdatePhonemeOto(UVoicePart part)
        {
            var singer = _project.Tracks[part.TrackNo].Singer;
            if (singer == null || !singer.Loaded) return;
            foreach (UNote note in part.Notes)
            {
                foreach (UPhoneme phoneme in note.Phonemes)
                {
                    if (phoneme.AutoRemapped)
                    {
                        if (phoneme.Phoneme.StartsWith("?"))
                        {
                            phoneme.Phoneme = phoneme.Phoneme.Substring(1);
                            phoneme.AutoRemapped = false;
                        }
                        else
                        {
                            string noteString = MusicMath.GetNoteString(note.NoteNum);
                            if (singer.PitchMap.ContainsKey(noteString))
                                phoneme.RemappedBank = singer.PitchMap[noteString];
                        }
                    }

                    if (singer.AliasMap.ContainsKey(phoneme.PhonemeRemapped))
                    {
                        phoneme.Oto = singer.AliasMap[phoneme.PhonemeRemapped];
                        phoneme.PhonemeError = false;
                        phoneme.Overlap = phoneme.Oto.Overlap;
                        phoneme.Preutter = phoneme.Oto.Preutter;
                        int vel = (int)phoneme.Parent.Expressions["velocity"].Data;
                        if (vel != 100)
                        {
                            double stretchRatio = Math.Pow(2, 1.0 - (double)vel / 100);
                            phoneme.Overlap *= stretchRatio;
                            phoneme.Preutter *= stretchRatio;
                        }
                    }
                    else
                    {
                        phoneme.PhonemeError = true;
                        phoneme.Overlap = 0;
                        phoneme.Preutter = 0;
                    }
                }
            }
        }

        private void UpdatePhonemeDurTick(UVoicePart part)
        {
            UNote lastNote = null;
            UPhoneme lastPhoneme = null;
            foreach (UNote note in part.Notes)
            {
                foreach(UPhoneme phoneme in note.Phonemes)
                {
                    phoneme.DurTick = phoneme.Parent.DurTick - phoneme.PosTick;
                    if (lastPhoneme != null)
                        if (lastPhoneme.Parent == phoneme.Parent)
                            lastPhoneme.DurTick = phoneme.PosTick - lastPhoneme.PosTick;
                    lastPhoneme = phoneme;
                }
                lastNote = note;
            }
        }

        private void CheckOverlappedNotes(UVoicePart part)
        {
            UNote lastNote = null;
            foreach (UNote note in part.Notes)
            {
                if (lastNote != null && lastNote.EndTick > note.PosTick)
                {
                    lastNote.Error = true;
                    note.Error = true;
                }
                else note.Error = false;
                lastNote = note;
            }
        }

        # region Cmd Handling


        # endregion

        # region ICmdSubscriber

        # endregion

    }
}
