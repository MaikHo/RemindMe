﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RemindMe
{
    /// <summary>
    /// This class handles reminders in the database.
    /// </summary>
    public abstract class DLReminders
    {
        /// <summary>
        /// Gets all reminders from the database
        /// </summary>
        /// <returns></returns>
        public static List<Reminder> GetReminders()
        {
            List<Reminder> storedDatabaseReminders = null;            
            using (RemindMeDbEntities db = new RemindMeDbEntities())
            {                
                storedDatabaseReminders = (from g in db.Reminder select g).ToList();
                db.Dispose();
            }
            return storedDatabaseReminders;
        }



        /// <summary>
        /// Inserts an reminder in the database.
        /// </summary>
        /// <param name="name">The name of the reminder</param>
        /// <param name="date">The date it should pop up</param>
        /// <param name="repeatingType">The type of repeat</param>
        /// <param name="note">The optionaln ote</param>
        /// <param name="enabled"></param>
        /// <param name="soundPath">The path to the sound file that plays when this reminder pops up</param>
        public static void InsertReminder(string name, DateTime date, ReminderRepeatType repeatingType, string note, bool enabled, string soundPath)
        {
            Reminder rem = new Reminder();
            rem.Name = name;
            rem.Date = date.ToString();
            rem.RepeatType = repeatingType.ToString();
            rem.Note = note;
            rem.SoundFilePath = soundPath;
            if (enabled)
                rem.Enabled = 1;
            else
                rem.Enabled = 0;
            using (RemindMeDbEntities db = new RemindMeDbEntities())
            {
                if (db.Reminder.Count() > 0)
                    rem.Id = db.Reminder.Max(i => i.Id) + 1;
                else
                    rem.Id = 0;
                
                db.Reminder.Add(rem);
                db.SaveChanges();
                db.Dispose();                
            }

        }

        public static void InsertReminder(string name, DateTime date, ReminderRepeatType repeatingType, int dayOfMonth, string note, bool enabled, string soundPath) 
        {
            Reminder rem = new Reminder();
            rem.Name = name;
            rem.Date = date.ToString();
            rem.RepeatType = repeatingType.ToString();
            rem.DayOfMonth = dayOfMonth;
            rem.Note = note;
            rem.SoundFilePath = soundPath;
            if (enabled)
                rem.Enabled = 1;
            else
                rem.Enabled = 0;
            PushReminderToDatabase(rem);
        }

        public static void InsertReminder(string name, DateTime date, ReminderRepeatType repeatingType, string note, int dayOfWeek, bool enabled, string soundPath)
        {
            Reminder rem = new Reminder();
            rem.Name = name;
            rem.Date = date.ToString();
            rem.RepeatType = repeatingType.ToString();
            rem.DayOfWeek = dayOfWeek;
            rem.Note = note;
            rem.SoundFilePath = soundPath;
            if (enabled)
                rem.Enabled = 1;
            else
                rem.Enabled = 0;
            PushReminderToDatabase(rem);
        }

        public static void InsertReminder(string name, DateTime date, ReminderRepeatType repeatingType, string note, bool enabled, int everyXDays, string soundPath) 
        {
            Reminder rem = new Reminder();
            rem.Name = name;
            rem.Date = date.ToString();
            rem.RepeatType = repeatingType.ToString();
            rem.EveryXDays = everyXDays;
            rem.Note = note;
            rem.SoundFilePath = soundPath;
            
            if (enabled)
                rem.Enabled = 1;
            else
                rem.Enabled = 0;
            PushReminderToDatabase(rem);
        }

        private static void PushReminderToDatabase(Reminder rem)
        {
            using (RemindMeDbEntities db = new RemindMeDbEntities())
            {
                if (db.Reminder.Count() > 0)
                    rem.Id = db.Reminder.Max(i => i.Id) + 1;
                else
                    rem.Id = 0;

                db.Reminder.Add(rem);
                db.SaveChanges();
                db.Dispose();
            }
        }

        /// <summary>
        /// Gets an reminder with the matching unique id.
        /// </summary>
        /// <param name="id">The unique id</param>
        /// <returns>Reminder that matches the given id. null if no reminder was found</returns>
        public static Reminder GetReminderById(long id)
        {
            Reminder rem = null;
            using (RemindMeDbEntities db = new RemindMeDbEntities())
            {
                rem = (from g in db.Reminder select g).Where(i => i.Id == id).SingleOrDefault();
                db.Dispose();
            }
            return rem;
        }

        /// <summary>
        /// Update an existing reminder.
        /// </summary>
        /// <param name="rem">The altered reminder</param>
        public static void EditReminder(Reminder rem)
        {
            if (GetReminderById(rem.Id) != null) //Check if the reminder exists
            {
                using (RemindMeDbEntities db = new RemindMeDbEntities())
                {
                    db.Reminder.Attach(rem);
                    var entry = db.Entry(rem);
                    entry.State = System.Data.Entity.EntityState.Modified; //Mark it for update                                
                    db.SaveChanges();                                      //push to database
                    db.Dispose();
                }
            }
            else
                RemindMeBox.Show("Could not edit that reminder, it doesn't exist.", RemindMeBoxIcon.EXCLAMATION);
        }

        public static void DeleteReminder(Reminder rem)
        {
            if (GetReminderById(rem.Id) != null) //Check if the reminder exists
            {
                using (RemindMeDbEntities db = new RemindMeDbEntities())
                {
                    db.Reminder.Attach(rem);
                    db.Reminder.Remove(rem);                                              
                    db.SaveChanges();
                    db.Dispose();
                }
            }
        }
    }
}
