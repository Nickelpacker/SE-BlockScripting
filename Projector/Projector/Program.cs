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
        IMyRefinery f;
        IMyProjector projector;

        public List<string> items;
        public Program()
        {
            
            foreach (var item in projector.RemainingBlocksPerType.Keys.ToList())
            {
                items.Add(item);
            }
            List<int> itemCount = projector.RemainingBlocksPerType.Values.ToList();
            f.AddQueueItem();
        }



        public void Main(string argument, UpdateType updateSource)
        {

        }
    }
}
