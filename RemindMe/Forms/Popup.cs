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
using System.Threading.Tasks;
using System.Windows.Forms;
using WMPLib;

namespace RemindMe
{
    public partial class Popup : Form
    {
        private Reminder rem;
        //Used to play a sound
        private static WindowsMediaPlayer myPlayer = new WindowsMediaPlayer();
        private IWMPMedia mediaInfo;
        
        public Popup(Reminder rem)
        {
            BLIO.Log("Constructing Popup reminderId = " + rem.Id);
            InitializeComponent();
            this.Opacity = 0;
            this.rem = rem;

            this.Size = new Size((int)BLPopupDimensions.GetPopupDimensions().FormWidth, (int)BLPopupDimensions.GetPopupDimensions().FormHeight);
            lblTitle.Font = new Font(lblTitle.Font.FontFamily, BLPopupDimensions.GetPopupDimensions().FontTitleSize, FontStyle.Bold);
            lblNoteText.Font = new Font(lblNoteText.Font.FontFamily, BLPopupDimensions.GetPopupDimensions().FontNoteSize, FontStyle.Bold);
            this.Text = rem.Name;

            lblNoteText.MaximumSize = new Size((pnlText.Width - lblNoteText.Location.X) - 10, 0);
            lblTitle.MaximumSize = new Size((pnlTitle.Width - lblTitle.Location.X) - 10, 0);


            //Assign the events that the user can raise while doing something on the popup. The stopflash event stops the taskbar icon from flashing            
            lblTitle.MouseClick += stopFlash_Event;
            lblNoteText.MouseClick += stopFlash_Event;
            this.MouseClick += stopFlash_Event;
            this.ResizeEnd += stopFlash_Event;

            BLIO.Log("Popup constructed");
        }

        private void lblExit_MouseEnter(object sender, EventArgs e)
        {
            lblExit.ForeColor = Color.DarkRed;
        }

        /// <summary>
        /// Stops the flashing of the taskbar icon
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void stopFlash_Event(object sender, EventArgs e)
        {
            this.Activate();
            FlashWindowHelper.Stop(this);
        }
        private void lblExit_MouseLeave(object sender, EventArgs e)
        {
            lblExit.ForeColor = Color.Transparent;
        }

        private void Popup2_Load(object sender, EventArgs e)
        {
            BLIO.Log("Popup_load");
            AdvancedReminderProperties avrProps = BLAVRProperties.GetAVRProperties(rem.Id);
            List<AdvancedReminderFilesFolders> avrFF = BLAVRProperties.GetAVRFilesFolders(rem.Id);
            if (avrProps != null) //Not null? this reminder has advanced properties.
            {
                BLIO.Log("Reminder " + rem.Id + " has advanced reminder properties!");
                if (!string.IsNullOrWhiteSpace(avrProps.BatchScript))
                    BLIO.ExecuteBatch(avrProps.BatchScript);

                this.Visible = avrProps.ShowReminder == 1;
            }

            if(avrFF != null && avrFF.Count > 0)
            {
                //Go through each action, for example c:\test , delete. c:\sometest\testFile.txt , open
                foreach(AdvancedReminderFilesFolders avr in avrFF)
                {
                    if (avr.Action.ToString() == "Open")
                    {
                        if(File.Exists(avr.Path) || Directory.Exists(avr.Path))
                            System.Diagnostics.Process.Start(avr.Path);
                    }
                    else if (avr.Action.ToString() == "Delete")
                    {
                        FileAttributes attr = File.GetAttributes(avr.Path);
                        //Check if it's a file or a directory
                        if (File.Exists(avr.Path))
                            File.Delete(avr.Path);
                        else if (Directory.Exists(avr.Path))                        
                            Directory.Delete(avr.Path,true);                       
                    }
                }
            }            

            if (this.Visible)
                tmrFadeIn.Start();
            else
            {
                btnOk_Click(sender,e);
                return;
            }

            DateTime date = Convert.ToDateTime(rem.Date.Split(',')[0]);
            lblSmallDate.Text = date.ToShortDateString() + " " + date.ToShortTimeString();
            lblRepeat.Text = BLReminder.GetRepeatTypeText(rem);

            if(!String.IsNullOrWhiteSpace(rem.PostponeDate))
            {
                pbDate.BackgroundImage = Properties.Resources.RemindMeZzz;                
                lblSmallDate.Text = Convert.ToDateTime(rem.PostponeDate) + " (Postponed)";
            }

            //If some country has a longer date string, move the repeat icon/text more to the right so it doesnt overlap
            while (lblSmallDate.Bounds.IntersectsWith(pbRepeat.Bounds))
            {
                pbRepeat.Location = new Point(pbRepeat.Location.X + 5, pbRepeat.Location.Y);
                lblRepeat.Location = new Point(lblRepeat.Location.X + 5, lblRepeat.Location.Y);
            }

            //Play the sound
            if (rem.SoundFilePath != null && rem.SoundFilePath != "")
            {

                if (System.IO.File.Exists(rem.SoundFilePath))
                {
                    BLIO.Log("SoundFilePath not null / empty and exists on the hard drive!");
                    myPlayer.URL = rem.SoundFilePath;
                    myPlayer.controls.play();
                    BLIO.Log("Playing sound");
                }
                else
                {
                    BLIO.Log("SoundFilePath not null / empty but doesn't exist on the hard drive!");
                    RemindMeBox.Show("Could not play " + Path.GetFileNameWithoutExtension(rem.SoundFilePath) + " located at \"" + rem.SoundFilePath + "\" \r\nDid you move,rename or delete the file ?\r\nThe sound effect has been removed from this reminder. If you wish to re-add it, select it from the drop-down list.", RemindMeBoxReason.OK);
                    //make sure its removed from the reminder
                    rem.SoundFilePath = "";
                }
            }

            FlashWindowHelper.Start(this);
            //this.MaximumSize = this.Size;

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
            

            lblTitle.Text = rem.Name;

            if(rem.Note != null)
                lblNoteText.Text = rem.Note.Replace("\\n", Environment.NewLine);

            if (rem.Note == "")
                lblNoteText.Text = "( No text set )";

            lblNoteText.Text = Environment.NewLine + Environment.NewLine + lblNoteText.Text;              

            

            if (rem.Date == null)
                rem.Date = DateTime.Now.ToString();            
        }

        private void lblMinimize_MouseEnter(object sender, EventArgs e)
        {
            lblMinimize.ForeColor = Color.CornflowerBlue;
        }

        private void lblMinimize_MouseLeave(object sender, EventArgs e)
        {
            lblMinimize.ForeColor = Color.Transparent;
        }

      

        private void lblPostpone_Click(object sender, EventArgs e)
        {
            cbPostpone.Checked = !cbPostpone.Checked;
        }

        private void lblExit_Click(object sender, EventArgs e)
        {
            btnOk_Click(sender, e);
            /*if (rem.Id != -1)
            {
                rem.PostponeDate = null;
                BLReminder.UpdateReminder(rem);
                //RefreshMainFormListView();
            }
            this.Close();
            this.Dispose();*/
        }

        private void lblMinimize_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = true;
        }

       

        private void RepositionControls()
        {
            //give new locations to the controls if the size changes.                                    

            //cbPostpone.Location = new Point(3, pnlFooter.Height - cbPostpone.Height - 3);

            //lblPostpone.Location = new Point(cbPostpone.Location.X + cbPostpone.Width + 3, cbPostpone.Location.Y);
            // todo pnlPostpone.Location = new Point(lblPostpone.Location.X + lblPostpone.Width + 5, cbPostpone.Location.Y + 1);
            //todo tbtime.Location = new Point(numPostponeTime.Location.X + numPostponeTime.Width + 3, numPostponeTime.Location.Y - 7);
            btnOk.Location = new Point(pnlFooter.Width - btnOk.Width - 3, pnlFooter.Height - btnOk.Height - 3);
            
        }


        private void rbHours_CheckedChanged(object sender, EventArgs e)
        {
            cbPostpone.Checked = true;
        }

        private void rbMinutes_CheckedChanged(object sender, EventArgs e)
        {
            cbPostpone.Checked = true;
        }

        private void numPostponeTime_ValueChanged(object sender, EventArgs e)
        {            
            cbPostpone.Checked = true;
        }

        private void Popup2_SizeChanged(object sender, EventArgs e)
        {
            RepositionControls();
            lblNoteText.MaximumSize = new Size((pnlText.Width - lblNoteText.Location.X) - 10, 0);
            lblTitle.MaximumSize = new Size((pnlTitle.Width - lblTitle.Location.X) - 10, 0);            
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            rem = BLReminder.GetReminderById(rem.Id);

            if (rem == null)
                goto close;

            if (rem.Id != -1 && rem.Deleted == 0) //Don't do stuff if the id is -1, invalid. the id is set to -1 when the user previews an reminder
            {
                if(BLReminder.GetReminderById(rem.Id) == null)
                {
                    //The reminder popped up, it existed, but when pressing OK it doesn't exist anymore (maybe the user deleted it or tempered with the .db file)
                    BLIO.Log("DETECTED NONEXISTING REMINDER WITH ID " + rem.Id + ", Attempted to press OK on a reminder that somehow doesn't exist");
                    goto close;
                }
                
                if (cbPostpone.Checked)
                {
                    BLIO.Log("Postponing reminder with id " + rem.Id);
                    if (numPostponeTime.Value == 0)
                        return;

                    DateTime newReminderTime = new DateTime();

                    if (cbPostpone.Checked && tbtime.ForeColor == Color.White && !string.IsNullOrWhiteSpace(tbtime.Text)) //postpone option is x minutes                
                    {
                        newReminderTime = DateTime.Now.AddMinutes(BLFormLogic.GetTextboxMinutes(tbtime));
                        rem.PostponeDate = newReminderTime.ToString();
                    }
                    else
                    {                        
                        rem.PostponeDate = null;
                        BLReminder.UpdateReminder(rem);
                    }



                    
                    BLIO.Log("Postpone date assigned to reminder");
                    rem.Enabled = 1;
                    BLReminder.EditReminder(rem);
                    BLIO.Log("Reminder postponed!");
                }
                else
                {
                    rem.PostponeDate = null;
                    BLReminder.UpdateReminder(rem);
                }                
            }

            close:
            UCReminders.GetInstance().UpdateCurrentPage();
            BLIO.Log("Stopping media player & Closing popup");
            myPlayer.controls.stop();
            btnOk.Enabled = false;
            this.Close();
        }

        private void numPostponeTime_KeyUp(object sender, KeyEventArgs e)
        {            
            if(!cbPostpone.Checked) //If its not already checked, then...
                cbPostpone.Checked = char.IsNumber((char)e.KeyCode);            
        }

        private void Popup_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.WindowsShutDown)
                e.Cancel = true;
        }

        private void tmrFadeIn_Tick(object sender, EventArgs e)
        {
            this.Opacity += 0.1;
            if (this.Opacity >= 1)
                tmrFadeIn.Stop();
        }

        private void tbPrompt_KeyUp(object sender, KeyEventArgs e)
        {            
            //Show the user that whatever it is they are inputting is invalid
            if (tbtime.Text == "" || BLFormLogic.GetTextboxMinutes(tbtime) != -1)
                tbtime.BorderColorFocused = Color.FromArgb(64, 64, 64);
            else
                tbtime.BorderColorFocused = Color.Red;
        }

        private void label13_Click(object sender, EventArgs e)
        {
            cbPostpone.Checked = !cbPostpone.Checked;

            if (cbPostpone.Checked)
                btnOk.Text = "Postpone";
            else
                btnOk.Text = "Ok";

            tbtime.Visible = cbPostpone.Checked;
        }

        private void tbtime_Enter(object sender, EventArgs e)
        {
            if (tbtime.ForeColor == Color.Gray)
            {
                tbtime.Text = "";
                tbtime.ForeColor = Color.White;                
            }
        }

        private void tbtime_Leave(object sender, EventArgs e)
        {
            if(String.IsNullOrEmpty(tbtime.Text))
            {
                tbtime.ForeColor = Color.Gray;
                tbtime.Text = "1h30m";
            }
        }

       

        private void cbPostpone_OnChange_1(object sender, EventArgs e)
        {
            if (cbPostpone.Checked)
                btnOk.Text = "Postpone";
            else
                btnOk.Text = "Ok";

            tbtime.Visible = cbPostpone.Checked;
        }
    }
}
