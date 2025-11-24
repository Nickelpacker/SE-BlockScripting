using Sandbox.Game.EntityComponents;
using Sandbox.Game.Replication;
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
        #region Program

        // ---- Config ---- //

        public static string CAMERA_TAG = "[MCR]";
        public static string INFO_SCREEN = "Radar LCD";


        // --- Scanning Pattern --- //
        public static double SCAN_DISTANCE_MAX = 2500;
        public static float YAW_MIN_DEG = -30f;
        public static float YAW_MAX_DEG = 30f;
        public static float PITCH_MIN_DEG = -15f;
        public static float PITCH_MAX_DEG = 15f;
        public static float STEP_SIZE = 2.5f;
        // ---- Config End ---- //

        const string version_num = "1.10.4-alpha";
        const string version_date = "8.23.25";
        const string name = "--- Penny's Advanced Radar System ---";


        readonly List<IMyCameraBlock> _cams = new List<IMyCameraBlock>();
        readonly List<float> yaw_steps = new List<float>();
        readonly List<float> pitch_steps = new List<float>();
        public IMyMotorStator azRotor;
        public IMyMotorStator eleRotor;
        public static readonly List<RadarProfile> radarProfiles;
        public class RadarProfile : MyGridProgram
        {
            public float yawMin, YawMax, pitchMin, pitchMax, stepSize;
            public double maxRange;
            public List<float> yawSteps = new List<float>();
            public List<float> pitchSteps = new List<float>();
            public List<IMyCameraBlock> radarCams = new List<IMyCameraBlock>();
            
            public RadarProfile(float yaw_min, float yaw_max, float pitch_min, float pitch_max, float step, double range) 
            {
                yawMin = yaw_min; YawMax = yaw_max;
                pitchMin = pitch_min; pitchMax = pitch_max;
                stepSize = step;
                maxRange = range;
                radarProfiles.Add(this);
 
            }
            public void AssignCams(string tag)
            {
                
                GridTerminalSystem.GetBlocksOfType(radarCams, c => c.CustomName.Contains(tag) && c.IsFunctional);
            }
            public IMyCameraBlock FindBestCam()
            {
                IMyCameraBlock best = null;
                foreach (var cam in radarCams)
                {
                    if (best == null || best.AvailableScanRange < cam.AvailableScanRange) best = cam;
                    else continue;
                }
                return best;
            }
        }

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update1;
            azRotor = GridTerminalSystem.GetBlockWithName("Azimuth Rotor") as IMyMotorStator;
            eleRotor = GridTerminalSystem.GetBlockWithName("Elevation Rotor") as IMyMotorStator;
            Init();
        }
        
        void Init()
        {
            _cams.Clear();
            GridTerminalSystem.GetBlocksOfType(_cams, c => c.IsFunctional && c.IsSameConstructAs(Me) && c.CustomName.Contains(CAMERA_TAG));
            BuildSteps(yaw_steps, YAW_MIN_DEG, YAW_MAX_DEG);
            BuildSteps(pitch_steps, PITCH_MIN_DEG, PITCH_MAX_DEG);
            SetupCustomData(true);
            azRotor.UpperLimitDeg = 360;
            azRotor.LowerLimitDeg = 0;
            eleRotor.UpperLimitDeg = 360;
            eleRotor.LowerLimitDeg = 0;
            foreach (var cam in _cams)
            {
                cam.EnableRaycast = true;
            }
        }
        void Refresh()
        {
            ParseCustomData();
            BuildSteps(yaw_steps, YAW_MIN_DEG, YAW_MAX_DEG);
            BuildSteps(pitch_steps, PITCH_MIN_DEG, PITCH_MAX_DEG);
        }
        void DisplayCamInfo()
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var cam in _cams)
            {
                stringBuilder.AppendLine($"{cam.CustomName} Range: {cam.AvailableScanRange/1000:f1}km");
            }
            IMyTextSurfaceProvider sp = Me;
            var s = sp.GetSurface(0);
            s.ContentType = ContentType.TEXT_AND_IMAGE;
            s.FontSize = 1;
            s.WriteText(stringBuilder.ToString());
        }

        public void Save(){}
        #region Custom Data Tools
        void SetupCustomData(bool checkForWhiteSpace)
        {
            if (checkForWhiteSpace && !string.IsNullOrWhiteSpace(Me.CustomData)) return;
            StringBuilder sb = new StringBuilder();

            //tags
            sb.AppendLine($"{nameof(CAMERA_TAG)}=[MCR]");
            sb.AppendLine($"{nameof(INFO_SCREEN)}=Radar LCD");

            //scanning pattern
            sb.AppendLine($"{nameof(SCAN_DISTANCE_MAX)}=2500");
            sb.AppendLine($"{nameof(YAW_MIN_DEG)}=-30");
            sb.AppendLine($"{nameof(YAW_MAX_DEG)}=30");
            sb.AppendLine($"{nameof(PITCH_MIN_DEG)}=-15");
            sb.AppendLine($"{nameof(PITCH_MAX_DEG)}=15");
            sb.AppendLine($"{nameof(STEP_SIZE)}=2.5");
            Me.CustomData = sb.ToString();
        }
        void ParseCustomData()
        {
            string[] lines = Me.CustomData.Split('\n');
            foreach (string line in lines)
            {
                string[] parts = line.Split('=');
                if (line.Contains("CAMERA_TAG")) CAMERA_TAG = parts[1];
                else if (line.Contains("INFO_SCREEN")) INFO_SCREEN = parts[1];
                else if (line.Contains("SCAN_DISTANCE_MAX")) 
                {
                    double d;
                    double.TryParse(parts[1], out d);
                    SCAN_DISTANCE_MAX = d;
                }
                else if (line.Contains("YAW_MIN_DEG"))
                {
                    float f;
                    float.TryParse(parts[1], out f);
                    YAW_MIN_DEG = f;
                }
                else if (line.Contains("YAW_MAX_DEG"))
                {
                    float f;
                    float.TryParse(parts[1], out f);
                    YAW_MAX_DEG = f;
                }
                else if (line.Contains("PITCH_MIN_DEG"))
                {
                    float f;
                    float.TryParse(parts[1], out f);
                    PITCH_MIN_DEG = f;
                }
                else if (line.Contains("PITCH_MAX_DEG"))
                {
                    float f;
                    float.TryParse(parts[1], out f);
                    PITCH_MAX_DEG = f;
                }
                else if (line.Contains("STEP_SIZE"))
                {
                    float f;
                    float.TryParse(parts[1], out f);
                    STEP_SIZE = f;
                }

            }
        }
        #endregion
        public MyDetectedEntityInfo hitInfo;
        public void Main(string argument, UpdateType updateSource)
        {
            trackingp
            IMyFlightMovementBlock f;

            IMyAutopilotWaypoint q;
            q.Matrix.get

            switch (argument)
            {
                case "refresh": Refresh(); break;
                case "reset": SetupCustomData(false); break;
                case "target": ScanForTarget(); break;
            }
            EchoInfo();
            DisplayCamInfo();
            
        }
        public void CalibrateRotor(IMyMotorStator rotor)
        {

        }
        public void EchoInfo()
        {
            StringBuilder sb = new StringBuilder();
            
            
            sb.AppendLine("--- Penny's Advanced Radar System ---");
            sb.AppendLine($"{String.Concat(Enumerable.Repeat(" ", name.Length-(version_date.Length+version_num.Length+9)))}~~{version_num}~~  ~~{version_date}~~");
            sb.AppendLine($"Step Size: {STEP_SIZE}");

            Echo(sb.ToString());
        }
        public void ScanForTarget()
        {
            for (int i = 0; i < pitch_steps.Count; i++)
            {
                for (int j = 0; j < yaw_steps.Count; j++)
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    IMyCameraBlock bestCam = FindBest();
                    IMyTextPanel lcd = GridTerminalSystem.GetBlockWithName(INFO_SCREEN) as IMyTextPanel;
                    stringBuilder.AppendLine($"Cam: {bestCam.CustomName}");
                    stringBuilder.AppendLine($"Scan Range: {bestCam.AvailableScanRange}m");
                    stringBuilder.AppendLine($"Pitch: {pitch_steps[i]}\nYaw: {yaw_steps[j]}");
                    if (bestCam.CanScan(SCAN_DISTANCE_MAX))
                    {
                        hitInfo = bestCam.Raycast(SCAN_DISTANCE_MAX, pitch_steps[i], yaw_steps[j]);
                        if (!hitInfo.IsEmpty() && (hitInfo.Type == MyDetectedEntityType.SmallGrid || hitInfo.Type == MyDetectedEntityType.LargeGrid))
                        {
                            DriveRotorVelocity(azRotor, (float)GetDesiredYaw(azRotor, (Vector3D)hitInfo.HitPosition), 5);
                            DriveRotorVelocity(eleRotor, (float)GetDesiredYaw(eleRotor, (Vector3D)hitInfo.HitPosition), 1);
                            stringBuilder.AppendLine($"Target Hit: {Vector3D.Distance((Vector3D)hitInfo.HitPosition, bestCam.GetPosition()):f0}m");
                            lcd.WriteText(stringBuilder.ToString());
                            return;
                        }
                    }
                    else
                    {
                        lcd.WriteText(stringBuilder.ToString());
                        return;
                    }
                }
            }
        }
        public IMyCameraBlock FindBest()
        {
            IMyCameraBlock best = null;
            foreach (var cam in _cams)
            {
                if (best == null || best.AvailableScanRange < cam.AvailableScanRange) best = cam;
                else continue;
            }

            return best;
        }
        #region To Degree Functions
        void ToDegrees(ref float angle)
        {
            angle = (float)(angle * (180 / Math.PI));
        
        }
        void ToDegrees(ref double angle)
        {
            angle = (angle * (180 / Math.PI));
        }
        double ToDegrees(ref double angle, bool echo)
        {
            return angle = (angle * (180 / Math.PI));
        }
        float ToDegrees(ref float angle, bool echo)
        { 
            
            return angle = (float)(angle * (180 / Math.PI));

        }
        #endregion
        void DriveRotorVelocity(IMyMotorStator rotor, float deltaRad, float? speed)
        {
            double R360 = 2*Math.PI;
            double R180 = Math.PI;
            double R0 = 0;

            double currRad = rotor.Angle;
            double desiredAbsRad = currRad - deltaRad;
            desiredAbsRad += R180;
            while (desiredAbsRad > R360) desiredAbsRad -= R360;
            while (desiredAbsRad < R0) desiredAbsRad += R360;

            if (desiredAbsRad < currRad)
            {
                rotor.UpperLimitRad = (float)R360;
                rotor.LowerLimitRad = (float)desiredAbsRad;
                rotor.TargetVelocityRPM = (speed != null) ? -(float)speed : -3;
            }
            if (desiredAbsRad > currRad)
            {
                rotor.UpperLimitRad = (float)desiredAbsRad;
                rotor.LowerLimitRad = (float)R0;
                rotor.TargetVelocityRPM = (speed != null) ? (float)speed : 3;
            }
       
            Echo($"{rotor.CustomName} Desired Angle: {ToDegrees(ref desiredAbsRad, true):f1}");
        }

        void ResetRotor(IMyMotorStator rotor)
        {
            rotor.RotorLock = false;
            rotor.TargetVelocityRPM = 0;
            rotor.LowerLimitDeg = -361f;
            rotor.UpperLimitDeg = 361f;
        }
        const double TAU = Math.PI * 2.0;
        float GetDesiredYaw(IMyMotorStator rotor, Vector3D targ)
        {
            var top = rotor.Top as IMyMotorRotor; if (top == null) return 0f;

            Vector3D U = rotor.WorldMatrix.Up;        
            Vector3D Z0 = rotor.WorldMatrix.Backward;  
            Vector3D O = top.WorldMatrix.Translation;

 
            Z0 -= U * Vector3D.Dot(Z0, U);
            if (Z0.LengthSquared() < 1e-8) return 0f;
            Z0.Normalize();

            Vector3D vT = targ - O;
            vT -= U * Vector3D.Dot(vT, U);
            if (vT.LengthSquared() < 1e-8) return 0f;
            vT.Normalize();

            Vector3D vF = top.WorldMatrix.Forward;
            vF -= U * Vector3D.Dot(vF, U);
            vF.Normalize();

            double aT = Math.Atan2(Vector3D.Dot(Vector3D.Cross(Z0, vT), U), Vector3D.Dot(Z0, vT));
            double aC = Math.Atan2(Vector3D.Dot(Vector3D.Cross(Z0, vF), U), Vector3D.Dot(Z0, vF));
            double delta = aT - aC;
            while (delta > Math.PI) delta -= TAU;
            while (delta <= -Math.PI) delta += TAU;
            return (float)delta;   // radians
        }



        void BuildSteps(List<float> steps, float minDeg, float maxDeg)
        {
            steps.Clear();
            if (STEP_SIZE <= 0f) return;
            if (minDeg > maxDeg) { var temp = minDeg; minDeg = maxDeg; maxDeg = temp; }
            double span = maxDeg - minDeg;
            int intervals = (int)Math.Round(span/STEP_SIZE);
            for (int i = 0; i <= intervals; i++) steps.Add((i == intervals) ? maxDeg : (float)(minDeg + i * STEP_SIZE));          
        }
        #endregion
    }
}
