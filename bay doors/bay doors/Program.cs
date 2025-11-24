using IngameScript;
using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;

namespace IngameScript
{

    partial class Program : MyGridProgram
    {
        //start here
        IMyShipController Cockpit;
        IMyCameraBlock Cameras;
        IMyTextSurface lcd;
        float raycastDistance;
        
        public Program()
        {
            Cockpit = GridTerminalSystem.GetBlockWithName("(Main) Cockpit") as IMyShipController;
            Cameras = GridTerminalSystem.GetBlockWithName("Camera") as IMyCameraBlock;
            lcd = GridTerminalSystem.GetBlockWithName("LCD") as IMyTextSurface;
        }
        public void Main(string argument, UpdateType updateSource)
        {
            IMyBlockGroup group = GridTerminalSystem.GetBlockGroupWithName("Landing Gear");
            List<IMyMotorSuspension> wheels = new List<IMyMotorSuspension>();
            group.GetBlocksOfType(wheels, wheel => wheel.Enabled);
            IMyBlockGroup group2 = GridTerminalSystem.GetBlockGroupWithName("Thruster");
            List<IMyThrust> thrusters = new List<IMyThrust>();
            group2.GetBlocksOfType(thrusters, thrust => thrust.Enabled);
            Runtime.UpdateFrequency = UpdateFrequency.Update1;
            Cameras.EnableRaycast = true;
            MyDetectedEntityInfo hitinfo;
            if (argument.Equals("add_100"))
            {
                raycastDistance += 100; 
            }
            if (argument.Equals("sub_100") & raycastDistance > 500)
            {
                raycastDistance -= 100;
            }
            hitinfo = Cameras.Raycast(raycastDistance);
            if (!hitinfo.IsEmpty())
            {
                double distance = (hitinfo.HitPosition.Value - Cameras.GetPosition()).Length();
                double distance2 = Math.Ceiling(distance);
                Echo.Invoke("AC-130 System\n-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-\nRaycast Max Distance: " + raycastDistance.ToString() + "\nRaycast Distance: " + distance2.ToString() + "\n-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-\nSystem Fully Operational");
                lcd.WriteText("Raycast Max Distance: " + raycastDistance.ToString() + "m" + "\nRaycast Distance: " + distance2.ToString() + "m");
                if (Cockpit.GetShipSpeed() >= 150 & distance >= 500)
                { 
                        foreach (var block in wheels)
                        {
                            
                            block.Brake = true;
                            block.Enabled = false;
                        }
                        foreach (var block in thrusters)
                        {
                            block.Enabled = false;
                        }
                }
                else
                {
                    foreach (var block in thrusters)
                    {
                        block.Enabled = true;
                    }
                    if (distance <= 100)
                    {
                        foreach (var block in wheels)
                        {
                            block.Enabled = true;
                            block.Brake = true;
                        }
                    }
                }
            }
        }
        //end here
    }
}
