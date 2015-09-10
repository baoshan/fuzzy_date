binary_from_string = require './binary_from_string'

module.exports = (input) ->
  bytes = binary_from_string input
  return null unless bytes
  bytes[0] * 256 * 256 + bytes[1] * 256 + bytes[2]
