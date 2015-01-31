should = require 'should'

is_valid_binary = require '../is_valid_binary'
binary_from_string = require '../binary_from_string'
string_from_binary = require '../string_from_binary'

describe 'Fuzzy Date Test Suite', ->

  describe 'Negative Cases', ->

    negative_cases = [
      "d-0"
      "d-13"
      "d-12-32"
      "d-2-0"
      "d-2-30"
      "11sBC"
      "2001-2-29"
      "1025BC"
      "3072"
      "0"
      "0-1"
      "0-1-1"
      "2011s"
      "d-2-30"
      "2014+0"
      "2014+33"
      "2020s+1"
      "2020s+110"
    ]

    for negative_case in negative_cases
      do (negative_case) ->
        it negative_case, ->
          should(binary_from_string(negative_case)).not.be.ok

  describe 'Positive Cases', ->

    positive_cases = [
      ["d-12"       , "December"]
      ["d-12-25"    ,"December 25"]
      ["1024BC"     , "1024 BC"]
      ["212BC"      , "212 BC"]
      ["212BC-8"    , "August 212 BC"],
      ["212BC-8-6"  , "August 6, 212 BC"],
      ["?24BC"      , "? 24 BC"],
      ["?c.20BC"    , "? c. 20 BC"],
      ["10sBC+10"   , "10s BC – 0s BC"],
      ["10sBC+20"   , "10s BC – 0s AD"],
      ["0sBC"       , "0s BC"],
      ["0sBC+10"    , "0s BC – 0s AD"],
      ["c.9BC+2"    , "c. 9 BC – 7 BC"],
      ["c.9BC+20"   , "c. 9 BC – 12 AD"],
      ["1BC"        , "1 BC"],
      ["1BC+1"      , "1 BC – 1 AD"],
      ["0s"         , "0s"],
      ["1"          , "1"],
      ["2010s"      , "2010s"],
      ["2010+10"    , "2010 – 2020"],
      ["fl.?c.2014" , "fl. ? c. 2014"],
      ["?c.2014"    , "? c. 2014"],
      ["c.2014"     , "c. 2014"],
      ["2014"       , "2014"],
      ["2014-8"     , "August 2014"],
      ["2014-8-6"   , "August 6, 2014"],
      ["2014+1"     , "2014 – 2015"],
      ["2020s+20"   , "2020s – 2040s"],
      ["3071"       , "3071"]
    ]

    for positive_case in positive_cases
      do (positive_case) ->
        it positive_case[0], ->
          binary = binary_from_string positive_case[0]
          binary.should.be.ok
          # string_from_binary(binary).should.equal(positive_case[1])
