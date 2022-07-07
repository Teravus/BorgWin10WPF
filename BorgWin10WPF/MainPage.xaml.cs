using System;
using System.Collections.Generic;
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

        // So..  if the game is expecting input from the user...  Don't allow them to pause the game.
        private bool _actionTime = false;

        // Delegate that defines what happens when the debug combobox with the main scenes is changed.
        // We keep a reference here because we hook and unhook from this event.
        private SelectionChangedEventHandler lstSceneChanged = null;

        // This is the tricorder cursor for when the game is paused.
        BitmapImage TricorderCursor = null;  //new BitmapImage(new Uri(Path.Combine("Assets", "KlingonHolodeckCur.gif"), UriKind.Relative));

        // This is the borg cube when the game says User do something.
        BitmapImage CubeCursor = null; // new BitmapImage(new Uri(Path.Combine("Assets", "dktahg.gif"), UriKind.Relative));

        // We have two VideoViews on the form.   The loading order isn't guaranteed.   So..   we keep track of if we have initialized libVLC with this
        bool _coreVLCInitialized = false;


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

                long time = 0;
                TimeSpan ts = TimeSpan.Zero;
                System.Diagnostics.Debug.WriteLine("{0},{1}({2},{3})[{4}] - t{5} - f{6}", _lastClickPoint.X, _lastClickPoint.Y, relclickX, relclickY, ts.ToString(@"hh\:mm\:ss"), (long)((float)time), Utilities.MsTo15fpsFrames(time));
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

                        // Video Player Loaded
                        //if (_mainScenePlayer != null)
                        //{
                        //    txtGenericErrorText.Text = unhandledException.Message;
                        //    GenericErrorDialog.Visibility = Visibility.Visible;
                        //    _mcurVisible = true;
                        //    CurEmulator.Source = dktahgCursor;
                        //    AnimationBehavior.SetSourceUri(CurEmulator, dktahgCursor.UriSource);
                        //    CurEmulator.Visibility = Visibility.Visible;
                        //    err.Handled = true;
                        //    return;
                        //}

                        // Video player not loaded.
                        //VideoErrorDialog.Visibility = Visibility.Visible;
                        //txtVideoErrorText.Text = unhandledException.Message;
                        _mcurVisible = true;
                        CurEmulator.Source = CubeCursor;
                        //AnimationBehavior.SetSourceUri(CurEmulator, dktahgCursor.UriSource);
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

                            //SwitchGameModeActiveInfo();

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

                //var _mediaPlayerInfo = new LibVLCSharp.Shared.MediaPlayer(_libVLCInfo);
                //VideoInfo.MediaPlayer = _mediaPlayerInfo;
                //_mediaPlayerInfo.EnableMouseInput = false;
                //_mediaPlayerInfo.EnableKeyInput = false;

                //// Uncomment this and the line in log_fired to lag the game..   and..  get the reason why libVLC is not happy.
                ////_libVLCInfo.Log += Log_Fired;

                //var InfoScenes = _scenes.Where(xy => xy.SceneType == SceneType.Info).ToList();
                //_infoScenes = InfoScenes;

                //var ComputerScenes = SceneLoader.LoadSupportingScenesFromAsset("computerscenes.txt");
                //_computerScenes = ComputerScenes;
                //var HolodeckScenes = SceneLoader.LoadSupportingScenesFromAsset("holodeckscenes.txt");
                //_holodeckScenes = HolodeckScenes;
                //_supportingPlayer = new SupportingPlayer(VideoInfo, InfoScenes, ComputerScenes, HolodeckScenes, _libVLCInfo);
                //Load_Computer_list(_computerScenes);

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

            // Window says mouse has moved.
            this.MouseMove += (s, e) =>
            {
                Mouse_Moved();

            };

            // Create the Spinning klingon logo cursor to show the user when the game is paused.
            TricorderCursor = new BitmapImage();
            //TricorderCursor.BeginInit();
            //TricorderCursor.UriSource = new Uri(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "Assets", "KlingonHolodeckCur.gif"));
            //TricorderCursor.EndInit();

            //new BitmapImage(new Uri(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "Assets", "KlingonHolodeckCur.gif")));


            // Create the Klingon knife cursor for the action scenes where we demand the user do something!
            CubeCursor = new BitmapImage();
            //CubeCursor.BeginInit();
            //CubeCursor.UriSource = new Uri(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "Assets", "dktahg.gif"));// new BitmapImage(new Uri(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "Assets", "dktahg.gif")));
            //CubeCursor.EndInit();
            // You clicked the clickable surface!   Start a timer..  to see if you only single clickd or double clicked.  
            // If the timer fires..  you have single clickddd.  If it doesn't fire you have double clickdd.
            ClickSurface.Click += (o, cEventArgs) =>
            {
                var tappedspot = Mouse.GetPosition(ClickSurface);
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

                    //SwitchGameModeActiveInfo();

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

            // All the mouse move event relays!
            // Everything has to have a mouse move event otherwise when the mouse is over
            // that thing that doesn't..  the Cursor Emulator won't move there.
            ClickSurface.MouseMove += (o, cEventArgs) =>
            {
                Mouse_Moved();
            };
            CurEmulator.MouseMove += (o, cEventArgs) =>
            {
                Mouse_Moved();
            };
            //txtSaveText.MouseMove += (o, cEventArgs) =>
            //{
            //    Mouse_Moved();
            //};
            //txtSaveName.MouseMove += (o, cEventArgs) =>
            //{
            //    Mouse_Moved();
            //};
            //txtSaveErrorText.MouseMove += (o, cEventArgs) =>
            //{
            //    Mouse_Moved();
            //};
            txtOffsetMs.MouseMove += (o, cEventArgs) =>
            {
                Mouse_Moved();
            };
            txtMS.MouseMove += (o, cEventArgs) =>
            {
                Mouse_Moved();
            };
            //txtLoadText.MouseMove += (o, cEventArgs) =>
            //{
            //    Mouse_Moved();
            //};
            lstScene.MouseMove += (o, cEventArgs) =>
            {
                Mouse_Moved();
            };
            //SaveDialog.MouseMove += (o, cEventArgs) =>
            //{
            //    Mouse_Moved();
            //};
            //LoadDialog.MouseMove += (o, cEventArgs) =>
            //{
            //    Mouse_Moved();
            //};
            //GenericErrorDialog.MouseMove += (o, cEventArgs) =>
            //{
            //    Mouse_Moved();
            //};
            //VideoErrorDialog.MouseMove += (o, cEventArgs) =>
            //{
            //    Mouse_Moved();
            //};
            //btnGenericcErrorOKCancel.MouseMove += (o, cEventArgs) =>
            //{
            //    Mouse_Moved();
            //};
            //btnVideoFileMissingOKCancel.MouseMove += (o, cEventArgs) =>
            //{
            //    Mouse_Moved();
            //};



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
            if (!_mcurVisible)
                return;
            CurEmulator.Margin = new Thickness(point.X, point.Y + 1, 0, 0);
        }


        // We have resized the window.  Adjust all the maths!
        private void WindowResized(object o, SizeChangedEventArgs e)
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

            //if (_MainVideoLoaded)
            //{

            ClickSurface.Width = width;
            ClickSurface.Height = height;

            VideoView.Width = width;
            VideoView.Height = height;
            //}
            ImgStartMain.Height = height;
            ImgStartMain.Width = width;
            //grdStartControls.Height = height;
            //grdStartControls.Width = width;

        }
        private void Keydown(object o, KeyEventArgs ea)
        {
        }
        private void Keyup(object o, KeyEventArgs ea)
        {
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
    }
}
