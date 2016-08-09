using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Vigilant.Utils {
    class TableFormatter 
        {
        public static String MakeTable<T>(IEnumerable<T> list, string[] headers = null) {
            PropertyInfo[] props = list.ElementAt(0).GetType().GetProperties();
            string table = "";
            int[] colLength = new int[props.Length];
            int width;

            // Setting column headers
            if (headers == null) {
                headers = new string[props.Length];
                for (int i = 0; i < props.Length; i++)
                    headers[i] = props[i].Name;
            }

            // Determing the max length of columns
            for (int i = 0; i < headers.Length; i++) {
                colLength[i] = list.Aggregate(headers[i].Length, (acc, t) => {
                    var value = props[i].GetValue(t);
                    int length = 0;

                    // Applying thousand seperators if possible
                    if (value is decimal)
                        length = ((decimal)value).ToString("#,##0.0").Length;
                    else if (value is int)
                        length = ((int)value).ToString("#,##0").Length;
                    else if (value is Int64)
                        length = ((Int64)value).ToString("#,##0").Length;
                    else
                        length = value.ToString().Length;

                    return length > acc ? length : acc;
                });
            }

            width = colLength.Sum() + (colLength.Length * 3) + 1;

            // Building table
            table += "." + "".PadRight(width - 2, '-') + ".\r\n";

            // Headers
            for (int i = 0; i < props.Length; i++) {
                table += "| " + headers[i].PadRight(colLength[i]) + " ";
            }

            table += "|\r\n|" + "".PadRight(width - 2, '-') + "|";

            for (int row = 0; row < list.Count(); row++) {
                table += "\r\n";

                for (int col = 0; col < props.Length; col++) {
                    var data = props[col].GetValue(list.ElementAt(row));
                    table += "| ";

                    // Data
                    if (data is decimal)
                        table += ((decimal)data).ToString("#,##0.0").PadRight(colLength[col]);
                    else if (data is int)
                        table += ((int)data).ToString("#,##0").PadRight(colLength[col]);
                    else if (data is Int64)
                        table += ((Int64)data).ToString("#,##0").PadRight(colLength[col]);
                    else
                        table += data.ToString().PadRight(colLength[col]);

                    table += " ";
                }

                table += "|";
            }

            table += "\r\n'" + "".PadRight(width - 2, '-') + "'";

            return table;
        }
    }
}
