using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.AI;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;
using VRageRender;
namespace IngameScript
    
{
   
    partial class Program : MyGridProgram
    {
        // Minimap and Radar

        const bool _serverFriendlier = false; //Lowers the refresh rate of the screen
        const bool _doRadar = false; //Displays ENEMIES on the map (will not display friendlies)
        //      Requires Designator Guns to be included in map group      //
        const bool _collisionAvoidance = true; //Causes issues on planets from my experience
        const int _numOfScreen = 1; //Number of screens the map will take up
        const string _groupName = "(MAR)";
        //DO NOT EDIT BELOW





        //DO NOT EDIT BELOW





        //DO NOT EDIT BELOW





        //DO NOT EDIT BELOW





        //DO NOT EDIT BELOW





        //DO NOT EDIT BELOW





        //DO NOT EDIT BELOW





        //DO NOT EDIT BELOW





        //DO NOT EDIT BELOW
        const string _version = "1.0.3";
        const string _date = "2.12.2025";
        public Program()
        {
            bool t = _serverFriendlier;
            if (t) Runtime.UpdateFrequency = UpdateFrequency.Update100;
            else Runtime.UpdateFrequency = UpdateFrequency.Update1;
            Me.CustomName = $"{_groupName} Programmable Block";
        }
        public void Main(string argument, UpdateType updateSource)
        {
            // Get the cockpit
            IMyCockpit cockpit = GridTerminalSystem.GetBlockWithName("Cockpit") as IMyCockpit;
            IMyRemoteControl controller = GridTerminalSystem.GetBlockWithName("auto controller") as IMyRemoteControl;
            IMyTextPanel lcd = GridTerminalSystem.GetBlockWithName("LCD") as IMyTextPanel;
            IMyTextSurface drawingSurface = lcd;
            var frame = drawingSurface.DrawFrame();
            if (cockpit == null)
            {
                Echo("Error: No cockpit found!");
                return;
            }
            // Get the cockpit's orientation vectors
            Vector3D controllerPos = controller.GetPosition();
            Vector3D forward = controller.WorldMatrix.Forward;
            Vector3D left = controller.WorldMatrix.Right;
            // Example: Transform a list of 3D points to 2D
            List<MyWaypointInfo> myWaypoints = new List<MyWaypointInfo>();
            controller.GetWaypointInfo(myWaypoints);
            List<Vector3D> points3D = new List<Vector3D>{};
            for (int i = 0; i < myWaypoints.Count; i++)
            {
                points3D.Insert(i, myWaypoints[i].Coords);
            }
            List<Vector2D> points2D = Convert3DTo2D(points3D, forward, left);
            // Print results
            SetupScreen(drawingSurface);
            PrintInfo(myWaypoints, controller.CurrentWaypoint, controller.IsAutoPilotEnabled);
            DrawMap(ref frame, points2D, controller.CurrentWaypoint.Coords, controller);
        }
        string _Storage;
        int _totalTicks = 0;
        const int _ticksPerSec = 60;
        public void Save()
        {
            _Storage = _totalTicks.ToString();
        }
        // Convert 3D points to 2D based on a given plane
        List<Vector2D> Convert3DTo2D(List<Vector3D> points3D, Vector3D forward, Vector3D right)
        {
            IMyRemoteControl controller = GridTerminalSystem.GetBlockWithName("auto controller") as IMyRemoteControl;
            List<Vector2D> points2D = new List<Vector2D>();
            foreach (var point in points3D)
            {
            Vector3D targetPosition = point;
            Vector3D relativeVector = targetPosition - controller.GetPosition();
            double x = Vector3D.Dot(relativeVector, right);
            double y = Vector3D.Dot(relativeVector, forward);
            points2D.Add(new Vector2D(x, y));
            }

            return points2D;
        }
        public Dictionary<string, MyWaypointInfo> SetUpDic(MyWaypointInfo currentGPS, List<MyWaypointInfo> allGPS)
        {
            Dictionary<string, MyWaypointInfo> waypoint = new Dictionary<string, MyWaypointInfo>();
            waypoint.Clear();
            for (int i = 0; i < allGPS.Count; i++)
            {
                waypoint.Add((i+1).ToString(), allGPS[i]);
            }      
            return waypoint;       
        }
        public void PrintInfo(List<MyWaypointInfo> allGPS, MyWaypointInfo currGPS, bool isAuto)
        {
            List<double> distances = new List<double>();
            List<MyWaypointInfo> gpsRemaining = new List<MyWaypointInfo>();
            int gpsCount = allGPS.Count;
            bool t = _serverFriendlier;
            double totalDistance = 0;
            StringBuilder info = new StringBuilder();
            info.Append("===Penny's Map Projection Script===\n");
            info.Append($"      Version {_version} -- Date {_date}\n\n\n");
            if (!string.IsNullOrEmpty(Storage))
            {
                _totalTicks = int.Parse(Storage);
            }
            if (t) _totalTicks += 100;
            if (!t) _totalTicks += 1;
            double totalSeconds = (double)_totalTicks / _ticksPerSec;
            if (isAuto)
            {
                info.Append($"Total GPS Count: {gpsCount}\n");
                info.Append($"Current GPS: \n   --{currGPS.Name}--{"\nX:" + (int)currGPS.Coords.X + "\nY:" + (int)currGPS.Coords.Y + "\nZ:" + (int)currGPS.Coords.Z}\n\n");
                gpsRemaining.AddOrInsert(currGPS, 0);
                foreach (MyWaypointInfo waypoint in allGPS)
                {
                    if (allGPS.IndexOf(waypoint) > allGPS.IndexOf(currGPS)) gpsRemaining.Add(waypoint);                    
                }
                foreach (MyWaypointInfo waypointInfo in gpsRemaining)
                {
                    if (gpsRemaining.IndexOf(waypointInfo) + 2 <= gpsRemaining.Count)
                    {
                        totalDistance += (gpsRemaining[gpsRemaining.IndexOf(waypointInfo) + 1].Coords - waypointInfo.Coords).Length();
                    }
                }             
                double distanceToNextGPS = (Me.GetPosition() - currGPS.Coords).Length();
                totalDistance += distanceToNextGPS;
                info.Append($"Total Distance: {totalDistance:F1}m\n");
                info.Append($"Distance To Next GPS: {distanceToNextGPS:F1}m");
            }
            else
            {
                info.Append("Autopilot Disabled\n");
                info.Append("Waiting for task...");
            }
            info.Append($"\n\n\nRuntime: {totalSeconds * 1000:F1}ms");
            Echo(info.ToString());
        }
        public void SetupScreen(IMyTextSurface drawingSurface)
        {
            drawingSurface.ContentType = ContentType.SCRIPT;
            drawingSurface.Script = "";
            drawingSurface.ScriptBackgroundColor = Color.Black;
        }
        public Vector2D currentMarker;
        public Color randomColor;
        public void DrawMap(ref MySpriteDrawFrame frame, List<Vector2D> gpsPoints, Vector3D currGPS, IMyRemoteControl conn)
        {
            IMyTextSurface drawingSurface = GridTerminalSystem.GetBlockWithName("LCD") as IMyTextPanel;
            const TextAlignment _center = TextAlignment.CENTER;
            float markerSize = 10f;
            Vector3D relativeVector = currGPS - conn.GetPosition();
            double xv = Vector3D.Dot(relativeVector, conn.WorldMatrix.Right);
            double yv = Vector3D.Dot(relativeVector, conn.WorldMatrix.Forward);
            Vector2 currGpsVec2 = new Vector2((float)xv, (float)yv);
            currentMarker = currGpsVec2;

            foreach (Vector2D gpsPoint in gpsPoints)
            {
                if (gpsPoint.Equals(currGpsVec2))
                {
                    return;
                }
                else
                {
                    Random randNum = new Random();
                    
                    switch(gpsPoints.IndexOf(gpsPoint) % 7)
                    {
                            case 0: randomColor = Color.Blue; break;
                            case 1: randomColor = Color.Green; break;
                            case 2: randomColor = Color.Red; break;
                            case 3: randomColor = Color.Yellow; break;
                            case 4: randomColor = Color.Brown; break;
                            case 5: randomColor = Color.Magenta; break;
                            case 6: randomColor = Color.Cyan; break;
                            case 7: randomColor = Color.DarkBlue; break;
                    }
                    
                    float xFF = (float)Math.Round((gpsPoint.X / Math.Log10(Math.Abs(gpsPoint.X) + 1) + 1) / 5, 2);
                    float yFF = -(float)Math.Round((gpsPoint.Y / Math.Log10(Math.Abs(gpsPoint.Y) + 1) + 1) / 5, 2);
                    MySprite mySprite = new MySprite(type: SpriteType.TEXTURE, data: "Circle", position: new Vector2(xFF + drawingSurface.SurfaceSize.X / 2, yFF + drawingSurface.SurfaceSize.Y / 2), size: new Vector2(markerSize, markerSize), color: randomColor, alignment: _center);
                    frame.Add(mySprite);
                }
            }
            float GPSx = (float)Math.Round((currentMarker.X / Math.Log10(Math.Abs(currentMarker.X) + 1) + 1) / 5, 2);
            float GPSy = -(float)Math.Round((currentMarker.Y / Math.Log10(Math.Abs(currentMarker.Y) + 1) + 1) / 5, 2);
            MySprite shipSprite = new MySprite(SpriteType.TEXTURE, "Circle", new Vector2(0 + drawingSurface.SurfaceSize.X / 2, 0 + drawingSurface.SurfaceSize.Y / 2), new Vector2(markerSize, markerSize), Color.DarkRed, null, _center);
            MySprite gpsSpriteTest = new MySprite(type: SpriteType.TEXTURE, data:"Circle", position: new Vector2(GPSx + drawingSurface.SurfaceSize.X / 2, GPSy + drawingSurface.SurfaceSize.Y / 2), size: new Vector2(markerSize, markerSize),color: Color.DarkGreen, alignment: _center);          
            float a = shipSprite.Position.Value.X - drawingSurface.SurfaceSize.X / 2;
            float b = shipSprite.Position.Value.Y - drawingSurface.SurfaceSize.Y / 2;
            float m = gpsSpriteTest.Position.Value.X - drawingSurface.SurfaceSize.X / 2;
            float n = gpsSpriteTest.Position.Value.Y - drawingSurface.SurfaceSize.Y / 2;
            Me.GetSurface(0).WriteText($"a:{a}\nb:{b}\nm:{m}\nn:{n}");
            float x = ((m - a) / 2) + a;
            float y = ((n - b) / 2) + b;
            MySprite middlePoint = new MySprite(SpriteType.TEXTURE, "Circle", new Vector2(x + drawingSurface.SurfaceSize.X / 2, y + drawingSurface.SurfaceSize.Y / 2), new Vector2(10f, 10f), Color.Blue, null, _center);
            double lineRotation = Math.Atan(n / m);
            double lineLength = Math.Sqrt(Math.Pow(m, 2) + Math.Pow(n, 2)); ;
            MySprite gpsLineConn = new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2(x + drawingSurface.SurfaceSize.X / 2, y + drawingSurface.SurfaceSize.Y / 2), new Vector2(markerSize / 5f, (float)lineLength), Color.White, null, _center, (float)(lineRotation + Math.PI / 2));            
            frame.Add(gpsLineConn);
            frame.Add(shipSprite);
            frame.Add(gpsSpriteTest);
            frame.Dispose();
        }
    }
}
