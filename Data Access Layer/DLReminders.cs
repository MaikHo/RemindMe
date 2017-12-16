﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using System.Data.Entity;
using System.IO;
using Database.Entity;
//using RemindMe;

namespace Data_Access_Layer
{
    //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    //TODO: Find a way to call RefreshList() whenever the SQLite database changes(if possible)
    //Currently, there is a custom method SaveAndCloseDataBase() which saves the pending changes, closes the database, and refreshes the list
    //It would be much nicer  if there was some kind of listener for SQLite database changes, but i couldn't find any
    //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------


    /// <summary>
    /// This class handles reminders in the database.
    /// </summary>
    public class DLReminders
    {
        private DLReminders() { }

        //Instead of connecting with the database everytime, we fill this list and return it when the user calls GetReminders(). 
        private static List<Reminder> localReminders;

        

        /// <summary>
        /// Gets all reminders from the database
        /// </summary>
        /// <returns></returns>
        public static List<Reminder> GetReminders()
        {            
            //If the list  is still null, it means GetReminders() hasn't been called yet. So, we give it a value once. Then, the list will only
            //be altered when the database changes. This way we minimize wthe amount of database calls
            if (localReminders == null)            
                RefreshCacheList();            

            //If the list was null, it now returns the list of reminders from the database.
            //If it wasn't null, it will return the list as it was last known, which should be how the database is.
            return localReminders.Where(r => r.Deleted == 0).ToList(); //only return "existing" reminders
        }
        /// <summary>
        /// Gets all reminders from the database
        /// </summary>
        /// <returns></returns>
        public static List<Reminder> GetAllReminders()
        {
            //If the list  is still null, it means GetReminders() hasn't been called yet. So, we give it a value once. Then, the list will only
            //be altered when the database changes. This way we minimize wthe amount of database calls
            if (localReminders == null)
                RefreshCacheList();

            //If the list was null, it now returns the list of reminders from the database.
            //If it wasn't null, it will return the list as it was last known, which should be how the database is.
            return localReminders; //only return "existing" reminders
        }

        /// <summary>
        /// Forces the local list to be refreshed
        /// </summary>
        public static void NotifyChange()
        {
            RefreshCacheList();
        }

        /// <summary>
        /// Gives the localReminders list a new value after the database has changed.
        /// </summary>
        private static void RefreshCacheList()
        {
            using (RemindMeDbEntities db = new RemindMeDbEntities())
            {                
                localReminders = (from g in db.Reminder select g).ToList();
                db.Dispose();      
                          
            }
        }

        


        /// <summary>
        /// Inserts the reminder into the database.
        /// </summary>
        /// <param name="rem">The reminder you want added into the database</param>
        public static long PushReminderToDatabase(Reminder rem)
        {            
            using (RemindMeDbEntities db = new RemindMeDbEntities())
            {
                if (db.Reminder.Count() > 0)
                    rem.Id = db.Reminder.Max(i => i.Id) + 1;
                else
                    rem.Id = 0;

                rem.Deleted = 0;
                db.Reminder.Add(rem);
                SaveAndCloseDataBase(db);
            }
            return rem.Id;
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
        /// Gets all "deleted" reminders. Deleted reminders are reminders that are marked as deleted, but still exist.
        /// </summary>
        /// <returns>A list of reminders that are marked as deleted</returns>
        public static List<Reminder> GetDeletedReminders()
        {
            List<Reminder> toReturnList = new List<Reminder>();
            using (RemindMeDbEntities db = new RemindMeDbEntities())
            {
                toReturnList = localReminders.Where(r => r.Deleted == 1).ToList();
                db.Dispose();
            }
            return toReturnList;
        }



        /// <summary>
        /// Update an existing reminder.
        /// </summary>
        /// <param name="rem">The altered reminder</param>
        public static void EditReminder(Reminder rem)
        {
            using (RemindMeDbEntities db = new RemindMeDbEntities())
            {
                db.Reminder.Attach(rem);
                var entry = db.Entry(rem);
                entry.State = System.Data.Entity.EntityState.Modified; //Mark it for update                                
                SaveAndCloseDataBase(db);
            }
        }

        /// <summary>
        /// Marks a single reminder as deleted
        /// </summary>
        /// <param name="rem">The reminder you wish to remove</param>
        public static void DeleteReminder(Reminder rem)
        {
            if (GetReminderById(rem.Id) != null) //Check if the reminder exists
            {
                rem.Deleted = 1;
                EditReminder(rem);
            }
        }
        /// <summary>
        /// Marks a single reminder as archived
        /// </summary>
        /// <param name="rem">The reminder you wish to archive</param>
        public static void ArchiveReminder(Reminder rem)
        {
            if (GetReminderById(rem.Id) != null) //Check if the reminder exists
            {
                rem.Deleted = 2;
                EditReminder(rem);
            }
        }
        /// <summary>
        /// Marks a single reminder as archived
        /// </summary>
        /// <param name="reminderId">The id of the reminder you wish to remove</param>
        public static void ArchiveReminder(int reminderId)
        {
            if (GetReminderById(reminderId) != null) //Check if the reminder exists
            {
                Reminder toRemoveReminder = GetReminderById(reminderId);
                toRemoveReminder.Deleted = 2;
                EditReminder(toRemoveReminder);
            }
        }

        /// <summary>
        /// Archives multiple reminders. 
        /// </summary>
        /// <param name="rems"></param>
        public static void ArchiveReminders(List<Reminder> rems)
        {
            //We use this method so we can attach and remove the reminders in a foreach loop, and save changes to the database after the loop.
            //If you use the ArchiveReminders method in a foreach loop, it will open and close the database each time
            using (RemindMeDbEntities db = new RemindMeDbEntities())
            {
                foreach (Reminder rem in rems)
                {
                    rem.Deleted = 2;
                    db.Reminder.Attach(rem);
                    var entry = db.Entry(rem);
                    entry.State = System.Data.Entity.EntityState.Modified; //Mark it for update                                                                        
                }
                SaveAndCloseDataBase(db);
            }


        }

        /// <summary>
        /// Permanentely deletes a single reminder from the database
        /// </summary>
        /// <param name="rem">The reminder you wish to remove</param>
        public static void PermanentelyDeleteReminder(Reminder rem)
        {        
            using (RemindMeDbEntities db = new RemindMeDbEntities())
            {
                db.Reminder.Attach(rem);
                db.Reminder.Remove(rem);
                SaveAndCloseDataBase(db);
            }
        }

        /// <summary>
        /// Permanentely deletes a single reminder from the database
        /// </summary>
        /// <param name="rem">The reminder you wish to remove</param>
        public static void PermanentelyDeleteReminder(int reminderId)
        {
            Reminder toRemoveReminder = GetReminderById(reminderId);
            using (RemindMeDbEntities db = new RemindMeDbEntities())
            {
                db.Reminder.Attach(toRemoveReminder);
                db.Reminder.Remove(toRemoveReminder);
                SaveAndCloseDataBase(db);
            }
        }


        /// <summary>
        /// Deletes multiple reminders from the database. 
        /// </summary>
        /// <param name="rems"></param>
        public static void PermanentelyDeleteReminders(List<Reminder> rems)
        {
            //We use this method so we can attach and remove the reminders in a foreach loop, and save changes to the database after the loop.
            //If you use the DeleteReminder method in a foreach loop, it will open and close the database each time
            using (RemindMeDbEntities db = new RemindMeDbEntities())
            {
                foreach (Reminder rem in rems)
                {
                    if (GetReminderById(rem.Id) != null) //Check if the reminder exists
                    {
                        db.Reminder.Attach(rem);
                        db.Reminder.Remove(rem);
                    }
                }
                SaveAndCloseDataBase(db);
            }
        }

        /// <summary>
        /// Marks a single reminder as deleted
        /// </summary>
        /// <param name="reminderId">The id of the reminder you wish to remove</param>
        public static void DeleteReminder(int reminderId)
        {
            if (GetReminderById(reminderId) != null) //Check if the reminder exists
            {
                Reminder toRemoveReminder = GetReminderById(reminderId);
                toRemoveReminder.Deleted = 1;
                EditReminder(toRemoveReminder);
            }
        }

        

        /// <summary>
        /// Deletes multiple reminders from the database. 
        /// </summary>
        /// <param name="rems"></param>
        public static void DeleteReminders(List<Reminder> rems)
        {
            //We use this method so we can attach and remove the reminders in a foreach loop, and save changes to the database after the loop.
            //If you use the DeleteReminder method in a foreach loop, it will open and close the database each time
            using (RemindMeDbEntities db = new RemindMeDbEntities())
            {

                foreach (Reminder rem in rems)
                {
                    rem.Deleted = 1;
                    db.Reminder.Attach(rem);
                    var entry = db.Entry(rem);
                    entry.State = System.Data.Entity.EntityState.Modified; //Mark it for update                                                                        
                }
                SaveAndCloseDataBase(db);
            }
                
            
        }

        /// <summary>
        /// Saves pending changes to the database, disposes it, and refreshes the local cache list
        /// </summary>
        /// <param name="db"></param>
        private static void SaveAndCloseDataBase(RemindMeDbEntities db)
        {            
            db.SaveChanges();            
            RefreshCacheList();
            db.Dispose();
        }
    }
}
