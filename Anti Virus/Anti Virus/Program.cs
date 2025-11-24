using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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
using VRageRender;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        //Penny's Anti Virus
        /* p.s. the name is a little misleading but it prevents malicious programmables blocks from affecting your ship
         * 
         * 1. Run the Script
         * 
         * 2. Check Custom Data for the Block Group Name
         *      - you can disable this feature for your own custom name
         */
        
        
        //Config
        const bool _isExluding = false; //Excludes Programmable blocks froms being affected
        const string _exlusionTag = "[EX]"; //  as of now there is no purpose to this

        const string _unsafeBlockRename = "Unidentified Programmable Block"; //What the script will rename blocks to //P.S. it remembers your previous block names so dont worry
        const bool _useRandomGroupName = true; // Can only be set to false if _doTimerBlocks = false // Can be toggle with "groupname_toggle" (letter case does not matter)
        const string _setGroupName = "Programmable Blocks";

        //DO NOT EDIT BELOW//







        //DO NOT EDIT BELOW//






        //DO NOT EDIT BELOW//






        //DO NOT EDIT BELOW//






        //DO NOT EDIT BELOW//






        //DO NOT EDIT BELOW//






        //DO NOT EDIT BELOW//
        const string _version = "1.12.7";
        const string _date = "1.4.2025";
        public string groupName;
        public List<long> ids = new List<long>();
        public List<string> names = new List<string>();
        public bool _groupNameToggle;

        public Program()
        {
            Me.CustomName = "Programmable Block Anti Virus";
            Runtime.UpdateFrequency = UpdateFrequency.Update1;
        }
        private static Random random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOGQRSTUVWXYZ0123456789!@#$%^&*()-=[];',.<>?/";
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }
        public void Main(string argument, UpdateType updateSource)
        {   
            _groupNameToggle = _useRandomGroupName;             
            if (groupName == null & _groupNameToggle) { groupName = RandomString(10);  }
            if (!_groupNameToggle) { groupName = _setGroupName; }
            IMyTextSurface drawingSurface = Me.GetSurface(0);
            var frame = drawingSurface.DrawFrame();
            bool isGroup = true;
            List<IMyProgrammableBlock> groupprogram = new List<IMyProgrammableBlock>();
            IMyBlockGroup myProgrammableblocks = GridTerminalSystem.GetBlockGroupWithName(groupName);
            if (myProgrammableblocks == null)
            { groupprogram.Add(Me);
              isGroup = false;
            }
            else myProgrammableblocks.GetBlocksOfType(groupprogram, program => program.IsFunctional);
            List<IMyProgrammableBlock> allprogram = new List<IMyProgrammableBlock>();   
            List<IMyShipConnector> connectors = new List<IMyShipConnector>();
            GridTerminalSystem.GetBlocksOfType<IMyShipConnector>(connectors);
            GridTerminalSystem.GetBlocksOfType<IMyProgrammableBlock>(allprogram);
            foreach (IMyProgrammableBlock block in allprogram)
            {
                if (!block.CustomName.Contains(_unsafeBlockRename))
                { 

                    ids.AddOrInsert(block.GetId(), allprogram.IndexOf(block));
                    names.AddOrInsert(block.CustomName, allprogram.IndexOf(block));
                }
            }
            foreach (IMyShipConnector connector in connectors)
            {
                if (connector.IsConnected)
                {
                    GridTerminalSystem.GetBlocksOfType<IMyProgrammableBlock>(allprogram);
                }
            }
            foreach (IMyProgrammableBlock block in allprogram)
            {
                if (!block.CustomName.Contains("Program")) block.CustomName = "[Programmable] " + block.CustomName; 
            }
            for (int i = 0; i < groupprogram.Count; i++)
            {
                allprogram.Remove(groupprogram[i]);
            }
            foreach(IMyProgrammableBlock block in allprogram)
            {
                block.Enabled = false;
                block.CustomName = _unsafeBlockRename;                 
            }
            foreach(IMyProgrammableBlock block in groupprogram)
            {
                if (block.CustomName.Contains(_unsafeBlockRename))
                {
                    block.Enabled = true;
                    for (int i = 0; i < ids.Count; i++)
                    {
                        if (block.GetId() == ids[i])
                        {                        
                            if (names[i].Contains(_unsafeBlockRename))
                            {
                                block.CustomName = "Programmable Block";
                            }
                            else block.CustomName = names[i];
                        }
                    }
                }

            }
            PrintInfo(groupprogram.Count, allprogram.Count, isGroup);
            ScreenInfo(groupprogram.Count, allprogram.Count);
            SetupScreen(drawingSurface);
            DrawSprites(ref frame);
        }
        public void PrintInfo(int groupcount, int allcount, bool group)
        {
            StringBuilder info = new StringBuilder();
            info.Append($"======== Penny's Anti Virus ========\n");
            info.Append($"        (Version {_version} ~~ Date {_date})\n\n\n");
            Me.CustomData = $"Group Name: {groupName}\n !DO NOT RECOMPILE! !GROUP NAME WILL CHANGE!";
            if (!group)
            {
                info.Append($"No Group Detected...");
                info.Append($"\nCheck Custom Data For Group Name");         
            }
            else
            {
                info.Append($"Number Of Safe Programs: {groupcount}\n\n");
                info.Append($"Number Of Unitdentified Programs: {allcount}");
            }

            Echo(info.ToString());
        }
        public void SetupScreen(IMyTextSurface drawingSurface)
        {

            drawingSurface.ContentType = ContentType.SCRIPT;
            drawingSurface.Script = "";
            drawingSurface.ScriptBackgroundColor = Color.Black;
        }
        public void DrawSprites(ref MySpriteDrawFrame frame)
        {
            Color _white = new Color(200, 200, 200);
            Color _gray = new Color(90, 90, 90);
            Color _black = new Color(0,0,0);
            Color _blue = new Color(0, 0, 200);
            const string _largetitleText = "Penny's Anti Virus - " + _version;
            const string _smalltitleText = "- Penny's Anti Virus -";
            const TextAlignment Center = TextAlignment.CENTER;
            const float ShieldSpriteScale = 1.5f;
            IMyTextSurface _surface = Me.GetSurface(0);
            Vector2 screenCenter = _surface.TextureSize * 0.5f;
            Vector2 scale = _surface.SurfaceSize / 512f;
            float minScale = Math.Min(scale.X, scale.Y);
            float spriteScale = minScale * ShieldSpriteScale;
            if (Me.CubeGrid.GridSizeEnum == MyCubeSize.Small)
            {
                if (Me.GetSurface(0).SurfaceSize.Y == 256)
                {
                    // Shield
                    frame.Add(new MySprite(SpriteType.TEXTURE, "RightTriangle", new Vector2(33f, -60) * spriteScale + screenCenter, new Vector2(-100f, -100f) * scale, _white, null, Center, 3.1416f)); // Shield Top Right
                    frame.Add(new MySprite(SpriteType.TEXTURE, "RightTriangle", new Vector2(-33f, -60) * spriteScale + screenCenter, new Vector2(100f, -100f) * scale, _white, null, Center, 3.1416f)); // Shield Top Left
                    frame.Add(new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2(0, -10f) * spriteScale + screenCenter, new Vector2(200f, 50f) * scale, _white, null, Center, 3.1416f)); // Shield Middle Bar
                    frame.Add(new MySprite(SpriteType.TEXTURE, "RightTriangle", new Vector2(-33f, 65) * spriteScale + screenCenter, new Vector2(100, 180f) * scale, _white, null, Center, 3.1416f)); // Shield Bottom Left
                    frame.Add(new MySprite(SpriteType.TEXTURE, "RightTriangle", new Vector2(33f, 65) * spriteScale + screenCenter, new Vector2(-100, 180f) * scale, _white, null, Center, 3.1416f)); // Shield Bottom Right
                    frame.Add(new MySprite(SpriteType.TEXTURE, "IconEnergy", new Vector2(0, 10) * spriteScale + screenCenter, new Vector2(150f, 150f) * scale, _black, null, Center, 3.1416f)); // Shield Logo

                    // Bar
                    frame.Add(new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2(0, -130f) * spriteScale + screenCenter, new Vector2(500f, 45f) * scale, _gray, null, Center, 0));
                    frame.Add(new MySprite(SpriteType.TEXT, _smalltitleText, new Vector2(127, 15), null, _black, "ScreenCaption", Center, .9f));
                }
                else
                {
                    // Shield
                    frame.Add(new MySprite(SpriteType.TEXTURE, "RightTriangle", new Vector2(46.5f, -51) * spriteScale + screenCenter, new Vector2(-100f, -50f) * scale, _white, null, Center, 3.1416f)); // Shield Top Right
                    frame.Add(new MySprite(SpriteType.TEXTURE, "RightTriangle", new Vector2(-46.5f, -51) * spriteScale + screenCenter, new Vector2(100f, -50f) * scale, _white, null, Center, 3.1416f)); // Shield Top Left
                    frame.Add(new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2(0, -10f) * spriteScale + screenCenter, new Vector2(200f, 75f) * scale, _white, null, Center, 3.1416f)); // Shield Middle Bar
                    frame.Add(new MySprite(SpriteType.TEXTURE, "RightTriangle", new Vector2(-46.5f, 73) * spriteScale + screenCenter, new Vector2(100, 175f) * scale, _white, null, Center, 3.1416f)); // Shield Bottom Left
                    frame.Add(new MySprite(SpriteType.TEXTURE, "RightTriangle", new Vector2(46.5f, 73) * spriteScale + screenCenter, new Vector2(-100, 175f) * scale, _white, null, Center, 3.1416f)); // Shield Bottom Right
                    frame.Add(new MySprite(SpriteType.TEXTURE, "IconEnergy", new Vector2(0, 15) * spriteScale + screenCenter, new Vector2(150f, 150f) * scale, _black, null, Center, 3.1416f)); // Shield Logo

                    // Bar
                    frame.Add(new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2(0, -180f) * spriteScale + screenCenter, new Vector2(600f, 175f) * scale, _gray, null, Center, 0));
                    frame.Add(new MySprite(SpriteType.TEXT, _smalltitleText, new Vector2(130, 37), null, _black, "ScreenCaption", Center, .9f));
                }
            }
            else
            {
                if (Me.GetSurface(0).SurfaceSize.X == 512)
                {
                    // Shield
                    frame.Add(new MySprite(SpriteType.TEXTURE, "RightTriangle", new Vector2(52.75f, -60) * spriteScale + screenCenter, new Vector2(-100f, -100f) * scale, _white, null, Center, 3.1416f)); // Shield Top Right
                    frame.Add(new MySprite(SpriteType.TEXTURE, "RightTriangle", new Vector2(-52.75f, -60) * spriteScale + screenCenter, new Vector2(100f, -100f) * scale, _white, null, Center, 3.1416f)); // Shield Top Left
                    frame.Add(new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2(0, -10f) * spriteScale + screenCenter, new Vector2(200f, 50f) * scale, _white, null, Center, 3.1416f)); // Shield Middle Bar
                    frame.Add(new MySprite(SpriteType.TEXTURE, "RightTriangle", new Vector2(-52.75f, 73) * spriteScale + screenCenter, new Vector2(100, 200f) * scale, _white, null, Center, 3.1416f)); // Shield Bottom Left
                    frame.Add(new MySprite(SpriteType.TEXTURE, "RightTriangle", new Vector2(52.75f, 73) * spriteScale + screenCenter, new Vector2(-100, 200f) * scale, _white, null, Center, 3.1416f)); // Shield Bottom Right
                    frame.Add(new MySprite(SpriteType.TEXTURE, "IconEnergy", new Vector2(0, 10) * spriteScale + screenCenter, new Vector2(150f, 150f) * scale, _black, null, Center, 3.1416f)); // Shield Logo

                    // Bar
                    frame.Add(new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2(0, -130f) * spriteScale + screenCenter, new Vector2(500f, 45f) * scale, _gray, null, Center, 0));
                    frame.Add(new MySprite(SpriteType.TEXT, _largetitleText, new Vector2(250, 113), null, _black, "ScreenCaption", Center, 1.25f));
                }
                else
                {

                    // Shield
                    frame.Add(new MySprite(SpriteType.TEXTURE, "RightTriangle", new Vector2(33f, -51) * spriteScale + screenCenter, new Vector2(-100f, -50f) * scale, _white, null, Center, 3.1416f)); // Shield Top Right
                    frame.Add(new MySprite(SpriteType.TEXTURE, "RightTriangle", new Vector2(-33f, -51) * spriteScale + screenCenter, new Vector2(100f, -50f) * scale, _white, null, Center, 3.1416f)); // Shield Top Left
                    frame.Add(new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2(0, -10f) * spriteScale + screenCenter, new Vector2(200f, 50f) * scale, _white, null, Center, 3.1416f)); // Shield Middle Bar
                    frame.Add(new MySprite(SpriteType.TEXTURE, "RightTriangle", new Vector2(-33f, 73) * spriteScale + screenCenter, new Vector2(100, 150f) * scale, _white, null, Center, 3.1416f)); // Shield Bottom Left
                    frame.Add(new MySprite(SpriteType.TEXTURE, "RightTriangle", new Vector2(33f, 73) * spriteScale + screenCenter, new Vector2(-100, 150f) * scale, _white, null, Center, 3.1416f)); // Shield Bottom Right
                    frame.Add(new MySprite(SpriteType.TEXTURE, "IconEnergy", new Vector2(0, 15) * spriteScale + screenCenter, new Vector2(150f, 150f) * scale, _black, null, Center, 3.1416f)); // Shield Logo
                   
                    // Bar
                    frame.Add(new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2(0, -170f) * spriteScale + screenCenter, new Vector2(500f, 45f) * scale, _gray, null, Center, 0));
                    frame.Add(new MySprite(SpriteType.TEXT, _smalltitleText, new Vector2(255, 30), null, _black, "ScreenCaption", Center, 1.5f));
                }

            }

            frame.Dispose();
        }
        public void ScreenInfo(int groupcount, int allcount)
        {
            IMyTextSurface drawingSurface = Me.GetSurface(1);
            drawingSurface.ContentType = ContentType.TEXT_AND_IMAGE;
            drawingSurface.BackgroundColor = new Color(0, 0, 0);
            drawingSurface.FontColor = new Color(200, 200, 200);
            if (Me.GetSurface(0).SurfaceSize.X == 512 | Me.GetSurface(0).SurfaceSize.X == 256)
            {
                drawingSurface.FontSize = 2.25f;
            }
            else
            {
                drawingSurface.FontSize = 1.15f;
            }
            drawingSurface.Alignment = TextAlignment.CENTER;
            drawingSurface.TextPadding = 25;
            drawingSurface.WriteText($"Number Of Safe Programs: {groupcount}\n\nNumber Of Unitdentified Programs: {allcount}");
        }
    }
}
