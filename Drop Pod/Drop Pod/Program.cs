using Sandbox.Game.EntityComponents;
using Sandbox.Graphics.GUI;
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
        double raycastdistance;
        double distance;
        string direction;
        string distanceS;
        MatrixD facing;

        MyDetectedEntityInfo hitinfo;
        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update1;
            facing = MatrixD.Identity;
        }

        public void Main(string argument, UpdateType updateSource)
        {

            IMyCameraBlock camera = GridTerminalSystem.GetBlockWithName("Drop Pod Camera") as IMyCameraBlock;
            IMyCockpit Cockpit = GridTerminalSystem.GetBlockWithName("Drop Pod Cockpit") as IMyCockpit;
            raycastdistance = camera.AvailableScanRange;

            IMyBlockGroup group = GridTerminalSystem.GetBlockGroupWithName("Drop Pod System");
            List<IMyThrust> thrust = new List<IMyThrust>();
            group.GetBlocksOfType(thrust, thrusters => thrusters.IsFunctional);
            List<IMyGyro> gyro = new List<IMyGyro>();
            group.GetBlocksOfType(gyro, gyros => gyros.IsFunctional);
            camera.EnableRaycast = true;
            hitinfo = camera.Raycast(raycastdistance);
            if (!hitinfo.IsEmpty())
            {
                distance = (hitinfo.HitPosition.Value - camera.GetPosition()).Length();
                distanceS = distance.ToString();
            }

            facing = hitinfo.Orientation;
            direction = facing.ToString();
            Echo(direction);

        }
    }
}
