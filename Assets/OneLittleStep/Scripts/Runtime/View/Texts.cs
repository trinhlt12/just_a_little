namespace OneLittleStep
{
    /// <summary>All player-facing Vietnamese copy in one place. Gentle wording: no blame, no streak pressure.</summary>
    public static class Texts
    {
        public const string AppTitle = "Một Chút Thôi";
        public const string IntroBody = "Chọn một việc, chọn một bước thật nhỏ, rồi làm trong vài phút. Mèo sẽ ngồi cạnh bạn.\nMỗi phiên tập trung cho bạn Gỗ để sửa sang căn phòng.";
        public const string IntroNote = "Đây là công cụ giúp bắt đầu làm việc và nghỉ ngơi, không phải công cụ chẩn đoán hay điều trị.";
        public const string StartLittle = "Bắt đầu một chút";
        public const string Skip = "Bỏ qua";

        public const string HomeEmpty = "Hôm nay mình làm một chút nhé?";
        public const string NextStepLabel = "Bước nhỏ tiếp theo";
        public const string DoThisStep = "Làm bước này";
        public const string NoStepYet = "Chưa có bước nhỏ tiếp theo.";
        public const string ChooseStep = "Chọn bước nhỏ";
        public const string EditTask = "Sửa việc";
        public const string Decorate = "Trang trí";
        public const string Notes = "Ghi chú";

        public const string TaskLabel = "Việc mình muốn làm";
        public const string TaskPlaceholder = "Ví dụ: Viết báo cáo";
        public const string StepPlaceholder = "Ví dụ: Mở tài liệu";
        public static readonly string[] StepChips = { "Mở tài liệu", "Viết một dòng", "Cất một món đồ" };
        public const string StepHint = "Bước nhỏ càng nhỏ càng dễ bắt đầu.";
        public const string Continue = "Tiếp tục";
        public const string Later = "Để sau";
        public const string Back = "Quay lại";

        public const string StepToDo = "Bước nhỏ sẽ làm";
        public const string HowLong = "Làm trong bao lâu?";
        public const string Begin = "Bắt đầu";
        public const string QuickTest = "10 giây (test)";

        public const string Working = "Đang làm";
        public const string PausedLabel = "Đang tạm dừng";
        public const string Pause = "Tạm dừng";
        public const string Resume = "Tiếp tục";
        public const string SendThought = "Gửi ý nghĩ";
        public const string EndEarly = "Kết thúc sớm";

        public const string NoteTitle = "Ý nghĩ chợt đến";
        public const string NotePlaceholder = "Ghi nhanh rồi quay lại việc nhé...";
        public const string NoteTimerRuns = "Đồng hồ vẫn chạy trong lúc bạn ghi.";
        public const string SaveNote = "Lưu ý nghĩ";
        public const string Close = "Đóng";
        public const string NoteSaved = "Đã lưu ý nghĩ.";

        public const string TimeUp = "Hết giờ rồi!";
        public const string StoppedEarly = "Mình dừng ở đây nhé.";
        public const string HowWasIt = "Phiên vừa rồi thế nào?";
        public const string ShortSession = "Phiên này chưa tới 30 giây nên chưa có Gỗ, nhưng vẫn được ghi nhận. Gỗ được trao sau 30 giây hoạt động.";
        public const string OptDone = "Xong bước này";
        public const string OptLittle = "Đã làm được một chút";
        public const string OptDistracted = "Mình bị phân tâm";
        public const string OptBreak = "Mình cần nghỉ";

        public const string DoneTitle = "Xong một bước rồi!";
        public const string DoNextStep = "Làm bước tiếp theo";
        public const string FinishTask = "Hoàn thành cả việc";
        public const string LittleTitle = "Một chút cũng là tiến bộ.";
        public const string BackToRoom = "Về phòng";
        public const string KeepGoing = "Làm tiếp bước này";
        public const string DistractedTitle = "Không sao cả.";
        public const string DistractedBody = "Bị cuốn đi là chuyện bình thường. Muốn đổi bước nhỏ cho dễ hơn không?";
        public const string Restart = "Bắt đầu lại";
        public const string BreakTitle = "Nghỉ một chút nhé.";
        public const string BreakBody = "Uống ngụm nước, duỗi vai, nhìn ra cửa sổ một lát. Khi nào sẵn sàng thì quay lại.";
        public const string BackFromBreak = "Quay về phòng";
        public const string TaskDoneTitle = "Hoàn thành rồi!";

        public const string SettingsTitle = "Cài đặt";
        public const string ReduceMotionOn = "Giảm chuyển động: Bật";
        public const string ReduceMotionOff = "Giảm chuyển động: Tắt";
        public const string ReduceMotionHint = "Khi bật, mèo và hiệu ứng chuyển cảnh đứng yên.";
        public const string NotesTitle = "Ghi chú của bạn";
        public const string NotesEmpty = "Chưa có ghi chú nào.";

        public const string WoodGained = "+1 Gỗ";

        public static string LittleBody(string step) => $"Bước “{step}” vẫn ở đây khi bạn quay lại.";
        public static string TaskDoneBody(string task) => $"“{task}” đã xong. Căn phòng nhỏ chúc mừng bạn.";
        public static string StayedWith(double seconds) => $"Bạn đã ở bên bước này {Duration(seconds)}.";
        public static string Minutes(int m) => $"{m} phút";

        public static string Duration(double seconds)
        {
            int s = (int)System.Math.Floor(seconds);
            if (s < 60) return $"{s} giây";
            int m = s / 60, r = s % 60;
            return r == 0 ? $"{m} phút" : $"{m} phút {r} giây";
        }

        public static string Clock(double remaining)
        {
            int s = (int)System.Math.Ceiling(remaining - 1e-6);
            if (s < 0) s = 0;
            return $"{s / 60:00}:{s % 60:00}";
        }
    }
}
