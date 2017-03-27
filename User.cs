using BuildServerUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using static BuildServer.BuildServerSettings;

namespace BuildServer
{
    public class User
    {
        public enum NotifySetting
        {
            kAlwaysNotify,
            kNotifyOnFailOnly
        }

        #region Properties and Fields

        private string Email { get; set; }

        private NotifySetting UserNotifySetting { get; set; } = NotifySetting.kNotifyOnFailOnly;

        public string NotifySettingString
        {
            set { UserNotifySetting = value == "Y" ? NotifySetting.kNotifyOnFailOnly : NotifySetting.kAlwaysNotify; }
        }

        #endregion

        public User(string email, string notifySetting)
        {
            Email = email;
            NotifySettingString = notifySetting;
        }

        /// <summary>
        /// Either sends back via comms or emails the inputted string in the string builder to me if there is no connection
        /// </summary>
        /// <param name="testRunInformation"></param>
        public void Message(string logFilePath, StringBuilder testRunInformation, string branchName, bool passed)
        {
            if (passed && UserNotifySetting != NotifySetting.kAlwaysNotify)
            {
                Console.WriteLine("Skipping message because the build passed and the user indicated not to email them.");
                return;
            }

            Console.WriteLine("Creating message");

            DateTime buildCompleteTime = DateTime.Now;

            testRunInformation.AppendLine("Build Request completed at " + buildCompleteTime.ToShortTimeString());

            string url = "http://" + ServerIP + ":" + ServerPort + CommandStrings.GetFailedTests + "?branch=" + branchName;
            testRunInformation.AppendLine("<a href=\"" + url + "\"/>");

            try
            {
                MailMessage mail = new MailMessage(ServerEmail, Email, (branchName + " - ") + (passed ? "Build Passed" : "Build Failed"), testRunInformation.ToString());

                Console.WriteLine("Attaching log from file " + logFilePath);
                mail.Attachments.Add(new Attachment(logFilePath));

                SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(ServerEmailUsername, ServerEmailPassword);
                client.EnableSsl = true;

                client.Send(mail);
                Console.WriteLine("Message sent to " + Email);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error occurred in sending email with message: " + e.Message);
            }
        }
    }
}
