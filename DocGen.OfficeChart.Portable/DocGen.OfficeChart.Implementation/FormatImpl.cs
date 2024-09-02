using System;
using System.Globalization;
using DocGen.OfficeChart.FormatParser;
using DocGen.OfficeChart.Implementation.Collections;
using DocGen.OfficeChart.Interfaces;
using DocGen.OfficeChart.Parser;
using DocGen.OfficeChart.Parser.Biff_Records;

namespace DocGen.OfficeChart.Implementation;

internal class FormatImpl : CommonObject, INumberFormat, IParentApplication, ICloneParent
{
	private FormatRecord m_format;

	private FormatSectionCollection m_parsedFormat;

	private FormatParserImpl m_parser;

	public int Index => m_format.Index;

	public string FormatString => m_format.FormatString;

	[CLSCompliant(false)]
	public FormatRecord Record => m_format;

	public OfficeFormatType FormatType
	{
		get
		{
			PrepareFormat();
			return m_parsedFormat[0].FormatType;
		}
	}

	public bool IsFraction
	{
		get
		{
			PrepareFormat();
			return m_parsedFormat[0].IsFraction;
		}
	}

	public bool IsScientific
	{
		get
		{
			PrepareFormat();
			return m_parsedFormat[0].IsScientific;
		}
	}

	public bool IsThousandSeparator
	{
		get
		{
			PrepareFormat();
			return m_parsedFormat[0].IsThousandSeparator;
		}
	}

	public int DecimalPlaces
	{
		get
		{
			PrepareFormat();
			return m_parsedFormat[0].DecimalNumber;
		}
	}

	protected FormatImpl(IApplication application, object parent)
		: base(application, parent)
	{
		SetParents();
	}

	[CLSCompliant(false)]
	public FormatImpl(IApplication application, object parent, FormatRecord format)
		: this(application, parent)
	{
		if (format == null)
		{
			throw new ArgumentNullException("format");
		}
		m_format = (FormatRecord)format.Clone();
	}

	public FormatImpl(IApplication application, object parent, int index, string strFormat)
		: this(application, parent)
	{
		if (strFormat == null)
		{
			throw new ArgumentNullException("strFormat");
		}
		if (strFormat.Length == 0)
		{
			throw new ArgumentException("strFormat - string cannot be empty.");
		}
		m_format = (FormatRecord)BiffRecordFactory.GetRecord(TBIFFRecord.Format);
		m_format.Index = index;
		m_format.FormatString = strFormat;
	}

	private void SetParents()
	{
		if (!(FindParent(typeof(FormatsCollection)) is FormatsCollection formatsCollection))
		{
			throw new ArgumentNullException("Parent", "Can't find parent collection of formats.");
		}
		m_parser = formatsCollection.Parser;
	}

	[CLSCompliant(false)]
	public void Serialize(OffsetArrayList records)
	{
		if (records == null)
		{
			throw new ArgumentNullException("records");
		}
		if (m_format == null)
		{
			throw new ApplicationException("Format was not initialized propertly.");
		}
		records.Add(m_format);
	}

	private void PrepareFormat()
	{
		if (m_parsedFormat == null)
		{
			string text = FormatString;
			if (base.Parent != null && base.Parent is FormatsCollection && Array.IndexOf((base.Parent as FormatsCollection).DEF_FORMAT_STRING, text) >= 0)
			{
				text = text.Replace("$", CultureInfo.CurrentCulture.NumberFormat.CurrencySymbol.ToString());
			}
			m_parsedFormat = m_parser.Parse(text);
		}
	}

	public OfficeFormatType GetFormatType(double value)
	{
		PrepareFormat();
		return m_parsedFormat.GetFormatType(value);
	}

	public OfficeFormatType GetFormatType(string value)
	{
		PrepareFormat();
		return m_parsedFormat.GetFormatType(value);
	}

	public string ApplyFormat(double value)
	{
		return ApplyFormat(value, bShowHiddenSymbols: false);
	}

	public string ApplyFormat(double value, bool bShowHiddenSymbols)
	{
		PrepareFormat();
		return m_parsedFormat.ApplyFormat(value, bShowHiddenSymbols);
	}

	public string ApplyFormat(string value)
	{
		return ApplyFormat(value, bShowHiddenSymbols: false);
	}

	public string ApplyFormat(string value, bool bShowHiddenSymbols)
	{
		PrepareFormat();
		return m_parsedFormat.ApplyFormat(value, bShowHiddenSymbols);
	}

	internal bool IsTimeFormat(double value)
	{
		return m_parsedFormat.IsTimeFormat(value);
	}

	internal bool IsDateFormat(double value)
	{
		return m_parsedFormat.IsDateFormat(value);
	}

	public object Clone(object parent)
	{
		FormatImpl formatImpl = (FormatImpl)MemberwiseClone();
		formatImpl.SetParent(parent);
		formatImpl.SetParents();
		formatImpl.m_format = (FormatRecord)CloneUtils.CloneCloneable(m_format);
		if (m_parsedFormat != null)
		{
			formatImpl.m_parsedFormat = (FormatSectionCollection)m_parsedFormat.Clone(formatImpl);
		}
		return formatImpl;
	}

	internal void Clear()
	{
		m_parser.Clear();
		if (m_parsedFormat != null)
		{
			m_parsedFormat.Dispose();
			m_parsedFormat.Clear();
		}
		m_format = null;
		m_parsedFormat = null;
		Dispose();
	}
}
