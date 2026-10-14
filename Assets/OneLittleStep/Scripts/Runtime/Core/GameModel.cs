using System;

namespace OneLittleStep
{
    /// <summary>
    /// Game rules on top of <see cref="SaveData"/>: the current task and step, quick notes, focus sessions and
    /// the one-time wood reward. No Unity types here, so it is easy to test. Every state change raises
    /// <see cref="Changed"/>; the view saves on that signal (never every frame).
    /// </summary>
    public sealed class GameModel
    {
        readonly Func<DateTime> utcNow;
        public SaveData Data { get; }
        public FocusTimer Timer { get; }

        /// <summary>Raised after any persisted state changed.</summary>
        public event Action Changed;

        public GameModel(SaveData data, Func<DateTime> utcNow)
        {
            this.utcNow = utcNow;
            Data = data ?? new SaveData();
            if (Data.task == null) Data.task = new TaskData();
            if (Data.notes == null) Data.notes = new System.Collections.Generic.List<NoteData>();
            if (Data.session == null) Data.session = new SessionData();
            if (Data.wood < 0) Data.wood = 0;
            Timer = new FocusTimer(utcNow, Data.session);
            Data.session = Timer.Data;
        }

        void Notify() => Changed?.Invoke();

        // ------------------------------------------------------------------ task & step
        public bool HasTask => Data.task.active && !TextRules.IsBlank(Data.task.title);
        public bool HasStep => HasTask && !TextRules.IsBlank(Data.task.step);
        public string TaskTitle => HasTask ? Data.task.title : "";
        public string Step => HasStep ? Data.task.step : "";

        /// <summary>Creates or edits the task. Both fields must contain more than whitespace.</summary>
        public bool SetTask(string title, string step)
        {
            var t = TextRules.Clean(title, TextRules.TaskMax);
            var s = TextRules.Clean(step, TextRules.StepMax);
            if (t.Length == 0 || s.Length == 0 || Timer.IsRunningOrPaused) return false;
            Data.task.active = true;
            Data.task.title = t;
            Data.task.step = s;
            Notify();
            return true;
        }

        public bool SetStep(string step)
        {
            var s = TextRules.Clean(step, TextRules.StepMax);
            if (!HasTask || s.Length == 0 || Timer.IsRunningOrPaused) return false;
            Data.task.step = s;
            Notify();
            return true;
        }

        /// <summary>The step is done and no next step was chosen yet.</summary>
        public void ClearStep()
        {
            if (!HasTask) return;
            Data.task.step = "";
            Notify();
        }

        public bool CompleteTask()
        {
            if (!HasTask || Timer.IsRunningOrPaused) return false;
            Data.task = new TaskData();
            Data.tasksCompleted++;
            Notify();
            return true;
        }

        public void MarkIntroSeen()
        {
            if (Data.introSeen) return;
            Data.introSeen = true;
            Notify();
        }

        public void SetReduceMotion(bool on)
        {
            if (Data.reduceMotion == on) return;
            Data.reduceMotion = on;
            Notify();
        }

        // ------------------------------------------------------------------ notes
        public bool AddNote(string text)
        {
            var t = TextRules.Clean(text, TextRules.NoteMax);
            if (t.Length == 0) return false;
            Data.notes.Add(new NoteData { text = t, createdUtcTicks = utcNow().Ticks, step = Timer.IsBusy ? Timer.Data.step : Step });
            while (Data.notes.Count > TextRules.MaxNotes) Data.notes.RemoveAt(0);
            Notify();
            return true;
        }

        // ------------------------------------------------------------------ focus session
        public bool StartSession(int seconds)
        {
            if (!HasStep) return false;
            if (!Timer.Start(seconds, Data.task.step)) return false;
            Data.session = Timer.Data;
            Notify();
            return true;
        }

        public bool Pause() => Do(Timer.Pause());
        public bool Resume() => Do(Timer.Resume());
        public bool EndEarly() => Do(Timer.EndEarly());

        /// <summary>Call regularly (and on resume / load). True only at the moment the time runs out.</summary>
        public bool Tick() => Do(Timer.Tick());

        bool Do(bool changed)
        {
            if (changed) Notify();
            return changed;
        }

        /// <summary>Closes the session with the player's own assessment and grants wood at most once.</summary>
        public ReflectionResult Reflect(Reflection r)
        {
            var result = Timer.Reflect(r);
            if (!result.Accepted) return result;
            Data.wood += result.WoodAwarded;
            Data.sessionsRecorded++;
            Notify();
            return result;
        }
    }
}
