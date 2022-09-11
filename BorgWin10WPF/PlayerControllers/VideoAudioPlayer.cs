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
using BorgWin10WPF.Hotspot;
using BorgWin10WPF.Scene;


namespace BorgWin10WPF.PlayerControllers
{

    public class VideoAudioPlayer : IDisposable
    {
        public event EndScene SceneComplete;

        private readonly VideoView _displayElement;
        private SceneDefinition _lastScene { get; set; }
        private SceneDefinition _currentScene { get; set; }
        private List<SceneDefinition> _InfoSceneOptions { get; set; }
        private List<SceneDefinition> _HolodeckSceneOptions { get; set; }


        private int _timerMS = 10;
        private int _sceneEndMS = 0;
        // I'm using the DispatcherTimer so that it is always on the main thread and we can interact with the unmanaged library.
        private DispatcherTimer _PlayHeadTimer = new DispatcherTimer();
        private string _info_videopath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "CDAssets", "IP.AVI");
        private string _computer_videopath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "CDAssets", "computer.avi");
        private string _holodeck_videopath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "Assets", "BridgeHum.wav");
        private List<HotspotDefinition> _aggregatehotspots = new List<HotspotDefinition>();
        private LibVLC _libVLCInfo = null;
        private bool _InfoVideoLoaded;
        private bool _holodeckVideoLoaded;
        private bool _ComputerVideoLoaded;
        private long _lastPlayheadMS;
        private bool _loopVid = false;
        private string _videotype = string.Empty;
        private bool disposedValue;

        private bool _debugEventsOff = false;

        private readonly Queue<VideoQueueItem> _playQueue = null;


        public VideoAudioPlayer(VideoView displayElement, List<SceneDefinition> InfoScenes, List<SceneDefinition> ComputerScenes, List<SceneDefinition> HolodeckScenes, List<HotspotDefinition> ips, LibVLC vlcobject)
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
                _displayElement.MediaPlayer.Pause();
        }
        public void Resume()
        {
            if (!_displayElement.MediaPlayer.IsPlaying && _displayElement.MediaPlayer.WillPlay)
                _displayElement.MediaPlayer.Play();
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
            var vol = _displayElement.MediaPlayer.Volume;
            vol -= 15;
            if (vol < 0)
            {
                vol = 0;
            }
            _displayElement.MediaPlayer.Volume = vol;
        }

        public void IncreaseVolume()
        {
            var vol = _displayElement.MediaPlayer.Volume;
            vol += 15;
            if (vol > 100)
            {
                vol = 100;
            }
            _displayElement.MediaPlayer.Volume = vol;
        }
        private void PlayScene(SceneDefinition def, string type, long specifictimecode, bool loop)
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
                _displayElement.MediaPlayer.Play();
            _PlayHeadTimer.Start();

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

                            EndScene evt = SceneComplete;
                            if (evt != null && !_debugEventsOff)
                                evt(this, _currentScene.Name, _videotype);
                            _debugEventsOff = false; // Turn events back on after this debug trigger
                        }
                    }
                }

            }


        }

        private void SwitchToInfoVideo()
        {
            SwitchVideo(_info_videopath);
            _InfoVideoLoaded = true;
            _ComputerVideoLoaded = false;
            _holodeckVideoLoaded = false;
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
        ~VideoAudioPlayer()
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
}
