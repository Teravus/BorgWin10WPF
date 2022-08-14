using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BorgWin10WPF
{
    public class HyposprayFormulaPuzzle : SpecialPuzzleBase
    {
        private string ButtonsPressedSoFar { get; set; } = String.Empty;
        private string BijaniAdrenalineFormula { get; set; } = "1331";
        public string BijaniAdrenalineBlock { get; set; } = "1332";

        private string HumanAdrenalineFormula { get; set; } = "1231";
        private string HumanAdrenalineBlock { get; set; } = "1232";

        private string Level1NeuralBlock { get; set; } = "2311";
        private string Level2NeuralBlock { get; set; } = "2312";
        private string Level3NeuralBlock { get; set; } = "2313";


        private int FailedCount { get; set; } = 0;
        private int ClicksSoFar { get; set; } = 0;

        private Dictionary<string, string> _combinationScenes = new Dictionary<string, string>();

        public override string PuzzleInputActiveScene { get; } = "V_25";
        public override string PuzzleTriggerActiveScene { get; } = "V_26";

        public HyposprayFormulaPuzzle()
        {
            _combinationScenes.Add(BijaniAdrenalineFormula, "V_30");
            _combinationScenes.Add(BijaniAdrenalineBlock, "V_30");
            _combinationScenes.Add(HumanAdrenalineFormula, "V_30");
            _combinationScenes.Add(HumanAdrenalineBlock, "V_30");
            _combinationScenes.Add(Level1NeuralBlock, "V_30");
            _combinationScenes.Add(Level2NeuralBlock, "V_30");
            _combinationScenes.Add(Level3NeuralBlock, "V_30");
        }

        public override SpecialPuzzleResult Click(string ButtonName, bool checkonly)
        {

            SpecialPuzzleResult result = new SpecialPuzzleResult();

            string buttonnumber = "";

            switch (ButtonName)
            {

                case "D25B1":
                    buttonnumber = "1";
                    break;
                case "D25B2":
                    buttonnumber = "2";
                    break;
                case "D25B3":
                    buttonnumber = "3";
                    break;
            }
            if (!checkonly)
            {
                ++ClicksSoFar;
                ButtonsPressedSoFar += buttonnumber;
                if (_combinationScenes.ContainsKey(ButtonsPressedSoFar))
                {
                    result.JumpToScene = _combinationScenes[ButtonsPressedSoFar];
                    result.SuccessYN = ButtonsPressedSoFar == BijaniAdrenalineFormula ? true : false;
                    result.OverrideNeeded = true;
                    Retry();

                }
                else
                {
                    if (ButtonName == "Idle")
                    {
                        result.SuccessYN = ButtonsPressedSoFar == BijaniAdrenalineFormula ? true : false;
                        result.OverrideNeeded = true;
                        result.JumpToScene = "D30FL";
                        Reset();
                        return result; ;
                    }
                    if (ClicksSoFar >= 3)
                    {
                        // Go to a bad scene
                    }
                }
            }

            return result;
        }

        public override void Reset()
        {
            FailedCount = 0;
            ButtonsPressedSoFar = "";
            ClicksSoFar = 0;
        }
        public override void Retry()
        {
            ButtonsPressedSoFar = "";
            ClicksSoFar = 0;
        }

    }
}
