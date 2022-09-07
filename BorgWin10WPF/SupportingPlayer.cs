using LibVLCSharp;
using LibVLCSharp.WPF;
using LibVLCSharp.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xaml;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace BorgWin10WPF
{
    public delegate void EndScene(object o, string s, string t);

    public class SupportingPlayer : IDisposable
    {
        public event EndScene SceneComplete;

        private readonly VideoView _displayElement;
        private SceneDefinition _lastScene { get; set; }
        private SceneDefinition _currentScene { get; set; }
        private List<SceneDefinition> _InfoSceneOptions { get; set; }
        private List<SceneDefinition> _HolodeckSceneOptions { get; set; }

        private Grid _VisualizationGrid { get; set; }

        private PlayerBreadCrumbTrail InfoBreadcrumb = new PlayerBreadCrumbTrail();


        private bool _visualizationEnabled = false;
        public double VisualizationWidthMultiplier = 1d;
        public double VisualizationHeightMultiplier = 1d;

        private int _timerMS = 10;
        private int _sceneEndMS = 0;
        // I'm using the DispatcherTimer so that it is always on the main thread and we can interact with the unmanaged library.
        private DispatcherTimer _PlayHeadTimer = new DispatcherTimer();
        private string _info_videopath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "CDAssets", "IPX.AVI");
        private string _computer_videopath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "CDAssets", "computer.avi");
        private string _holodeck_videopath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "CDAssets", "holodeck.avi");
        private List<HotspotDefinition> _aggregatehotspots = new List<HotspotDefinition>();
        private LibVLC _libVLCInfo = null;
        private bool _InfoVideoLoaded;
        private bool _holodeckVideoLoaded;
        private bool _ComputerVideoLoaded;
        private long _lastPlayheadMS;
        private bool _loopVid = false;
        private string _videotype = string.Empty;
        private bool disposedValue;
        private bool _isOriginalVideo = true;
        private int _volume = 100;

        private bool _debugEventsOff = false;

        private readonly Queue<VideoQueueItem> _playQueue = null;
        private string _idleActionVisualizationText = string.Empty;
        private Label DoNothingVisualization;
        private float hotspotscale = 4f;
        // This resizes the visualization for the hotspots.   It's funky.  Don't mess with it.

        private void UnsetEQ()
        {
            _displayElement.MediaPlayer.UnsetEqualizer();
        }

        /// <summary>
        /// It looks like in some of the videos, they purposely injected noise to make the audio play poorly in other players.
        /// This cuts the noise down to barely noticable. It does drop the frequency of John de Lancie's voice..  but it is a 
        /// compromise that I can live with for the noise reduction and better understandability of his voice with the reduced noise.
        /// </summary>
        private void SetEQ()
        {
            if (_isOriginalVideo)
            {
                using (var _libvlcsharpEQ = new Equalizer())
                {
                    _libvlcsharpEQ.SetPreamp(11.9f);


                    // Adjustment that we're going for 
                    //   | | | | | | | | | |
                    //         __
                    //   -----/  \
                    //            \
                    //             |______
                    //

                    for (uint bandid = 0; bandid < _libvlcsharpEQ.BandCount; bandid++)
                    {
                        var freq = _libvlcsharpEQ.BandFrequency(bandid);
                        switch (freq)
                        {
                            case 500:

                                _libvlcsharpEQ.SetAmp(3.0f, bandid);
                                break;
                            case 1000:
                                _libvlcsharpEQ.SetAmp(-3.2f, bandid);
                                break;
                            case 4000:
                                _libvlcsharpEQ.SetAmp(-8.1f, bandid);
                                break;
                            case 8000:
                            case 12000:
                            case 14000:
                            case 16000:
                                _libvlcsharpEQ.SetAmp(-19.9f, bandid);
                                break;
                        }
                    }

                    _displayElement.MediaPlayer.UnsetEqualizer();
                    _displayElement.MediaPlayer.SetEqualizer(_libvlcsharpEQ);
                }
            }

        }

        public float HotspotScale
        {
            get { return hotspotscale; }
            set
            {
                hotspotscale = value;
                if (_currentScene == null)
                    return;
                foreach (var item in _currentScene.PlayingHotspots)
                    item.HotspotScale = value;

                foreach (var item in _currentScene.PausedHotspots)
                    item.HotspotScale = value;
                
            }
        }

        public SupportingPlayer(VideoView displayElement, List<SceneDefinition> InfoScenes, List<SceneDefinition> ComputerScenes, List<SceneDefinition> HolodeckScenes, List<HotspotDefinition> ips, LibVLC vlcobject)
        {
            _displayElement = displayElement;
            _aggregatehotspots = ips;
            _InfoSceneOptions = InfoScenes;
            //_ComputerSceneOptions = ComputerScenes;
            _HolodeckSceneOptions = HolodeckScenes;
            _libVLCInfo = vlcobject;
            _playQueue = new Queue<VideoQueueItem>();
            _PlayHeadTimer.Interval = new TimeSpan(0, 0, 0, 0, _timerMS);
            _PlayHeadTimer.Tick += (o, e) =>
            {
                TimerTickAction();

            };
           
            _volume = displayElement.MediaPlayer.Volume;
            
        }

       

        public void ClearQueue()
        {
            lock (_playQueue)
            {
                _playQueue.Clear();
            }
        }

        public void Pause()
        {
            _PlayHeadTimer.Stop();
            if (_displayElement.MediaPlayer.IsPlaying)
            {
                _displayElement.MediaPlayer.Pause();
                UnsetEQ();
            }
            
        }
        public void Resume()
        {
            if (!_displayElement.MediaPlayer.IsPlaying && _displayElement.MediaPlayer.WillPlay)
            {
                SetEQ();
                _displayElement.MediaPlayer.Play();
            }
            _PlayHeadTimer.Start();
        }

        /// <summary>
        /// Play a scene.  Start from the beginning of the scene or optionally have a time
        /// </summary>
        /// <param name="def"></param>
        /// <param name="specifictimecode"></param>
        public void QueueScene(SceneDefinition def, string type, long specifictimecode = 0, bool vloop = false)
        {
            if (def == null)
                return;

            bool playImmediately = false;
            if (def.Name.ToUpperInvariant().StartsWith("IP"))
            {
                playImmediately = true;
            }


            if (!playImmediately && _displayElement.MediaPlayer.IsPlaying) // We don't want Info items to queue up. They should play immediately
            {
                // Don't double queue the same scene.
                if (_currentScene.Name != def.Name)
                    lock (_playQueue)
                    {
                        _playQueue.Enqueue(new VideoQueueItem() { Video = def, VideoType = type, TimecodeMS = specifictimecode, loop = vloop });
                    }
            }
            else
            {
                playImmediately = true;
            }

            if (playImmediately)
                PlayScene(def, type, specifictimecode, vloop);


        }
        public void LowerVolume()
        {

            _volume -= 15;
            if (_volume < 0)
            {
                _volume = 0;
            }
            _displayElement.MediaPlayer.Volume = _volume;
        }

        public void IncreaseVolume()
        {
           
            _volume += 15;
            if (_volume > 100)
            {
                _volume = 100;
            }
            _displayElement.MediaPlayer.Volume = _volume;
        }
        private void PlayScene(SceneDefinition def, string type, long specifictimecode, bool loop, bool FromBreadcrumb = false)
        {
            _PlayHeadTimer.Stop();
            switch (type)
            {
                case "info":
                    if (!_InfoVideoLoaded || !_displayElement.MediaPlayer.IsSeekable)
                        SwitchToInfoVideo();
                    break;
                //case "computer":
                //    if (!_ComputerVideoLoaded || !_displayElement.MediaPlayer.IsSeekable)
                //        SwitchToComputerVideo();
                //    break;
                case "holodeck":
                    if (!_holodeckVideoLoaded || !_displayElement.MediaPlayer.IsSeekable)
                        SwitchToHolodeckVideo();
                    break;

            }
            _loopVid = loop;
            _videotype = type;
            _sceneEndMS = (int)def.EndMS;
            _currentScene = def;

            if (specifictimecode > 0 && _displayElement.MediaPlayer.Media.Duration > specifictimecode)
                _displayElement.MediaPlayer.Time = specifictimecode;
            else
            {
                _displayElement.MediaPlayer.Time = def.StartMS;
            }

            if (_displayElement.MediaPlayer.WillPlay)
            {
                SetEQ();
                _displayElement.MediaPlayer.Play();
            }
            _PlayHeadTimer.Start();
            //if (!FromBreadcrumb)
            //{
                
            //}
            System.Diagnostics.Debug.WriteLine(string.Format("\tSPlaying {1} Scene {0}", def.Name, type));
        }
        public string ScenePlaying
        {
            get
            {
                if (_currentScene == null)
                    return string.Empty;
                return _currentScene.Name;
            }
        }
        public void DebugSetEvents(bool OnOffYN)
        {
            _debugEventsOff = !OnOffYN;
        }
        private void TimerTickAction()
        {
            // There is nothing to do here if we don't have a scene.
            if (_currentScene == null)
                return;

            if (_displayElement != null && _displayElement.MediaPlayer != null)
            {
                if (_displayElement.MediaPlayer.State == VLCState.Ended && _loopVid)
                {
                    switch (_videotype)
                    {
                        case "holodeck":
                            SwitchToHolodeckVideo();
                            break;
                        case "info":
                            SwitchToInfoVideo();
                            break;
                            //case "computer":
                            //    SwitchToComputerVideo();
                            //    break;
                    }

                    _displayElement.MediaPlayer.Time = _currentScene.StartMS;
                    Task.Delay(30).Wait();
                    if (!_displayElement.MediaPlayer.IsPlaying)
                    {
                        SetEQ();
                        _displayElement.MediaPlayer.Play();
                    }
                }

                if (_displayElement.MediaPlayer.IsPlaying)
                {
                    _lastPlayheadMS = _displayElement.MediaPlayer.Time;
                }
                else
                {
                    _lastPlayheadMS = 0;
                }


                if (_lastPlayheadMS >= _sceneEndMS - _timerMS)
                {
                    InfoBreadcrumb.VisitedScene(_currentScene);
                    VideoQueueItem itemtoplay = null;
                    // Move on to next
                    lock (_playQueue)
                    {
                        VideoQueueItem queueentry = null;
                        if (_playQueue.Count > 0)
                        {
                            itemtoplay = _playQueue.Dequeue();
                        }
                        //if (_playQueue.TryDequeue(out queueentry))
                        //{
                        //    itemtoplay = queueentry;

                        //}
                    }
                    if (itemtoplay != null)
                    {
                        _PlayHeadTimer.Stop();
                        EndScene evt = SceneComplete;
                        if (evt != null && !_debugEventsOff)
                            evt(this, _currentScene.Name, _videotype);
                        _debugEventsOff = false; // Turn events back on after this debug trigger

                        PlayScene(itemtoplay.Video, itemtoplay.VideoType, itemtoplay.TimecodeMS, itemtoplay.loop);
                    }
                    else // loop only if there isn't anything in the queue
                    {
                        if (_loopVid)
                        {
                            if (_displayElement.MediaPlayer.IsSeekable)
                            {
                                _displayElement.MediaPlayer.Time = _currentScene.StartMS;
                                if (!_displayElement.MediaPlayer.IsPlaying)
                                {
                                    SetEQ();
                                    _displayElement.MediaPlayer.Play();
                                }
                            }
                            else // if the video isn't seekable, the video isn't loaded anymore
                            {
                                _ComputerVideoLoaded = false;
                                _holodeckVideoLoaded = false;
                                _InfoVideoLoaded = false;
                                PlayScene(_currentScene, _videotype, 0, true);
                            }
                        }
                        else // Not looping, no video to play.  Pause
                        {
                            _displayElement.MediaPlayer.Pause();
                            UnsetEQ();

                            EndScene evt = SceneComplete;
                            if (evt != null && !_debugEventsOff)
                                evt(this, _currentScene.Name, _videotype);
                            _debugEventsOff = false; // Turn events back on after this debug trigger
                        }
                    }
                }

            }

            if (_visualizationEnabled)
            {
                Grid ParentGrid = _VisualizationGrid as Grid;
                if (_currentScene == null)
                    return;
                List<HotspotDefinition> hotspotstocheck = _aggregatehotspots;//_currentScene.PausedHotspots;
                List<HotspotDefinition> inFrame = new List<HotspotDefinition>();
                var currtime = _displayElement.MediaPlayer.Time;
                bool playing = _displayElement.MediaPlayer.IsPlaying;
                foreach (var hotspot in hotspotstocheck)
                {

                    var FrameStartMS = Utilities.Frames15fpsToMS(hotspot.FrameStart) + _currentScene.OffsetTimeMS;
                    var FrameEndMS = Utilities.Frames15fpsToMS(hotspot.FrameEnd) + _currentScene.OffsetTimeMS;
                    //if (currtime >= FrameStartMS && currtime <= FrameEndMS)
                    //{
                    //    System.Diagnostics.Debug.WriteLine($"{hotspotInfo} ..|..");
                    //}
                    //else if (currtime + 4000 >= FrameStartMS && currtime <= FrameEndMS)
                    //{
                    //    System.Diagnostics.Debug.WriteLine($"{hotspotInfo} .<|..");
                    //}
                    //else if (currtime >= FrameStartMS && currtime + 4000 <= FrameEndMS)
                    //{
                    //    System.Diagnostics.Debug.WriteLine($"{hotspotInfo} ..|.>");
                    //}
                    //else if (currtime < FrameStartMS)
                    //{
                    //    //System.Diagnostics.Debug.WriteLine("<.|..");
                    //}
                    //else if (currtime > FrameEndMS)
                    //{
                    //    //System.Diagnostics.Debug.WriteLine("..|.>");
                    //}
                    if (currtime + 3000 >= FrameStartMS && currtime - 3000 <= FrameEndMS)
                    {
                        inFrame.Add(hotspot);
                    }
                
                }

                    

                foreach (var item in inFrame)
                    item.Draw(ParentGrid, _displayElement.MediaPlayer.Time, _currentScene, VisualizationWidthMultiplier, VisualizationHeightMultiplier);

                if (DoNothingVisualization != null)
                {
                    var frame15fps = Utilities.MsTo15fpsFrames(_lastPlayheadMS);
                    int cdvis = 0;
                    string scenename = String.Empty;
                    if (_currentScene != null)
                    {
                        cdvis = _currentScene.CD;
                        scenename = _currentScene.Name;
                    }

                    if (_displayElement.MediaPlayer.IsPlaying) // I don't want it to update the content if we're not playing because it will set the frame to zero.
                    {
                        string friendlytime = GetReadableTimeByMs(_lastPlayheadMS);
                        DoNothingVisualization.Content = $"Scene { scenename}, Frame: {frame15fps}, CD: {cdvis}, {_idleActionVisualizationText}. ({friendlytime})";
                    }
                }
            }
        }
        private void SwitchToInfoVideo()
        {
            SwitchVideo(GetMP4OrAVI(_info_videopath));
            _InfoVideoLoaded = true;
            _ComputerVideoLoaded = false;
            _holodeckVideoLoaded = false;
        }
        public string GetMP4OrAVI(string input)
        {
            string mp4name = input.Replace("X.AVI", "X.mp4");
            if (System.IO.File.Exists(mp4name))
            {
                _isOriginalVideo = false;
                return mp4name;
            }
            return input;
        }
        //private void SwitchToComputerVideo()
        //{
        //    SwitchVideo(_computer_videopath);
        //    _ComputerVideoLoaded = true;
        //    _InfoVideoLoaded = false;
        //    _holodeckVideoLoaded = false;
        //}
        private void SwitchToHolodeckVideo()
        {
            SwitchVideo(_holodeck_videopath);
            _ComputerVideoLoaded = false;
            _InfoVideoLoaded = false;
            _holodeckVideoLoaded = true;
        }

        private void SwitchVideo(string path)
        {
            using (var media = new Media(_libVLCInfo, path, FromType.FromPath))
            {
                _displayElement.MediaPlayer.Play(media);
                WaitWhileLoading(path);
                _displayElement.MediaPlayer.Pause();
            }
        }

        private void WaitWhileLoading(string filename)
        {
            int whilelooptimeout = 0;
            while (!_displayElement.MediaPlayer.IsPlaying)
            {
                Task.Delay(50).Wait();
                if (++whilelooptimeout > 300)
                {
                    string ExceptionMessage = "The Video or Video Player cannot be initialized loading file {0}";
                    throw new Exception(string.Format(ExceptionMessage, filename));
                }
            }
        }

        private void BackClicked()
        {
            if (InfoBreadcrumb.HistoryCount > 0)
            {
                SceneDefinition backScene = InfoBreadcrumb.Back();
                if (backScene != null)
                {
                    _playQueue.Clear();
                    _displayElement.MediaPlayer.Pause();
                    UnsetEQ();
                    PlayScene(backScene, "info", 0, false, true);
                }
            }
        }
        private void ForwardClicked()
        {
            if (InfoBreadcrumb.ForwardCount > 0)
            {
                SceneDefinition backScene = InfoBreadcrumb.Forward();
                if (backScene != null)
                {
                    _playQueue.Clear();
                    _displayElement.MediaPlayer.Pause();
                    UnsetEQ();
                    PlayScene(backScene, "info", 0, false, true);
                }
            }
        }

        private void NextSceneClicked()
        {
            if (!_displayElement.MediaPlayer.IsPlaying && _currentScene != null)
            {
                int foundinListIterator = 0;
                for (int i=0;i< _InfoSceneOptions.Count;i++)
                {
                    if (_currentScene.Name == _InfoSceneOptions[i].Name)
                    {
                        foundinListIterator = i;
                    }
                }
                if (_InfoSceneOptions.Count > foundinListIterator)
                {
                    var def = _InfoSceneOptions[foundinListIterator + 1];
                    if (def != null)
                    {
                        _playQueue.Clear();
                        _displayElement.MediaPlayer.Pause();
                        UnsetEQ();
                        PlayScene(def, "info", 0, false, false);
                    }
                }
            }
        }

        public void MouseClick(int X, int Y)
        {
            if (_currentScene == null)
                return;
            List<HotspotDefinition> inFrame = new List<HotspotDefinition>();

            //item.Draw(ParentGrid, _displayElement.MediaPlayer.Time, _currentScene);
            X += 5;
            //X += 10;
            Y += 6;
            var currtime = _displayElement.MediaPlayer.Time;
            bool playing = _displayElement.MediaPlayer.IsPlaying;
            List<HotspotDefinition> hotspotstocheck = _aggregatehotspots;//_currentScene.PausedHotspots;
            foreach (var hotspot in hotspotstocheck)
            {

                var FrameStartMS = Utilities.Frames15fpsToMS(hotspot.FrameStart) + _currentScene.OffsetTimeMS;
                var FrameEndMS = Utilities.Frames15fpsToMS( hotspot.FrameEnd) + _currentScene.OffsetTimeMS;
                string hotspotInfo = string.Format($"\t[{hotspot.Name}]: {FrameStartMS}:{currtime}:{FrameEndMS}");
                if (currtime >= FrameStartMS && currtime <= FrameEndMS)
                {
                    System.Diagnostics.Debug.WriteLine($"{hotspotInfo} ..|..");
                }
                else if (currtime+4000 >= FrameStartMS && currtime <= FrameEndMS)
                {
                    System.Diagnostics.Debug.WriteLine($"{hotspotInfo} .<|..");
                }
                else if (currtime >= FrameStartMS && currtime+4000 <= FrameEndMS)
                {
                    System.Diagnostics.Debug.WriteLine($"{hotspotInfo} ..|.>");
                }
                else if (currtime < FrameStartMS)
                {
                    //System.Diagnostics.Debug.WriteLine("<.|..");
                }
                else if (currtime > FrameEndMS)
                {
                    //System.Diagnostics.Debug.WriteLine("..|.>");
                }
                if (currtime+2000 >= FrameStartMS && currtime - 2000 <= FrameEndMS)
                {
                    inFrame.Add(hotspot);
                }
            }
            for (int i = 0; i < inFrame.Count; i++)
            {
                var hittestresults = (inFrame[i].HitTest(X, Y, currtime, _currentScene,false));
                System.Diagnostics.Debug.WriteLine(string.Format("\t[{0}]: Hit test {1},{2}-{7}.  Box (X:{3}-{4},Y:{5}-{6})", inFrame[i].Name + "/" + inFrame[i].ActionVideo, X, Y, inFrame[i].Area[0].TopLeft.X, inFrame[i].Area[0].BottomRight.X, inFrame[i].Area[0].TopLeft.Y, inFrame[i].Area[0].BottomRight.Y, hittestresults));
                if (hittestresults)
                {
                    SceneDefinition clickactionScene = null;
                    foreach (var clickactionpotential in _InfoSceneOptions)
                    {
                        if (clickactionpotential.Name.ToLowerInvariant() == inFrame[i].ActionVideo.ToLowerInvariant())
                        {
                            clickactionScene = clickactionpotential;
                            break;
                        }
                    }
                    _InfoSceneOptions.Where(xy => xy.Name == inFrame[i].ActionVideo).FirstOrDefault();
                    if (clickactionScene != null)
                    {
                        QueueScene(clickactionScene, "info", 0, false);
                        System.Diagnostics.Debug.WriteLine($"Fired off {inFrame[i].ActionVideo}.");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"We are supposed to fire off {inFrame[i].ActionVideo} but we could not find it.");
                    }
                }
            }
            //10,176-33,193

            if (X >= 24 && X <=45 && Y >=178 && Y <=194)
            {
                //Exit button
                InfoBreadcrumb.VisitedScene(_currentScene);
                var endscenevent = SceneComplete;
                if (endscenevent != null)
                {
                    endscenevent(this, "ExitButton", "info");
                }
            }
            if (X >= 24 && X <= 43 && Y >= 84 && Y <= 101)
            {
                //Back Button 25,84,44,94

                BackClicked();
            }
            if (X >= 24 && X <= 43 && Y >= 102 && Y <= 116)
            {
                //Forward Button 24,97,38,116, 23,157,45,176
                ForwardClicked();

            }
            if (X >= 24 && X <= 157 && Y >= 102 && Y <= 176)
            {

               NextSceneClicked();
            }
        }
        /// <summary>
        /// Add squares around the clickable hotspots
        /// </summary>
        /// <param name="ParentGrid"></param>
        public void VisualizeHotspots(Grid ParentGrid)
        {
            if (_visualizationEnabled)
                return;
            if (_currentScene == null)
                return;
            foreach (var item in _aggregatehotspots)
                item.Draw(ParentGrid, _displayElement.MediaPlayer.Time, _currentScene, VisualizationWidthMultiplier, VisualizationHeightMultiplier);
            //foreach (var item in _currentScene.PlayingHotspots)
            //    

            //foreach (var item in _currentScene.PausedHotspots)
            //    item.Draw(ParentGrid, _displayElement.MediaPlayer.Time, _currentScene, VisualizationWidthMultiplier, VisualizationHeightMultiplier);

            if (DoNothingVisualization == null)
            {
                _VisualizationGrid = ParentGrid;
                Label displayLabel = new Label();
                displayLabel.Width = ParentGrid.Width;
                displayLabel.Height = ParentGrid.Height;
                var frame15fps = Utilities.MsTo15fpsFrames(_lastPlayheadMS);
                int cdvis = 0;
                string scenename = String.Empty;
                if (_currentScene != null)
                {
                    cdvis = _currentScene.CD;
                    scenename = _currentScene.Name;
                }
                if (_displayElement.MediaPlayer.IsPlaying) // I don't want it to update the content if we're not playing because it will set the frame to zero.
                {
                    string friendlytime = GetReadableTimeByMs(_lastPlayheadMS);
                    displayLabel.Content = $"Scene { scenename}, Frame: {frame15fps}, CD: {cdvis}, {_idleActionVisualizationText}. ({friendlytime})";
                }
                displayLabel.Margin = new Thickness(5, 0, 0, 50);
                displayLabel.HorizontalAlignment = HorizontalAlignment.Left;
                displayLabel.Foreground = Brushes.Red;

                displayLabel.IsHitTestVisible = false;
                //displayLabel.MouseMove += DisplayLabel_MouseMove;
                DoNothingVisualization = displayLabel;
                ParentGrid.Children.Add(DoNothingVisualization);
            }
            
            _visualizationEnabled = true;
        }
        public string GetReadableTimeByMs(long ms)
        {
            TimeSpan t = TimeSpan.FromMilliseconds(ms);
            if (t.Hours > 0) return $"{t.Hours}h:{t.Minutes}m:{t.Seconds}s";
            else if (t.Minutes > 0) return $"{t.Minutes}m:{t.Seconds}s";
            else if (t.Seconds > 0) return $"{t.Seconds}s:{t.Milliseconds}ms";
            else return $"{t.Milliseconds}ms";
        }

        /// <summary>
        /// Remove the squares that represent the hotspot positions.
        /// </summary>
        public void VisualizeRemoveHotspots()
        {
            if (!_visualizationEnabled)
                return;

            _visualizationEnabled = false;

            if (DoNothingVisualization != null)
            {
                var item = DoNothingVisualization;
                DoNothingVisualization = null;
                //displayLabel.MouseMove -= DisplayLabel_MouseMove;
                Grid parentGrid = item.Parent as Grid;
                parentGrid.Children.Remove(item);
            }

            foreach (var item in _aggregatehotspots)
                item.ClearVisualization();
            


        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)

                }


                //_displayElement.MediaPlayer.Dispose();
                disposedValue = true;
            }
        }

        // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        ~SupportingPlayer()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
    internal class VideoQueueItem
    {
        public SceneDefinition Video { get; set; }
        public string VideoType { get; set; }
        public bool loop { get; set; } = false;
        public long TimecodeMS { get; set; } = 0;
    }
}
