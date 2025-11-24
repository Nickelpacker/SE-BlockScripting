using ParallelTasks;
using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
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


        public int modeNum;
        public Dictionary<string, Action<string[]>> Modes = new Dictionary<string, Action<string[]>>();


        public void Main(string argument, UpdateType updateSource)
        {
            if (updateSource == UpdateType.Terminal) modeNum = 0;       
            IMyTextPanel _infoPanel = GridTerminalSystem.GetBlockWithName("Info Panel") as IMyTextPanel;
            if (argument.ToLower().Trim().Equals("mode_switch")) if (modeNum == 2) modeNum = 0;else modeNum++;                
            
        }
        void RegisterModes()
        {
            Modes["raycast"] = RaycastMode.Main;
            Modes["gps"] = GpsMode.Main;
            Modes["glide"] = GlideMode.Main;
        }
        void SwitchModes()
        {

        }
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

        public class RaycastMode : InheritGridProgram
        {
            public RaycastMode(MyGridProgram parent) : base(parent) { }

            public static void Main(string[] args)
            {

            }
            public MyDetectedEntityInfo RaycastDesignate()
            {
                IMyCameraBlock _designator = GridTerminalSystem.GetBlockWithName("Designator") as IMyCameraBlock;
                _designator.Enabled = true;
                _designator.EnableRaycast = true;
                double _availableDistance = _designator.RaycastDistanceLimit;
                MyDetectedEntityInfo _entityInfo = _designator.Raycast(_availableDistance);
                return _entityInfo;
            }
            public void DisplayInfo(IMyTextPanel panel)
            {
                StringBuilder sb = new StringBuilder();
                string _entityName = RaycastDesignate().Name;
                string _entityVelocity = RaycastDesignate().Velocity.ToString();
                string _entityPosition = RaycastDesignate().Position.ToString();
                string _entityRelation = RaycastDesignate().Relationship.ToString();
            }
        }
        public class GpsMode
        {
            public static void Main(string[] args)
            {

            }

        }
        public class GlideMode
        {
            public static void Main(string[] args)
            {

            }
        }
    }
}
