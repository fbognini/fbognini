using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace fbognini.Core.Utilities
{
    public class RegexUtilities
    {
        public static bool IsDigitsOnly(string str)
        {
            foreach (char c in str)
            {
                if (c < '0' || c > '9')
                    return false;
            }

            return true;
        }

        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                // Normalize the domain
                email = Regex.Replace(email, @"(@)(.+)$", DomainMapper,
                                      RegexOptions.None, TimeSpan.FromMilliseconds(200));

                // Examines the domain part of the email and normalizes it.
                string DomainMapper(Match match)
                {
                    // Use IdnMapping class to convert Unicode domain names.
                    var idn = new IdnMapping();

                    // Pull out and process domain name (throws ArgumentException on invalid)
                    string domainName = idn.GetAscii(match.Groups[2].Value);

                    return match.Groups[1].Value + domainName;
                }
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
            catch (ArgumentException)
            {
                return false;
            }

            try
            {
                return Regex.IsMatch(email,
                    @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                    RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }
    
        public static bool IsValidSdi(string sdi)
        {
            if (string.IsNullOrWhiteSpace(sdi))
                return false;

            try
            {
                return Regex.IsMatch(sdi, @"^[a-zA-Z0-9]{7}$", RegexOptions.IgnoreCase);
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }

        }


        public static async Task<string> ReplaceAsync(string input, string pattern, Func<Match, Task<string>> replacementFn)
        {
            var regex = new Regex(pattern);
            var sb = new StringBuilder();
            var lastIndex = 0;

            foreach (Match match in regex.Matches(input))
            {
                sb.Append(input, lastIndex, match.Index - lastIndex)
                  .Append(await replacementFn(match).ConfigureAwait(false));

                lastIndex = match.Index + match.Length;
            }

            sb.Append(input, lastIndex, input.Length - lastIndex);
            return sb.ToString();
        }
    }
}
