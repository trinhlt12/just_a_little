using System;

namespace OneLittleStep
{
    public struct ReflectionResult
    {
        public bool Accepted;        // false when there was no finished session to reflect on
        public int WoodAwarded;      // 0 or 1
        public bool TooShortForWood; // productive answer but under the 30 s threshold
        public double ActiveSeconds;
    }

    /// <summary>
    /// Wall-clock focus timer. Only active (unpaused) time counts. Time keeps flowing while the app is in the
    /// background because elapsed time is derived from UTC timestamps, never from Time.deltaTime / timeScale.
    /// All state lives in <see cref="SessionData"/> so it survives save / load.
    /// </summary>
    public sealed class FocusTimer
    {
        public const int MinRewardSeconds = 30;

        readonly Func<DateTime> utcNow;
        public SessionData Data { get; private set; }

        public FocusTimer(Func<DateTime> utcNow, SessionData data)
        {
            this.utcNow = utcNow;
            Data = data ?? new SessionData();
            Sanitize();
        }

        public SessionPhase Phase => (SessionPhase)Data.phase;
        public bool IsRunningOrPaused => Phase == SessionPhase.Running || Phase == SessionPhase.Paused;
        public bool IsBusy => Phase != SessionPhase.Idle;
        public int DurationSeconds => Data.durationSeconds;

        public double ActiveSeconds
        {
            get
            {
                double t = Data.bankedSeconds;
                if (Phase == SessionPhase.Running && Data.runningSinceUtcTicks > 0)
                {
                    var since = new DateTime(Data.runningSinceUtcTicks, DateTimeKind.Utc);
                    // A clock moved backwards never produces negative time.
                    t += Math.Max(0, (utcNow() - since).TotalSeconds);
                }
                return Math.Max(0, Math.Min(t, Data.durationSeconds));
            }
        }

        public double RemainingSeconds => Math.Max(0, Data.durationSeconds - ActiveSeconds);

        /// <summary>Starts a new session. Refused while another session exists, so a timer can never run twice.</summary>
        public bool Start(int durationSeconds, string step)
        {
            if (IsBusy || durationSeconds <= 0) return false;
            Data = new SessionData
            {
                id = Guid.NewGuid().ToString("N"),
                phase = (int)SessionPhase.Running,
                durationSeconds = durationSeconds,
                bankedSeconds = 0,
                runningSinceUtcTicks = utcNow().Ticks,
                step = step ?? "",
            };
            return true;
        }

        public bool Pause()
        {
            if (Phase != SessionPhase.Running) return false;
            Data.bankedSeconds = ActiveSeconds;
            Data.runningSinceUtcTicks = 0;
            Data.phase = (int)SessionPhase.Paused;
            return true;
        }

        public bool Resume()
        {
            if (Phase != SessionPhase.Paused) return false;
            Data.phase = (int)SessionPhase.Running;
            Data.runningSinceUtcTicks = utcNow().Ticks;
            Tick();
            return true;
        }

        /// <summary>Moves a running session to reflection once its time is used up. Returns true on that transition only.</summary>
        public bool Tick()
        {
            if (Phase != SessionPhase.Running) return false;
            if (ActiveSeconds < Data.durationSeconds) return false;
            Freeze(endedEarly: false);
            return true;
        }

        public bool EndEarly()
        {
            if (!IsRunningOrPaused) return false;
            if (Tick()) return true;   // time had already run out: treat as a normal finish
            Freeze(endedEarly: true);
            return true;
        }

        void Freeze(bool endedEarly)
        {
            Data.bankedSeconds = ActiveSeconds;
            Data.runningSinceUtcTicks = 0;
            Data.endedEarly = endedEarly;
            Data.phase = (int)SessionPhase.AwaitingReflection;
        }

        /// <summary>
        /// Records how the session went and closes it. Wood is earned only for "done" / "a little" after at least
        /// 30 s of active time, and at most once per session.
        /// </summary>
        public ReflectionResult Reflect(Reflection r)
        {
            if (Phase != SessionPhase.AwaitingReflection) return new ReflectionResult { Accepted = false };
            bool productive = r == Reflection.DoneStep || r == Reflection.DidALittle;
            bool longEnough = Data.bankedSeconds >= MinRewardSeconds;
            int wood = productive && longEnough && !Data.rewardGranted ? 1 : 0;
            if (wood > 0) Data.rewardGranted = true;
            var result = new ReflectionResult
            {
                Accepted = true,
                WoodAwarded = wood,
                TooShortForWood = productive && !longEnough,
                ActiveSeconds = Data.bankedSeconds,
            };
            Data.phase = (int)SessionPhase.Idle;
            Data.runningSinceUtcTicks = 0;
            return result;
        }

        /// <summary>Repairs impossible values from an edited or damaged save.</summary>
        void Sanitize()
        {
            if (!Enum.IsDefined(typeof(SessionPhase), Data.phase)) Data.phase = (int)SessionPhase.Idle;
            if (Data.durationSeconds <= 0 && Phase != SessionPhase.Idle) Data.phase = (int)SessionPhase.Idle;
            if (double.IsNaN(Data.bankedSeconds) || Data.bankedSeconds < 0) Data.bankedSeconds = 0;
            if (Data.runningSinceUtcTicks < 0 || Data.runningSinceUtcTicks > DateTime.MaxValue.Ticks) Data.runningSinceUtcTicks = 0;
            if (Phase == SessionPhase.Running && Data.runningSinceUtcTicks == 0) Data.runningSinceUtcTicks = utcNow().Ticks;
            if (Phase != SessionPhase.Running) Data.runningSinceUtcTicks = 0;
            if (Data.step == null) Data.step = "";
        }
    }
}
