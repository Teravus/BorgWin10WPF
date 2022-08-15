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
                            case "v319":
                                SdResult = "v_21";
                                break;
                            case "v_26":
                                SdResult = "Keep Playing";

                                break;
                            case "v_27":
                                SdResult = "D27BS";
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
                                SdResult = "V_88";
                                break;

                            case "d10sf":
                                SdResult = "V_11";
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
                KeepPlaying = SdResult == "Keep Playing" ? true : false
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
