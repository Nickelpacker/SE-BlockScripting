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
        const float _defaultPosition = 0;
        public int i;

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update1;
        }



        public void Main(string argument, UpdateType updateSource)
        {
            IMyMotorStator rotor = GridTerminalSystem.GetBlockWithName("Elevation") as IMyMotorStator;
            IMyBlockGroup connectors = GridTerminalSystem.GetBlockGroupWithName("Loading Connectors");
            List<IMyShipConnector> shipConnectors = new List<IMyShipConnector>();
            connectors.GetBlocksOfType(shipConnectors, connector => connector.IsFunctional);

            if (argument.ToLower() == "reload" | i == 1)
            {
                i = 1;
                SetRotorTargetAngle(rotor, _defaultPosition);
                if (NormalizeAngle(rotor.Angle) == _defaultPosition)
                {
                    for (int j = 0; j < 360; j++)
                    {
                        foreach (IMyShipConnector block in shipConnectors)
                        {
                            block.Connect();
                        }
                        if (j == 360)
                        {
                            foreach(IMyShipConnector block in shipConnectors) block.Disconnect();
                            rotor.RotorLock = false;
                        }
                    }                    
                }
            }
        }
        void SetRotorTargetAngle(IMyMotorStator rotor, float targetAngle)
        {
            double currentAngle = NormalizeAngle(rotor.Angle);
            double angleDiff = NormalizeAngle(targetAngle - currentAngle);

            // Smooth rotation speed
            double speed = MathHelper.Clamp(angleDiff * 10, -2, 2);
            rotor.TargetVelocityRad = (float)speed;
        }
        double NormalizeAngle(double angle)
        {
            while (angle > Math.PI) angle -= 2 * Math.PI;
            while (angle < -Math.PI) angle += 2 * Math.PI;
            return angle;
        }
    }
}
