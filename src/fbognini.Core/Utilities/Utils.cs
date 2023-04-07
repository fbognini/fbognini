using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Mail;
using System.Reflection;

namespace fbognini.Core.Utilities
{
    public static class Utils
    {
        private static readonly Random random = new Random();
        public static string RandomString(int length, bool uppercase = true, bool lowercase = true, bool number = true)
        {
            if (!uppercase && !lowercase && !number)
                throw new ArgumentException($"{nameof(uppercase)}, {nameof(lowercase)} or {nameof(number)} must be true");

            const string lowers = "abcdefghijklmnopqrstuvwxyz";
            const string uppers = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string numbers = "0123456789";

            string chars = string.Concat(uppercase ? uppers : string.Empty, lowercase ? lowers : string.Empty, number ? numbers : string.Empty);
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static string GetCurrentMethodName()
        {
            StackTrace st = new StackTrace();
            StackFrame sf = st.GetFrame(1);

            return sf.GetMethod().Name;
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


        public static string LocalToUNC(this string localPath, string server)
        {
            var unc = @$"\\{server}\{localPath.Replace(@":\", @"$\")}";
            return unc;
        }

    }
}
