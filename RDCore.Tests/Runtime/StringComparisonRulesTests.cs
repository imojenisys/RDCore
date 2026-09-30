using RDCore.SDK.Model.Symbols;
using RDCore.SDK.Model.Values.Intrinsic;
using RDCore.SDK.Runtime;

namespace RDCore.Tests.Runtime;

/// <summary>
/// <see cref="StringComparisonRules"/> is where <c>String</c> values are ordered (<strong>MS-VBAL §5.6.9.5</strong>): binary mode
/// compares their data, a trailing byte included, in the one order binary mode takes; text mode compares their text regardless of
/// case, and a trailing byte only breaks a tie.
/// </summary>
/// <remarks>
/// Binary mode has two readings, behind the one switch <see cref="StringComparisonRules.BinaryModeComparesByteImages"/>: code units
/// then a trailing byte, and the byte image. Both are tested here as they are; the tests of the order binary mode takes pin whichever
/// the switch selects, and are the ones to change when it changes.
/// </remarks>
[TestClass]
[TestCategory("MS-VBAL 5.6.9.5 Relational Operators (runtime semantics)")]
public sealed class StringComparisonRulesTests
{
    private static readonly StringComparisonRules Binary = new(OptionCompare.Binary);
    private static readonly StringComparisonRules Text = new(OptionCompare.Text);

    private static VBStringValue Bytes(params byte[] data) => VBStringValue.FromBytes(data);

    // strings of whole characters and strings with a trailing byte, among them the pairs the two binary readings disagree about.
    private static VBStringValue[] Sample() =>
    [
        new(string.Empty),
        Bytes(0x41),
        new("A"),
        Bytes(0x41, 0x00, 0x42),
        Bytes(0x41, 0x00, 0x50),
        new("AB"),
        new("a"),
        Bytes(0x61, 0x00, 0x42),
        new("Ł"),
        new("B"),
        new("\uD800"),
        Bytes(0x00, 0xD8, 0x42),
    ];

    private static void AssertTotalOrder(Func<VBStringValue, VBStringValue, int> compare)
    {
        var sample = Sample();
        foreach (var a in sample)
        {
            Assert.AreEqual(0, compare(a, a), $"{Show(a)} is not equal to itself");
            foreach (var b in sample)
            {
                Assert.AreEqual(Math.Sign(compare(a, b)), -Math.Sign(compare(b, a)), $"{Show(a)} and {Show(b)} are not antisymmetric");
                foreach (var c in sample)
                {
                    if (compare(a, b) <= 0 && compare(b, c) <= 0)
                    {
                        Assert.IsLessThanOrEqualTo(0, compare(a, c), $"{Show(a)} <= {Show(b)} <= {Show(c)}, but not {Show(a)} <= {Show(c)}");
                    }
                }
            }
        }
    }

    private static string Show(VBStringValue value) => Convert.ToHexString(value.GetBytes());

    #region the order binary mode takes

    [TestMethod]
    public void Compare_InBinaryMode_IsATotalOrder()
        => AssertTotalOrder(Binary.Compare);

    [TestMethod]
    public void Compare_InBinaryMode_OrdersCodeUnits_ThenATrailingByte()
    {
        // 🎯 these two flip with the switch: under the byte image "Ł" (41 01) is less than "B" (42 00), and "A" followed by the byte
        // 50 is greater than "AB" (41 00 50 against 41 00 42 00).
        Assert.IsGreaterThan(0, Binary.Compare(new VBStringValue("Ł"), new VBStringValue("B")), "U+0141 is after U+0042");
        Assert.IsLessThan(0, Binary.Compare(Bytes(0x41, 0x00, 0x50), new VBStringValue("AB")), "the characters \"A\" are before \"AB\"");
    }

    [TestMethod]
    public void Compare_InBinaryMode_AStringWithATrailingByte_IsAfterTheSameCharactersWithout()
        => Assert.IsGreaterThan(0, Binary.Compare(Bytes(0x41, 0x00, 0x42), new VBStringValue("A")));

    [TestMethod]
    public void Compare_InBinaryMode_TrailingBytes_AreOrderedByValue()
        => Assert.IsLessThan(0, Binary.Compare(Bytes(0x41, 0x00, 0x42), Bytes(0x41, 0x00, 0x43)));

    [TestMethod]
    public void Compare_InBinaryMode_AnOddOnlyString_IsNotTheZeroLengthString()
        // ChrB(65) = "" is False: one byte is data, where a zero-length string has none.
        => Assert.IsGreaterThan(0, Binary.Compare(Bytes(0x41), VBStringValue.ZeroLengthString));

    [TestMethod]
    public void Compare_InBinaryMode_TheNullString_IsTheZeroLengthString()
        // vbNullString = "" is True in MS-VBA: neither has any data.
        => Assert.AreEqual(0, Binary.Compare(VBStringValue.VBNullString, VBStringValue.ZeroLengthString));

    [TestMethod]
    [DataRow("Ł", "B")]
    [DataRow("B", "a")]
    [DataRow("abc", "abd")]
    [DataRow("", "x")]
    [DataRow("\uD800", "�")]
    [DataRow("same", "same")]
    public void Comparer_InBinaryMode_AgreesWithCompare(string lhs, string rhs)
        // the StringComparer cannot see a trailing byte, but wherever there is none it is the same order: one binary order, not two.
        => Assert.AreEqual(Math.Sign(Binary.Compare(new VBStringValue(lhs), new VBStringValue(rhs))), Math.Sign(Binary.Comparer.Compare(lhs, rhs)));

    [TestMethod]
    public void Comparer_InBinaryMode_ReadsNullAsTheZeroLengthString()
    {
        var sut = Binary.Comparer;

        Assert.AreEqual(0, sut.Compare(null, string.Empty));
        Assert.AreEqual(sut.GetHashCode(string.Empty), sut.GetHashCode(null!), "the hash agrees with the order");
        Assert.IsTrue(sut.Equals(null, string.Empty), "and so does equality");
    }

    #endregion

    #region the two readings of binary mode

    [TestMethod]
    public void CompareCodeUnits_OrdersByCodeUnit_ThenByATrailingByte()
    {
        static int Compare(VBStringValue lhs, VBStringValue rhs) => StringComparisonRules.CompareCodeUnits(lhs.GetBytes(), rhs.GetBytes());

        Assert.IsGreaterThan(0, Compare(new VBStringValue("Ł"), new VBStringValue("B")), "U+0141 is after U+0042");
        Assert.IsLessThan(0, Compare(Bytes(0x41, 0x00, 0x50), new VBStringValue("AB")), "a string whose characters begin another's is before it");
        Assert.IsGreaterThan(0, Compare(Bytes(0x41, 0x00, 0x42), new VBStringValue("A")), "no trailing byte is before any");
        Assert.IsLessThan(0, Compare(Bytes(0x41, 0x00, 0x42), Bytes(0x41, 0x00, 0x43)), "trailing bytes are ordered by value");
        Assert.AreEqual(0, Compare(Bytes(0x41, 0x00, 0x42), Bytes(0x41, 0x00, 0x42)));
    }

    [TestMethod]
    public void CompareCodeUnits_IsATotalOrder()
        => AssertTotalOrder((lhs, rhs) => StringComparisonRules.CompareCodeUnits(lhs.GetBytes(), rhs.GetBytes()));

    [TestMethod]
    public void CompareByteImages_OrdersByByte_TheLongerGreater()
    {
        // "each byte [...] is compared [...] If all bytes in one string are equal to their respective bytes in the other string, but the
        // other string is longer, the longer string is considered greater."
        static int Compare(VBStringValue lhs, VBStringValue rhs) => StringComparisonRules.CompareByteImages(lhs.GetBytes(), rhs.GetBytes());

        Assert.IsLessThan(0, Compare(new VBStringValue("Ł"), new VBStringValue("B")), "41 01 is before 42 00");
        Assert.IsGreaterThan(0, Compare(Bytes(0x41, 0x00, 0x50), new VBStringValue("AB")), "41 00 50 is after 41 00 42 00");
        Assert.IsGreaterThan(0, Compare(Bytes(0x41, 0x00, 0x42), new VBStringValue("A")), "the longer string is greater");
        Assert.IsLessThan(0, Compare(Bytes(0x41, 0x00, 0x42), Bytes(0x41, 0x00, 0x43)));
        Assert.AreEqual(0, Compare(Bytes(0x41, 0x00, 0x42), Bytes(0x41, 0x00, 0x42)));
    }

    [TestMethod]
    public void CompareByteImages_IsATotalOrder()
        => AssertTotalOrder((lhs, rhs) => StringComparisonRules.CompareByteImages(lhs.GetBytes(), rhs.GetBytes()));

    [TestMethod]
    [DataRow("A", "B")]
    [DataRow("B", "a")]
    [DataRow("A", "AB")]
    [DataRow("ÿ", "þÿ")]
    [DataRow("same", "same")]
    public void TheTwoReadings_AgreeAboutWholeCharactersBelowU0100(string lhs, string rhs)
    {
        byte[] Data(string text) => new VBStringValue(text).GetBytes();

        Assert.AreEqual(
            Math.Sign(StringComparisonRules.CompareCodeUnits(Data(lhs), Data(rhs))),
            Math.Sign(StringComparisonRules.CompareByteImages(Data(lhs), Data(rhs))));
    }

    #endregion

    #region text mode

    [TestMethod]
    public void Compare_InTextMode_IsATotalOrder()
        => AssertTotalOrder(Text.Compare);

    [TestMethod]
    public void Compare_InTextMode_ATrailingByteBreaksATie()
    {
        // "a", "A" + 41 and "a" + 42 are all the text "a" regardless of case: no trailing byte, then 41, then 42.
        Assert.IsLessThan(0, Text.Compare(new VBStringValue("a"), Bytes(0x41, 0x00, 0x41)));
        Assert.IsLessThan(0, Text.Compare(Bytes(0x41, 0x00, 0x41), Bytes(0x61, 0x00, 0x42)));
    }

    [TestMethod]
    public void Compare_InTextMode_TheTextComesFirst()
        // a trailing byte only breaks a tie: "A" is before "b" whatever follows it.
        => Assert.IsLessThan(0, Text.Compare(Bytes(0x41, 0x00, 0xFF), new VBStringValue("b")));

    [TestMethod]
    public void Compare_InTextMode_TheSameTrailingByte_LeavesTheTextsEqual()
        => Assert.AreEqual(0, Text.Compare(Bytes(0x61, 0x00, 0x42), Bytes(0x41, 0x00, 0x42)));

    #endregion
}
