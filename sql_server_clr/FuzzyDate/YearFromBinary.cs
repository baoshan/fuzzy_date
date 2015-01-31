using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Server;

public partial class FuzzyDate
{
  [SqlFunction(IsDeterministic = true, IsPrecise = true)]
  public static SqlInt16 YearFromBinary(SqlBinary sql_binary)
  {
    // NULL in NULL out.
    if (sql_binary.IsNull) { return SqlInt16.Null; }

    // Get the input binary.
    var bytes = sql_binary.Value;

    // The string to be populated.
    string result;

    // Separate the year, month, and day as it's a regular date.
    var year = (bytes[0] << 4 | bytes[1] >> 4) - 1024;

    // When the binary does not have a year part:
    if (year == -1024)
    {
      return SqlInt16.Null;
    }

    return new SqlInt16((Int16)year);
  }
}