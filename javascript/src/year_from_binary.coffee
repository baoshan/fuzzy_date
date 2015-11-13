module.exports = (bytes) ->

  # NULL in NULL out.
  return null unless bytes

  if not bytes.length
    bytes = [~~(bytes / 256 / 256), ~~(bytes / 256 % 256), bytes % 256]

  # The string to be populated.
  # string result

  # Separate the year, month, and day as it's a regular date.
  year = (bytes[0] << 4 | bytes[1] >> 4) - 1024
  month = (bytes[1] & 0x0F) - 1
  day = bytes[2] >> 3

  # When the binary does not have a year part:
  if year is -1024
    return null

  # When the binary has a year part:
  else

    # 1 BC before 1 AD.
    before_christ = year < 1

    # Flow control according to month bits:
    switch month

      # A Decade.
      when 0x00 - 1

        # Valid BC decades start from:
        #   * -18 (10s BC starts from 19 BC)
        #   * -8 (0s BC starts from 9 BC)
        #   * ...
        if before_christ then result = -8 - year + "sBC"

        # Valid AD decades start from:
        #   * 1 (0s BC starts from 1 AD)
        #   * 10 (10 BC starts from 10 AD)
        #   * ...
        else return (if year is 1 then 0 else year) + "s"

      # A fuzzy date with only year part.
      when 0x01 - 1
        result = if before_christ then 1 - year + "BC" else "#{year}"

      # A fuzzy date spanning multi-years.
      when 0x0F - 1
        result = (if before_christ then 1 - year + "BC" else "#{year}")

      # A fuzzy date with a month part.
      else
        result = (if before_christ then 1 - year + "BC" else "#{year}")

  # Return the populated string.
  result
