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
        private List<File> _files = new List<File>();
        private const string STORAGE_KEY = "FileSystem";

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update1;
            LoadFiles();
        }

        public void Save()
        {
            Storage = SerializeFiles();
        }

        public void Main(string argument, UpdateType updateSource)
        {
            if (string.IsNullOrEmpty(argument))
            {
                Echo("Available commands:\n");
                Echo("save <filename> - Save a file");
                Echo("load <filename> - Load a file");
                Echo("list - List all files");
                Echo("delete <filename> - Delete a file");
                return;
            }

            string[] args = argument.Split(' ');
            string command = args[0].ToLower();

            switch (command)
            {
                case "save":
                    if (args.Length < 2)
                    {
                        Echo("Usage: save <filename>");
                        return;
                    }
                    SaveFile(args[1]);
                    break;
                case "load":
                    if (args.Length < 2)
                    {
                        Echo("Usage: load <filename>");
                        return;
                    }
                    LoadFile(args[1]);
                    break;
                case "list":
                    ListFiles();
                    break;
                case "delete":
                    if (args.Length < 2)
                    {
                        Echo("Usage: delete <filename>");
                        return;
                    }
                    DeleteFile(args[1]);
                    break;
                default:
                    Echo("Unknown command. Use no arguments to see available commands.");
                    break;
            }
        }

        private void SaveFile(string fileName)
        {
            var file = new File { FileName = fileName, FileType = "Text", Content = "" };
            _files.Add(file);
            Save();
            Echo($"File '{fileName}' saved successfully.");
        }

        private void LoadFile(string fileName)
        {
            var file = _files.FirstOrDefault(f => f.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase));
            if (file != null)
            {
                Echo($"File '{fileName}' loaded successfully.");
                Echo($"Content: {file.Content}");
            }
            else
            {
                Echo($"File '{fileName}' not found.");
            }
        }

        private void ListFiles()
        {
            if (_files.Count == 0)
            {
                Echo("No files found.");
                return;
            }

            Echo("Files:");
            foreach (var file in _files)
            {
                Echo($"- {file.FileName} ({file.FileType})");
            }
        }

        private void DeleteFile(string fileName)
        {
            var file = _files.FirstOrDefault(f => f.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase));
            if (file != null)
            {
                _files.Remove(file);
                Save();
                Echo($"File '{fileName}' deleted successfully.");
            }
            else
            {
                Echo($"File '{fileName}' not found.");
            }
        }

        private void LoadFiles()
        {
            if (!string.IsNullOrEmpty(Storage))
            {
                _files = DeserializeFiles(Storage);
            }
        }

        private string SerializeFiles()
        {
            var sb = new StringBuilder();
            foreach (var file in _files)
            {
                // Use a special separator that's unlikely to appear in content
                string escapedContent = file.Content.Replace("\\", "\\\\").Replace("|", "\\|").Replace("\n", "\\n");
                sb.AppendLine($"{file.FileName}|{file.FileType}|{escapedContent}");
            }
            return sb.ToString();
        }

        private List<File> DeserializeFiles(string data)
        {
            var files = new List<File>();
            // Split the storage string into lines, each line represents one file
            var lines = data.Split('\n');
            
            foreach (var line in lines)
            {
                // Skip empty lines
                if (string.IsNullOrWhiteSpace(line)) continue;
                
                // We need to find the first two pipe characters (|) that aren't escaped
                // These pipes separate the filename, filetype, and content
                int firstPipe = -1;  // Position of first unescaped pipe
                int secondPipe = -1; // Position of second unescaped pipe
                bool isEscaped = false; // Tracks if the current character is escaped
                
                // Scan through the line character by character
                for (int i = 0; i < line.Length; i++)
                {
                    // If we find a backslash, toggle the escaped state
                    // Two backslashes in a row cancel each other out
                    if (line[i] == '\\')
                    {
                        isEscaped = !isEscaped;
                        continue;
                    }
                    
                    // If we find a pipe and it's not escaped, record its position
                    if (line[i] == '|' && !isEscaped)
                    {
                        if (firstPipe == -1)
                            firstPipe = i;  // First pipe found
                        else if (secondPipe == -1)
                            secondPipe = i; // Second pipe found
                    }
                    
                    // Reset escaped state after processing the current character
                    isEscaped = false;
                }
                
                // If we found both pipes, we can extract the file information
                if (firstPipe != -1 && secondPipe != -1)
                {
                    // Extract the three parts of the file:
                    // 1. Filename (everything before first pipe)
                    string fileName = line.Substring(0, firstPipe);
                    // 2. Filetype (between first and second pipe)
                    string fileType = line.Substring(firstPipe + 1, secondPipe - firstPipe - 1);
                    // 3. Content (everything after second pipe)
                    string content = line.Substring(secondPipe + 1);
                    
                    // Unescape special characters in the content:
                    // - Convert \n back to actual newlines
                    // - Convert \| back to actual pipes
                    // - Convert \\ back to single backslashes
                    content = content.Replace("\\n", "\n")
                                   .Replace("\\|", "|")
                                   .Replace("\\\\", "\\");
                    
                    // Create and add the file to our list
                    files.Add(new File
                    {
                        FileName = fileName,
                        FileType = fileType,
                        Content = content
                    });
                }
            }
            return files;
        }

        public class File
        {
            public string FileName { get; set; }
            public string FileType { get; set; }
            public string Content { get; set; }

            public string CreateFile()
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append(FileName + String.Concat(Enumerable.Repeat(" ", 8)) + DateTime.Now.ToShortDateString());
                return stringBuilder.ToString();
            }
        }
    }
}
