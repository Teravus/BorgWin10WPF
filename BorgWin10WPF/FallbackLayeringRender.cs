using LibVLCSharp.Shared;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Processing;
using System;
using System.Linq;
using System.Collections.Concurrent;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using LibVLCSharp.WPF;
using System.Buffers;
using System.Windows.Media.Imaging;

namespace BorgWin10WPF
{
    public class FallbackLayeringRender
    {
        private volatile bool _screenshotRequested = false;
        private volatile bool _DelegateActive = false;
        /// <summary>
        /// RGBA is used, so 4 byte per pixel, or 32 bits.
        /// </summary>
        private const uint BytePerPixel = 4;

        /// <summary>
        /// the number of bytes per "line"
        /// For performance reasons inside the core of VLC, it must be aligned to multiples of 32.
        /// </summary>
        private static uint Pitch = 32;

        /// <summary>
        /// The number of lines in the buffer.
        /// For performance reasons inside the core of VLC, it must be aligned to multiples of 32.
        /// </summary>
        private static uint Lines = 32;

        private static uint _Width = 5;
        private static uint _Height = 5;

        private static MemoryMappedFile CurrentMappedFile;
        private static MemoryMappedViewAccessor CurrentMappedViewAccessor;
        
        private static long FrameCounter = 0;
        private static System.Windows.Controls.Image _fallbackImageSurface;
        private LibVLC _libVLC;
        private MediaPlayer _mediaPlayer;
        private VideoView _displayElement;
        private static System.Windows.Media.Imaging.BitmapImage _image;

        public FallbackLayeringRender(LibVLC libvlc, MediaPlayer mediaplayer, VideoView displayelement, System.Windows.Controls.Image fallbackImage)
        {
            // Listen to events
            var processingCancellationTokenSource = new CancellationTokenSource();
            _libVLC = libvlc;
            _mediaPlayer = mediaplayer;
            _displayElement = displayelement;

            _displayElement.SizeChanged += DisplayElement_SizeChanged;
            _fallbackImageSurface = fallbackImage;

        }
        public void Start()
        {
            if (_DelegateActive)
                return;
            _DelegateActive = true;
            _mediaPlayer.Pause();
            _mediaPlayer.SetVideoFormat("RV32", _Width, _Height, Pitch);
            _mediaPlayer.SetVideoCallbacks(Lock, null, Display);
            _mediaPlayer.Play();
        }
        public void Stop()
        {
            _mediaPlayer.SetVideoCallbacks(null, null, null);
            _DelegateActive = false;
            CurrentMappedViewAccessor?.Dispose();
            CurrentMappedViewAccessor = null;
            CurrentMappedFile?.Dispose();
            CurrentMappedFile = null;
        }
        private static System.Windows.Media.Imaging.BitmapImage ToImage(MemoryStream stram)
        {
            var image = new System.Windows.Media.Imaging.BitmapImage();
            image.BeginInit();
            image.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad; // here
            image.StreamSource = stram;
            image.EndInit();
            return image;
        }
        private void DisplayElement_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            _Width = (uint)e.NewSize.Width;
            _Height = (uint)e.NewSize.Height;
            Pitch = Align(_Width * BytePerPixel);
            Lines = Align(_Height);

            uint Align(uint size)
            {
                if (size % 32 == 0)
                {
                    return size;
                }

                return ((size / 32) + 1) * 32;// Align on the next multiple of 32
            }

        }
        public void Shutdown()
        {


            // Unhook events

            _displayElement.SizeChanged -= DisplayElement_SizeChanged;
            // dispose of the things and then
            _displayElement = null;
            _mediaPlayer = null;
            _libVLC = null;
        }
        private static IntPtr Lock(IntPtr opaque, IntPtr planes)
        {
            if (CurrentMappedFile == null)
            {
                CurrentMappedFile = MemoryMappedFile.CreateNew(null, Pitch * Lines);
                CurrentMappedViewAccessor = CurrentMappedFile.CreateViewAccessor();
            }
            Marshal.WriteIntPtr(planes, CurrentMappedViewAccessor.SafeMemoryMappedViewHandle.DangerousGetHandle());
            return IntPtr.Zero;
        }

        public static BitmapImage ToBitmapImage(MemoryStream data)
        {
                BitmapImage img = new BitmapImage();
                img.BeginInit();
                img.CacheOption = BitmapCacheOption.OnLoad;//CacheOption must be set after BeginInit()
                img.StreamSource = data;
                img.EndInit();

                if (img.CanFreeze)
                {
                    img.Freeze();
                }

                return img;
        }

        private static void Display(IntPtr opaque, IntPtr picture)
        {
            //if (FrameCounter % 50 == 0)
            //{
                //FilesToProcess.Enqueue((CurrentMappedFile, CurrentMappedViewAccessor));
                using (var image = new Image<SixLabors.ImageSharp.PixelFormats.Bgra32>((int)(Pitch / BytePerPixel), (int)Lines))
                using (var sourceStream = CurrentMappedFile.CreateViewStream())
                {
                    var mg = image.GetPixelMemoryGroup();
                    for (int i = 0; i < mg.Count; i++)
                    {
                        sourceStream.Read(MemoryMarshal.AsBytes(mg[i].Span));
                    }
                    //image.Mutate(ctx => ctx.Crop((int)_Width, (int)_Height));

                    MemoryStream stram = new MemoryStream();
                    image.SaveAsBmp(stram);
                    stram.Seek(0, SeekOrigin.Begin);
                    //image = FallbackLayeringRender.ToImage(stram);
                    var aBitmapImage = ToBitmapImage(stram);

                    //_fallbackImageSurface.Dispatcher.BeginInvoke((Action)(() =>
                    //{
                    //    _fallbackImageSurface.BeginInit();
                    //    _fallbackImageSurface.Source = aBitmapImage;
                    //    _fallbackImageSurface.EndInit();
                    //}));

                }
                CurrentMappedViewAccessor.Dispose();
                CurrentMappedFile.Dispose();

                CurrentMappedFile = null;
                CurrentMappedViewAccessor = null;
            //}
            //else
            //{
            //    CurrentMappedViewAccessor.Dispose();
            //    CurrentMappedFile.Dispose();
            //    CurrentMappedFile = null;
            //    CurrentMappedViewAccessor = null;
            //}
            //FrameCounter++;

            //if (FrameCounter > long.MaxValue - 50)
            //    FrameCounter = 1;
        }
        

    }

    /// <summary>
    /// You don't need these if you're using .NET CORE or .NET 6
    /// </summary>
    public static class DotNetCoreStreamExtension
    {
        public static int Read(this Stream thisStream, Span<byte> buffer)
        {
            byte[] sharedBuffer = ArrayPool<byte>.Shared.Rent(buffer.Length);
            try
            {
                int numRead = thisStream.Read(sharedBuffer, 0, buffer.Length);
                if ((uint)numRead > (uint)buffer.Length)
                {
                    throw new IOException("Stream Too Long");
                }
                new Span<byte>(sharedBuffer, 0, numRead).CopyTo(buffer);
                return numRead;
            }
            finally { ArrayPool<byte>.Shared.Return(sharedBuffer); }
        }
        public static void Write(this Stream thisStream, ReadOnlySpan<byte> buffer)
        {
            byte[] sharedBuffer = ArrayPool<byte>.Shared.Rent(buffer.Length);
            try
            {
                buffer.CopyTo(sharedBuffer);
                thisStream.Write(sharedBuffer, 0, buffer.Length);
            }
            finally { ArrayPool<byte>.Shared.Return(sharedBuffer); }
        }
    }
}
