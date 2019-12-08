﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data_Access_Layer;
using Database.Entity;

namespace Business_Logic_Layer
{
    public class BLSettings
    {
        private BLSettings() { }
        /// <summary>
        /// Reads the settings from the database and checks if reminders should be set to always on top.
        /// </summary>
        /// <returns>True if reminders are set to be always on top, false if not</returns>
        public static bool IsAlwaysOnTop()
        {
            //no business logic (yet)            
            return DLSettings.IsAlwaysOnTop();
        }

       
        /// <summary>
        /// Reads the settings from the database and checks if reminders should be set to always on top.
        /// </summary>
        /// <returns>True if reminders are set to be always on top, false if not</returns>
        public static bool IsReminderCountPopupEnabled()
        {
            //no business logic (yet)
            return DLSettings.IsReminderCountPopupEnabled();
        }

        /// <summary>
        /// Reads the settings from the database and checks if the user wants to see the popup explaining the hide reminder feature.
        /// </summary>
        /// <returns>True if the user hasn't pressed the don't remind again option yet, false if not</returns>
        public static bool HideReminderOptionEnabled
        {            
            get { return DLSettings.HideReminderOptionEnabled(); } 
        }



        /// <summary>
        /// Reads the settings from the database and checks if there should be a notification 1 hour before the reminder that there is a reminder
        /// </summary>
        /// <returns>True if the notification is enabled, false if not</returns>
        public static bool IsHourBeforeNotificationEnabled()
        {
            //no business logic (yet)
            return DLSettings.IsHourBeforeNotificationEnabled();
        }

        /// <summary>
        /// Gets the settings table from the SQLite database
        /// </summary>
        /// <returns></returns>
        public static Settings GetSettings()
        {
            return DLSettings.GetSettings();            
        }
        
        /// <summary>
        /// Update the settings in the SQLite database
        /// </summary>
        /// <param name="set">The new settings object</param>
        public static void UpdateSettings(Settings set)
        {
            if (set != null)
                DLSettings.UpdateSettings(set);
        }

    }
}
