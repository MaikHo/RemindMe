﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using Business_Logic_Layer;
using System.Net.Mail;
using System.IO;
using Database.Entity;

namespace RemindMe
{
    public partial class UCSupport : UserControl
    {                
        public UCSupport()
        {
            InitializeComponent();
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            try
            {
                //Don't do anything without internet
                if (!BLIO.HasInternetAccess())
                {
                    MessageFormManager.MakeMessagePopup("You do not currently have an active internet connection", 3);
                    return;
                }

                string email = tbEmail.Text;
                string subject = tbSubject.Text;
                string note = tbNote.Text;

                BLOnlineDatabase.InsertEmailAttempt(File.ReadAllText(IOVariables.uniqueString), note, subject, email);
                MessageFormManager.MakeMessagePopup("Feedback Sent. Thank you!", 5);
                tbEmail.Text = "";
                tbSubject.Text = "";
                tbNote.Text = "";
            }
            catch (Exception ex)
            {
                MessageFormManager.MakeMessagePopup("Something went wrong. Could not send the e-mail",3);
            }
        }

        private void btnBack_Click(object sender, EventArgs e)
        {            
            pnlMessageOverview.Location = new Point(0, 0);
            pnlSendMessages.Location = new Point(667, 0);
        }

        private void bunifuFlatButton2_Click(object sender, EventArgs e)
        {            
            pnlSendMessages.Location = new Point(0, 0);
            pnlMessageOverview.Location = new Point(667, 0);
        }

        private void UCSupport_VisibleChanged(object sender, EventArgs e)
        {
            lvMessages.Items.Clear();

            ListViewItem itm;
            foreach (ReadMessages mes in BLReadMessages.Messages)
            {
                itm = new ListViewItem(mes.MessageText);
                itm.Tag = mes.ReadMessageId;
                lvMessages.Items.Add(itm);
            }
        }

        private void bunifuFlatButton1_Click(object sender, EventArgs e)
        {
            if (lvMessages.SelectedItems.Count > 0)
            {
                ListViewItem itm = lvMessages.SelectedItems[0];                

                RemindMeMessages mess = BLOnlineDatabase.GetRemindMeMessageById(Convert.ToInt32(itm.Tag));
                if (mess == null)
                {
                    MessageFormManager.MakeMessagePopup("Could not show this message. It does not exist anymore", 4);
                    lvMessages.Items.Remove(itm);
                    BLReadMessages.DeleteMessage(Convert.ToInt32(itm.Tag));
                    return; //Doesn't exist in the database anymore
                }

                if (mess.NotificationType == "REMINDMEBOX")
                {
                    RemindMeBox.Show("A Message from the creator of RemindMe", mess.Message.Replace("¤", Environment.NewLine), RemindMeBoxReason.OK);
                }
                else if (mess.NotificationType == "REMINDMEMESSAGEFORM")
                {
                    MessageFormManager.MakeMessagePopup(mess.Message.Replace("¤", Environment.NewLine), mess.NotificationDuration.Value);
                }
                else
                {
                    MessageFormManager.MakeMessagePopup("Could not preview this message. Unknown notification type", 4);
                    lvMessages.Items.Remove(itm);                    
                    BLReadMessages.DeleteMessage(mess.Id);
                }
                
            }
        }
    }
}
