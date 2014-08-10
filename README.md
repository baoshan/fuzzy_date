TL;DR
=====

A **3-byte** data type for historical and general purpose dates handling:

| Fuzzy Date         | Short String |
|--------------------|--------------|
| December           | `d-12`       |
| December 25        | `d-12-25`    |
| 1024 BC            | `1024BC`     |
| 212 BC             | `212BC`      |
| August 212 BC      | `212BC-8`    |
| August 6, 212 BC   | `212BC-8-6`  |
| ? 24 BC            | `?24BC`      |
| ? c. 20 BC         | `?c.20BC`    |
| 10s BC - 0s BC     | `10sBC+10`   |
| 10s BC - 0s AD     | `10sBC+20`   |
| 0s BC              | `0sBC`       |
| 0s BC - 0s AD      | `0sBC+10`    |
| c. 9 BC - 7 BC     | `c.9BC+2`    |
| c. 9 BC - 12 AD    | `c.9BC+20`   |
| 1 BC               | `1BC`        |
| 1 BC - 1 AD        | `1BC+1`      |
| 0s                 | `0s`         |
| 1                  | `1`          |
| 2010s              | `2010s`      |
| 2010s - 2020s      | `2010s+10`   |
| 2014               | `2014`       |
| January 2014       | `2014-1`     |
| January 1, 2014    | `2014-1-1`   |
| 2014 - 2015        | `2014+1`     |
| 3071               | `3071`       |

and more.

The Long Version
================

Most data types dealing with date are designed to represent a certain date with
varying ranges (usually starts from 1, 1753, 1900, or 1970) and accuracies
(usually from 100 nanoseconds to 1 day).

Below examples are common seen in historical and general purpose writings:

  + 2014 (year only)
  + January 2014 (year and month only)
  + 252 BC (before Christ)
  + 1950s (one decade)
  + 1966 - 1976 (spanning 10 years)
  + circa 1791 (around a certain year)
  + ? January 16, 1078 (uncertain date)

None of above can be stored or processed using existing data types. We do not
consider `String` as an option since it's:

  1. storage inefficient
  2. hard to sort
  3. hard to localize

**Fuzzy Date** proposes:

  1. a 3-byte data type flexible in modeling date
  2. a set of functions dealing with above format


Binary Format
=============

The binary format aims to be:

  1. storage efficient
  2. sortable

3 bytes are used to store a fuzzy date:

    [8Y]  [4Y][4M]  [5D][A][B][C]

+ 12 Year Bits:

The 1<sup>st</sup> byte and top 4 bits of the 2<sup>nd</sup> byte are year bits. Total space of:

    2 ^ 12 = 4096

The mapping between 12 year bits and the actual year is:

  - `0b000000000000` represents a fuzzy date without a year part, e.g.:

    * February (month only)
    * February 1 (month and day only)
    * February 29 (all possible days of a month)

  - substract 1024 for all other combos, we get a year within:

    * 1024 BC (`0b000000000001` - 1024 = -1023) and
    * 3071 AD (`0b111111111111` - 1024 =  3071)

+ 4 Month Bits:

  - `0b0000` represents a decade (day bits should be `0b00000`) or a fuzzy date
    spanning multi-decades, e.g.:

    * 1860s
    * 1860s - 1870s

    For BC decades, the year is the start year of the decades, e.g.:

    * The year bits for 10s BC are same as 19 BC (-18).

  - `0b0001` represents a fuzzy date with only a year part (day bits should be
    `0b00000`), e.g.:

    * 1790

  - `0b1110` is reserved which should not be used in the current spec version.

  - `0b1111` represents a fuzzy date spanning multiple years, e.g.:

    * 1791+2 (day bits denote year span, from 1 to 32)

  - substract 1 for all other combos, we get a month within:

    * 1  (`0b0010` - 1) and
    * 12 (`0b1101` - 1)

+ 5 Day Bits:

When month bits are `0b1111`, day bits denote year span, otherwise:

  - `0b00000` represents a fuzzy date without a day part, e.g.:

    * January 1791

  - `0b00001` to `0b11111` represents day 1 to day 31 accordingly.

+ Flag Bit A: Certainty.

  - `1`: the date is certain
  - `0`: the date is uncertain, e.g.:

    * ? 1791
    * ? January 1791

+ Flag Bit B: Accuracy.

  - `1`: the date is accurate
  - `0`: the date is approximate, e.g.:

    * circa 1791
    * circa January 1791

+ Flag Bit C: Special flag. ICMD use flag bit C to denote `floruit`:

  - `1`: the fuzzy date is not a floruit date
  - `0`: the fuzzy date is not a floruit date, e.g.:

    * fl. 1234

An application can define its own usage for flag bit C. Keep it `1` when you
don't need it.

*Uncertain/approximate dates go before certain/accurate dates in
ascending order (default for print). Thus, all flag bits are `1` by default.*


Helper Functions
================

+ `is_valid_binary_fuzzy_date`: Check if a binary is a valid fuzzy date or not.

+ `is_valid_string_fuzzy_date`: Check if a string is a valid fuzzy date or not.

+ `binary_fuzzy_date_from_string`: Parse below short texts into a 3-bytes binary:

  - 1791-2-11 (normal date)
  - 1791-2 (date without day part)
  - 1791 (date with only year part)
  - 1790s (a decade)
  - 1791+3 (spanning a maximum of 32 years)
  - 212BC (before Christ)
  - d-1-1 (January 1, date without year part)
  - d-1 (January, date with only month part)
  - ?1791 (uncertain date)
  - c1791 (approximate date)
  - f1791 (Special flag)

+ `fuzzy_date_string_from_binary`: Format a 3-byte binary into its short text representation.
  `fuzzy_date_from_short_text` and `short_text_from_fuzzy_date` are mutually inverse functions.

+ `readable`: Format a 3-byte binary into its readable (longer) version, e.g.:

  - ? circa January 212 BC

+ `year_of`   : Return the year  part of a fuzzy date, `null` when inapplicable.
+ `month_of`  : Return the month part of a fuzzy date, `null` when inapplicable.
+ `day_of`    : Return the day   part of a fuzzy date, `null` when inapplicable.
+ `year_diff` : Return the difference in years between two fuzzy dates, `null`
  when inapplicable.


Obsolete
========

+ `fuzzy_date_2_markup`: Return markuped date (for navigation use):
+ `fuzzy_date_2_julian_markup`: Return markuped Julian date (for navigation use).


Data Polish
===========

1. 1 BC before 1 AD, no 0 AD.
2. Year span 1 to 32, not 0 to 31.
