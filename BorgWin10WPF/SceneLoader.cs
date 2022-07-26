using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace BorgWin10WPF
{
    public static class SceneLoader
    {
        public static List<SceneDefinition> LoadScenesFromAsset(string FileName)
        {
            List<SceneDefinition> defs = new List<SceneDefinition>();
            string[] lines = File.ReadAllLines(Path.Combine(System.IO.Directory.GetCurrentDirectory(), "CDAssets", FileName));
            if (lines.Length > 0)
            {
                for (var i = 0; i < lines.Length; i++)
                {
                    string[] linevals = lines[i].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (linevals.Length > 0)
                    {
                        if (linevals[0].StartsWith("DN"))
                        {
                            switch (linevals.Length)
                            {
                                case 5:
                                    defs.Add(new SceneDefinition(SceneType.Inaction, linevals[0], Convert.ToInt32(linevals[2]), Convert.ToInt32(linevals[3]), Convert.ToInt32(linevals[1])));
                                    break;
                                case 4: // Older version of DM Line.  No Offset.
                                    defs.Add(new SceneDefinition(SceneType.Inaction, linevals[0], Convert.ToInt32(linevals[1]), Convert.ToInt32(linevals[2]), 0));
                                    break;
                                default:
                                    // I dunno
                                    break;
                            }
                        }
                        else
                        {
                            switch (linevals.Length)
                            {
                                //Name CD Start End Success ? Retry
                                //V_01 0 1 4064 3972 0 4421
                                //case 8:
                                //    defs.Add(new SceneDefinition(SceneType.Main, linevals[0], Convert.ToInt32(linevals[2]), Convert.ToInt32(linevals[3]), Convert.ToInt32(linevals[1]), Convert.ToInt32(linevals[4]), Convert.ToInt32(linevals[6])));
                                //    break;
                                case 7: // Older version that has CDs of above.  No offset.
                                    defs.Add(new SceneDefinition(SceneType.Main, linevals[0], Convert.ToInt32(linevals[1]), Convert.ToInt32(linevals[2]), Convert.ToInt32(linevals[4]),0, Convert.ToInt32(linevals[3]), Convert.ToInt32(linevals[6])));
                                    break;
                                //case 6:
                                //    defs.Add(new SceneDefinition(SceneType.Bad, linevals[0], Convert.ToInt32(linevals[2]), Convert.ToInt32(linevals[3]), Convert.ToInt32(linevals[1]), 0, Convert.ToInt32(linevals[4])));
                                //    break;
                                case 5:// Older version of above that has Cds.  No offset.
                                    if (linevals[0].StartsWith("I_"))
                                    {
                                        defs.Add(new SceneDefinition(SceneType.Info, linevals[0], Convert.ToInt32(linevals[1]), Convert.ToInt32(linevals[2]), 0));
                                    }
                                    else
                                        defs.Add(new SceneDefinition(SceneType.Bad, linevals[0], Convert.ToInt32(linevals[1]), Convert.ToInt32(linevals[2]), Convert.ToInt32(linevals[3]), 0, 0, Convert.ToInt32(linevals[4])));
                                    break;
                                default:
                                    // I dunno
                                    break;
                            }
                        }
                    }
                }
            }
            // Looking for hierarchical main track, alternate track options
            for (int i = 0; i < defs.Count; i++)
            {
                for (int j = 0; j < defs.Count; j++)
                {
                    if (defs[j].Name.Contains(defs[i].Name) && defs[j].SceneType == SceneType.Bad && defs[i].SceneType == SceneType.Main)
                    {
                        defs[i].ParentScene = defs[j];
                    }
                }
            }
            // Fixup CDs
            foreach (var scene in defs)
            {
                scene.CD = GetCDBySceneName(scene.Name);
            }
            return defs;
        }

        public static List<SceneDefinition> LoadSupportingScenesFromAsset(string FileName)
        {
            List<SceneDefinition> defs = new List<SceneDefinition>();
            string[] lines = File.ReadAllLines(Path.Combine(System.IO.Directory.GetCurrentDirectory(), "Assets", FileName));
            if (lines.Length > 0)
            {
                for (var i = 0; i < lines.Length; i++)
                {
                    string[] linevals = lines[i].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (linevals.Length > 0)
                    {
                        defs.Add(new SceneDefinition() { SceneType = SceneType.Info, Name = linevals[0], OffsetTimeMS = Convert.ToInt32(linevals[1]), FrameStart = Convert.ToInt32(linevals[2]), FrameEnd = Convert.ToInt32(linevals[3]) });
                    }
                }
            }

            return defs;
        }

        public static int GetCDBySceneName(string sceneName)
        {
            int result;
            // It isn't strictly required to name every Main/Alternate scene here, but I did it anyway.
            switch (sceneName)
            {
                case "V_01":
                case "V_02":
                case "V_03":
                case "V_04":
                case "V_05":
                case "V_06":
                case "V_07":
                case "V_08":
                    result = 1;
                    break;
                case "V_88":
                case "V_09":
                case "V_10":
                case "V_11":
                case "V_12":
                case "V_13":
                case "V_14":
                case "V14A":
                case "V_15":
                case "V_16":
                case "V_17":
                case "V_18":
                case "V_19":
                    result = 2;
                    break;
                case "V319":
                case "V_20":
                case "V_21":
                case "V_22":
                case "V_23":
                case "V_24":
                case "V_25":
                case "V_26":
                case "V_27":
                case "V_28":
                case "V_29":
                case "V_30":
                case "V_31":
                case "V_32":
                    result = 3;
                    break;
                default:
                    result = 1;
                    break;

            }
            return result;
        }

    }
}
