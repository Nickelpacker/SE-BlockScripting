using Sandbox.Game.Entities;
using Sandbox.Game.EntityComponents;
using Sandbox.Game.WorldEnvironment.Modules;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Security.Cryptography;
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
        // Penny's Multi-Grid Targeting System //
        /* (or MGTS for short)
         * 
         * 
         * 
         */
        const string _broadcastTagPos = "Pos";
        const string _brodcastTagFire = "fire";
        
        IMyBroadcastListener _myBroadcastListenerpos;
        IMyBroadcastListener _myBroadcastListenerfire;
        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update10;
            _myBroadcastListenerpos = IGC.RegisterBroadcastListener(_broadcastTagPos);
            _myBroadcastListenerfire = IGC.RegisterBroadcastListener(_brodcastTagFire);
        }

        public void Save()
        {

        }
        IMyCameraBlock camera;
        IMyRemoteControl controller;
        public bool _isGroup;
        public bool _targetLocked;
        public bool _isViableName;
        public List<string> _missingItems = new List<string>();
        public void Main(string argument, UpdateType updateSource)
        {
            _missingItems.Clear();
            _missingItems.Add("Missing Items:");
            if (updateSource == UpdateType.Terminal)
            {
                StringBuilder _customData = new StringBuilder();
                _customData.Append("GroupName=MGTS\n");
                Me.CustomData = _customData.ToString();
            }
             
            string[] f = Me.CustomData.Split('=');
            string groupName = f[1].Trim('\n');
            if (groupName.Length == 0) _isViableName = false;
            else _isViableName = true;
            IMyBlockGroup blockGroup = GridTerminalSystem.GetBlockGroupWithName(groupName);
            if (blockGroup != null)
            {
                _isGroup = true;
                List<IMyCameraBlock> cam = new List<IMyCameraBlock>();
                List<IMyRemoteControl> controllers = new List<IMyRemoteControl>();
                List<IMyTimerBlock> timerBlocks = new List<IMyTimerBlock>();
                GridTerminalSystem.GetBlocksOfType<IMyCameraBlock>(cam);
                GridTerminalSystem.GetBlocksOfType<IMyRemoteControl>(controllers);
                GridTerminalSystem.GetBlocksOfType<IMyTimerBlock>(timerBlocks);

                if (controllers.Count > 0) controller = controllers[0];
                else
                {
                    _missingItems.Add("Remote Control");

                }
                if (cam.Count > 0) camera = cam[0];
                else
                {
                    _missingItems.Add("Camera");
                }
                camera.EnableRaycast = true;
                if (_missingItems.Count == 1)
                {
                    if (!_myBroadcastListenerpos.HasPendingMessage)
                    {
                        MyDetectedEntityInfo hitinfo = camera.Raycast(25000, 0, 0);
                        if (!hitinfo.IsEmpty()) if (hitinfo.Type.Equals(MyDetectedEntityType.LargeGrid) || hitinfo.Type.Equals(MyDetectedEntityType.SmallGrid))
                            {
                                IGC.SendBroadcastMessage(_broadcastTagPos, (Vector3D)hitinfo.HitPosition);
                            }
                    }

                    if (argument.ToLower().Equals("fire"))
                    {
                        IGC.SendBroadcastMessage(_brodcastTagFire, "fire");
                        timerBlocks[0].TriggerDelay = 1;
                        timerBlocks[0].Silent = true;
                        timerBlocks[0].StartCountdown();
                    }
                    while (_myBroadcastListenerpos.HasPendingMessage)
                    {
                        MyIGCMessage receivedMessage = _myBroadcastListenerpos.AcceptMessage();
                        if (receivedMessage.Tag == _broadcastTagPos)
                        {
                            _targetLocked = true;
                            if (receivedMessage.Data is Vector3D)
                            {
                                MyWaypointInfo target = new MyWaypointInfo("Target", (Vector3D)receivedMessage.Data);
                                if (!controller.CurrentWaypoint.Equals(target)) controller.ClearWaypoints();
                                controller.ControlThrusters = false;
                                controller.FlightMode = FlightMode.OneWay;
                                controller.AddWaypoint(target);
                                controller.SetAutoPilotEnabled(true);
                            }
                        }
                        else _targetLocked = false;

                    }
                    while (_myBroadcastListenerfire.HasPendingMessage)
                    {
                        MyIGCMessage receivedMessage = _myBroadcastListenerfire.AcceptMessage();
                        if (receivedMessage.Tag == _brodcastTagFire)
                        {
                            if (receivedMessage.Data is string)
                            {
                                timerBlocks[0].TriggerDelay = 1;
                                timerBlocks[0].Silent = true;
                                timerBlocks[0].StartCountdown();
                            }
                        }
                    }
                }
                else
                {
                    _isGroup = false;
                }
                
            }
            WriteInfo();
        }
        const string _buildVersion = "1.4.2";
        const string _buildDate = "3.4.2025";
       public void WriteInfo()
       {
            StringBuilder sb = new StringBuilder();
            sb.Append("===Multi-Grid Targeting System===");
            sb.Append($"\n   (Date: {_buildDate} Version: {_buildVersion})\n\n");
            if (!_isGroup) sb.Append("Error: Missing Group\nCheck Group Name in Custom Data");
            else 
            {
                if (_missingItems.Count > 1) foreach (var item in _missingItems) sb.Append(item.ToString() + '\n');    
                else 
                {
                    if (_targetLocked) sb.Append("              ===Target Locked===");
                    else sb.Append("              ===No Target===");
                }
            }
            Echo(sb.ToString());
       }


    }
}
