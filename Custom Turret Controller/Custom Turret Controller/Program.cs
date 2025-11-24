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
        //start
        IMyTurretControlBlock turretController;
        
        IMyMotorStator elvRotor2;

        public Program()
        {
            turretController = GridTerminalSystem.GetBlockWithName("Turret Controller") as IMyTurretControlBlock;
            elvRotor2 = GridTerminalSystem.GetBlockWithName("Elevation Rotor 2") as IMyMotorStator;   
        }
        public void Main(string argument, UpdateType updateSource)
        {
            IMyBlockGroup group = GridTerminalSystem.GetBlockGroupWithName("Stabilizers");
            List<IMyGyro> Gyros = new List<IMyGyro>();
            group.GetBlocksOfType(Gyros, Gyro => Gyro.Enabled);
            Runtime.UpdateFrequency = UpdateFrequency.Update1;
            var elvRotor = turretController.ElevationRotor;
            var azmRotor = turretController.AzimuthRotor;
            elvRotor2.TargetVelocityRPM = elvRotor.TargetVelocityRPM * -1;
            foreach (var block in Gyros) 
            {
                block.ShowInTerminal = false; 
                if (block.Name.Contains("Stablizer") == false) {block.CustomName = "(Stablizer) " + block.Name;}
            }
            if (elvRotor.TargetVelocityRPM == 0 & azmRotor.TargetVelocityRPM == 0)
            {
                foreach (var block in Gyros)
                {
                    block.GyroOverride = true;
                }
            }
            else 
            {
                foreach (var block in Gyros)
                {
                    block.GyroOverride = false;
                }
            }
            if(argument.Equals("Reverse Y-axis"))
            {
                turretController.VelocityMultiplierElevationRpm = turretController.VelocityMultiplierElevationRpm * -1;
            }
            if (argument.Equals("Reverse X-axis"))
            {
                turretController.VelocityMultiplierAzimuthRpm = turretController.VelocityMultiplierAzimuthRpm * -1;
            }

        }
        //end
    }
}
