using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.SqlTypes;

[TestClass]
public partial class FuzzyDateTest
{
  /// <summary>
  /// Negative examples which must fail.
  /// </summary>
  [TestMethod]
  public void InvalidFuzzyDateStrings()
  {
    string[] negative_examples = {
        "d-0",
        "d-13",
        "d-12-32",
        "d-2-0",
        "d-2-30",
        "11sBC",
        "2001-2-29",
        "1025BC",
        "3072",
        "0",
        "0-1",
        "0-1-1",
        "2011s",
        "d-2-30",
        "2014+0",
        "2014+33",
        "2020s+1",
        "2020s+110"
      };
    foreach (var negative_example in negative_examples)
    {
      Assert.IsTrue(FuzzyDate.BinaryFromString(negative_example).IsNull);
    }
  }

  /// <summary>
  /// Positive examples which should pass. Examples are given in ascending order.
  /// </summary>
  [TestMethod]
  public void ValidFuzzyDateStrings()
  {
    string[,] positive_examples = { 
        {"d-12", "December"},
        {"d-12-25","December 25"},
        {"1024BC", "1024 BC"},
        {"212BC", "212 BC"},
        {"212BC-8", "August 212 BC"},
        {"212BC-8-6", "August 6, 212 BC"},
        {"?24BC", "? 24 BC"},
        {"?c.20BC", "? c. 20 BC"},
        {"10sBC+10", "10s BC – 0s BC"},
        {"10sBC+20", "10s BC – 0s AD"},
        {"0sBC", "0s BC"},
        {"0sBC+10", "0s BC – 0s AD"},
        {"c.9BC+2", "c. 9 BC – 7 BC"},
        {"c.9BC+20", "c. 9 BC – 12 AD"},
        {"1BC", "1 BC"},
        {"1BC+1", "1 BC – 1 AD"},
        {"0s", "0s"},
        {"1", "1"},
        {"2010s", "2010s"},
        {"2010+10", "2010 – 2020"},
        {"fl.?c.2014", "fl. ? c. 2014"},
        {"?c.2014", "? c. 2014"},
        {"c.2014", "c. 2014"},
        {"2014", "2014"},
        {"2014-8", "August 2014"},
        {"2014-8-6", "August 6, 2014"},
        {"2014+1", "2014 – 2015"},
        {"2020s+20", "2020s – 2040s"},
        {"3071", "3071"}
      };
    var prev_valid_binary = SqlBinary.Null;
    for (var i = 0; i < positive_examples.GetLength(0); i++)
    {
      var valid_binary = FuzzyDate.BinaryFromString(positive_examples[i, 0]);
      Assert.IsFalse(valid_binary.IsNull);
      Assert.AreEqual(positive_examples[i, 0], FuzzyDate.StringFromBinary(valid_binary));
      Assert.AreEqual(positive_examples[i, 1], FuzzyDate.ReadableStringFromBinary(valid_binary));
      if (!prev_valid_binary.IsNull) { Assert.IsTrue((valid_binary > prev_valid_binary).Value); }
      prev_valid_binary = valid_binary;
    }
  }
}