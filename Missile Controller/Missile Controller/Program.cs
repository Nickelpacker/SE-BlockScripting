using EmptyKeys.UserInterface.Generated;
using EmptyKeys.UserInterface.Generated.DataTemplatesContracts_Bindings;
using EmptyKeys.UserInterface.Generated.PlantManagementView_Bindings;
using Sandbox.Game.Screens;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using VRage;
using VRage.Game.ModAPI.Ingame;
using VRageMath;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        #region Script
        //Script Info

        /* Although this script is partially function, it is nowhere near its final stage.
         * I plan for many more features to be added in the future. (LIST CAN BE FOUND IN DESCRIPTION)
         * 
         * Commands:
         * ("launch") If the number of missiles is null it will by default launch one.
         * ("refresh") refetches custom data and gets refreshes missiles
         * 
         * 
         * 
         */

        public const string _date = "11.27.25";
        public const string _version = "0.4.0";


        public static string missileTag = "#A#";
        public string launchInfo;

        //Guidance Blocks
        public static IMyFlightMovementBlock _flightControl;
        public IMyOffensiveCombatBlock _targettingBlock;

        //Optional Blocks
        public IMySoundBlock ALARM_sound;
        public bool Has_Sound;

        public IMyLightingBlock ALARM_light;
        public bool Has_Light;

        public bool ALARM_active;
        public bool RELAY_active;


        public List<Missile> MISSILES = new List<Missile>();
        public static int ACTIVEMISSILES;
        public int MissileRemaining;
        public int MergeRefresh;
        
        public static MyGridProgram Program_Local;
        public static CustomDataConfig cfg;
        public static RelayModule relay;




        public bool haveWaypoint;
        public Program()
        {
            Me.CustomName = "Missile Control Script";
            Runtime.UpdateFrequency = UpdateFrequency.Update1;
            

            
            MergeRefresh = 0;

            Program_Local = this;

            CustomDataConfig.Fix();
            cfg = new CustomDataConfig(Me.CustomData);
            missileTag = cfg.tag;
            RELAY_active = cfg.IsRelay;
            if (RELAY_active) relay = new RelayModule(cfg.networkName);
            
            _targettingBlock = GridTerminalSystem.GetBlockWithName("AI Targetting Control") as IMyOffensiveCombatBlock;
            _flightControl = GridTerminalSystem.GetBlockWithName("AI Flight Control") as IMyFlightMovementBlock;
            
            ALARM_sound = GridTerminalSystem.GetBlockWithName("Lock Sound Indicator") as IMySoundBlock;
            Has_Sound = ALARM_sound != null;
            
            ALARM_light = GridTerminalSystem.GetBlockWithName("Lock Light Indicator") as IMyLightingBlock;
            Has_Light = ALARM_light != null;

            SETUP_OptionalBlocks();
            SETUP_ControlBlocks();
        }
        public void Refresh()
        {
            _targettingBlock = GridTerminalSystem.GetBlockWithName("AI Targetting Control") as IMyOffensiveCombatBlock;
            _flightControl = GridTerminalSystem.GetBlockWithName("AI Flight Control") as IMyFlightMovementBlock;
            ALARM_sound = GridTerminalSystem.GetBlockWithName("Lock Sound Indicator") as IMySoundBlock;
            Has_Sound = ALARM_sound != null;
            ALARM_light = GridTerminalSystem.GetBlockWithName("Lock Light Indicator") as IMyLightingBlock;
            Has_Light = ALARM_light != null;

            cfg = new CustomDataConfig(Me.CustomData);
            MergeRefresh = 0;
            missileTag = cfg.tag;
            List<IMyRadioAntenna> temp = new List<IMyRadioAntenna>();
            GridTerminalSystem.GetBlocksOfType(temp, t => t.IsFunctional);
            RELAY_active = (temp.Count > 0) && cfg.IsRelay;
            if (RELAY_active) relay = new RelayModule(cfg.networkName);
            
            SETUP_OptionalBlocks();
            SETUP_ControlBlocks();
        }
        public void Main(string argument, UpdateType updateSource)
        {
            if (_targettingBlock == null || _flightControl == null)
            {
                Echo("ERROR: Missing Targetting and Flight Blocks");
                return;
            }

            if (MergeRefresh > 120)
            {
                List<IMyShipMergeBlock> mergeBlocks = new List<IMyShipMergeBlock>();
                GridTerminalSystem.GetBlocksOfType(mergeBlocks, b => b.CustomName.Contains(missileTag));
                MissileRemaining = mergeBlocks.Count;
                MergeRefresh = 0;
            }
            MergeRefresh++;
            Vector3D targetPos = GetMissileTarget();
            ACTIVEMISSILES = MISSILES.Count;

            Alarm(haveWaypoint);

            //Arguments
            if (argument.ToLower().Trim() == "launch" && haveWaypoint) INIT_NEXT_MISSILE();

            if (argument.ToLower().Trim() == "refresh")
            {
                Refresh();
            }
            if (argument.ToLower().Trim() == "fix") CustomDataConfig.Fix();

            if (argument.ToLower().Trim() == "dispose") DisposeAll();


            for (int i = 0; i < MISSILES.Count; i++)
            {
                Missile thisMissile = MISSILES[i];
                thisMissile.TARGETPOS = targetPos;
                thisMissile.Check_Detonate();
                thisMissile.Run_Guidance();

                if (thisMissile.RequireDispose())
                {
                    thisMissile.Force_Detonate();
                    MISSILES.Remove(thisMissile);
                }
            }
            relay.Main();
            EchoInfo();
        }
        #region Unsorted
        public void DisposeAll()
        {
            for (int i = 0; i < MISSILES.Count; i++)
            {
                var missile = MISSILES[i];
                missile.Force_Detonate();
                MISSILES.Remove(missile);
            }
        }
        Vector3D GetMissileTarget()
        {
            Vector3D target = Vector3D.Zero;
            if (_flightControl.CurrentWaypoint != null)
            {
                var translation = _flightControl.CurrentWaypoint.Matrix.GetRow(3);
                target = new Vector3D(translation.X, translation.Y, translation.Z);

                haveWaypoint = true;
                relay.SendTransmission(target);
                
                return target;
            }
            else
            {
                haveWaypoint = false;
                return target;
            }
        }
        void Alarm(bool isActive)
        {
            if (isActive)
            {
                if (Has_Light)
                {
                    ALARM_light.Color = Color.Red;
                    ALARM_light.BlinkIntervalSeconds = 1;
                }

                if (!ALARM_active)
                {

                    if (Has_Sound) ALARM_sound.Play();
                    ALARM_active = true;
                }
            }
            else
            {
                if (Has_Light)
                {
                    ALARM_light.Color = Color.Green;
                    ALARM_light.BlinkIntervalSeconds = 0;
                }

                if (Has_Sound) ALARM_sound.Stop();
                ALARM_active = false;
            }
        }
        void SETUP_ControlBlocks()
        {
            _targettingBlock.Enabled = true;
            _targettingBlock.UpdateTargetInterval = 4;
            _targettingBlock.SelectedAttackPattern = 3;
            _targettingBlock.TargetPriority = OffensiveCombatTargetPriority.Largest;
            _targettingBlock.SearchEnemyComponent.TargetingLockOptions = MyGridTargetingRelationFiltering.Enemy;
            _targettingBlock.SetValue<long>("OffensiveCombatIntercept_GuidanceType", 0);
            _targettingBlock.SetValue<long>("TargetingGroup", 2);
            _targettingBlock.ApplyAction("ActivateBehavior_On");

            _flightControl.Enabled = false;
            _flightControl.MinimalAltitude = 10;
            _flightControl.PrecisionMode = false;
            _flightControl.SpeedLimit = 400;
            _flightControl.AlignToPGravity = false;
            _flightControl.CollisionAvoidance = false;
            _flightControl.ApplyAction("ActivateBehavior_On");

        }
        void SETUP_OptionalBlocks()
        {
            if (ALARM_light != null)
            {
                ALARM_light.Enabled = true;
                ALARM_light.Radius = 3f;
                ALARM_light.BlinkIntervalSeconds = 0;
                ALARM_light.BlinkLength = 50;
            }
            if (ALARM_sound != null)
            {
                ALARM_sound.Range = 25;
                ALARM_sound.Volume = 100;
                ALARM_sound.SelectedSound = "Alert 1";
                ALARM_sound.LoopPeriod = 1800;
            }
        }
        bool INIT_NEXT_MISSILE()
        {
            List<IMyShipMergeBlock> MERGES = new List<IMyShipMergeBlock>();
            GridTerminalSystem.GetBlocksOfType(MERGES, b => b.CustomName.Contains(missileTag));

            List<IMyTerminalBlock> MissileGridView = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType(MissileGridView, b => b.CustomName.Contains(missileTag));
            if (MERGES.Count < 1)
            {
                launchInfo = "Error: Missile Failed To Fire\nCause: No Missiles Found";
                return false;
            }
            else
            {
                MISSILES.Add(new Missile(MERGES[0], MissileGridView));
                MissileRemaining -= 1;
                return true;
            }
        }
        #endregion

        public class RelayModule
        {
            private readonly string IDENTIFIER = "";
            private readonly IMyBroadcastListener RECIEVER;
            private readonly IMyIntergridCommunicationSystem IGC = Program_Local.IGC;
            private List<Missile> MISSILES = new List<Missile>();

            private DataPoint receivedData;
            private struct DataPoint
            {
                public Vector3D transmitterPos;
                public Vector3D contactPos;
                public double contactDistance;
                public DataPoint(Vector3D targPos)
                {
                    transmitterPos = Program_Local.Me.GetPosition(); 
                    contactPos = targPos; 
                    contactDistance = Vector3D.Distance(targPos, transmitterPos);
                }
            }
            public RelayModule(string network)
            {
                IDENTIFIER = network;
                RECIEVER = IGC.RegisterBroadcastListener(IDENTIFIER);
            }
            private bool CheckForTransmission(ref DataPoint data)
            {
                if (RECIEVER.HasPendingMessage)
                {
                    MyTuple<Vector3D, Vector3D, double> dataT = new MyTuple<Vector3D, Vector3D, double>();
                    MyIGCMessage msg = RECIEVER.AcceptMessage();
                    dataT = (MyTuple<Vector3D, Vector3D, double>)msg.Data;
                    data.transmitterPos = dataT.Item1;
                    data.contactPos = dataT.Item2;
                    data.contactDistance = dataT.Item3;
                    
                    return true;
                }
                else 
                {
                    return false;
                }
            }
            public void Main()
            {
                DataPoint data = new DataPoint();
                if (CheckForTransmission(ref data))
                {
                    receivedData = data;
                    RunMissiles();
                    if (ACTIVEMISSILES + MISSILES.Count >= 2) return;
                    
                    List<IMyShipMergeBlock> merges = new List<IMyShipMergeBlock>();
                    Program_Local?.GridTerminalSystem.GetBlocksOfType(merges, m => m.CustomName.Contains(missileTag));

                    List<IMyTerminalBlock> MissileGridView = new List<IMyTerminalBlock>();
                    Program_Local?.GridTerminalSystem.GetBlocksOfType(MissileGridView, b => b.CustomName.Contains(missileTag));
                    if (merges.Count < 1) return;
                    MISSILES.Add(new Missile(merges[0], MissileGridView));
                }                
            }
            private void RunMissiles()
            {
                if (MISSILES.Count == 0) return;
                for (int i = 0; i < MISSILES.Count; i++)
                {
                    Missile thisMissile = MISSILES[i];
                    thisMissile.TARGETPOS = receivedData.contactPos;
                    thisMissile.Check_Detonate();
                    thisMissile.Run_Guidance();

                    if (thisMissile.RequireDispose())
                    {
                        thisMissile.Force_Detonate();
                        MISSILES.Remove(thisMissile);
                    }
                }
            }
            public void SendTransmission(Vector3D targetPos)
            {
                DataPoint data = new DataPoint(targetPos);
                MyTuple<Vector3D, Vector3D, double> dataT = new MyTuple<Vector3D, Vector3D, double>
                {
                    Item1 = data.transmitterPos,
                    Item2 = data.contactPos,
                    Item3 = data.contactDistance
                };

                    
                IGC.SendBroadcastMessage(IDENTIFIER, dataT, TransmissionDistance.AntennaRelay);
            }           
        }
        public class CustomDataConfig
        {
            private readonly string CONFIGSTRING;
            private readonly string[] CONFIGARR;
            public float dmsDistance;
            public float proxDistance;
            
            public float gravAggression;
            public float PNGain;

            public bool IsRelay;
            public string networkName;

            public string tag;
            
            public CustomDataConfig(string configStr)
            {
                CONFIGSTRING = configStr;
                CONFIGARR = configStr.Split('\n');
                List<string> lines = new List<string>();
                lines = CONFIGARR.ToList();
                for(int i  = 0; i < lines.Count; i++)
                {
                    var line = lines[i];
                    if(string.IsNullOrWhiteSpace(line)) lines.Remove(line);
                }
                ParseFull(lines);
            }
            void ParseFull(List<string> lines)
            {
                foreach (var line in lines)
                {
                    ParseLine(line);
                }
            }
            public static void Fix()
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"Tag=#A#\n");
                sb.AppendLine("#Detonation Variables:");
                sb.AppendLine($"DMS=15.0");
                sb.AppendLine($"PROX=2.0");
                sb.AppendLine($"\n#Guidance Variables:");
                sb.AppendLine($"Gravity Aggression=2.5");
                sb.AppendLine($"\n#Communication Variables:");
                sb.AppendLine($"Active Relay=True");
                sb.AppendLine($"Network Name=#RELOC#");
                //sb.AppendLine($"PN Gain=4.0");
                Program_Local.Me.CustomData = sb.ToString();
            }
            private void ParseLine(string line)
            {
                if (!line.Contains('=') || line.TrimStart().StartsWith("#")) return;
                string[] lineArr = line.Split('=');
                string name = lineArr[0].ToLower();
                string value = lineArr[1].Trim();
                switch(name)
                {
                    case "tag":
                        tag = value; break;
                    //case "pn gain":
                        //PNGain = float.Parse(value); break;
                    case "dms":
                        dmsDistance = float.Parse(value); break;
                    case "prox":
                        proxDistance = float.Parse(value); break;
                    case "gravity aggression":
                        gravAggression = float.Parse(value); break;
                    case "active relay":
                        IsRelay = Boolean.Parse(value); break;
                    case "network name":
                        networkName = value; break;
                    default:
                        break;
                }
            }
        }
        public class Missile
        {
            #region Missile Variables
            //Missile Root
            public IMyShipMergeBlock MERGE;

            //Vital Missile Blocks
            public List<IMyThrust> THRUSTERS = new List<IMyThrust>();
            public IMyGyro GYRO;
            public IMyShipController REMOTE;
            
            //Optional Missile Blocks
            public List<IMyWarhead> WARHEADS = new List<IMyWarhead>(); //recommend warheads
            public List<IMyBatteryBlock> BATTERIES = new List<IMyBatteryBlock>();
            public IMyShipConnector CONNECTOR;
            public List<IMyGasTank> TANKS = new List<IMyGasTank>();

            //Guidance Variables
            public float THRUSTPERCENTAGE;
            public Vector3D TARGETPOS;
            public Vector3D AIMPOINT;
            public bool ISTRACKING;

            //Detonation Variables
            private readonly float DMS_Distance = 25;
            private readonly float PROX_Distance = 1.5f;

            //Constant Variables
            public double PNGain
            { 
                get
                {
                    if (TARGETDISTANCE < 500) return MathHelper.Lerp(4.0, 1.0, (500 - TARGETDISTANCE) / 500);
                    else return 4.5;
                }
            }
            readonly double MinThrustPct = .75f;
            readonly double GravityAggressiveness;

            //Launch Variables
            //const int IGNITE_DELAY = 5;
            const int GUIDANCE_DELAY = 45;
            public int LAUNCH_TIMER = 0;

            //Other Variables
            List<IMyTerminalBlock> GridViewAtLaunch = new List<IMyTerminalBlock>();

            public bool IsConnected;
            public double PREV_PITCH;
            public double PREV_YAW;

            Vector3D MIS_PREV_POS = Vector3D.Zero;
            Vector3D TARGET_PREV_POS = Vector3D.Zero;
            bool _havePrev = false;
            public bool Active = false;


            public double TARGETDISTANCE
            {
                get { return Vector3D.Distance(TARGETPOS, WARHEADS[0].GetPosition()); }
            }

            public Vector3D MISSILE_VEL
            {
                get { return REMOTE.GetShipVelocities().LinearVelocity; }
            }

            public double MAXTHRUST
            {
                get 
                {
                    float maxThrust = 0;
                    foreach (var thruster in THRUSTERS)
                    {
                        maxThrust += thruster.MaxThrust;
                    }
                    return maxThrust / Math.Max(1.0, (REMOTE?.CalculateShipMass().TotalMass ?? 1.0)); 
                }
            }
            #endregion
            

            //Missile Boot
            public Missile(IMyShipMergeBlock mergeBlock, List<IMyTerminalBlock> gridViewAtLaunch)
            {
                //PNGain = cfg.PNGain;
                GravityAggressiveness = cfg.gravAggression;
                DMS_Distance = cfg.dmsDistance;
                PROX_Distance = cfg.proxDistance;
                
                MERGE = mergeBlock;
                GridViewAtLaunch = gridViewAtLaunch;
                MERGE.Enabled = false;


            }
            #region Setup
            void SETUP_FilterBlocksOnMissileGrid()
            {
                IMyCubeGrid localGrid = MERGE.CubeGrid;
                GridViewAtLaunch.RemoveAll(b => b.CubeGrid != localGrid);
            }
            void SETUP_AssignBlocksToMissile()
            {
                foreach (var item in GridViewAtLaunch)
                {
                    if (item is IMyGyro && GYRO == null) GYRO = (IMyGyro)item;
                    else if (item is IMyThrust) THRUSTERS.Add(item as IMyThrust);
                    else if (item is IMyRemoteControl && REMOTE == null) REMOTE = item as IMyRemoteControl;
                    else if (item is IMyWarhead) WARHEADS.Add(item as IMyWarhead);
                    else if (item is IMyShipConnector && CONNECTOR == null) CONNECTOR = item as IMyShipConnector;
                    else if (item is IMyGasTank) TANKS.Add(item as IMyGasTank);
                    else if (item is IMyBatteryBlock) BATTERIES.Add(item as IMyBatteryBlock);
                }
            }
            void SETUP_SetDefaultBlockBehavior()
            {
                //Enables All Blocks
                foreach (var item in GridViewAtLaunch)
                {
                    IMyFunctionalBlock function = item as IMyFunctionalBlock;
                    if (function != null) function.Enabled = true;
                }
                foreach (var b in BATTERIES) b.ChargeMode = ChargeMode.Discharge;
                MERGE.Enabled = false;

                //Set warheads to safe
                foreach (var warhead in WARHEADS) warhead.IsArmed = false;
                if (CONNECTOR != null)
                {
                    CONNECTOR.Disconnect();
                    CONNECTOR.Enabled = false;
                }
                foreach (var tank in TANKS)
                {
                    tank.Stockpile = false;
                }
            }
            public void ActivateThrusters()
            {
                foreach (IMyThrust thrust in THRUSTERS) thrust.Enabled = true;
                THRUSTPERCENTAGE = 1;
            }
            #endregion
            public void InitializePostDetach()
            {
                SETUP_FilterBlocksOnMissileGrid();
                SETUP_AssignBlocksToMissile();
                SETUP_SetDefaultBlockBehavior();
                ActivateThrusters();
                Active = true;
            }
            public bool RequireDispose()
            {
                if (!Active) return false;

                if (GYRO == null || THRUSTERS.Count == 0) return true;

                bool Isgyroout = GYRO.CubeGrid.GetCubeBlock(GYRO.Position) == null;
                bool Isthrusterout = THRUSTERS[0].CubeGrid.GetCubeBlock(THRUSTERS[0].Position) == null;
                if (Isgyroout || Isthrusterout) return true;
                return false;
            }
            public void Force_Detonate()
            {
                if (WARHEADS.Count > 0)
                {
                    foreach (var war in WARHEADS)
                    {
                        war.IsArmed = true;
                        war.Detonate();
                    }
                }
            }
            public void Check_Detonate()
            {
                if (WARHEADS.Count > 0)
                {
                    if (TARGETDISTANCE <= DMS_Distance) foreach (var w in WARHEADS) w.IsArmed = true;
                    if (TARGETDISTANCE <= PROX_Distance) WARHEADS[0].Detonate();
                }
            }
            #region Missile Guidance
            public void TurnGyro6(Vector3D _targetVector, IMyTerminalBlock _ref, IMyGyro _gyro, double _gain, double _dampingGain, double _currPitch, double _currYaw, out double _newPitch, out double _newYaw)
            {
                _newPitch = 0;
                _newYaw = 0;

                double dt = Program.Program_Local?.Runtime.TimeSinceLastRun.TotalSeconds ?? (1.0 / 60.0);
                if (dt <= 0) dt = 1.0 / 60.0;

                Vector3D ShipUp = _ref.WorldMatrix.Up;
                Vector3D ShipForward = _ref.WorldMatrix.Backward;

                Quaternion Quat_Two = Quaternion.CreateFromForwardUp(ShipForward, ShipUp);
                Quaternion InvQuat = Quaternion.Inverse(Quat_Two);

                Vector3D DirectionVector = Vector3D.Normalize(_targetVector - _gyro.GetPosition());
                Vector3D RefFrameVector = Vector3D.Transform(DirectionVector, InvQuat);

                double ShipAzimuth = 0;
                double ShipElevation = 0;
                Vector3D.GetAzimuthAndElevation(RefFrameVector, out ShipAzimuth, out ShipElevation);

                _newPitch = ShipElevation;
                _newYaw = ShipAzimuth;

                ShipAzimuth = ShipAzimuth + _dampingGain * ((ShipAzimuth - _currYaw) / dt);
                ShipElevation = ShipElevation + _dampingGain * ((ShipElevation - _currPitch) / dt);

                var REF_Matrix = MatrixD.CreateWorld(_ref.GetPosition(), (Vector3)ShipForward, (Vector3)ShipUp).GetOrientation();
                var Vector = Vector3.Transform((new Vector3D(ShipElevation, ShipAzimuth, 0)), REF_Matrix);
                var TRANS_VECT = Vector3.Transform(Vector, Matrix.Transpose(_gyro.WorldMatrix.GetOrientation()));

                if (double.IsNaN(TRANS_VECT.X) || double.IsNaN(TRANS_VECT.Y) || double.IsNaN(TRANS_VECT.Z)) return;

                _gyro.Pitch = (float)MathHelper.Clamp((-TRANS_VECT.X) * _gain, -1000, 1000);
                _gyro.Yaw = (float)MathHelper.Clamp(((-TRANS_VECT.Y)) * _gain, -1000, 1000);
                _gyro.Roll = (float)MathHelper.Clamp(((-TRANS_VECT.Z)) * _gain, -1000, 1000);
                _gyro.GyroOverride = true;
            }

            Vector3D GetGravity()
            {
                return REMOTE?.GetNaturalGravity() ?? Vector3D.Zero;
            }

            public double DT()
            {
                double dt = Program.Program_Local?.Runtime.TimeSinceLastRun.TotalSeconds ?? (1.0 / 60.0);
                if (dt <= 0) dt = 1.0 / 60.0;
                return dt;
            }
            public void Run_Guidance()
            {
                //Check For Detach
                if (MERGE.IsSameConstructAs(Program_Local.Me)) return;

                //After Detach, Check for Initialization
                if (!Active)
                {
                    InitializePostDetach();
                    return;
                }

                foreach (var t in THRUSTERS) t.ThrustOverridePercentage = THRUSTPERCENTAGE;

                LAUNCH_TIMER++;
                if (LAUNCH_TIMER < GUIDANCE_DELAY) return;

                double dt = DT();

                Vector3D missilePos = GYRO.CubeGrid.WorldVolume.Center;
                Vector3D targetPos = TARGETPOS;

                if (!_havePrev)
                {
                    MIS_PREV_POS = missilePos;
                    TARGET_PREV_POS = targetPos;
                    _havePrev = true;
                    return;
                }

                Vector3D missileVel = MISSILE_VEL;
                Vector3D targetVel = (targetPos - TARGET_PREV_POS) / dt;

                Vector3D r = targetPos - missilePos;
                double r2 = r.LengthSquared();
                double rLen = Math.Sqrt(Math.Max(r2, 1e-9));
                Vector3D u = r / rLen;
                Vector3D vrel = targetVel - missileVel;
                Vector3D vrel_hat = vrel;
                if (vrel_hat.LengthSquared() > 1e-12) vrel_hat = Vector3D.Normalize(vrel_hat);

                Vector3D u_old = Vector3D.Normalize(TARGET_PREV_POS - MIS_PREV_POS);
                Vector3D u_new = u;
                Vector3D losDelta = (u_new.LengthSquared() > 0 && u_old.LengthSquared() > 0) ? (u_new - u_old) : Vector3D.Zero;
                double losRate = (losDelta.Length() / dt);
                double Vc = Math.Max(0.0, -Vector3D.Dot(vrel, u));

                Vector3D lateralDir = Vector3D.Cross(Vector3D.Cross(vrel_hat, u_new), vrel_hat);
                if (lateralDir.LengthSquared() > 1e-12) lateralDir = Vector3D.Normalize(lateralDir);

                Vector3D aLat = lateralDir * PNGain * losRate * Vc + losDelta * 9.8 * (0.5 * PNGain);
                double missileAccel = Math.Max(0.1, MAXTHRUST);
                double aLatMag = aLat.Length();
                if (aLatMag > missileAccel)
                {
                    // Just clamp the magnitude, no crazy flip
                    aLat *= missileAccel / aLatMag;
                }
                Vector3D thrustFwd = THRUSTERS[0].WorldMatrix.Backward;
                double thrustPower = Vector3D.Dot(Vector3D.Normalize(thrustFwd), Vector3D.Normalize(aLat));
                thrustPower = MathHelper.Clamp(thrustPower, MinThrustPct, 1.0);

                foreach (var thr in THRUSTERS)
                {
                    float goal = (float)(thr.MaxThrust * thrustPower);
                    if (Math.Abs(thr.ThrustOverride - goal) > 0.1f) thr.ThrustOverride = goal;
                }

                double aLatLen = aLat.Length();
                double aFwd = 0.0;
                if (aLatLen < missileAccel) aFwd = Math.Sqrt(Math.Max(0.0, missileAccel * missileAccel - aLatLen * aLatLen));

                Vector3D aCmd = aLat + u * aFwd;
                Vector3D g = GetGravity();
                Vector3D aDem = aCmd - g * GravityAggressiveness;
                if (aDem.LengthSquared() < 1e-12) aDem = u;

                Vector3D cmdDir = Vector3D.Normalize(aDem);
                AIMPOINT = GYRO.GetPosition() + cmdDir * 100.0;

                double yaw, pitch;
                TurnGyro6(AIMPOINT, THRUSTERS[0], GYRO, 3.0, .7, PREV_PITCH, PREV_YAW, out pitch, out yaw);
                PREV_YAW = yaw;
                PREV_PITCH = pitch;

                MIS_PREV_POS = missilePos;
                TARGET_PREV_POS = targetPos;
            }
            #endregion
        }

        public void EchoInfo()
        {
            var sb = new StringBuilder();
            sb.AppendLine("                  === R.E.L.O.C. ===");
            sb.AppendLine($"Version: {_version} - Date: {_date}\n");
            // sb.AppendLine($"Targetting Control: {_targettingBlock != null}");
            // sb.AppendLine($"Flight Control: {_flightControl != null}");
            sb.AppendLine($"Inactive Missile Count: {MissileRemaining}\n");
            sb.AppendLine($"Tracking Target: {haveWaypoint}");
            sb.AppendLine($"Active Missiles: {MISSILES.Count}");
            Echo(sb.ToString());
        }
        #endregion
    }
}

