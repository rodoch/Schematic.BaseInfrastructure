using System;
using System.Globalization;
using System.Text;
using System.Web;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Schematic.Core;
using Schematic.Identity;

namespace Schematic.Infrastructure.Email
{
    public class ForgotPasswordEmail : IForgotPasswordEmail<User>
    {
        private readonly IOptionsMonitor<SchematicSettings> _settings;
        private readonly IStringLocalizer<ForgotPasswordEmail> _localizer;

        public ForgotPasswordEmail(
            IOptionsMonitor<SchematicSettings> settings,
            IStringLocalizer<ForgotPasswordEmail> localizer)
        {
            _settings = settings;
            _localizer = localizer;
        }

        public string Subject(string applicationName = "")
        {
            applicationName = (applicationName.HasValue()) ? applicationName : _settings.CurrentValue.ApplicationName;
            string subject = String.Format(_localizer["Reset password for your {0} account"].Value, applicationName);
            return subject;
        }

        public string Body(User user, string domain, string subject, string token)
        {
            StringBuilder sb = new StringBuilder();

            var applicationName = _settings.CurrentValue.ApplicationName;
            var contactEmail = _settings.CurrentValue.ContactEmail;
            var uiLocale = CultureInfo.CurrentCulture.Name;
            var sender = _settings.CurrentValue.Email.FromDisplayName;
            var senderContactEmail = $"<a href=\"mailto:{contactEmail}\">" + contactEmail + "</a>";
            var userFullName = user.FullName;
            var userEmail = $"<a href=\"mailto:{user.Email}\">" + user.Email + "</a>";
            var title = _localizer[subject];

            token = HttpUtility.UrlEncode(token);

            sb.Append("<!doctype html>");
            sb.AppendLine("<html>");
            sb.AppendLine("<head>");
            sb.AppendLine("<meta name=\"viewport\" content=\"width=device-width\" />");
            sb.AppendLine("<meta http-equiv=\"Content-Type\" content=\"text/html; charset=UTF-8\" />");
            sb.AppendLine("<title>");
            sb.Append(title);
            sb.AppendLine("</title>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");

            var greeting = _localizer["Hello"] + ",";
            var intro = _localizer["You have requested a new password for the {0} account for {1}."].Value;
            intro = String.Format(intro, applicationName, userEmail);
            var please = _localizer["Please click on the link below and you will be taken to a page where you can update your password."];
            var link = $"<a href=\"https://{domain}/{uiLocale}/in/set?token={token}\">" + _localizer["Reset my password"] + " »</a>";
            var questions = _localizer[@"If you did not request a new password, or if you are not {0}, you can ignore this message. 
                If you would like further information, please contact us at {1}."].Value;
            questions = String.Format(questions, userFullName, senderContactEmail);
            var thanks = _localizer["Thank you"] + ",";
            var signature = _localizer[sender];

            sb.AppendParagraph(greeting);
            sb.AppendParagraph(intro);
            sb.AppendParagraph(please);
            sb.AppendParagraph(link);
            sb.AppendParagraph(questions);
            sb.AppendParagraph(thanks);
            sb.AppendParagraph(signature);

            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            string body = sb.ToString();
            return body;
        }
    }
}