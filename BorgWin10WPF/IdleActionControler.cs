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

        private List<SpecialPuzzleBase> _specialPuzzles = new List<SpecialPuzzleBase>();

        public IdleActionControler(List<SpecialPuzzleBase> specialpuzzles)
        {
            _specialPuzzles = specialpuzzles;
        }

        public IdleActionResult IdleActionScene(SceneDefinition currentScene, bool checkOnly)
        {
            if (!checkOnly && currentScene.SceneType != SceneType.Bad)
                ++idleactioncount;

            string SdResult = string.Empty;
            int jumptoframe = 0;
            bool UsePuzzleControllerYN = false;
            SpecialPuzzleBase puzzleController = null;

            bool puzzleOverrideNeeded = false;

            foreach (var pzzu in _specialPuzzles)
            {
                if (currentScene.Name.ToLowerInvariant() == pzzu.PuzzleInputActiveScene.ToLowerInvariant())
                {
                    UsePuzzleControllerYN = true;
                    puzzleController = pzzu;
                    break;
                }
            }

            if (UsePuzzleControllerYN)
            {
                var puzzleResult = puzzleController.Click("Idle", checkOnly);
                SdResult = puzzleResult.JumpToScene;
                puzzleOverrideNeeded = puzzleResult.OverrideNeeded;
            }

            if (!puzzleOverrideNeeded)
            {
                switch (currentScene.SceneType)
                {
                    case SceneType.Main:
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
                                    SdResult = "Dp4IdleA";
                                else
                                    SdResult = "Dp4IdleB";

                                break;
                            case "v_05": // a b /c/ d
                                SdResult = "Dp5Idle";
                                break;
                            case "v_06":
                                SdResult = "PuzzleControler";
                                break;
                            case "v_07": // Borg Implant, do nothing.
                                SdResult = "";
                                break;
                            case "v_88":
                            case "v_08":
                                SdResult = "D8Idle";
                                break;
                            case "v_09":
                                SdResult = "D8Idle";
                                break;
                            case "v_11":
                                SdResult = "D11Idle";
                                break;
                            case "v_14":
                            case "v14a":
                                SdResult = "D13Idle";
                                break;
                            //case "v319":
                            //    SdResult = "v_21";
                            //    break;
                            case "v_16":
                            case "v_17":
                                SdResult = "D17Idle";
                                break;
                            case "v_18":
                                SdResult = "D18Idle";
                                break;
                            case "v319":
                                SdResult = "V_21";
                                break;
                            case "v_19c":
                                SdResult = "V319";
                                break;
                            case "v_21":
                                SdResult = "D19BC";
                                break;
                            case "v_23":
                                SdResult = "D23Idle";
                                break;
                            case "v_24":
                                SdResult = "D24Idle";
                                break;
                            case "v_26":
                                SdResult = "V_28";
                                break;
                            case "v_27":
                                SdResult = "D27BS";
                                break;
                            case "v_28":
                                SdResult = "D30FL";
                                break;
                            case "v_29":
                                SdResult = "V_25";
                                break;
                            case "v_30":
                                SdResult = "D30Idle";
                                break;
                            case "v_31":
                                SdResult = "V_32";
                                break;
                            case "v_32":
                                SdResult = "D32K9";
                                break;
                            case "v_31b":
                                SdResult = "DQUIT";
                                break;
                        }
                        break;
                    case SceneType.Bad:
                        switch (currentScene.Name.ToLowerInvariant())
                        {
                            case "d8idle":
                                SdResult = "V_11";
                                break;

                            case "d8rhn":
                            case "d8lhn":
                                SdResult = "V_10";
                                break;
                            case "d8rh":
                            case "d8lh":
                            case "d11sp":
                            case "d12cc":
                            case "d12bb":
                            case "d12es":
                            case "d11idle":
                            case "d12idle":
                                SdResult = "V_88";
                                break;
                            case "d13idle":
                                SdResult = "V_14";
                                break;
                            case "d10sf":
                                SdResult = "V_11";
                                break;
                            case "v_19c":
                                SdResult = "V319";
                                break;
                            case "d19bq":
                            case "d19bc":
                                SdResult = "V_19c";
                                break;
                            case "d20b1":
                                SdResult = "V_21";
                                break;
                            case "d20b2":
                            case "d20b3":
                                SdResult = "V319";
                                break;                                
                            case "d20b4":
                                SdResult = "V_20";
                                break;
                            case "d22bd":
                                SdResult = "V_24";
                                break;
                            case "d23idle":
                                SdResult = "V_24";
                                break;
                            case "d24fl":
                                SdResult = "D24BI";
                                jumptoframe = 11670;
                                break;
                            case "d22cr":
                            case "d24bi":
                                SdResult = "V_22";
                                jumptoframe = 8348;
                                break;
                            case "d26qn":
                                SdResult = "V_27";
                                break;
                            case "d27bd":
                                SdResult = "V_25";
                                break;
                            case "d30qm":
                                SdResult = "V_29";
                                break;
                            case "d30fl":
                                SdResult = "V_28";
                                break;
                            case "d30qf":
                                SdResult = "V_29";
                                break;
                            case "d32k1":
                                SdResult = "V_25";
                                break;
                         
                            case "d32k5":
                            case "d32k6":
                            case "d32k7":
                                SdResult = "v_31";
                                break;
                            case "d32k9":
                                SdResult = "v_31";
                                jumptoframe = 30535;
                                break;

                                //case "d12cc": d12cc is a shoot you scene, not a borg you scene.
                                //    SdResult = "V_11";
                                //    break;
                        }
                        break;

                    case SceneType.Inaction:
                        break;
                }

            }


            return new IdleActionResult()
            {
                IdleScene = SdResult,
                IdleBad = !string.IsNullOrEmpty(SdResult) && SdResult != "Keep Playing",
                KeepPlaying = SdResult == "Keep Playing" ? true : false,
                JumpToFrame = jumptoframe
            };
            
        }
        public class IdleActionResult
        {
            public bool IdleBad { get; set; } = false;
            public string IdleScene { get; set; } = string.Empty;
            public bool EndGame { get; set; } = false;
            public bool KeepPlaying { get; set; } = false;
            public int NewEndFrame { get; set; } = 0;
            public int JumpToFrame { get; set; } = 0;

        }
    }
}
