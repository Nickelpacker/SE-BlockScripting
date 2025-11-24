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
        //config
        const bool _doBlink = true;
        const string _prefixLight = "[EWS]";
        const float _lightRadius = 5;
        const float _lightIntensity = 1;
        const float _lightFalloff = 1;
        const float _lightBlinkLength = 2;
        const float _lightBlinkInterval = 35;
        Color _warningColor = Color.DarkRed;
        

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update1;
        }

        public void Main(string argument, UpdateType updateSource)
        {
            IMyBlockGroup lights = GridTerminalSystem.GetBlockGroupWithName("Warning Lights");
            List<IMyInteriorLight> warningLights = new List<IMyInteriorLight>();
            lights.GetBlocksOfType(warningLights, light => light.IsFunctional);
            IMyLargeGatlingTurret _targetingTurret = GridTerminalSystem.GetBlockWithName("[EWS] Targeting Turret") as IMyLargeGatlingTurret;
            foreach (IMyInteriorLight block in warningLights) if (!block.CustomName.Contains(_prefixLight)) block.CustomName = $"{_prefixLight} {block.CustomName}";
            if (_targetingTurret.HasTarget)
            {
                Echo("has target");
                foreach (IMyInteriorLight light in warningLights)
                {
                    bool isBlink = _doBlink;
                    light.Enabled = true;
                    light.Color = _warningColor;
                    light.Radius = _lightRadius;
                    light.Falloff = _lightFalloff;
                    light.Intensity = _lightIntensity;
                    if (isBlink)
                    {
                        light.BlinkLength = _lightBlinkLength;
                        light.BlinkIntervalSeconds = _lightBlinkInterval;
                    }
                    if (!isBlink)
                    {
                        light.BlinkIntervalSeconds = 0;
                        light.BlinkIntervalSeconds = 0;
                    }
                }
            }
        }
    }
}
