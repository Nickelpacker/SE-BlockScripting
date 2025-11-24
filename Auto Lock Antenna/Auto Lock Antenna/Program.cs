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
        // === Space Engineers Rotor Alignment Script ===
        // Rotates a rotor to face a specified GPS point using quaternions for precision.
        //
        // Instructions:
        // 1. Attach this script to a programmable block in your world.
        // 2. Assign the rotor's name and the target GPS coordinates in the Custom Data.
        //
        // Example Custom Data:
        // rotorName=MyRotor
        // targetGPS=12345.6:78901.2:34567.8

        // Configuration
        string rotorName = "MyRotor"; // Default rotor name
        Vector3D targetGPS = new Vector3D(0, 0, 0); // Default target GPS

        IMyMotorStator rotor;

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update1;
            ParseCustomData();

            rotor = GridTerminalSystem.GetBlockWithName(rotorName) as IMyMotorStator;
            if (rotor == null)
            {
                Echo($"Error: Rotor '{rotorName}' not found.");
                return;
            }

            Echo("Rotor Alignment Script Initialized.");
        }

        public void Main(string argument, UpdateType updateSource)
        {
            if (rotor == null)
            {
                Echo("Error: Rotor not configured.");
                return;
            }

            // Get rotor's position and orientation
            var rotorBase = rotor.CubeGrid.WorldMatrix;
            var rotorForward = rotor.WorldMatrix.Forward;
            var rotorPosition = rotor.GetPosition();

            // Calculate the direction to the target GPS point
            var targetDirection = Vector3D.Normalize(targetGPS - rotorPosition);

            // Project the target direction onto the rotor's rotational plane
            var planeNormal = rotorBase.Up; // Rotor's plane of rotation
            var projectedDirection = Vector3D.ProjectOnPlane(ref targetDirection, ref planeNormal);

            // Calculate the angle between the rotor's forward vector and the target direction
            double anglePre = (Vector3D.Angle(rotorForward, projectedDirection)) * (180 / Math.PI);
            double numFullrotations = Math.Floor(anglePre / 360) * 360;
            double angle = anglePre - numFullrotations;
            
            if (Vector3D.Dot(rotor.WorldMatrix.Right, targetDirection) < 0)
            {
                angle = -angle; // Determine the sign of the angle
            }

            // Convert the angle to radians and set the rotor's target velocity
            float desiredVelocity = Math.Sign(angle) * Math.Min((float)Math.Abs(angle) * 10, 1); // Adjust speed multiplier as needed
            rotor.TargetVelocityRad = desiredVelocity;

            Echo($"Target Angle: {angle}°");
            Echo($"Rotor Velocity: {rotor.TargetVelocityRad:F2}");
        }

        void ParseCustomData()
        {
            var lines = Me.CustomData.Split('\n');
            foreach (var line in lines)
            {
                if (line.StartsWith("rotorName="))
                {
                    rotorName = line.Substring("rotorName=".Length).Trim();
                }
                else if (line.StartsWith("targetGPS="))
                {

                }
            }
        }




    }
}
