is_valid_binary = require './is_valid_binary'
string_from_binary = require './string_from_binary'

# Parse a short string into a fuzzy date binary.
module.exports = (input) ->

  # NULL in NULL out.
  return null unless input?.trim()

  # Any exception due to invalid input leads to SqlBytes.Null be returned.
  try

    # Get the input string (w/o whitespaces, in lowercase).
    input = input.replace(/\ /g, '').toLowerCase()

    # Initialize the output bytes.
    bytes = new Uint8Array 3

    # Search the string for all continuous digits.
    matches = input.match /\d+/g

    # When the string does not have a year part:
    if input.match /d/

      # 1. Calculate the month bits.
      # 2. Calculate the day bits if the string contains a day part.
      bytes[1] = +matches[0] + 1
      if matches.length > 1
        bytes[2] = +matches[1] << 3

    # When the string has a year part:
    else

      # Parse the year part.
      year = +matches[0]

      if input.match /bc/

        # Valid BC decades start from:
        #   * -18 (10s BC starts from 19 BC)
        #   * -8 (0s BC starts from 9 BC)
        #   * ...
        if input.match /s/
          year = -8 - year

        # 1 BC before 1 AD.
        else
          year = 1 - year

      # 0s starts from 1 AD.
      else if year is 0
        year = 1

      # Calculate year bits.
      year += 1024
      bytes[0] = year >> 4
      bytes[1] = year << 4

      if input.match /\+/

        # For fuzzy decades spanning multiple years, 1 day bit equals 10 years.
        if input.match /s/
          bytes[2] = +matches[1] / 10 << 3

        # For fuzzy dates spanning multiple years:
        # 1. Month bits are `0b1111`;
        # 2. Calculate day bits (spanning years).
        else
          bytes[1] |= 0x0F
          bytes[2] = matches[1] - 1 << 3

      # For other fuzzy dates that aren't decades:
      # 1. Calculate month bits;
      # 2. Calculate day bits.
      else if not input.match /s/
        bytes[1] |= if matches.length > 1 then +matches[1] + 1 else 1
        bytes[2] = if matches.length > 2 then +matches[2] << 3 else 0

    # Flag Bit A: Certainty.
    if not input.match /\?/
      bytes[2] |= 0x04

    # Flag Bit B: Accuracy.
    if not input.match /c\./
      bytes[2] |= 0x02

    # Flag Bit C: Special flag.
    if not input.match /f/
      bytes[2] |= 0x01

    # Ensure the output is a valid fuzzy date binary.
    if not is_valid_binary bytes
      return null

    # Ensure the output is the same as input.
    if input.replace('.', '') isnt string_from_binary(bytes).replace(' ', '').replace('.', '').toLowerCase()
      return null

    # Return the parsed binary.
    bytes

  # The parser returns SqlBytes.Null for all invalid inputs.
  catch
    null
