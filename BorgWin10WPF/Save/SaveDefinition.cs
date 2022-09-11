using BorgWin10WPF.Puzzles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BorgWin10WPF.Save
{
    public class SaveDefinition
    {
        public string SaveRowType { get; set; }
        public string SaveName { get; set; }
        public string SaveScene { get; set; }
        public int SaveSceneInt { get; set; }
        public int SaveFrame { get; set; }
        public int DoNothingCount { get; set; }

        public int VisitedBorgifiedScenev_12 { get; set; }
        public int Chapter_V_16_ComputerCoreClicks { get; set; }
        public int Chapter_V_17_ComputerCoreClicks { get; set; }
        public int Chapter_V_18_CircuitClicks { get; set; }

        public int BorgIPCount { get; set; }

        public int GridID { get; set; } = 1;
        public int Volume { get; set; } = -1;

        public List<SpecialPuzzleSaveState> puzzlestate = new List<SpecialPuzzleSaveState>();
        
    }
}
