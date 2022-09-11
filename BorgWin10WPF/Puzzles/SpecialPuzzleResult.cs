using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BorgWin10WPF.Puzzles
{
    public class SpecialPuzzleResult
    {
        public bool OverrideNeeded { get; set; } = false;
        public string JumpToScene { get; set; } = "";

        public bool SuccessYN { get; set; } = false;


    }
}
