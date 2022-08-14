﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BorgWin10WPF
{
    public class Borg21ComputerPuzzle : SpecialPuzzleBase
    {
        public string ButtonsPressedSoFar { get; set; } = String.Empty;
        public string BorgComputerButtonSequence { get; set; } = "133422";
        public int FailedCount { get; set; } = 0;
        public int ClicksSoFar { get; set; } = 0;

        public override string PuzzleInputActiveScene { get; } = "V_21";

        public override string PuzzleTriggerActiveScene { get; } = "V_21";

        public override SpecialPuzzleResult Click(string ButtonName)
        {

            SpecialPuzzleResult result = new SpecialPuzzleResult();

            string buttonnumber = "";

            switch (ButtonName)
            {

                case "D20B1":
                    buttonnumber = "1";
                    break;
                case "D20B2":
                    buttonnumber = "2";
                    break;
                case "D20B3":
                    buttonnumber = "3";
                    break;
                case "D20B4":
                    buttonnumber = "4";
                    break;


            }

            ++ClicksSoFar;
            ButtonsPressedSoFar += buttonnumber;
            if (!BorgComputerButtonSequence.StartsWith(ButtonsPressedSoFar) || ButtonName == "Idle")
            {
                ++FailedCount;
                //wrong button pressed.
                switch (FailedCount)
                {
                    case 0:
                        break;
                    case 1:
                        result.JumpToScene = "D20B1";
                        result.SuccessYN = false;
                        result.OverrideNeeded = true;
                        Retry();
                        break;
                    case 2:
                        result.JumpToScene = "D20B2";
                        result.SuccessYN = false;
                        result.OverrideNeeded = true;
                        Retry();
                        break;
                    case 3:
                        result.JumpToScene = "D20B3";
                        result.SuccessYN = false;
                        result.OverrideNeeded = true;
                        Reset();
                        break;

                }

            }
            else
            {

                // We're good. 
                if (ButtonsPressedSoFar == BorgComputerButtonSequence)
                {
                    // Pressed the right button.
                    result.JumpToScene = "V_22";
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


    public class Borg20ComputerPuzzle : SpecialPuzzleBase
    {
        public string ButtonsPressedSoFar { get; set; } = String.Empty;
        public string BorgComputerButtonSequence { get; set; } = "133422";
        public int FailedCount { get; set; } = 0;
        public int ClicksSoFar { get; set; } = 0;

        public override string PuzzleInputActiveScene { get; } = "V_20";
        public override string PuzzleTriggerActiveScene { get; } = "V_20";

        public override SpecialPuzzleResult Click(string ButtonName)
        {

            SpecialPuzzleResult result = new SpecialPuzzleResult();

            string buttonnumber = "";

            switch (ButtonName)
            {

                case "D20B1":
                    buttonnumber = "1";
                    break;
                case "D20B2":
                    buttonnumber = "2";
                    break;
                case "D20B3":
                    buttonnumber = "3";
                    break;
                case "D20B4":
                    buttonnumber = "4";
                    break;


            }

            ++ClicksSoFar;
            ButtonsPressedSoFar += buttonnumber;
            if (!BorgComputerButtonSequence.StartsWith(ButtonsPressedSoFar) || ButtonName == "Idle")
            {
                ++FailedCount;
                //wrong button pressed.
                switch (FailedCount)
                {
                    case 0:
                        break;
                    case 1:
                        result.JumpToScene = "D20B1";
                        result.SuccessYN = false;
                        result.OverrideNeeded = true;
                        Retry();
                        break;
                    case 2:
                        result.JumpToScene = "D20B2";
                        result.SuccessYN = false;
                        result.OverrideNeeded = true;
                        Retry();
                        break;
                    case 3:
                        result.JumpToScene = "D20B3";
                        result.SuccessYN = false;
                        result.OverrideNeeded = true;
                        Retry();
                        break;
                    case 4:
                        result.JumpToScene = "D20B4";
                        result.SuccessYN = false;
                        result.OverrideNeeded = true;
                        Reset();
                        break;

                }

            }
            else
            {

                // We're good. 
                if (ButtonsPressedSoFar == BorgComputerButtonSequence)
                {
                    // Pressed the right button.
                    result.JumpToScene = "V_22";
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

