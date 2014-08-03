Fuzzy Date
==========

Most data types dealing with date are designed to represent a (certain) date
with varying range (usually from 1, 1753, 1900, or 1970) and accuracy (usually
millisecond).

Below examples are common seen in historical and general purpose writings:

  + 2014 (year only)
  + January 2014 (year and month only)
  + 252 BC (before Christ)
  + 1950s (one decade)
  + 1966 - 1976 (spanning 10 years)
  + circa 1791 (around a certain year)
  + ? January 16, 1078 (uncertain date)

None of above can be stored or processed easily using existing data types. We
do not consider `String` as an option since it's:

  1. storage inefficient
  2. hard to sort
  3. hard to localize

**Fuzzy Date** proposes:

  1. a 3 bytes format flexible in modeling date
  2. a set of functions to deal with dates of above format


Binary Format
=============

The binary format aims to be:

  1. storage efficient
  2. sortable

3 bytes are used to store a fuzzy date:

    [Y8] | [Y4][M4] | [D5][A][B][C]

+ 12 Year Bits:

The 1st byte and top 4 bits of the 2nd byte are year bits. Total space of:

    2 ^ 12 = 4096

The mapping between 12 year bits and the actual year is:

  - all `0` (`0b000000000000`) represents a fuzzy date without year, e.g.:

    * January (month only)
    * January 1st (month and day only)

  - substract 1024 for all other combos, we get a year within:

    * 1023 BC (`0b000000000001` - 1024) and
    * 3071 AD (`0b111111111111` - 1024)

+ 4 Month Bits:

  - `0b0000` represents a decade, e.g.:

    * 1790s

  - `0b0001` represents a fuzzy date with only a year part (day bits should be
    `0b00000`), e.g.:

    * 1790

  - `0b1110` is reserved which should not be used in the current spec version.

  - `0b1111` represents a fuzzy date spanning multiple years, e.g.:

    * 1791 - 1793 (day bits denote year span, from 1 to 32)

  - substract 1 for all other combos, we get a month within:

    * 1  (`0b0010` - 1) and
    * 12 (`0b1101` - 1)

+ 5 Day Bits:

When month bits are `0b1111`, day bits denote year span, otherwise:

  - `0b00000` represents a fuzzy date without a day part, e.g.:

    * January 1791

  - `0b00001` to `0b11111` represents day 1 to day 31 accordingly.

+ Flag Bit A: Certainty

  - `1`: the date is certain
  - `0`: the date is uncertain, e.g.:

    * ? 1791
    * ? January 1791

+ Flag Bit B: Accuracy

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

*Uncertain or approximate fuzzy dates go before certain and accurate fuzzy
dates in ascending order. All flag bits are `1` by default*


Helper Functions
================

+ `is_valid`: Check if a 3-bytes binary is a valid fuzzy date or not.

+ `fuzzy_date_of`: Parse below short texts into a 3-bytes binary

  - 1791-2-11 (normal date)
  - 1791-2 (date without day part)
  - 1791 (date with only year part)
  - 1790s (a decade)
  - 1791-1974 (spanning a maximum of 32 years)
  - 212BC (before Christ)
  - d-1-1 (January 1st, date without year part)
  - d-1 (January, date with only month part)
  - ?1791 (uncertain date)
  - c1791 (approximate date)
  - f1791 (Special flag)

+ `short_text_of`: Format a 3-byte binary into a short text. `fuzzy_date_of` and `short_text_of` are not mutually inverse functions.

+ `readable`: Format a 3-byte binary into its readable version, e.g.:

  - ? circa January 212 BC

+ `year_of`: Return the year part of a fuzzy date (`null` when inapplicable).

+ `month_of`: Return the month part of a fuzzy date (`null` when inapplicable).

+ `day_of`: Return the day part of a fuzzy date (`null` when inapplicable).

+ `year_diff`: Return the difference in year between two fuzzy dates (`null` when inapplicable).


Obsolete
========

+ `fuzzy_date_2_markup`: Return markuped date (for navigation use):

+ `fuzzy_date_2_julian_markup`: Return markuped Julian date (for navigation use).
