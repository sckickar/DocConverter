using System;
using System.Collections.Generic;
using DocGen.Drawing;
using DocGen.OfficeChart.Implementation.XmlSerialization.Charts;
using DocGen.OfficeChart.Interfaces;
using DocGen.OfficeChart.Interfaces.Charts;
using DocGen.OfficeChart.Parser.Biff_Records;
using DocGen.OfficeChart.Parser.Biff_Records.Charts;

namespace DocGen.OfficeChart.Implementation.Charts;

internal class ChartDataLabelsImpl : CommonObject, IOfficeChartDataLabels, IOfficeChartTextArea, IParentApplication, IOfficeFont, IOptimizedUpdate, ISerializable, IInternalOfficeChartTextArea, IInternalFont
{
	private IRange m_valueFromIRange;

	private bool m_isHighlightColor;

	private bool m_conditionCheck;

	private ChartSerieImpl m_serie;

	private ChartImpl m_chart;

	private ChartTextAreaImpl m_textArea;

	private ChartDataPointImpl m_dataPoint;

	private IOfficeChartLayout m_layout;

	private bool m_isDelete;

	private ChartParagraphType m_paraType;

	internal const string DEFAULT_FONTNAME = "Tahoma";

	internal const string DEFAULT_LANGUAGE = "en-US";

	internal const double DEFAULT_FONTSIZE = 10.0;

	private bool m_bShowTextProperties = true;

	private bool m_bShowSizeProperties;

	private bool m_bShowBoldProperties;

	private bool m_bShowItalicProperties;

	internal bool m_bHasValueOption;

	internal bool m_bHasSeriesOption;

	internal bool m_bHasCategoryOption;

	internal bool m_bHasPercentageOption;

	internal bool m_bHasLegendKeyOption;

	internal bool m_bHasBubbleSizeOption;

	private bool m_isSourceLinked = true;

	private string m_numberFormat;

	private bool m_bIsFormula;

	private bool m_bShowLeaderLines;

	private string[] m_stringCache;

	internal bool m_bFontChanged;

	private bool m_isCreated;

	public bool IsValueFromCells
	{
		get
		{
			return m_textArea.IsValueFromCells;
		}
		set
		{
			m_textArea.IsValueFromCells = value;
			if (ParentBook.IsWorkbookOpening || !(base.Parent is ChartDataPointImpl) || m_conditionCheck)
			{
				return;
			}
			ChartDataPointsCollection chartDataPointsCollection = (base.Parent as ChartDataPointImpl).Parent as ChartDataPointsCollection;
			if (chartDataPointsCollection.m_hashDataPoints != null)
			{
				for (int i = 0; i < chartDataPointsCollection.m_hashDataPoints.Count; i++)
				{
					(chartDataPointsCollection.m_hashDataPoints[i].DataLabels as ChartDataLabelsImpl).m_conditionCheck = true;
					chartDataPointsCollection.m_hashDataPoints[i].DataLabels.IsValueFromCells = value;
					UpdateDataLabelText(chartDataPointsCollection.m_hashDataPoints[i].DataLabels as ChartDataLabelsImpl, value);
				}
			}
		}
	}

	public IOfficeDataRange ValueFromCellsRange
	{
		get
		{
			return m_textArea.ValueFromCellsRange;
		}
		set
		{
			int firstRow = value.FirstRow;
			int firstColumn = value.FirstColumn;
			int lastRow = value.LastRow;
			int lastColumn = value.LastColumn;
			ValueFromCellsIRange = ParentBook.Worksheets[m_serie.ParentChart.ActiveSheetIndex][firstRow, firstColumn, lastRow, lastColumn];
		}
	}

	internal IRange ValueFromCellsIRange
	{
		get
		{
			return m_valueFromIRange;
		}
		set
		{
			m_valueFromIRange = value;
			if (!ParentBook.IsWorkbookOpening && m_serie != null && value != null)
			{
				Dictionary<int, object> dictionary = new Dictionary<int, object>();
				for (int i = 0; i < value.Cells.Length; i++)
				{
					dictionary.Add(i, value.Cells[i].DisplayText);
				}
				m_serie.DataLabelCellsValues = dictionary;
			}
			ChartDataRange chartDataRange = new ChartDataRange(m_serie.ParentChart);
			chartDataRange.Range = ValueFromCellsIRange;
			m_textArea.ValueFromCellsRange = chartDataRange;
		}
	}

	public bool IsSeriesName
	{
		get
		{
			if (!m_bHasSeriesOption)
			{
				if (!ParentBook.Saving)
				{
					if (base.Application.DefaultVersion == OfficeVersion.Excel2013)
					{
						return true;
					}
					return m_textArea.IsSeriesName;
				}
				return m_textArea.IsSeriesName;
			}
			return m_textArea.IsSeriesName;
		}
		set
		{
			m_textArea.IsSeriesName = value;
			m_bHasSeriesOption = true;
			SetIndividualDataLabels(value, "SeriesName");
		}
	}

	public bool IsCategoryName
	{
		get
		{
			if (!m_bHasCategoryOption)
			{
				if (!ParentBook.Saving)
				{
					if (base.Application.DefaultVersion == OfficeVersion.Excel2013)
					{
						return true;
					}
					return m_textArea.IsCategoryName;
				}
				return m_textArea.IsCategoryName;
			}
			return m_textArea.IsCategoryName;
		}
		set
		{
			m_textArea.IsCategoryName = value;
			m_bHasCategoryOption = true;
			ChartSerieDataFormatImpl format = Format;
			if (format != null)
			{
				format.AttachedLabel.ShowCategoryLabel = value;
			}
			SetIndividualDataLabels(value, "CategoryName");
		}
	}

	public bool IsValue
	{
		get
		{
			if (!m_bHasValueOption)
			{
				if (!ParentBook.Saving)
				{
					if (base.Application.DefaultVersion == OfficeVersion.Excel2013)
					{
						return true;
					}
					return m_textArea.IsValue;
				}
				return m_textArea.IsValue;
			}
			return m_textArea.IsValue;
		}
		set
		{
			ChartSerieDataFormatImpl format = Format;
			m_bHasValueOption = true;
			if (format != null)
			{
				format.AttachedLabel.ShowActiveValue = value;
			}
			m_textArea.IsValue = value;
			SetIndividualDataLabels(value, "Value");
		}
	}

	public bool IsPercentage
	{
		get
		{
			if (!m_bHasPercentageOption)
			{
				if (!ParentBook.Saving)
				{
					if (base.Application.DefaultVersion == OfficeVersion.Excel2013)
					{
						return true;
					}
					return m_textArea.IsPercentage;
				}
				return m_textArea.IsPercentage;
			}
			return m_textArea.IsPercentage;
		}
		set
		{
			m_textArea.IsPercentage = value;
			m_bHasPercentageOption = true;
			ChartSerieDataFormatImpl format = Format;
			if (format != null)
			{
				format.AttachedLabel.ShowPieInPercents = true;
			}
			SetIndividualDataLabels(value, "Percentage");
		}
	}

	internal bool IsSourceLinked
	{
		get
		{
			m_isSourceLinked = !TextArea.ChartAI.IsCustomNumberFormat;
			return m_isSourceLinked;
		}
		set
		{
			if (value != IsSourceLinked)
			{
				TextArea.ChartAI.IsCustomNumberFormat = !value;
			}
			m_isSourceLinked = value;
		}
	}

	public bool IsBubbleSize
	{
		get
		{
			return m_textArea.IsBubbleSize;
		}
		set
		{
			m_textArea.IsBubbleSize = value;
			m_bHasBubbleSizeOption = true;
			_ = Format;
			SetIndividualDataLabels(value, "BubbleSize");
		}
	}

	public string Delimiter
	{
		get
		{
			return m_textArea.Delimiter;
		}
		set
		{
			m_textArea.Delimiter = value;
		}
	}

	public bool IsLegendKey
	{
		get
		{
			if (!m_bHasLegendKeyOption)
			{
				if (!ParentBook.Saving)
				{
					if (base.Application.DefaultVersion == OfficeVersion.Excel2013)
					{
						return true;
					}
					return m_textArea.IsLegendKey;
				}
				return m_textArea.IsLegendKey;
			}
			return m_textArea.IsLegendKey;
		}
		set
		{
			m_textArea.IsLegendKey = value;
			m_bHasLegendKeyOption = true;
			SetIndividualDataLabels(value, "LegendKey");
		}
	}

	public bool ShowLeaderLines
	{
		get
		{
			if (m_bShowLeaderLines)
			{
				return true;
			}
			if (m_serie != null && m_serie.ParentChart.IsChartPie)
			{
				return m_serie.InnerChart.ChartFormat.ShowLeaderLines;
			}
			if (m_chart != null && m_chart.IsChartPie)
			{
				return m_chart.ChartFormat.ShowLeaderLines;
			}
			return false;
		}
		set
		{
			if (m_serie != null)
			{
				m_serie.SetLeaderLines(value);
			}
			if (m_serie != null && m_serie.ParentChart.IsChartPie)
			{
				m_serie.InnerChart.ChartFormat.ShowLeaderLines = value;
			}
			else if (m_chart != null && m_chart.IsChartPie)
			{
				m_chart.ChartFormat.ShowLeaderLines = value;
			}
			m_bShowLeaderLines = value;
		}
	}

	public OfficeDataLabelPosition Position
	{
		get
		{
			return m_textArea.Position;
		}
		set
		{
			m_textArea.Position = value;
		}
	}

	internal OfficeChartBackgroundMode BackgroundMode
	{
		get
		{
			return m_textArea.BackgroundMode;
		}
		set
		{
			m_textArea.BackgroundMode = value;
		}
	}

	internal bool IsAutoMode
	{
		get
		{
			return m_textArea.IsAutoMode;
		}
		set
		{
			m_textArea.IsAutoMode = value;
		}
	}

	public string Text
	{
		get
		{
			return m_textArea.Text;
		}
		set
		{
			m_textArea.Text = value;
		}
	}

	public IOfficeChartRichTextString RichText => TextArea.RichText;

	public int TextRotationAngle
	{
		get
		{
			return m_textArea.TextRotationAngle;
		}
		set
		{
			m_textArea.TextRotationAngle = value;
			ShowTextProperties = true;
		}
	}

	public IOfficeChartFrameFormat FrameFormat => m_textArea.FrameFormat;

	public bool Bold
	{
		get
		{
			return m_textArea.Bold;
		}
		set
		{
			m_textArea.Bold = value;
			ShowTextProperties = true;
		}
	}

	public OfficeKnownColors Color
	{
		get
		{
			return m_textArea.Color;
		}
		set
		{
			m_textArea.Color = value;
			ShowTextProperties = true;
		}
	}

	internal bool IsHighlightColor
	{
		get
		{
			return m_isHighlightColor;
		}
		set
		{
			m_isHighlightColor = value;
		}
	}

	public Color RGBColor
	{
		get
		{
			return m_textArea.RGBColor;
		}
		set
		{
			m_textArea.RGBColor = value;
			ShowTextProperties = true;
		}
	}

	public bool Italic
	{
		get
		{
			return m_textArea.Italic;
		}
		set
		{
			m_textArea.Italic = value;
			ShowTextProperties = true;
		}
	}

	public bool MacOSOutlineFont
	{
		get
		{
			return m_textArea.MacOSOutlineFont;
		}
		set
		{
			m_textArea.MacOSOutlineFont = value;
			ShowTextProperties = true;
		}
	}

	public bool MacOSShadow
	{
		get
		{
			return m_textArea.MacOSShadow;
		}
		set
		{
			m_textArea.MacOSShadow = value;
		}
	}

	public double Size
	{
		get
		{
			return m_textArea.Size;
		}
		set
		{
			m_textArea.Size = value;
			ShowTextProperties = true;
		}
	}

	public bool Strikethrough
	{
		get
		{
			return m_textArea.Strikethrough;
		}
		set
		{
			m_textArea.Strikethrough = value;
			ShowTextProperties = true;
		}
	}

	public bool Subscript
	{
		get
		{
			return m_textArea.Subscript;
		}
		set
		{
			m_textArea.Subscript = value;
			ShowTextProperties = true;
		}
	}

	public bool Superscript
	{
		get
		{
			return m_textArea.Superscript;
		}
		set
		{
			m_textArea.Superscript = value;
			ShowTextProperties = true;
		}
	}

	public OfficeUnderline Underline
	{
		get
		{
			return m_textArea.Underline;
		}
		set
		{
			m_textArea.Underline = value;
			ShowTextProperties = true;
		}
	}

	public string FontName
	{
		get
		{
			return m_textArea.FontName;
		}
		set
		{
			m_textArea.FontName = value;
			ShowTextProperties = true;
			if (!ParentBook.IsWorkbookOpening)
			{
				m_bFontChanged = true;
			}
		}
	}

	public OfficeFontVerticalAlignment VerticalAlignment
	{
		get
		{
			return m_textArea.VerticalAlignment;
		}
		set
		{
			m_textArea.VerticalAlignment = value;
			ShowTextProperties = true;
		}
	}

	public bool IsAutoColor => m_textArea.IsAutoColor;

	public ChartTextAreaImpl TextArea
	{
		get
		{
			return m_textArea;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			m_textArea = value;
		}
	}

	public ChartSerieDataFormatImpl Format => m_dataPoint.InnerDataFormat;

	public IOfficeChartLayout Layout
	{
		get
		{
			if (m_layout == null)
			{
				m_layout = new ChartLayoutImpl(base.Application, this, base.Parent);
			}
			return m_layout;
		}
		set
		{
			m_layout = value;
		}
	}

	internal bool IsDelete
	{
		get
		{
			return m_isDelete;
		}
		set
		{
			m_isDelete = value;
		}
	}

	public bool HasTextRotation => TextArea.HasTextRotation;

	public ChartParagraphType ParagraphType
	{
		get
		{
			if (m_paraType != ChartParagraphType.CustomDefault)
			{
				ChartParserCommon.CheckDefaultSettings(TextArea);
				m_paraType = TextArea.ParagraphType;
			}
			return m_paraType;
		}
		set
		{
			m_paraType = value;
		}
	}

	public string NumberFormat
	{
		get
		{
			IOfficeChartDataPoint officeChartDataPoint = base.Parent as IOfficeChartDataPoint;
			if (m_numberFormat == null && officeChartDataPoint.IsDefault && TextArea != null)
			{
				m_numberFormat = TextArea.NumberFormat;
			}
			return m_numberFormat;
		}
		set
		{
			m_numberFormat = value;
			IOfficeChartDataPoint obj = base.Parent as IOfficeChartDataPoint;
			if (IsSourceLinked)
			{
				TextArea.ChartAI.IsCustomNumberFormat = true;
			}
			if (obj.IsDefault && TextArea != null)
			{
				TextArea.NumberFormat = value;
			}
		}
	}

	public bool IsFormula
	{
		get
		{
			return m_bIsFormula;
		}
		set
		{
			m_bIsFormula = value;
		}
	}

	internal bool ShowTextProperties
	{
		get
		{
			return m_bShowTextProperties;
		}
		set
		{
			m_bShowTextProperties = value;
		}
	}

	internal bool ShowSizeProperties
	{
		get
		{
			return m_bShowSizeProperties;
		}
		set
		{
			m_bShowSizeProperties = value;
		}
	}

	internal bool ShowBoldProperties
	{
		get
		{
			return m_bShowBoldProperties;
		}
		set
		{
			m_bShowBoldProperties = value;
		}
	}

	internal bool ShowItalicProperties
	{
		get
		{
			return m_bShowItalicProperties;
		}
		set
		{
			m_bShowItalicProperties = value;
		}
	}

	public Excel2007TextRotation TextRotation
	{
		get
		{
			return m_textArea.TextRotation;
		}
		set
		{
			m_textArea.TextRotation = value;
		}
	}

	public bool IsCreated
	{
		get
		{
			return m_isCreated;
		}
		set
		{
			m_isCreated = value;
		}
	}

	internal bool CheckSerieIsPie
	{
		get
		{
			if (m_serie != null)
			{
				return ChartImpl.GetIsChartPie(m_serie.SerieType);
			}
			return false;
		}
	}

	private WorkbookImpl ParentBook
	{
		get
		{
			if (m_chart == null)
			{
				return m_serie.ParentBook;
			}
			return m_chart.ParentWorkbook;
		}
	}

	internal string[] StringCache
	{
		get
		{
			return m_stringCache;
		}
		set
		{
			m_stringCache = value;
		}
	}

	internal ChartSerieImpl Serie
	{
		get
		{
			return m_serie;
		}
		set
		{
			m_serie = value;
		}
	}

	public ChartColor ColorObject => m_textArea.ColorObject;

	public int Index => m_textArea.Index;

	public FontImpl Font => m_textArea.Font;

	internal bool IsFontChanged
	{
		get
		{
			return m_bFontChanged;
		}
		set
		{
			m_bFontChanged = value;
		}
	}

	public ChartDataLabelsImpl(IApplication application, object parent, int index)
		: base(application, parent)
	{
		if (((parent as ChartDataPointImpl).Parent as ChartDataPointsCollection).Parent is ChartImpl)
		{
			SetParents(isChart: true);
		}
		else
		{
			SetParents(isChart: false);
		}
		m_textArea = new ChartWrappedTextAreaImpl(base.Application, this);
		if (((ChartImpl)(parent as ChartDataPointImpl).FindParent(typeof(ChartImpl))).ChartType == OfficeChartType.WaterFall)
		{
			Position = OfficeDataLabelPosition.Outside;
		}
		m_textArea.ObjectLink.DataPointNumber = (ushort)index;
		m_textArea.TextRecord.IsAutoText = true;
		m_textArea.ChartAI.Reference = ChartAIRecord.ReferenceType.EnteredDirectly;
		m_paraType = ChartParagraphType.Default;
		_ = m_dataPoint.InnerDataFormat;
	}

	private void SetParents(bool isChart)
	{
		object[] array = new object[2];
		if (isChart)
		{
			array = FindParents(new Type[2]
			{
				typeof(ChartImpl),
				typeof(ChartDataPointImpl)
			});
			m_chart = array[0] as ChartImpl;
			m_serie = null;
		}
		else
		{
			array = FindParents(new Type[2]
			{
				typeof(ChartSerieImpl),
				typeof(ChartDataPointImpl)
			});
			m_serie = array[0] as ChartSerieImpl;
			m_chart = null;
		}
		if (isChart)
		{
			if (m_chart == null)
			{
				throw new ArgumentNullException("parent", "Can't find parent chart.");
			}
		}
		else if (m_serie == null)
		{
			throw new ArgumentNullException("parent", "Can't find parent serie.");
		}
		m_dataPoint = array[1] as ChartDataPointImpl;
		if (m_dataPoint == null)
		{
			throw new ArgumentNullException("parent", "Can't find data point.");
		}
	}

	private void SetIndividualDataLabels(bool value, string dataLabelName)
	{
		if (!(base.Parent is ChartDataPointImpl { IsDefault: not false, Parent: ChartDataPointsCollection { IsLoading: false, m_hashDataPoints: { } hashDataPoints } }))
		{
			return;
		}
		foreach (ChartDataPointImpl value2 in hashDataPoints.Values)
		{
			if (value2.HasDataLabels)
			{
				IOfficeChartDataLabels dataLabels = value2.DataLabels;
				switch (dataLabelName)
				{
				case "SeriesName":
					dataLabels.IsSeriesName = value;
					break;
				case "CategoryName":
					dataLabels.IsCategoryName = value;
					break;
				case "Value":
					dataLabels.IsValue = value;
					break;
				case "LegendKey":
					dataLabels.IsLegendKey = value;
					break;
				case "Percentage":
					dataLabels.IsPercentage = value;
					break;
				case "BubbleSize":
					dataLabels.IsBubbleSize = value;
					break;
				}
			}
		}
	}

	public Font GenerateNativeFont()
	{
		return m_textArea.GenerateNativeFont();
	}

	[CLSCompliant(false)]
	public void Serialize(IList<IBiffStorage> records)
	{
		if (records == null)
		{
			throw new ArgumentNullException("records");
		}
		if (m_textArea.ContainDataLabels)
		{
			m_textArea.IsShowLabelPercent = m_serie != null && m_serie.IsPie && IsPercentage && IsCategoryName && !IsValue && !IsSeriesName;
		}
		SetObjectLink();
		int num;
		if (!IsValue)
		{
			num = (IsCategoryName ? 1 : 0);
			if (num != 0)
			{
				m_textArea.TextRecord.IsShowLabel = true;
			}
		}
		else
		{
			num = 0;
		}
		m_textArea.Serialize(records);
		if (num != 0)
		{
			m_textArea.TextRecord.IsShowLabel = false;
		}
	}

	private void SetObjectLink()
	{
		ChartObjectLinkRecord objectLink = m_textArea.ObjectLink;
		objectLink.DataPointNumber = (ushort)m_dataPoint.Index;
		objectLink.LinkObject = ExcelObjectTextLink.DataLabel;
		if (!(FindParent(typeof(ChartSerieImpl)) is ChartSerieImpl chartSerieImpl))
		{
			throw new NotImplementedException("Can't find parent series");
		}
		objectLink.SeriesNumber = (ushort)chartSerieImpl.Index;
	}

	internal void SetNumberFormat(string value)
	{
		m_numberFormat = value;
		if ((base.Parent as IOfficeChartDataPoint).IsDefault && TextArea != null)
		{
			TextArea.NumberFormat = value;
		}
	}

	internal void UpdateDataLabelText(ChartDataLabelsImpl dataLabelsImpl, bool isValueFromCells)
	{
		if (dataLabelsImpl.Text == null)
		{
			return;
		}
		string[] array = dataLabelsImpl.Text.Split(',');
		_ = dataLabelsImpl.Text;
		bool flag = false;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].Contains("[CELLRANGE]") || array[i].Contains("[SERIES NAME]") || array[i].Contains("[CATEGORY NAME]") || array[i].Contains("[VALUE]") || array[i].Contains("[PERCENTAGE]") || array[i].Contains("[X VALUE]") || array[i].Contains("[Y VALUE]"))
			{
				flag = true;
				continue;
			}
			flag = false;
			break;
		}
		if (flag)
		{
			dataLabelsImpl.Text = null;
		}
	}

	public void UpdateSerieIndex()
	{
		if (m_serie != null)
		{
			m_textArea.UpdateSerieIndex(m_serie.Index);
		}
	}

	public object Clone(object parent, Dictionary<int, int> dicFontIndexes, Dictionary<string, string> dicNewSheetNames)
	{
		ChartDataLabelsImpl chartDataLabelsImpl = (ChartDataLabelsImpl)MemberwiseClone();
		chartDataLabelsImpl.SetParent(parent);
		if (((parent as ChartDataPointImpl).Parent as ChartDataPointsCollection).Parent is ChartImpl)
		{
			chartDataLabelsImpl.SetParents(isChart: true);
		}
		else
		{
			chartDataLabelsImpl.SetParents(isChart: false);
		}
		chartDataLabelsImpl.m_paraType = m_paraType;
		if (m_layout != null)
		{
			chartDataLabelsImpl.m_layout = m_layout;
		}
		chartDataLabelsImpl.m_textArea = (ChartTextAreaImpl)m_textArea.Clone(chartDataLabelsImpl, dicFontIndexes, dicNewSheetNames);
		return chartDataLabelsImpl;
	}

	public void BeginUpdate()
	{
		m_textArea.BeginUpdate();
	}

	public void EndUpdate()
	{
		m_textArea.EndUpdate();
	}
}
