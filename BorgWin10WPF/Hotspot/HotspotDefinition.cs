using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using BorgWin10WPF.Scene;

namespace BorgWin10WPF.Hotspot
{
    public enum HotspotType
    {
        Diagnal,
        Interpolate,
        Multi,
        Unknown

    }
    public class HotspotDefinition
    {
        private static float _Scale = 1.0f;
        public string Group { get; set; } = "h";// Don't know what this is.  But it seems to be the first element always.  Maybe represents 'Hit' FailedSleep suggested this might mean hitbox.
        public string HotSpotType { get; set; } = "i"; // Looks like this is a type identifier.   Guessing..   i=interpolate, d=Static.  m=multi (have to hit two or more)
        public string Name { get; set; }
        public string RelativeVideoName { get; set; } = "V001"; // Contains the video that this hotspot is in reference to
        public string ActionVideo { get; set; } = "V001A"; //triggers/affects video.  FOr example V001A is an Alternate/Bad branch.  IP087 is a holodeck info audio.  If this is blank then go-on.
        public int FrameStart { get; set; } = 0;
        public int FrameEnd { get; set; } = 0;
        public List<Box2d> Area { get; set; }
        public int SourceLine { get; set; }
        
        public List<MultiAction> multiAction { get; set; } = new List<MultiAction>();
        public HotspotType HType
        {
            get
            {
                switch (HotSpotType)
                {
                    case "i":
                        return HotspotType.Interpolate;
                    case "m":
                        return HotspotType.Multi;
                    case "d":
                        return HotspotType.Diagnal;
                }
                return HotspotType.Unknown;

            }
            set
            {
                switch (value)
                {
                    case HotspotType.Diagnal:
                        HotSpotType = "d";
                        break;
                    case HotspotType.Multi:
                        HotSpotType = "m";
                        break;
                    case HotspotType.Interpolate:
                        HotSpotType = "i";
                        break;
                    default:
                        HotSpotType = "u";
                        break;
                }
            }

        }
        public float HotspotScale
        {
            get { return _Scale; }
            set { _Scale = value; }
        }
        public bool HitTest(int X, int Y, long Milliseconds, SceneDefinition scene, bool OffsetSceneYN = true)
        {
            if (Area == null)
                return false;

            if (Area.Count < 1)
                return false;

            if (OffsetSceneYN && scene.Name.ToLowerInvariant() != RelativeVideoName.ToLowerInvariant())
                return false;

            long startMS = Utilities.Frames15fpsToMS(FrameStart);
            if (OffsetSceneYN)
                startMS += scene.StartMS;

            long endMS = Utilities.Frames15fpsToMS(FrameEnd);
            if (OffsetSceneYN)
                endMS += scene.StartMS;

            bool InFrame = Milliseconds >= startMS && Milliseconds <= endMS;
            float clickscale = 1f;
            if (scene.Name.ToLowerInvariant().StartsWith("i_"))
            {
                InFrame = (Milliseconds + 2000 >= Utilities.Frames15fpsToMS(FrameStart) && Milliseconds - 200 <= Utilities.Frames15fpsToMS(FrameEnd));
            }
            switch (HType)
            {
                case HotspotType.Diagnal:
                case HotspotType.Interpolate:
                case HotspotType.Multi:
                    if (Area.Count == 1)
                    {
                        if (X >= (Area[0].TopLeft.X * clickscale) && X <= (Area[0].BottomRight.X * clickscale) && Y >= (Area[0].TopLeft.Y * clickscale) && Y <= (Area[0].BottomRight.Y * clickscale) && InFrame)
                            return true;
                        return false;
                    }
                    if (Area.Count == 2)
                    {
                        float DistanceCompleted = ((float)(Milliseconds - startMS) / (float)(endMS - startMS));

                        long interpolatetopleftX = Area[0].TopLeft.X;
                        long interpolatetopleftY = Area[0].TopLeft.Y;
                        long interpolatebottomrightX = Area[0].BottomRight.X;
                        long interpolatebottomrightY = Area[0].BottomRight.Y;
                        interpolatetopleftX = (long)((float)(interpolatetopleftX + ((Area[1].TopLeft.X - Area[0].TopLeft.X) * DistanceCompleted)));
                        interpolatetopleftY = (long)((float)interpolatetopleftY + ((Area[1].TopLeft.Y - Area[0].TopLeft.Y) * DistanceCompleted));
                        interpolatebottomrightX = (long)((float)interpolatebottomrightX + ((Area[1].BottomRight.X - Area[0].BottomRight.X) * DistanceCompleted));
                        interpolatebottomrightY = (long)((float)interpolatebottomrightY + ((Area[1].BottomRight.Y - Area[0].BottomRight.Y) * DistanceCompleted));

                        if (X >= (interpolatetopleftX * clickscale) && X <= (interpolatebottomrightX * clickscale) && Y >= (interpolatetopleftY * clickscale) && Y <= (interpolatebottomrightY * clickscale) && InFrame)
                            return true;
                        return false;
                    }
                    break;


            }
            return false;
        }
        public void Draw(Grid parentElement, long Milliseconds, SceneDefinition scene, double visualizationWidthMultiplier, double visualizationHeightMultiplier)
        {
            int left = 0;
            int top = 0;
            int right = 0;
            int bot = 0;

            if (Area == null)
                return;

            if (Area.Count < 1)
                return;

            if (scene.Name.ToLowerInvariant() != RelativeVideoName.ToLowerInvariant() && !scene.Name.ToLowerInvariant().StartsWith("i_"))
                return;

            long startMS = Utilities.Frames15fpsToMS(FrameStart) + scene.StartMS;
            long endMS = Utilities.Frames15fpsToMS(FrameEnd) + scene.StartMS;

            bool InFrame = Milliseconds >= startMS && Milliseconds <= endMS;

            if (scene.Name.ToLowerInvariant().StartsWith("i_"))
            {
                InFrame =  (Milliseconds + 2000 >= Utilities.Frames15fpsToMS(FrameStart) && Milliseconds - 200 <= Utilities.Frames15fpsToMS(FrameEnd));
            }

            switch (HType)
            {
                case HotspotType.Diagnal:
                case HotspotType.Interpolate:
                case HotspotType.Multi:
                    if (Area.Count == 1)
                    {
                        left = (int)(Area[0].TopLeft.X * (_Scale * visualizationWidthMultiplier));
                        top = (int)(Area[0].TopLeft.Y * (_Scale * visualizationHeightMultiplier));
                        right = (int)(Area[0].BottomRight.X * (_Scale * visualizationWidthMultiplier));
                        bot = (int)(Area[0].BottomRight.Y * (_Scale * visualizationHeightMultiplier));
                    }


                    if (Area.Count > 1)
                    {
                        float DistanceCompleted = ((float)(Milliseconds - startMS) / (float)(endMS - startMS));
                        if (DistanceCompleted < 0)
                            DistanceCompleted = 0;

                        if (DistanceCompleted > 1)
                        {
                            DistanceCompleted = 1;
                        }

                        long interpolatetopleftX = Area[0].TopLeft.X;
                        long interpolatetopleftY = Area[0].TopLeft.Y;
                        long interpolatebottomrightX = Area[0].BottomRight.X;
                        long interpolatebottomrightY = Area[0].BottomRight.Y;
                        interpolatetopleftX = (long)((float)(interpolatetopleftX + ((Area[1].TopLeft.X - Area[0].TopLeft.X) * DistanceCompleted)));
                        interpolatetopleftY = (long)((float)interpolatetopleftY + ((Area[1].TopLeft.Y - Area[0].TopLeft.Y) * DistanceCompleted));
                        interpolatebottomrightX = (long)((float)interpolatebottomrightX + ((Area[1].BottomRight.X - Area[0].BottomRight.X) * DistanceCompleted));
                        interpolatebottomrightY = (long)((float)interpolatebottomrightY + ((Area[1].BottomRight.Y - Area[0].BottomRight.Y) * DistanceCompleted));

                        left = (int)(interpolatetopleftX * (_Scale * visualizationWidthMultiplier));
                        top = (int)(interpolatetopleftY * (_Scale * visualizationWidthMultiplier));
                        right = (int)(interpolatebottomrightX * (_Scale * visualizationWidthMultiplier));
                        bot = (int)(interpolatebottomrightY * (_Scale * visualizationWidthMultiplier));

                    }
                    break;
                    //break;


            }
           ;


            if (VisualizationControl == null)
            {
                Rectangle VisiRect = new Rectangle();
                VisiRect.Margin = new Thickness(left, top, 0, 0);// right - left, bot - top);
                VisiRect.HorizontalAlignment = HorizontalAlignment.Left;
                VisiRect.VerticalAlignment = VerticalAlignment.Top;
                VisiRect.Height = bot - top;
                VisiRect.Width = right - left;
                VisiRect.Stroke = new SolidColorBrush(Colors.Pink);
                VisiRect.StrokeThickness = 2;
                VisiRect.IsHitTestVisible = false;

                VisualizationControl = VisiRect;
                parentElement.Children.Add(VisiRect);

                Label displayLabel = new Label();
                displayLabel.Width = VisiRect.Width;
                displayLabel.Content = $"{Name}-|{RelativeVideoName}|{ActionVideo}";
                displayLabel.Margin = VisiRect.Margin;
                displayLabel.HorizontalAlignment = HorizontalAlignment.Left;
                displayLabel.IsHitTestVisible = false;
                //displayLabel.MouseMove += DisplayLabel_MouseMove;
                VisualizationLabel = displayLabel;
                parentElement.Children.Add(VisualizationLabel);

            }
            else
            {
                VisualizationControl.Margin = new Thickness(left, top, 0, 0);// right - left, bot - top);
                VisualizationControl.Height = bot - top;
                VisualizationControl.Width = right - left;

                if (scene.Name.ToLowerInvariant().StartsWith("i_"))
                {
                    VisualizationControl.Margin = new Thickness(left*2, top*2, 0, 0);
                    VisualizationControl.Width = (right - left) *6;
                    VisualizationControl.Height = (bot - top) *2;
                }
                VisualizationLabel.Margin = VisualizationControl.Margin;
                VisualizationLabel.Width = VisualizationControl.Width;
            }
            if (!InFrame)
            {
                VisualizationControl.Stroke = new SolidColorBrush(Colors.LimeGreen);
                VisualizationControl.Visibility = Visibility.Collapsed;
                VisualizationLabel.Visibility = Visibility.Collapsed;
            }
            else
            {
                if (ActionVideo.ToLowerInvariant().StartsWith("i_") || (Name.ToLowerInvariant().StartsWith("ip_") && !ActionVideo.ToLowerInvariant().StartsWith("d")))
                    VisualizationControl.Stroke = new SolidColorBrush(Colors.LimeGreen);
                else
                    VisualizationControl.Stroke = new SolidColorBrush(Colors.Red);
                VisualizationControl.Visibility = Visibility.Visible;
                VisualizationLabel.Foreground = Brushes.Red;
                VisualizationLabel.Visibility = Visibility.Visible;
            }

        }

        private void DisplayLabel_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            //if (VisualizationLabel != null)
            //{
            //    Grid parentGrid = VisualizationLabel.Parent as Grid;
            //    parentGrid.mouse
            //}

        }

        public void ClearVisualization()
        {
            if (VisualizationControl != null)
            {
                var item = VisualizationControl;
                VisualizationControl = null;
                Grid parentGrid = item.Parent as Grid;
                parentGrid.Children.Remove(item);

            }
            if (VisualizationLabel != null)
            {
                var item = VisualizationLabel;
                VisualizationLabel = null;
                //displayLabel.MouseMove -= DisplayLabel_MouseMove;
                Grid parentGrid = item.Parent as Grid;
                parentGrid.Children.Remove(item);

            }
        }
        private Rectangle VisualizationControl { get; set; }
        private Label VisualizationLabel { get; set; }

    }
    public class Box2d
    {
        public System.Drawing.Point TopLeft { get; set; }
        public System.Drawing.Point BottomRight { get; set; }
        public int ActionId { get; set; } = 0;
    }
    public class MultiAction
    {
        public int ClickIndex { get; set; }
        public List<int> NextButtonSequence { get; set; } = new List<int>();
        public string ResultVideo { get; set; } = string.Empty;

        //// THe problem that we have is we're not factoring in the last ID before the first Option/IF    Pretty sure this represents which button to load.
        //public override bool Equals(object obj)
        //{
        //    if (obj is MultiAction)
        //    {
        //        var objitem = obj as MultiAction;
        //        return objitem.ResultVideo == ResultVideo && objitem.NextButtonSequence == NextButtonSequence;
        //    }
        //    return base.Equals(obj);
        //}
        //public static bool operator ==(MultiAction m1, MultiAction m2)
        //{
        //    if (m1 == null && m2 == null)
        //        return true;
        //    if (m1 == null && m2 != null)
        //        return false;
        //    if (m2 == null && m1 != null)
        //        return false;
        //    return m1.Equals(m2);
        //}
        //public static bool operator !=(MultiAction m1, MultiAction m2)
        //{
        //    if (m1 == null && m2 != null)
        //        return true;
        //    if (m1 == null && m2 == null)
        //        return false;

        //    return !m1.Equals(m2);
        //}
        //public override string ToString()
        //{
        //    return base.ToString() + ClickIndex.ToString() + String.Join(",", NextButtonSequence.Select(x => x.ToString()).ToArray()) + ResultVideo.ToString();
        //}

        //public override int GetHashCode() => (ClickIndex, NextButtonSequence, ResultVideo).GetHashCode();
    }
}
