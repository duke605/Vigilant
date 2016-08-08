using System.Linq;
using Fclp;
using Fclp.Internals.Extensions;

namespace Vigilant.Utils {
    class HelpFormatter {

        public static string GetHelpForCommand(FluentCommandLineParser parser) {
            var lines = parser.Options.Select(o => {
                string options = "";

                // Adding shortname
                options += o.HasShortName ? $"-{o.ShortName}, " : "";

                // Adding long name
                options += $"--{o.LongName}";

                return new {
                    OptionName = options,
                    Descrption = $"({(o.IsRequired ? "Required" : "Optional")}) {o.Description}"
                };
            });

            int longestOption = lines.Aggregate(0, (acc, e) => e.OptionName.Length > acc ? e.OptionName.Length : acc) + 1;
            string help = "";

            lines.ForEach(l => help += l.OptionName.PadRight(longestOption) + l.Descrption + "\r\n");
            help = help.TrimEnd('\r', '\n');

            return help;
        }
    }
}
