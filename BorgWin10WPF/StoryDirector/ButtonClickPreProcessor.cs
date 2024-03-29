﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BorgWin10WPF.StoryDirector
{
    public static class ButtonClickPreProcessor
    {
        private static int Chapter_V_18_CircuitClicks {get;set;} = 0;

        private static int Chapter_V_17_ComputerCoreClicks { get; set; } = 0;

        private static bool _visitedBorgifiedScenev_12 { get; set; } = false;

        private static int Chapter_V_16_ComputerCoreClicks { get; set; } = 0;
        public static Tuple<int,int,bool,int> GetState()
        {
            return new Tuple<int, int, bool, int>(Chapter_V_18_CircuitClicks, Chapter_V_17_ComputerCoreClicks, _visitedBorgifiedScenev_12, Chapter_V_16_ComputerCoreClicks);
        }
        public static void LoadState(Tuple<int, int, bool, int> state)
        {
            Chapter_V_18_CircuitClicks = state.Item1;
            Chapter_V_17_ComputerCoreClicks = state.Item2;
            _visitedBorgifiedScenev_12 = state.Item3;
            Chapter_V_16_ComputerCoreClicks = state.Item4;
        }
        public static bool TryButtonPressTransformAction(string OriginalButton, out string NextScene)
        {
            NextScene = OriginalButton;
            bool triggeredaction = false;
            switch (OriginalButton)
            {
                case "D1Phs":
                    NextScene = "V_02";
                    triggeredaction = true;
                    break;
                case "D2Phs":
                    NextScene = "V_03";
                    triggeredaction = true;
                    break;
                case "D3Con":
                    NextScene = "V_04";
                    triggeredaction = true;
                    break;
                case "Dp4D":
                    NextScene = "V_05";
                    triggeredaction = true;
                    break;
                case "Dp5C":
                    NextScene = "V_06";
                    triggeredaction = true;
                    break;
                case "D8Chi":
                    NextScene = "V_13";
                    triggeredaction = true;
                    break;
                case "D8RHn":
                    if (_visitedBorgifiedScenev_12)
                    {
                        NextScene = "D8RH";
                        triggeredaction = true;
                    }
                    _visitedBorgifiedScenev_12 = true;
                    break;
                case "D8LHn":
                    if (_visitedBorgifiedScenev_12)
                    {
                        NextScene = "D8LH";
                        triggeredaction = true;
                    }
                    _visitedBorgifiedScenev_12 = true;
                    break;
                //case "D9RN":

                //    break;
                //case "D9LN":
                
                //    break;
                case "D9Chi":
                    NextScene = "V_13";
                    triggeredaction = true;
                    break;
                case "D11AP":
                    NextScene = "V_12";
                    triggeredaction = true;
                    break;
                case "D14Ph":
                    NextScene = "V14A";
                    triggeredaction = true;
                    break;
                case "D14AP":
                    NextScene = "V_15";
                    triggeredaction = true;
                    break;
                case "D16TI":
                    NextScene = "V_18";
                    triggeredaction = true;
                    break;
                case "D16CS":
                    ++Chapter_V_17_ComputerCoreClicks;
                    if (Chapter_V_17_ComputerCoreClicks >= 2)
                    {
                        NextScene = "D16CSb";
                        triggeredaction = true;
                    }
                    break;
                case "D17CS":
                    ++Chapter_V_17_ComputerCoreClicks;
                    if (Chapter_V_17_ComputerCoreClicks >= 2)
                    {
                        NextScene = "D16CSb";
                        triggeredaction = true;
                    }
                    break;
                case "D18TI":
                    // Click Targus' implant first and Q tries to trick you.
                    ++Chapter_V_18_CircuitClicks;
                    if (Chapter_V_18_CircuitClicks >= 2)
                    {
                        NextScene = "V_19";
                        triggeredaction = true;
                    }
                    else 
                        triggeredaction = false;
                    break;
                case "D23TA":
                    NextScene = "V23b";
                    triggeredaction = true;
                    break;
                case "D31KS":
                    NextScene = "V_32";
                    triggeredaction = true;
                    break;
                //case "D25B1":
                //case "D25B2":
                //case "D25B3":
                //    NextScene = "V_26";
                //    triggeredaction = true;
                //    break;
                //case "D19BC":
                //    NextScene = "V_20";
                //    triggeredaction = true;
                //    break;
                //case "D19BQ":
                //    NextScene = "V_20";
                //    triggeredaction = true;
                //    break;

            }


            return triggeredaction;
        }

    }
}
