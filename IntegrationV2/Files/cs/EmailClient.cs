namespace IntegrationV2
{
	using System;
	using System.Collections.Generic;
	using EmailContract.DTO;
	using IntegrationApi.MailboxDomain.Interfaces;
	using IntegrationApi.MailboxDomain.Model;
	using Terrasoft.Common;
	using Terrasoft.Core;
	using Terrasoft.Core.Factories;
	using Terrasoft.EmailDomain.Interfaces;
	using Terrasoft.Mail;
	using Terrasoft.Mail.Sender;
	using Credentials = EmailContract.DTO.Credentials;
	using Mail = Terrasoft.Mail;

	#region Class: EmailClient

	/// <summary>Represents email client class.</summary>
	[DefaultBinding(typeof(IEmailClient), Name = "EmailClient")]
	public class EmailClient : IEmailClient
	{

		#region Fields: Private

		protected readonly UserConnection _userConnection;
		private readonly Credentials _credentials;
		private readonly IEmailService _emailService;

		#endregion

		#region Constructors: Public

		/// <summary>
		/// <see cref="EmailClient"/> ctor.
		/// </summary>
		/// <param name="userConnection">An instance of the user connection.</param>
		public EmailClient(UserConnection userConnection) {
			_userConnection = userConnection;
			_emailService = ClassFactory.Get<IEmailService>(
				new ConstructorArgument("uc", _userConnection));
		}

		/// <summary>
		/// <see cref="EmailClient"/> ctor.
		/// </summary>
		/// <param name="userConnection">An instance of the user connection.</param>
		/// <param name="credentials">Short email connection credentials.</param>
		public EmailClient(UserConnection userConnection, Mail.Credentials credentials)
			: this(userConnection) {
			var mailServer = GetMailServer(credentials.ServerId);
			_credentials = new Credentials {
				UserName = credentials.UserName,
				Password = credentials.UserPassword,
				UseOAuth = credentials.UseOAuth,
				ServiceUrl = mailServer.ServerAddress,
				ServerTypeId = mailServer.TypeId,
				Port = mailServer.Port,
				UseSsl = mailServer.UseSsl
			};
		}

		/// <summary>
		/// <see cref="EmailClient"/> ctor.
		/// </summary>
		/// <param name="userConnection">An instance of the user connection.</param>
		/// <param name="emailCredentials">Full email connection credentials.</param>
		public EmailClient(UserConnection userConnection, Credentials emailCredentials)
			: this(userConnection) {
			_credentials = emailCredentials;
		}

		#endregion

		#region Methods: Private

		/// <summary>
		/// Set email headers.
		/// </summary>
		/// <param name="email"><see cref="Email"/> instance.</param>
		/// <param name="headerProperties">List of <see cref="EmailMessageHeader"/>.</param>
		private void SetEmailHeaders(Email email, List<EmailMessageHeader> headerProperties) {
			var headers = new List<string>();
			if (headerProperties != null) {
				foreach (var property in headerProperties) {
					headers.Add(string.Concat(property.Name, "=", property.Value));
				}
			}
			email.Headers = headers;
		}

		/// <summary>
		/// Fills recipients from <see cref="EmailMessage"/> to <see cref="Email"/>.
		/// </summary>
		/// <param name="email"><see cref="Email"/> instance.</param>
		/// <param name="emailMessage"><see cref="EmailMessage"/> instance.</param>
		private void SetEmailRecipients(Email email, EmailMessage emailMessage) {
			FillMessageRecipientsCollection(email.Recepients, emailMessage.To);
			FillMessageRecipientsCollection(email.CopyRecepients, emailMessage.Cc);
			FillMessageRecipientsCollection(email.BlindCopyRecepients, emailMessage.Bcc);
		}

		/// <summary>
		/// Fills <paramref name="collection"/> recipients collection with <paramref name="values"/>.
		/// </summary>
		/// <param name="collection">Exchange email recipients collection.</param>
		/// <param name="values">Recipients values.</param>
		private void FillMessageRecipientsCollection(List<string> collection, List<string> values) {
			foreach (var address in values) {
				collection.Add(address);
			}
		}

		/// <summary>
		/// Returns <see cref="Email"/> instance.</summary>
		/// <param name="emailMessage"><see cref="EmailMessage"/> instance.</param>
		/// <returns><see cref="Email"/> instance.</returns>
		private Email GetEmail(EmailMessage emailMessage) {
			var email = new Email {
				Id = emailMessage.GetMessageId(),
				Subject = emailMessage.Subject,
				Body = emailMessage.Body,
				Sender = emailMessage.From,
				Importance = (EmailContract.EmailImportance)emailMessage.Priority,
				IsHtmlBody = emailMessage.IsHtmlBody
			};
			SetEmailRecipients(email, emailMessage);
			SetEmailHeaders(email, emailMessage.HeaderProperties);
			SetAttachments(email, emailMessage.Attachments);
			return email;
		}

		/// <summary>
		/// Fill <see cref="Email.Attachments"/> collection.</summary>
		/// <param name="emailMessage"><see cref="Email"/> instance.</param>
		/// <param name="emailMessage"><see cref="EmailAttachment"/> collection.</param>
		private void SetAttachments(Email email, List<EmailAttachment> attachments) {
			var attachmentsDto = new List<Attachment>();
			foreach (EmailAttachment attachment in attachments) {
				var attachmentDto = new Attachment {
					Id = attachment.Id.ToString(),
					Name = attachment.Name,
					IsInline = attachment.IsContent
				};
				attachmentDto.SetData(attachment.Data);
				attachmentsDto.Add(attachmentDto);
			}
			email.Attachments = attachmentsDto;
		}

		/// <summary>
		/// Get send email credentials.
		/// </summary>
		/// <param name="from">Sender email address.</param>
		/// <param name="ignoreRights">Ignore mailbox rights flag.</param>
		/// <returns><see cref="Credentials"/> instance.</returns>
		private Credentials GetCredentials(string from, bool ignoreRights) {
			return _credentials == null
				? GetEmailCredentialsFromMailbox(from, ignoreRights)
				: GetEmailCredentials(from);
		}

		/// <summary>
		/// Returns <see cref="Credentials"/> instance.</summary>
		/// <param name="from">Sender email.</param>
		/// <param name="ignoreRights">Ignore mailbox rights flag.</param>
		/// <returns><see cref="Credentials"/> instance.</returns>
		private Credentials GetEmailCredentialsFromMailbox(string from, bool ignoreRights) {
			var mailboxService = ClassFactory.Get<IMailboxService>(new ConstructorArgument("uc", _userConnection));
			var mailbox = mailboxService.GetMailboxBySenderEmailAddress(from, !ignoreRights, false);
			return mailbox.ConvertToSynchronizationCredentials(_userConnection);
		}

		/// <summary>
		/// Returns <see cref="Credentials"/> instance from <see cref="Mail.Credentials"/> instance.</summary>
		/// <param name="from">Sender email.</param>
		/// <returns><see cref="Credentials"/> instance.</returns>
		private Credentials GetEmailCredentials(string from) {
			var credentials = _credentials;
			credentials.SenderEmailAddress = from;
			return credentials;
		}

		/// <summary>
		/// Returns mail server instance.</summary>
		/// <param name="serverId">Mail server identifier.</param>
		/// <returns>Mail server instance.</returns>
		private MailServer GetMailServer(Guid serverId) {
			var service = ClassFactory.Get<IMailServerService>(new ConstructorArgument("uc", _userConnection));
			return service.GetServer(serverId, false);
		}

		#endregion

		#region Methods: Public

		/// <summary>
		/// Preparing email by <paramref name="emailMessage"/> data and send it.</summary>
		/// <param name="emailMessage">Email message data.</param>
		/// <param name="ignoreRights">Flag that indicates whether to ignore rights.</param>
		public void Send(EmailMessage emailMessage, bool ignoreRights = false) {
			var emailDto = GetEmail(emailMessage);
			var credentials = GetCredentials(emailMessage.From, ignoreRights);
			var sendResult = _emailService.Send(emailDto, credentials);
			if (sendResult.IsNotNullOrEmpty()) {
				throw new EmailException("ErrorOnSend", sendResult);
			}
		}

		#endregion

	}

	#endregion

}
