using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BorgWin10WPF
{
    public static class ButtonClickPreProcessor
    {
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

            }


            return triggeredaction;
        }

    }
}
