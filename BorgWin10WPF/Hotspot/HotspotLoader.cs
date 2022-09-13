using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BorgWin10WPF.Hotspot
{
    public static class HotspotLoader
    {
        public static List<HotspotDefinition> LoadHotspotsFromAsset(string FileName)
        {
            List<HotspotDefinition> defs = new List<HotspotDefinition>();
            string[] lines = File.ReadAllLines(Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "Assets", FileName));
            if (lines.Length > 0)
            {
                for (var i = 0; i < lines.Length; i++)
                {
                    string[] linevals = lines[i].Split(new char[] { ',' }, StringSplitOptions.None);
                    if (linevals.Length > 1)
                    {
                        int linevalcounter = 0;
                        HotspotDefinition def = new HotspotDefinition();
                        def.Group = linevals[linevalcounter++];
                        def.HotSpotType = linevals[linevalcounter++];
                        def.Name = linevals[linevalcounter++];
                        def.RelativeVideoName = linevals[linevalcounter++];
                        def.ActionVideo = linevals[linevalcounter++];
                        def.FrameStart = Convert.ToInt32(linevals[linevalcounter++]) - 3;
                        def.FrameEnd = Convert.ToInt32(linevals[linevalcounter++]) - 1;
                        def.Area = new List<Box2d>() {
                            new Box2d() {
                                TopLeft = new System.Drawing.Point() {
                                    X = Convert.ToInt32(linevals[linevalcounter++]),
                                    Y = Convert.ToInt32(linevals[linevalcounter++])
                                }, BottomRight = new System.Drawing.Point() {
                                    X = Convert.ToInt32(linevals[linevalcounter++]),
                                    Y = Convert.ToInt32(linevals[linevalcounter++])
                                }, ActionId = Convert.ToInt32(linevals[linevalcounter++])

                            }

                        };
                        // There's an int here for something. Likely which action it is..  but it is hard to tell.

                        if (def.HType == HotspotType.Interpolate || def.HType == HotspotType.Multi)
                        {
                            int unknownint = 0;


                            var box = new Box2d()
                            {
                                TopLeft = new System.Drawing.Point()
                                {
                                    X = Convert.ToInt32(linevals[linevalcounter++]),
                                    Y = Convert.ToInt32(linevals[linevalcounter++])
                                },
                                BottomRight = new System.Drawing.Point()
                                {
                                    X = Convert.ToInt32(linevals[linevalcounter++]),
                                    Y = Convert.ToInt32(linevals[linevalcounter++])
                                },
                                ActionId = unknownint

                            };
                            if (linevals.Length > 16)
                                box.ActionId = Convert.ToInt32(linevals[linevalcounter++]);

                            def.Area.Add(box);
                        }
                        if (def.HType == HotspotType.Multi)
                        {

                            // This is a special case.  The multi-button has an action video and the button sequence is missing its first button action.
                            // I feel like this is a hack.  I suspect there's something that I didn't figure out but it only comes up once in the detonator puzzle.
                            // One example may not be enough to reverse engineer it.


                            //h,m,green button,V018_1,V018A,3701,3775,20,65,65,94,2,20,65,65,94,1,4/4/4/3/-2/V019,2/-2/V018A,3/-2/V018A,4/0/-2/V018A,0/-2/V018A
                            //h,m,1 raise shields, V016,,4442,4446,47,98,113,123,0,47,98,113,123,1,0/-2/v016A,2/-2/v016A,3/-2/,4/-2/V016D
                            //h, multi, name, Applies to, ActionVideo, Frame start, frame end, Top Left point, bottom right point, Interpolation top left point, bottom right point,
                            // button ID, Click Sequence/Consequence.


                            int specialcaseButtonSequenceFixCounter = 0;
                            for (int stridei = linevalcounter; stridei < linevals.Length; stridei++)
                            {
                                ++specialcaseButtonSequenceFixCounter;

                                string HotspotActionsStr = linevals[linevalcounter++];
                                string[] HotspotActions = HotspotActionsStr.Split('/');
                                List<int> ClickSequence = new List<int>();


                                for (int hotspoti = 0; hotspoti < HotspotActions.Length; hotspoti++)
                                {

                                    if (Convert.ToInt32(HotspotActions[hotspoti]) == -2)
                                        break;
                                    ClickSequence.Add(Convert.ToInt32(HotspotActions[hotspoti]));
                                }
                                def.multiAction.Add(new MultiAction()
                                {
                                    ClickIndex = Convert.ToInt32(HotspotActions[0]),
                                    NextButtonSequence = ClickSequence,
                                    ResultVideo = HotspotActions.Last().ToUpperInvariant()
                                });

                                // special case.  The multi-button has an action video and the button sequence is missing its first button action.  We have to add the first button to the sequence for the engine to understand it.
                                if (!string.IsNullOrEmpty(def.ActionVideo) && specialcaseButtonSequenceFixCounter == 1)
                                {
                                    def.multiAction.Add(new MultiAction()
                                    {
                                        ClickIndex = def.Area.Last().ActionId,
                                        NextButtonSequence = ClickSequence,
                                        ResultVideo = HotspotActions.Last().ToUpperInvariant()
                                    });
                                }

                            }
                        }
                        defs.Add(def);
                    }
                }
            }
            return defs;
        }
    }
}

