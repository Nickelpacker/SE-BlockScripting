using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Net.Http.Headers;
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
        /*
        To Begin Put All: 
        
        Thrusters 
        Gyros
        Air Vents
        H2/O2 Generators
        Oxygen Tanks
        
        in a Group Named "System"
        After, Name the Programmable Block "(System) Programmable Block"
        Compile Then Run and Wait
         */
        string blockCount;
        string group = "";
        bool OnOff;
        int numBlocks;
        int timeunit;
        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update10;
        }
        public void Main(string argument, UpdateType updateSource)
        {
            IMyBlockGroup Allblock = GridTerminalSystem.GetBlockGroupWithName("System");
            IMyTextSurfaceProvider myProgrammable = GridTerminalSystem.GetBlockWithName("(System) Programmable Block") as IMyTextSurfaceProvider;
            IMyProgrammableBlock myProgrammableg = GridTerminalSystem.GetBlockWithName("(System) Programmable Block") as IMyProgrammableBlock;
            var screen = myProgrammable.GetSurface(0);
            screen.Alignment = TextAlignment.CENTER;
            screen.ContentType = ContentType.TEXT_AND_IMAGE;
            screen.BackgroundColor = Color.Blue;
            screen.FontColor = Color.Green;
            screen.WriteText("Penny's Startup System\n======================\n\n" + "Missing Blocks: " + blockCount + group + "\n\nNumber Of Blocks in System: " + numBlocks + "\n" + timeunit.ToString());
            if (blockCount == null) { blockCount += ""; }
            if (Allblock != null)
            {
                group = "";
                List<IMyThrust> thrusters = new List<IMyThrust>();
                List<IMyGyro> gyros = new List<IMyGyro>();
                List<IMyGasGenerator> o2gen = new List<IMyGasGenerator>();
                List<IMyGasTank> o2tanks = new List<IMyGasTank>();
                List<IMyAirVent> vents = new List<IMyAirVent>();
                List<IMyBatteryBlock> batteries = new List<IMyBatteryBlock>();
                Allblock.GetBlocksOfType(thrusters, thrust => thrust.IsFunctional);
                Allblock.GetBlocksOfType(gyros, gyro => gyro.IsFunctional);
                Allblock.GetBlocksOfType(o2gen, gen => gen.IsFunctional);
                Allblock.GetBlocksOfType(o2tanks, tank => tank.IsFunctional);
                Allblock.GetBlocksOfType(vents, vent => vent.IsFunctional);
                Allblock.GetBlocksOfType(batteries, battery => battery.IsFunctional);
                if (thrusters.Count == 0 & !blockCount.Contains("Thrusters Missing")) { blockCount += "\nThrusters Missing"; }
                if (gyros.Count == 0 & !blockCount.Contains("Gyros Missing")) { blockCount += "\nGyros Missing"; }
                if (o2gen.Count == 0 & !blockCount.Contains("H2/O2 Generators Missing")) { blockCount += "\nH2/O2 Generators Missing"; }
                if (o2tanks.Count == 0 & !blockCount.Contains("Oxygen Tanks Missing")) { blockCount += "\nOxygen Tanks Missing"; }
                if (vents.Count == 0 & !blockCount.Contains("Vents Missing")) { blockCount += "\nVents Missing"; }
                if (batteries.Count == 0 & !blockCount.Contains("Batteries Missing")) { blockCount += "\nBatteries Missing"; }
                numBlocks = thrusters.Count + gyros.Count + o2gen.Count + o2tanks.Count + vents.Count + batteries.Count;
                timeunit++;
                if (argument.Equals("Start_Up"))
                {
                    OnOff = true;
                    timeunit = 0;

                }
                if (argument.Equals("Shut_Down"))
                {
                    OnOff = false;
                    timeunit = 0;
                }
                if (OnOff == true)
                {
                    foreach (var block in thrusters) { block.Enabled = true; block.CustomName = "(ONLINE) Thruster"; }

                    if (timeunit >= 15)
                    {
                        foreach (var block in o2tanks)
                        {
                            block.Stockpile = false;
                            block.CustomName = "(ONLINE) Oxygen Tank";
                        }
                    }
                    if (timeunit >= 30)
                    {
                        foreach (var block in vents)
                        {
                            block.Enabled = true;
                            block.CustomName = "(ONLINE) Vent";
                        }
                    }
                    if (timeunit >= 45)
                    {
                        foreach (var block in o2gen)
                        {
                            block.Enabled = true;
                            block.CustomName = "(ONLINE) H2/O2 Generator";
                        }
                    }
                    if (timeunit >= 60)
                    {
                        foreach (var block in batteries)
                        {
                            block.ChargeMode = ChargeMode.Auto;
                            block.CustomName = "(ONLINE) Battery";
                        }
                    }
                }
                if (OnOff == false)
                {
                    foreach (var block in thrusters) { block.Enabled = true; block.CustomName = "(OFFLINE) Thruster"; }

                    if (timeunit >= 15)
                    {
                        foreach (var block in o2tanks)
                        {
                            block.Stockpile = true;
                            block.CustomName = "(OFFLINE) Oxygen Tank";
                        }
                    }
                    if (timeunit >= 30)
                    {
                        foreach (var block in vents)
                        {
                            block.Enabled = false;
                            block.CustomName = "(OFFLINE) Vent";
                        }
                    }
                    if (timeunit >= 45)
                    {
                        foreach (var block in o2gen)
                        {
                            block.Enabled = false;
                            block.CustomName = "(OFFLINE) H2/O2 Generator";
                        }
                    }
                    if (timeunit >= 60)
                    {
                        foreach (var block in batteries)
                        {
                            block.ChargeMode = ChargeMode.Recharge;
                            block.CustomName = "(OFFLINE) Battery";
                        }
                    }
                }
            }
                else { group = "\nNo Block Group Detected..."; blockCount = ""; };
                Echo("   Penny's Startup System\n======================\n\n" + blockCount + group);
                string arg = myProgrammableg.TerminalRunArgument;
                if (timeunit >= 61) { timeunit = 0; }

        }
            //end
        
    }
}
