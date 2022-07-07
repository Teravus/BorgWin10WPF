using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        internal static string CheckForOriginalMedia()
        {
            string result = string.Empty;
            List<string> FileLocations = new List<string>()
            {
                string.Format("MAIN_{0}X.AVI", 1),
                string.Format("MAIN_{0}X.AVI", 2),
                string.Format("SS_{0}X.AVI", 1),
                string.Format("SS_{0}X.AVI", 2),
                string.Format("COMPUTER.AVI"),
                string.Format("HOLODECK.AVI"),
                string.Format("IP_1.AVI")
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
