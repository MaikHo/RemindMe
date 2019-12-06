﻿using Data_Access_Layer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Business_Logic_Layer
{
    public class BLOnlineDatabase
    {
        private BLOnlineDatabase() {  }


        /// <summary>
        /// Logs an exception to the online database
        /// </summary>
        /// <param name="ex">The exception that is going to be logged</param>
        /// <param name="exceptionDate">The date the exception is logged at</param>
        public static void AddException(Exception ex, DateTime exceptionDate)
        {
            try
            {
                new Thread(() =>
                {

                    //Don't do anything without internet
                    if (!BLIO.HasInternetAccess())
                        return;

                    if (ex != null && ex.Message != null && ex.StackTrace != null && exceptionDate != null)
                        DLOnlineDatabase.AddException(ex, exceptionDate);
                    else
                        BLIO.Log("BLOnlineDatabase.AddException() failed: parameter(s) null");
                }).Start();
            }
            catch (Exception exc)
            {
                BLIO.Log("BLOnlineDatabase.AddException() failed: exception occured: " + exc.ToString());
            }
        }

        /// <summary>
        /// Adds a new entry to the database where a user updates their RemindMe version
        /// </summary>
        /// <param name="updateDate">Date of update</param>
        /// <param name="previousVersion">The previously installed RemindMe version on his/her machine</param>
        /// <param name="updateVersion">The version the user updated to</param>
        public static void AddNewUpdate(DateTime updateDate, string previousVersion, string updateVersion)
        {
            try
            {
                new Thread(() =>
                {
                    //Don't do anything without internet
                    if (!BLIO.HasInternetAccess())
                        return;

                    if (!string.IsNullOrWhiteSpace(previousVersion) && !string.IsNullOrWhiteSpace(updateVersion))
                        DLOnlineDatabase.AddNewUpdate(updateDate, previousVersion, updateVersion);
                    else
                        BLIO.Log("Invalid previous/update version string parameter in BLOnlineDatabase.AddNewUpdate()");
                }).Start();
            }
            catch (Exception exc)
            {
                BLIO.Log("BLOnlineDatabase.AddNewUpdate() failed: exception occured: " + exc.ToString());
            }
        }


        /// <summary>
        /// Inserts a user into the database to keep track of how many users RemindMe has (after version 2.6.02)
        /// </summary>
        /// <param name="uniqueString"></param>
        public static void InsertOrUpdateUser(string uniqueString)
        {
            try
            {
                new Thread(() =>
                {

                    //Don't do anything without internet
                    if (!BLIO.HasInternetAccess())
                        return;

                    if (!string.IsNullOrWhiteSpace(uniqueString))
                        DLOnlineDatabase.InsertOrUpdateUser(uniqueString, IOVariables.RemindMeVersion);
                    else
                        BLIO.Log("Invalid uniqueString version string parameter in BLOnlineDatabase.InsertUser(). String: " + uniqueString);
                }).Start();
            }
            catch (Exception exc)
            {
                BLIO.Log("BLOnlineDatabase.InsertUser() failed: exception occured: " + exc.ToString());
            }
        }
    }
}
