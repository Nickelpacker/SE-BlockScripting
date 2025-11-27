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
        public IMyCameraBlock _cam;
        public IMyMotorStator _elevation;
        public IMyMotorStator _azimuth;
        public IMyShipController _controller;
        public IMyOffensiveCombatBlock _targetingBlock;
        public IMyFlightMovementBlock _flightBlock;
        Vector3D TARGETPOS;

        public Program()
        {
            _cam = GridTerminalSystem.GetBlockWithName("Camera") as IMyCameraBlock;
            _controller = GridTerminalSystem.GetBlockWithName("Main Cockpit") as IMyShipController;
            _elevation = GridTerminalSystem.GetBlockWithName("Elevation Rotor") as IMyMotorStator;
            _azimuth = GridTerminalSystem.GetBlockWithName("Azimuth Rotor") as IMyMotorStator;
            _targetingBlock = GridTerminalSystem.GetBlockWithName("AI Offensive") as IMyOffensiveCombatBlock;
            _flightBlock = GridTerminalSystem.GetBlockWithName("AI Flight") as IMyFlightMovementBlock;
            Runtime.UpdateFrequency = UpdateFrequency.Update1;
        }

        public void Save()
        {

        }

        public void Main(string argument, UpdateType updateSource)
        {

            if (_flightBlock.CurrentWaypoint == null) TARGETPOS = Vector3D.Zero;
            else
            {
                var v = _flightBlock.CurrentWaypoint.Matrix.GetRow(3);
                TARGETPOS = new Vector3D(v.X, v.Y, v.Z);
            }
            TurnCamera(TARGETPOS, _cam);
        }
        public void TurnCamera(Vector3D targetPos, IMyTerminalBlock _ref, double PGain = 10, double DGain = 2.5f, double MAXRDS = 2 * Math.PI)
        {
            
            Vector3D shipUp = _ref.WorldMatrix.Up;
            Vector3D shipForward = _ref.WorldMatrix.Forward;

            Quaternion Quat = Quaternion.CreateFromForwardUp(shipForward, shipUp);
            Quaternion InvQuat = Quaternion.Inverse(Quat);

            Vector3D DirectionVector = Vector3D.Normalize(targetPos - _ref.GetPosition());
            Vector3D RefFrameVector = Vector3D.Transform(DirectionVector, InvQuat);


            double azi, ele;
            Vector3D.GetAzimuthAndElevation(RefFrameVector, out azi, out ele);

            var refMatrix = MatrixD.CreateWorld(_ref.GetPosition(), shipForward, shipUp).GetOrientation();
            var Vector = Vector3.Transform(new Vector3D(ele, azi, 0), refMatrix);
            
            var angularVel = _controller.GetShipVelocities().AngularVelocity;
            angularVel = Vector3.Transform(angularVel, refMatrix);
            Vector3D angLocal = (Vector3)Vector3D.TransformNormal(angularVel, Matrix.Transpose(_ref.WorldMatrix.GetOrientation()));


            double yawRateCmd = MathHelper.Clamp(azi * PGain - angLocal.Y * DGain, -MAXRDS, MAXRDS);
            double pitchRateCmd = MathHelper.Clamp(((_elevation.Top.WorldMatrix.Forward == _ref.WorldMatrix.Up) ? (float)ele: -(float)ele) * PGain - angLocal.X * DGain, -MAXRDS, MAXRDS);

            _azimuth.TargetVelocityRad = (float)yawRateCmd;
            _elevation.TargetVelocityRad = (float)pitchRateCmd;
            Echo($"Target: {targetPos}\nAzi: {azi:F3}  Ele: {ele:F3}");
        }
    }
}
