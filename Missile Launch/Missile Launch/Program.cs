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

        public Dictionary<string, Action<string[]>> commands = new Dictionary<string, Action<string[]>>();
        public List<IMyTimerBlock> timers = new List<IMyTimerBlock>();
        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update1;
            RegisterCommands();
            Init(null);
        }

        public void Init(string[] args)
        {
            GridTerminalSystem.GetBlocksOfType(timers, t => t.IsFunctional && t.CustomName.ToLower().Contains("timer launch"));
        }


        public void Main(string argument, UpdateType updateSource)
        {


            if (!string.IsNullOrWhiteSpace(argument))
            {
                argument.ToLower();
                string[] args = argument.Split(' ');
                string cmd = args[0];
                string[] cmdARG = args.Skip(1).ToArray();

                commands[cmd](cmdARG);
            }
            EchoInfo();
            
        }
        public void Launch(string[] args)
        {
            Init(null);
            int delay = 1;
            int time = 0;
            int numLaunch = int.Parse(args[0]);
            for (int i = 0; i < Math.Min(numLaunch, timers.Count); i++)
            {
                timers[i].TriggerDelay = time;
                timers[i].StartCountdown();            
                time += delay;
            }
            Init(null);
        }
        public void EchoInfo()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Missile Count: {timers.Count}");
            Echo(sb.ToString());
        }
        public void RegisterCommands()
        {
            commands["launch"] = Launch;
            commands["refresh"] = Init;
        }
    }
}
