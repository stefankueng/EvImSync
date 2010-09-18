// EvImSync - A tool to sync Evernote notes to IMAP mails and vice versa
// Copyright (C) 2010 - Stefan Kueng

// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.IO;
using System.Net.Mail;
using System.Reflection;
using System.Text;

namespace EveImSync
{
    /// <summary>
    /// Extension class to extend the MailMessage
    /// </summary>
    public static class MailMessageExt
    {
        /// <summary>
        /// Returns the email message as a string in eml format
        /// </summary>
        /// <param name="message">the MailMessage object</param>
        /// <returns>the eml string</returns>
        public static string GetEmailAsString(this MailMessage message)
        {
            string email = null;
            Assembly assembly = typeof(SmtpClient).Assembly;
            Type mailWriterType =
              assembly.GetType("System.Net.Mail.MailWriter");

            using (MemoryStream stringStream =
                   new MemoryStream())
            {
                // Get reflection info for MailWriter constructor
                ConstructorInfo mailWriterContructor =
                    mailWriterType.GetConstructor(
                        BindingFlags.Instance | BindingFlags.NonPublic,
                        null,
                        new Type[] { typeof(Stream) },
                        null);

                // Construct MailWriter object with our FileStream
                object mailWriter =
                  mailWriterContructor.Invoke(new object[] { stringStream });

                // Get reflection info for Send() method on MailMessage
                MethodInfo sendMethod =
                    typeof(MailMessage).GetMethod(
                        "Send",
                        BindingFlags.Instance | BindingFlags.NonPublic);

                // Call method passing in MailWriter
                sendMethod.Invoke(
                    message,
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null,
                    new object[] { mailWriter, true },
                    null);

                // Finally get reflection info for Close() method on our MailWriter
                MethodInfo closeMethod =
                    mailWriter.GetType().GetMethod(
                        "Close",
                        BindingFlags.Instance | BindingFlags.NonPublic);

                // Call close method
                closeMethod.Invoke(
                    mailWriter,
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null,
                    new object[] { },
                    null);

                email = Encoding.UTF8.GetString(stringStream.ToArray());
            }

            return email;
        }
    }
}
