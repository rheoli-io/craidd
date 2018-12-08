using System;
using MailKit.Net.Smtp;

namespace Craidd.Config
{
    public class EmailsServiceOptions
    {
        /// <summary>
        /// Gets or sets the name or IP Address of the host used for SMTP transactions.
        /// </summary>
        public string host { get; set; }

        /// <summary>
        /// Gets or sets the port used for SMTP transactions.
        /// </summary>
        public int port { get; set; } = 25;

        /// <summary>
        /// Specify whether the <see cref="SmtpClient"/> uses Secure Sockets Layer (SSL) to encrypt the connection.
        /// </summary>
        public bool useSSL { get; set; } = false;

        /// <summary>
        /// Gets or sets the account used for SMTP transactions.
        /// </summary>
        public string username { get; set; }

        /// <summary>
        /// Gets or sets the password used for SMTP transactions.
        /// </summary>
        public string password { get; set; }

        /// <summary>
        /// Gets or sets the email for the from address.
        ///
        public string fromEmail { get; set; }

        /// <summary>
        /// Gets or sets the name for the from address.
        /// </summary>
        public string fromName { get; set; }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(host))
            {
                throw new ArgumentNullException(nameof(host));
            }
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentNullException(nameof(username));
            }
            if (password == null)
            {
                throw new ArgumentNullException(nameof(password));
            }
            if (fromName == null)
            {
                throw new ArgumentNullException(nameof(fromName));
            }
            if (fromEmail == null)
            {
                throw new ArgumentNullException(nameof(fromEmail));
            }
        }
    }
}
