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
         *          Ship Alarm and Lights System
         *                     SALS
         *  Create a group named Lights and edit the following variables to what you would like them to be set too
         *  or you could name one light as the template by giving it the name "[SALS] Light Main"
         * 
         */
             // CONFIG //
        public bool i;
        public bool isSoundblocks;
        public bool isLights;
        const float intensity = 6.0f;
        const float radius = 5.5f;
        const float falloff = 1.0f;
        const int r = 255;
        const int g = 164;
        const int b = 0;

        const int defRange = 50;
        const int defVolume = 100;
        // DO NOT EDIT BELOW //



        public string alarmInfo;
        
        public float savedIntensity;
        public float savedRadius;
        public float savedFalloff;
        public Color savedColor;
        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update10;
        }
        public void Main(string argument, UpdateType updateSource)
        {
            
            IMyBlockGroup Lgroup = GridTerminalSystem.GetBlockGroupWithName("Lights");
            IMyLightingBlock lightTemp = GridTerminalSystem.GetBlockWithName("[SALS] Light Main") as IMyLightingBlock;
            IMyBlockGroup Agroup = GridTerminalSystem.GetBlockGroupWithName("Alarm");
            
            List<IMyLightingBlock> lights = new List<IMyLightingBlock>();
            List<IMySoundBlock> sounds = new List<IMySoundBlock>(); 
            Lgroup.GetBlocksOfType(lights, light => light.IsFunctional);
            Agroup.GetBlocksOfType(sounds, sound => sound.IsFunctional);
            if (Lgroup != null)
            {
                isLights = true;
                foreach (var block in lights)
                {
                    if (argument.ToLower() == "alarm")
                    {
                        if (lightTemp != null)
                        {
                            savedIntensity = block.Intensity;
                            savedRadius = block.Radius;
                            savedFalloff = block.Falloff;
                            savedColor = block.Color;
                        }
                        i = true;
                        block.Color = Color.DarkRed;
                        block.BlinkIntervalSeconds = 2;
                        block.BlinkLength = 60f;
                        alarmInfo = "Alarm Active";
                    }
                    if (argument.ToLower() == "end_alarm" & i)
                    {
                        lightTemp.Color = savedColor;
                        lightTemp.Intensity = savedIntensity;
                        lightTemp.Radius = savedRadius;
                        lightTemp.Falloff = savedFalloff;
                        i = false;
                        alarmInfo = "Alarm Inactive";
                    }
                    if (argument.ToLower() == "reset")
                    {
                        savedIntensity = intensity;
                        savedRadius = radius;
                        savedFalloff = falloff;
                        savedColor = new VRageMath.Color(r, g, b);
                        block.Intensity = intensity;
                        block.Radius = radius;
                        block.Falloff = falloff;
                        block.Color = new VRageMath.Color(r, g, b);
                        block.BlinkIntervalSeconds = 0;
                        block.BlinkLength = 0f;
                        
                    }
                    if (i != true & lightTemp != null)
                    {
                        block.Intensity = lightTemp.Intensity;
                        block.Radius = lightTemp.Radius;
                        block.Falloff = lightTemp.Falloff;
                        block.Color = lightTemp.Color;
                        block.BlinkIntervalSeconds = 0;
                        block.BlinkLength = 0f;
                    }
                    else
                    {
                        if (i != true)
                        {
                            block.Intensity = intensity;
                            block.Radius = radius;
                            block.Falloff = falloff;
                            block.Color = new VRageMath.Color(r, g, b);
                            block.BlinkIntervalSeconds = 0;
                            block.BlinkLength = 0f;
                        }
                    }
                }
            }
            else {isLights = false;}
           
            if (Agroup != null) 
            { 
                isSoundblocks = true;
                foreach (var block in sounds)
                {
                    if (argument.ToLower() == "alarm")
                    {
                        block.Range = 500;
                        block.LoopPeriod = 1800;
                        block.Volume = 100;
                        block.SelectedSound = "Alert 2";
                        block.Play();
                    }
                    if (argument.ToLower() == "end_alarm")
                    {
                        block.Stop();
                        block.Range = defRange;
                        block.Volume = defVolume;
                    }
                }
            }
            else { isSoundblocks = false; }
            Echo($"=========== SALS ===========\nLights Detected: {isLights}\nAlarm Sound System Detected: {isSoundblocks}\n\n              {alarmInfo}");
        }
        //
    }
}