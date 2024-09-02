using System;
using System.Collections.Generic;
using DocGen.OfficeChart.FormatParser.FormatTokens;
using DocGen.OfficeChart.Implementation;

namespace DocGen.OfficeChart.FormatParser;

internal class FormatParserImpl : CommonObject
{
	private List<FormatTokenBase> m_arrFormatTokens = new List<FormatTokenBase>();

	internal const string DEF_EXPONENTIAL = "E";

	private const string DEF_HASH = "#";

	public FormatParserImpl(IApplication application, object parent)
		: base(application, parent)
	{
		m_arrFormatTokens.Add(new GeneralToken());
		m_arrFormatTokens.Add(new StringToken());
		m_arrFormatTokens.Add(new ReservedPlaceToken());
		m_arrFormatTokens.Add(new CharacterToken());
		m_arrFormatTokens.Add(new YearToken());
		m_arrFormatTokens.Add(new MonthToken());
		m_arrFormatTokens.Add(new DayToken());
		m_arrFormatTokens.Add(new HourToken());
		m_arrFormatTokens.Add(new Hour24Token());
		m_arrFormatTokens.Add(new MinuteToken());
		m_arrFormatTokens.Add(new MinuteTotalToken());
		m_arrFormatTokens.Add(new SecondToken());
		m_arrFormatTokens.Add(new SecondTotalToken());
		m_arrFormatTokens.Add(new AmPmToken());
		m_arrFormatTokens.Add(new SectionSeparatorToken());
		m_arrFormatTokens.Add(new ColorToken());
		m_arrFormatTokens.Add(new ConditionToken());
		m_arrFormatTokens.Add(new TextToken());
		m_arrFormatTokens.Add(new SignificantDigitToken());
		m_arrFormatTokens.Add(new InsignificantDigitToken());
		m_arrFormatTokens.Add(new PlaceReservedDigitToken());
		m_arrFormatTokens.Add(new PercentToken());
		m_arrFormatTokens.Add(new DecimalPointToken());
		m_arrFormatTokens.Add(new ThousandsSeparatorToken());
		m_arrFormatTokens.Add(new AsterixToken());
		m_arrFormatTokens.Add(new ScientificToken());
		m_arrFormatTokens.Add(new FractionToken());
		m_arrFormatTokens.Add(new CultureToken());
		m_arrFormatTokens.Add(new UnknownToken());
	}

	public FormatSectionCollection Parse(string strFormat)
	{
		if (strFormat == null)
		{
			throw new ArgumentNullException("strFormat");
		}
		int length = strFormat.Length;
		if (length == 0)
		{
			throw new ArgumentException("strFormat - string cannot be empty");
		}
		List<FormatTokenBase> list = new List<FormatTokenBase>();
		int num = 0;
		while (num < length)
		{
			int i = 0;
			for (int count = m_arrFormatTokens.Count; i < count; i++)
			{
				FormatTokenBase formatTokenBase = m_arrFormatTokens[i];
				int num2 = formatTokenBase.TryParse(strFormat, num);
				if (num2 > num)
				{
					formatTokenBase = (FormatTokenBase)formatTokenBase.Clone();
					if (formatTokenBase.TokenType == TokenType.Section && list.Count == 0)
					{
						list.Add(m_arrFormatTokens[17]);
					}
					else if (list.Count > 0 && list[list.Count - 1].TokenType == TokenType.Section && formatTokenBase.TokenType == TokenType.Section)
					{
						list.Add(m_arrFormatTokens[17]);
					}
					num = num2;
					list.Add(formatTokenBase);
					break;
				}
			}
		}
		if (list.Count > 0 && list[list.Count - 1].TokenType == TokenType.Section)
		{
			list.Add(m_arrFormatTokens[17]);
		}
		return new FormatSectionCollection(base.Application, this, list);
	}

	internal void Clear()
	{
		if (m_arrFormatTokens != null)
		{
			m_arrFormatTokens.Clear();
		}
		m_arrFormatTokens = null;
	}
}
