using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BorgWin10WPF
{
    public class PlayerBreadCrumbTrail
    {
        private Stack<SceneDefinition> _Back;
        private Stack<SceneDefinition> _Forward;

        public PlayerBreadCrumbTrail()
        {
            Reset();
        }

        public int HistoryCount
        {
            get
            {
                return _Back.Count;
            }
        }
        public int ForwardCount
        {
            get
            {
                return _Back.Count;
            }
        }

        public void VisitedScene(SceneDefinition def)
        {
            if (def == null)
                return;

            if (_Back.Count > 0)
            {
                var scenecomparison = _Back.Peek();
                if (def.Name == scenecomparison.Name)
                    return;
            }
            _Back.Push(def);
            _Forward.Clear();
        }



        public SceneDefinition Back()
        {
            if (_Back.Count > 0)
            {
                SceneDefinition result = _Back.Pop();
                _Forward.Push(result);
                
                return result;
            }
            return null;
        }

        public SceneDefinition Forward()
        {
            if (_Forward.Count > 0)
            {
                SceneDefinition result = _Forward.Pop();
                _Back.Push(result);
                return result;
            }
            return null;
        }

        public List<SceneDefinition> GetHistoryList()
        {
            List<SceneDefinition> ResultList = _Back.ToList();
            ResultList.Reverse();
            return ResultList;
        }



        public void Reset()
        {
            _Back = new Stack<SceneDefinition>();
            _Forward = new Stack<SceneDefinition>();
        }
    }
}
