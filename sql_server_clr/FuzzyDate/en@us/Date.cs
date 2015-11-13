using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.SqlServer.Server;

public partial class FuzzyDate
{
  [SqlFunction(IsDeterministic = true, IsPrecise = true)]
  public static SqlString Date2Markup(SqlBytes input)
  {
    if (input.IsNull)
    {
      return SqlString.Null;
    }
    byte[] inputBytes = input.Value;
    string result;
    int year = (inputBytes[0] << 4 | inputBytes[1] >> 4);
    if (year == 0)
    {
      int month = (inputBytes[1] & 0x0F) - 1;
      int day = inputBytes[2] >> 3;
      if (day == 0)
      {
        result = "[Month[" + date_time_format_info.GetMonthName(month) + "|" + date_time_format_info.GetMonthName(month) + "]]";
      }
      else
      {
        result = "[Day[" + date_time_format_info.GetMonthName(month) + "-" + day + "|" + date_time_format_info.GetMonthName(month) + " " + day + "]]";
      }
    }
    else
    {
      year -= 1024;
      bool bc = year < 0;
      switch (inputBytes[1] & 0x0F)
      {
        case 0x00:
          result = "[Decade[" + Math.Abs(year).ToString() + "s" + (bc ? "-BC" : "") + "|" + Math.Abs(year).ToString() + "s" + (bc ? " BC" : "") + "]]";
          int span = (inputBytes[2] >> 3);
          if (span > 0)
          {
            year += span;
            bc = year < 0;
            result += " - [Decade[" + Math.Abs(year).ToString() + "s" + (bc ? "-BC" : "") + "|" + Math.Abs(year).ToString() + "s" + (bc ? " BC" : "") + "]]";
          }
          break;
        case 0x01:
          result = "[Year[" + Math.Abs(year).ToString() + (bc ? "-BC" : "") + "|" + Math.Abs(year).ToString() + (bc ? " BC" : "") + "]]";
          break;
        case 0x0F:
          result = "[Year[" + Math.Abs(year).ToString() + (bc ? "-BC" : "") + "|" + Math.Abs(year).ToString() + (bc ? " BC" : "") + "]]";
          span = inputBytes[2] >> 3;
          if (span > 0)
          {
            year += span + 1;
            bc = year < 0;
            result += " - [Year[" + Math.Abs(year).ToString() + (bc ? "-BC" : "") + "|" + Math.Abs(year).ToString() + (bc ? " BC" : "") + "]]";
          }
          break;
        default:
          int month = (inputBytes[1] & 0x0F) - 1;
          int day = inputBytes[2] >> 3;
          if (day == 0)
          {
            result = "[Month[" + date_time_format_info.GetMonthName(month) + "|" + date_time_format_info.GetMonthName(month) + "]] [Year[" + Math.Abs(year).ToString() + (bc ? "-BC" : "") + "|" + Math.Abs(year).ToString() + (bc ? " BC" : "") + "]]";
          }
          else
          {
            result = "[Day[" + date_time_format_info.GetMonthName(month) + "-" + day + "|" + date_time_format_info.GetMonthName(month) + " " + day + "]], [Year[" + Math.Abs(year).ToString() + (bc ? "-BC" : "") + "|" + Math.Abs(year).ToString() + (bc ? " BC" : "") + "]]";
          }
          break;
      }
      if ((inputBytes[2] & 0x02) == 0)
      {
        result = "circa " + result;
      }
      if ((inputBytes[2] & 0x04) == 0)
      {
        result = "? " + result;
      }
    }
    return new SqlString(result);
  }

  [SqlFunction(IsDeterministic = true, IsPrecise = true)]
  public static SqlString Date2JulianMarkup(SqlBytes input)
  {
    if (input.IsNull)
    {
      return SqlString.Null;
    }
    byte[] inputBytes = input.Value;
    int year = (inputBytes[0] << 4 | inputBytes[1] >> 4) - 1024;
    int month = (inputBytes[1] & 0x0F) - 1;
    int day = inputBytes[2] >> 3;
    string result;
    DateTime date = new DateTime(year, month, day);
    JulianCalendar julianCalendar = new JulianCalendar();
    int jYear = julianCalendar.GetYear(date);
    int jMonthOffset = julianCalendar.GetMonth(date) - 1;
    int jDay = julianCalendar.GetDayOfMonth(date);
    if (jYear == year)
    {
      result = "[Day[" + date_time_format_info.GetMonthName(month) + "-" + day + "|" + date_time_format_info.GetMonthName(month) + " " + day + "]] (O.S. " + date_time_format_info.GetMonthName(month) + " " + jDay + "), " + "[Year[" + year + "]]";
    }
    else
    {
      result = "[Day[" + date_time_format_info.GetMonthName(month) + "-" + day + "|" + date_time_format_info.GetMonthName(month) + " " + day + "]], [Year[" + year + "|" + year + "]], (O.S. " + date_time_format_info.GetMonthName(month) + " " + jDay + ", " + jYear + ")";
    }
    if ((inputBytes[2] & 0x02) == 0)
    {
      result = "circa " + result;
    }
    if ((inputBytes[2] & 0x04) == 0)
    {
      result = "? " + result;
    }
    return new SqlString(result);
  }
};