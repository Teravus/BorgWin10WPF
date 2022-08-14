namespace BorgWin10WPF
{
    public abstract class SpecialPuzzleBase
    {
        public abstract string PuzzleInputActiveScene { get; }
        public abstract string PuzzleTriggerActiveScene { get; }

        public abstract SpecialPuzzleResult Click(string ButtonName, bool CheckOnly);
        public abstract void Reset();
        public abstract void Retry();
    }
}