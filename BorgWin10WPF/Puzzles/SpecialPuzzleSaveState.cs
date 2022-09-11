using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BorgWin10WPF.Puzzles
{
    public class SpecialPuzzleSaveState
    {
        // These save and restore elements are really simple. Why make them more complicated?
        public string puzzlename { get; set; } = "";
        public int int0 { get; set; } = 0;
        public int int1 { get; set; } = 0;
        public int int2 { get; set; } = 0;
        public string str0 { get; set; } = "";
        public string str1 { get; set; } = "";
        public string str2 { get; set; } = "";

        public string State2Line()
        {
            return $"{puzzlename},{int0},{int1},{int2},{str0},{str1},{str2}";
        }
    }
}
