namespace OneLittleStep
{
    /// <summary>Decorating mode (furniture unlocks and placement).</summary>
    public sealed partial class AppController
    {
        void BuildDecor() => ScreenRoot(ScreenId.Decor);
        void EnterDecor() { }
        void ExitDecor() { }
        void UpdateDecorMarkers() { }
    }
}
