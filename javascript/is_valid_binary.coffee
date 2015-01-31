# Is Valid Binary

days_in_month = (year, month) ->
  new Date(year, month, 0).getDate()

module.exports = (bytes) ->

  # NULL in NULL out.
  return null unless bytes

  # Fuzzy date binaries are 3-bytes length.
  if bytes.length isnt 3
    return false

  # Separate the year, month, and day as it's a regular date.
  year = (bytes[0] << 4 | bytes[1] >> 4) - 1024
  month = (bytes[1] & 0x0F) - 1
  day = bytes[2] >> 3

  # When the binary does not have a year part:
  if year is -1024

    # 1. It should have a valid month part;
    # 2. It should have a valid day part or have no day part.
    if month < 1 or month > 12 or day > days_in_month(2000, month)
      return false

  # When the binary has a year part:
  else

    # Flow control according to month bits:
    switch month

      # Valid decades start from:
      #   * -18 (10s BC starts from 19 BC)
      #   * -8 (0s BC starts from 9 BC)
      #   * 1 (0s starts from 1 AD)
      #   * 10 (10s starts from 10 AD)
      #   * ...
      # Spanning maximum of 100 years (1 day bit equals 10 years).
      when 0x00 - 1
        if (year > 1 and year % 10 isnt 0) or (year < 1 and year % 10 isnt -8) or day > 10
          return false

      # Day bits should be `0b00000` for a fuzzy date with only a year part.
      when 0x01 - 1
        if day isnt 0
          return false

      # Reserved, should not be used in the current spec version.
      when 0x0E - 1
        return false

      # A binary spanning multiple years with a year part is valid.
      when 0x0F - 1

      # Refuse invalid AD date.
      else
        if year > 0 and day > days_in_month(year, month)
          return false

  # The binary is valid unless proven invalid.
  true
