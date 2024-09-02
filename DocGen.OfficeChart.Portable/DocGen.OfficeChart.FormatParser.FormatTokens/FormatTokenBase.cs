using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace DocGen.OfficeChart.FormatParser.FormatTokens;

internal abstract class FormatTokenBase : ICloneable
{
	protected internal const RegexOptions DEF_OPTIONS = RegexOptions.None;

	protected string m_strFormat;

	public string Format
	{
		get
		{
			return m_strFormat;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (value.Length == 0)
			{
				throw new ArgumentException("value - string cannot be empty.");
			}
			if (m_strFormat != value)
			{
				m_strFormat = value;
				OnFormatChange();
			}
		}
	}

	public abstract TokenType TokenType { get; }

	public FormatTokenBase()
	{
	}

	public abstract int TryParse(string strFormat, int iIndex);

	protected int TryParseRegex(Regex regex, string strFormat, int iIndex)
	{
		Match m;
		return TryParseRegex(regex, strFormat, iIndex, out m);
	}

	protected int TryParseRegex(Regex regex, string strFormat, int iIndex, out Match m)
	{
		if (regex == null)
		{
			throw new ArgumentNullException("regex");
		}
		if (strFormat == null)
		{
			throw new ArgumentNullException("strFormat");
		}
		int length = strFormat.Length;
		if (length == 0)
		{
			throw new ArgumentException("strFormat - string cannot be empty");
		}
		if (iIndex < 0 || iIndex > length - 1)
		{
			throw new ArgumentOutOfRangeException("iIndex", "Value cannot be less than 0 or greater than Format Length - 1");
		}
		m = regex.Match(strFormat, iIndex);
		if (m.Success && m.Index == iIndex)
		{
			Format = m.Value;
			iIndex += m_strFormat.Length;
		}
		return iIndex;
	}

	public virtual string ApplyFormat(ref double value)
	{
		return ApplyFormat(ref value, bShowHiddenSymbols: false, null, null);
	}

	public abstract string ApplyFormat(string value, bool bShowHiddenSymbols);

	public virtual string ApplyFormat(string value)
	{
		return ApplyFormat(value, bShowHiddenSymbols: false);
	}

	public object Clone()
	{
		return MemberwiseClone();
	}

	public abstract string ApplyFormat(ref double value, bool bShowHiddenSymbols, CultureInfo culture, FormatSection section);

	public int FindString(string[] arrStrings, string strFormat, int iIndex, bool bIgnoreCase)
	{
		if (strFormat == null)
		{
			throw new ArgumentNullException("strFormat");
		}
		int length = strFormat.Length;
		if (length == 0)
		{
			throw new ArgumentException("strFormat - string cannot be empty.");
		}
		if (iIndex < 0 || iIndex > length - 1)
		{
			throw new ArgumentOutOfRangeException("iIndex", "Value cannot be less than 0 and greater than than format length - 1.");
		}
		int i = 0;
		for (int num = arrStrings.Length; i < num; i++)
		{
			string text = arrStrings[i];
			StringComparison comparisonType = (bIgnoreCase ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture);
			if (string.Compare(strFormat, iIndex, text, 0, text.Length, comparisonType) == 0)
			{
				return i;
			}
		}
		return -1;
	}

	protected virtual void OnFormatChange()
	{
	}
}
