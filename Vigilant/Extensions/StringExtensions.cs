using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Vigilant.Extensions {
    static class StringExtensions
    {

        /// <summary>
        /// Uses regex to replace patterns in a string
        /// </summary>
        /// <param name="pattern">The pattern to replace</param>
        /// <param name="replace">The replacement for the pattern</param>
        /// <returns>the string with the patterns replaced</returns>
        public static string RReplace(this string source, string pattern, string replace) =>
            Regex.Replace(source, pattern, replace);
    }
}
