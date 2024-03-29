﻿namespace BorgWin10WPF.Puzzles
{
    public abstract class SpecialPuzzleBase
    {
        public abstract string PuzzleInputActiveScene { get; }
        public abstract string PuzzleTriggerActiveScene { get; }

        public abstract bool PuzzleTriggersOnScene(string SceneName);

        public abstract SpecialPuzzleResult Click(string ButtonName, bool CheckOnly);
        public abstract void Reset();
        public abstract void Retry();
        public abstract SpecialPuzzleSaveState GetSaveState();

        public abstract void LoadSaveState(SpecialPuzzleSaveState state);

        public abstract string Name { get; }

    }
}