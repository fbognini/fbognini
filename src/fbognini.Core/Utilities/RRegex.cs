using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace fbognini.Core.Utilities
{
    public static class RRegex
    {
        //public static DateTime? Max(DateTime? a, DateTime? b)
        //{
        //    if (!a.HasValue)
        //        return b;

        //    if (!b.HasValue)
        //        return a;

        //    return new DateTime(Math.Max(a.Value.Ticks, a.Value.Ticks));
        //}

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
