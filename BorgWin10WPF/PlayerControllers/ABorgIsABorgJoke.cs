using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BorgWin10WPF.Scene;

namespace BorgWin10WPF.PlayerControllers
{
    public class ABorgIsABorgJoke
    {
        //I_30, I_01,I_02,I_03,I_04

        private int _borgCount =-1;

        public bool ShouldActivate(SceneDefinition scene)
        {
            if (scene.Name == "I_01" )
                return true;
            return false;
        }

        public string Activate(SceneDefinition def)
        {
            _borgCount++;
            if (_borgCount > 4)
                _borgCount = 4;

            switch (_borgCount)
            {
                case 0:
                    return "I_30";
                case 1:
                    return "I_01";
                case 2:
                    return "I_02";
                case 3:
                    return "I_03";
                case 4:
                    return "I_04";
                default:
                    return "I_30";
            }
        }
    }
}
