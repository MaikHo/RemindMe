﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

namespace RemindMe
{
    public partial class UCDebugMode : UserControl
    {        
        public UCDebugMode()
        {
            InitializeComponent();
        }

        private void bunifuFlatButton1_Click(object sender, EventArgs e)
        {
            ErrorPopup pop = new ErrorPopup("This is a test error in debug mode", new ReminderException("Test"));
            pop.Show();
        }

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            Process.Start(Path.GetDirectoryName(IOVariables.errorLog));
        }

        private void tmrDetails_Tick(object sender, EventArgs e)
        {                        
            if (this.Parent != null && this.Parent.Controls[0].GetType() == typeof(UCDebugMode))
            {
                lblMemoryUsage.Text = GetMemory() / 1000 + " Mb";// Process.GetCurrentProcess().VirtualMemorySize64 / 1024 / 1024 + "Mb";               
            }
        }

        private void UCDebugMode_Load(object sender, EventArgs e)
        {
            tmrDetails.Start();            
        }

        private long GetMemory()
        {            
            PerformanceCounter PC = new PerformanceCounter();
            PC.CategoryName = "Process";
            PC.CounterName = "Working Set - Private";
            PC.InstanceName = Process.GetCurrentProcess().ProcessName;
            return Convert.ToInt32(PC.NextValue()) / (int)(1024);
            PC.Close();
            PC.Dispose();
        }

        private void bunifuFlatButton2_Click(object sender, EventArgs e)
        {
            MessageFormManager.MakeMessagePopup("This is a test.", 4);
        }
    }
}
