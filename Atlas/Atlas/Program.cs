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
        //          TAS
        /*
         *  Name your begin timer block to "Fire 1"
         *  Name your Controller to TAS Controller
         *  Edit the following setting for how you would like your turret to function
         */
        const bool targetPeople = false;
        const bool targetProjectiles = false;
        const bool targetLargeGrid = true;
        const bool targetSmallGrid = true;
        const bool targetStation = true;
        const bool targetNeutral = true;
        const bool targetFriends = false;
        IMyTurretControlBlock controller;
        IMyTimerBlock timer;

        public Program()
        {
          timer = GridTerminalSystem.GetBlockWithName("Fire 1") as IMyTimerBlock;
          controller = GridTerminalSystem.GetBlockWithName("TAS Controller") as IMyTurretControlBlock;
        }



        public void Main(string argument, UpdateType updateSource)
        {
           if (timer != null & controller != null)
                {
                controller.TargetCharacters = targetPeople;
                controller.TargetMeteors = targetProjectiles;
                controller.TargetMissiles = targetProjectiles;
                controller.TargetLargeGrids = targetLargeGrid;
                controller.TargetSmallGrids = targetSmallGrid;
                controller.TargetStations = targetStation;
                controller.TargetNeutrals = targetNeutral;
                controller.TargetFriends = targetFriends;
                    if (controller.HasTarget == true & AimingAtTarget())
                        {
                        timer.Trigger();
                        }
                    
                }
                {
                    Echo("Block missing...\nCheck you correctly named them");
                }
        }
        public bool AimingAtTarget()
        {
            Vector3 aimDirection = controller.GetShootDirection();
            MyDetectedEntityInfo target = controller.GetTargetedEntity();
            Vector3D targetPostion = target.Position;
            Vector3 DesiredDirection = targetPostion.
            return true;
        }
    }
}
