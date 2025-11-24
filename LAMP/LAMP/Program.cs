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
        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update1; // Updates every tick (~16.67ms)
        }

        // Block Names
        const string AzimuthRotorName = "Azimuth Rotor";
        const string ElevationRotorName = "Elevation Rotor";
        const string CameraName = "Turret Camera";

        // Target GPS (Modify as needed)
        Vector3D targetGps = new Vector3D(1000, 2000, 3000);

        IMyMotorStator azimuthRotor;
        IMyMotorStator elevationRotor;
        IMyCameraBlock camera;

        public void Main(string argument, UpdateType updateSource)
        {
            // Retrieve blocks
            azimuthRotor = GridTerminalSystem.GetBlockWithName(AzimuthRotorName) as IMyMotorStator;
            elevationRotor = GridTerminalSystem.GetBlockWithName(ElevationRotorName) as IMyMotorStator;
            camera = GridTerminalSystem.GetBlockWithName(CameraName) as IMyCameraBlock;

            if (azimuthRotor == null || elevationRotor == null || camera == null)
            {
                Echo("ERROR: One or more blocks not found!");
                return;
            }

            // Turret base position (assume azimuth rotor base as reference)
            Vector3D turretBasePosition = azimuthRotor.GetPosition();

            // Direction to target
            Vector3D directionToTarget = Vector3D.Normalize(targetGps - turretBasePosition);

            // Azimuth calculation
            MatrixD azimuthMatrix = azimuthRotor.WorldMatrix;
            Vector3D azimuthForward = azimuthMatrix.Forward;
            Vector3D azimuthRight = azimuthMatrix.Right;
            double azimuthTarget = Math.Atan2(directionToTarget.Dot(azimuthRight), directionToTarget.Dot(azimuthForward));

            // Elevation calculation
            double elevationTarget = CalculateElevationAngle(turretBasePosition, targetGps, camera.WorldMatrix);

            // Adjust rotors
            SetRotorTargetAngle(azimuthRotor, azimuthTarget);
            SetRotorTargetAngle(elevationRotor, elevationTarget);

            // Debug information
            Echo($"Target GPS: {targetGps}");
            Echo($"Azimuth Target: {MathHelper.ToDegrees(azimuthTarget):0.00}°");
            Echo($"Elevation Target: {MathHelper.ToDegrees(elevationTarget):0.00}°");
            Echo($"Current Azimuth: {MathHelper.ToDegrees(NormalizeAngle(azimuthRotor.Angle)):0.00}°");
            Echo($"Current Elevation: {MathHelper.ToDegrees(NormalizeAngle(elevationRotor.Angle)):0.00}°");
        }

        // Calculate elevation angle
        double CalculateElevationAngle(Vector3D turretPosition, Vector3D targetPosition, MatrixD turretOrientation)
        {
            // Vector from turret to target
            Vector3D directionToTarget = Vector3D.Normalize(targetPosition - turretPosition);

            // Turret's forward vector
            Vector3D turretForward = turretOrientation.Forward;

            // Quaternion representing rotation from turret forward to target direction
            Quaternion rotationQuat = Quaternion.CreateFromTwoVectors(turretForward, directionToTarget);

            // Convert quaternion to Euler angles
            Vector3D eulerAngles = QuaternionToEuler(rotationQuat);

            // Elevation angle corresponds to pitch (rotation around X-axis)
            double elevationAngle = eulerAngles.X;

            return elevationAngle;
        }

        // Convert quaternion to Euler angles
        Vector3D QuaternionToEuler(Quaternion q)
        {
            // Roll (X-axis rotation)
            double sinr_cosp = 2 * (q.W * q.X + q.Y * q.Z);
            double cosr_cosp = 1 - 2 * (q.X * q.X + q.Y * q.Y);
            double roll = Math.Atan2(sinr_cosp, cosr_cosp);

            // Pitch (Y-axis rotation)
            double sinp = 2 * (q.W * q.Y - q.Z * q.X);
            double pitch;
            if (Math.Abs(sinp) >= 1)
                pitch = CopySign(Math.PI / 2, sinp); // Use 90 degrees if out of range
            else
                pitch = Math.Asin(sinp);

            // Yaw (Z-axis rotation)
            double siny_cosp = 2 * (q.W * q.Z + q.X * q.Y);
            double cosy_cosp = 1 - 2 * (q.Y * q.Y + q.Z * q.Z);
            double yaw = Math.Atan2(siny_cosp, cosy_cosp);

            return new Vector3D(roll, pitch, yaw);
        }

        // Custom CopySign function
        double CopySign(double magnitude, double signSource)
        {
            return Math.Abs(magnitude) * Math.Sign(signSource);
        }

        // Adjust rotor towards target angle smoothly
        void SetRotorTargetAngle(IMyMotorStator rotor, double targetAngle)
        {
            double currentAngle = NormalizeAngle(rotor.Angle);
            double angleDiff = NormalizeAngle(targetAngle - currentAngle);

            // Smooth rotation speed
            double speed = MathHelper.Clamp(angleDiff * 10, -2, 2);
            rotor.TargetVelocityRad = (float)speed;

            // Stop rotor if angle difference is minimal
            if (Math.Abs(angleDiff) < 0.01)
            {
                rotor.TargetVelocityRad = 0;
                rotor.RotorLock = true;
            }
            else
            {
                rotor.RotorLock = false;
            }
        }

        // Normalize angles between -π and π
        double NormalizeAngle(double angle)
        {
            while (angle > Math.PI) angle -= 2 * Math.PI;
            while (angle < -Math.PI) angle += 2 * Math.PI;
            return angle;
        }



    }
}
