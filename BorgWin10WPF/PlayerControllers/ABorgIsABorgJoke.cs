using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BorgWin10WPF.Scene;

namespace BorgWin10WPF.PlayerControllers
{
    /// <summary>
    /// This class redirects the user to the correct borg scene when they info point click on a borg.  
    /// In the story, Q gets frustrated when you click on borg over and over again.
    /// This follows a similar pattern to the puzzle controllers..  but it isn't a puzzle.
    /// </summary>
    public class ABorgIsABorgJoke
    {
        //I_30, I_01,I_02,I_03,I_04

        /// <summary>
        /// How many times we've visited the borg info point.
        /// </summary>
        private int _borgCount =-1;

        /// <summary>
        /// Should we redirect?   Yes or no
        /// </summary>
        /// <param name="scene">The Scene that we are playing</param>
        /// <returns></returns>
        public bool ShouldActivate(SceneDefinition scene)
        {
            if (scene.Name == "I_01" )
                return true;
            return false;
        }
        /// <summary>
        /// Which scene should we redirect to?  
        /// </summary>
        /// <param name="def">Unused. But there as an example of a general interface</param>
        /// <returns>Scene name that we should redirect the user to</returns>
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
