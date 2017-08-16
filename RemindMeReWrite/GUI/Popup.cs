﻿using Business_Logic_Layer;
using Database.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WMPLib;

namespace RemindMe
{
    public partial class Popup : Form
    {
        private Reminder rem;
        public Popup(Reminder rem)
        {
            InitializeComponent();
            this.rem = rem;            
        }

        private const int WM_NCHITTEST = 0x84;
        private const int HT_CLIENT = 0x1;
        private const int HT_CAPTION = 0x2;

        //Used to play a sound
        private static WindowsMediaPlayer myPlayer = new WindowsMediaPlayer();
        IWMPMedia mediaInfo;

        protected override void WndProc(ref Message m)
        {
            //Makes the popup draggable
            base.WndProc(ref m);
            if (m.Msg == WM_NCHITTEST)
                m.Result = (IntPtr)(HT_CAPTION);
        }

        private void pbClosePopup_Click(object sender, EventArgs e)
        {
            this.Close();
            this.Dispose();
            
        }

        private void Popup_Load(object sender, EventArgs e)
        {
            if (BLSettings.IsAlwaysOnTop())
            {
                this.TopMost = true; //Popup will be always on top. no matter what you are doing, playing a game, watching a video, you will ALWAYS see the popup.
                this.TopLevel = true;
            }
            else
            {
                this.TopMost = false;
                this.WindowState = FormWindowState.Minimized;
            }

            //Show the reminder note            
            tbText.Text = rem.Note.Replace("\\n", Environment.NewLine);

            //Show what date this reminder was set for
            if(rem.PostponeDate == null)
                lblDate.Text = "This reminder was set for " + rem.Date.Split(',')[0];
            else
                lblDate.Text = "This reminder was set for " + rem.PostponeDate;

            tbTitle.Text = rem.Name;            

            //Make the button look better
            btnOk.TabStop = false;
            btnOk.FlatStyle = FlatStyle.Flat;
            btnOk.FlatAppearance.BorderSize = 0;

            cbPostponeType.SelectedItem = cbPostponeType.Items[0];

            lblDate.Focus();


            //Play the sound
            if (rem.SoundFilePath != null && rem.SoundFilePath != "")
            {
                if (System.IO.File.Exists(rem.SoundFilePath))
                {
                    myPlayer.URL = rem.SoundFilePath;
                    myPlayer.controls.play();                    
                }
                else
                {
                    RemindMeBox.Show("Could not play " + Path.GetFileNameWithoutExtension(rem.SoundFilePath) + " located at \"" + rem.SoundFilePath + "\" \r\nDid you move,rename or delete the file ?\r\nThe sound effect has been removed from this reminder. If you wish to re-add it, select it from the drop-down list.", RemindMeBoxIcon.INFORMATION);
                    //make sure its removed from the reminder
                    rem.SoundFilePath = "";
                }
            }
        }

        private void pbMinimizePopup_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (rem.Id != -1) //Don't do stuff if the id is -1, invalid. the id is set to -1 when the user previews an reminder
            {
                if (cbPostpone.Checked)
                {
                    DateTime newReminderTime = new DateTime();

                    if (cbPostponeType.SelectedItem == cbPostponeType.Items[0]) //postpone option is x minutes                
                        newReminderTime = DateTime.Now.AddMinutes((double)cbPostponeTime.Value);
                    else //postpone option is x hours                
                        newReminderTime = DateTime.Now.AddHours((double)cbPostponeTime.Value);

                    rem.PostponeDate = newReminderTime.ToString();
                    rem.Enabled = 1;
                    BLReminder.EditReminder(rem);
                }
                else
                {
                    rem.PostponeDate = null;
                    BLReminder.UpdateReminder(rem);
                }


                RefreshMainFormListView();
            }
                        
            this.Close();
            this.Dispose();
        }

        private void pbCloseApplication_Click(object sender, EventArgs e)
        {
            if (rem.Id != -1)
            {
                rem.PostponeDate = null;
                BLReminder.UpdateReminder(rem);
                RefreshMainFormListView();
            }
            this.Close();
            this.Dispose();
        }

        private void pbMinimizeApplication_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = true;            
        }

        private void cbPostpone_CheckedChanged(object sender, EventArgs e)
        {            
            if (cbPostpone.Checked)
                btnOk.Text = "Postpone";
            else
                btnOk.Text = "Ok";
        }

        private void Popup_FormClosing(object sender, FormClosingEventArgs e)
        {
            //Try to update the reminder before the form unexpectedly closes
            if (rem.Id != -1)
            {
                rem.PostponeDate = null;
                BLReminder.UpdateReminder(rem);
                RefreshMainFormListView();
            }

            //Stop the sound from playing when the popup closes. You don't want it to keep playing, especially if it is a soundtrack that is quite long
            myPlayer.controls.stop();
        }

        private void RefreshMainFormListView()
        {
            Form mainForm = Application.OpenForms["Form1"];
            ListView lvReminders = (ListView)mainForm.Controls["pnlMain"].Controls["lvReminders"];
            BLFormLogic.RefreshListview(lvReminders);
                        
        }

        private void cbPostponeTime_ValueChanged(object sender, EventArgs e)
        {
            cbPostpone.Checked = true;
        }

        private void cbPostponeType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbPostponeType.SelectedItem.ToString() == "Hours")
                cbPostponeTime.Maximum = 23;
            else
                cbPostponeTime.Maximum = 1400;
        }
    }
}
