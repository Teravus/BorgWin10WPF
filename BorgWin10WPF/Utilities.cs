using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BorgWin10WPF.Scene;

namespace BorgWin10WPF
{
    public static class Utilities
    {

        public static long Frames15fpsToMS(int frames)
        {
            long result = (long)(frames * 66.666666666666666666666666666667f);
            return result;
        }
        public static long MsTo15fpsFrames(long ms)
        {
            int frames = (int)((float)ms / 66.666666666666666666666666666667f);
            return frames;
        }

        // Find next forward direction scene based on SceneMS
        public static SceneDefinition FindNextMainScene(List<SceneDefinition> options, SceneDefinition existingScene)
        {
            SceneDefinition nextScene = null; // At the end of the game, we won't find one and we want to return null
                                              // so the game knows to end

            // Find our current position in the options array.
            int currentMainSceneArrayPosition = -1; // We didn't find one if -1

            for (int i = 0; i < options.Count; i++)
            {
                if (options[i].SceneType != SceneType.Main)
                    continue;

                if (existingScene.Name == options[i].Name)
                {

                    currentMainSceneArrayPosition = i;
                    break;
                }
            }


            if (currentMainSceneArrayPosition > -1) // Make sure to account for V000
            {
                // search for the next main video in the array
                // This will most likely be the next position in the array, but I'm using
                // a less naive search so people can edit the scene file

                int nextScenePosition = currentMainSceneArrayPosition + 1;

                for (int i = nextScenePosition; i < options.Count; i++)
                {
                    if (options[i].SceneType != SceneType.Main)
                        continue;
                    nextScene = options[i];
                    break;
                }

            }

            return nextScene;
        }
        public static AspectRatioMaxResult GetMax(double width, double height, double AspectDecimal)
        {
            var heightbywidth = width / AspectDecimal;
            var widthbyheight = height * AspectDecimal;
            string direction = string.Empty;
            int length = 0;

            if (widthbyheight < width)
            {
                direction = "W";
                length = (int)widthbyheight;
            }
            if (heightbywidth < height)
            {
                direction = "H";
                length = (int)heightbywidth;
            }
            System.Diagnostics.Debug.WriteLine(String.Format("\tAspect:({0},{1})-{2},{3}-{4}", width, height, heightbywidth, widthbyheight, direction));
            return new AspectRatioMaxResult() { Direction = direction, Length = length };
            // we know if height is a certain thing and it isn't in ratio
        }
        public static bool ValidateSaveGameName(string SaveName)
        {
            bool GoodYN = true;

            if (SaveName.Contains(","))
                GoodYN = false;

            if (SaveName.Contains("\n"))
                GoodYN = false;

            if (SaveName.Contains("\r"))
                GoodYN = false;

            if (SaveName.Contains("<"))
                GoodYN = false;
            if (SaveName.Contains(">"))
                GoodYN = false;

            if (SaveName.Contains(";"))
                GoodYN = false;


            return GoodYN;
        }
        internal static string CheckForOriginalMedia()
        {
            string result = string.Empty;
            List<string> FileLocations = new List<string>()
            {
                string.Format("MAIN_{0}X.AVI", 1),
                string.Format("MAIN_{0}X.AVI", 2),
                string.Format("MAIN_{0}X.AVI", 3),
                string.Format("LOGOX.AVI"),
                string.Format("IPX.AVI")
            };
            for (int i = 0; i < FileLocations.Count; i++)
            {
                if (!File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "CDAssets", FileLocations[i])))
                {
                    //if (result.Length == 0)
                    //    result += "\r\n";

                    result += "\t* " + FileLocations[i] + "\r\n";
                }
            }

            if (result.Length > 0)
            {
                result = string.Format("The original game media wasn't found.\r\nWe are missing:\r\n{0}\r\nPlease check the github readme for how to prepare the original game media. https://github.com/Teravus/KlingonWin10WPF", result);
            }
            return result;
        }

    }
}
