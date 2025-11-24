using Sandbox.Game.EntityComponents;
using Sandbox.Game.GameSystems;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Utils;
using VRageMath;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {

        public const string SECONDARY_THRUST_GROUP = "Secondary Thrust";
        public const string INFO_DISPLAY_PREFIX = "[OSF]";
        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update10;
        }

        public void Save()
        {
            
        }

        public void Main(string argument, UpdateType updateSource)
        {
            
            SystemSetup();
            var checker = new AutoChecks(this);
        }
        public void SystemSetup()
        {
            string shipName = Me.CubeGrid.CustomName;
            IEnumerable<string> a = shipName.Trim().Split(' ').Where(s => !s.Contains("["));
            string[] excludeList = { "Class", "Corvette", "Crusier", "Frigate", "Freighter", "Heavy", "Light"};
            var filtered = a.Where(s => !excludeList.Contains(s));
            Me.CustomName = $"[OS] {string.Join(" ", filtered)} Programmable Block";
        }
        #region Grid Program Inherit
        public class InheritGridProgram
        {
            private static MyGridProgram G;
            public static IMyGridTerminalSystem GridTerminalSystem;
            public static IMyProgrammableBlock Me;
            public static Action<string> Echo;
            public InheritGridProgram(MyGridProgram parent)
            {
                G = parent;
                Me = G.Me;
                GridTerminalSystem = G.GridTerminalSystem;
                Echo = G.Echo;
            }
        }
        #endregion
        #region Checks

        public class AutoChecks : InheritGridProgram
        {
            private static string GravityInfo;
            private static string PlayerControlInfo;
            
            public AutoChecks(MyGridProgram parent) : base(parent) 
            {

                RunChecks();
            }

            public void RunChecks()
            {
                GravityCheck();
                IsInPlayerControl();
                FinalizeCheck();
            }
            private static void GravityCheck()
            {
                List<IMyCockpit> cockpitList = new List<IMyCockpit>();
                GridTerminalSystem.GetBlocksOfType(cockpitList);
                if (cockpitList[0].GetNaturalGravity().Length() != 0)
                {
                    GravityInfo = $"Gravity: {cockpitList[0].GetNaturalGravity().Length() / 9.80665:F2}g";
                }
                else
                {
                    GravityInfo = "No Gravity Well Detected";
                }
                
            }
            private static void IsInPlayerControl()
            {
                StringBuilder sb = new StringBuilder();
                StringBuilder sb2 = new StringBuilder();
                int i = 0;
                List<IMyCockpit> cockpits = new List<IMyCockpit>();
                GridTerminalSystem.GetBlocksOfType(cockpits, b => b.IsFunctional && b.CubeGrid == Me.CubeGrid);
                foreach (IMyCockpit cockpit in cockpits)
                {
                    if (cockpit.IsUnderControl)
                    {
                        i++;
                        sb2.AppendLine(cockpit.CustomName);
                    }
                }
                sb.AppendLine($"Total Cockpits Under Control: {i}");
                sb.Append( "\n" + sb2.ToString() );
                PlayerControlInfo = sb.ToString();
            }
            private static void FinalizeCheck()
            {
                List<IMyTerminalBlock> TextSurfaceProviders = new List<IMyTerminalBlock>();
                GridTerminalSystem.GetBlocksOfType(TextSurfaceProviders, b => b is IMyTextSurfaceProvider && b.CustomName.Contains(INFO_DISPLAY_PREFIX) && b.CubeGrid == Me.CubeGrid);
                foreach (var provider in TextSurfaceProviders)
                {
                    StringBuilder sb = new StringBuilder();
                    if (provider.CustomData.Split('\n')[0] != provider.CustomName)
                    {
                        StringBuilder cd = new StringBuilder("");
                        cd.AppendLine(provider.CustomName);
                        cd.AppendLine("Surface Number=0");
                        cd.AppendLine("Gravity Info=true").AppendLine("Player Control Info=true");
                        provider.CustomData = cd.ToString();
                    }
                    sb.AppendLine($"[{Me.GetOwnerFactionTag()}]");
                    sb.AppendLine($"System Checks: {Me.CubeGrid.CustomName.Replace($"[{Me.GetOwnerFactionTag()}]", " ").Trim()}");
                    sb.AppendLine("");
                    if (Boolean.Parse(provider.CustomData.Split('\n')[2].Split('=')[1])) sb.AppendLine(GravityInfo);
                    if (Boolean.Parse(provider.CustomData.Split('\n')[3].Split('=')[1])) sb.AppendLine(PlayerControlInfo);
                    int surfaceToDisplay;
                    int.TryParse(provider.CustomData.Split('\n')[1].Split('=')[1].Trim(), out surfaceToDisplay);
                    var b = provider as IMyTextSurfaceProvider;
                    IMyTextSurface surface = b.GetSurface(surfaceToDisplay);
                    
                    surface.ContentType = ContentType.TEXT_AND_IMAGE;
                    surface.FontColor = Color.DarkOrange;
                    surface.Alignment = TextAlignment.CENTER;
                    surface.WriteText(sb.ToString(), false);
                }

            }
        }
        #endregion

        public class LandingSystem : InheritGridProgram
        {
            public LandingSystem(MyGridProgram parent) : base(parent) { }
        }
        public class AirlockSystem : InheritGridProgram
        {
            public AirlockSystem(MyGridProgram parent) : base(parent) { }
        }
        public class PowerManagementSystem : InheritGridProgram
        {
            public PowerManagementSystem(MyGridProgram parent) : base(parent) { }
        }
        public class DamageAssementSystem : InheritGridProgram
        {
}
        }
        
    }
}
