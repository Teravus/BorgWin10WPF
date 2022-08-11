using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BorgWin10WPF
{
    public class IdleActionControler
    {
        private int idleactioncount = 0;
        public IdleActionResult IdleActionScene(SceneDefinition currentScene)
        {
            ++idleactioncount;
            string SdResult = string.Empty;
            switch (currentScene.Name.ToLowerInvariant())
            {
                case "v_01": // Phaser/Tricorder
                    SdResult = "D1Idle";
                    break;
                case "v_02": // Phaser/Tricorder
                    SdResult = "D2Idle";
                    break;
                case "v_03": // Console/Borg/Targus
                    SdResult = "D3Idle";
                    break;
                case "v_04": // A B C/d/
                    
                    if (idleactioncount % 2 == 0)
                        SdResult = "Dp4IdleB";
                    else
                        SdResult = "Dp4IdleA";

                    break;
                case "v_05": // a b /c/ d
                    SdResult = "Dp5Idle";
                    break;
                case "v_06":
                    SdResult = "PuzzleControler";
                    break;
                case "v_07": // Borg Implant
                    SdResult = "";
                    break;
                case "v_88":
                case "v_08":
                    SdResult = "D8Idle";
                    break;
                case "v_09":
                    SdResult = "D8Idle";
                    break;
                

            }

            return new IdleActionResult() { IdleScene = SdResult, IdleBad = !string.IsNullOrEmpty(SdResult) };
            
        }
        public class IdleActionResult
        {
            public bool IdleBad { get; set; } = false;
            public string IdleScene { get; set; } = string.Empty;
            public bool EndGame { get; set; } = false;

            

        }
    }
}
