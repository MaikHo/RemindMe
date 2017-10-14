﻿using Database.Entity;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data_Access_Layer
{
    public abstract class DLDatabase
    {
        private static readonly string DB_FILE = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\RemindMe\\RemindMeDatabase.db";

        //The neccesary query to execute to create the table Reminder
        private const string TABLE_REMINDER = "CREATE TABLE [Reminder] ([Id] INTEGER NOT NULL, [Name]text NOT NULL, [Date]text NOT NULL, [RepeatType]text NOT NULL, [Note]text NOT NULL, [Enabled]bigint NOT NULL, [EveryXCustom] bigint NULL, [RepeatDays] text NULL, [SoundFilePath] text NULL, [PostponeDate] text NULL, CONSTRAINT[sqlite_master_PK_Reminder] PRIMARY KEY([Id]));";

        //The neccesary query to execute to create the table Settings
        private const string TABLE_SETTINGS = "CREATE TABLE [Settings] ([Id] INTEGER NOT NULL, [AlwaysOnTop]bigint DEFAULT 1 NOT NULL, [StickyForm] bigint NOT NULL, [EnableReminderCountPopup]bigint DEFAULT 1 NOT NULL, [EnableHourBeforeReminder] bigint DEFAULT 1 NOT NULL, CONSTRAINT[sqlite_master_PK_Settings] PRIMARY KEY([Id]));";

        //The neccesary query to execute to create the table Songs
        private const string TABLE_SONGS = "CREATE TABLE [Songs] ( [Id] INTEGER NOT NULL, [SongFileName]text NOT NULL, [SongFilePath]text NOT NULL, CONSTRAINT[sqlite_master_PK_Songs] PRIMARY KEY([Id]));";

        //The neccesary query to execute to create the table PopupDimensions
        private const string TABLE_POPUP_DIMENSIONS = "CREATE TABLE [PopupDimensions] ([Id] INTEGER NOT NULL, [FormWidth]bigint NOT NULL, [FormHeight]bigint NOT NULL, [FontTitleSize]bigint NOT NULL, [FontNoteSize]bigint NOT NULL, CONSTRAINT[sqlite_master_PK_PopupDimensions] PRIMARY KEY([Id]));";       


        /// <summary>
        /// Creates the database with associated tables
        /// </summary>
        public static void CreateDatabase()
        {
            SQLiteConnection conn = new SQLiteConnection();
            conn.ConnectionString = "data source = " + DB_FILE;
            conn.Open();

            SQLiteCommand tableReminder = new SQLiteCommand();
            SQLiteCommand tableSettings = new SQLiteCommand();
            SQLiteCommand tableSongs = new SQLiteCommand();
            SQLiteCommand tablePopupDimensions = new SQLiteCommand();
            tableReminder.CommandText = TABLE_REMINDER;
            tableSettings.CommandText = TABLE_SETTINGS;
            tableSongs.CommandText = TABLE_SONGS;
            tablePopupDimensions.CommandText = TABLE_POPUP_DIMENSIONS;

            tableReminder.Connection = conn;
            tableSettings.Connection = conn;
            tableSongs.Connection = conn;
            tablePopupDimensions.Connection = conn;

            tableReminder.ExecuteNonQuery();
            tableSettings.ExecuteNonQuery();
            tableSongs.ExecuteNonQuery();
            tablePopupDimensions.ExecuteNonQuery();

            conn.Close();
            conn.Dispose();

        }
        /// <summary>
        /// Checks wether the table Reminder has column x
        /// </summary>
        /// <param name="columnName">The column you want to check on</param>
        /// <returns></returns>
        public static bool HasColumn(string columnName, string table)
        {
            using (RemindMeDbEntities db = new RemindMeDbEntities())
            {
                try
                {
                    var t = db.Database.SqlQuery<object>("SELECT " + columnName + " FROM " + table).ToList();
                    db.Dispose();
                    return true;
                }
                catch (SQLiteException ex)
                {
                    db.Dispose();
                    //if (ex.Message.ToLower().Contains("no such column"))
                    //{
                    return false;
                    //}                                        
                }
            }
        }

        /// <summary>
        /// Checks if the user's .db file has all the columns from the database model.
        /// </summary>
        /// <returns></returns>
        public static bool HasAllColumns()
        {
            var reminderNames = typeof(Reminder).GetProperties().Select(property => property.Name).ToList();

            foreach (string columnName in reminderNames)
            {
                if (!HasColumn(columnName, "reminder"))
                    return false; //aww damn! the user has an outdated .db file!                
            }

            var settingNames = typeof(Settings).GetProperties().Select(property => property.Name).ToList();

            foreach (string columnName in settingNames)
            {
                if (!HasColumn(columnName, "settings"))
                    return false; //aww damn! the user has an outdated .db file!                
            }

            var songNames = typeof(Songs).GetProperties().Select(property => property.Name).ToList();

            foreach (string columnName in songNames)
            {
                if (!HasColumn(columnName, "songs"))
                    return false; //aww damn! the user has an outdated .db file!                
            }

            var popupDimensionNames = typeof(PopupDimensions).GetProperties().Select(property => property.Name).ToList();

            foreach (string columnName in popupDimensionNames)
            {
                if (!HasColumn(columnName, "PopupDimensions"))
                    return false; //aww damn! the user has an outdated .db file!                
            }

            return true;
        }

        /// <summary>
        /// Checks if the database has the given table
        /// </summary>
        /// <param name="table"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        private static bool HasTable(string table,RemindMeDbEntities db)
        {
            try
            {
                var result = db.Database.ExecuteSqlCommand("select * from " + table);
                return true;
            }
            catch(SQLiteException ex)
            {
                return false;
            }
        }
        /// <summary>
        /// Checks if the user's .db file has all the tables from the database model.
        /// </summary>
        /// <returns></returns>
        public static bool HasAllTables()
        {
            using (RemindMeDbEntities db = new RemindMeDbEntities())
            {
                if (HasTable("reminder", db) && HasTable("settings", db) && HasTable("songs", db) && HasTable("popupdimensions", db))
                    return true;
                else
                    return false;                
            }
            
        }

        /// <summary>
        /// Inserts all missing tables into the user's .db file 
        /// </summary>
        public static void InsertMissingTables()
        {
            using (RemindMeDbEntities db = new RemindMeDbEntities())
            {
                SQLiteConnection conn = new SQLiteConnection();
                conn.ConnectionString = "data source = " + DB_FILE;
                conn.Open();

                SQLiteCommand tableReminder = new SQLiteCommand();
                SQLiteCommand tableSettings = new SQLiteCommand();
                SQLiteCommand tableSongs = new SQLiteCommand();
                SQLiteCommand tablePopupDimensions = new SQLiteCommand();
                tableReminder.CommandText = TABLE_REMINDER;
                tableSettings.CommandText = TABLE_SETTINGS;
                tableSongs.CommandText = TABLE_SONGS;
                tablePopupDimensions.CommandText = TABLE_POPUP_DIMENSIONS;

                tableReminder.Connection = conn;
                tableSettings.Connection = conn;
                tableSongs.Connection = conn;
                tablePopupDimensions.Connection = conn;

                if (!HasTable("Reminder", db))
                    tableReminder.ExecuteNonQuery();

                if (!HasTable("Settings", db))
                    tableSettings.ExecuteNonQuery();

                if (!HasTable("Songs", db))
                    tableSongs.ExecuteNonQuery();

                if (!HasTable("Popupdimensions", db))
                    tablePopupDimensions.ExecuteNonQuery();

                conn.Close();
                conn.Dispose();
                db.Dispose();
            }
        }

        /// <summary>
        /// This method will insert every missing column for each table into the database. Will only be called if HasallColumns() returns false. This means the user has an outdated .db file
        /// </summary>
        public static void InsertNewColumns()
        {
            using (RemindMeDbEntities db = new RemindMeDbEntities())
            {
                //every column that SHOULD exist
                var reminderNames = typeof(Reminder).GetProperties().Select(property => property.Name).ToArray();
                var settingNames = typeof(Settings).GetProperties().Select(property => property.Name).ToArray();
                var songNames = typeof(Songs).GetProperties().Select(property => property.Name).ToArray();
                var popupDimensionsNames = typeof(PopupDimensions).GetProperties().Select(property => property.Name).ToArray();

                foreach (string column in reminderNames)
                {
                    if (!HasColumn(column, "reminder"))
                        db.Database.ExecuteSqlCommand("ALTER TABLE REMINDER ADD COLUMN " + column + " " + GetReminderColumnSqlType(column));
                }

                foreach (string column in settingNames)
                {
                    if (!HasColumn(column, "settings"))
                        db.Database.ExecuteSqlCommand("ALTER TABLE SETTINGS ADD COLUMN " + column + " " + GetSettingColumnSqlType(column));
                }

                foreach (string column in songNames)
                {
                    if (!HasColumn(column, "songs"))
                        db.Database.ExecuteSqlCommand("ALTER TABLE SONGS ADD COLUMN " + column + " " + GetSongColumnSqlType(column));
                }

                foreach (string column in popupDimensionsNames)
                {
                    if (!HasColumn(column, "PopupDimensions"))
                        db.Database.ExecuteSqlCommand("ALTER TABLE POPUPDIMENSIONS ADD COLUMN " + column + " " + GetPopupDimensionsColumnSqlType(column));
                }

                db.SaveChanges();                
                db.Dispose();
            }
        }

        /// <summary>
        /// Gets the SQLite data types of the reminder columns, "text not null", "bigint null", etc
        /// </summary>
        /// <param name="columnName">The column you want to know the data type of</param>
        /// <returns>Data type of the column</returns>
        private static string GetReminderColumnSqlType(string columnName)
        {
            //Yes, this is not really the "correct" way of dealing with a problem, but after a lot of searching it's quite a struggle
            //to get the data types of the sqlite columns, especially when they're nullable.
            switch (columnName)
            {
                case "Id": return "INTEGER NOT NULL";
                case "Name": return "text NOT NULL DEFAULT ''";
                case "Date": return "text NOT NULL DEFAULT '1-1-1990'";
                case "RepeatType": return "text NOT NULL DEFAULT 'none'";
                case "Note": return "text NULL ";
                case "Enabled": return "bigint NOT NULL DEFAULT '1'";
                case "DayOfWeek": return "bigint NULL";
                case "EveryXCustom": return "bigint NULL";
                case "RepeatDays": return "text NULL";
                case "SoundFilePath": return "text NULL";
                case "PostponeDate": return "text NULL";
                default: return "text NULL";
            }
        }

        /// <summary>
        /// Gets the SQLite data types of the settings columns, "text not null", "bigint null", etc
        /// </summary>
        /// <param name="columnName">The column you want to know the data type of</param>
        /// <returns>Data type of the column</returns>
        private static string GetSettingColumnSqlType(string columnName)
        {
            //Yes, this is not really the "correct" way of dealing with a problem, but after a lot of searching it's quite a struggle
            //to get the data types of the sqlite columns, especially when they're nullable.
            switch (columnName)
            {
                case "AlwaysOnTop": return "INTEGER DEFAULT 1 NOT NULL";
                case "StickyForm": return "INTEGER DEFAULT 0 NOT NULL";
                case "EnablePopupMessage": return "INTEGER DEFAULT 1 NOT NULL";
                case "EnableHourBeforeReminder": return "INTEGER DEFAULT 1 NOT NULL";
                default: return "text NULL";
            }
        }

        /// <summary>
        /// Gets the SQLite data types of the songs columns, "text not null", "bigint null", etc
        /// </summary>
        /// <param name="columnName">The column you want to know the data type of</param>
        /// <returns>Data type of the column</returns>
        private static string GetSongColumnSqlType(string columnName)
        {
            //Yes, this is not really the "correct" way of dealing with a problem, but after a lot of searching it's quite a struggle
            //to get the data types of the sqlite columns, especially when they're nullable.
            switch (columnName)
            {
                case "Id": return "INTEGER NOT NULL";
                case "SongFileName": return "text NOT NULL";
                case "SongFilePath": return "text NOT NULL";
                default: return "text NULL";
            }
        }


        /// <summary>
        /// Gets the SQLite data types of the songs columns, "text not null", "bigint null", etc
        /// </summary>
        /// <param name="columnName">The column you want to know the data type of</param>
        /// <returns>Data type of the column</returns>
        private static string GetPopupDimensionsColumnSqlType(string columnName)
        {
            //Yes, this is not really the "correct" way of dealing with a problem, but after a lot of searching it's quite a struggle
            //to get the data types of the sqlite columns, especially when they're nullable.
            switch (columnName)
            {
                case "Id": return "INTEGER NOT NULL";
                case "FormWidth": return "bigint NOT NULL";
                case "FormHeight": return "bigint NOT NULL";
                case "FontTitleSize": return "bigint NOT NULL";
                case "FontNoteSize ": return "bigint NOT NULL";

                default: return "text NULL";
            }
        }

       
    }
}
