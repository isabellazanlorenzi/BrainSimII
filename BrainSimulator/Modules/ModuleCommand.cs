﻿//
// Copyright (c) Charles Simon. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
//  

using System.IO;
using System.Windows.Controls;
using System.Xml.Serialization;

namespace BrainSimulator.Modules
{
    public class ModuleCommand : ModuleBase
    {
        //public override string ShortDescription { get => "Reads neuron firing instructions from a file"; }
        //public override string LongDescription
        //{
        //    get =>
        //        "For testing purposes, this module reads a script file with a direction to fire specific neurons in the network. " +
        //        "You can edit the script file in the dialog box. \r\n\r\n" +
        //        "Format / commands:\r\n" +
        //        "In general, the format to fire a neuron is '[moduleLabel:] [neuronLabel]...[neuronLabel]\r\n" +
        //        "Every line in the file represents an engine cycle so commands on the same line execute in the same cycle.\r\n" +
        //        "Commands may be entered on full lines if they contain '//'\r\n" +
        //        "The 'WaitFor' command which pauses execution until the specified neuron fires.\r\n" +
        //        "The 'Stop' command aborts execution at the line in the file...useful for executing just the first lines of a file.";
        //}

        public string textFile = ""; //path to the text file
        [XmlIgnore]
        public string[] commands;

        [XmlIgnore]
        public int line = 0;

        string currentModule = "";
        int countDown = 0;

        public ModuleCommand()
        {
            minHeight = 1;
            minWidth = 1;
        }

        public override void Fire()
        {
            Init();  //be sure to leave this here to enable use of the na variable
            if (commands == null) return;
            if (line >= commands.Length || line < 0) return;
            if (countDown > 0)
            {
                countDown--;
                return;
            }

            string[] commandLine = commands[line].Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);

            if (commandLine.Length > 0 && commandLine[0].IndexOf("//") != 0)
                for (int i = 0; i < commandLine.Length; i++)
                {
                    string currentCommand = commandLine[i];
                    if (currentCommand == "Stop")
                    {
                        line = commands.Length;
                        return;
                    }
                    if (currentCommand == "Loop")
                    {
                        line = 0;
                        return;
                    }
                    if (currentCommand == "Wait-for")
                    {
                        if (commandLine.Length > i + 1)
                        {
                            if (!int.TryParse(commandLine[i + 1], out countDown))
                            {
                                SetCurrentModule(commandLine[i + 1]);
                                Neuron n = GetNeuron(currentModule, commandLine[i + 2]);
                                if (n != null && !n.Fired()) return;
                            }
                        }
                    }

                    if (currentCommand != "")
                    {
                        //if it contains a colon, set the current module
                        if (currentCommand.IndexOf(':') != -1)
                            SetCurrentModule(currentCommand);
                        else
                        {
                            if (i + 1 < commandLine.Length)
                            {
                                float param = 1;
                                if (float.TryParse(commandLine[i+1], out param))
                                {
                                    SetNeuronValue(currentModule, currentCommand, param);
                                    i++;
                                    continue;
                                }
                            }
                            SetNeuronValue(currentModule, currentCommand, 1);
                        }
                    }
                }
            line++;
            UpdateDialog();
        }

        private void SetCurrentModule(string s)
        {
            int colonLoc = s.IndexOf(':');
            if (colonLoc != -1)
            {
                currentModule = s.Substring(0, colonLoc);
            }
        }

        public override void Initialize()
        {

            if (File.Exists(textFile))
            {
                commands = File.ReadAllLines(textFile);
            }
            UpdateDialog();
            line = 0;
        }

        public override void ShowDialog()
        {
            base.ShowDialog();
            Initialize();
        }

        public override MenuItem GetCustomMenuItems()
        {
            Button runBtn = new Button {  Content = "Run",  };
            runBtn.Click += RunBtn_Click;
            MenuItem mi = new MenuItem { Header = runBtn};
            return mi;
        }

        private void RunBtn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            line = 0;
        }
    }
}
