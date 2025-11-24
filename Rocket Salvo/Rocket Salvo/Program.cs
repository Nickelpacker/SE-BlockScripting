using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        List<string> _fireModes = new List<string>(3) {"random", "in order", "all" };
        string _fireMode;

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update1;
        }

        public void Save()
        {

        }
        public int modeNum;
        public void Main(string argument, UpdateType updateSource)
        {
            
            if (updateSource == UpdateType.Terminal) modeNum = 0;
            IMyBlockGroup Group = GridTerminalSystem.GetBlockGroupWithName("SALVO");
            List<IMyUserControllableGun> Rockets = new List<IMyUserControllableGun>();
            Group.GetBlocksOfType(Rockets, Rocket => Rocket.IsFunctional);
            if (argument.ToLower().TrimEnd().Equals("mode_switch"))
            {
                modeNum++;
            }    

        }
    }
}
