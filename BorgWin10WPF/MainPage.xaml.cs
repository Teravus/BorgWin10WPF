using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LibVLCSharp;
using LibVLCSharp.WPF;
using LibVLCSharp.Shared;
using System.Windows.Threading;
using XamlAnimatedGif;
using System.Runtime.InteropServices;

namespace BorgWin10WPF
{
    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPage : Window
    {
        // Reference to the LibVLC Unmanaged Library
        private LibVLC _libVLCMain;

        // In the UWP this is separate because each view area on the form is tied to a single LibVLC.   
        // In WPF, this is just another name for the above single LibVLC.   
        // If you don't have a second one in the UWP app, the screen gets overwritten by the supporting player and the game crashes!
        private LibVLC _libVLCInfo;

        private LibVLC _libVLCAudio;

        // The main video media player.   We keep it because we have to dispose it when closing.
        private LibVLCSharp.Shared.MediaPlayer _mediaPlayerMain;

        // To distinguish between single clicks and double clicks, we record the place the user clicks and then..   we wait for the timer to fire.
        // if the timer fires, we know they only clicked once.   If the timer fire, they double clickd!
        private Point _lastClickPoint = new Point(0d, 0d);

        // Original video heights and widths..    When the main video loads, these get set.
        private static double _OriginalMainVideoHeight = 200d;
        private static double _OriginalMainVideoWidth = 320d;

        private static double _HotspotOriginalMainVideoHeight = 200d;
        private static double _HotspotOriginalMainVideoWidth = 320d;

        // Aspect ratio.  Used for calculating the black bar offset when window doesn't match the aspect ratio.
        private double _OriginalAspectRatio = _OriginalMainVideoWidth / _OriginalMainVideoHeight;

        // All Scenes
        private List<SceneDefinition> _scenes = new List<SceneDefinition>();

        // Just Info scenes (ip000)
        private List<SceneDefinition> _infoScenes = new List<SceneDefinition>();

        // Just Computer video Scenes (Replaying from time Stop Zero Zero.  Zero Zero.  Zero Zero.)
        private List<SceneDefinition> _computerScenes = new List<SceneDefinition>();

        // Just the holodeck video scenes (Chirp, and hum)
        private List<SceneDefinition> _holodeckScenes = new List<SceneDefinition>();

        // All the hotspots!
        private List<HotspotDefinition> _hotspots = new List<HotspotDefinition>();

        private List<HotspotDefinition> _infohotspots = new List<HotspotDefinition>();

        // This is the visualization of the translated position that you clicked.
        private Rectangle _clickRectangle = null;

        // Soo..  when the title bar is visible, we have to offset things further
        private int _titlebarsize = 40;

        // When the mouse is higher than this, show the title bar.
        private int _titlebarShowHeight = 20;

        // This is the current size of the titlebar.
        private int _titlebarcurrentsize = 0;

        // This is the tile bar pixels when it is hidden.  (spoiler: It still takes up some space)
        private int _titlebarinvisibleheight = 15;

        // This is the timer that keeps track of if you single clicked.
        private readonly DispatcherTimer _clickTimer = new DispatcherTimer();

        // If we successfully loaded the main video.
        private bool _MainVideoLoaded = false;

        // Is the cursor visible?
        private bool _mcurVisible = false;

        // Here is the main processor for the game.
        private ScenePlayer _mainScenePlayer = null;

        // This plays sound only videos..   that support the main player.   The main player needs this.  It is precious.
        private SupportingPlayer _supportingPlayer = null;

        private VideoAudioPlayer _videoAudioPlayer = null;

        // So..  if the game is expecting input from the user...  Don't allow them to pause the game.
        private bool _actionTime = false;

        // Delegate that defines what happens when the debug combobox with the main scenes is changed.
        // We keep a reference here because we hook and unhook from this event.
        private SelectionChangedEventHandler lstSceneChanged = null;

        // This is the tricorder cursor for when the game is paused.
        BitmapImage TricorderCursor = new BitmapImage(new Uri(System.IO.Path.Combine("Assets", "TricorderHolodeckCur.gif"), UriKind.Relative));

        // This is the borg cube when the game says User do something.
        BitmapImage CubeCursor = new BitmapImage(new Uri(System.IO.Path.Combine("Assets", "BorgCubeCursor.gif"), UriKind.Relative));

        private bool TricorderOpen = false;

        private bool SwapDisplay = false;

        // We have two VideoViews on the form.   The loading order isn't guaranteed.   So..   we keep track of if we have initialized libVLC with this
        bool _coreVLCInitialized = false;

        TricorderGifAnimationController TricorderAnimation;

        public MainPage()
        {
            InitializeComponent();

            this.KeyUp += (ob, ea) =>
            {
                Keyup(ob, ea);
            };

            this.KeyDown += (ob, ea) =>
            {
                Keydown(ob, ea);
            };

            _clickTimer.Interval = TimeSpan.FromSeconds(0.2); //wait for the other click for 200ms

            _clickTimer.Tick += (o1, em) =>
            {
                lock (_clickTimer)
                    _clickTimer.Stop();

                var clickareawidth = ClickSurface.ActualWidth;
                var clickareaheight = ClickSurface.ActualHeight;

                // ILOVEPIE ( https://github.com/ILOVEPIE )  suggested this alternative to my broken home-grown code.
                var letterbox_width = Math.Max(0, clickareawidth - (_OriginalAspectRatio * clickareaheight)) * 0.5f;
                var letterbox_height = Math.Max(0, clickareaheight - (clickareawidth / _OriginalAspectRatio)) * 0.5f;

                var relclickX = (int)((_lastClickPoint.X - letterbox_width) / ((clickareawidth - (letterbox_width * 2)) / _HotspotOriginalMainVideoWidth));
                var relclickY = (int)((_lastClickPoint.Y - letterbox_height) / ((clickareaheight - (letterbox_height * 2)) / _HotspotOriginalMainVideoHeight));
                relclickY += 19; // Borg wants things offset by more
                long time = 0;
                TimeSpan ts = TimeSpan.Zero;
                // When you click, it shows a debug message on the output window.  Including the current time in milliseconds since video start.
                if (_MainVideoLoaded)
                {
                    time = VideoView.MediaPlayer.Time;
                    ts = TimeSpan.FromMilliseconds(time);
                }
                System.Diagnostics.Debug.WriteLine("{0},{1}({2},{3})[{4}] - t{5} - f{6}", _lastClickPoint.X, _lastClickPoint.Y, relclickX, relclickY, ts.ToString(@"hh\:mm\:ss"), (long)((float)time), Utilities.MsTo15fpsFrames(time));

                // If we have loaded the main video and player..   Tell it a user clickdd!
                if (_MainVideoLoaded && _mainScenePlayer != null && !string.IsNullOrEmpty(_mainScenePlayer.ScenePlaying))
                {
                    if (!TricorderOpen)
                        _mainScenePlayer.MouseClick(relclickX, relclickY);
                    else
                    {
                        //Original Video: 256x200
                        double OriginalInfoHeight = 200d;
                        double OriginalInfoWidth = 256d;
                        double infoClickAreaWidth = VideoInfo.ActualWidth;
                        double infoClickAreaHeight = VideoInfo.ActualHeight;
                        relclickX = (int)(_lastClickPoint.X / (infoClickAreaWidth / OriginalInfoWidth));
                        relclickY = (int)(_lastClickPoint.Y / (infoClickAreaHeight / OriginalInfoHeight));
                        _supportingPlayer.MouseClick(relclickX, relclickY);
                    }
                }
                // trnslated click Visualization for the click spot
                if (_clickRectangle == null)
                {
                    _clickRectangle = new Rectangle();
                    _clickRectangle.Margin = new Thickness(relclickX, relclickY, 0, 0);// right - left, bot - top);
                    _clickRectangle.HorizontalAlignment = HorizontalAlignment.Left;
                    _clickRectangle.VerticalAlignment = VerticalAlignment.Top;
                    _clickRectangle.Height = 2;
                    _clickRectangle.Width = 2;
                    _clickRectangle.Stroke = new SolidColorBrush(Colors.Pink);
                    _clickRectangle.StrokeThickness = 2;
                    if (txtMS.Visibility == Visibility.Visible && txtOffsetMs.Visibility == Visibility.Visible)
                        _clickRectangle.Visibility = Visibility.Visible;
                    else
                        _clickRectangle.Visibility = Visibility.Collapsed;

                    VVGrid.Children.Add(_clickRectangle);
                }
                else
                {
                    _clickRectangle.Margin = new Thickness(relclickX, relclickY, 0, 0);// right - left, bot - top);
                }
                

            };

            // Oh hey! our main form loaded!   Let's do stuff!
            Loaded += (s, e) =>
            {


                // Oh No.  We have a Un handle d.  Err or. Show The Use er!
                Application.Current.DispatcherUnhandledException += (o, err) =>
                {
                    Exception unhandledException = err.Exception;
                    if (!(unhandledException is OutOfMemoryException || unhandledException is StackOverflowException || unhandledException is SEHException))
                    {

                        // Because vlc player only lets you draw controlls on top of it if the controls are in the content of the tag..  
                        // We have to show errors in potentially two spots.   Spot 1.   If the video player is loaded.
                        // Spot 2!   If the vido player isn't loaded.

                        //Video Player Loaded
                        if (_mainScenePlayer != null)
                        {
                            txtGenericErrorText.Text = unhandledException.Message;
                            GenericErrorDialog.Visibility = Visibility.Visible;
                            _mcurVisible = true;
                            CurEmulator.Source = CubeCursor;
                            AnimationBehavior.SetSourceUri(CurEmulator, CubeCursor.UriSource);
                            CurEmulator.Visibility = Visibility.Visible;
                            err.Handled = true;
                            return;
                        }

                        // Video player not loaded.
                        VideoErrorDialog.Visibility = Visibility.Visible;
                        txtVideoErrorText.Text = unhandledException.Message;
                        _mcurVisible = true;
                        CurEmulator.Source = CubeCursor;
                        AnimationBehavior.SetSourceUri(CurEmulator, CubeCursor.UriSource);
                        CurEmulator.Visibility = Visibility.Visible;
                        err.Handled = true;
                        err.Handled = true;
                    }
                };
                // When the libVLC player control is loaded, initialize the unmanaged libVLC library. 
                // Loading order is not guaranteed so..    the other viewer may load first.
                // Only initialize libvlc once.
                VideoView.Loaded += (s1, e1) =>
                {
                    if (!_coreVLCInitialized)
                    {
                        _coreVLCInitialized = true;
                        Core.Initialize();

                        // Command line options to VLC
                        List<string> options = new List<string>();

                        var optionsarray = options.ToArray();
                        _libVLCMain = new LibVLC(optionsarray);
                        _libVLCInfo = _libVLCMain;
                    }
                    _mediaPlayerMain = new LibVLCSharp.Shared.MediaPlayer(_libVLCMain);
                    VideoView.MediaPlayer = _mediaPlayerMain;
                    VideoView.MediaPlayer.Scale = 0;
                    VideoView.MediaPlayer.AspectRatio = "4:3";
                    SwapDisplay = !SwapDisplay;
                    // If you want console spam.  Uncomment this and the line in log_fired to lag the game..   and..  get the reason why libVLC is not happy.
                    // _libVLCMain.Log += Log_Fired;


                    VideoView.PreviewMouseDown += (s2, e2) =>
                    {
                        if (e2.ClickCount != 2)
                            return;
                        if (!_actionTime) // No pausing during active time.  It is too difficult to separate single and double clicks during some scenes that you need to rapid click.
                        {

                            //SwitchGameModeActiveInfo();

                            e2.Handled = true;

                            lock (_clickTimer)
                                _clickTimer.Stop();
                        }
                    };
                    VideoView.MouseDoubleClick += (s2, e2) =>
                    {
                        if (!_actionTime) // No pausing during active time.  It is too difficult to separate single and double clicks during some scenes that you need to rapid click.
                        {

                            SwitchGameModeActiveInfo();

                            e2.Handled = true;

                            lock (_clickTimer)
                                _clickTimer.Stop();
                        }
                    };
                    VideoViewGrid.MouseDown += (s2, e2) =>
                    {
                        if (e2.ClickCount != 2)
                            return;
                        if (!_actionTime) // No pausing during active time.  It is too difficult to separate single and double clicks during some scenes that you need to rapid click.
                        {

                            //SwitchGameModeActiveInfo();

                            e2.Handled = true;

                            lock (_clickTimer)
                                _clickTimer.Stop();
                        }
                    };

                    VideoView.MouseDown += (s3, s4) =>
                    {
                        var tappedspot = s4.GetPosition(s3 as UIElement);


                        _lastClickPoint = tappedspot;

                        lock (_clickTimer)
                        {
                            _clickTimer.Stop();
                            _clickTimer.Start();
                        }
                    };

                    VideoViewGrid.KeyUp += (o4, s5) =>
                    {
                        Keyup(o4, s5);
                    };
                    VideoViewGrid.KeyDown += (o4, s5) =>
                    {
                        Keydown(o4, s5);
                    };
                    this.KeyUp += (o4, s5) =>
                    {
                        Keyup(o4, s5);
                    };
                    this.KeyDown += (o4, s5) =>
                    {
                        Keydown(o4, s5);
                    };
                    VideoView.KeyUp += (o4, s5) =>
                    {
                        Keyup(o4, s5);
                    };
                    VideoView.KeyDown += (o4, s5) =>
                    {
                        Keydown(o4, s5);
                    };


                };

                // When the libVLC player control is loaded, initialize the unmanaged libVLC library. 
                // Loading order is not guaranteed so..    the other viewer may load first.
                // Only initialize libvlc once.
                VideoInfo.Loaded += (s2, e2) =>
                {
                    if (!_coreVLCInitialized)
                    {
                        _coreVLCInitialized = true;
                        Core.Initialize();
                        // Command line Options to VLC
                        List<string> options = new List<string>();
                        var optionsarray = options.ToArray();
                        _libVLCMain = new LibVLC(optionsarray);
                        _libVLCInfo = _libVLCMain;
                    }
                    List<string> options2 = new List<string>();


                    _libVLCInfo = _libVLCMain;

                    var _mediaPlayerInfo = new LibVLCSharp.Shared.MediaPlayer(_libVLCInfo);
                    VideoInfo.MediaPlayer = _mediaPlayerInfo;
                    _mediaPlayerInfo.EnableMouseInput = false;
                    _mediaPlayerInfo.EnableKeyInput = false;

                    //// Uncomment this and the line in log_fired to lag the game..   and..  get the reason why libVLC is not happy.
                    ////_libVLCInfo.Log += Log_Fired;
                    _scenes = SceneLoader.LoadScenesFromAsset("scenes.txt");
                    var InfoScenes = _scenes.Where(xy => xy.SceneType == SceneType.Info).ToList();
                    var _ipsHotspots = HotspotLoader.LoadHotspotsFromAsset("ips.txt");
                    _infoScenes = InfoScenes;
                    for (int i=0;i<_infoScenes.Count; i++)
                    {
                        _infoScenes[i].FrameStart += 2;
                        _infoScenes[i].FrameEnd -= 20;
                    }
                    for (int i = 0; i < _ipsHotspots.Count; i++)
                    {
                        _ipsHotspots[i].FrameStart -= (int)Utilities.MsTo15fpsFrames(6000);
                        _ipsHotspots[i].FrameEnd += (int)Utilities.MsTo15fpsFrames(2000);
                    }
                        
                    //var ComputerScenes = SceneLoader.LoadSupportingScenesFromAsset("computerscenes.txt");
                    //_computerScenes = ComputerScenes;
                    var HolodeckScenes = SceneLoader.LoadSupportingScenesFromAsset("holodeckscenes.txt");
                    //var HolodeckScenes = new List<SceneDefinition>();

                    _holodeckScenes = HolodeckScenes;
                    //_supportingPlayer = new SupportingPlayer(VideoInfo, InfoScenes, ComputerScenes, HolodeckScenes, _libVLCInfo);
                    _supportingPlayer = new SupportingPlayer(VideoInfo, InfoScenes, null, HolodeckScenes, _ipsHotspots, _libVLCInfo);
                    //Load_Computer_list(_computerScenes);

                };

                // Only initialize libvlc once.
                VideoAudio.Loaded += (s2, e2) =>
                {
                    if (!_coreVLCInitialized)
                    {
                        _coreVLCInitialized = true;
                        Core.Initialize();
                        // Command line Options to VLC
                        List<string> options = new List<string>();
                        var optionsarray = options.ToArray();
                        _libVLCMain = new LibVLC(optionsarray);
                        _libVLCInfo = _libVLCMain;
                        _libVLCAudio = _libVLCMain;
                    }
                    List<string> options2 = new List<string>();


                    _libVLCAudio = _libVLCMain;

                    var _mediaPlayerInfo = new LibVLCSharp.Shared.MediaPlayer(_libVLCAudio);
                    VideoAudio.MediaPlayer = _mediaPlayerInfo;
                    _mediaPlayerInfo.EnableMouseInput = false;
                    _mediaPlayerInfo.EnableKeyInput = false;

                    //// Uncomment this and the line in log_fired to lag the game..   and..  get the reason why libVLC is not happy.
                    ////_libVLCInfo.Log += Log_Fired;
                    //_scenes = SceneLoader.LoadScenesFromAsset("scenes.txt");
                    //var InfoScenes = _scenes.Where(xy => xy.SceneType == SceneType.Info).ToList();
                    //var _ipsHotspots = HotspotLoader.LoadHotspotsFromAsset("ips.txt");
                    //_infoScenes = InfoScenes;
                    //for (int i = 0; i < _infoScenes.Count; i++)
                    //{
                    //    _infoScenes[i].FrameStart += 2;
                    //    _infoScenes[i].FrameEnd -= 20;
                    //}
                    //for (int i = 0; i < _ipsHotspots.Count; i++)
                    //{
                    //    _ipsHotspots[i].FrameStart -= (int)Utilities.MsTo15fpsFrames(6000);
                    //    _ipsHotspots[i].FrameEnd += (int)Utilities.MsTo15fpsFrames(2000);
                    //}

                    //var ComputerScenes = SceneLoader.LoadSupportingScenesFromAsset("computerscenes.txt");
                    //_computerScenes = ComputerScenes;
                    var HolodeckScenes = SceneLoader.LoadSupportingScenesFromAsset("holodeckscenes.txt");
                    //var HolodeckScenes = new List<SceneDefinition>();

                    _videoAudioPlayer = new VideoAudioPlayer(VideoAudio, null, null, HolodeckScenes, null, _libVLCAudio);
                    _videoAudioPlayer.LowerVolume();
                    _videoAudioPlayer.LowerVolume();
                    _videoAudioPlayer.LowerVolume();
                    //_supportingPlayer = new SupportingPlayer(VideoInfo, InfoScenes, ComputerScenes, HolodeckScenes, _libVLCInfo);
                    //_supportingPlayer = new SupportingPlayer(VideoInfo, InfoScenes, null, HolodeckScenes, _ipsHotspots, _libVLCInfo);
                    //Load_Computer_list(_computerScenes);

                };
                // Save this as a delgate because we need to hook/unhook it
                lstSceneChanged = (o, arg) =>
                {

                    var ClickedItems = arg.AddedItems;
                    foreach (var item in ClickedItems)
                    {
                        ComboBoxItem citem = item as ComboBoxItem;
                        string scenename = citem.Content.ToString();
                        SceneDefinition founddef = null;
                        foreach (var scenedef in _scenes)
                        {
                            if (scenedef.Name == scenename)
                            {
                                founddef = scenedef;
                                break;
                            }
                        }
                        if (founddef != null && _MainVideoLoaded)
                        {
                            _mainScenePlayer.PlayScene(founddef);
                        }
                    }
                };
                lstScene.SelectionChanged += lstSceneChanged;

            };

            Unloaded += (s, e) =>
            {
                VideoView.MediaPlayer = null;
                //_supportingPlayer.Dispose();
                VideoInfo.MediaPlayer = null;

                _mediaPlayerMain.Dispose();


                this._libVLCMain.Dispose();
                //this._libVLCInfo.Dispose(); // Kept separate for the UWP app that needs 2 of them.
            };

            // Window size changes!
            SizeChanged += (s, e) => WindowResized(s, e);

            // When you maximize the window..    trigger reize also.
            this.StateChanged += (s, e) =>
            {
                if (this.WindowState == WindowState.Maximized)
                {
                    WindowResized(s, null);
                }
            };


            // You clicked New game!
            btnNewGame.Click += (s, e) =>
            {
                string FileCheckResult = Utilities.CheckForOriginalMedia();
                if (!string.IsNullOrEmpty(FileCheckResult))
                {
                    // Display message.
                    txtVideoErrorText.Text = FileCheckResult;
                    VideoErrorDialog.Visibility = Visibility.Visible;
                    return;
                }
                VideoErrorDialog.Visibility = Visibility.Collapsed;
                PrepPlayer();
                _mainScenePlayer.TheSupportingPlayer = _supportingPlayer;
                WindowResized(this, null);
                _mainScenePlayer.PlayScene(_scenes[0]);
                TricorderAnimation.CloseTricorder();
                _supportingPlayer.SceneComplete += _supportingPlayer_SceneComplete;
                //CurEmulator.Visibility = Visibility.Visible;


            };

            // You clicked OK on the video not found error message
            btnVideoFileMissingOKCancel.Click += (s, e) =>
            {
                VideoErrorDialog.Visibility = Visibility.Collapsed;
                _mcurVisible = false;
                CurEmulator.Source = CubeCursor;

                CurEmulator.Visibility = Visibility.Collapsed;

            };
            // You clicked OK on the error message.  This is an Unhandled Error!   Baaaaad.   So try to autosave and quit.!
            btnGenericcErrorOKCancel.Click += async (s, e) =>
            {
                try
                {
                    var savedata = _mainScenePlayer.GetSaveInfo();
                    DateTime now = DateTime.Now;
                    string SaveName = string.Format("AutoSave_{0}{1}{2}{3}{4}{5}", now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);
                    SaveDefinition info = _mainScenePlayer.GetSaveInfo();

                    info.SaveName = SaveName;

                    //List<SaveDefinition> saves = new List<SaveDefinition>();// await SaveLoader.LoadSavesFromAsset("RIVER.TXT");
                    //saves.Add(info);
                    //await SaveLoader.SaveSavesToAsset(saves, "RIVER.TXT");

                }
                catch
                {
                    // Whoops  can't do anything about it.
                }
                Close();
            };
            // Debug when you play a computer video scene
            lstComputer.SelectionChanged += (s, e) =>
            {

                var ClickedItems = e.AddedItems;
                foreach (var item in ClickedItems)
                {
                    ComboBoxItem citem = item as ComboBoxItem;
                    string scenename = citem.Content.ToString();
                    SceneDefinition founddef = null;
                    foreach (var scenedef in _computerScenes)
                    {
                        if (scenedef.Name == scenename)
                        {
                            founddef = scenedef;
                            break;
                        }
                    }
                    if (founddef != null && _MainVideoLoaded)
                    {
                        _supportingPlayer.DebugSetEvents(false);
                        _supportingPlayer.QueueScene(founddef, "computer"); // I'm treating these like info regardless of the actual type so it doesn't affect the main video when testing.
                    }
                }
            };
            // Window says mouse has moved.
            this.MouseMove += (s, e) =>
        {
            Mouse_Moved();

        };
            // You picked one of your saved games!   Enable the load button!
            lstRiver.SelectionChanged += (s, e) =>
            {
                if (lstRiver.SelectedIndex >= 0)
                {
                    btnLoad.IsEnabled = true;
                }
                else
                {
                    btnLoad.IsEnabled = false;
                }
            };
            // You cancelled your game load!  Make up your mind!

            btnLoadCancel.Click += (s, e) =>
            {
                LoadDialog.Visibility = Visibility.Collapsed;
            };

            // You're quitting the game!   Fill out my survey.  Like..  Follow..  Subscribe!
            btnQuitGame.Click += (s, e) =>
            {
                VideoView.Width = VideoViewGrid.ActualWidth;
                VideoView.Height = VideoViewGrid.ActualHeight;
                Quit();
            };
            // Create the Spinning klingon logo cursor to show the user when the game is paused.
            TricorderCursor = new BitmapImage();
            TricorderCursor.BeginInit();
            TricorderCursor.UriSource = new Uri(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "Assets", "TricorderHolodeckCur.gif"));
            TricorderCursor.EndInit();

            


            // Create the Klingon knife cursor for the action scenes where we demand the user do something!
            CubeCursor = new BitmapImage();
            CubeCursor.BeginInit();

            CubeCursor.UriSource = new Uri(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "Assets", "BorgCubeCursor.gif"));// new BitmapImage(new Uri(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "Assets", "dktahg.gif")));
            CubeCursor.EndInit();
            // You clicked the clickable surface!   Start a timer..  to see if you only single clickd or double clicked.  
            // If the timer fires..  you have single clickddd.  If it doesn't fire you have double clickdd.


            // You have cancelled the save dialog.
            btnSaveCancel.Click += (s, e) =>
            {
                if (!VideoView.MediaPlayer.IsPlaying)
                    VideoView.MediaPlayer.Play();
                SaveDialog.Visibility = Visibility.Collapsed;

                _mcurVisible = false;
                CurEmulator.Source = CubeCursor;
                AnimationBehavior.SetSourceUri(CurEmulator, CubeCursor.UriSource);
                CurEmulator.Visibility = Visibility.Collapsed;
            };

            // You clicked the save button!

            // Get the user game state from the player and save it to the save file!
            btnSave.Click += async (s, e) =>
            {
                string SaveName = txtSaveName.Text;

                if (string.IsNullOrEmpty(SaveName))
                {
                    txtSaveErrorText.Text = "Please type a Name in the box.";
                    txtSaveErrorText.Visibility = Visibility.Visible;
                    return;
                }

                if (Utilities.ValidateSaveGameName(SaveName))
                {
                    SaveDefinition info = _mainScenePlayer.GetSaveInfo();

                    info.SaveName = SaveName;

                    //List<SaveDefinition> saves = await SaveLoader.LoadSavesFromAsset("RIVER.TXT");
                    //saves.Add(info);
                    //await SaveLoader.SaveSavesToAsset(saves, "RIVER.TXT");

                    if (!VideoView.MediaPlayer.IsPlaying)
                        VideoView.MediaPlayer.Play();
                    SaveDialog.Visibility = Visibility.Collapsed;

                    _mcurVisible = false;

                    CurEmulator.Source = CubeCursor;
                    AnimationBehavior.SetSourceUri(CurEmulator, CubeCursor.UriSource);

                    CurEmulator.Visibility = Visibility.Collapsed;
                    txtSaveErrorText.Visibility = Visibility.Collapsed;
                    txtSaveErrorText.Text = "";
                    return;

                }
                txtSaveErrorText.Text = "Please remove this dishonorable text you Ferengi Ha'DIbaH!";
                txtSaveErrorText.Visibility = Visibility.Visible;


            };
            ClickSurface.Click += (o, cEventArgs) =>
            {
                Point tappedspot; 
                if (!TricorderOpen) 
                    tappedspot = Mouse.GetPosition(ClickSurface);
                else 
                    tappedspot = Mouse.GetPosition(InfoClickSurface);

                tappedspot = new Point(tappedspot.X, tappedspot.Y - _titlebarcurrentsize);// Counter for titlebar.
                _lastClickPoint = tappedspot;

                lock (_clickTimer)
                {
                    _clickTimer.Stop();
                    _clickTimer.Start();
                }

            };
            InfoClickSurface.Click += (o, cEventArgs) =>
            {
                var tappedspot = Mouse.GetPosition(InfoClickSurface);
                tappedspot = new Point(tappedspot.X, tappedspot.Y - _titlebarcurrentsize);// Counter for titlebar.
                _lastClickPoint = tappedspot;

                lock (_clickTimer)
                {
                    _clickTimer.Stop();
                    _clickTimer.Start();
                }

            };
            // You have double clicked!   Huraah.  This one is easy.
            ClickSurface.MouseDoubleClick += (o, cEventArgs) =>
            {
                if (!_actionTime) // No pausing during active time.  It is too difficult to separate single and double clicks during some scenes that you need to rapid click.
                {

                    SwitchGameModeActiveInfo();

                    cEventArgs.Handled = true;

                    lock (_clickTimer)
                        _clickTimer.Stop();
                }

            };
            // You have double clicked!   Huraah.  This one is easy.
            InfoClickSurface.MouseDoubleClick += (o, cEventArgs) =>
            {
                if (!_actionTime) // No pausing during active time.  It is too difficult to separate single and double clicks during some scenes that you need to rapid click.
                {

                    SwitchGameModeActiveInfo();

                    cEventArgs.Handled = true;

                    lock (_clickTimer)
                        _clickTimer.Stop();
                }

            };
            ClickSurface.KeyUp += (o4, s5) =>
            {
                Keyup(o4, s5);
            };
            ClickSurface.KeyDown += (o4, s5) =>
            {
                Keydown(o4, s5);
            };
            InfoClickSurface.KeyUp += (o4, s5) =>
            {
                Keyup(o4, s5);
            };
            InfoClickSurface.KeyDown += (o4, s5) =>
            {
                Keydown(o4, s5);
            };

            // All the mouse move event relays!
            // Everything has to have a mouse move event otherwise when the mouse is over
            // that thing that doesn't..  the Cursor Emulator won't move there.
            ClickSurface.MouseMove += (o, cEventArgs) =>
            {
                Mouse_Moved();
            };
            InfoClickSurface.MouseMove += (o, cEventArgs) =>
            {
                Mouse_Moved();
            };
            CurEmulator.MouseMove += (o, cEventArgs) =>
            {
                Mouse_Moved();
            };
            txtSaveText.MouseMove += (o, cEventArgs) =>
            {
                Mouse_Moved();
            };
            txtSaveName.MouseMove += (o, cEventArgs) =>
            {
                Mouse_Moved();
            };
            txtSaveErrorText.MouseMove += (o, cEventArgs) =>
            {
                Mouse_Moved();
            };
            txtOffsetMs.MouseMove += (o, cEventArgs) =>
        {
            Mouse_Moved();
        };
            txtMS.MouseMove += (o, cEventArgs) =>
            {
                Mouse_Moved();
            };
            txtLoadText.MouseMove += (o, cEventArgs) =>
            {
                Mouse_Moved();
            };
            lstScene.MouseMove += (o, cEventArgs) =>
        {
            Mouse_Moved();
        };
            SaveDialog.MouseMove += (o, cEventArgs) =>
            {
                Mouse_Moved();
            };
            LoadDialog.MouseMove += (o, cEventArgs) =>
            {
                Mouse_Moved();
            };
            GenericErrorDialog.MouseMove += (o, cEventArgs) =>
            {
                Mouse_Moved();
            };
            VideoErrorDialog.MouseMove += (o, cEventArgs) =>
            {
                Mouse_Moved();
            };
            btnGenericcErrorOKCancel.MouseMove += (o, cEventArgs) =>
            {
                Mouse_Moved();
            };
            btnVideoFileMissingOKCancel.MouseMove += (o, cEventArgs) =>
            {
                Mouse_Moved();
            };
            


        }

        private void _supportingPlayer_SceneComplete(object obj, string scenename, string scenetype)
        {
            if (scenetype == "info")
            {
                // Special cases for hotspots from the supporting player.
                switch (scenename)
                {
                    case "ExitButton":
                        if (TricorderOpen)
                        {
                            _supportingPlayer.Pause();
                            _supportingPlayer.ClearQueue();
                            //VideoInfo.ReleaseMouseCapture();
                            TricorderOpen = false;

                            VideoInfo.Visibility = Visibility.Collapsed;
                            TricorderAnimation.CloseTricorder();
                        }
                        break;
                }
            }
            var hum = _holodeckScenes[0];
            _videoAudioPlayer.QueueScene(hum, "holodeck", 0, true);
        }

        private void Mouse_Moved()
        {

            var point = Mouse.GetPosition(ClickSurface);

            // If the mouse is close to the top of the window, show the title bar.  If it is far away..  hide the title bar.
            // Also..  because the window reisizes..  fix the math for the click translation.
            if (point.Y < _titlebarShowHeight && WindowStyle == WindowStyle.None)
            {
                _titlebarcurrentsize = _titlebarsize;
                WindowStyle = WindowStyle.SingleBorderWindow;
                WindowResized(this, null);
            }
            if (point.Y > _titlebarShowHeight && WindowStyle == WindowStyle.SingleBorderWindow)
            {
                WindowStyle = WindowStyle.None;
                _titlebarcurrentsize = _titlebarinvisibleheight;
                WindowResized(this, null);
            }
            // Don't move the cursor emulator if it isn't visible.

            //if (Debug is active, we should show mouse)
            if ((txtMS.Visibility == Visibility.Visible && txtOffsetMs.Visibility == Visibility.Visible) && !_mcurVisible)
            {
                _mcurVisible = true;
                CurEmulator.Visibility = Visibility.Visible;
            }
            if (!_mcurVisible)
                return;
            CurEmulator.Margin = new Thickness(point.X, point.Y + 1, 0, 0);
        }


        // We have resized the window.  Adjust all the maths!
        private void WindowResized(object o, SizeChangedEventArgs e)
        {
            
            WindowResized(e);
        }

        private void WindowResized( SizeChangedEventArgs e)
        {
            var width = this.ActualWidth;
            var height = this.ActualHeight;

            if (e != null)
            {
                width = e.NewSize.Width;
                height = e.NewSize.Height;
            }

            width = width - 15;
            height = height - _titlebarcurrentsize;
            VideoViewGrid.Width = width;
            VideoViewGrid.Height = height;

            //double widthby2 = ((double)width * 0.5d);
            //double widthby3 = ((double)width * 0.3333d);
            //double widthby4 = ((double)width * 0.25d);
            //double heightby3 = ((double)height * 0.3333d);
            //double heightby4 = ((double)height * 0.25d);
            //double infowidthby2 = widthby3 * 0.5d;
            //double widthby23 = ((double)width * 0.48d);
            //double leftmargin = widthby2 - infowidthby2;
            //double topmargin = heightby4 * 3d;



            //VideoInfo.Width = width * 0.24d;
            //VideoInfo.Height = heightby3;

            //InfoSpring.Width = widthby23;
            //InfoSpring.Height = height* 0.56d ;



            //VideoInfo.HorizontalAlignment = HorizontalAlignment.Center;
            //VideoInfo.VerticalAlignment = VerticalAlignment.Bottom;
            //VideoInfo.Margin = VideoInfoMargin;
            ////VideoInfo.MediaPlayer.Scale = 0;
            //InfoSpring.HorizontalAlignment = HorizontalAlignment.Center;
            //InfoSpring.VerticalAlignment = VerticalAlignment.Bottom;
            //InfoSpring.Margin = TricorderFrameMargin;
            //VideoInfo.HorizontalAlignment = HorizontalAlignment.Center;
            //VideoInfo.VerticalAlignment = VerticalAlignment.Bottom;
            //VideoInfo.Height = 426.29069999999996;
            //VideoInfo.Width = 534.25;
            //Thickness VideoInfoMargin = new Thickness(0, 0, 0, 138);
            //Thickness TricorderFrameMargin = new Thickness(250, 0, 0, 0);
            //VideoInfo.Margin = VideoInfoMargin;
            //InfoSpring.Margin = TricorderFrameMargin;
            ////VideoInfo.Background = Colors.Black;

            //InfoSpring.Height = 939.3004;
            //InfoSpring.Width = 1057.25;
            //InfoSpring.HorizontalAlignment = HorizontalAlignment.Center;
            //InfoSpring.VerticalAlignment = VerticalAlignment.Bottom;
            //InfoSpring.Stretch = Stretch.Fill;


            if (_MainVideoLoaded)
            {
                InfoClickSurface.Width = VideoInfo.ActualWidth;
                InfoClickSurface.Height = VideoInfo.ActualHeight;

                ClickSurface.Width = width;
                ClickSurface.Height = height;
                if (_mainScenePlayer != null)
                {
                    var aspectquery = Utilities.GetMax(width, height, _OriginalAspectRatio);
                    switch (aspectquery.Direction)
                    {
                        case "W":
                            _mainScenePlayer.HotspotScale = (float)(width / _OriginalMainVideoWidth);
                            break;
                        case "H":
                            _mainScenePlayer.HotspotScale = (float)(height / _OriginalMainVideoHeight);
                            break;
                    }

                }
                VideoView.Width = width;
                VideoView.Height = height;
            }
            ImgStartMain.Height = height;
            ImgStartMain.Width = width;
            grdStartControls.Height = height;
            grdStartControls.Width = width;

        }
        private void Keydown(object o, KeyEventArgs ea)
        {
            if (_MainVideoLoaded)
            {
                switch (ea.Key)
                {
                    case Key.H:
                        tbHelpText.Visibility = Visibility.Visible;
                        break;
                }
            }
        }
        private void Keyup(object o, KeyEventArgs ea)
        {
            if (_MainVideoLoaded)
            {
                // OemPlus
                // OemMinus
                // Add
                // Subtract
                switch (ea.Key)
                {
                    case Key.Q:
                        Quit();
                        return;
                    case Key.S:
                        if (SaveDialog.Visibility == Visibility.Collapsed)
                        {

                            if (VideoView.MediaPlayer.IsPlaying)
                                VideoView.MediaPlayer.Pause();
                            SaveDialog.Visibility = Visibility.Visible;
                            //var pointerPosition = Windows.UI.Core.CoreWindow.GetForCurrentThread().PointerPosition;
                            //var pos = Window.Current.CoreWindow.PointerPosition;
                            //CurEmulator.Margin = new Thickness(pos.X - Window.Current.Bounds.X, pos.Y - Window.Current.Bounds.Y + 1, 0, 0);
                            _mcurVisible = true;
                            CurEmulator.Source = CubeCursor;
                            AnimationBehavior.SetSourceUri(CurEmulator, CubeCursor.UriSource);
                            CurEmulator.Visibility = Visibility.Visible;
                        }
                        return;
                    case Key.N:
                        if (txtMS.Visibility == Visibility.Visible && txtOffsetMs.Visibility == Visibility.Visible) // Only if debug is visible
                        {
                            VideoView.MediaPlayer.Time -= 15000;
                        }
                        break;
                    case Key.J:
                        if (txtMS.Visibility == Visibility.Visible && txtOffsetMs.Visibility == Visibility.Visible) // Only if debug is visible
                        {
                            Window w = new UtilitiesWindow();
                            w.Show();
                        }
                        break;
                    case Key.M:
                        if (txtMS.Visibility == Visibility.Visible && txtOffsetMs.Visibility == Visibility.Visible) // Only if debug is visible
                        {
                            VideoView.MediaPlayer.Time += 15000;
                        }
                        break;

                    case Key.C:
                        if (txtMS.Visibility == Visibility.Visible && txtOffsetMs.Visibility == Visibility.Visible) // Only if debug is visible
                        {
                            if (_mainScenePlayer != null)
                            {
                                _mainScenePlayer.JumpToChallenge();
                            }
                        }
                        break;
                    case Key.A:
                        SwapDisplay = !SwapDisplay;
                        if (SwapDisplay)
                        {
                            VideoView.MediaPlayer.Scale = 0;
                            VideoView.MediaPlayer.AspectRatio = "4:3";
                        }
                        else
                        {
                            VideoView.MediaPlayer.Scale = 0;
                            VideoView.MediaPlayer.AspectRatio = "16:9";
                        }
                        break;
                    case Key.Enter:
                        if (txtMS.Visibility == Visibility.Visible && txtOffsetMs.Visibility == Visibility.Visible) // Only if debug is visible
                        {
                            float offsetint = 0;
                            if (txtOffsetMs.Text.Length > 0)
                                offsetint = Convert.ToSingle(txtOffsetMs.Text);
                            if (txtMS.Text.Length > 0)
                            {
                                //VideoView.MediaPlayer.Time = (long)(((Convert.ToSingle(txtMS.Text)-2)* 98.14f) + (offsetint));
                                VideoView.MediaPlayer.Time = (long)(Utilities.Frames15fpsToMS(Convert.ToInt32(txtMS.Text) - 2) + (offsetint * 100));

                            }
                        }
                        break;
                    case Key.Oem3: // Accento Debug
                        if (txtMS.Visibility == Visibility.Visible && txtOffsetMs.Visibility == Visibility.Visible)
                        {
                            txtMS.Visibility = Visibility.Collapsed;
                            txtOffsetMs.Visibility = Visibility.Collapsed;
                            lstScene.Visibility = Visibility.Collapsed;
                            lstComputer.Visibility = Visibility.Collapsed;
                            CurEmulator.Visibility = Visibility.Collapsed;
                            tbDebugTextBlock.Visibility = Visibility.Collapsed;
                            _mainScenePlayer.VisualizeRemoveHotspots();
                            if (_clickRectangle != null) _clickRectangle.Visibility = Visibility.Collapsed;
                            _mcurVisible = false;
                        }
                        else
                        {
                            txtMS.Visibility = Visibility.Visible;
                            txtOffsetMs.Visibility = Visibility.Visible;
                            lstScene.Visibility = Visibility.Visible;
                            lstComputer.Visibility = Visibility.Visible;
                            CurEmulator.Visibility = Visibility.Visible;
                            tbDebugTextBlock.Visibility = Visibility.Visible;
                            if (_clickRectangle != null) _clickRectangle.Visibility = Visibility.Visible;
                            // Unhook the scene changed event because we don't want it to restart the scene
                            lstScene.SelectionChanged -= lstSceneChanged;
                            var sp = _mainScenePlayer.ScenePlaying;
                            for (int i = 0; i < lstScene.Items.Count; i++)
                            {
                                ComboBoxItem item = lstScene.Items[i] as ComboBoxItem;
                                if (item.Content.ToString() == sp)
                                {
                                    lstScene.SelectedIndex = i;
                                    break;
                                }
                            }
                            lstScene.SelectionChanged += lstSceneChanged;
                            //var pointerPosition = Windows.UI.Core.CoreWindow.GetForCurrentThread().PointerPosition;

                            //var pos = this.PointFromScreen;// PointerPosition;
                            //CurEmulator.Margin = new Thickness(pos.X - Window.Current.Bounds.X, pos.Y - Window.Current.Bounds.Y + 1, 0, 0);
                            ShowCursor();
                            _mainScenePlayer.VisualizeHotspots(VVGrid);// VideoViewGrid);
                            _mcurVisible = true;
                        }
                        break;
                    case Key.Space:
                        SwitchGameModeActiveInfo();
                        break;
                    case Key.P:
                    case Key.Pause:
                    case Key.Play:
                    case Key.MediaPlayPause:
                        SwitchGameModeActiveInfo(ea.Key);
                        break;
                    // OemPlus
                    // OemMinus
                    // Add
                    // Subtract
                    case Key.OemPlus:
                    case Key.Add:
                        _mainScenePlayer.IncreaseVolume();
                        _supportingPlayer.IncreaseVolume();
                        _videoAudioPlayer.IncreaseVolume();
                        break;
                    case Key.OemMinus:
                    case Key.Subtract:
                        _mainScenePlayer.LowerVolume();
                        _supportingPlayer.LowerVolume();
                        _videoAudioPlayer.LowerVolume();
                        break;
                    case Key.H:
                        tbHelpText.Visibility = Visibility.Collapsed;
                        break;
                }
            }
        }
        private void SwitchGameModeActiveInfo(Key k = Key.None)
        {
            if (!_actionTime || (k == Key.MediaPlayPause && (txtMS.Visibility == Visibility.Visible && txtOffsetMs.Visibility == Visibility.Visible))) // No pausing during active time.  It is too difficult to separate single and double clicks during some scenes that you need to rapid click.
            {
                if (_MainVideoLoaded)
                {
                    // If the Save or load dialog is visible, we want them resuming the video with double click
                    if (SaveDialog.Visibility == Visibility.Collapsed && LoadDialog.Visibility == Visibility.Collapsed)
                    {
                        if (VideoView.MediaPlayer.IsPlaying)
                        {
                            VideoView.MediaPlayer.Pause();
                            //TricorderAnimation.OpenTricorder();
                            CurEmulator.Visibility = Visibility.Visible;
                            
                            CurEmulator.Source = TricorderCursor;
                            AnimationBehavior.SetSourceUri(CurEmulator, TricorderCursor.UriSource);

                            //var beep = _holodeckScenes[0];
                            var hum = _holodeckScenes[0];
                            //_supportingPlayer.QueueScene(beep, "holodeck");
                            //_supportingPlayer.QueueScene(hum, "holodeck", 0, true);
                            _videoAudioPlayer.QueueScene(hum, "holodeck", 0, true);

                            //var pointerPosition = Windows.UI.Core.CoreWindow.GetForCurrentThread().PointerPosition;
                            //var pos = Window.Current.CoreWindow.PointerPosition;
                            //CurEmulator.Margin = new Thickness(pos.X - Window.Current.Bounds.X, pos.Y - Window.Current.Bounds.Y + 1, 0, 0);
                            var point = Mouse.GetPosition(ClickSurface);
                            CurEmulator.Margin = new Thickness(point.X, point.Y + 1, 0, 0);
                            _mcurVisible = true;
                        }
                        else
                        {
                            if (TricorderOpen)
                            {
                                _supportingPlayer.Pause();
                                _supportingPlayer.ClearQueue();
                                //VideoInfo.ReleaseMouseCapture();
                                TricorderOpen = false;

                                VideoInfo.Visibility = Visibility.Collapsed;
                                TricorderAnimation.CloseTricorder();
                            }
                            VideoView.MediaPlayer.Play();
                            
                            CurEmulator.Source = CubeCursor;
                            AnimationBehavior.SetSourceUri(CurEmulator, CubeCursor.UriSource);
                            _videoAudioPlayer.Pause();
                            _videoAudioPlayer.ClearQueue();
                            CurEmulator.Visibility = Visibility.Collapsed;
                            _mcurVisible = false;
                        }
                    }
                }
            }
        }

        private void Quit()
        {
            Close();
            return;
            string FileCheckResult = Utilities.CheckForOriginalMedia();
            if (!string.IsNullOrEmpty(FileCheckResult))
            {
                // Display message.
                Close();
            }

            SceneDefinition scene = null;
            if (_scenes != null && _scenes.Count == 0)
            {
                PrepPlayer();
            }
            if (_scenes == null)
            {
                PrepPlayer();
            }
            if (_scenes.Count == 0)
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                Close();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
            else
            {
                scene = _scenes[0];
                if (scene != null)
                {
                    _mainScenePlayer.PlayScene(scene);
                }
                scene = _scenes.Where(xy => xy.Name == "LOGO1").FirstOrDefault();
                if (scene != null)
                {
                    _mainScenePlayer.PlayScene(scene);
                }
            }
        }

        private void PrepPlayer()
        {
            btnNewGame.IsEnabled = false;
            grdStartControls.Visibility = Visibility.Collapsed;

            _scenes = SceneLoader.LoadScenesFromAsset("scenes.txt");
            _hotspots = HotspotLoader.LoadHotspotsFromAsset("hotspots.txt");
            _infohotspots = HotspotLoader.LoadHotspotsFromAsset("ips.txt");
            _infoScenes = _scenes.Where(xy => xy.SceneType == SceneType.Info).ToList();

            for (int i = 0; i < _infohotspots.Count; i++)
            {
                _infohotspots[i].FrameStart -= (int)Utilities.MsTo15fpsFrames(6000);
                _infohotspots[i].FrameEnd += (int)Utilities.MsTo15fpsFrames(2000);
            }


            for (int i = 0; i < _infoScenes.Count; i++)
            {
                _infoScenes[i].FrameStart += 2;
                _infoScenes[i].FrameEnd -= 20;
            }

            for (int i = 0; i < _hotspots.Count; i++)
            {
                var hotspot = _hotspots[i];
                foreach (var scene in _scenes)
                {
                    if (hotspot.RelativeVideoName.ToLowerInvariant() == scene.Name.ToLowerInvariant())
                    {
                        if (hotspot.ActionVideo.ToLowerInvariant().StartsWith("i_") || (hotspot.Name.ToLowerInvariant().StartsWith("ip_") && !hotspot.ActionVideo.ToLowerInvariant().StartsWith("d")))
                        {
                            scene.PausedHotspots.Add(hotspot);
                        }
                        else
                        {
                            scene.PlayingHotspots.Add(hotspot);
                        }

                    }
                }
            }
           
            for (int i = 0; i < _infohotspots.Count; i++)
            {
                var hotspot = _infohotspots[i];

                foreach (var scene in _scenes)
                {
                    if (hotspot.RelativeVideoName.ToLowerInvariant() == scene.Name.ToLowerInvariant())
                    {
                        scene.PausedHotspots.Add(hotspot);
                    }
                }
            }
            Load_Scene_List(_scenes);
            _mainScenePlayer = new ScenePlayer(VideoView, _scenes);
            Load_Main_Video();

            ImgStartMain.Visibility = Visibility.Collapsed;
            Mouse.OverrideCursor = Cursors.None;

            _mainScenePlayer.VisualizationHeightMultiplier = _OriginalMainVideoHeight / _HotspotOriginalMainVideoHeight;
            _mainScenePlayer.VisualizationWidthMultiplier = _OriginalMainVideoWidth / _HotspotOriginalMainVideoWidth;
            _mainScenePlayer.innerGrid = VVGrid;
            _mainScenePlayer.ActionOn += () =>
            {
                _actionTime = true;
                _clickTimer.Interval = TimeSpan.FromSeconds(0.05);
                ShowCursor();
            };
            _mainScenePlayer.ActionOff += () =>
            {
                _actionTime = false;
                _clickTimer.Interval = TimeSpan.FromSeconds(0.2);
                HideCursor();

            };
            _mainScenePlayer.QuitGame += () =>
            {

                Close();
            };
            _mainScenePlayer.InfoVideoTrigger += InfoVideoTriggerShowFrame;

            Uri uri = new Uri(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "Assets", "QTricorderT20frame.gif"));
            AnimationBehavior.SetSourceUri(InfoSpring, uri);
            TricorderAnimation = new TricorderGifAnimationController(InfoSpring);
            TricorderAnimation.CloseTricorder();
            TricorderAnimation.OnTricorderOpen += InfoVideoPlayTimeSpan;
            AnimationBehavior.SetSourceUri(CurEmulator, CubeCursor.UriSource);
            InfoSpring.Visibility = Visibility.Collapsed;
            VideoInfo.Visibility = Visibility.Collapsed;
            WindowResized(null);
            ClickSurface.Focus();
        }


        private void InfoVideoTriggerShowFrame(long start, long end)
        {
            VideoInfo.MediaPlayer.Scale = 2;
            InfoSpring.Visibility = Visibility.Visible;
            if (!TricorderOpen)
            {
                TricorderOpen = true;
                TricorderAnimation.OpenTricorder(start, end);
                
                //VideoInfo.CaptureMouse();
            }
            
        }

        private void InfoVideoPlayTimeSpan(long start, long end)
        {
            if (start <= 0 || end <= 0)
                return;
            VideoInfo.Visibility = Visibility.Visible;
            SceneDefinition InfoSceneToPlay = _infoScenes.Where(xy => xy.StartMS >= start && xy.EndMS <= end).FirstOrDefault();
            // todo write a way to find the scene by start and end.

            if (InfoSceneToPlay != null)
            {
                var hum = _holodeckScenes[0];
                _videoAudioPlayer.Pause();
                _videoAudioPlayer.ClearQueue();
                _supportingPlayer.QueueScene(InfoSceneToPlay, "info", 0);

            }
        }

        private void ShowCursor()
        {
            CurEmulator.Visibility = Visibility.Visible;
            //var pointerPosition = Windows.UI.Core.CoreWindow.GetForCurrentThread().PointerPosition;

            //var pos = this.Window.Current.CoreWindow.PointerPosition;
            //CurEmulator.Margin = new Thickness(pos.X - this.Width, pos.Y - this.Height + 1, 0, 0);
            var point = Mouse.GetPosition(ClickSurface);
            CurEmulator.Margin = new Thickness(point.X, point.Y + 1, 0, 0);
            _mcurVisible = true;
        }
        private void HideCursor()
        {
            CurEmulator.Visibility = Visibility.Collapsed;

            _mcurVisible = false;
        }

        private void Load_Main_Video()
        {
            var result = _mainScenePlayer.Load_Main_Video(_libVLCMain);
            _OriginalMainVideoHeight = result.OriginalMainVideoHeight;
            _OriginalMainVideoWidth = result.OriginalMainVideoWidth;
            
            _MainVideoLoaded = result.Loaded;

        }
        private void Load_Scene_List(List<SceneDefinition> defs)
        {
            lstScene.Items.Clear();

            foreach (var def in defs)
            {
                if (def.SceneType == SceneType.Main || def.SceneType == SceneType.Inaction || def.SceneType == SceneType.Bad)
                    lstScene.Items.Add(new ComboBoxItem() { Content = def.Name });
            }
        }
        private void Log_Fired(object sender, LogEventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine(e.FormattedLog);
        }
    }
}
