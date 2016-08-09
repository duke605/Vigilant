using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vigilant.Extensions {
    static class PrimitiveExtensions
    {

        /// <summary>
        /// Outputs human readabe strings for bools
        /// </summary>
        /// <param name="true">The text if the bool is true</param>
        /// <param name="false">The text if the bool is false</param>
        public static string ToReadable(this bool s, string @true, string @false)
            => s ? @true : @false;
    }
}
