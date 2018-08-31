﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Database.Entity;
using Business_Logic_Layer;

namespace RemindMe
{
    public partial class UCTimer : UserControl
    {
        public decimal timerDuration = 0;
        public string timerNote = "";
        public UCTimer()
        {
            InitializeComponent();            
        }
     
        /// <summary>
        /// Subtracts a second from the numeric updown every second, a minute if seconds = 0, an hour if minutes = 0
        /// </summary>
        private void TickDownTime()
        {           
            if(numSeconds.Value > 0)
                numSeconds.Value--;
            else
            {
                if (numMinutes.Value > 0)
                {
                    numMinutes.Value--;
                    numSeconds.Value = 59;
                }
                else
                {
                    if (numHours.Value > 0)
                    {
                        numHours.Value--;
                        numMinutes.Value = 59;
                        numSeconds.Value = 59;
                    }//Else probably no time left.
                    
                }                
            }

            timerDuration = (numHours.Value * 60 * 60) + (numMinutes.Value * 60) + numSeconds.Value;

            if (timerDuration <= 0)
            {
                tmrCountdown.Stop();
                btnStartTimer.Text = "Start timer";
                Reminder rem = new Reminder();
                rem.Id = -1; //Set it to our "Invalid id" number. It is not a real reminder after all.
                rem.Name = "Timer";

                if (timerNote != "") //timerNote can be set from TimerPopup
                    rem.Note = timerNote;
                else
                    rem.Note = tbNote.Text;

                if(cbSound.SelectedItem != null)
                {
                    ComboBoxItem selectedItem = (ComboBoxItem)cbSound.SelectedItem;
                    Songs selectedSong = (Songs)selectedItem.Value;
                    rem.SoundFilePath = selectedSong.SongFilePath;
                }
                Popup pop = new Popup(rem);
                pop.Show();
            }
        }

        private void btnStartTimer_Click(object sender, EventArgs e)
        {
            ToggleTimer();            
        }
        public void ToggleTimer()
        {
            if (tmrCountdown.Enabled)
            {
                tmrCountdown.Stop();                
                btnStartTimer.Text = "Start timer";
                return;
            }
            
            timerDuration = (numHours.Value * 60 * 60) + (numMinutes.Value * 60) + numSeconds.Value;

            if (timerDuration > 0)
            {
                tmrCountdown.Start();
                label1.Focus(); //Dont leave focus on the numeric up down, showing the Ibeam cursor isnt very good looking
                btnStartTimer.Text = "Stop timer";
            }
        }

        private void FillSoundCombobox()
        {
            //Fill the list with all the sounds from the Database
            List<Songs> sounds = BLSongs.GetSongs();

            cbSound.Items.Clear();
            ComboBoxItemManager.ClearComboboxItems();

            if (sounds != null)
                foreach (Songs item in sounds)
                    if (item.SongFileName != "")
                        cbSound.Items.Add(new ComboBoxItem(System.IO.Path.GetFileNameWithoutExtension(item.SongFileName), item));

            cbSound.Sorted = true;
        }
        private void tmrCountdown_Tick(object sender, EventArgs e)
        {
            TickDownTime();
        }

        private void numHours_Click(object sender, EventArgs e)
        {
            timerDuration = (numHours.Value * 60 * 60) + (numMinutes.Value * 60) + numSeconds.Value;
        }

        private void numMinutes_Click(object sender, EventArgs e)
        {
            timerDuration = (numHours.Value * 60 * 60) + (numMinutes.Value * 60) + numSeconds.Value;
        }

        private void numSeconds_Click(object sender, EventArgs e)
        {
            timerDuration = (numHours.Value * 60 * 60) + (numMinutes.Value * 60) + numSeconds.Value;
        }

        private void UCTimer_Load(object sender, EventArgs e)
        {
            FillSoundCombobox();

            
            foreach (string m in BLHotkeys.TimerPopup.Modifiers.ToString().Split(','))
            {
                lblKeyCombination.Text += m + " + ";
            }
            lblKeyCombination.Text += BLHotkeys.TimerPopup.Key.ToString();
        }
    }
}
