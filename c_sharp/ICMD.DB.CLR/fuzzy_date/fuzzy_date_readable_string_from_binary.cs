using Microsoft.SqlServer.Server;
using System.Data.SqlTypes;

namespace ICMD.DB.CLR
{
  public partial class UDFs
  {
    /// <summary>
    /// Parse a fuzzy date binary into its readable (en-us) version.
    /// </summary>
    [SqlFunction(IsDeterministic = true, IsPrecise = true)]
    public static SqlString fuzzy_date_readable_string_from_binary(SqlBinary sql_binary)
    {
      // NULL in NULL out.
      if (sql_binary.IsNull) { return SqlString.Null; }

      // Get the input binary.
      var bytes = sql_binary.Value;

      // The string to be populated.
      string result;

      // Separate the year, month, and day as it's a regular date.
      var year = (bytes[0] << 4 | bytes[1] >> 4) - 1024;
      var month_index = (bytes[1] & 0x0F) - 2;
      var day = bytes[2] >> 3;

      // When the binary does not have a year part:
      if (year == -1024)
      {
        result = months[month_index];
        if (day > 0) { result += " " + day; }
      }

      // When the binary has a year part:
      else
      {
        // 1 BC before 1 AD.
        var before_christ = year < 1;
        
        // Flow control according to month bits:
        switch (month_index)
        {
          // A Decade.
          case 0x00 - 2:

            // Valid BC decades start from:
            //   * -18 (10s BC starts from 19 BC)
            //   * -8 (0s BC starts from 9 BC)
            //   * ...
            if (before_christ) { result = -8 - year + "s BC"; }

            // Valid AD decades start from:
            //   * 1 (0s BC starts from 1 AD)
            //   * 10 (10 BC starts from 10 AD)
            //   * ...
            else { result = (year == 1 ? 0 : year) + "s"; }

            // A fuzzy date spanning multiple decades.
            if (day > 0)
            {
              var end_decade_year = year + 10 * day;
              var end_decade_before_christ = end_decade_year < 1;
              result += " ¨C " + (end_decade_before_christ ? -8 - end_decade_year + "s BC" : before_christ ? end_decade_year - 2 + "s AD" : end_decade_year + "s");
            }
            break;

          // A fuzzy date with only year part.
          case 0x01 - 2:
            result = before_christ ? (1 - year) + " BC" : year.ToString();
            break;

          // A fuzzy date spanning multi-years.
          case 0x0F - 2:
            result = (before_christ ? 1 - year + " BC" : year.ToString()) + " ¨C ";
            var end_year = year + day + 1;
            var end_year_before_christ = end_year < 1;
            result += end_year_before_christ ? 1 - end_year + " BC" : before_christ ? end_year + " AD" : end_year.ToString();
            break;

          // A fuzzy date with a month part.
          default:
            result = months[month_index] +
              (day == 0 ? " " : " " + day + ", ") +
              (before_christ ? (1 - year) + " BC" : year.ToString());
            break;
        }

        // Flag Bit B: Accuracy.
        if ((bytes[2] & 0x02) == 0) { result = "c. " + result; }

        // Flag Bit A: Certainty.
        if ((bytes[2] & 0x04) == 0) { result = "? " + result; }

        // Flag Bit C: Special flag.
        if ((bytes[2] & 0x01) == 0) { result = "fl. " + result; }
      }

      // Return the populated string.
      return new SqlString(result);
    }

    // Localized months.
    static readonly string[] months =
    { 
      "January",
      "February",
      "March",
      "April",
      "May",
      "June",
      "July",
      "August",
      "September",
      "October",
      "November",
      "December"
    };
  }
}