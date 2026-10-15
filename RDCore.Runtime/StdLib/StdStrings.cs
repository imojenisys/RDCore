using RDCore.Runtime.Semantics.Conversion;
using RDCore.SDK.Model.Errors;
using RDCore.SDK.Model.Errors.Abstract;
using RDCore.SDK.Model.Symbols;
using RDCore.SDK.Model.Types;
using RDCore.SDK.Model.Types.Abstract;
using RDCore.SDK.Model.Values.Abstract;
using RDCore.SDK.Model.Values.Intrinsic;
using RDCore.SDK.Runtime;
using RDCore.SDK.Runtime.Abstract.Execution;
using RDCore.SDK.Runtime.Abstract.StdLib;
using RDCore.SDK.Runtime.Shared;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace RDCore.Runtime.StdLib;

/// <inheritdoc cref="IStdStringsModule"/>
/// <remarks>
/// <para>
/// 👉 The session is what these members read of the world: the regional settings of its environment, which a number or a
/// date is written in when a <c>Variant</c> argument is read as a <c>String</c> (as Let-coercion writes it,
/// <strong>MS-VBAL §5.5.1.2.4</strong>) and which <c>LCase</c> and <c>UCase</c> case by; the environment's ANSI code page,
/// which is what the implementation-defined character codes of <c>Asc</c>, <c>Chr</c> and <c>String</c> are; and the
/// comparison an explicit <c>Compare</c> names.
/// </para>
/// <para>
/// 👉 What a member is given has been through the call site first. A typed parameter's argument was Let-coerced to its type,
/// so a Null <c>String</c> or <c>Long</c> was error 94 before any member was reached; a typed optional left out arrives as its
/// declared default (<c>StdLibDefault</c>), and a <c>Compare</c> left out as the calling module's <c>Option Compare</c>
/// (<c>StdLibOptionCompare</c>). An omitted <c>Variant</c> optional, or a caller's Missing passed on to one, arrives as
/// <c>null</c>, and nothing else is read as omitted: an explicit Empty is Empty. A member reads a <c>Variant</c> argument the
/// way a parameter of the type it needs would be Let-coerced, and always in one order: the arguments' coercions in the order
/// they are declared, then a Null operand, which makes the result Null, then the values' own checks in the order they are
/// declared. A character member reads a <c>String</c>'s characters, so a trailing byte is not one of them, as <see cref="Len"/>
/// does not count it; a byte member (<c>AscB</c>, <c>ChrB</c>, <c>LeftB</c>, <c>RightB</c>, <c>MidB</c>) reads every byte.
/// Each <c>$</c> function is its <c>Variant</c> twin's result Let-coerced to <c>String</c>, so where the twin returns Null it
/// raises error 94.
/// </para>
/// <para>
/// 👉 <c>Nothing</c> given for a <c>Variant</c> parameter is error 91, as its Let-coercion to a value is
/// (<strong>MS-VBAL §5.5.1.2.13</strong>), and as the call site makes it for a typed parameter.
/// </para>
/// <para>
/// 🚧 An object that is not <c>Nothing</c>, given for a <c>Variant</c> parameter, is a type mismatch: §5.5.1.2.13 would evaluate
/// its default member, which nothing here can yet. TODO.
/// </para>
/// <para>
/// 🚧 A member whose body returns <see cref="NotImplemented{TValue}"/> is declared and not written yet: the symbol resolves,
/// the call is well-formed, and the platform has not got the code yet.
/// </para>
/// </remarks>
/// <param name="session">The session whose environment (regional settings, ANSI code page) and comparison rules these members read.</param>
public sealed class StdStrings(IRuntimeSession session) : IStdStringsModule
{
    // "spaces" are U+0020, and nothing else: a tab, a no-break space or an ideographic space is not one.
    private const char SpaceCharacter = ' ';

    // the longest String .NET can hold; a longer one is out of string space.
    private const int LongestString = 0x3FFFFFDF;

    // 👉 MS-VBAL 6.1.2.11.1.6 bounds ChrW at "-32,767", which is read as the bottom of an Integer: AscW of U+8000 is -32768,
    // and ChrW(AscW(c)) is c for every code unit.
    private const int LowestWideCode = -32768;

    /// <summary>
    /// The error every member that is declared but not written yet returns.
    /// </summary>
    /// <remarks>
    /// TODO the members not written yet; the <c>Format</c> family, <c>MonthName</c> and <c>WeekdayName</c> wait on the regional
    /// and calendar rules of the <c>Format</c> functions.
    /// </remarks>
    private static RuntimeSemanticsEvaluationResult<TValue> NotImplemented<TValue>(string member)
        where TValue : VBTypedValue
        => RuntimeSemanticsEvaluationResult<TValue>.Error(VBRuntimeErrorInfo.For(
            VBRuntimeErrorId.ApplicationDefinedOrObjectDefinedError, default,
            $"'{member}' is declared but not implemented yet."));

    // a member's errors say which member, and what about its arguments: "Mid: Start is at least 1, not 0."
    private static VBRuntimeErrorInfo Failure(VBRuntimeErrorId error, string member, string rule)
        => VBRuntimeErrorInfo.For(error, default, $"{member}: {rule}.");

    private static RuntimeSemanticsEvaluationResult<TValue> Fail<TValue>(VBRuntimeErrorId error, string member, string rule)
        where TValue : VBTypedValue
        => RuntimeSemanticsEvaluationResult<TValue>.Error(Failure(error, member, rule));

    private static RuntimeSemanticsEvaluationResult<TValue> Fail<TValue>(IVBRaisableError error)
        where TValue : VBTypedValue
        => RuntimeSemanticsEvaluationResult<TValue>.Error(error);

    private static RuntimeSemanticsEvaluationResult<TValue> Succeed<TValue>(TValue value)
        where TValue : VBTypedValue
        => RuntimeSemanticsEvaluationResult<TValue>.Success(value);

    private static RuntimeSemanticsEvaluationResult<VBVariantValue> InVariant(VBTypedValue value) => Succeed(new VBVariantValue(value));

    private static RuntimeSemanticsEvaluationResult<VBVariantValue> InVariant(string text) => InVariant(new VBStringValue(text));

    // a character member reads a String through this and nothing else, so a trailing byte is dropped the way Len drops it;
    // a byte member never comes through here.
    private static string Text(VBStringValue value) => value.Value;

    // a typed optional is filled from its declaration before any call reaches a member - by the call site from source, by the
    // dispatcher for a direct call that leaves it out - so null here is a C# caller's mistake. Members never restate a default.
    private static TValue Given<TValue>(TValue? argument, [CallerArgumentExpression(nameof(argument))] string name = "")
        where TValue : VBTypedValue
        => argument ?? throw new ArgumentNullException(name, "an omitted optional is filled from its declaration before the call reaches its implementation");

    // a Variant argument's String, as a String parameter's Let-coercion would give it (MS-VBAL 5.5.1.2.4), in the regional
    // settings of the session's environment; null for Null, which each member answers for itself. Nothing is error 91. A value
    // with no String form (an Error, Missing, an array that is not a Byte array, a record) is a type mismatch, and so is an object
    // that is not Nothing (see the remarks).
    private bool TryReadString(VBTypedValue argument, string member, string parameter, out VBStringValue? text, [NotNullWhen(false)] out IVBRaisableError? error)
    {
        var value = Unwrapped(argument);
        text = null;
        error = NothingFailure(value, member, parameter);
        if (error is not null)
        {
            return false;
        }

        if (value is VBNullValue)
        {
            return true;
        }

        var converted = ValueConversions.ToText(value, session.Environment.Culture);
        if (converted.IsSuccess)
        {
            text = (VBStringValue)converted.Value!;
            return true;
        }

        error = Failure(converted.Error ?? VBRuntimeErrorId.TypeMismatch, member, $"{parameter} cannot be a value of type {value.TypeInfo.Name}");
        return false;
    }

    // a Variant argument's Long, as a Long parameter's Let-coercion would give it (MS-VBAL 5.5.1.2.1): Empty is 0, True -1, a
    // number or a date rounds to even (6 outside a Long), a numeric String is read as one; Null is 94, Nothing 91, anything else 13.
    private static bool TryReadLong(VBTypedValue argument, string member, string parameter, out int number, [NotNullWhen(false)] out IVBRaisableError? error)
    {
        var value = Unwrapped(argument);
        number = default;
        error = NothingFailure(value, member, parameter);
        if (error is not null)
        {
            return false;
        }

        var converted = ValueConversions.ToNumeric(value, VBLongType.TypeInfo);
        if (converted.IsSuccess)
        {
            number = (int)((VBNumericTypedValue)converted.Value!).AsDouble;
            error = null;
            return true;
        }

        error = converted.Error switch
        {
            VBRuntimeErrorId.InvalidUseOfNull => Failure(VBRuntimeErrorId.InvalidUseOfNull, member, $"{parameter} is Null"),
            VBRuntimeErrorId.Overflow => Failure(VBRuntimeErrorId.Overflow, member, $"{parameter} is outside the range of a Long"),
            _ => Failure(VBRuntimeErrorId.TypeMismatch, member, $"{parameter} cannot be a value of type {value.TypeInfo.Name}"),
        };
        return false;
    }

    // Nothing, Let-coerced to a value of any type, is error 91 (MS-VBAL 5.5.1.2.13); null for anything else.
    private static VBRuntimeErrorInfo? NothingFailure(VBTypedValue value, string member, string parameter)
        => value is VBObjectValue held && held.IsNothing()
            ? Failure(VBRuntimeErrorId.ObjectVariableOrWithBlockVariableNotSet, member, $"{parameter} is Nothing")
            : null;

    // an explicit Compare is vbBinaryCompare or vbTextCompare, and nothing else; an omitted one has already been filled in as one
    // of the two, Option Compare Database resolved, so this never sees a caller's omission.
    private bool TryRulesFor(VBCompareMethod compare, string member, out StringComparisonRules rules, [NotNullWhen(false)] out IVBRaisableError? error)
    {
        (rules, error) = compare switch
        {
            VBCompareMethod.VBBinaryCompare => (session.StringComparisonIn(OptionCompare.Binary), null),
            VBCompareMethod.VBTextCompare => (session.StringComparisonIn(OptionCompare.Text), null),
            _ => (default(StringComparisonRules), Failure(VBRuntimeErrorId.InvalidProcedureCallOrArgument, member,
                $"Compare is vbBinaryCompare (0) or vbTextCompare (1), not {(int)compare}")),
        };
        return error is null;
    }

    // every $ function: "functionally identical to the Left function, with the exception that the return type of the function is
    // String rather than Variant" - the Variant member's result, Let-coerced to String, where Null is 94 (MS-VBAL 5.5.1.2.10).
    private static RuntimeSemanticsEvaluationResult<VBStringValue> Twin(string member, RuntimeSemanticsEvaluationResult<VBVariantValue> variant)
        => variant switch
        {
            { IsSuccess: true, Result.TypedValue: VBStringValue text } => Succeed(text),
            { IsSuccess: true, Result.TypedValue: VBNullValue } => Fail<VBStringValue>(VBRuntimeErrorId.InvalidUseOfNull, member, "a String cannot hold Null"),
            { ErrorInfo: { } error } => Fail<VBStringValue>(error),
            // a Variant member returned neither a String nor Null: a bug, not a program's error.
            _ => RuntimeSemanticsEvaluationResult<VBStringValue>.InternalError(),
        };

    // the character a code names in the environment's ANSI code page: one byte up to 255, a lead and a trail byte above it. A
    // single-byte code page names one with every byte. A double-byte code page names one only where it reads the bytes as exactly
    // one character it defines: not a lone lead byte, not two single-byte characters, and not the character its decoder puts in
    // place of bytes it does not define.
    private bool TryCharacterOf(int code, [NotNullWhen(true)] out string? character)
    {
        if (code < 128)
        {
            character = ((char)code).ToString();
            return true;
        }

        byte[] bytes = code <= 0xFF ? [(byte)code] : [(byte)(code >> 8), (byte)code];
        var encoding = session.Environment.AnsiEncoding;
        if (encoding.IsSingleByte)
        {
            character = encoding.GetString(bytes);
            return true;
        }

        var strict = (System.Text.Encoding)encoding.Clone();
        strict.DecoderFallback = System.Text.DecoderFallback.ExceptionFallback;
        try
        {
            var decoded = strict.GetString(bytes);
            character = decoded.Length == 1 ? decoded : null;
        }
        catch (System.Text.DecoderFallbackException)
        {
            character = null;
        }

        return character is not null;
    }

    // a String longer than .NET can hold is out of string space, and so is one the allocation fails for.
    private static bool TryRepeat(char character, int count, string member, [NotNullWhen(true)] out string? text, [NotNullWhen(false)] out IVBRaisableError? error)
    {
        if (count <= LongestString)
        {
            try
            {
                text = new string(character, count);
                error = null;
                return true;
            }
            catch (OutOfMemoryException)
            {
            }
        }

        text = null;
        error = Failure(VBRuntimeErrorId.OutOfStringSpace, member, $"{count} characters are more than a String can hold");
        return false;
    }

    /// <summary>
    /// The number of characters in a string, or the number of bytes a value of another type occupies
    /// (<strong>MS-VBAL §6.1.2.11.1.22</strong>).
    /// </summary>
    /// <remarks>
    /// "With user-defined types, <c>Len</c> returns the size as it will be written to the file" — the
    /// concatenation of the members with no padding between them, which is what a <c>Put</c> statement writes
    /// and therefore what this has to agree with. <see cref="LenB"/> is the in-memory size instead.
    /// </remarks>
    public RuntimeSemanticsEvaluationResult<VBVariantValue> Len(VBVariantValue expression)
        => Measure(expression, inMemory: false);

    /// <summary>
    /// The number of <em>bytes</em> a value occupies (<strong>MS-VBAL §6.1.2.11.1.22</strong>).
    /// </summary>
    /// <remarks>
    /// "<c>LenB</c> will return the same value as <c>Len</c>, except for strings or UDTs": a string's is
    /// "the number of bytes used to represent that string", and a UDT's "in-memory size, including any
    /// implementation-specific padding between elements" is what <see cref="VBUserDefinedTypeLayout"/> lays out.
    /// </remarks>
    public RuntimeSemanticsEvaluationResult<VBVariantValue> LenB(VBVariantValue expression)
        => Measure(expression, inMemory: true);

    // one measurement with one dial, because the specification defines LenB as Len "except for strings or
    // UDTs" - stating one function and its two exceptions rather than two functions.
    private static RuntimeSemanticsEvaluationResult<VBVariantValue> Measure(VBVariantValue expression, bool inMemory)
    {
        var value = Unwrapped(expression);

        // "If Expression contains the data value Null, Null is returned" - the one case that is not a number.
        if (value is VBNullValue)
        {
            return RuntimeSemanticsEvaluationResult<VBVariantValue>.Success(new VBVariantValue(VBNullValue.Null));
        }

        return Length(value, inMemory) is { } length
            ? RuntimeSemanticsEvaluationResult<VBVariantValue>.Success(new VBVariantValue(new VBLongValue(length)))
            : NotImplemented<VBVariantValue>(inMemory ? nameof(LenB) : nameof(Len));
    }

    private static int? Length(VBTypedValue value, bool inMemory) => value switch
    {
        // a fixed-length String is its declared length, whatever it currently holds.
        VBFixedStringValue fixedString => inMemory ? 2 * fixedString.Length : fixedString.Length,
        // "Returns a Long containing the number of characters in a string", and LenB "returns the number of
        // bytes used to represent that string" - every byte of its data, a trailing one included.
        VBStringValue text => inMemory ? text.ByteLength : text.Length,
        // the two UDT sizes: LenB's is the padded in-memory one the value reports, Len's is the unpadded
        // concatenation a record is written from.
        VBUserDefinedTypeValue udt => inMemory ? udt.Size : SerializedSize(udt),
        // "the number of bytes required to store a variable on the current platform", which for every scalar
        // is the size the value itself reports.
        VBEmptyValue => 0,
        _ => value.Size,
    };

    // Len of a UDT is "the size as it will be written to the file": the members' own file widths, added up,
    // with none of the padding LenB counts. A variable-length String member is the case MS-VBAL warns about -
    // "Len might not be able to determine the actual number of storage bytes required when used with
    // variable-length strings in user-defined data types" - and its file width is its characters.
    private static int? SerializedSize(VBUserDefinedTypeValue udt)
    {
        var total = 0;
        for (var index = 0; index < udt.Fields.Length; index++)
        {
            if (udt.FieldAt(index) is not { } field || Length(field, inMemory: false) is not { } width)
            {
                return null;
            }

            total += width;
        }

        return total;
    }

    // a Variant's own TypeInfo mirrors what it wraps while the instance stays a VBVariantValue, so measuring
    // one has to see the wrapped value - "if the variable name is a Variant, Len/LenB treats it the same as a
    // String and always returns the number of characters it contains".
    private static VBTypedValue Unwrapped(VBTypedValue value)
    {
        while (value is VBVariantValue { TypedValue: { } wrapped })
        {
            value = wrapped;
        }

        return value;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// "Returns an Integer data value representing the 7-bit ASCII code point of the first character of StringValue. If the
    /// character does not correspond to an ASCII character the result is implementation defined": its code in the environment's
    /// ANSI code page, a character the code page lacks being what its encoder maps it to, and a double-byte code read as one
    /// number - "code point value greater than 32,767 are returned as negative Integer data values".
    /// </remarks>
    public RuntimeSemanticsEvaluationResult<VBIntegerValue> Asc(VBStringValue stringValue)
    {
        if (!TryFirstCharacter(stringValue, nameof(Asc), out var first, out var error))
        {
            return Fail<VBIntegerValue>(error);
        }

        if (first < 128)
        {
            return Succeed(new VBIntegerValue((short)first));
        }

        Span<byte> code = stackalloc byte[8];
        var length = session.Environment.AnsiEncoding.GetBytes(Text(stringValue).AsSpan(0, 1), code);
        return Succeed(new VBIntegerValue(length == 1 ? code[0] : unchecked((short)((code[0] << 8) | code[1]))));
    }

    /// <inheritdoc/>
    /// <remarks>
    /// "The first eight bits (the first byte) of the implementation dependent character encoding of the string", which is
    /// UTF-16LE: the low byte of the first character, or the byte of a String that is a single byte.
    /// </remarks>
    public RuntimeSemanticsEvaluationResult<VBIntegerValue> AscB(VBStringValue stringValue)
    {
        var bytes = stringValue.GetBytes();
        return bytes.Length == 0
            ? Fail<VBIntegerValue>(VBRuntimeErrorId.InvalidProcedureCallOrArgument, nameof(AscB), "String is a zero-length string")
            : Succeed(new VBIntegerValue(bytes[0]));
    }

    /// <inheritdoc/>
    /// <remarks>
    /// "The 16-bit Unicode code point of the first character": its first code unit, so the high half of a surrogate pair.
    /// </remarks>
    public RuntimeSemanticsEvaluationResult<VBIntegerValue> AscW(VBStringValue stringValue)
        => TryFirstCharacter(stringValue, nameof(AscW), out var first, out var error)
            ? Succeed(new VBIntegerValue(unchecked((short)first)))
            : Fail<VBIntegerValue>(error);

    // "If the argument is the null string ("") Error Number 5"; a String that is a single byte has no first character either.
    private static bool TryFirstCharacter(VBStringValue value, string member, out char first, [NotNullWhen(false)] out IVBRaisableError? error)
    {
        var text = Text(value);
        if (text.Length == 0)
        {
            first = default;
            error = Failure(VBRuntimeErrorId.InvalidProcedureCallOrArgument, member,
                value.TrailingByte is null ? "String is a zero-length string" : "String is a single byte and no character");
            return false;
        }

        first = text[0];
        error = null;
        return true;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// 0 to 127 is "interpreted as a 7-bit ASCII code point", and 128 to 255 is "implementation defined": the character of the
    /// environment's ANSI code page. Anything else is error 5 "unless the implementation supports a character set with a larger
    /// code point range", which a double-byte code page is: there a code is -32,768 to 65,535, a negative one read as the code plus
    /// 65,536, and a code above 255 is a lead and a trail byte. The result is "a single character", so there a code from 128 on is
    /// error 5 unless the code page reads its byte, or its two bytes, as one character it defines: not a lone lead byte, not two
    /// single-byte characters, and not the character its decoder puts in place of bytes it does not define.
    /// </remarks>
    public RuntimeSemanticsEvaluationResult<VBVariantValue> Chr(VBLongValue charCode)
    {
        var code = charCode.Value;
        if (session.Environment.AnsiEncoding.IsSingleByte)
        {
            if (code is < 0 or > 0xFF)
            {
                return Fail<VBVariantValue>(VBRuntimeErrorId.InvalidProcedureCallOrArgument, nameof(Chr), $"CharCode is 0 to 255 in a single-byte code page, not {code}");
            }
        }
        else if (code is < short.MinValue or > ushort.MaxValue)
        {
            return Fail<VBVariantValue>(VBRuntimeErrorId.InvalidProcedureCallOrArgument, nameof(Chr), $"CharCode is -32768 to 65535 in a double-byte code page, not {code}");
        }

        return TryCharacterOf(code < 0 ? code + 0x10000 : code, out var character)
            ? InVariant(character)
            : Fail<VBVariantValue>(VBRuntimeErrorId.InvalidProcedureCallOrArgument, nameof(Chr), $"CharCode {code} names no character in the code page");
    }

    /// <inheritdoc/>
    public RuntimeSemanticsEvaluationResult<VBStringValue> ChrString(VBLongValue charCode) => Twin("Chr$", Chr(charCode));

    /// <inheritdoc/>
    /// <remarks>
    /// "A String data value consisting of a single byte character whose code point value is the data value of the argument": a
    /// String of that one byte, whose <c>Len</c> is 0 and <c>LenB</c> 1. "If the argument is not in the range 0 to 255, Error
    /// Number 6 ("Overflow") is raised."
    /// </remarks>
    public RuntimeSemanticsEvaluationResult<VBVariantValue> ChrB(VBLongValue charCode)
    {
        var code = charCode.Value;
        return code is < 0 or > 0xFF
            ? Fail<VBVariantValue>(VBRuntimeErrorId.Overflow, nameof(ChrB), $"CharCode is 0 to 255, not {code}")
            : InVariant(VBStringValue.FromBytes([(byte)code]));
    }

    /// <inheritdoc/>
    public RuntimeSemanticsEvaluationResult<VBStringValue> ChrBString(VBLongValue charCode) => Twin("ChrB$", ChrB(charCode));

    /// <inheritdoc/>
    /// <remarks>
    /// The character of a 16-bit code point, a negative argument "treated as if it was the value: CharCode + 65,536"; a lone
    /// surrogate is a character like any other. 👉 The range is -32,768 to 65,535 where the specification says "-32,767": see
    /// <see cref="LowestWideCode"/>.
    /// </remarks>
    public RuntimeSemanticsEvaluationResult<VBVariantValue> ChrW(VBLongValue charCode)
    {
        var code = charCode.Value;
        return code is < LowestWideCode or > ushort.MaxValue
            ? Fail<VBVariantValue>(VBRuntimeErrorId.InvalidProcedureCallOrArgument, nameof(ChrW), $"CharCode is -32768 to 65535, not {code}")
            : InVariant(((char)(code < 0 ? code + 0x10000 : code)).ToString());
    }

    /// <inheritdoc/>
    public RuntimeSemanticsEvaluationResult<VBStringValue> ChrWString(VBLongValue charCode) => Twin("ChrW$", ChrW(charCode));

    public RuntimeSemanticsEvaluationResult<VBVariantValue> Filter( VBVariantValue sourceArray, VBStringValue match, VBBooleanValue? include = default, VBCompareMethod compare = VBCompareMethod.VBBinaryCompare) => NotImplemented<VBVariantValue>(nameof(Filter));

    public RuntimeSemanticsEvaluationResult<VBVariantValue> Format( VBVariantValue expression, VBVariantValue? format = default, VBDayOfWeek firstDayOfWeek = VBDayOfWeek.VBSunday, VBFirstWeekOfYear firstWeekOfYear = VBFirstWeekOfYear.VBFirstJan1) => NotImplemented<VBVariantValue>(nameof(Format));

    public RuntimeSemanticsEvaluationResult<VBStringValue> FormatString( VBVariantValue expression, VBVariantValue? format = default, VBDayOfWeek firstDayOfWeek = VBDayOfWeek.VBSunday, VBFirstWeekOfYear firstWeekOfYear = VBFirstWeekOfYear.VBFirstJan1) => NotImplemented<VBStringValue>(nameof(FormatString));

    public RuntimeSemanticsEvaluationResult<VBStringValue> FormatCurrency( VBVariantValue expression, VBLongValue? numDigitsAfterDecimal = default, VBTriState includeLeadingDigit = VBTriState.VBUseDefault, VBTriState useParensForNegativeNumbers = VBTriState.VBUseDefault, VBTriState groupDigits = VBTriState.VBUseDefault) => NotImplemented<VBStringValue>(nameof(FormatCurrency));

    public RuntimeSemanticsEvaluationResult<VBStringValue> FormatDateTime( VBVariantValue expression, VBDateTimeFormat namedFormat = VBDateTimeFormat.VBGeneralDate) => NotImplemented<VBStringValue>(nameof(FormatDateTime));

    public RuntimeSemanticsEvaluationResult<VBStringValue> FormatNumber( VBVariantValue expression, VBLongValue? numDigitsAfterDecimal = default, VBTriState includeLeadingDigit = VBTriState.VBUseDefault, VBTriState useParensForNegativeNumbers = VBTriState.VBUseDefault, VBTriState groupDigits = VBTriState.VBUseDefault) => NotImplemented<VBStringValue>(nameof(FormatNumber));

    public RuntimeSemanticsEvaluationResult<VBStringValue> FormatPercent( VBVariantValue expression, VBLongValue? numDigitsAfterDecimal = default, VBTriState includeLeadingDigit = VBTriState.VBUseDefault, VBTriState useParensForNegativeNumbers = VBTriState.VBUseDefault, VBTriState groupDigits = VBTriState.VBUseDefault) => NotImplemented<VBStringValue>(nameof(FormatPercent));

    public RuntimeSemanticsEvaluationResult<VBVariantValue> InStr( VBVariantValue? arg1 = default, VBVariantValue? arg2 = default, VBVariantValue? arg3 = default, VBCompareMethod compare = VBCompareMethod.VBBinaryCompare) => NotImplemented<VBVariantValue>(nameof(InStr));

    public RuntimeSemanticsEvaluationResult<VBVariantValue> InStrB( VBVariantValue? arg1 = default, VBVariantValue? arg2 = default, VBVariantValue? arg3 = default, VBCompareMethod compare = VBCompareMethod.VBBinaryCompare) => NotImplemented<VBVariantValue>(nameof(InStrB));

    public RuntimeSemanticsEvaluationResult<VBLongValue> InStrRev( VBStringValue stringCheck, VBStringValue stringMatch, VBLongValue? start = default, VBCompareMethod compare = VBCompareMethod.VBBinaryCompare) => NotImplemented<VBLongValue>(nameof(InStrRev));

    public RuntimeSemanticsEvaluationResult<VBStringValue> Join(VBVariantValue sourceArray, VBVariantValue? delimiter = default) => NotImplemented<VBStringValue>(nameof(Join));

    /// <inheritdoc/>
    /// <remarks>
    /// "Only uppercase letters are converted to lowercase; all lowercase letters and non-letter characters remain unchanged", by
    /// the casing of the environment's regional settings (a Turkish <c>I</c> becomes a dotless <c>i</c>), one character for one.
    /// "If String contains the data value Null, Null is returned."
    /// </remarks>
    public RuntimeSemanticsEvaluationResult<VBVariantValue> LCase(VBVariantValue @string) => Cased(nameof(LCase), @string, upper: false);

    /// <inheritdoc/>
    public RuntimeSemanticsEvaluationResult<VBStringValue> LCaseString(VBVariantValue @string) => Twin("LCase$", LCase(@string));

    // LCase and UCase: TextInfo maps each character to one character, so "ß" stays "ß" and the length never changes.
    private RuntimeSemanticsEvaluationResult<VBVariantValue> Cased(string member, VBVariantValue @string, bool upper)
    {
        if (!TryReadString(@string, member, "String", out var text, out var error))
        {
            return Fail<VBVariantValue>(error);
        }

        if (text is null)
        {
            return InVariant(VBNullValue.Null);
        }

        var casing = session.Environment.Culture.TextInfo;
        return InVariant(upper ? casing.ToUpper(Text(text)) : casing.ToLower(Text(text)));
    }

    /// <inheritdoc/>
    /// <remarks>
    /// The first Length characters, "the entire string" when Length is at least its length, a zero-length string for 0; "if string
    /// contains Null, Null is returned", whatever Length is. A character is a UTF-16 code unit, so a surrogate pair can be split.
    /// 👉 A negative Length is error 5, as a negative length is to the <c>Mid</c> statement (<strong>MS-VBAL §5.4.3.5</strong>).
    /// </remarks>
    public RuntimeSemanticsEvaluationResult<VBVariantValue> Left(VBVariantValue @string, VBLongValue length) => Take(nameof(Left), @string, length, fromEnd: false, inBytes: false);

    /// <inheritdoc/>
    /// <remarks>
    /// <see cref="Left"/> in bytes: "instead of specifying the number of characters to return, length specifies the number of
    /// bytes", so an odd Length gives a String that ends in a byte.
    /// </remarks>
    public RuntimeSemanticsEvaluationResult<VBVariantValue> LeftB(VBVariantValue @string, VBLongValue length) => Take(nameof(LeftB), @string, length, fromEnd: false, inBytes: true);

    /// <inheritdoc/>
    public RuntimeSemanticsEvaluationResult<VBStringValue> LeftString(VBVariantValue @string, VBLongValue length) => Twin("Left$", Left(@string, length));

    /// <inheritdoc/>
    public RuntimeSemanticsEvaluationResult<VBStringValue> LeftBString(VBVariantValue @string, VBLongValue length) => Twin("LeftB$", LeftB(@string, length));

    // Left, Right and their byte forms: the String's coercion, then its Null, then Length's check.
    private RuntimeSemanticsEvaluationResult<VBVariantValue> Take(string member, VBVariantValue @string, VBLongValue length, bool fromEnd, bool inBytes)
    {
        if (!TryReadString(@string, member, "String", out var text, out var error))
        {
            return Fail<VBVariantValue>(error);
        }

        if (text is null)
        {
            return InVariant(VBNullValue.Null);
        }

        var count = length.Value;
        if (count < 0)
        {
            return Fail<VBVariantValue>(VBRuntimeErrorId.InvalidProcedureCallOrArgument, member, $"Length cannot be negative, not {count}");
        }

        if (inBytes)
        {
            var bytes = text.GetBytes();
            var taken = Math.Min(count, bytes.Length);
            return InVariant(VBStringValue.FromBytes(fromEnd ? bytes.AsSpan(bytes.Length - taken) : bytes.AsSpan(0, taken)));
        }

        var characters = Text(text);
        var kept = Math.Min(count, characters.Length);
        return InVariant(fromEnd ? characters[^kept..] : characters[..kept]);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// "A copy of a specified string without leading spaces": spaces are U+0020, and nothing else is removed. "If String contains
    /// the data value Null, Null is returned."
    /// </remarks>
    public RuntimeSemanticsEvaluationResult<VBVariantValue> LTrim(VBVariantValue @string) => Trimmed(nameof(LTrim), @string, leading: true, trailing: false);

    /// <inheritdoc/>
    public RuntimeSemanticsEvaluationResult<VBStringValue> LTrimString(VBVariantValue @string) => Twin("LTrim$", LTrim(@string));

    /// <inheritdoc/>
    /// <remarks>
    /// "A copy of a specified string without [...] trailing spaces": spaces are U+0020, and nothing else is removed. "If String
    /// contains the data value Null, Null is returned."
    /// </remarks>
    public RuntimeSemanticsEvaluationResult<VBVariantValue> RTrim(VBVariantValue @string) => Trimmed(nameof(RTrim), @string, leading: false, trailing: true);

    /// <inheritdoc/>
    public RuntimeSemanticsEvaluationResult<VBStringValue> RTrimString(VBVariantValue @string) => Twin("RTrim$", RTrim(@string));

    /// <inheritdoc/>
    /// <remarks>
    /// "A copy of a specified string without [...] both leading and trailing spaces": spaces are U+0020, so a tab, a line break, a
    /// no-break space or an ideographic space stays. "If String contains the data value Null, Null is returned."
    /// </remarks>
    public RuntimeSemanticsEvaluationResult<VBVariantValue> Trim(VBVariantValue @string) => Trimmed(nameof(Trim), @string, leading: true, trailing: true);

    /// <inheritdoc/>
    public RuntimeSemanticsEvaluationResult<VBStringValue> TrimString(VBVariantValue @string) => Twin("Trim$", Trim(@string));

    private RuntimeSemanticsEvaluationResult<VBVariantValue> Trimmed(string member, VBVariantValue @string, bool leading, bool trailing)
    {
        if (!TryReadString(@string, member, "String", out var text, out var error))
        {
            return Fail<VBVariantValue>(error);
        }

        if (text is null)
        {
            return InVariant(VBNullValue.Null);
        }

        var characters = Text(text);
        return InVariant(leading && trailing
            ? characters.Trim(SpaceCharacter)
            : leading ? characters.TrimStart(SpaceCharacter) : characters.TrimEnd(SpaceCharacter));
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Length characters from Start: "if omitted or if there are fewer than Length characters in the text (including the character
    /// at start), all characters from the start position to the end of the string are returned", and "if Start is greater than the
    /// number of characters in String, Mid returns a zero-length string". "If String contains the data value Null, Null is
    /// returned." Length is a <c>Variant</c>, Let-coerced here as a <c>Long</c> parameter would be, after the String and before
    /// its Null. 👉 A Start below 1 and a negative Length are error 5, past the end or not, as they are to the <c>Mid</c>
    /// statement (<strong>MS-VBAL §5.4.3.5</strong>).
    /// </remarks>
    public RuntimeSemanticsEvaluationResult<VBVariantValue> Mid(VBVariantValue @string, VBLongValue start, VBVariantValue? length = default) => Middle(nameof(Mid), @string, start, length, inBytes: false);

    /// <inheritdoc/>
    /// <remarks>
    /// <see cref="Mid"/> in bytes: "the arguments specify numbers of bytes", so a slice can begin or end inside a character.
    /// </remarks>
    public RuntimeSemanticsEvaluationResult<VBVariantValue> MidB(VBVariantValue @string, VBLongValue start, VBVariantValue? length = default) => Middle(nameof(MidB), @string, start, length, inBytes: true);

    /// <inheritdoc/>
    public RuntimeSemanticsEvaluationResult<VBStringValue> MidString(VBVariantValue @string, VBLongValue start, VBVariantValue? length = default) => Twin("Mid$", Mid(@string, start, length));

    /// <inheritdoc/>
    public RuntimeSemanticsEvaluationResult<VBStringValue> MidBString(VBVariantValue @string, VBLongValue start, VBVariantValue? length = default) => Twin("MidB$", MidB(@string, start, length));

    private RuntimeSemanticsEvaluationResult<VBVariantValue> Middle(string member, VBVariantValue @string, VBLongValue start, VBVariantValue? length, bool inBytes)
    {
        // the coercions in the order the parameters are declared: String, then Length (the call site coerced Start, a Long).
        if (!TryReadString(@string, member, "String", out var text, out var error))
        {
            return Fail<VBVariantValue>(error);
        }

        int? count = null;
        if (length is not null)
        {
            if (!TryReadLong(length, member, "Length", out var given, out error))
            {
                return Fail<VBVariantValue>(error);
            }

            count = given;
        }

        if (text is null)
        {
            return InVariant(VBNullValue.Null);
        }

        var from = start.Value;
        if (from < 1)
        {
            return Fail<VBVariantValue>(VBRuntimeErrorId.InvalidProcedureCallOrArgument, member, $"Start is at least 1, not {from}");
        }

        if (count < 0)
        {
            return Fail<VBVariantValue>(VBRuntimeErrorId.InvalidProcedureCallOrArgument, member, $"Length cannot be negative, not {count}");
        }

        if (inBytes)
        {
            var bytes = text.GetBytes();
            var restOfBytes = from > bytes.Length ? ReadOnlySpan<byte>.Empty : bytes.AsSpan(from - 1);
            return InVariant(VBStringValue.FromBytes(restOfBytes[..Math.Min(count ?? restOfBytes.Length, restOfBytes.Length)]));
        }

        var characters = Text(text).AsSpan();
        var rest = from > characters.Length ? ReadOnlySpan<char>.Empty : characters[(from - 1)..];
        return InVariant(rest[..Math.Min(count ?? rest.Length, rest.Length)].ToString());
    }

    public RuntimeSemanticsEvaluationResult<VBStringValue> MonthName(VBLongValue month, VBBooleanValue? abbreviate = default) => NotImplemented<VBStringValue>(nameof(MonthName));

    public RuntimeSemanticsEvaluationResult<VBStringValue> Replace( VBStringValue expression, VBStringValue find, VBStringValue replace, VBLongValue? start = default, VBLongValue? count = default, VBCompareMethod compare = VBCompareMethod.VBBinaryCompare) => NotImplemented<VBStringValue>(nameof(Replace));

    /// <inheritdoc/>
    /// <remarks>
    /// The last Length characters, the mirror of <see cref="Left"/>: "the entire string" when Length is at least its length, a
    /// zero-length string for 0, Null for a Null String whatever Length is, and error 5 for a negative Length.
    /// </remarks>
    public RuntimeSemanticsEvaluationResult<VBVariantValue> Right(VBVariantValue @string, VBLongValue length) => Take(nameof(Right), @string, length, fromEnd: true, inBytes: false);

    /// <inheritdoc/>
    /// <remarks>
    /// <see cref="Right"/> in bytes: the last Length bytes, so an odd Length gives a String that begins inside a character.
    /// </remarks>
    public RuntimeSemanticsEvaluationResult<VBVariantValue> RightB(VBVariantValue @string, VBLongValue length) => Take(nameof(RightB), @string, length, fromEnd: true, inBytes: true);

    /// <inheritdoc/>
    public RuntimeSemanticsEvaluationResult<VBStringValue> RightString(VBVariantValue @string, VBLongValue length) => Twin("Right$", Right(@string, length));

    /// <inheritdoc/>
    public RuntimeSemanticsEvaluationResult<VBStringValue> RightBString(VBVariantValue @string, VBLongValue length) => Twin("RightB$", RightB(@string, length));

    /// <inheritdoc/>
    /// <remarks>
    /// "A String consisting of the specified number of spaces." 👉 A negative Number is error 5, as a negative length is to the
    /// <c>Mid</c> statement (<strong>MS-VBAL §5.4.3.5</strong>), and more spaces than a String can hold is error 14.
    /// </remarks>
    public RuntimeSemanticsEvaluationResult<VBVariantValue> Space(VBLongValue number)
    {
        var count = number.Value;
        if (count < 0)
        {
            return Fail<VBVariantValue>(VBRuntimeErrorId.InvalidProcedureCallOrArgument, nameof(Space), $"Number cannot be negative, not {count}");
        }

        return TryRepeat(SpaceCharacter, count, nameof(Space), out var spaces, out var error) ? InVariant(spaces) : Fail<VBVariantValue>(error);
    }

    /// <inheritdoc/>
    public RuntimeSemanticsEvaluationResult<VBStringValue> SpaceString(VBLongValue number) => Twin("Space$", Space(number));

    public RuntimeSemanticsEvaluationResult<VBVariantValue> Split( VBStringValue expression, VBVariantValue? delimiter = default, VBLongValue? limit = default, VBCompareMethod compare = VBCompareMethod.VBBinaryCompare) => NotImplemented<VBVariantValue>(nameof(Split));

    public RuntimeSemanticsEvaluationResult<VBVariantValue> StrComp( VBVariantValue string1, VBVariantValue string2, VBCompareMethod compare = VBCompareMethod.VBBinaryCompare) => NotImplemented<VBVariantValue>(nameof(StrComp));

    public RuntimeSemanticsEvaluationResult<VBVariantValue> StrConv(VBVariantValue @string, VBStrConv conversion, VBLongValue? localeID = default) => NotImplemented<VBVariantValue>(nameof(StrConv));

    /// <inheritdoc/>
    /// <remarks>
    /// "A String containing a repeating character string of the length specified": the first character of a String Character,
    /// or the character a code names, as <see cref="Chr"/> names it - "if Character is a number greater than 255, String
    /// converts the number to a valid character code using the formula: character Mod 256". "If character contains Null, Null is
    /// returned"; a Null Number is error 94 at the call site, Number being a <c>Long</c>, whatever the table says.
    /// <para>
    /// 👉 A Character that is neither a String nor Null is a code, Let-coerced as a <c>Long</c> parameter would be: Empty is 0,
    /// <c>Chr(0)</c>, and True is -1. A zero-length String, a negative code, a code that names no character (a lone lead byte of a
    /// double-byte code page, as <see cref="Chr"/> has it) and a negative Number are error 5, and more characters than a String
    /// can hold error 14.
    /// </para>
    /// </remarks>
    public RuntimeSemanticsEvaluationResult<VBVariantValue> Repeat(VBLongValue number, VBVariantValue character)
    {
        const string member = "String";

        // the coercion first: a String is read for its first character, and anything else but Null as a code.
        var value = Unwrapped(character);
        var code = 0;
        if (value is not (VBStringValue or VBNullValue) && !TryReadLong(value, member, "Character", out code, out var mismatch))
        {
            return Fail<VBVariantValue>(mismatch);
        }

        if (value is VBNullValue)
        {
            return InVariant(VBNullValue.Null);
        }

        // then the values, in the order they are declared.
        var count = number.Value;
        if (count < 0)
        {
            return Fail<VBVariantValue>(VBRuntimeErrorId.InvalidProcedureCallOrArgument, member, $"Number cannot be negative, not {count}");
        }

        char repeated;
        if (value is VBStringValue text)
        {
            if (Text(text).Length == 0)
            {
                return Fail<VBVariantValue>(VBRuntimeErrorId.InvalidProcedureCallOrArgument, member, "Character is a zero-length string");
            }

            repeated = Text(text)[0];
        }
        else if (code < 0)
        {
            return Fail<VBVariantValue>(VBRuntimeErrorId.InvalidProcedureCallOrArgument, member, $"Character is a character code, which cannot be negative, not {code}");
        }
        else if (!TryCharacterOf(code % 256, out var named))
        {
            return Fail<VBVariantValue>(VBRuntimeErrorId.InvalidProcedureCallOrArgument, member, $"Character is the code {code}, whose byte {code % 256} names no character in the code page");
        }
        else
        {
            repeated = named[0];
        }

        return TryRepeat(repeated, count, member, out var repeats, out var error) ? InVariant(repeats) : Fail<VBVariantValue>(error);
    }

    /// <inheritdoc/>
    public RuntimeSemanticsEvaluationResult<VBStringValue> RepeatString(VBLongValue number, VBVariantValue character) => Twin("String$", Repeat(number, character));

    /// <inheritdoc/>
    /// <remarks>
    /// "A String in which the character order of a specified String is reversed": its code units, so a surrogate pair comes out
    /// as its two halves swapped, and a combining mark before the letter it followed. A Null Expression is error 94 at the call
    /// site ("If Expression is Null, an error occurs").
    /// </remarks>
    public RuntimeSemanticsEvaluationResult<VBStringValue> StrReverse(VBStringValue expression)
    {
        var characters = Text(expression).ToCharArray();
        Array.Reverse(characters);
        return Succeed(new VBStringValue(new string(characters)));
    }

    /// <inheritdoc/>
    /// <remarks>
    /// "Only lowercase letters are converted to uppercase; all uppercase letters and non-letter characters remain unchanged", by
    /// the casing of the environment's regional settings (a Turkish <c>i</c> becomes a dotted <c>I</c>), one character for one,
    /// so "ß" stays "ß". "If String contains the data value Null, Null is returned."
    /// </remarks>
    public RuntimeSemanticsEvaluationResult<VBVariantValue> UCase(VBVariantValue @string) => Cased(nameof(UCase), @string, upper: true);

    /// <inheritdoc/>
    public RuntimeSemanticsEvaluationResult<VBStringValue> UCaseString(VBVariantValue @string) => Twin("UCase$", UCase(@string));

    public RuntimeSemanticsEvaluationResult<VBStringValue> WeekdayName( VBLongValue weekday, VBBooleanValue? abbreviate = default, VBDayOfWeek firstDayOfWeek = VBDayOfWeek.VBUseSystemDayOfWeek) => NotImplemented<VBStringValue>(nameof(WeekdayName));

}
