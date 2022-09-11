using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BorgWin10WPF.Puzzles
{
    public class HyposprayFormulaPuzzle : SpecialPuzzleBase
    {
        private string ButtonsPressedSoFar { get; set; } = String.Empty;
        private string BijaniAdrenalineFormulaNeck { get; set; } = "1331n";
        private string BijaniAdrenalineFormulaHand { get; set; } = "1331h";
        public string BijaniAdrenalineBlockNeck { get; set; } = "1332n";
        public string BijaniAdrenalineBlockHand { get; set; } = "1332h";

        private string HumanAdrenalineFormulaNeck { get; set; } = "1231n";
        private string HumanAdrenalineFormulaHand { get; set; } = "1231h";
        private string HumanAdrenalineBlockNeck { get; set; } = "1232n";
        private string HumanAdrenalineBlockHand { get; set; } = "1232h";

        private string Level1NeuralBlockNeck { get; set; } = "2311n";
        private string Level1NeuralBlockHand { get; set; } = "2311h";
        private string Level2NeuralBlockNeck { get; set; } = "2312n";
        private string Level2NeuralBlockHand { get; set; } = "2312h";
        private string Level3NeuralBlockNeck { get; set; } = "2313n";
        private string Level3NeuralBlockHand { get; set; } = "2313h";
        private string LastTriggerSceneNameTest { get; set; } = "";

        private int FailedCount { get; set; } = 0;
        private int ClicksSoFar { get; set; } = 0;

        private Dictionary<string, string> _combinationScenes = new Dictionary<string, string>();

        public override string PuzzleInputActiveScene { get; } = "V_25";
        public override string PuzzleTriggerActiveScene { get; } = "V_28";
        public override bool PuzzleTriggersOnScene(string SceneName)
        {
            LastTriggerSceneNameTest = SceneName;
            if (SceneName.ToLowerInvariant() == PuzzleTriggerActiveScene.ToLowerInvariant())
                return true;
            return false;
        }
        public HyposprayFormulaPuzzle()
        {
            _combinationScenes.Add(BijaniAdrenalineFormulaNeck, "D31KS");
            _combinationScenes.Add(BijaniAdrenalineBlockNeck, "D30QF");
            _combinationScenes.Add(HumanAdrenalineFormulaNeck, "D31KS");
            _combinationScenes.Add(HumanAdrenalineBlockNeck, "D30QF");
            _combinationScenes.Add(Level1NeuralBlockNeck, "D30QF");
            _combinationScenes.Add(Level2NeuralBlockNeck, "D30QF");
            _combinationScenes.Add(Level3NeuralBlockNeck, "D30QM");
            _combinationScenes.Add(BijaniAdrenalineFormulaHand, "V_30");
            _combinationScenes.Add(BijaniAdrenalineBlockHand, "D32K1");
            _combinationScenes.Add(HumanAdrenalineFormulaHand, "D32K1");
            _combinationScenes.Add(HumanAdrenalineBlockHand, "D32K1");
            _combinationScenes.Add(Level1NeuralBlockHand, "D32K1");
            _combinationScenes.Add(Level2NeuralBlockHand, "D32K1");
            _combinationScenes.Add(Level3NeuralBlockHand, "D32K4");
        }

        public override SpecialPuzzleResult Click(string ButtonName, bool checkonly)
        {
            //v_25, v_26
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
                case "D28BC":
                    buttonnumber = "n";
                    break;
                case "D28PV":
                    buttonnumber = "h";
                    break;
            }
            if (!checkonly)
            {
                ++ClicksSoFar;
                ButtonsPressedSoFar += buttonnumber;
                if (_combinationScenes.ContainsKey(ButtonsPressedSoFar))
                {
                    result.JumpToScene = _combinationScenes[ButtonsPressedSoFar];
                    result.SuccessYN = ButtonsPressedSoFar == BijaniAdrenalineFormulaNeck ? true : false;
                    result.OverrideNeeded = true;
                    if (ButtonsPressedSoFar != BijaniAdrenalineFormulaNeck)
                    {
                        Retry();
                    }
                    else
                    {
                        // Remove last button so user doesn't need to enter the code again.
                        ButtonsPressedSoFar = ButtonsPressedSoFar.Substring(0, ButtonsPressedSoFar.Length - 1);
                    }

                }
                else
                {
                    if (ButtonName == "Idle")
                    {
                        result.SuccessYN = ButtonsPressedSoFar == BijaniAdrenalineFormulaNeck ? true : false;
                        result.OverrideNeeded = true;
                        result.JumpToScene = "D30FL";
                        Reset();
                        return result;
                    }
                    if (buttonnumber == "h" )
                    {
                        result.SuccessYN = false;
                        result.OverrideNeeded = true;
                        result.JumpToScene = "D32K1";
                        Reset();
                        
                        return result;
                    } 
                    else if (buttonnumber == "n")
                    {
                        result.SuccessYN = false;
                        result.OverrideNeeded = true;
                        result.JumpToScene = "D30QF";
                        Reset();

                        return result;
                    }
                    else
                    {
                        if (ClicksSoFar > 3 && ClicksSoFar < 5)
                        {
                            result.SuccessYN = ButtonsPressedSoFar == BijaniAdrenalineFormulaNeck ? true : false;
                            result.OverrideNeeded = true;
                            result.JumpToScene = "V_26";

                            return result;
                            // Go to a bad scene
                        }
                        // These are for if the puzzle input goes off the rails.   Realistically, this shouldn't happen unless they click random buttons.
                        if (ClicksSoFar >= 5)
                        {
                            if (LastTriggerSceneNameTest.ToLowerInvariant() == "")
                            {
                                result.SuccessYN = ButtonsPressedSoFar == BijaniAdrenalineFormulaNeck ? true : false;
                                result.OverrideNeeded = true;
                                result.JumpToScene = "V_25";
                                Reset();
                                return result;
                            }
                            if (LastTriggerSceneNameTest.ToLowerInvariant() == "v_25")
                            {
                                result.SuccessYN = ButtonsPressedSoFar == BijaniAdrenalineFormulaNeck ? true : false;
                                result.OverrideNeeded = true;
                                result.JumpToScene = "V_26";
                                Reset();
                                return result;
                            }
                            if (LastTriggerSceneNameTest.ToLowerInvariant() == "v_26")
                            {
                                result.SuccessYN = ButtonsPressedSoFar == BijaniAdrenalineFormulaNeck ? true : false;
                                result.OverrideNeeded = true;
                                result.JumpToScene = "V_28";
                                Reset();
                                return result;
                            }
                            if (LastTriggerSceneNameTest.ToLowerInvariant() == "v_28")
                            {
                                result.SuccessYN = ButtonsPressedSoFar == BijaniAdrenalineFormulaNeck ? true : false;
                                result.OverrideNeeded = true;
                                result.JumpToScene = "D32K2";
                                Reset();
                                return result;
                            }
                        }
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

        public override string Name => "HypoSprayPuzzle";

        public override SpecialPuzzleSaveState GetSaveState()
        {
            SpecialPuzzleSaveState result = new SpecialPuzzleSaveState();
            result.puzzlename = Name;
            result.str0 = ButtonsPressedSoFar;
            result.int0 = FailedCount;
            result.int1 = ClicksSoFar;
            return result;
        }

        public override void LoadSaveState(SpecialPuzzleSaveState state)
        {
            if (state.puzzlename == Name)
            {
                ButtonsPressedSoFar = state.str0;
                FailedCount = state.int0;
                ClicksSoFar = state.int1;
            }
        }
    }
}
