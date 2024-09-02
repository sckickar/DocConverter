using System;
using System.Collections.Generic;
using DocGen.OfficeChart.FormatParser.FormatTokens;
using DocGen.OfficeChart.Implementation.Collections;

namespace DocGen.OfficeChart.FormatParser;

internal class FormatSectionCollection : CollectionBaseEx<FormatSection>
{
	private const string DEF_TWO_MANY_SECTIONS_MESSAGE = "Two many sections in format.";

	private const int DEF_CONDITION_MAX_COUNT = 3;

	private const int DEF_NONCONDITION_MAX_COUNT = 4;

	private const int DEF_POSITIVE_SECTION = 0;

	private const int DEF_NEGATIVE_SECTION = 1;

	private const int DEF_ZERO_SECTION = 2;

	private const int DEF_TEXT_SECTION = 3;

	private bool m_bConditionalFormat;

	private FormatSectionCollection(IApplication application, object parent)
		: base(application, parent)
	{
	}

	public FormatSectionCollection(IApplication application, object parent, List<FormatTokenBase> arrTokens)
		: base(application, parent)
	{
		if (arrTokens == null)
		{
			throw new ArgumentNullException("arrTokens");
		}
		Parse(arrTokens);
	}

	public OfficeFormatType GetFormatType(double value)
	{
		return (GetSection(value) ?? throw new FormatException("Can't find required format section.")).FormatType;
	}

	public OfficeFormatType GetFormatType(string value)
	{
		if (!HasDateTimeFormat())
		{
			return GetSection(3).FormatType;
		}
		return OfficeFormatType.DateTime;
	}

	private void Parse(List<FormatTokenBase> arrTokens)
	{
		if (arrTokens == null)
		{
			throw new ArgumentNullException("arrTokens");
		}
		List<FormatTokenBase> list = new List<FormatTokenBase>();
		int i = 0;
		for (int count = arrTokens.Count; i < count; i++)
		{
			FormatTokenBase formatTokenBase = arrTokens[i];
			if (formatTokenBase.TokenType == TokenType.Section)
			{
				base.InnerList.Add(new FormatSection(base.Application, this, list));
				list = new List<FormatTokenBase>();
			}
			else
			{
				list.Add(formatTokenBase);
			}
		}
		base.InnerList.Add(new FormatSection(base.Application, this, list));
		if (base[0].HasCondition)
		{
			int num = 0;
			for (int j = 0; j < base.Count; j++)
			{
				if (base[j].HasCondition)
				{
					num++;
				}
			}
			if (num > 3)
			{
				throw new FormatException("Two many sections in format.");
			}
			m_bConditionalFormat = true;
		}
		else if (base.Count > 4)
		{
			throw new FormatException("Two many sections in format.");
		}
	}

	public string ApplyFormat(double value, bool bShowReservedSymbols)
	{
		FormatSection section = GetSection(value);
		if (section != null)
		{
			if (!m_bConditionalFormat && value < 0.0 && base.Count > 1)
			{
				value = 0.0 - value;
			}
			return section.ApplyFormat(value, bShowReservedSymbols);
		}
		throw new FormatException("Can't locate correct section.");
	}

	public string ApplyFormat(string value, bool bShowReservedSymbols)
	{
		FormatSection textSection = GetTextSection();
		if (textSection == null)
		{
			return value;
		}
		return textSection.ApplyFormat(value, bShowReservedSymbols);
	}

	private FormatSection GetSection(int iSectionIndex)
	{
		return base[iSectionIndex % base.Count];
	}

	private FormatSection GetSection(double value)
	{
		FormatSection result = null;
		if (!m_bConditionalFormat)
		{
			result = ((value > 0.0) ? GetSection(0) : ((!(value < 0.0)) ? GetZeroSection() : GetSection(1)));
		}
		else
		{
			int count = base.Count;
			for (int i = 0; i < count; i++)
			{
				FormatSection formatSection = base[i];
				bool hasCondition = formatSection.HasCondition;
				if (!hasCondition || (hasCondition && formatSection.CheckCondition(value)))
				{
					result = formatSection;
					break;
				}
			}
		}
		return result;
	}

	private FormatSection GetZeroSection()
	{
		if (m_bConditionalFormat)
		{
			throw new NotSupportedException("This method is not supported for number formats with conditions.");
		}
		int num = base.InnerList.Count - 1;
		FormatSection formatSection = null;
		if (num < 2)
		{
			formatSection = base[0];
		}
		else if (num > 2)
		{
			formatSection = base[2];
		}
		else
		{
			formatSection = base[2];
			if (formatSection.FormatType == OfficeFormatType.Text)
			{
				formatSection = base[0];
			}
		}
		return formatSection;
	}

	private FormatSection GetTextSection()
	{
		FormatSection formatSection = null;
		if (!m_bConditionalFormat)
		{
			int num = base.InnerList.Count - 1;
			if (num >= 3)
			{
				formatSection = base[3];
			}
			else
			{
				formatSection = base[num];
				if (formatSection.FormatType != OfficeFormatType.Text)
				{
					formatSection = base[0];
				}
			}
		}
		return formatSection;
	}

	internal bool IsTimeFormat(double value)
	{
		if (value < 0.0)
		{
			return false;
		}
		return GetSection(value).IsTimeFormat;
	}

	internal bool IsDateFormat(double value)
	{
		if (value < 0.0)
		{
			return false;
		}
		return GetSection(value).IsDateFormat;
	}

	private bool HasDateTimeFormat()
	{
		using (IEnumerator<FormatSection> enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.FormatType == OfficeFormatType.DateTime)
				{
					return true;
				}
			}
		}
		return false;
	}

	public override object Clone(object parent)
	{
		if (parent == null)
		{
			throw new ArgumentNullException("parent");
		}
		FormatSectionCollection formatSectionCollection = new FormatSectionCollection(base.Application, parent);
		List<FormatSection> innerList = base.InnerList;
		List<FormatSection> innerList2 = formatSectionCollection.InnerList;
		int i = 0;
		for (int count = base.Count; i < count; i++)
		{
			FormatSection formatSection = innerList[i];
			formatSection = (FormatSection)formatSection.Clone(formatSectionCollection);
			innerList2.Add(formatSection);
		}
		return formatSectionCollection;
	}

	internal void Dispose()
	{
		int count = base.InnerList.Count;
		for (int i = 0; i < count; i++)
		{
			base.InnerList[i].Clear();
			base.InnerList[i] = null;
		}
	}
}
