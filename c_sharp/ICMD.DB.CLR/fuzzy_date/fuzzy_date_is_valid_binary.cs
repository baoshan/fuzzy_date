using Microsoft.SqlServer.Server;
using System;
using System.Data.SqlTypes;

namespace ICMD.DB.CLR
{
  public partial class UDFs
  {
    /// <summary>
    /// Check if a binary is a valid fuzzy date binary.
    /// *This method does NOT check if a BC date is valid.*
    /// </summary>
    [SqlFunction(IsDeterministic = true, IsPrecise = true)]
    public static SqlBoolean fuzzy_date_is_valid_binary(SqlBinary sql_binary)
    {
      // NULL in NULL out.
      if (sql_binary.IsNull) { return SqlBoolean.Null; }

      // Get the input binary.
      var bytes = sql_binary.Value;

      // Fuzzy date binaries are 3-bytes length.
      if (bytes.Length != 3) { return SqlBoolean.False; }

      // Separate the year, month, and day as it's a regular date.
      var year = (bytes[0] << 4 | bytes[1] >> 4) - 1024;
      var month = (bytes[1] & 0x0F) - 1;
      var day = bytes[2] >> 3;

      // When the binary does not have a year part:
      if (year == -1024)
      {
        // 1. It should have a valid month part;
        // 2. It should have a valid day part or have no day part.
        if (month < 1 || month > 12 || day > DateTime.DaysInMonth(2000, month))
        { return SqlBoolean.False; }
      }

      // When the binary has a year part:
      else
      {
        // Flow control according to month bits:
        switch (month)
        {
          // Valid decades start from:
          //   * -18 (10s BC starts from 19 BC)
          //   * -8 (0s BC starts from 9 BC)
          //   * 1 (0s starts from 1 AD)
          //   * 10 (10s starts from 10 AD)
          //   * ...
          // Spanning maximum of 100 years (1 day bit equals 10 years).
          case 0x00 - 1:
            if ((year > 1 && year % 10 != 0) || (year < 1 && year % 10 != -8) || day > 10)
            { return SqlBoolean.False; }
            break;

          // Day bits should be `0b00000` for a fuzzy date with only a year part.
          case 0x01 - 1:
            if (day != 0) { return SqlBoolean.False; }
            break;

          // Reserved, should not be used in the current spec version.
          case 0x0E - 1:
            return SqlBoolean.False;

          // A binary spanning multiple years with a year part is valid.
          case 0x0F - 1:
            break;

          // Refuse invalid AD date.
          default:
            if (year > 0 && day > DateTime.DaysInMonth(year, month)) { return SqlBoolean.False; }
            break;
        }
      }

      // The binary is valid unless proven invalid.
      return SqlBoolean.True;
    }
  }
}