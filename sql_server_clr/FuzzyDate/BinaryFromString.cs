using Microsoft.SqlServer.Server;
using System.Data.SqlTypes;
using System.Text.RegularExpressions;

public partial class FuzzyDate
{
  // Compiled static regex to separate digits.
  static readonly Regex regex = new Regex(@"\d+", RegexOptions.Compiled);

  /// <summary>
  /// Parse a short string into a fuzzy date binary.
  /// </summary>
  [SqlFunction(IsDeterministic = true, IsPrecise = true)]
  public static SqlBinary BinaryFromString(SqlString sql_string)
  {
    // NULL in NULL out.
    if (sql_string.IsNull) { return SqlBinary.Null; }

    // Any exception due to invalid input leads to SqlBytes.Null be returned.
    try
    {
      // Get the input string (w/o whitespaces in lowercase).
      var input = sql_string.Value.Replace(" ", "").ToLower();

      // Initialize the output bytes.
      byte[] bytes = { 0, 0, 0 };

      // Search the string for all continuous digits.
      var matches = regex.Matches(input);

      // When the string does not have a year part:
      if (input.Contains("d"))
      {
        // 1. Calculate the month bits.
        // 2. Calculate the day bits if the string contains a day part.
        bytes[1] = (byte)(byte.Parse(matches[0].Value) + 1);
        if (matches.Count > 1) { bytes[2] = (byte)(byte.Parse(matches[1].Value) << 3); }
      }

      // When the string has a year part:
      else
      {
        // Parse the year part.
        var year = int.Parse(matches[0].Value);

        if (input.Contains("bc"))
        {
          // Valid BC decades start from:
          //   * -18 (10s BC starts from 19 BC)
          //   * -8 (0s BC starts from 9 BC)
          //   * ...
          if (input.Contains("s")) { year = -8 - year; }

          // 1 BC before 1 AD
          else { year = 1 - year; }
        }

        // 0s starts from 1 AD.
        else if (year == 0) { year = 1; }

        // Calculate year bits.
        year += 1024;
        bytes[0] = (byte)(year >> 4);
        bytes[1] = (byte)(year << 4);

        if (input.Contains("+"))
        {
          // For fuzzy decades spanning multiple years, 1 day bit equals 10 years.
          if (input.Contains("s"))
          {
            bytes[2] = (byte)(byte.Parse(matches[1].Value) / 10 << 3);
          }

          // For fuzzy dates spanning multiple years:
          // 1. Month bits are `0b1111`;
          // 2. Calculate day bits (spanning years).
          else
          {
            bytes[1] |= 0x0F;
            bytes[2] = (byte)(byte.Parse(matches[1].Value) - 1 << 3);
          }
        }

        // For other fuzzy dates that aren't decades:
        // 1. Calculate month bits;
        // 2. Calculate day bits.
        else if (!input.Contains("s"))
        {
          bytes[1] |= (byte)(matches.Count > 1 ? (byte.Parse(matches[1].Value) + 1) : 1);
          bytes[2] = (byte)(matches.Count > 2 ? (byte.Parse(matches[2].Value) << 3) : 0);
        }
      }

      // Flag Bit A: Certainty.
      if (!input.Contains("?")) { bytes[2] |= 0x04; }

      // Flag Bit B: Accuracy.
      if (!input.Contains("c.")) { bytes[2] |= 0x02; }

      // Flag Bit C: Special flag.
      if (!input.Contains("f")) { bytes[2] |= 0x01; }

      // Prepare output binary.
      var sql_binary = new SqlBinary(bytes);

      // Ensure the output is a valid fuzzy date binary.
      if (!IsValidBinary(sql_binary)) { return SqlBinary.Null; }

      // Ensure the output is the same as input.
      if (input.Replace(".", "") != StringFromBinary(sql_binary).Value.Replace(" ", "").Replace(".", "").ToLower()) { return SqlBinary.Null; }

      // Return the parsed binary.
      return sql_binary;
    }

    // The parser returns SqlBytes.Null for all invalid inputs.
    catch { return SqlBinary.Null; }
  }
}