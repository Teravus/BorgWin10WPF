using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media.Animation;
using Microsoft.Win32;
using XamlAnimatedGif;
using System.Diagnostics;

namespace BorgWin10WPF
{
    public class TricorderGifAnimationController : DependencyObject
    {
        private Animator _animator;
        public event PropertyChangedEventHandler PropertyChanged;
        private bool _autoStart = true;
        private RepeatBehavior _repeatBehavior;
        private bool _completed;
        private int _repeatCount = 3;
        private bool _useSpecificRepeatCount;
        private bool _repeatForever;
        private bool _useDefaultRepeatBehavior = true;
        private int _sldPosition = 0;
        private int _sldPositionMax = 0;
        private Stopwatch _stopwatch;
        private int _pauseFrame = 40;
        private bool _OpeningYN = true;
        private int _maxFrame = 50;
        

        private ObservableCollection<string> _images;
        private TimeSpan? _lastRunTime;
        private string _selectedImage;
        private DependencyObject BoundControl;

        public TricorderGifAnimationController(DependencyObject boundControl)
        {
            BoundControl = boundControl;
            AnimationBehavior.AddLoadedHandler(boundControl, AnimationBehavior_OnLoaded);
            AnimationBehavior.SetRepeatBehavior(boundControl, RepeatBehavior.Forever);

        }

        private bool _playing 
        { 
            get 
            {
                if (BoundControl == null)
                    return false;
                if (_animator == null)
                    return false;

                return !_animator.IsPaused;
            } 
            set 
            {
                if (BoundControl == null)
                    return;
                if (_animator == null)
                    return;
                OnPropertyChanged();
                if (value)
                {
                    _stopwatch?.Start();
                    _animator.Play();
                }
                else
                {
                    _stopwatch?.Stop();
                    _animator.Pause();
                }

                return; 
            } 
        }

        public void OpenTricorder()
        {
            if (BoundControl == null)
                return;
            if (_animator == null)
                return;

            _OpeningYN = true;
            //_animator.Rewind();
            _playing = true;

            //StartStopwatch();
        }
        public void CloseTricorder()
        {
            if (BoundControl == null)
                return;
            if (_animator == null)
                return;

            _OpeningYN = false;
            _playing = true;

        }
        private void AnimationBehavior_OnLoaded(object sender, RoutedEventArgs e)
        {
           

            if (_animator != null)
            {
                _animator.CurrentFrameChanged -= CurrentFrameChanged;
            }

            _animator = AnimationBehavior.GetAnimator(BoundControl);

            if (_animator != null)
            {
                _animator.CurrentFrameChanged += CurrentFrameChanged;
                _sldPosition = 0;
                _animator.Rewind();
                _sldPositionMax = _animator.FrameCount - 1;
                _playing = false;
                //SetPlayPauseEnabled(_animator.IsPaused || _animator.IsComplete);
            }
        }
        private void AnimationBehavior_OnAnimationStarted(DependencyObject d, AnimationStartedEventArgs e)
        {
            StartStopwatch();
        }

        private void AnimationBehavior_OnAnimationCompleted(DependencyObject sender, AnimationCompletedEventArgs e)
        {
            StopStopwatch();
            Completed = true;
            //if (_animator != null)
            //    SetPlayPauseEnabled(_animator.IsPaused || _animator.IsComplete);
        }
        private void AnimationBehavior_OnError(DependencyObject d, AnimationErrorEventArgs e)
        {
            MessageBox.Show($"An error occurred ({e.Kind}): {e.Exception}");
        }
   

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private RepeatBehavior RepeatBehavior
        {
            get => _repeatBehavior;
            set
            {
                _repeatBehavior = value;
                OnPropertyChanged();
                Completed = false;
            }
        }

        private bool AutoStart
        {
            get => _autoStart;
            set
            {
                _autoStart = value;
                OnPropertyChanged();
            }
        }

        private bool Completed
        {
            get => _completed;
            set
            {
                _completed = value;
                OnPropertyChanged();
            }
        }


        private int RepeatCount
        {
            get => _repeatCount;
            set
            {
                _repeatCount = value;
                OnPropertyChanged();
                if (UseSpecificRepeatCount)
                    RepeatBehavior = new RepeatBehavior(value);
            }
        }

        private bool UseSpecificRepeatCount
        {
            get => _useSpecificRepeatCount;
            set
            {
                _useSpecificRepeatCount = value;
                OnPropertyChanged();
                if (value)
                    RepeatBehavior = new RepeatBehavior(RepeatCount);
            }
        }


        private bool RepeatForever
        {
            get => _repeatForever;
            set
            {
                _repeatForever = value;
                OnPropertyChanged();
                if (value)
                    RepeatBehavior = RepeatBehavior.Forever;
            }
        }

        private bool UseDefaultRepeatBehavior
        {
            get => _useDefaultRepeatBehavior;
            set
            {
                _useDefaultRepeatBehavior = value;
                OnPropertyChanged();
                if (value)
                    RepeatBehavior = default;
            }
        }
        private void StartStopwatch()
        {
            if (_stopwatch == null)
                _stopwatch = new Stopwatch();
            
            _stopwatch.Restart();
        }
        private void PauseStopwatch() => _stopwatch.Stop();

        private void ResumeStopwatch() => _stopwatch?.Start();

        private void StopStopwatch()
        {
            _stopwatch?.Stop();
            LastRunTime = _stopwatch?.Elapsed;
        }

        private void ClearStopwatch()
        {
            _stopwatch?.Stop();
            LastRunTime = null;
        }

        private void CurrentFrameChanged(object sender, EventArgs e)
        {
            if (_animator != null)
            {
                _sldPosition = _animator.CurrentFrameIndex;
                if (_playing)
                {
                    if (_OpeningYN)
                    {
                        if (_sldPosition >= _pauseFrame)
                        {
                            _animator.Pause();
                            _playing = false;
                            _OpeningYN = false;
                        }
                    }
                    else
                    {
                        if (!_OpeningYN && _sldPosition == 0)
                        {
                            _OpeningYN = true;
                        }
                        if (_sldPosition >= _maxFrame-1)
                        {
                            _animator.Rewind();
                            _playing = false;
                            _OpeningYN = true;
                        }

                    }
                }
              

                _sldPosition = _animator.CurrentFrameIndex;
            }
        }
        public TimeSpan? LastRunTime
        {
            get => _lastRunTime;
            set
            {
                _lastRunTime = value;
                OnPropertyChanged(nameof(LastRunTime));
            }
        }

    }
}
