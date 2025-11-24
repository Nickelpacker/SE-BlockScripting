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
        const bool doLights = true;
        const bool doThrusters = true;

        public Program()
        {

        }

        public void Save()
        {

        }
        public int numMode;
        public void Main(string arg, UpdateType updateSource)
        {


            float batPerc = 0;
            List<IMyBatteryBlock> batteries = new List<IMyBatteryBlock>();
            List<IMyGasTank> hydrogenTanks = new List<IMyGasTank>();
            List<IMyGasGenerator> hydroGen = new List<IMyGasGenerator>();
            List<IMyFunctionalBlock> allBlocks = new List<IMyFunctionalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyBatteryBlock>(batteries);
            GridTerminalSystem.GetBlocksOfType<IMyGasTank>(hydrogenTanks);
            GridTerminalSystem.GetBlocksOfType<IMyGasGenerator>(hydroGen);
            GridTerminalSystem.GetBlocksOfType<IMyFunctionalBlock>(allBlocks);

            float avgBatPer = batPerc / batteries.Count;
            List<IMyLightingBlock>lightingBlocks = new List<IMyLightingBlock>();
            foreach (IMyLightingBlock block in allBlocks)
            {
                lightingBlocks.Add(block);
            }
            if (avgBatPer < 25)
            {

            }
            if (updateSource == UpdateType.Terminal)
            {
                numMode = 1;
            }

            switch (arg.ToLower())
            {
                case "mode_switch":
                    if (numMode < 3) numMode++;              
                    else numMode = 1;
                    break;

                case "override":
                    numMode = 0;
                    break;

            }
            foreach (IMyBatteryBlock block in allBlocks)
            {
                batPerc += block.CurrentStoredPower;
            }


        }

    }
}
