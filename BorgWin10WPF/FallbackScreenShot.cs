using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibVLCSharp;
using LibVLCSharp.Shared;
using LibVLCSharp.WPF;
namespace BorgWin10WPF
{
    public class FallbackScreenShot
    {
        private VideoView _videoView;
        private string _assetdir = string.Empty;

        public delegate void SnapshotTaken(string filename);

        public event SnapshotTaken OnSnapshotTaken;


        public FallbackScreenShot(VideoView player)
        {
            if (player == null)
                throw new NullReferenceException("Player isn't crate");

            if (player.MediaPlayer == null)
                throw new NullReferenceException("Media player isn't crate.");

            _videoView = player;
            _assetdir = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "Assets");
            _videoView.MediaPlayer.SnapshotTaken += MediaPlayer_SnapshotTaken;
        }

        private void MediaPlayer_SnapshotTaken(object sender, MediaPlayerSnapshotTakenEventArgs e)
        {
            var evt = OnSnapshotTaken;
            
            if (evt != null)
                evt(e.Filename);
        }

        public void TriggerScreenShot()
        {
            _videoView.MediaPlayer.TakeSnapshot(0, _assetdir, (uint)_videoView.ActualWidth, (uint)_videoView.ActualWidth);
        }


    }
}
