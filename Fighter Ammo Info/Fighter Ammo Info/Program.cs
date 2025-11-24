using Sandbox.Game.Entities;
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
        /* 
         *                  How To Use AAC
         * Create group with all cargo containers called "Cargo"
         * Create group with all weapons you would like to include named "Weapons"
         * Now wait
        */
        public int numAmmo;


        public Program()
        {

        }


        public void Main(string argument, UpdateType updateSource)
        {
            MyInventoryItem item;
            IMyBlockGroup cargoBG = GridTerminalSystem.GetBlockGroupWithName("Cargo");
            IMyBlockGroup gunsBG = GridTerminalSystem.GetBlockGroupWithName("Weapons");
            List<IMyCargoContainer> cargos = new List<IMyCargoContainer>();
            
            cargoBG.GetBlocksOfType(cargos, cargo => cargo.IsFunctional);
            
            foreach (IMyInventory block in cargos.Cast<IMyInventory>())
            {
                List<MyInventoryItem> items = new List<MyInventoryItem>();
                block.GetItems(items);
                for (int i = 0; i < items.Count; i++)
                {
                    item = items[i];
                    MyItemType itemt = item.Type;
                    MyItemInfo myItemInfo = itemt.GetItemInfo();
                    if(myItemInfo.IsAmmo) 
                    {
                        numAmmo = (int)block.GetItemAmount(itemt);
                    }
                }

            }
        }
    }
}
