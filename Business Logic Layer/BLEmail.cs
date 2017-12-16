﻿using QiHe.CodeLib.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Business_Logic_Layer
{
    /// <summary>
    /// This class will for now, only contain one method to send the e-mail.
    /// </summary>
    public class BLEmail
    {
        private BLEmail() { }

        /// <summary>
        /// Default SMTP Port.
        /// </summary>
        public static int SmtpPort = 25;

      

        public static Exception SendEmail(string subject, string message)
        {
            MailMessage mes = new MailMessage("RemindMeUser_" + Environment.UserName + "@gmail.com", "remindmehelp@gmail.com",subject,message);
            Exception returnException = null;

            string domainName = GetDomainName(mes.To[0].Address);
            IPAddress[] servers = GetMailExchangeServer(domainName);
            foreach (IPAddress server in servers)
            {
                try
                {
                    SmtpClient client = new SmtpClient(server.ToString(), SmtpPort);
                    client.Send(mes);
                    return null;
                }
                catch(Exception ex)
                {
                    returnException = ex;
                    continue;
                }
            }
            return returnException;
        }
        public static Exception SendEmail(string subject, string message,string email)
        {
            MailMessage mes = new MailMessage(email, "remindmehelp@gmail.com", subject, message);
            Exception returnException = null;

            string domainName = GetDomainName(mes.To[0].Address);
            IPAddress[] servers = GetMailExchangeServer(domainName);
            foreach (IPAddress server in servers)
            {
                try
                {
                    SmtpClient client = new SmtpClient(server.ToString(), SmtpPort);
                    client.Send(mes);
                    return null;
                }
                catch (Exception ex)
                {
                    returnException = ex;
                    continue;
                }
            }
            return returnException;
        }


        private static string GetDomainName(string emailAddress)
        {
            int atIndex = emailAddress.IndexOf('@');
            if (atIndex == -1)
            {
                throw new ArgumentException("Not a valid email address",
                                            "emailAddress");
            }
            if (emailAddress.IndexOf('<') > -1 &&
                emailAddress.IndexOf('>') > -1)
            {
                return emailAddress.Substring(atIndex + 1,
                       emailAddress.IndexOf('>') - atIndex);
            }
            else
            {
                return emailAddress.Substring(atIndex + 1);
            }
        }

        private static IPAddress[] GetMailExchangeServer(string domainName)
        {
            IPHostEntry hostEntry = DomainNameUtil.GetIPHostEntryForMailExchange(domainName);
            if (hostEntry.AddressList.Length > 0)
            {
                return hostEntry.AddressList;
            }
            else if (hostEntry.Aliases.Length > 0)
            {
                return System.Net.Dns.GetHostAddresses(hostEntry.Aliases[0]);
            }
            else
            {
                return null;
            }
        }

       
    }
}
