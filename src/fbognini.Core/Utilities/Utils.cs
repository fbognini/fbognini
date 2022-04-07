using DnsClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace fbognini.Core.Utilities
{
    public static class Utils
    {
        private static Random random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static void SendMail(Exception exception, string storeId = null, object obj = null)
        {
            try
            {
                //using SmtpClient client = new SmtpClient()
                //{
                //    Host = "in.mailjet.com",
                //    Port = 587,
                //    Credentials = new NetworkCredential("17480485baf3a2d3206075e0d3d85b20", "12db1f7c47e63db937cf3d54ac9d1530"),
                //    EnableSsl = true,
                //    DeliveryMethod = SmtpDeliveryMethod.Network,
                //};
                using SmtpClient client = new SmtpClient()
                {
                    Host = "smtp.office365.com",
                    Port = 587,
                    Credentials = new NetworkCredential("dominositapp@dominositalia.it", "Jof97427!"),
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                };
                using MailMessage mail = new MailMessage
                {
                    From = new MailAddress("dominositapp@dominositalia.it")
                };
                mail.To.Add("f.bognini@dominositalia.it");
                mail.Subject = $"{Assembly.GetCallingAssembly().GetName().Name} - Exception PBO";
                mail.IsBodyHtml = false;

                var dic = new Dictionary<string, string>
                {
                    ["Message"] = exception.Message,
                    ["StackTrace"] = exception.StackTrace,
                };
                if (exception.InnerException != null)
                {
                    dic.Add("InnerException.Exception", exception.InnerException.Message);
                    dic.Add("InnerException.StackTrace", exception.InnerException.StackTrace);
                }

                //mail.Body = JsonSerializer.Serialize(dic);
                //if (obj != null)
                //    mail.Body = JsonSerializer.Serialize(obj) + Environment.NewLine + Environment.NewLine + mail.Body;
                //if (storeId != null)
                //    mail.Body = storeId + Environment.NewLine + Environment.NewLine + mail.Body;

                client.Send(mail);
            }
            catch (Exception)
            {

                //throw;
            }            
        }

        public static string GetCurrentMethodName()
        {
            StackTrace st = new StackTrace();
            StackFrame sf = st.GetFrame(1);

            return sf.GetMethod().Name;
        }

        public static List<TEnum> GetEnumList<TEnum>() where TEnum : Enum
            => ((TEnum[])Enum.GetValues(typeof(TEnum))).ToList();

        public static Dictionary<int, string> GetEnumDictionary<TEnum>() where TEnum : Enum
        {
            var type = typeof(TEnum);
            var values = Enum.GetValues(type).Cast<TEnum>();
            return values.ToDictionary(e => Convert.ToInt32(e), e => e.ToString());
        }

        public static DateTime FirstDateOfWeek(int year, int weekNum, CalendarWeekRule rule)
        {
            DateTime first = new DateTime(year, 1, 1);

            int daysOffset = DayOfWeek.Monday - first.DayOfWeek;
            DateTime firstMonday = first.AddDays(daysOffset);

            var cal = CultureInfo.CurrentCulture.Calendar;
            int firstWeek = cal.GetWeekOfYear(firstMonday, rule, DayOfWeek.Monday);

            if (firstWeek <= 1)
            {
                weekNum -= 1;
            }

            DateTime result = firstMonday.AddDays(weekNum * 7);

            return result;
        }

        public static bool IsFileLocked(FileInfo file)
        {
            try
            {
                using FileStream stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None);
                stream.Close();
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }

            //file is not locked
            return false;
        }


        public static async Task<bool> CheckEmailValidity(string email)
        {
            int GetResponseCode(string ResponseString)
            {
                return int.Parse(ResponseString.Substring(0, 3));
            }

            try
            {
                bool toReturn = true;

                var domain = email.Substring(email.IndexOf('@') + 1);
                var lookup = new LookupClient(
                    new LookupClientOptions() { 
                        Timeout = TimeSpan.FromSeconds(5)
                });

                var result = await lookup.QueryAsync(domain, QueryType.MX).ConfigureAwait(false);
                var records = result.Answers.MxRecords();

                if (!records.Any())
                {
                    return false;
                }

                TcpClient tClient = new TcpClient(records.First().Exchange.Value, 25);
                string CRLF = "\r\n";
                byte[] dataBuffer;
                string ResponseString;
                NetworkStream netStream = tClient.GetStream();
                StreamReader reader = new StreamReader(netStream);
                ResponseString = reader.ReadLine();
                /* Perform HELO to SMTP Server and get Response */
                dataBuffer = Encoding.ASCII.GetBytes("HELO KirtanHere" + CRLF);
                netStream.Write(dataBuffer, 0, dataBuffer.Length);
                ResponseString = reader.ReadLine();
                dataBuffer = Encoding.ASCII.GetBytes("MAIL FROM:<dominositapp@dominositalia.it>" + CRLF);
                netStream.Write(dataBuffer, 0, dataBuffer.Length);
                ResponseString = reader.ReadLine();
                /* Read Response of the RCPT TO Message to know from google if it exist or not */
                dataBuffer = Encoding.ASCII.GetBytes("RCPT TO:<" + email + ">" + CRLF);
                netStream.Write(dataBuffer, 0, dataBuffer.Length);
                ResponseString = reader.ReadLine();
                if (GetResponseCode(ResponseString) == 550)
                {
                    toReturn = false;
                }
                /* QUITE CONNECTION */
                dataBuffer = Encoding.ASCII.GetBytes("QUITE" + CRLF);
                netStream.Write(dataBuffer, 0, dataBuffer.Length);
                tClient.Close();

                return toReturn;
            }
            catch
            {
                return false;
            }
        }

        public static string LocalToUNC(this string localPath, string server)
        {
            var unc = @$"\\{server}\{localPath.Replace(@":\", @"$\")}";
            return unc;
        }

        /// <summary>
        /// x => x.OrderLines.First().Product.RetailerId returns Product.RetailerId if ignoreMethods = false, otherwise OrderLines.Product.RetailerId
        /// </summary>
        public static string GetPropertyPath<T>(Expression<Func<T, object>> expression, bool ignoreMethods = false)
        {
            return string.Join(".", GetPropertyNames(expression, ignoreMethods));
        }

        /// <summary>
        /// x => x.OrderLines.First().Product.RetailerId returns Product.RetailerId if ignoreMethods = false, otherwise OrderLines.Product.RetailerId
        /// </summary>
        public static  IEnumerable<string> GetPropertyNames<T>(Expression<Func<T, object>> expression, bool ignoreMethods = false)
        {
            var body = expression.Body as MemberExpression;

            if (body == null)
            {
                body = ((UnaryExpression)expression.Body).Operand as MemberExpression;
            }

            return GetPropertyNames(body, ignoreMethods);
        }

        public static IEnumerable<string> GetPropertyNames(MemberExpression body, bool ignoreMethods)
        {
            var names = new List<string>();

            while (body != null)
            {
                names.Add(body.Member.Name);
                var inner = body.Expression;
                switch (inner.NodeType)
                {
                    case ExpressionType.MemberAccess:
                        body = inner as MemberExpression;
                        break;
                    case ExpressionType.Call:
                        if (ignoreMethods)
                        {
                            var call = inner as MethodCallExpression;
                            body = call.Arguments[0] as MemberExpression;
                        }
                        else
                        {
                            body = null;
                        }
                        break;
                    default:
                        body = null;
                        break;

                }
            }

            names.Reverse();

            return names;
        }

    }
}
