using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using GlobalReportingSystem.Core.Abstract.ProviderInterfaces;

namespace GlobalReportingSystem.BL.Implementation
{
    public class Emailer : IEmailer
    {
        MailMessage msg;
        private void AddItems(string strFrom, List<string> To, string strBody, string strSubject, string[] arrAttachments, bool HtmlBody)
        {
            msg = new MailMessage();
            try
            {
                To.ForEach(p => msg.To.Add(p));

                msg.From = new MailAddress(strFrom);

                msg.Subject = strSubject;
                msg.IsBodyHtml = true;
                msg.Body = strBody;
                msg.IsBodyHtml = HtmlBody;
                //Attaching settings file :) 

                foreach (string strF in arrAttachments)
                {
                    Attachment attach = new Attachment(strF);
                    msg.Attachments.Add(attach);
                }
            }
            catch (Exception e)
            {
                string strMessage = String.Format("Exception appears during defining the parameters of email. Message {0}", e.Message);
                Console.WriteLine(strMessage);
                throw new Exception(strMessage);
            }
        }

        private void Send(string strEmailServer, int strPort, string strUser, string strPassword)
        {

            SmtpClient smtp = new SmtpClient();
            smtp.Host = strEmailServer;
            smtp.Timeout = 600000;

            if (strPort == 587)
            {
                smtp.Port = 587;
                smtp.EnableSsl = true;
            }
            else
            {
                smtp.Port = 25;
            }

            if ((strUser == "username") || (strPassword == "password"))
            {
                smtp.UseDefaultCredentials = true;
            }
            else
            {
                smtp.Credentials = new NetworkCredential(strUser, strPassword);                
            }
            try
            {
                smtp.Send(msg);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }

        }

        public void SendMail(List<string> To, string from, string subj, string text, string[] attachments, bool HtmlBody)
        {
            try
            {
                AddItems(from, To, text, subj, attachments, HtmlBody);
                //Send("relay.int.westgroup.com", 25, "qcadmin", "passw0rd!");
                Send("smtp.gmail.com", 587, "grs.clarivate@gmail.com", "GRS_Clarivate_123");
                //Send("smtp.gmail.com", 587, "prod.consistency@gmail.com", "ClarivateIndia@123");                
            }
            catch
            {
            }
            //catch(Exception ex) {JSONER.LogMessage("Email wasn't sent to: "+To.First()+"  "+DateTime.Now+": "+ex.Message+" Inner: "+ ex.InnerException); }
        }

        public void SendMail(List<string> To, string from, string subj, string text)
        {
            SendMail(To, from, subj, text, new string[] { }, true);
        }

        //public void SendMailByGmail(List<string> To, string from, string Subject, 
    }
}