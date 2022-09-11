using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BorgWin10WPF.Hotspot;

namespace BorgWin10WPF.Scene
{

    public enum SceneType
    {
        Main,
        Bad,
        Info,
        Inaction
    }
    public class SceneDefinition
    {
        private long _startMS { get; set; } = 0;
        private long _endMS { get; set; } = 0;
        private long _offsetTimeMS { get; set; } = 0;
        private long _successMS { get; set; } = 0;
        private long _retryMS { get; set; } = 0;

        public int OriginalRetryFrames { get; set; } = 0;

        internal HotspotDefinition LastHotspotTrigger { get; set; }

        private SceneType _sceneType { get; set; }

        private int _requiresChoice { get; set; } =  -1;

        public SceneDefinition()
        {

        }
        public SceneDefinition(SceneType type, string name, long msStart, long msEnd, long sOffset)
        {
            _startMS = msStart;
            _endMS = msEnd;
            _offsetTimeMS = sOffset * 100;
            SceneType = type;
            Name = name;
            CD = 1;
        }
        public SceneDefinition(SceneType type, string name, int frameStart, int frameEnd, long sOffset)
        {
            _startMS = Utilities.Frames15fpsToMS(frameStart);
            _endMS = Utilities.Frames15fpsToMS(frameEnd);
            _offsetTimeMS = sOffset * 100;
            SceneType = type;
            Name = name;
            CD = 1;
        }
        public SceneDefinition(SceneType type, string name, int cd, int frameStart, int frameEnd, long sOffset, int successFrame, int retryFrame)
        {
            _startMS = Utilities.Frames15fpsToMS(frameStart);
            _endMS = Utilities.Frames15fpsToMS(frameEnd);
            _offsetTimeMS = sOffset * 100;
            _successMS = Utilities.Frames15fpsToMS(successFrame);
            _retryMS = Utilities.Frames15fpsToMS(retryFrame);
            OriginalRetryFrames = retryFrame;
            SceneType = type;
            Name = name;
            CD = cd;
        }
        

        public string Name { get; set; }

        public SceneType SceneType { get { return _sceneType; } set { _sceneType = value; } }
        public long OffsetTimeMS { get { return _offsetTimeMS; } set { _offsetTimeMS = value; } }
        public bool SceneHasChallengeYN
        {
            get
            {
                return _retryMS > 0;
            }
        }
        public void SetStartMS(long ms)
        {
            _startMS = ms;
        }

        public long StartMS => _startMS + _offsetTimeMS;
        public long EndMS => _endMS + _offsetTimeMS;
        public long SuccessMS => _successMS + _offsetTimeMS;
        public long retryMS => _retryMS + _offsetTimeMS;

        public int FrameStart
        {
            get { return (int)Utilities.MsTo15fpsFrames(_startMS) + 2; }
            set { _startMS = Utilities.Frames15fpsToMS(value - 2); }
        }

        public bool InactionBad
        {
            get
            {

                foreach (var item in PlayingHotspots)
                    if (item != null && string.IsNullOrEmpty(item.ActionVideo))
                        return true;
                return false;
            }
        }

        public int FrameEnd
        {
            get { return (int)Utilities.MsTo15fpsFrames(_endMS) + 2; }
            set { _endMS = Utilities.Frames15fpsToMS(value - 2); }
        }

        public SceneDefinition ParentScene { get; set; } = null;

        public List<HotspotDefinition> PlayingHotspots { get; set; } = new List<HotspotDefinition>();
        public List<HotspotDefinition> PausedHotspots { get; set; } = new List<HotspotDefinition>();

        public int CD { get; set; }
    }
}
