using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using GlobalReportingSystem.Core.Abstract.ProviderInterfaces;

namespace GlobalReportingSystem.Core.Abstract.ProviderInterfaces
{
    public interface IEmailer
    {
        void SendMail(List<string> To, string from, string subj, string text, string[] attachments, bool HtmlBody);

        void SendMail(List<string> To, string from, string subj, string text);
    }
}