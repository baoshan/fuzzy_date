
module.exports = (input) ->
  return null unless input
  input[0] * 256 * 256 + input[1] * 256 + input[2]
