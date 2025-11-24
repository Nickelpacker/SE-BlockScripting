using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
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
        

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update1;
        }


        public void Main(string argument, UpdateType updateSource)
        {
            MyWaypointInfo way = new MyWaypointInfo(name:"f", coords: new Vector3D(0,0,0));
            List<IMyWarhead> myWarheads = new List<IMyWarhead>();
            GridTerminalSystem.GetBlocksOfType<IMyWarhead>(myWarheads);
            if (myWarheads.Count == 0)
            {
                Echo("No Warheads Detected...");
            }
            Echo($"\nTotal Number Of Warheads: {myWarheads.Count}");
            if (argument.ToLower().Equals("scuttle"))
            {
                Echo("Scuttle Procedure in Progress...");
                foreach (var block in myWarheads)
                {
                    block.DetonationTime = ((float)(myWarheads.IndexOf(block) + 1 * Math.Ceiling(Math.Log10(myWarheads.IndexOf(block) + 1)))) + 10;
                    block.StartCountdown();
                }
            }
        }
    }
}
