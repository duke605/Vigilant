using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vigilant.Extensions {
    static class NumberExtensions {

        /// <summary>
        /// Converts a string into a ulong
        /// </summary>
        /// <returns>the ulong represented by the string</returns>
        public static ulong ToUlong(this string source) =>
            ulong.Parse(source);
    }
}
