// Generated by CoffeeScript 1.10.0
(function() {
  var is_valid_binary, string_from_binary;

  is_valid_binary = require('./is_valid_binary');

  string_from_binary = require('./string_from_binary');

  module.exports = function(input) {
    var bytes, error, matches, year;
    if (!(input != null ? input.trim() : void 0)) {
      return null;
    }
    try {
      input = input.replace(/\ /g, '').replace(/\d+/g, function(m) {
        return +m;
      }).toLowerCase();
      bytes = new Uint8Array(3);
      matches = input.match(/\d+/g);
      if (input.match(/d/)) {
        bytes[1] = +matches[0] + 1;
        if (matches.length > 1) {
          bytes[2] = +matches[1] << 3;
        }
      } else {
        year = +matches[0];
        if (input.match(/bc/)) {
          if (input.match(/s/)) {
            year = -8 - year;
          } else {
            year = 1 - year;
          }
        } else if (year === 0) {
          year = 1;
        }
        year += 1024;
        bytes[0] = year >> 4;
        bytes[1] = year << 4;
        if (input.match(/\+/)) {
          if (input.match(/s/)) {
            bytes[2] = +matches[1] / 10 << 3;
          } else {
            bytes[1] |= 0x0F;
            bytes[2] = matches[1] - 1 << 3;
          }
        } else if (!input.match(/s/)) {
          bytes[1] |= matches.length > 1 ? +matches[1] + 1 : 1;
          bytes[2] = matches.length > 2 ? +matches[2] << 3 : 0;
        }
      }
      if (!input.match(/\?/)) {
        bytes[2] |= 0x04;
      }
      if (!input.match(/c\./)) {
        bytes[2] |= 0x02;
      }
      if (!input.match(/f/)) {
        bytes[2] |= 0x01;
      }
      if (!is_valid_binary(bytes)) {
        return null;
      }
      if (input.replace('.', '').replace(/\d+/g, function(m) {
        return +m;
      }) !== string_from_binary(bytes).replace(' ', '').replace('.', '').replace(/\d+/g, function(m) {
        return +m;
      }).toLowerCase()) {
        return null;
      }
      return bytes;
    } catch (error) {
      return null;
    }
  };

}).call(this);
