using System;
using System.Collections.Generic;
using DocGen.Drawing;
using DocGen.OfficeChart.Interfaces;
using DocGen.OfficeChart.Parser;
using DocGen.OfficeChart.Parser.Biff_Records;
using DocGen.OfficeChart.Parser.Biff_Records.Charts;

namespace DocGen.OfficeChart.Implementation.Charts;

internal class ChartInteriorImpl : CommonObject, IOfficeChartInterior, ICloneParent
{
	private ChartAreaFormatRecord m_area;

	private WorkbookImpl m_book;

	private ChartSerieDataFormatImpl m_serieFormat;

	private ChartColor m_foreColor;

	private ChartColor m_backColor;

	private static Dictionary<OfficePattern, OfficeGradientPattern> m_hashPat;

	public ChartColor ForegroundColorObject => m_foreColor;

	public ChartColor BackgroundColorObject => m_backColor;

	public Color ForegroundColor
	{
		get
		{
			return m_foreColor.GetRGB(m_book);
		}
		set
		{
			m_foreColor.SetRGB(value, m_book);
		}
	}

	public Color BackgroundColor
	{
		get
		{
			return m_backColor.GetRGB(m_book);
		}
		set
		{
			m_backColor.SetRGB(value, m_book);
		}
	}

	public OfficePattern Pattern
	{
		get
		{
			if (!UseAutomaticFormat)
			{
				return m_area.Pattern;
			}
			return OfficePattern.Solid;
		}
		set
		{
			if (Pattern == value)
			{
				return;
			}
			IOfficeFill fill = (base.Parent as IFillColor).Fill;
			if (value < OfficePattern.Percent50)
			{
				if (Pattern > OfficePattern.Solid)
				{
					fill.Solid();
				}
			}
			else
			{
				fill.Patterned(m_hashPat[value]);
			}
			UseAutomaticFormat = false;
			m_area.Pattern = value;
		}
	}

	public OfficeKnownColors ForegroundColorIndex
	{
		get
		{
			return m_foreColor.GetIndexed(m_book);
		}
		set
		{
			m_foreColor.SetIndexed(value);
		}
	}

	public OfficeKnownColors BackgroundColorIndex
	{
		get
		{
			return m_backColor.GetIndexed(m_book);
		}
		set
		{
			m_backColor.SetIndexed(value);
		}
	}

	public bool UseAutomaticFormat
	{
		get
		{
			return m_area.UseAutomaticFormat;
		}
		set
		{
			if (value != UseAutomaticFormat)
			{
				m_area.UseAutomaticFormat = value;
				if (!value && m_area.Pattern == OfficePattern.None)
				{
					m_area.Pattern = OfficePattern.Solid;
				}
			}
		}
	}

	public bool SwapColorsOnNegative
	{
		get
		{
			return m_area.SwapColorsOnNegative;
		}
		set
		{
			m_area.SwapColorsOnNegative = value;
		}
	}

	static ChartInteriorImpl()
	{
		m_hashPat = new Dictionary<OfficePattern, OfficeGradientPattern>(18);
		m_hashPat.Add(OfficePattern.Percent50, OfficeGradientPattern.Pat_50_Percent);
		m_hashPat.Add(OfficePattern.Percent70, OfficeGradientPattern.Pat_70_Percent);
		m_hashPat.Add(OfficePattern.Percent25, OfficeGradientPattern.Pat_25_Percent);
		m_hashPat.Add(OfficePattern.Percent60, OfficeGradientPattern.Pat_30_Percent);
		m_hashPat.Add(OfficePattern.Percent10, OfficeGradientPattern.Pat_20_Percent);
		m_hashPat.Add(OfficePattern.Percent05, OfficeGradientPattern.Pat_10_Percent);
		for (int i = 5; i < 16; i++)
		{
			m_hashPat.Add((OfficePattern)i, (OfficeGradientPattern)(i + 8));
		}
	}

	public ChartInteriorImpl(IApplication application, object parent)
		: base(application, parent)
	{
		m_area = (ChartAreaFormatRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartAreaFormat);
		SetParents();
	}

	[CLSCompliant(false)]
	public ChartInteriorImpl(IApplication application, object parent, ChartAreaFormatRecord area)
		: base(application, parent)
	{
		if (area == null)
		{
			throw new ArgumentNullException("area");
		}
		m_area = area;
		SetParents();
	}

	public ChartInteriorImpl(IApplication application, object parent, IList<BiffRecordRaw> data, ref int iPos)
		: base(application, parent)
	{
		Parse(data, ref iPos);
		SetParents();
	}

	private void SetParents()
	{
		m_book = (WorkbookImpl)FindParent(typeof(WorkbookImpl));
		m_serieFormat = FindParent(typeof(ChartSerieDataFormatImpl)) as ChartSerieDataFormatImpl;
		if (m_book == null)
		{
			throw new ApplicationException("cannot find parent object");
		}
		m_foreColor = new ChartColor(m_area.ForegroundColorIndex);
		m_foreColor.AfterChange += UpdateForeColor;
		m_backColor = new ChartColor(m_area.BackgroundColorIndex);
		m_backColor.AfterChange += UpdateBackColor;
	}

	public void Parse(IList<BiffRecordRaw> data, ref int iPos)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		BiffRecordRaw biffRecordRaw = data[iPos];
		biffRecordRaw.CheckTypeCode(TBIFFRecord.ChartAreaFormat);
		m_area = (ChartAreaFormatRecord)biffRecordRaw;
		iPos++;
	}

	[CLSCompliant(false)]
	public void Serialize(IList<IBiffStorage> records)
	{
		if (records == null)
		{
			throw new ArgumentNullException("records");
		}
		if (m_area != null)
		{
			records.Add((BiffRecordRaw)m_area.Clone());
		}
	}

	internal void UpdateForeColor()
	{
		m_area.ForegroundColorIndex = ForegroundColorIndex;
		m_area.ForegroundColor = ForegroundColor.ToArgb() & 0xFFFFFF;
		UseAutomaticFormat = false;
		(base.Parent as IFillColor).Visible = true;
	}

	internal void UpdateBackColor()
	{
		m_area.BackgroundColorIndex = BackgroundColorIndex;
		m_area.BackgroundColor = BackgroundColor;
		UseAutomaticFormat = false;
		(base.Parent as IFillColor).Visible = true;
	}

	public void InitForFrameFormat(bool bIsAutoSize, bool bIs3DChart, bool bIsInteriorGray)
	{
		InitForFrameFormat(bIsAutoSize, bIs3DChart, bIsInteriorGray, bIsGray50: false);
	}

	public void InitForFrameFormat(bool bIsAutoSize, bool bIs3DChart, bool bIsInteriorGray, bool bIsGray50)
	{
		m_area.Pattern = OfficePattern.Solid;
		m_area.UseAutomaticFormat = bIs3DChart;
		m_area.SwapColorsOnNegative = false;
		m_area.ForegroundColorIndex = ((!bIsInteriorGray) ? OfficeKnownColors.White : OfficeKnownColors.Grey_25_percent);
		m_area.BackgroundColorIndex = (bIsAutoSize ? ((OfficeKnownColors)79) : ((OfficeKnownColors)77));
		if (bIsGray50)
		{
			m_area.ForegroundColorIndex = OfficeKnownColors.Grey_50_percent;
		}
	}

	public ChartInteriorImpl Clone(object parent)
	{
		if (parent == null)
		{
			throw new ArgumentNullException("parent");
		}
		ChartInteriorImpl obj = (ChartInteriorImpl)MemberwiseClone();
		obj.m_area = (ChartAreaFormatRecord)CloneUtils.CloneCloneable(m_area);
		obj.SetParent(parent);
		obj.SetParents();
		return obj;
	}

	object ICloneParent.Clone(object parent)
	{
		return Clone(parent);
	}
}
