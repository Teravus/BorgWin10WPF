using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BorgWin10WPF
{
    public class DisableBorgPhonePadPuzzle : SpecialPuzzleBase
    {
        public string ButtonsPressedSoFar { get; set; } = String.Empty;
        public string ComputerCoreCode { get; set; } = "61330";
       
        public int FailedCount { get; set; } = 0;
        public int ClicksSoFar { get; set; } = 0;

        public override string PuzzleInputActiveScene { get; } = "V_32";

        public override string PuzzleTriggerActiveScene { get; } = "V_32";

        public override bool PuzzleTriggersOnScene(string SceneName)
        {
            if (SceneName.ToLowerInvariant() == PuzzleTriggerActiveScene.ToLowerInvariant())
                return true;
            return false;
        }

        public override SpecialPuzzleResult Click(string ButtonName, bool checkonly)
        {

            SpecialPuzzleResult result = new SpecialPuzzleResult();

            string buttonnumber = "";

            switch (ButtonName)
            {
                case "D32K0":
                    buttonnumber = "0";
                    break;
                case "D32K1":
                    buttonnumber = "1";
                    break;
                case "D32K2":
                    buttonnumber = "2";
                    break;
                case "D32K3":
                    buttonnumber = "3";
                    break;
                case "D32K4":
                    buttonnumber = "4";
                    break;
                case "D32K5":
                    buttonnumber = "5";
                    break;
                case "D32K6":
                    buttonnumber = "6";
                    break;
                case "D32K7":
                    buttonnumber = "7";
                    break;
                case "D32K8":
                    buttonnumber = "8";
                    break;
                case "D32K9":
                    buttonnumber = "9";
                    break;

            }
            if (!checkonly)
            {
                ++ClicksSoFar;
                ButtonsPressedSoFar += buttonnumber;
                if (!ComputerCoreCode.StartsWith(ButtonsPressedSoFar) || ButtonName == "Idle")
                {
                    ++FailedCount;
                    //wrong button pressed.
                    switch (FailedCount)
                    {
                        case 0:
                            break;
                        case 1:
                            result.JumpToScene = "D32K9";
                            result.SuccessYN = false;
                            result.OverrideNeeded = true;
                            Retry();
                            break;
                      
                    }

                }
                else
                {

                    // We're good. 
                    if (ButtonsPressedSoFar == ComputerCoreCode)
                    {
                        // Pressed the right button.
                        result.JumpToScene = "V_31b";
                        result.SuccessYN = true;
                        result.OverrideNeeded = true;
                        Reset();
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
