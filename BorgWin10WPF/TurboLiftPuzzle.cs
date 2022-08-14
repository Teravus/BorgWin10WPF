using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BorgWin10WPF
{
    public class TurboLiftPuzzle : SpecialPuzzleBase
    {
        public string ButtonsPressedSoFar { get; set; } = String.Empty;
        public string ComputerCoreCode { get; set; } = "150619";
        public string CrewQuartersCode  { get; set; } = "050302";
        public string BridgeCode { get; set; } = "010001";
        public string EngineeringCode { get; set; } = "120826";
        public int FailedCount { get; set; } = 0;
        public int ClicksSoFar { get; set; } = 0;

        public override string PuzzleInputActiveScene { get; } = "V_06";

        public override string PuzzleTriggerActiveScene { get; } = "V_06";

        public override SpecialPuzzleResult Click(string ButtonName)
        {

            SpecialPuzzleResult result = new SpecialPuzzleResult();

            string buttonnumber = "";
            
            switch (ButtonName)
            {
                case "Dp60":
                    buttonnumber = "0";
                    break;
                case "Dp61":
                    buttonnumber = "1";
                    break;
                case "Dp62":
                    buttonnumber = "2";
                    break;
                case "Dp63":
                    buttonnumber = "3";
                    break;
                case "Dp64":
                    buttonnumber = "4";
                    break;
                case "Dp65":
                    buttonnumber = "5";
                    break;
                case "Dp66":
                    buttonnumber = "6";
                    break;
                case "Dp67":
                    buttonnumber = "7";
                    break;
                case "Dp68":
                    buttonnumber = "8";
                    break;
                case "Dp69":
                    buttonnumber = "9";
                    break;

            }
          
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
                        result.JumpToScene = "DP6Idle";
                        result.SuccessYN = false;
                        result.OverrideNeeded = true;
                        Retry();
                        break;
                    case 2:
                        result.JumpToScene = "Dp61";
                        result.SuccessYN = false;
                        result.OverrideNeeded = true;
                        Retry();
                        break;
                    case 3:
                        result.JumpToScene = "Dp60";
                        result.SuccessYN = false;
                        result.OverrideNeeded = true;
                        Retry();
                        break;
                    case 4:
                        result.JumpToScene = "V_07";
                        result.SuccessYN = true;
                        result.OverrideNeeded = true;
                        Reset();
                        break;
                }

            }
            else
            {
                
                // We're good. 
                if (ButtonsPressedSoFar== ComputerCoreCode)
                {
                    // Pressed the right button.
                    result.JumpToScene = "V_07";
                    result.SuccessYN = true;
                    result.OverrideNeeded = true;
                    Reset();
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
