using System;
using System.Collections.Generic;

namespace OneLittleStep
{
    public enum SessionPhase { Idle = 0, Running = 1, Paused = 2, AwaitingReflection = 3 }

    public enum Reflection { DoneStep = 0, DidALittle = 1, Distracted = 2, NeedBreak = 3 }

    [Serializable]
    public sealed class TaskData
    {
        public bool active;
        public string title = "";
        public string step = "";
    }

    [Serializable]
    public sealed class NoteData
    {
        public string text = "";
        public long createdUtcTicks;
        public string step = "";
    }

    [Serializable]
    public sealed class SessionData
    {
        public string id = "";
        public int phase;                 // SessionPhase
        public int durationSeconds;
        public double bankedSeconds;      // active time before the current running stretch
        public long runningSinceUtcTicks; // 0 unless running
        public bool endedEarly;
        public bool rewardGranted;
        public string step = "";
    }

    [Serializable]
    public sealed class PlacementData
    {
        public string slot = "";
        public string item = "";
    }

    /// <summary>Everything that is persisted. Plain fields so JsonUtility can round-trip it.</summary>
    [Serializable]
    public sealed class SaveData
    {
        public const int CurrentVersion = 1;

        public int version = CurrentVersion;
        public bool introSeen;
        public TaskData task = new TaskData();
        public List<NoteData> notes = new List<NoteData>();
        public int wood;
        public List<string> unlocked = new List<string>();
        public List<PlacementData> placements = new List<PlacementData>();
        public bool reduceMotion;
        public SessionData session = new SessionData();
        public int sessionsRecorded;
        public int tasksCompleted;
        public string lastSavedUtc = "";
    }

    /// <summary>Text clean-up shared by every input field.</summary>
    public static class TextRules
    {
        public const int TaskMax = 60;
        public const int StepMax = 80;
        public const int NoteMax = 280;
        public const int MaxNotes = 200;

        /// <summary>Collapses whitespace and line breaks, trims, and cuts to max characters without splitting a surrogate pair.</summary>
        public static string Clean(string s, int max)
        {
            if (string.IsNullOrEmpty(s)) return "";
            var sb = new System.Text.StringBuilder(s.Length);
            bool space = false;
            foreach (var ch in s)
            {
                if (char.IsWhiteSpace(ch) || char.IsControl(ch))
                {
                    space = sb.Length > 0;
                    continue;
                }
                if (space) { sb.Append(' '); space = false; }
                sb.Append(ch);
            }
            var r = sb.ToString();
            if (r.Length > max)
            {
                int cut = max;
                if (char.IsHighSurrogate(r[cut - 1])) cut--;
                r = r.Substring(0, cut).TrimEnd();
            }
            return r;
        }

        public static bool IsBlank(string s) => Clean(s, int.MaxValue).Length == 0;
    }
}
