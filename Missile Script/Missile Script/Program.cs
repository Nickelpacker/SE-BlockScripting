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

         target;

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update1;

        }

    


        public void Main(string argument, UpdateType updateSource)
        {
            IMyBlockGroup group = GridTerminalSystem.GetBlockGroupWithName("Missile System");
            List<IMyCameraBlock> cameraBlocks = new List<IMyCameraBlock>();
            group.GetBlocksOfType(cameraBlocks, camera => camera.IsFunctional);
            foreach (var camera in cameraBlocks)
            {
              target = camera.Raycast(camera.AvailableScanRange);
             
            }

        }
    }
}
