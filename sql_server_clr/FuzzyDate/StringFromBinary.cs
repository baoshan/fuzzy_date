using Microsoft.SqlServer.Server;
using System.Data.SqlTypes;

public partial class FuzzyDate
{
  /// <summary>
  /// Parse a fuzzy date binary into a short string.
  /// </summary>
  [SqlFunction(IsDeterministic = true, IsPrecise = true)]
  public static SqlString StringFromBinary(SqlBinary sql_binary)
  {
    // NULL in NULL out.
    if (sql_binary.IsNull) { return SqlString.Null; }

    // Get the input binary.
    var bytes = sql_binary.Value;

    // The string to be populated.
    string result;

    // Separate the year, month, and day as it's a regular date.
    var year = (bytes[0] << 4 | bytes[1] >> 4) - 1024;
    var month = (bytes[1] & 0x0F) - 1;
    var day = bytes[2] >> 3;

    // When the binary does not have a year part:
    if (year == -1024)
    {
      result = "D-" + month;
      if (day > 0) { result += "-" + day; }
    }

    // When the binary has a year part:
    else
    {
      // 1 BC before 1 AD.
      var before_christ = year < 1;

      // Flow control according to month bits:
      switch (month)
      {
        // A Decade.
        case 0x00 - 1:

          // Valid BC decades start from:
          //   * -18 (10s BC starts from 19 BC)
          //   * -8 (0s BC starts from 9 BC)
          //   * ...
          if (before_christ) { result = -8 - year + "sBC"; }

          // Valid AD decades start from:
          //   * 1 (0s BC starts from 1 AD)
          //   * 10 (10 BC starts from 10 AD)
          //   * ...
          else { result = (year == 1 ? 0 : year) + "s"; }

          // A fuzzy date spanning multiple decades.
          if (day > 0)
          {
            result += "+" + 10 * day;
          }
          break;

        // A fuzzy date with only year part.
        case 0x01 - 1:
          result = (before_christ ? 1 - year + "BC" : year.ToString());
          break;

        // A fuzzy date spanning multi-years.
        case 0x0F - 1:
          result = (before_christ ? 1 - year + "BC" : year.ToString()) + "+" + (day + 1);
          break;

        // A fuzzy date with a month part.
        default:
          result = (before_christ ? 1 - year + "BC" : year.ToString()) + "-" + month;
          if (day > 0) { result += "-" + day; }
          break;
      }
    }

    // Flag Bit B: Accuracy.
    if ((bytes[2] & 0x02) == 0) { result = "c." + result; }

    // Flag Bit A: Certainty.
    if ((bytes[2] & 0x04) == 0) { result = "?" + result; }

    // Flag Bit C: Special flag.
    if ((bytes[2] & 0x01) == 0) { result = "fl." + result; }

    // Return the populated string.
    return new SqlString(result);
  }
}