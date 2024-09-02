using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DocGen.Chart;
using DocGen.Chart.Drawing;
using DocGen.Drawing;
using DocGen.Drawing.SkiaSharpHelper;
using DocGen.OfficeChart;
using DocGen.OfficeChart.Implementation;
using DocGen.OfficeChart.Implementation.Charts;
using DocGen.OfficeChart.Implementation.Collections;
using DocGen.OfficeChart.Implementation.Shapes;
using DocGen.OfficeChart.Parser.Biff_Records;

namespace DocGen.ChartToImageConverter;

internal class ChartUtilities
{
	internal const string ExcelNumberFormat = "General";

	internal const string DefaultFont = "Calibri";

	internal int AxisTitleRotationAngle = 90;

	internal bool SecondayAxisAchived;

	internal bool IsChart3D;

	internal bool IsChartEx;

	internal string FontName;

	internal int ChartWidth;

	internal int ChartHeight;

	private Regex _percentagePattern = new Regex("(^(0)(\\.[0]+)*)%$");

	internal const float DEF_BORDER_WIDTH_EXCEL = 12700f;

	internal WorkbookImpl parentWorkbook;

	internal int[] m_def_MarkerType_Array = new int[9] { 2, 1, 3, 4, 5, 8, 9, 6, 7 };

	internal const float DEF_LINE_THICKNESS_XML = 2f;

	internal const float DEF_LINE_THICKNESS_BINARY = 1f;

	private ChartControl m_sfChart;

	private IList<ChartPointInternal> m_itemSource;

	private bool m_hasNewSeries;

	private bool m_isSeriesReverseOrder;

	private int m_newSeriesIndex;

	private List<string> _fontList = new List<string>();

	private readonly List<string> _excludefontList = new List<string>();

	private readonly string[] _latinfontList = new string[6] { "Arial", "Microsoft Sans Serif", "Segoe UI", "Tahoma", "Times New Roman", "Courier New" };

	private int[] _unicodeChar = new int[2] { 34, 183 };

	private char[] _numberFormatChar = new char[1] { '€' };

	internal ExcelEngine engine;

	internal bool m_isSeriesSorted = true;

	internal bool m_isForImage;

	internal Dictionary<ChartAxis, Tuple<string, double, bool, bool>> m_TextToMeasure;

	internal ChartDateTimeIntervalType DateTimeIntervalType;

	internal bool IsLegendManualLayout { get; set; }

	internal Dictionary<int, IList<ChartPointInternal>> ListOfPoints { get; set; }

	internal IWorksheet Worksheet
	{
		get
		{
			if (engine == null)
			{
				engine = new ExcelEngine();
				engine.Excel.Workbooks.Create(1);
			}
			return engine.Excel.Workbooks[0].Worksheets[0];
		}
	}

	internal ChartControl SfChart
	{
		get
		{
			return m_sfChart;
		}
		set
		{
			m_sfChart = value;
		}
	}

	internal IList<ChartPointInternal> ItemSource
	{
		get
		{
			return m_itemSource;
		}
		set
		{
			m_itemSource = value;
		}
	}

	internal bool HasNewSeries
	{
		get
		{
			return m_hasNewSeries;
		}
		set
		{
			m_hasNewSeries = value;
		}
	}

	internal int NewseriesIndex
	{
		get
		{
			return m_newSeriesIndex;
		}
		set
		{
			m_newSeriesIndex = value;
		}
	}

	internal bool IsSeriesReverseOrder
	{
		get
		{
			return m_isSeriesReverseOrder;
		}
		set
		{
			m_isSeriesReverseOrder = value;
		}
	}

	internal bool IsSeriesSorted
	{
		get
		{
			return m_isSeriesSorted;
		}
		set
		{
			m_isSeriesSorted = value;
		}
	}

	protected ChartUtilities()
	{
	}

	internal void IntializeFonts()
	{
		_fontList.Add("Verdana");
		_fontList.Add("Times New Roman");
		_fontList.Add("Microsoft Sans Serif");
		_fontList.Add("Tahoma");
		_fontList.Add("Arial");
		_fontList.Add("SimSun");
		_fontList.Add("MingLiU");
		_fontList.Add("Calibri");
		_excludefontList.Add("Arial");
		_excludefontList.Add("Arial Unicode MS");
		_excludefontList.Add("Microsoft Sans Serif");
		_excludefontList.Add("Segoe UI");
		_excludefontList.Add("Tahoma");
		_excludefontList.Add("Times New Roman");
		_excludefontList.Add("Calibri");
	}

	internal void SetChartSize(IOfficeChart _chart)
	{
		if (!(_chart is ChartShapeImpl))
		{
			ChartWidth = (int)(_chart.Width * 96.0 / 72.0);
			ChartHeight = (int)(_chart.Height * 96.0 / 72.0);
		}
		else
		{
			ChartWidth = (_chart as ChartShapeImpl).Width;
			ChartHeight = (_chart as ChartShapeImpl).Height;
		}
		if (ChartHeight == 0)
		{
			ChartHeight = ((_chart is ChartShapeImpl) ? 288 : 660);
		}
		if (ChartWidth == 0)
		{
			ChartWidth = ((_chart is ChartShapeImpl) ? 480 : 910);
		}
	}

	internal string ApplyNumberFormat(object value, string numberFormat)
	{
		RangeImpl obj = Worksheet["A1"] as RangeImpl;
		obj.Value2 = value;
		obj.NumberFormat = numberFormat;
		return obj.DisplayText;
	}

	private bool IsCategoryAxisAuto(ChartAxisImpl axis)
	{
		int axisId = axis.AxisId;
		ChartCategoryAxisImpl chartCategoryAxisImpl = axis.ParentChart.PrimaryCategoryAxis as ChartCategoryAxisImpl;
		ChartCategoryAxisImpl chartCategoryAxisImpl2 = (axis.ParentChart.IsSecondaryValueAxisAvail ? (axis.ParentChart.SecondaryCategoryAxis as ChartCategoryAxisImpl) : null);
		if (axisId == chartCategoryAxisImpl.AxisId)
		{
			if (chartCategoryAxisImpl.CategoryLabels != null)
			{
				return false;
			}
		}
		else
		{
			if (chartCategoryAxisImpl2 == null || axisId != chartCategoryAxisImpl2.AxisId)
			{
				return false;
			}
			if (chartCategoryAxisImpl2.CategoryLabels != null)
			{
				return false;
			}
		}
		return true;
	}

	private bool IsStacked100AxisFormat(ChartAxisImpl axis)
	{
		ChartFormatImpl chartFormatImpl = ((IList<ChartFormatImpl>)(axis.Parent as ChartParentAxisImpl).ChartFormats)[0];
		if ((chartFormatImpl.FormatRecordType == TBIFFRecord.ChartArea && chartFormatImpl.IsCategoryBrokenDown) || (chartFormatImpl.FormatRecordType == TBIFFRecord.ChartBar && chartFormatImpl.ShowAsPercentsBar) || (chartFormatImpl.FormatRecordType == TBIFFRecord.ChartLine && chartFormatImpl.ShowAsPercentsLine))
		{
			return true;
		}
		return false;
	}

	private string GetSourceNumberFormat(ChartValueAxisImpl axis)
	{
		string result = "General";
		IEnumerable<IOfficeChartSerie> enumerable = axis.ParentChart.Series.Where((IOfficeChartSerie item) => !item.IsFiltered && item.UsePrimaryAxis == axis.IsPrimary);
		if (enumerable != null && enumerable.Count() > 0)
		{
			ChartSerieImpl firstSerie = enumerable.First() as ChartSerieImpl;
			if (axis is ChartCategoryAxisImpl)
			{
				IRange categoryLabelsIRange = firstSerie.CategoryLabelsIRange;
				if (CheckIfValidValueRange(categoryLabelsIRange))
				{
					result = TryAndGetFirstCellNumberFormat(categoryLabelsIRange);
				}
				else if (firstSerie.CategoriesFormatCode != null && firstSerie.CategoriesFormatCode != "General")
				{
					string text = (enumerable.All((IOfficeChartSerie x) => (x as ChartSerieImpl).CategoriesFormatCode == firstSerie.CategoriesFormatCode) ? firstSerie.CategoriesFormatCode : null);
					if (text != null)
					{
						result = text;
					}
				}
			}
			else
			{
				IRange valuesIRange = firstSerie.ValuesIRange;
				if (CheckIfValidValueRange(valuesIRange))
				{
					result = TryAndGetFirstCellNumberFormat(valuesIRange);
				}
				else if (firstSerie.FormatCode != null && firstSerie.FormatCode != "General")
				{
					string text2 = (enumerable.All((IOfficeChartSerie x) => (x as ChartSerieImpl).FormatCode == firstSerie.FormatCode) ? firstSerie.FormatCode : null);
					if (text2 != null)
					{
						result = text2;
					}
				}
			}
		}
		return result;
	}

	private string TryAndGetFirstCellNumberFormat(IRange range)
	{
		string numberFormat = range.Cells[0].NumberFormat;
		WorksheetImpl workSheet = range.Worksheet as WorksheetImpl;
		if (IsRowOrColumnIsHidden(range.Cells[0], workSheet))
		{
			for (int i = 1; i < range.Cells.Length; i++)
			{
				if (!IsRowOrColumnIsHidden(range.Cells[i], workSheet))
				{
					numberFormat = range.Cells[i].NumberFormat;
					break;
				}
			}
		}
		return numberFormat;
	}

	protected internal bool CheckIfValidValueRange(IRange range)
	{
		if (range == null)
		{
			return false;
		}
		if (range is ExternalRange)
		{
			return false;
		}
		if (range is RangesCollection)
		{
			if ((range as RangesCollection).Any((IRange x) => x is ExternalRange))
			{
				return false;
			}
		}
		else
		{
			NameImpl nameImpl = ((range != null) ? (range as NameImpl) : null);
			if (nameImpl != null && (nameImpl.RefersToRange == null || nameImpl.RefersToRange is ExternalRange))
			{
				return false;
			}
		}
		return true;
	}

	protected bool IsRowOrColumnIsHidden(IRange valueCell, WorksheetImpl workSheet)
	{
		return false;
	}

	protected double GetDisplayUnitValue(ChartValueAxisImpl valueAxis)
	{
		if (valueAxis.DisplayUnit == OfficeChartDisplayUnit.Custom)
		{
			return valueAxis.DisplayUnitCustom;
		}
		return (int)valueAxis.DisplayUnit switch
		{
			1 => 100.0, 
			2 => 1000.0, 
			3 => 10000.0, 
			4 => 100000.0, 
			5 => 1000000.0, 
			6 => 10000000.0, 
			7 => 100000000.0, 
			8 => 1000000000.0, 
			9 => 1000000000000.0, 
			0 => 1.0, 
			_ => 1.0, 
		};
	}

	private int GetAxisLayoutTransformForTitle(ChartAxisImpl axis, out bool isVertical)
	{
		isVertical = false;
		bool flag = axis.ParentChart.ChartType.ToString().ToLower().Contains("bar");
		ChartTextAreaImpl chartTextAreaImpl = axis.TitleArea as ChartTextAreaImpl;
		bool num;
		if (!axis.AxisPosition.HasValue)
		{
			if (!flag)
			{
				if (!(axis is ChartValueAxisImpl))
				{
					goto IL_00cc;
				}
				num = IsVerticalAxis(axis);
			}
			else
			{
				num = axis is ChartCategoryAxisImpl;
			}
		}
		else
		{
			if (axis.AxisPosition == ChartAxisPos.l)
			{
				goto IL_00ab;
			}
			num = axis.AxisPosition == ChartAxisPos.r;
		}
		if (num)
		{
			goto IL_00ab;
		}
		goto IL_00cc;
		IL_00cc:
		return axis.TitleArea.TextRotationAngle;
		IL_00ab:
		isVertical = true;
		int num2 = (chartTextAreaImpl.HasTextRotation ? chartTextAreaImpl.TextRotationAngle : (-90));
		return AxisTitleRotationAngle + num2;
	}

	private bool IsVerticalAxis(ChartAxisImpl axis)
	{
		ChartValueAxisImpl chartValueAxisImpl = axis.ParentChart.PrimaryValueAxis as ChartValueAxisImpl;
		ChartValueAxisImpl chartValueAxisImpl2 = (axis.ParentChart.IsSecondaryValueAxisAvail ? (axis.ParentChart.SecondaryValueAxis as ChartValueAxisImpl) : null);
		if (axis.AxisId == chartValueAxisImpl.AxisId || (chartValueAxisImpl2 != null && chartValueAxisImpl2.AxisId == axis.AxisId))
		{
			return true;
		}
		return false;
	}

	private bool IsVerticalCategoryAxis(ChartAxisImpl axis)
	{
		ChartFormatImpl chartFormatImpl = ((IList<ChartFormatImpl>)(axis.Parent as ChartParentAxisImpl).ChartFormats)[0];
		if ((chartFormatImpl.FormatRecordType == TBIFFRecord.ChartBar && chartFormatImpl.IsHorizontalBar) || axis.ParentChart.ChartType.ToString().Contains("Funnel"))
		{
			return true;
		}
		return false;
	}

	internal bool IsVaryColorSupported(ChartSerieImpl serie)
	{
		if (serie.GetCommonSerieFormat().IsVaryColor && serie.ParentChart.Series.Count == 1)
		{
			return true;
		}
		return false;
	}

	internal Color SfColor(byte R, byte G, byte B, double transparency)
	{
		return Color.FromArgb((byte)(255.0 - transparency * 255.0), R, G, B);
	}

	internal Color SfColor(byte R, byte G, byte B)
	{
		return Color.FromArgb(255, R, G, B);
	}

	internal Color SfColor(Color chartcolor)
	{
		return SfColor(chartcolor, 0.0);
	}

	internal Color SfColor(Color chartcolor, double transparency)
	{
		return Color.FromArgb((byte)(255.0 - transparency * 255.0), chartcolor.R, chartcolor.G, chartcolor.B);
	}

	internal bool IsLine(OfficeChartType chartType)
	{
		if ((uint)(chartType - 13) <= 6u)
		{
			return true;
		}
		return false;
	}

	internal string GetSerieName(ChartSerieImpl serie)
	{
		if (serie.ParentChart.SeriesNameLevel == OfficeSeriesNameLevel.SeriesNameLevelAll)
		{
			return serie.Name;
		}
		return "Series" + (serie.Index + 1);
	}

	internal float GetBorderThickness(ChartBorderImpl border)
	{
		float result = 0f;
		if ((!parentWorkbook.IsCreated || !border.AutoFormat) && (short)border.LinePattern != 5)
		{
			result = ((border.LineWeightString != null) ? ((!(border.LineWeightString != "0")) ? 0.5f : ((float)int.Parse(border.LineWeightString) / 12700f)) : (((short)border.LineWeight != -1) ? ((float)((short)border.LineWeight + 1)) : 0.75f));
		}
		return result;
	}

	internal bool TryAndGetColorBasedOnElement(ChartElementsEnum inputelement, ChartImpl chart, out Color color)
	{
		int num = chart.Style;
		color = Color.FromArgb(0, 0, 0, 0);
		if (num > 100)
		{
			num -= 100;
		}
		if (num <= 0 || num > 48)
		{
			return false;
		}
		Color mixColor = Color.FromArgb(0, 255, 255, 255);
		Color color2 = Color.FromArgb(0, 0, 0, 0);
		switch (inputelement)
		{
		case ChartElementsEnum.AxisLine:
		case ChartElementsEnum.MajorGridLines:
			color = Color.FromArgb(0, 134, 134, 134);
			break;
		case ChartElementsEnum.MinorGridLines:
			if (num < 41)
			{
				color = Color.FromArgb(0, 134, 134, 134);
				color = GetPercentageTintOrShadeOfColor(color, 0.5, mixColor);
			}
			else
			{
				color = Color.FromArgb(0, 70, 70, 70);
				color = GetPercentageTintOrShadeOfColor(color, 0.9, mixColor);
			}
			break;
		case ChartElementsEnum.ChartAreaLine:
		case ChartElementsEnum.FloorLine:
			if (num < 33)
			{
				color = Color.FromArgb(0, 134, 134, 134);
			}
			else if (num >= 33 && num < 41)
			{
				color = Color.FromArgb(0, 134, 134, 134);
			}
			else
			{
				color = Color.FromArgb(0, 255, 255, 255);
			}
			break;
		case ChartElementsEnum.ChartAreaFill:
			if (num < 33)
			{
				color = Color.FromArgb(0, 255, 255, 255);
			}
			else if (num >= 33 && num < 41)
			{
				color = Color.FromArgb(0, 255, 255, 255);
			}
			else
			{
				color = color2;
			}
			break;
		case ChartElementsEnum.FloorFill:
		case ChartElementsEnum.WallsFill:
		case ChartElementsEnum.PlotAreaFill:
			if (num < 33)
			{
				color = Color.FromArgb(0, 255, 255, 255);
				break;
			}
			switch (num)
			{
			case 33:
			case 34:
				color = Color.FromArgb(0, 134, 134, 134);
				color = GetPercentageTintOrShadeOfColor(color, 0.2, mixColor);
				break;
			case 35:
			case 36:
			case 37:
			case 38:
			case 39:
			case 40:
			{
				int num2 = num - 35;
				num2 = 4 + num2;
				color = chart.GetChartThemeColorByColorIndex(num2);
				color = GetPercentageTintOrShadeOfColor(color, 0.2, mixColor);
				break;
			}
			default:
				color = Color.FromArgb(0, 70, 70, 70);
				color = GetPercentageTintOrShadeOfColor(color, 0.95, mixColor);
				break;
			}
			break;
		case ChartElementsEnum.UpBarLine:
		case ChartElementsEnum.DownBarLine:
		{
			if (num < 17)
			{
				color = chart.GetChartThemeColorByColorIndex(1);
				break;
			}
			if ((num >= 17 && num < 33) || (num >= 41 && num <= 48))
			{
				return false;
			}
			if (num == 33 || num == 34)
			{
				color = chart.GetChartThemeColorByColorIndex(1);
				break;
			}
			int num5 = num - 35;
			num5 = 4 + num5;
			color = chart.GetChartThemeColorByColorIndex(num5);
			color = GetPercentageTintOrShadeOfColor(color, 0.5, color2);
			break;
		}
		case ChartElementsEnum.UpBarFill:
		{
			if (num == 2 || num == 10 || num == 18 || num == 26)
			{
				color = Color.FromArgb(0, 70, 70, 70);
				color = GetPercentageTintOrShadeOfColor(color, 0.05, mixColor);
				break;
			}
			if (num == 1 || num == 9 || num == 17 || num == 25 || num == 41)
			{
				color = Color.FromArgb(0, 134, 134, 134);
				color = GetPercentageTintOrShadeOfColor(color, 0.25, mixColor);
				break;
			}
			if ((num >= 33 && num <= 40) || num == 42)
			{
				color = Color.FromArgb(0, 255, 255, 255);
				break;
			}
			int num4 = num;
			num4 = ((num4 < 9) ? (num4 - 3) : ((num4 < 17) ? (num4 - 11) : ((num4 < 25) ? (num4 - 19) : ((num4 >= 33) ? (num4 - 43) : (num4 - 27)))));
			num4 = 4 + num4;
			color = chart.GetChartThemeColorByColorIndex(num4);
			color = GetPercentageTintOrShadeOfColor(color, 0.25, mixColor);
			break;
		}
		case ChartElementsEnum.DownBarFill:
			switch (num)
			{
			case 42:
				color = color2;
				break;
			case 2:
			case 10:
			case 18:
			case 26:
			case 34:
				color = Color.FromArgb(0, 56, 56, 56);
				color = GetPercentageTintOrShadeOfColor(color, 0.95, mixColor);
				break;
			case 1:
			case 9:
			case 17:
			case 25:
			case 33:
			case 41:
				color = Color.FromArgb(0, 70, 70, 70);
				color = GetPercentageTintOrShadeOfColor(color, 0.85, mixColor);
				break;
			default:
			{
				int num3 = num;
				num3 = ((num3 < 9) ? (num3 - 3) : ((num3 < 17) ? (num3 - 11) : ((num3 < 25) ? (num3 - 19) : ((num3 < 33) ? (num3 - 27) : ((num3 >= 41) ? (num3 - 43) : (num3 - 35))))));
				num3 = 4 + num3;
				color = chart.GetChartThemeColorByColorIndex(num3);
				color = GetPercentageTintOrShadeOfColor(color, 0.5, color2);
				break;
			}
			}
			break;
		case ChartElementsEnum.OtherLines:
			if (num < 33)
			{
				color = color2;
				break;
			}
			switch (num)
			{
			case 33:
			case 34:
				color = color2;
				break;
			case 35:
			case 36:
			case 37:
			case 38:
			case 39:
			case 40:
				color = color2;
				color = GetPercentageTintOrShadeOfColor(color, 0.25, color2);
				break;
			default:
				color = Color.FromArgb(0, 255, 255, 255);
				break;
			}
			break;
		}
		return true;
	}

	internal bool TryAndGetThicknessBasedOnElement(ChartElementsEnum inputelement, ChartImpl chart, out float thickness, short? value)
	{
		thickness = ((parentWorkbook.DefaultThemeVersion == "124226") ? 0.75f : 0.5f);
		int num = chart.Style;
		if (num > 100)
		{
			num -= 100;
		}
		if (num <= 0 || num > 48)
		{
			return false;
		}
		switch (inputelement)
		{
		case ChartElementsEnum.DataPointLineThickness:
			if (num <= 8)
			{
				thickness *= 3f;
			}
			else if (num >= 25 && num <= 32)
			{
				thickness *= 7f;
			}
			else
			{
				thickness *= 5f;
			}
			thickness = (float)ApplicationImpl.ConvertUnitsStatic(thickness, MeasureUnits.Point, MeasureUnits.Pixel);
			break;
		case ChartElementsEnum.MarkerThickness:
			if (!value.HasValue)
			{
				if (parentWorkbook.DefaultThemeVersion == "124226")
				{
					if (num <= 8)
					{
						thickness = 7f;
					}
					else if (num >= 25 && num <= 32)
					{
						thickness = 13f;
					}
					else
					{
						thickness = 9f;
					}
				}
				else if (num <= 8)
				{
					thickness = 5f;
				}
				else if (num >= 25 && num <= 32)
				{
					thickness = 9f;
				}
				else
				{
					thickness = 7f;
				}
			}
			else
			{
				thickness = 5f;
				thickness += value.Value * 2;
			}
			break;
		}
		return true;
	}

	private Color GetPercentageTintOrShadeOfColor(Color inputColor, double percentValue, Color mixColor)
	{
		byte red = (byte)Math.Truncate((double)(int)inputColor.R * percentValue + (double)(int)mixColor.R * (1.0 - percentValue));
		byte green = (byte)Math.Truncate((double)(int)inputColor.G * percentValue + (double)(int)mixColor.G * (1.0 - percentValue));
		byte blue = (byte)Math.Truncate((double)(int)inputColor.B * percentValue + (double)(int)mixColor.B * (1.0 - percentValue));
		return Color.FromArgb(0, red, green, blue);
	}

	internal bool TryAndGetFillOrLineColorBasedOnPattern(ChartImpl chart, bool isLine, int formattingIndex, int highFormattingIndex, out Color color)
	{
		int num = chart.Style;
		color = Color.FromArgb(0, 0, 0, 0);
		if (IsChartEx)
		{
			return false;
		}
		if (num > 100)
		{
			num -= 100;
		}
		if (num <= 0 || num > 48)
		{
			return false;
		}
		Color color2 = Color.FromArgb(0, 255, 255, 255);
		Color color3 = Color.FromArgb(0, 70, 70, 70);
		int[] array = new int[12]
		{
			1, 9, 17, 25, 33, 2, 10, 18, 26, 34,
			42, 41
		};
		if (isLine ? (num != 34) : (Array.IndexOf(array, num) == -1))
		{
			if (isLine)
			{
				if (num >= 9 && num <= 16)
				{
					color = Color.FromArgb(0, 255, 255, 255);
				}
				else
				{
					switch (num)
					{
					case 33:
						color = GetPercentageTintOrShadeOfColor(color, 0.5, color3);
						break;
					case 35:
					case 36:
					case 37:
					case 38:
					case 39:
					case 40:
					{
						int num2 = num - 35;
						num2 = 4 + num2;
						color = chart.GetChartThemeColorByColorIndex(num2);
						break;
					}
					default:
						return false;
					}
				}
			}
			else if ((num >= 3 && num <= 8) || (num >= 11 && num <= 16) || (num >= 19 && num <= 24) || (num >= 27 && num <= 32) || (num >= 35 && num <= 40) || (num >= 43 && num <= 48))
			{
				int num3 = num;
				num3 = ((num3 < 9) ? (num3 - 3) : ((num3 < 17) ? (num3 - 11) : ((num3 < 25) ? (num3 - 19) : ((num3 < 33) ? (num3 - 27) : ((num3 >= 41) ? (num3 - 43) : (num3 - 35))))));
				num3 = 4 + num3;
				color = chart.GetChartThemeColorByColorIndex(num3);
				double num4 = 0.0;
				if (highFormattingIndex != 0)
				{
					num4 = -35.0 + 70.0 * ((double)formattingIndex / (double)(highFormattingIndex + 1));
				}
				num4 /= 100.0;
				if (num4 != 0.0)
				{
					color = GetPercentageTintOrShadeOfColor(color, 1.0 - ((num4 < 0.0) ? (-1.0 * num4) : num4), (num4 < 0.0) ? color3 : color2);
				}
			}
		}
		else
		{
			int num5;
			if (isLine)
			{
				num5 = 3;
			}
			else
			{
				int num6 = Array.IndexOf(array, num);
				if (num6 <= 4)
				{
					num5 = 1;
				}
				else
				{
					if (num6 != 11)
					{
						return false;
					}
					num5 = 4;
				}
			}
			double percentValue = 1.0;
			double num7 = 0.0;
			if (highFormattingIndex > 5)
			{
				num7 = -35.0 + 70.0 * ((double)(formattingIndex / 6) / (double)((highFormattingIndex + 1) / 6));
				num7 /= 100.0;
			}
			switch (num5)
			{
			case 1:
				color = Color.FromArgb(0, 70, 70, 70);
				switch (formattingIndex % 6)
				{
				case 0:
					percentValue = 0.885;
					break;
				case 1:
					percentValue = 0.55;
					break;
				case 2:
					percentValue = 0.78;
					break;
				case 3:
					percentValue = 0.925;
					break;
				case 4:
					percentValue = 0.7;
					break;
				case 5:
					percentValue = 0.3;
					break;
				}
				color = GetPercentageTintOrShadeOfColor(color, percentValue, color2);
				break;
			case 2:
			case 3:
			{
				int color_Index = 4 + formattingIndex % 6;
				color = chart.GetChartThemeColorByColorIndex(color_Index);
				if (num5 == 3)
				{
					color = GetPercentageTintOrShadeOfColor(color, 0.5, color3);
				}
				break;
			}
			case 4:
				color = Color.FromArgb(0, 70, 70, 70);
				switch (formattingIndex % 6)
				{
				case 1:
					percentValue = 0.5;
					break;
				case 2:
					percentValue = 0.55;
					break;
				case 3:
					percentValue = 0.78;
					break;
				case 4:
					percentValue = 0.15;
					break;
				case 5:
					percentValue = 0.7;
					break;
				case 6:
					percentValue = 0.3;
					break;
				}
				color = GetPercentageTintOrShadeOfColor(color, percentValue, color2);
				break;
			}
			if (num7 != 0.0)
			{
				color = GetPercentageTintOrShadeOfColor(color, 1.0 - ((num7 < 0.0) ? (num7 * -1.0) : num7), (num7 < 0.0) ? color3 : color2);
			}
		}
		return true;
	}

	private BrushInfo GetWallBrush(ChartImpl chart, ChartWallOrFloorImpl wall, bool isFloor)
	{
		if (!wall.IsAutomaticFormat && wall.Interior.Pattern != 0)
		{
			return GetBrushFromDataFormat(wall);
		}
		if (isFloor && parentWorkbook.Version == OfficeVersion.Excel97to2003)
		{
			return new BrushInfo(SfColor(192, 192, 192));
		}
		if (TryAndGetColorBasedOnElement(isFloor ? ChartElementsEnum.FloorFill : ChartElementsEnum.WallsFill, chart, out var color))
		{
			return new BrushInfo(SfColor(color, ((chart.Style > 100) ? (chart.Style < 133) : (chart.Style < 33)) ? 1.0 : 0.0));
		}
		return new BrushInfo(SfColor(byte.MaxValue, byte.MaxValue, byte.MaxValue, (parentWorkbook.Version == OfficeVersion.Excel97to2003) ? 0.0 : 1.0));
	}

	protected BrushInfo GetBrushFromBorder(ChartBorderImpl border)
	{
		Color empty = Color.Empty;
		empty = ((border.Fill == null || (short)border.Fill.FillType != 7) ? border.LineColor : border.Fill.ForeColor);
		return new BrushInfo(SfColor(empty, border.Transparency));
	}

	protected Color GetLineColorFromBorder(ChartBorderImpl border)
	{
		Color empty = Color.Empty;
		empty = ((border.Fill == null || (short)border.Fill.FillType != 7) ? border.LineColor : border.Fill.ForeColor);
		return SfColor(empty, border.Transparency);
	}

	internal BrushInfo GetBrushFromDataFormat(IOfficeChartFillBorder fillFormat)
	{
		Color foreColor = fillFormat.Fill.ForeColor;
		short num = (short)fillFormat.Fill.FillType;
		BrushInfo result = null;
		switch (num)
		{
		case 0:
			result = new BrushInfo(SfColor(foreColor, fillFormat.Fill.Transparency));
			break;
		case 1:
			result = (((short)fillFormat.Fill.Pattern != 0 || (short)fillFormat.Interior.Pattern != 0) ? new BrushInfo(SfColor(foreColor)) : new BrushInfo(SfColor(foreColor, 1.0)));
			break;
		case 7:
		{
			ChartFillImpl chartFillImpl = fillFormat.Fill as ChartFillImpl;
			if (chartFillImpl != null && chartFillImpl.PreservedGradient != null)
			{
				GradientStopImpl maxGradientStop = GetMaxGradientStop(chartFillImpl.PreservedGradient);
				double transparency = 0.0;
				if (maxGradientStop.Transparency != 0)
				{
					transparency = (double)(100000 - maxGradientStop.Transparency) / 100000.0;
				}
				GradientType gradientType = chartFillImpl.PreservedGradient.GradientType;
				bool flag = false;
				GradientStops preservedGradient = chartFillImpl.PreservedGradient;
				int value = preservedGradient[0].ColorObject.Value;
				for (int i = 1; i < preservedGradient.Count; i++)
				{
					if (value != preservedGradient[i].ColorObject.Value)
					{
						flag = true;
					}
				}
				result = ((!((gradientType == GradientType.Liniar || gradientType == GradientType.Circle) && flag)) ? new BrushInfo(SfColor(maxGradientStop.ColorObject.GetRGB(parentWorkbook), transparency)) : ApplyGradientFill(chartFillImpl));
			}
			else if (chartFillImpl != null && chartFillImpl.GradientStops != null)
			{
				GradientStopImpl maxGradientStop2 = GetMaxGradientStop(chartFillImpl.GradientStops);
				double transparency2 = 0.0;
				if (maxGradientStop2.Transparency != 0)
				{
					transparency2 = (double)(100000 - maxGradientStop2.Transparency) / 100000.0;
				}
				GradientType gradientType2 = chartFillImpl.GradientStops.GradientType;
				bool flag2 = false;
				GradientStops gradientStops = chartFillImpl.GradientStops;
				int value2 = gradientStops[0].ColorObject.Value;
				for (int j = 1; j < gradientStops.Count; j++)
				{
					if (value2 != gradientStops[j].ColorObject.Value)
					{
						flag2 = true;
					}
				}
				result = ((!((gradientType2 == GradientType.Liniar || gradientType2 == GradientType.Circle) && flag2)) ? new BrushInfo(SfColor(maxGradientStop2.ColorObject.GetRGB(parentWorkbook), transparency2)) : ApplyGradientFill(chartFillImpl));
			}
			else
			{
				result = new BrushInfo(SfColor(foreColor));
			}
			break;
		}
		}
		return result;
	}

	internal BrushInfo ApplyGradientFill(ChartFillImpl fillObject)
	{
		BrushInfo result = null;
		List<GradientStopImpl> list = ((fillObject.PreservedGradient != null) ? fillObject.PreservedGradient.OrderBy((GradientStopImpl x) => x.Position).ToList() : fillObject.GradientStops.OrderBy((GradientStopImpl x) => x.Position).ToList());
		int num = 0;
		BrushInfoColorArrayList brushInfoColorArrayList = new BrushInfoColorArrayList();
		float[] array;
		double gradientAngle;
		GradientType gradientType;
		Rectangle fillToRect;
		if (fillObject.PreservedGradient != null)
		{
			array = new float[fillObject.PreservedGradient.Count + 2];
			gradientAngle = fillObject.PreservedGradient.Angle / 60000;
			gradientType = fillObject.PreservedGradient.GradientType;
			fillToRect = fillObject.PreservedGradient.FillToRect;
		}
		else
		{
			array = new float[fillObject.GradientStops.Count + 2];
			gradientAngle = fillObject.GradientStops.Angle / 60000;
			gradientType = fillObject.GradientStops.GradientType;
			fillToRect = fillObject.GradientStops.FillToRect;
			list = fillObject.GradientStops.OrderBy((GradientStopImpl x) => x.Position).ToList();
		}
		switch (gradientType)
		{
		case GradientType.Liniar:
		{
			if (list[0].Position != 0)
			{
				brushInfoColorArrayList.Add(SfColor(list[0].ColorObject.GetRGB(parentWorkbook)));
				array[0] = 0f;
				num++;
			}
			for (int k = 0; k < list.Count; k++)
			{
				brushInfoColorArrayList.Add(SfColor(list[k].ColorObject.GetRGB(parentWorkbook)));
				array[k + num] = (float)list[k].Position / 100000f;
			}
			if (list[list.Count - 1].Position / 100000 != 1)
			{
				array[brushInfoColorArrayList.Count] = 1f;
				brushInfoColorArrayList.Add(SfColor(list[list.Count - 1].ColorObject.GetRGB(parentWorkbook)));
			}
			ColorBlend colorBlend = new ColorBlend(brushInfoColorArrayList.Count);
			for (int l = 0; l < brushInfoColorArrayList.Count; l++)
			{
				colorBlend.Colors[l] = brushInfoColorArrayList[l];
				colorBlend.Positions[l] = array[l];
			}
			switch (fillObject.GradientStyle.ToString())
			{
			case "Horizontal":
				result = new BrushInfo(GradientStyle.Horizontal, brushInfoColorArrayList, gradientAngle, colorBlend);
				break;
			case "Vertical":
				result = new BrushInfo(GradientStyle.Vertical, brushInfoColorArrayList, gradientAngle, colorBlend);
				break;
			case "DiagonalUp":
			case "Diagonal_Up":
				result = new BrushInfo(GradientStyle.ForwardDiagonal, brushInfoColorArrayList, gradientAngle, colorBlend);
				break;
			default:
				result = new BrushInfo(GradientStyle.BackwardDiagonal, brushInfoColorArrayList, gradientAngle, colorBlend);
				break;
			}
			break;
		}
		case GradientType.Circle:
		{
			for (int i = 0; i < list.Count; i++)
			{
				brushInfoColorArrayList.Add(SfColor(list[i].ColorObject.GetRGB(parentWorkbook)));
				array[i + num] = (float)list[i].Position / 100000f;
			}
			ColorBlend colorBlend = new ColorBlend(brushInfoColorArrayList.Count);
			for (int j = 0; j < brushInfoColorArrayList.Count; j++)
			{
				colorBlend.Colors[j] = brushInfoColorArrayList[j];
				colorBlend.Positions[j] = array[j];
			}
			result = new BrushInfo(GradientStyle.PathRectangle, brushInfoColorArrayList, colorBlend, fillToRect);
			break;
		}
		}
		return result;
	}

	private GradientStopImpl GetMaxGradientStop(GradientStops gradientStops)
	{
		int num = 0;
		int index = 0;
		List<int> list = new List<int>(gradientStops.Count);
		for (int i = 0; i < gradientStops.Count; i++)
		{
			list.Add(gradientStops[i].Position / 1000);
		}
		list.Sort();
		for (int j = 0; j < gradientStops.Count; j++)
		{
			int num2 = gradientStops[j].Position / 1000;
			int num3 = list.IndexOf(num2);
			int num4 = 0;
			int num5 = 0;
			num4 = num2 - ((num3 != 0) ? list[num3 - 1] : 0);
			num5 = ((num3 == list.Count - 1) ? 100 : list[num3 + 1]) - num2;
			if (num5 > num)
			{
				index = j;
				num = num5;
			}
			if (num4 > num)
			{
				index = j;
				num = num4;
			}
			if (j == gradientStops.Count - 1 && num == num4 && num5 == 0)
			{
				index = j;
			}
		}
		list.Clear();
		return gradientStops[index];
	}

	private string GetTextFromDisplayUnits(ChartValueAxisImpl axis)
	{
		int displayUnit = (int)axis.DisplayUnit;
		string result = string.Empty;
		if (axis.DisplayUnitLabel.Text == null)
		{
			switch (displayUnit)
			{
			case 1:
			case 2:
			case 5:
				result = axis.DisplayUnit.ToString();
				break;
			case 3:
				result = "x10000";
				break;
			case 4:
				result = "x100000";
				break;
			case 6:
				result = "x10000000";
				break;
			case 7:
				result = "x100000000";
				break;
			case 8:
				result = "Billions";
				break;
			case 9:
				result = "Trillions";
				break;
			case 65535:
				result = "x" + axis.DisplayUnitCustom;
				break;
			}
		}
		else
		{
			result = (axis.DisplayUnitLabel as ChartTextAreaImpl).Text;
		}
		return result;
	}

	private void SfNumericalAxisCommon(ChartAxis sfaxis, ChartValueAxisImpl serieValAxis, ChartValueAxisImpl oppositeAxis, bool condition)
	{
		sfaxis.EdgeLabelsDrawingMode = ChartAxisEdgeLabelsDrawingMode.Center;
		sfaxis.IsVerticalAxisAutoCross = oppositeAxis.IsAutoCross;
		double num = 1.0;
		ChartCategoryAxisImpl chartCategoryAxisImpl = oppositeAxis as ChartCategoryAxisImpl;
		if (chartCategoryAxisImpl == null || chartCategoryAxisImpl.IsChartBubbleOrScatter)
		{
			num = GetDisplayUnitValue(oppositeAxis);
		}
		sfaxis.DisplayUnits = num;
		if (!oppositeAxis.IsAutoCross)
		{
			sfaxis.Origin = oppositeAxis.CrossesAt / num - ((chartCategoryAxisImpl == null || chartCategoryAxisImpl.IsChartBubbleOrScatter) ? 0.0 : 1.5);
		}
		CheckAndApplyAxisLineStyle(serieValAxis.Border as ChartBorderImpl, out var lineStyle, serieValAxis.ParentChart, ChartElementsEnum.AxisLine);
		sfaxis.LineType = lineStyle;
		if (serieValAxis.HasAxisTitle)
		{
			SfChartAxisTitle(sfaxis, serieValAxis);
		}
		SfTickLines(serieValAxis, sfaxis);
		SfGridLines(serieValAxis, sfaxis);
		sfaxis.LabelRotateAngle = serieValAxis.TextRotationAngle;
		if (sfaxis.LabelRotateAngle != 0)
		{
			if (serieValAxis.LabelAlign == AxisLabelAlignment.Center)
			{
				sfaxis.LabelAlignment = StringAlignment.Center;
			}
			else
			{
				sfaxis.LabelAlignment = StringAlignment.Near;
			}
			sfaxis.LabelRotate = true;
		}
		TrySetValueAxisNumberFormat(serieValAxis, sfaxis);
		if (IsBarChartAxis(serieValAxis))
		{
			if (((serieValAxis.TickLabelPosition == OfficeTickLabelPosition.TickLabelPosition_High || oppositeAxis.ReversePlotOrder) && (serieValAxis.TickLabelPosition != OfficeTickLabelPosition.TickLabelPosition_High || !oppositeAxis.ReversePlotOrder)) || oppositeAxis.IsMaxCross)
			{
				sfaxis.OpposedPosition = true;
			}
		}
		else if (condition && (((serieValAxis.TickLabelPosition == OfficeTickLabelPosition.TickLabelPosition_High || oppositeAxis.ReversePlotOrder) && (serieValAxis.TickLabelPosition != OfficeTickLabelPosition.TickLabelPosition_High || !oppositeAxis.ReversePlotOrder)) || oppositeAxis.IsMaxCross))
		{
			if (!oppositeAxis.IsMaxCross && oppositeAxis.IsAutoCross && !IsChart3D)
			{
				CreateNewLineSeries(sfaxis, serieValAxis, oppositeAxis);
			}
			else
			{
				sfaxis.OpposedPosition = true;
			}
		}
		if (serieValAxis.TickLabelPosition == OfficeTickLabelPosition.TickLabelPosition_None)
		{
			sfaxis.ForeColor = SfColor(Color.Transparent, 1.0);
		}
		if (serieValAxis.IsAutoCross && serieValAxis.TickLabelPosition == OfficeTickLabelPosition.TickLabelPosition_NextToAxis)
		{
			sfaxis.Crossing = double.NaN;
		}
		if (serieValAxis.ReversePlotOrder)
		{
			sfaxis.Inversed = true;
		}
		if (!serieValAxis.Visible)
		{
			sfaxis.IsVisible = false;
		}
	}

	internal void CreateNewLineSeries(ChartAxis sfaxis, ChartValueAxisImpl serieValAxis, ChartValueAxisImpl oppositeAxis)
	{
		if (ItemSource != null)
		{
			ChartSeries chartSeries = new ChartSeries(m_sfChart);
			chartSeries.SetItemSource(ItemSource);
			chartSeries.Type = ChartSeriesType.Line;
			chartSeries.YAxis = new ChartAxis
			{
				DrawGrid = false,
				OpposedPosition = true,
				Orientation = ChartOrientation.Vertical
			};
			chartSeries.XAxis = new ChartAxis
			{
				DrawGrid = false,
				OpposedPosition = true,
				ValueType = ChartValueType.Category,
				IsVisible = false
			};
			chartSeries.ParentChart.Axes.Add(chartSeries.YAxis);
			chartSeries.ParentChart.Axes.Add(chartSeries.XAxis);
			chartSeries.Style.Border.Width = 0f;
			chartSeries.Style.Interior = new BrushInfo(SfColor(Color.Transparent, 1.0));
			chartSeries.Style.Border.Color = SfColor(Color.Transparent, 1.0);
			SfNumericalAxis(chartSeries.YAxis, serieValAxis, oppositeAxis, condition: false);
			LineInfo lineInfo = new LineInfo();
			lineInfo.BackColor = SfColor(Color.FromArgb(255, 255, 255), 1.0);
			lineInfo.ForeColor = SfColor(Color.FromArgb(255, 255, 255), 1.0);
			chartSeries.YAxis.LineType = lineInfo;
			m_hasNewSeries = true;
			m_sfChart.Series.Add(chartSeries);
			chartSeries.LegendItem.Visible = false;
			m_newSeriesIndex = m_sfChart.Series.Count - 1;
			sfaxis.OpposedPosition = false;
			sfaxis.Font = new Font(oppositeAxis.Font.FontName, 0.01f);
		}
	}

	private void TrySetValueAxisNumberFormat(ChartValueAxisImpl xlsiovalue, ChartAxis sfaxis)
	{
		sfaxis.Font = GetDrawingFont(xlsiovalue.Font);
		Color color = ((!xlsiovalue.Font.IsAutoColor || !TryAndGetColorBasedOnElement(ChartElementsEnum.ChartAreaFill, xlsiovalue.ParentChart, out color) || color.R != color.G || color.B != color.R || color.R != 0) ? xlsiovalue.Font.RGBColor : Color.FromArgb(0, 255, 255, 255));
		sfaxis.ForeColor = SfColor(color);
		if (!(xlsiovalue.FrameFormat as ChartFrameFormatImpl).IsAutomaticFormat && xlsiovalue.FrameFormat.Interior.Pattern != 0)
		{
			sfaxis.BackInterior = GetBrushFromDataFormat(xlsiovalue.FrameFormat);
		}
		bool flag = false;
		if (IsStacked100AxisFormat(xlsiovalue) && xlsiovalue.NumberFormat == "General")
		{
			flag = true;
		}
		AxisLabelConverter axisLabelConverter = new AxisLabelConverter();
		if (xlsiovalue.TickLabelPosition == OfficeTickLabelPosition.TickLabelPosition_None)
		{
			axisLabelConverter.AxisTypeInByte = 8;
			return;
		}
		if (sfaxis.ValueType == ChartValueType.Logarithmic)
		{
			axisLabelConverter.AxisTypeInByte = 1;
			return;
		}
		axisLabelConverter.NumberFormatApplyEvent += ApplyNumberFormat;
		if (flag || IsStacked100AxisFormat(xlsiovalue))
		{
			axisLabelConverter.AxisTypeInByte = 2;
			axisLabelConverter.NumberFormat = (flag ? "0%" : xlsiovalue.NumberFormat);
		}
		else if (xlsiovalue.IsSourceLinked && !IsCategoryAxisAuto(xlsiovalue))
		{
			axisLabelConverter.NumberFormat = GetSourceNumberFormat(xlsiovalue);
		}
		else if (xlsiovalue.NumberFormat != null && xlsiovalue.NumberFormat.ToLower() != "General".ToLower() && xlsiovalue.NumberFormat.ToLower() != "standard")
		{
			axisLabelConverter.NumberFormat = xlsiovalue.NumberFormat;
		}
		else
		{
			axisLabelConverter.NumberFormat = "General";
		}
		sfaxis.AxisLabelConverter = axisLabelConverter;
	}

	private Font GetDrawingFont(IOfficeFont fontFrom)
	{
		FontStyle fontStyle = FontStyle.Regular;
		if (fontFrom.Bold)
		{
			fontStyle |= FontStyle.Bold;
		}
		if (fontFrom.Italic)
		{
			fontStyle |= FontStyle.Italic;
		}
		if (fontFrom.Strikethrough)
		{
			fontStyle |= FontStyle.Strikeout;
		}
		if (fontFrom.Underline != 0)
		{
			fontStyle |= FontStyle.Underline;
		}
		return new Font(fontFrom.FontName, (float)fontFrom.Size, fontStyle);
	}

	private Font GetDrawingFont(string fontName, float fontSize, IOfficeFont fontFrom)
	{
		FontStyle fontStyle = FontStyle.Regular;
		if (fontFrom.Bold)
		{
			fontStyle |= FontStyle.Bold;
		}
		if (fontFrom.Italic)
		{
			fontStyle |= FontStyle.Italic;
		}
		if (fontFrom.Strikethrough)
		{
			fontStyle |= FontStyle.Strikeout;
		}
		if (fontFrom.Underline != 0)
		{
			fontStyle |= FontStyle.Underline;
		}
		return new Font(fontName, fontSize, fontStyle);
	}

	internal string SwitchFonts(string testString, string fontName)
	{
		if (!string.IsNullOrEmpty(testString) && CheckForCJK(testString))
		{
			return "Arial Unicode MS";
		}
		return fontName;
	}

	internal bool CheckUnicode(string unicodeText)
	{
		if (!string.IsNullOrEmpty(unicodeText))
		{
			char[] array = unicodeText.ToCharArray();
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] > 'ÿ' && Array.IndexOf(_numberFormatChar, array[i]) < 0)
				{
					return true;
				}
			}
		}
		return false;
	}

	private bool CheckForGeneralPunctuationSegeoUI(string unicodeText)
	{
		char[] array = unicodeText.ToCharArray();
		List<int> list = new List<int>();
		list.Add(9632);
		list.Add(9644);
		list.Add(9650);
		list.Add(9658);
		list.Add(9660);
		list.Add(9668);
		list.Add(9688);
		list.Add(9689);
		list.Add(65533);
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] > 'ÿ' && Array.IndexOf(_numberFormatChar, array[i]) < 0 && ((array[i] >= '‐' && array[i] <= '‑') || array[i] == '\u2029' || (array[i] >= '♠' && array[i] <= '♡') || array[i] == '♣' || (array[i] >= '♥' && array[i] <= '♦') || array[i] == '♯' || (array[i] >= '❶' && array[i] <= '❿') || (array[i] >= 'Ա' && array[i] <= 'Ֆ') || (array[i] >= 'ՙ' && array[i] <= '՟') || (array[i] >= 'ա' && array[i] <= '֊') || (array[i] >= '֍' && array[i] <= '֏') || array[i] == '⓿' || list.Contains(array[i])))
			{
				return true;
			}
		}
		return false;
	}

	private bool CheckForExtendedPlan(string unicodeText)
	{
		char[] array = unicodeText.ToCharArray();
		List<int> list = new List<int>();
		list.Add(56527);
		list.Add(56711);
		list.Add(56720);
		list.Add(56726);
		list.Add(56744);
		list.Add(56764);
		list.Add(56803);
		list.Add(56808);
		list.Add(56815);
		list.Add(56819);
		list.Add(57072);
		list.Add(56787);
		list.Add(55357);
		list.Add(55356);
		list.Add(55358);
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] > 'ÿ' && Array.IndexOf(_numberFormatChar, array[i]) < 0 && ((array[i] >= '\udc00' && array[i] <= '\udc2b') || (array[i] >= '\udd70' && array[i] <= '\udd9a') || (array[i] >= '\udde6' && array[i] <= '\uddff') || (array[i] >= '\ude01' && array[i] <= '\ude02') || (array[i] >= '\ude10' && array[i] <= '\ude3a') || (array[i] >= '\ude50' && array[i] <= '\ude51') || (array[i] >= '\udf00' && array[i] <= '\udf21') || (array[i] >= '\udf24' && array[i] <= '\udff0') || (array[i] >= '\udff3' && array[i] <= '\udff5') || (array[i] >= '\udd49' && array[i] <= '\udd67') || (array[i] >= '\udd6f' && array[i] <= '\udd70') || (array[i] >= '\udd73' && array[i] <= '\udd7a') || (array[i] >= '\udd8a' && array[i] <= '\udd8d') || (array[i] >= '\udda4' && array[i] <= '\udda5') || (array[i] >= '\uddb1' && array[i] <= '\uddb2') || (array[i] >= '\uddc2' && array[i] <= '\uddc4') || (array[i] >= '\uddd1' && array[i] <= '\uddd3') || (array[i] >= '\udddc' && array[i] <= '\uddde') || (array[i] >= '\ude80' && array[i] <= '\udeec') || (array[i] >= '\udd10' && array[i] <= '\udd6b') || (array[i] >= '\uddfa' && array[i] <= '\ude4f') || (array[i] >= '\udd80' && array[i] <= '\udd97') || (array[i] >= '\uddd0' && array[i] <= '\udde6') || (array[i] >= '\udff7' && array[i] <= '\udfff') || (array[i] >= '\udc00' && array[i] <= '\udd3d') || list.Contains(array[i])))
			{
				return true;
			}
		}
		return false;
	}

	private bool CheckForDingbats(string unicodeText)
	{
		char[] array = unicodeText.ToCharArray();
		List<int> list = new List<int>();
		list.Add(10013);
		list.Add(10017);
		list.Add(10024);
		list.Add(10071);
		list.Add(10145);
		list.Add(10160);
		list.Add(10175);
		list.Add(9000);
		list.Add(9167);
		list.Add(8505);
		list.Add(8419);
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] > 'ÿ' && Array.IndexOf(_numberFormatChar, array[i]) < 0 && ((array[i] >= '✁' && array[i] <= '✅') || (array[i] >= '✈' && array[i] <= '✐') || (array[i] >= '✒' && array[i] <= '✘') || (array[i] >= '✱' && array[i] <= '❌') || (array[i] >= '❓' && array[i] <= '❕') || (array[i] >= '➕' && array[i] <= '➗') || (array[i] >= '⌚' && array[i] <= '⌛') || (array[i] >= '⏩' && array[i] <= '⏳') || (array[i] >= '⏸' && array[i] <= '⏺') || (array[i] >= '↚' && array[i] <= '⇿') || list.Contains(array[i])))
			{
				return true;
			}
		}
		return false;
	}

	private bool CheckForGeneralPunctuation(string unicodeText)
	{
		char[] array = unicodeText.ToCharArray();
		List<int> list = new List<int>();
		list.Add(12868);
		list.Add(12869);
		list.Add(12870);
		list.Add(12871);
		list.Add(12951);
		list.Add(12953);
		list.Add(57352);
		list.Add(57353);
		list.Add(12336);
		list.Add(12349);
		list.Add(9752);
		list.Add(9766);
		list.Add(9770);
		list.Add(9855);
		list.Add(9881);
		list.Add(9937);
		list.Add(9949);
		list.Add(9955);
		list.Add(9981);
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] > 'ÿ' && Array.IndexOf(_numberFormatChar, array[i]) < 0 && (array[i] == '⁉' || array[i] == '⭕' || (array[i] >= '⬅' && array[i] <= '⬇') || (array[i] >= '⬒' && array[i] <= '⬜') || (array[i] >= '⭐' && array[i] <= '⭒') || (array[i] >= '⤴' && array[i] <= '⤷') || (array[i] >= '☀' && array[i] <= '☄') || (array[i] >= '☎' && array[i] <= '☒') || (array[i] >= '☔' && array[i] <= '☕') || (array[i] >= '☚' && array[i] <= '☠') || (array[i] >= '☢' && array[i] <= '☣') || (array[i] >= '☮' && array[i] <= '☯') || (array[i] >= '☸' && array[i] <= '☻') || (array[i] >= '☿' && array[i] <= '♓') || (array[i] >= '♠' && array[i] <= '♨') || (array[i] >= '♲' && array[i] <= '♽') || (array[i] >= '⚒' && array[i] <= '⚗') || (array[i] >= '⚛' && array[i] <= '⚜') || (array[i] >= '⚠' && array[i] <= '⚫') || (array[i] >= '⚰' && array[i] <= '⚱') || (array[i] >= '⚽' && array[i] <= '⚾') || (array[i] >= '⛄' && array[i] <= '⛅') || (array[i] >= '⛇' && array[i] <= '⛈') || (array[i] >= '⛎' && array[i] <= '⛏') || (array[i] >= '⛓' && array[i] <= '⛔') || (array[i] >= '⛩' && array[i] <= '⛪') || (array[i] >= '⛰' && array[i] <= '⛵') || (array[i] >= '⛷' && array[i] <= '⛺') || list.Contains(array[i])))
			{
				return true;
			}
		}
		return false;
	}

	private bool CheckForAmharic(string unicodeText)
	{
		char[] array = unicodeText.ToCharArray();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] > 'ÿ' && Array.IndexOf(_numberFormatChar, array[i]) < 0 && (array[i] < 'ሀ' || array[i] > '\u139f') && (array[i] < 'ⶀ' || array[i] > '\u2ddf') && (array[i] < '\uab00' || array[i] > '\uab2f'))
			{
				return false;
			}
		}
		return true;
	}

	private bool CheckForKhmer(string unicodeText)
	{
		char[] array = unicodeText.ToCharArray();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] > 'ÿ' && Array.IndexOf(_numberFormatChar, array[i]) < 0 && (array[i] < 'ក' || array[i] > '\u17ff'))
			{
				return false;
			}
		}
		return true;
	}

	private bool CheckForThai(string unicodeText)
	{
		char[] array = unicodeText.ToCharArray();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] > 'ÿ' && Array.IndexOf(_numberFormatChar, array[i]) < 0 && (array[i] < '\u0e00' || array[i] > '\u0e7f'))
			{
				return false;
			}
		}
		return true;
	}

	private bool CheckForSinhala(string unicodeText)
	{
		char[] array = unicodeText.ToCharArray();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] > 'ÿ' && Array.IndexOf(_numberFormatChar, array[i]) < 0 && (array[i] < '\u0d80' || array[i] > '\u0dff'))
			{
				return false;
			}
		}
		return true;
	}

	private bool CheckForMyanmar(string unicodeText)
	{
		char[] array = unicodeText.ToCharArray();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] > 'ÿ' && Array.IndexOf(_numberFormatChar, array[i]) < 0 && (array[i] < 'က' || array[i] > '႟'))
			{
				return false;
			}
		}
		return true;
	}

	private bool CheckForTamil(string unicodeText)
	{
		char[] array = unicodeText.ToCharArray();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] > 'ÿ' && Array.IndexOf(_numberFormatChar, array[i]) < 0 && (array[i] < '\u0b80' || array[i] > '\u0bff'))
			{
				return false;
			}
		}
		return true;
	}

	private bool CheckForTelugu(string unicodeText)
	{
		char[] array = unicodeText.ToCharArray();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] > 'ÿ' && Array.IndexOf(_numberFormatChar, array[i]) < 0 && (array[i] < '\u0c00' || array[i] > '౿'))
			{
				return false;
			}
		}
		return true;
	}

	private bool CheckForPunjabi(string unicodeText)
	{
		char[] array = unicodeText.ToCharArray();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] > 'ÿ' && Array.IndexOf(_numberFormatChar, array[i]) < 0 && (array[i] < '\u0a00' || array[i] > '\u0a7f'))
			{
				return false;
			}
		}
		return true;
	}

	private bool CheckForMalayalam(string unicodeText)
	{
		char[] array = unicodeText.ToCharArray();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] > 'ÿ' && Array.IndexOf(_numberFormatChar, array[i]) < 0 && (array[i] < '\u0d00' || array[i] > 'ൿ'))
			{
				return false;
			}
		}
		return true;
	}

	private bool CheckForKanndada(string unicodeText)
	{
		char[] array = unicodeText.ToCharArray();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] > 'ÿ' && Array.IndexOf(_numberFormatChar, array[i]) < 0 && (array[i] < 'ಀ' || array[i] > '\u0cff'))
			{
				return false;
			}
		}
		return true;
	}

	private bool CheckForGujarati(string unicodeText)
	{
		char[] array = unicodeText.ToCharArray();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] > 'ÿ' && Array.IndexOf(_numberFormatChar, array[i]) < 0 && (array[i] < '\u0a80' || array[i] > '\u0aff'))
			{
				return false;
			}
		}
		return true;
	}

	private bool CheckForMarathi(string unicodeText)
	{
		char[] array = unicodeText.ToCharArray();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] > 'ÿ' && Array.IndexOf(_numberFormatChar, array[i]) < 0 && (array[i] < '\u0900' || array[i] > 'ॿ'))
			{
				return false;
			}
		}
		return true;
	}

	private bool CheckForLatin(string unicodeText)
	{
		char[] array = null;
		array = unicodeText.ToCharArray();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] > 'ÿ' && Array.IndexOf(_numberFormatChar, array[i]) < 0 && (((array[i] < ' ' || array[i] > '~') && (array[i] < 'Ā' || array[i] > '\u036f')) || (array[i] >= '\u0590' && array[i] <= '\u05ff')))
			{
				return false;
			}
		}
		return true;
	}

	private bool CheckForCJK(string unicodeText)
	{
		char[] array = null;
		array = unicodeText.ToCharArray();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] > 'ÿ' && Array.IndexOf(_numberFormatChar, array[i]) < 0 && ((array[i] >= '一' && array[i] <= '鿿') || (array[i] >= '\u3040' && array[i] <= 'ヿ') || (array[i] >= 'ｶ' && array[i] <= 'ﾝ')))
			{
				return true;
			}
		}
		return false;
	}

	private bool CheckForArabicOrHebrew(string unicodeText)
	{
		char[] array = null;
		array = unicodeText.ToCharArray();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] > 'ÿ' && Array.IndexOf(_numberFormatChar, array[i]) < 0 && array[i] >= '\u0590' && array[i] <= 'ۿ')
			{
				return true;
			}
		}
		return false;
	}

	private bool CheckforKorean(string unicodeText)
	{
		char[] array = null;
		array = unicodeText.ToCharArray();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] > 'ÿ' && Array.IndexOf(_numberFormatChar, array[i]) < 0 && ((array[i] >= '가' && array[i] <= '힣') || (array[i] >= 'ᄀ' && array[i] <= 'ᇿ') || (array[i] >= '\u3130' && array[i] <= '\u318f') || (array[i] >= 'ꥠ' && array[i] <= '\ua97f') || (array[i] >= 'ힰ' && array[i] <= '\ud7ff')))
			{
				return true;
			}
		}
		return false;
	}

	internal void SfNumericalAxis(ChartAxis sfaxis, ChartValueAxisImpl xlsiovalue, ChartValueAxisImpl categoryAxis, bool condition)
	{
		SfNumericalAxisCommon(sfaxis, xlsiovalue, categoryAxis, condition);
		double num = 1.0;
		bool num2 = IsStacked100AxisFormat(xlsiovalue);
		if (!num2)
		{
			num = GetDisplayUnitValue(xlsiovalue);
		}
		sfaxis.DisplayUnits = num;
		if (xlsiovalue.MajorUnit > 0.0)
		{
			sfaxis.Range.Interval = xlsiovalue.MajorUnit / num;
			sfaxis.IsMajorSet = true;
		}
		if (xlsiovalue.MajorUnit > 0.0 && xlsiovalue.MinorUnit > 0.0)
		{
			double num3 = xlsiovalue.MajorUnit / xlsiovalue.MinorUnit;
			sfaxis.SmallTicksPerInterval = (int)((num3 >= 2.0) ? (num3 - 1.0) : ((num3 == 1.0) ? 0.0 : num3));
		}
		else if (xlsiovalue.MinorTickMark != 0)
		{
			sfaxis.SmallTicksPerInterval = 3;
		}
		if (!xlsiovalue.IsAutoMax)
		{
			sfaxis.Range.Max = xlsiovalue.MaximumValue / num;
			sfaxis.IsMaxSet = true;
			if (xlsiovalue.MajorUnit > 0.0 && sfaxis.Range.Max % sfaxis.Range.Interval > 0.0)
			{
				sfaxis.EdgeLabelsDrawingMode = ChartAxisEdgeLabelsDrawingMode.Center;
			}
		}
		if (!xlsiovalue.IsAutoMin)
		{
			sfaxis.Range.Min = xlsiovalue.MinimumValue / num;
			sfaxis.IsMinSet = true;
		}
		if (sfaxis.IsMinSet && sfaxis.IsMaxSet && sfaxis.IsMajorSet)
		{
			sfaxis.RangeType = ChartAxisRangeType.Set;
		}
		if (num2)
		{
			if (sfaxis.RangeType != 0 || sfaxis.IsMinSet)
			{
				sfaxis.Range.Min = sfaxis.Range.Min * 100.0;
			}
			if (sfaxis.RangeType != 0 || sfaxis.IsMaxSet)
			{
				sfaxis.Range.Max = sfaxis.Range.Max * 100.0;
			}
			else
			{
				sfaxis.Range.Max = 100.0;
			}
			if (sfaxis.RangeType != 0 || sfaxis.IsMajorSet)
			{
				sfaxis.Range.Interval = sfaxis.Range.Interval * 100.0;
			}
		}
		else if (!IsChartEx)
		{
			_ = sfaxis.RangeType;
		}
	}

	internal void SfNumericalAxis3D(ChartAxis sfaxis, ChartValueAxisImpl xlsiovalue, ChartCategoryAxisImpl categoryAxis)
	{
		SfNumericalAxisCommon(sfaxis, xlsiovalue, categoryAxis, condition: true);
		double num = 1.0;
		bool num2 = IsStacked100AxisFormat(xlsiovalue);
		if (!num2)
		{
			num = GetDisplayUnitValue(xlsiovalue);
		}
		if (xlsiovalue.MajorUnit > 0.0)
		{
			sfaxis.Range.Interval = xlsiovalue.MajorUnit / num;
		}
		sfaxis.DisplayUnits = num;
		if (xlsiovalue.MajorUnit > 0.0 && xlsiovalue.MinorUnit > 0.0)
		{
			double num3 = xlsiovalue.MajorUnit / xlsiovalue.MinorUnit;
			sfaxis.SmallTicksPerInterval = (int)((num3 >= 2.0) ? (num3 - 1.0) : ((num3 == 1.0) ? 0.0 : num3));
		}
		else
		{
			sfaxis.SmallTicksPerInterval = 3;
		}
		if (!xlsiovalue.IsAutoMax)
		{
			sfaxis.Range.Max = xlsiovalue.MaximumValue / num;
			if (xlsiovalue.MajorUnit > 0.0 && sfaxis.Range.Max % sfaxis.Range.Interval > 0.0)
			{
				sfaxis.EdgeLabelsDrawingMode = ChartAxisEdgeLabelsDrawingMode.Center;
			}
		}
		if (!xlsiovalue.IsAutoMin)
		{
			sfaxis.Range.Min = xlsiovalue.MinimumValue / num;
		}
		if (num2)
		{
			sfaxis.Range.Max = 100.0;
		}
	}

	private void SfGridLines(ChartAxisImpl xlsioAxis, ChartAxis sfAxis)
	{
		LineInfo lineStyle;
		if (xlsioAxis.HasMajorGridLines)
		{
			sfAxis.DrawGrid = true;
			CheckAndApplyAxisLineStyle(xlsioAxis.MajorGridLines.Border as ChartBorderImpl, out lineStyle, xlsioAxis.ParentChart, ChartElementsEnum.MajorGridLines);
			sfAxis.GridLineType = lineStyle;
		}
		else
		{
			sfAxis.DrawGrid = false;
		}
		if (xlsioAxis.HasMinorGridLines)
		{
			sfAxis.DrawMinorGrid = true;
			CheckAndApplyAxisLineStyle(xlsioAxis.MinorGridLines.Border as ChartBorderImpl, out lineStyle, xlsioAxis.ParentChart, ChartElementsEnum.MinorGridLines);
			sfAxis.MinorGridLineType = lineStyle;
		}
		else
		{
			sfAxis.DrawMinorGrid = false;
		}
	}

	private void SfCategoryAxisCommon(ChartAxis primaryValueAxis, ChartAxis sfCatAxis, ChartCategoryAxisImpl xlsioCatAxis, ChartValueAxisImpl xlsioValAxis, bool condition)
	{
		sfCatAxis.EdgeLabelsDrawingMode = ChartAxisEdgeLabelsDrawingMode.Center;
		sfCatAxis.IsHorizontalAxisAutoCross = xlsioValAxis.IsAutoCross;
		IOfficeFont font = xlsioCatAxis.Font;
		string fontName = font.FontName;
		sfCatAxis.Font = GetDrawingFont(font.FontName, (float)font.Size, font);
		if (xlsioCatAxis.CategoryLabelsIRange != null && xlsioCatAxis.CategoryLabelsIRange is RangeImpl && xlsioCatAxis.CategoryLabelsIRange.Cells != null && xlsioCatAxis.CategoryLabelsIRange.Cells.Length != 0)
		{
			string displayText = xlsioCatAxis.CategoryLabelsIRange.Cells[0].DisplayText;
			if (CheckUnicode(displayText))
			{
				fontName = SwitchFonts(displayText, fontName);
				sfCatAxis.Font = GetDrawingFont(fontName, (float)font.Size, font);
			}
			MultiLevelCategoryLable(xlsioCatAxis, sfCatAxis);
		}
		Color color = ((!xlsioCatAxis.Font.IsAutoColor || !TryAndGetColorBasedOnElement(ChartElementsEnum.ChartAreaFill, xlsioCatAxis.ParentChart, out color) || color.R != color.G || color.B != color.R || color.R != 0) ? xlsioCatAxis.Font.RGBColor : Color.FromArgb(0, 255, 255, 255));
		sfCatAxis.ForeColor = SfColor(color);
		CheckAndApplyAxisLineStyle(xlsioCatAxis.Border as ChartBorderImpl, out var lineStyle, xlsioCatAxis.ParentChart, ChartElementsEnum.AxisLine);
		sfCatAxis.LineType = lineStyle;
		if (xlsioCatAxis.HasAxisTitle)
		{
			SfChartAxisTitle(sfCatAxis, xlsioCatAxis);
		}
		if (!xlsioValAxis.IsAutoCross)
		{
			if (IsStacked100AxisFormat(xlsioCatAxis) && xlsioValAxis.CrossesAt != 0.0)
			{
				double num = (xlsioValAxis.IsAutoMin ? 0.0 : xlsioValAxis.MinimumValue);
				double num2 = (xlsioValAxis.IsAutoMax ? 0.0 : xlsioValAxis.MaximumValue);
				if ((xlsioValAxis.CrossesAt < num2 || xlsioValAxis.CrossesAt == num2) && (xlsioValAxis.CrossesAt > num || xlsioValAxis.CrossesAt == num))
				{
					sfCatAxis.Origin = xlsioValAxis.CrossesAt * 100.0;
				}
				else
				{
					sfCatAxis.Origin = 0.0;
				}
			}
			else
			{
				sfCatAxis.Origin = xlsioValAxis.CrossesAt;
			}
		}
		else if (!sfCatAxis.OpposedPosition)
		{
			sfCatAxis.Origin = 0.0;
			if (xlsioCatAxis.TickLabelPosition == OfficeTickLabelPosition.TickLabelPosition_NextToAxis)
			{
				sfCatAxis.Crossing = 0.0;
			}
			if (primaryValueAxis != null && primaryValueAxis.IsMinSet)
			{
				sfCatAxis.Crossing = primaryValueAxis.Range.Min;
			}
		}
		SfTickLines(xlsioCatAxis, sfCatAxis);
		if (xlsioCatAxis.ParentChart.IsChartRadar)
		{
			SfGridLines(xlsioValAxis, sfCatAxis);
		}
		else if (xlsioCatAxis.ParentChart.ChartType.ToString().Contains("Funnel"))
		{
			sfCatAxis.DrawGrid = false;
		}
		else
		{
			SfGridLines(xlsioCatAxis, sfCatAxis);
		}
		sfCatAxis.LabelRotateAngle = xlsioCatAxis.TextRotationAngle;
		if (sfCatAxis.LabelRotateAngle != 0)
		{
			sfCatAxis.LabelRotate = true;
			if (xlsioCatAxis.LabelAlign == AxisLabelAlignment.Center)
			{
				sfCatAxis.LabelAlignment = StringAlignment.Center;
			}
			else
			{
				sfCatAxis.LabelAlignment = StringAlignment.Far;
			}
		}
		if (xlsioCatAxis.ReversePlotOrder)
		{
			sfCatAxis.Inversed = true;
		}
		if (IsBarChartAxis(xlsioCatAxis))
		{
			if (((xlsioCatAxis.TickLabelPosition == OfficeTickLabelPosition.TickLabelPosition_High || xlsioValAxis.ReversePlotOrder) && (xlsioCatAxis.TickLabelPosition != OfficeTickLabelPosition.TickLabelPosition_High || !xlsioValAxis.ReversePlotOrder)) || xlsioValAxis.IsMaxCross)
			{
				sfCatAxis.Origin = 0.0;
				sfCatAxis.OpposedPosition = true;
			}
		}
		else if (condition && (((xlsioCatAxis.TickLabelPosition == OfficeTickLabelPosition.TickLabelPosition_High || xlsioValAxis.ReversePlotOrder) && (xlsioCatAxis.TickLabelPosition != OfficeTickLabelPosition.TickLabelPosition_High || !xlsioValAxis.ReversePlotOrder)) || xlsioValAxis.IsMaxCross))
		{
			if (!xlsioValAxis.IsMaxCross && xlsioValAxis.IsAutoCross && !IsChart3D)
			{
				CreateNewLineSeries(sfCatAxis, xlsioCatAxis, xlsioValAxis);
			}
			else
			{
				sfCatAxis.OpposedPosition = true;
			}
		}
		if (xlsioCatAxis.TickLabelPosition == OfficeTickLabelPosition.TickLabelPosition_None)
		{
			sfCatAxis.ForeColor = SfColor(Color.Transparent, 1.0);
		}
		else
		{
			AxisLabelConverter axisLabelConverter = ((sfCatAxis.AxisLabelConverter == null) ? new AxisLabelConverter() : sfCatAxis.AxisLabelConverter);
			if (sfCatAxis.ValueType == ChartValueType.DateTime)
			{
				axisLabelConverter.AxisTypeInByte = 4;
				axisLabelConverter.NumberFormatApplyEvent += ApplyNumberFormat;
				if (xlsioCatAxis.IsSourceLinked && !IsCategoryAxisAuto(xlsioCatAxis))
				{
					axisLabelConverter.NumberFormat = GetSourceNumberFormat(xlsioCatAxis);
				}
				else if (xlsioCatAxis.NumberFormat != null && xlsioCatAxis.NumberFormat.ToLower() != "General".ToLower() && xlsioCatAxis.NumberFormat.ToLower() != "standard")
				{
					axisLabelConverter.NumberFormat = xlsioCatAxis.NumberFormat;
				}
				else
				{
					axisLabelConverter.NumberFormat = "General";
				}
			}
			else
			{
				axisLabelConverter.AxisTypeInByte = 1;
			}
			sfCatAxis.AxisLabelConverter = axisLabelConverter;
		}
		if (!(xlsioCatAxis.FrameFormat as ChartFrameFormatImpl).IsAutomaticFormat && xlsioCatAxis.FrameFormat.Interior.Pattern != 0)
		{
			sfCatAxis.BackInterior = GetBrushFromDataFormat(xlsioCatAxis.FrameFormat);
		}
		if (!xlsioCatAxis.Visible)
		{
			sfCatAxis.IsVisible = false;
		}
	}

	internal void MultiLevelCategoryLable(ChartCategoryAxisImpl xlsioCatAxis, ChartAxis sfCatAxis)
	{
		IRange range = xlsioCatAxis.CategoryLabelsIRange as RangeImpl;
		if (range.Column >= range.LastColumn || range.Row == range.LastRow || !(range is RangeImpl) || !(range as RangeImpl).IsMultiReference)
		{
			return;
		}
		Dictionary<string, DoubleRange> dictionary = new Dictionary<string, DoubleRange>();
		string text = null;
		double num = -0.5;
		double num2 = 0.5;
		for (int i = range.Row; i <= range.LastRow; i++)
		{
			if (!string.IsNullOrEmpty(range[i, range.Column].Value))
			{
				if (num != -0.5 && dictionary.Count == 0)
				{
					dictionary.Add(string.Empty, new DoubleRange(-0.5, num));
				}
				text = range[i, range.Column].DisplayText;
				for (; string.IsNullOrEmpty(range[i + 1, range.Column].Value) && i + 1 <= range.LastRow; i++)
				{
					num2 += 1.0;
				}
				dictionary.Add(text, new DoubleRange(num, num2));
				num = num2;
				num2 += 1.0;
			}
			else if (text == null)
			{
				num += 1.0;
				num2 += 1.0;
			}
		}
		foreach (KeyValuePair<string, DoubleRange> item in dictionary)
		{
			ChartAxisGroupingLabel chartAxisGroupingLabel = new ChartAxisGroupingLabel(item.Value, item.Key);
			if (xlsioCatAxis.Border.LinePattern == OfficeChartLinePattern.None)
			{
				chartAxisGroupingLabel.BorderStyle = ChartAxisGroupingLabelBorderStyle.WithoutBorder;
			}
			else
			{
				chartAxisGroupingLabel.BorderStyle = ChartAxisGroupingLabelBorderStyle.WithoutTopAndBottomBorder;
			}
			chartAxisGroupingLabel.Font = sfCatAxis.Font;
			sfCatAxis.GroupingLabels.Add(chartAxisGroupingLabel);
		}
	}

	internal void CreateNewLineSeries(ChartAxis sfCatAxis, ChartCategoryAxisImpl xlsioCatAxis, ChartValueAxisImpl xlsioValAxis)
	{
		if (ItemSource != null)
		{
			ChartSeries chartSeries = new ChartSeries(m_sfChart);
			chartSeries.SetItemSource(ItemSource);
			chartSeries.SortPoints = IsSeriesSorted;
			chartSeries.Type = ChartSeriesType.Line;
			chartSeries.YAxis = new ChartAxis
			{
				DrawGrid = false,
				OpposedPosition = true,
				Orientation = ChartOrientation.Vertical,
				IsVisible = false
			};
			chartSeries.XAxis = new ChartAxis
			{
				DrawGrid = false,
				OpposedPosition = true,
				ValueType = ChartValueType.Category
			};
			chartSeries.ParentChart.Axes.Add(chartSeries.YAxis);
			chartSeries.ParentChart.Axes.Add(chartSeries.XAxis);
			chartSeries.Style.Border.Width = 0f;
			chartSeries.Style.Interior = new BrushInfo(SfColor(Color.Transparent, 1.0));
			chartSeries.Style.Border.Color = SfColor(Color.Transparent, 1.0);
			SfCategoryAxis(chartSeries.YAxis, chartSeries.XAxis, xlsioCatAxis, xlsioValAxis, condition: false);
			LineInfo lineInfo = new LineInfo();
			lineInfo.BackColor = SfColor(Color.FromArgb(255, 255, 255), 1.0);
			lineInfo.ForeColor = SfColor(Color.FromArgb(255, 255, 255), 1.0);
			chartSeries.XAxis.LineType = lineInfo;
			m_hasNewSeries = true;
			m_sfChart.Series.Add(chartSeries);
			chartSeries.LegendItem.Visible = false;
			m_newSeriesIndex = m_sfChart.Series.Count - 1;
			sfCatAxis.OpposedPosition = false;
			sfCatAxis.Font = new Font(xlsioCatAxis.Font.FontName, 0.01f);
		}
	}

	internal void SfChartAxisTitle(ChartAxis chartAxis, ChartAxisImpl xlsioChartAxis)
	{
		ChartTextAreaImpl chartTextAreaImpl = xlsioChartAxis.TitleArea as ChartTextAreaImpl;
		ChartTitle chartAxisTitle = chartAxis.ChartAxisTitle;
		SetTitleBorder(chartTextAreaImpl.FrameFormat.Border as ChartBorderImpl, chartAxisTitle);
		if (chartTextAreaImpl.RichText != null && chartTextAreaImpl.RichText.FormattingRuns.Length > 1)
		{
			SetTextBlockInlines(chartTextAreaImpl, chartAxisTitle);
		}
		else
		{
			SfTextBlock(chartAxisTitle, chartTextAreaImpl);
			chartAxisTitle.Text = ((xlsioChartAxis.Title != null) ? xlsioChartAxis.Title : "Axis Title");
		}
		SetTransformAndBackGround(chartAxisTitle, chartTextAreaImpl, xlsioChartAxis);
		chartAxis.Title = chartAxisTitle.Text;
		chartAxis.TitleColor = chartAxisTitle.ForeColor;
		chartAxis.TitleFont = chartAxisTitle.Font;
	}

	internal void SfCategoryAxis(ChartAxis primaryValueAxis, ChartAxis categoryAxis, ChartCategoryAxisImpl xlsioCatAxis, ChartValueAxisImpl xlsioValAxis, bool condition)
	{
		SfCategoryAxisCommon(primaryValueAxis, categoryAxis, xlsioCatAxis, xlsioValAxis, condition);
		if (xlsioCatAxis.IsAutoTextRotation)
		{
			categoryAxis.LabelIntersectAction = ChartLabelIntersectAction.Rotate;
		}
		categoryAxis.Range.Interval = xlsioCatAxis.TickLabelSpacing;
		if (xlsioCatAxis.HasMinorGridLines)
		{
			categoryAxis.SmallTicksPerInterval = 1;
		}
		if (xlsioCatAxis.IsBetween)
		{
			categoryAxis.LabelPlacement = ChartAxisLabelPlacement.BetweenTicks;
		}
	}

	internal void SfCategoryAxis3D(ChartAxis categoryAxis, ChartCategoryAxisImpl xlsioCatAxis, ChartValueAxisImpl xlsioValAxis)
	{
		SfCategoryAxisCommon(null, categoryAxis, xlsioCatAxis, xlsioValAxis, condition: true);
		categoryAxis.Range.Interval = xlsioCatAxis.TickLabelSpacing;
		if (xlsioCatAxis.IsBetween)
		{
			categoryAxis.LabelPlacement = ChartAxisLabelPlacement.BetweenTicks;
		}
	}

	private void SfLogerthmicAxisCommon(ChartAxis sfaxis, ChartValueAxisImpl xlsiovalue, ChartCategoryAxisImpl xlsiocat)
	{
		if (!xlsiocat.IsAutoCross)
		{
			sfaxis.Origin = xlsiocat.CrossesAt - 2.0 + 0.5;
		}
		CheckAndApplyAxisLineStyle(xlsiovalue.Border as ChartBorderImpl, out var lineStyle, xlsiovalue.ParentChart, ChartElementsEnum.AxisLine);
		sfaxis.GridLineType = lineStyle;
		SfTickLines(xlsiovalue, sfaxis);
		if (!xlsiovalue.HasMajorGridLines)
		{
			sfaxis.DrawGrid = false;
		}
		if (xlsiovalue.HasAxisTitle)
		{
			SfChartAxisTitle(sfaxis, xlsiovalue);
		}
		sfaxis.LabelRotateAngle = xlsiovalue.TextRotationAngle;
		if (((xlsiovalue.TickLabelPosition == OfficeTickLabelPosition.TickLabelPosition_High || xlsiocat.ReversePlotOrder) && (xlsiovalue.TickLabelPosition != OfficeTickLabelPosition.TickLabelPosition_High || !xlsiocat.ReversePlotOrder)) || xlsiocat.IsMaxCross)
		{
			sfaxis.OpposedPosition = true;
		}
		TrySetValueAxisNumberFormat(xlsiovalue, sfaxis);
		if (xlsiovalue.ReversePlotOrder)
		{
			sfaxis.Inversed = true;
		}
		if (xlsiovalue.NumberFormat != "General")
		{
			sfaxis.Format = xlsiovalue.NumberFormat;
		}
	}

	internal void SfLogerthmicAxis(ChartAxis sfaxis, ChartValueAxisImpl xlsiovalue, ChartCategoryAxisImpl xlsiocat)
	{
		SfLogerthmicAxisCommon(sfaxis, xlsiovalue, xlsiocat);
		if (xlsiovalue.HasMinorGridLines)
		{
			sfaxis.SmallTicksPerInterval = 3;
		}
		sfaxis.LogBase = (int)xlsiovalue.LogBase;
		if (xlsiovalue.MajorUnit > 0.0)
		{
			sfaxis.Range.Interval = xlsiovalue.MajorUnit;
			sfaxis.LogBase = (int)xlsiovalue.MajorUnit;
		}
		if (!xlsiovalue.IsAutoMax)
		{
			sfaxis.Range.Max = xlsiovalue.MaximumValue;
			sfaxis.IsMaxSet = true;
		}
		if (!xlsiovalue.IsAutoMin)
		{
			sfaxis.Range.Min = xlsiovalue.MinimumValue;
			sfaxis.IsMinSet = true;
		}
	}

	internal void SfLogerthmicAxis3D(ChartAxis sfaxis, ChartValueAxisImpl xlsiovalue, ChartCategoryAxisImpl xlsiocat)
	{
		SfLogerthmicAxisCommon(sfaxis, xlsiovalue, xlsiocat);
		if (xlsiovalue.HasMinorGridLines)
		{
			sfaxis.SmallTicksPerInterval = 3;
		}
		if (xlsiovalue.MajorUnit > 0.0)
		{
			sfaxis.Range.Interval = xlsiovalue.MajorUnit;
		}
	}

	internal void SfDateTimeAxis(ChartAxis sfDateTimeAxis, ChartCategoryAxisImpl xlsioCatAxis, ChartValueAxisImpl xlsioValAxis)
	{
		SfCategoryAxisCommon(null, sfDateTimeAxis, xlsioCatAxis, xlsioValAxis, condition: true);
		if (xlsioCatAxis.IsAutoTextRotation)
		{
			sfDateTimeAxis.LabelIntersectAction = ChartLabelIntersectAction.None;
		}
		if (DateTimeIntervalType != 0)
		{
			sfDateTimeAxis.IntervalType = DateTimeIntervalType;
		}
		else if (!xlsioCatAxis.MajorUnitScaleIsAuto)
		{
			switch ((int)xlsioCatAxis.MajorUnitScale)
			{
			case 0:
				sfDateTimeAxis.IntervalType = ChartDateTimeIntervalType.Days;
				break;
			case 1:
				sfDateTimeAxis.IntervalType = ChartDateTimeIntervalType.Months;
				break;
			case 2:
				sfDateTimeAxis.IntervalType = ChartDateTimeIntervalType.Years;
				break;
			}
		}
		if (!xlsioCatAxis.BaseUnitIsAuto)
		{
			switch ((int)xlsioCatAxis.BaseUnit)
			{
			case 0:
				sfDateTimeAxis.IntervalType = ChartDateTimeIntervalType.Days;
				break;
			case 1:
				sfDateTimeAxis.IntervalType = ChartDateTimeIntervalType.Months;
				break;
			case 2:
				sfDateTimeAxis.IntervalType = ChartDateTimeIntervalType.Years;
				break;
			}
		}
		if (!xlsioCatAxis.IsAutoMajor)
		{
			sfDateTimeAxis.Range.Interval = xlsioCatAxis.MajorUnit;
		}
		else if (DateTimeIntervalType != 0 || sfDateTimeAxis.IntervalType != 0)
		{
			sfDateTimeAxis.Range.Interval = 1.0;
		}
		if (sfDateTimeAxis.IntervalType != 0 && !xlsioCatAxis.IsAutoMinor && !xlsioCatAxis.MinorUnitScaleIsAuto)
		{
			int num = (int)(sfDateTimeAxis.IntervalType - 5);
			int minorUnitScale = (int)xlsioCatAxis.MinorUnitScale;
			if (num == minorUnitScale && minorUnitScale != 0)
			{
				sfDateTimeAxis.SmallTicksPerInterval = num / minorUnitScale - 1;
			}
			else
			{
				switch (num)
				{
				case 1:
					if (minorUnitScale == 0)
					{
						sfDateTimeAxis.SmallTicksPerInterval = (int)(sfDateTimeAxis.Range.Interval * 30.0 / xlsioCatAxis.MinorUnit);
					}
					break;
				case 2:
					switch (minorUnitScale)
					{
					case 0:
						sfDateTimeAxis.SmallTicksPerInterval = (int)(sfDateTimeAxis.Range.Interval * 365.0 / xlsioCatAxis.MinorUnit);
						break;
					case 1:
						sfDateTimeAxis.SmallTicksPerInterval = (int)(sfDateTimeAxis.Range.Interval * 12.0 / xlsioCatAxis.MinorUnit);
						break;
					}
					break;
				}
			}
		}
		if (!xlsioCatAxis.IsAutoMax)
		{
			sfDateTimeAxis.Range.Max = xlsioCatAxis.MaximumValue;
		}
		if (!xlsioCatAxis.IsAutoMin)
		{
			sfDateTimeAxis.Range.Min = xlsioCatAxis.MinimumValue;
		}
		if (sfDateTimeAxis.IntervalType == ChartDateTimeIntervalType.Days)
		{
			sfDateTimeAxis.Offset = 0.5;
			sfDateTimeAxis.LabelsOffset = 0.5;
		}
	}

	internal void SfDateTimeAxis3D(ChartAxis sfDateTimeAxis, ChartCategoryAxisImpl xlsioCatAxis, ChartValueAxisImpl xlsioValAxis)
	{
		SfCategoryAxisCommon(null, sfDateTimeAxis, xlsioCatAxis, xlsioValAxis, condition: true);
		if (DateTimeIntervalType != 0)
		{
			sfDateTimeAxis.IntervalType = DateTimeIntervalType;
		}
		else if (!xlsioCatAxis.MajorUnitScaleIsAuto)
		{
			switch ((int)xlsioCatAxis.MajorUnitScale)
			{
			case 0:
				sfDateTimeAxis.IntervalType = ChartDateTimeIntervalType.Days;
				break;
			case 1:
				sfDateTimeAxis.IntervalType = ChartDateTimeIntervalType.Months;
				break;
			case 2:
				sfDateTimeAxis.IntervalType = ChartDateTimeIntervalType.Years;
				break;
			}
		}
		if (!xlsioCatAxis.IsAutoMajor)
		{
			sfDateTimeAxis.Range.Interval = xlsioCatAxis.MajorUnit;
		}
		else if (DateTimeIntervalType != 0)
		{
			sfDateTimeAxis.Range.Interval = 1.0;
		}
		if (sfDateTimeAxis.IntervalType == ChartDateTimeIntervalType.Auto || xlsioCatAxis.IsAutoMinor || xlsioCatAxis.MinorUnitScaleIsAuto)
		{
			return;
		}
		int num = (int)(sfDateTimeAxis.IntervalType - 5);
		int minorUnitScale = (int)xlsioCatAxis.MinorUnitScale;
		if (num == minorUnitScale)
		{
			sfDateTimeAxis.SmallTicksPerInterval = num / minorUnitScale - 1;
			return;
		}
		switch (num)
		{
		case 1:
			if (minorUnitScale == 0)
			{
				sfDateTimeAxis.SmallTicksPerInterval = (int)(sfDateTimeAxis.Range.Interval * 30.0 / xlsioCatAxis.MinorUnit);
			}
			break;
		case 2:
			switch (minorUnitScale)
			{
			case 0:
				sfDateTimeAxis.SmallTicksPerInterval = (int)(sfDateTimeAxis.Range.Interval * 365.0 / xlsioCatAxis.MinorUnit);
				break;
			case 1:
				sfDateTimeAxis.SmallTicksPerInterval = (int)(sfDateTimeAxis.Range.Interval * 12.0 / xlsioCatAxis.MinorUnit);
				break;
			}
			break;
		}
	}

	private void SfTickLines(ChartAxisImpl xlsioAxis, ChartAxis sfAxis)
	{
		if (xlsioAxis.MinorTickMark != 0)
		{
			if (sfAxis.Orientation == ChartOrientation.Horizontal)
			{
				sfAxis.SmallTickSize = new Size(1, 2);
			}
			else
			{
				sfAxis.SmallTickSize = new Size(2, 1);
			}
		}
		if (xlsioAxis.MajorTickMark != 0)
		{
			if (sfAxis.Orientation == ChartOrientation.Horizontal)
			{
				sfAxis.TickSize = new Size(1, 2);
			}
			else
			{
				sfAxis.TickSize = new Size(2, 1);
			}
			sfAxis.TickColor = sfAxis.LineType.ForeColor;
		}
	}

	internal void SfSecondaryAxis(ChartSerieImpl xlsioSerie, ChartImpl xlsioChart, ChartSeries chartSerie)
	{
		if (!xlsioSerie.UsePrimaryAxis && !SecondayAxisAchived)
		{
			ChartAxis value = (chartSerie.YAxis = new ChartAxis
			{
				DrawGrid = false,
				OpposedPosition = true,
				Orientation = ChartOrientation.Vertical
			});
			if ((xlsioChart.SecondaryValueAxis as ChartAxisImpl).Deleted)
			{
				chartSerie.YAxis.IsVisible = false;
			}
			if ((xlsioChart.SecondaryCategoryAxis as ChartCategoryAxisImpl).IsChartBubbleOrScatter)
			{
				chartSerie.XAxis = new ChartAxis
				{
					DrawGrid = false,
					OpposedPosition = true,
					ValueType = ChartValueType.Double
				};
			}
			else
			{
				chartSerie.XAxis = new ChartAxis
				{
					DrawGrid = false,
					OpposedPosition = true,
					ValueType = ChartValueType.Category
				};
			}
			if ((xlsioChart.SecondaryCategoryAxis as ChartAxisImpl).Deleted)
			{
				chartSerie.XAxis.IsVisible = false;
			}
			if ((xlsioChart.SecondaryCategoryAxis as ChartCategoryAxisImpl).CategoryType == OfficeCategoryType.Time)
			{
				chartSerie.XAxis.IsDateTimeCategoryAxis = true;
			}
			chartSerie.ParentChart.Axes.Add(value);
			chartSerie.ParentChart.Axes.Add(chartSerie.XAxis);
			SfSecondaryAxisCommon(xlsioChart, chartSerie.XAxis, chartSerie.YAxis);
		}
	}

	protected void SfSecondaryAxisCommon(ChartImpl xlsioChart, ChartAxis sfXAxis, ChartAxis sfYAxis)
	{
		ChartValueAxisImpl chartValueAxisImpl = xlsioChart.SecondaryValueAxis as ChartValueAxisImpl;
		ChartCategoryAxisImpl chartCategoryAxisImpl = xlsioChart.SecondaryCategoryAxis as ChartCategoryAxisImpl;
		if (SecondayAxisAchived)
		{
			return;
		}
		if (sfYAxis != null)
		{
			if (sfYAxis.IsVisible)
			{
				SfNumericalAxis(sfYAxis, chartValueAxisImpl, chartCategoryAxisImpl, condition: true);
				if (!chartCategoryAxisImpl.IsMaxCross)
				{
					sfYAxis.OpposedPosition = false;
				}
			}
			else if (chartValueAxisImpl.ReversePlotOrder)
			{
				sfYAxis.Inversed = true;
			}
		}
		if (sfXAxis != null)
		{
			if (sfXAxis.IsVisible)
			{
				if (sfXAxis.ValueType == ChartValueType.Double)
				{
					SfNumericalAxis(sfXAxis, chartCategoryAxisImpl, chartValueAxisImpl, condition: true);
				}
				else
				{
					SfCategoryAxis(sfYAxis, sfXAxis, chartCategoryAxisImpl, chartValueAxisImpl, condition: true);
				}
			}
			else
			{
				if (chartCategoryAxisImpl.ReversePlotOrder)
				{
					sfXAxis.Inversed = true;
				}
				sfXAxis.Origin = 0.0;
				if (sfXAxis != null)
				{
					if (chartCategoryAxisImpl.IsBetween)
					{
						sfXAxis.LabelPlacement = ChartAxisLabelPlacement.BetweenTicks;
					}
				}
			}
		}
		if (sfXAxis != null || sfYAxis != null)
		{
			SecondayAxisAchived = true;
		}
	}

	private void CheckAndApplyAxisLineStyle(ChartBorderImpl border, out LineInfo lineStyle, ChartImpl chart, ChartElementsEnum elementEnum)
	{
		lineStyle = new LineInfo();
		Color empty = Color.Empty;
		float width = 0f;
		if (border.LinePattern != OfficeChartLinePattern.None)
		{
			width = GetBorderThickness(border);
			if (border.IsAutoLineColor && !border.HasGradientFill)
			{
				empty = ((parentWorkbook.Version == OfficeVersion.Excel97to2003) ? Color.FromArgb(255, 0, 0, 0) : ((!TryAndGetColorBasedOnElement(elementEnum, chart, out var color)) ? Color.FromArgb(255, 134, 134, 134) : color));
				width = 1f;
			}
			else
			{
				empty = Color.FromArgb(GetBrushFromBorder(border).BackColor.ToArgb());
			}
			lineStyle.ForeColor = SfColor(empty);
			lineStyle.BackColor = SfColor(empty);
		}
		else
		{
			lineStyle.ForeColor = SfColor(Color.FromArgb(255, 255, 255), 1.0);
			lineStyle.BackColor = SfColor(Color.FromArgb(255, 255, 255), 1.0);
		}
		lineStyle.Width = width;
	}

	internal bool GetBrushForTextElements(ChartBorderImpl border, ChartLineInfo drawBorder)
	{
		BrushInfo brushInfo = null;
		float num = 0f;
		if (border.LinePattern != OfficeChartLinePattern.None)
		{
			if (border.HasLineProperties)
			{
				num = GetBorderThickness(border);
			}
			brushInfo = ((border.IsAutoLineColor && !border.HasGradientFill) ? null : GetBrushFromBorder(border));
		}
		else
		{
			brushInfo = null;
		}
		if (brushInfo != null)
		{
			if (num != 0f)
			{
				num = ((num < 1f) ? 1f : (num + 1f));
			}
			drawBorder.Width = num;
			drawBorder.Color = brushInfo.BackColor;
		}
		return brushInfo != null;
	}

	private IDictionary<char, Color> GetNumberFormatColorConverter(string numberFormat, Color defaultColor)
	{
		return null;
	}

	internal void SfChartDataLabel(ChartSerieImpl serie, ChartSeries sfChartSeries)
	{
		ChartDataLabelsImpl chartDataLabelsImpl = null;
		if ((serie.DataPoints.DefaultDataPoint as ChartDataPointImpl).HasDataLabels)
		{
			chartDataLabelsImpl = serie.DataPoints.DefaultDataPoint.DataLabels as ChartDataLabelsImpl;
		}
		bool flag = sfChartSeries.ToString().Contains("Stacking");
		ChartSeriesModel chartSeriesModel = sfChartSeries.SeriesModel as ChartSeriesModel;
		List<int> list = null;
		bool flag2 = sfChartSeries.Type == ChartSeriesType.Pie;
		bool flag3 = (serie.DataPoints as ChartDataPointsCollection).CheckDPDataLabels();
		if ((chartDataLabelsImpl != null && !chartDataLabelsImpl.IsDelete && (chartDataLabelsImpl.IsCategoryName || chartDataLabelsImpl.IsSeriesName || chartDataLabelsImpl.IsValue || chartDataLabelsImpl.IsPercentage || chartDataLabelsImpl.IsValueFromCells)) || (serie.SerieFormat.IsMarkerSupported && ((serie.SerieFormat as ChartSerieDataFormatImpl).MarkerFormat.MarkerType != 0 || (serie.SerieType.ToString().Contains("Scatter") && (serie.SerieFormat as ChartSerieDataFormatImpl).IsLine))) || flag3)
		{
			ChartStyleInfo style = sfChartSeries.Style;
			if (chartDataLabelsImpl != null && !chartDataLabelsImpl.IsDelete)
			{
				if (chartDataLabelsImpl.IsCategoryName || chartDataLabelsImpl.IsSeriesName || chartDataLabelsImpl.IsValue || chartDataLabelsImpl.IsPercentage || chartDataLabelsImpl.IsValueFromCells)
				{
					style.DisplayText = true;
				}
				SetLabelPosition(sfChartSeries, style, chartDataLabelsImpl);
			}
			if (!style.DisplayText && flag3)
			{
				style.DisplayText = true;
				style.TextOrientation = ChartTextOrientation.Center;
			}
			ChartSerieDataFormatImpl chartSerieDataFormatImpl = serie.SerieFormat as ChartSerieDataFormatImpl;
			if (sfChartSeries.ParentChart.AllowGapForEmptyPoints && sfChartSeries.EmptyPointValue == EmptyPointValue.Average && chartSeriesModel.ContainsAnyEmptyPoint())
			{
				list = new List<int>(chartSeriesModel.Count);
				for (int i = 0; i < chartSeriesModel.Count; i++)
				{
					if (chartSeriesModel.GetEmpty(i))
					{
						list.Add(i);
					}
				}
			}
			if (sfChartSeries.ToString().Contains("Bubble") && !serie.SerieFormat.CommonSerieOptions.ShowNegativeBubbles)
			{
				if (list == null)
				{
					list = new List<int>(chartSeriesModel.Count);
				}
				for (int j = 0; j < chartSeriesModel.Count; j++)
				{
					if (chartSeriesModel.GetY(j)[1].Equals(0.0))
					{
						list.Add(j);
					}
				}
			}
			if (chartSerieDataFormatImpl.IsMarkerSupported)
			{
				SetMarkerFormattings(serie, list, chartSerieDataFormatImpl, sfChartSeries, style);
			}
			if (!((chartDataLabelsImpl != null && !chartDataLabelsImpl.IsDelete) || flag3))
			{
				return;
			}
			if ((chartDataLabelsImpl?.IsDelete ?? true) && flag3)
			{
				chartDataLabelsImpl = serie.DataPoints.DefaultDataPoint.DataLabels as ChartDataLabelsImpl;
			}
			float value = 0f;
			Color color = ((!chartDataLabelsImpl.IsAutoColor || !TryAndGetColorBasedOnElement(ChartElementsEnum.ChartAreaFill, serie.ParentChart, out color) || color.R != color.G || color.B != color.R || color.R != 0) ? chartDataLabelsImpl.RGBColor : Color.Transparent);
			Font drawingFont = GetDrawingFont(chartDataLabelsImpl.FontName, (float)chartDataLabelsImpl.Size, chartDataLabelsImpl.TextArea);
			style.TextColor = SfColor(color, color.Equals(Color.Transparent) ? 1.0 : 0.0);
			ChartFrameFormatImpl chartFrameFormatImpl = chartDataLabelsImpl.FrameFormat as ChartFrameFormatImpl;
			if (!chartFrameFormatImpl.IsAutomaticFormat && chartFrameFormatImpl.Interior.Pattern != 0)
			{
				BrushInfo brushFromDataFormat = GetBrushFromDataFormat(chartFrameFormatImpl);
				style.DrawTextShape = true;
				if (brushFromDataFormat != null)
				{
					style.TextShape.Color = brushFromDataFormat.BackColor;
				}
			}
			if (GetBrushForTextElements(chartFrameFormatImpl.Border as ChartBorderImpl, style.TextShape.Border))
			{
				style.TextShape.BorderWidth = style.TextShape.Border.Width;
				style.TextShape.BorderColor = style.TextShape.Border.Color;
				style.DrawTextShape = true;
			}
			else if (style.DrawTextShape)
			{
				style.TextShape.BorderColor = Color.Transparent;
				style.TextShape.BorderWidth = 0f;
			}
			LabelConvertor labelConvertor = new LabelConvertor();
			labelConvertor.BlankIndexes = list;
			labelConvertor.NumberFormatApplyEvent += ApplyNumberFormat;
			ChartDataLabelsImpl dataLabelsImpl = chartDataLabelsImpl;
			DataLabelSetting commonDataLabelSetting = new DataLabelSetting(dataLabelsImpl, flag2, style.TextOrientation);
			IDictionary<int, string> tempDataLabelsResult = sfChartSeries.TempDataLabelsResult;
			Dictionary<int, ChartDataPointImpl> dictionary = (serie.DataPoints as ChartDataPointsCollection).m_hashDataPoints.Where((KeyValuePair<int, ChartDataPointImpl> x) => x.Value.HasDataLabels).ToDictionary((KeyValuePair<int, ChartDataPointImpl> x) => x.Key, (KeyValuePair<int, ChartDataPointImpl> x) => x.Value);
			labelConvertor.ValueFromCells = ((serie.DataLabelCellsValues != null && serie.DataLabelCellsValues.Count > 0) ? serie.DataLabelCellsValues : null);
			labelConvertor.SeriesName = GetSerieName(serie);
			labelConvertor.CategoryNames = new object[chartSeriesModel.Count];
			if (sfChartSeries.Type == ChartSeriesType.Candle)
			{
				_ = 1;
			}
			else
				_ = sfChartSeries.Type == ChartSeriesType.HiLo;
			bool flag4 = chartSeriesModel.Count <= 0 || !tempDataLabelsResult.ContainsKey(0) || tempDataLabelsResult[0] == null || tempDataLabelsResult[0] == null;
			labelConvertor.IsFunnelLabel = sfChartSeries.Type == ChartSeriesType.ColumnRange && sfChartSeries.Rotate;
			for (int k = 0; k < chartSeriesModel.Count; k++)
			{
				if ((!labelConvertor.IsFunnelLabel && flag4) || sfChartSeries.Type == ChartSeriesType.Pie)
				{
					labelConvertor.CategoryNames[k] = chartSeriesModel.GetCategoryOrX(k);
				}
				else if (tempDataLabelsResult.ContainsKey(k))
				{
					labelConvertor.CategoryNames[k] = tempDataLabelsResult[k];
				}
			}
			if (flag2)
			{
				for (int l = 0; l < chartSeriesModel.Count; l++)
				{
					double num = chartSeriesModel.GetY(l)[0];
					if (!double.IsNaN(num))
					{
						labelConvertor.Percentage += num;
					}
				}
			}
			if (chartDataLabelsImpl.IsSourceLinked && serie.ValuesIRange != null && !(serie.ValuesIRange is ExternalRange) && IsRangesCollectionContainsExternalRange(serie))
			{
				commonDataLabelSetting.NumberFormat = serie.ValuesIRange.Cells[0].NumberFormat;
				commonDataLabelSetting.IsSourceLinked = true;
			}
			else if (chartDataLabelsImpl.NumberFormat != null && chartDataLabelsImpl.NumberFormat.ToLower() != "General".ToLower())
			{
				commonDataLabelSetting.NumberFormat = chartDataLabelsImpl.NumberFormat;
			}
			else
			{
				commonDataLabelSetting.NumberFormat = "General";
			}
			labelConvertor.ColorOnNumFmts = GetNumberFormatColorConverter(commonDataLabelSetting.NumberFormat, chartDataLabelsImpl.RGBColor);
			labelConvertor.BorderColor = style.TextShape.BorderColor;
			labelConvertor.BorderWidth = style.TextShape.BorderWidth;
			labelConvertor.Color = style.TextShape.Color;
			if (dictionary.Count > 0)
			{
				labelConvertor.Fonts = new Dictionary<int, Font>(dictionary.Count);
				labelConvertor.FillColors = new Dictionary<int, Color>(dictionary.Count);
				labelConvertor.FontColors = new Dictionary<int, Color>(dictionary.Count);
				labelConvertor.BorderColors = new Dictionary<int, Color>(dictionary.Count);
				labelConvertor.BorderWidths = new Dictionary<int, float>(dictionary.Count);
				ChartLineInfo chartLineInfo = new ChartLineInfo();
				foreach (KeyValuePair<int, ChartDataPointImpl> item in dictionary)
				{
					if (item.Key >= chartSeriesModel.Count)
					{
						continue;
					}
					ChartDataLabelsImpl chartDataLabelsImpl2 = item.Value.DataLabels as ChartDataLabelsImpl;
					chartFrameFormatImpl = item.Value.DataLabels.FrameFormat as ChartFrameFormatImpl;
					GetBrushForTextElements(chartFrameFormatImpl.Border as ChartBorderImpl, chartLineInfo);
					if (chartDataLabelsImpl2.IsDelete)
					{
						labelConvertor.FillColors.Add(item.Value.Index, Color.Empty);
						labelConvertor.BorderColors.Add(item.Value.Index, Color.Empty);
						labelConvertor.BorderWidths.Add(item.Value.Index, 0f);
					}
					else if (!chartFrameFormatImpl.IsAutomaticFormat)
					{
						labelConvertor.FillColors.Add(item.Value.Index, (chartFrameFormatImpl.Interior.Pattern != 0) ? GetBrushFromDataFormat(chartFrameFormatImpl).BackColor : Color.Empty);
						labelConvertor.BorderColors.Add(item.Value.Index, chartLineInfo.Color);
						labelConvertor.BorderWidths.Add(item.Value.Index, chartLineInfo.Width);
					}
					else if (style.DrawTextShape)
					{
						labelConvertor.FillColors.Add(item.Value.Index, style.TextShape.Color);
						labelConvertor.BorderColors.Add(item.Value.Index, style.TextShape.BorderColor);
						labelConvertor.BorderWidths.Add(item.Value.Index, value);
					}
					if (chartDataLabelsImpl2.ShowTextProperties)
					{
						string fontName = chartDataLabelsImpl2.FontName;
						labelConvertor.FontColors.Add(item.Value.Index, SfColor(chartDataLabelsImpl2.TextArea.RGBColor));
						Font drawingFont2 = GetDrawingFont(fontName, (float)chartDataLabelsImpl2.Size, chartDataLabelsImpl2.TextArea);
						if (item.Value.DataLabels != null && CheckUnicode(item.Value.DataLabels.Text))
						{
							fontName = SwitchFonts(item.Value.DataLabels.Text, fontName);
							drawingFont2 = GetDrawingFont(fontName, (float)chartDataLabelsImpl2.Size, chartDataLabelsImpl2.TextArea);
						}
						labelConvertor.Fonts.Add(item.Value.Index, drawingFont2);
					}
					else if (chartDataLabelsImpl2.IsDelete)
					{
						labelConvertor.FontColors.Add(item.Value.Index, Color.Empty);
					}
					else
					{
						labelConvertor.FontColors.Add(item.Value.Index, style.TextColor);
						labelConvertor.Fonts.Add(item.Value.Index, drawingFont);
					}
					ChartTextOrientation labelPosition = GetLabelPosition(sfChartSeries, chartDataLabelsImpl2);
					DataLabelSetting value2 = new DataLabelSetting(chartDataLabelsImpl2, flag2, style.TextOrientation);
					if (style.TextOrientation != labelPosition)
					{
						value2.TextOrientation = labelPosition;
					}
					if (value2.IsSourceLinked && serie.ValuesIRange != null && !(serie.ValuesIRange is ExternalRange) && chartDataLabelsImpl2.NumberFormat != null)
					{
						value2.NumberFormat = serie.ValuesIRange.Cells[item.Value.Index].NumberFormat;
					}
					else
					{
						value2.NumberFormat = commonDataLabelSetting.NumberFormat;
					}
					labelConvertor.DataLabelSettings.Add(item.Value.Index, value2);
				}
			}
			if (chartSeriesModel.Count >= 1 && (labelConvertor.Fonts == null || labelConvertor.Fonts.Count != chartSeriesModel.Count) && drawingFont != null)
			{
				if (labelConvertor.Fonts == null)
				{
					labelConvertor.Fonts = new Dictionary<int, Font>(chartSeriesModel.Count);
				}
				for (int m = 0; chartSeriesModel.Count > m; m++)
				{
					if (!labelConvertor.Fonts.ContainsKey(m))
					{
						labelConvertor.Fonts.Add(m, drawingFont);
					}
				}
			}
			labelConvertor.CommonDataLabelSetting = commonDataLabelSetting;
			sfChartSeries.LabelConverterObject = labelConvertor;
			sfChartSeries.PrepareStyle += SeriesDataLabelsImportMethod;
		}
		else if (flag)
		{
			ChartStyleInfo style2 = sfChartSeries.Style;
			style2.DisplayText = false;
			style2.DrawTextShape = false;
		}
	}

	private bool IsRangesCollectionContainsExternalRange(ChartSerieImpl serie)
	{
		if (serie.Values is RangesCollection { InnerList: not null } rangesCollection && rangesCollection.InnerList.Count > 0)
		{
			if (rangesCollection.InnerList[0] is ExternalRange)
			{
				return false;
			}
			return true;
		}
		return true;
	}

	private int GetFontStyle(ChartTextAreaImpl label)
	{
		FontStyle fontStyle = FontStyle.Regular;
		if (label.Bold)
		{
			fontStyle |= FontStyle.Bold;
		}
		if (label.Italic)
		{
			fontStyle |= FontStyle.Italic;
		}
		if (label.Strikethrough)
		{
			fontStyle |= FontStyle.Strikeout;
		}
		if (label.Underline == OfficeUnderline.Single)
		{
			fontStyle |= FontStyle.Underline;
		}
		return (int)fontStyle;
	}

	protected void SeriesDataLabelsImportMethod(object sender, ChartPrepareStyleInfoEventArgs args)
	{
		ChartSeries chartSeries = sender as ChartSeries;
		ChartStyleInfo style = args.Style;
		int index = args.Index;
		chartSeries.LabelConverterObject.SetStyle(chartSeries, style, index, "", chartSeries.Points[index]);
	}

	protected void SeriesMarkerImportMethod(object sender, ChartPrepareStyleInfoEventArgs args)
	{
		ChartSeries chartSeries = sender as ChartSeries;
		ChartStyleInfo style = args.Style;
		int index = args.Index;
		chartSeries.MarkerConverterObject.SetSymbolStyle(chartSeries, style, index, "", chartSeries.Points[index]);
	}

	private void SetMarkerFormattings(ChartSerieImpl chartSerieImpl, List<int> averageIndexes, ChartSerieDataFormatImpl dataFormat, ChartSeries sfChartSerie, ChartStyleInfo info)
	{
		var enumerable = from _003C_003Eh__TransparentIdentifier0 in (chartSerieImpl.DataPoints as ChartDataPointsCollection).m_hashDataPoints.Select(delegate(KeyValuePair<int, ChartDataPointImpl> point)
			{
				KeyValuePair<int, ChartDataPointImpl> keyValuePair = point;
				return new
				{
					point = point,
					x = keyValuePair.Value.DataFormatOrNull
				};
			})
			where _003C_003Eh__TransparentIdentifier0.x != null && _003C_003Eh__TransparentIdentifier0.x.MarkerFormatOrNull != null && !_003C_003Eh__TransparentIdentifier0.x.IsAutoMarker
			select new
			{
				Key = _003C_003Eh__TransparentIdentifier0.point.Key,
				Value = _003C_003Eh__TransparentIdentifier0.x
			};
		if (dataFormat.MarkerFormatOrNull != null && !dataFormat.IsAutoMarker && dataFormat.MarkerFormat.MarkerType == OfficeChartMarkerType.None && !dataFormat.IsLine && enumerable.Count() == 0)
		{
			return;
		}
		bool flag = !IsChartEx && IsVaryColorSupported(chartSerieImpl);
		_ = sfChartSerie.Points.SeriesModel;
		int number = chartSerieImpl.Number;
		int count = dataFormat.ParentChart.Series.Count;
		if (dataFormat.MarkerFormatOrNull == null)
		{
			return;
		}
		MarkerSetting markerSettings = GetMarkerSettings(chartSerieImpl, dataFormat, null, number, count, isvaryColor: false);
		if (sfChartSerie.Type == ChartSeriesType.Scatter)
		{
			info.Symbol.Color = markerSettings.FillBrush;
		}
		if (dataFormat.MarkerFormat.MarkerType == OfficeChartMarkerType.None && enumerable.Count() == 0)
		{
			return;
		}
		bool flag2 = false;
		if (dataFormat.MarkerFormat.MarkerType == OfficeChartMarkerType.None && enumerable.Count() > 0)
		{
			foreach (var item in enumerable)
			{
				if (item.Value.MarkerFormat.MarkerType != 0)
				{
					flag2 = true;
					break;
				}
			}
			if (!flag2)
			{
				return;
			}
		}
		if (enumerable.Count() != 0 || flag)
		{
			MarkerConverter markerConverter = new MarkerConverter();
			markerConverter.CommonMarkerSetting = markerSettings;
			markerConverter.MarkerSettings = new Dictionary<int, MarkerSetting>(enumerable.Count());
			int count2 = sfChartSerie.Points.Count;
			int i;
			for (i = 0; i < count2; i++)
			{
				var anon = enumerable.FirstOrDefault(x => x.Key == i);
				if (flag || anon != null)
				{
					MarkerSetting markerSettings2 = GetMarkerSettings(chartSerieImpl, (anon != null) ? anon.Value : dataFormat, markerSettings, flag ? i : number, flag ? count2 : count, flag);
					markerConverter.MarkerSettings.Add(anon?.Key ?? i, markerSettings2);
				}
			}
			markerConverter.AverageMarkerIndexes = averageIndexes;
			sfChartSerie.MarkerConverterObject = markerConverter;
			sfChartSerie.PrepareStyle += SeriesMarkerImportMethod;
		}
		else if (averageIndexes != null)
		{
			MarkerConverter markerConverter2 = new MarkerConverter();
			markerConverter2.CommonMarkerSetting = markerSettings;
			markerConverter2.MarkerSettings = new Dictionary<int, MarkerSetting>(1);
			markerConverter2.AverageMarkerIndexes = averageIndexes;
			sfChartSerie.MarkerConverterObject = markerConverter2;
			sfChartSerie.PrepareStyle += SeriesMarkerImportMethod;
		}
		else
		{
			sfChartSerie.Style.Symbol.Shape = GetMarkerSymbolShape(markerSettings.MarkerTypeInInt);
			sfChartSerie.Style.Symbol.Border.Width = markerSettings.BorderThickness * 1.25f;
			sfChartSerie.Style.Symbol.Border.Color = markerSettings.BorderBrush;
			sfChartSerie.Style.Symbol.Color = markerSettings.FillBrush;
			sfChartSerie.Style.Symbol.Size = new Size(markerSettings.MarkerSize, markerSettings.MarkerSize);
		}
	}

	private MarkerSetting GetMarkerSettings(ChartSerieImpl chartSerieImpl, ChartSerieDataFormatImpl dataFormat, MarkerSetting parentMarkerSttings, int index, int count, bool isvaryColor)
	{
		MarkerSetting markerSetting = new MarkerSetting();
		ChartBorderImpl chartBorderImpl = chartSerieImpl.SerieFormat.LineProperties as ChartBorderImpl;
		short? value = null;
		if (chartBorderImpl.LineWeightString != null)
		{
			value = (short)chartBorderImpl.LineWeight;
		}
		if (dataFormat.IsAutoMarker && TryAndGetThicknessBasedOnElement(ChartElementsEnum.MarkerThickness, chartSerieImpl.ParentChart, out var thickness, value))
		{
			markerSetting.MarkerSize = (int)thickness;
		}
		else
		{
			markerSetting.MarkerSize = dataFormat.MarkerSize;
		}
		if (markerSetting.MarkerSize > 1)
		{
			markerSetting.MarkerSize--;
		}
		int num = (int)dataFormat.MarkerStyle;
		if (dataFormat.MarkerFormatOrNull == null || dataFormat.IsAutoMarker)
		{
			num = m_def_MarkerType_Array[(isvaryColor ? index : chartSerieImpl.Index) % m_def_MarkerType_Array.Length];
		}
		else if (dataFormat.MarkerStyle == OfficeChartMarkerType.None)
		{
			num = -1;
		}
		if (num == -1 && (!chartSerieImpl.SerieType.ToString().Contains("Scatter") || !dataFormat.IsLine))
		{
			markerSetting.FillBrush = SfColor(byte.MaxValue, byte.MaxValue, byte.MaxValue, 1.0);
			markerSetting.BorderBrush = SfColor(byte.MaxValue, byte.MaxValue, byte.MaxValue, 1.0);
			return markerSetting;
		}
		markerSetting.MarkerTypeInInt = num;
		bool flag = parentWorkbook.Version == OfficeVersion.Excel97to2003;
		Color color;
		Color color2 = ((!TryAndGetFillOrLineColorBasedOnPattern(chartSerieImpl.ParentChart, isLine: false, isvaryColor ? index : chartSerieImpl.Number, isvaryColor ? (count - 1) : (chartSerieImpl.ParentSeries.Count - 1), out color)) ? SfColor(dataFormat.ParentChart.GetChartColor(isvaryColor ? index : chartSerieImpl.Number, isvaryColor ? (count - 1) : (chartSerieImpl.ParentSeries.Count - 1), flag, isColorPalette: true)) : SfColor(color));
		if (!dataFormat.IsAutoMarker)
		{
			if (!dataFormat.MarkerFormat.IsNotShowInt)
			{
				if (flag || (((uint)dataFormat.MarkerFormat.FlagOptions & (true ? 1u : 0u)) != 0 && dataFormat.MarkerFormat.FillColorIndex == (ushort)dataFormat.MarkerBackColorObject.GetIndexed(parentWorkbook)))
				{
					markerSetting.FillBrush = SfColor(dataFormat.MarkerBackgroundColor);
				}
				else
				{
					markerSetting.FillBrush = color2;
				}
			}
			else
			{
				markerSetting.FillBrush = SfColor(byte.MaxValue, byte.MaxValue, byte.MaxValue, 1.0);
			}
			if (!dataFormat.MarkerFormat.IsNotShowBrd)
			{
				if (flag || ((dataFormat.MarkerFormat.FlagOptions & 2u) != 0 && dataFormat.MarkerFormat.HasLineProperties))
				{
					markerSetting.BorderBrush = SfColor(dataFormat.MarkerForegroundColor);
				}
				else
				{
					markerSetting.BorderBrush = color2;
				}
			}
		}
		else if (isvaryColor)
		{
			markerSetting.BorderBrush = color2;
			markerSetting.FillBrush = color2;
		}
		else if (parentMarkerSttings != null)
		{
			markerSetting.BorderBrush = parentMarkerSttings.BorderBrush;
			markerSetting.FillBrush = parentMarkerSttings.FillBrush;
		}
		else if (num == 4 || num == 5 || num == 9)
		{
			markerSetting.BorderBrush = color2;
			markerSetting.FillBrush = SfColor(byte.MaxValue, byte.MaxValue, byte.MaxValue, 1.0);
		}
		else
		{
			markerSetting.FillBrush = color2;
			markerSetting.BorderBrush = SfColor(byte.MaxValue, byte.MaxValue, byte.MaxValue, 1.0);
		}
		markerSetting.BorderThickness = (float)dataFormat.MarkerLineWidth;
		return markerSetting;
	}

	private void SetLabelPosition(ChartSeries sfChartSeries, object info, ChartDataLabelsImpl label)
	{
		bool flag = true;
		ChartSeriesType type = sfChartSeries.Type;
		if (type == ChartSeriesType.Area || type == ChartSeriesType.Radar || type == ChartSeriesType.StackingArea || type == ChartSeriesType.StackingArea100 || (type == ChartSeriesType.Pie && sfChartSeries.ConfigItems.PieItem.DoughnutCoeficient != 0f))
		{
			flag = false;
		}
		string text = type.ToString().ToLower();
		int position = (int)label.Position;
		if (position != 0 && position != 10 && flag)
		{
			switch (position)
			{
			case 2:
			case 9:
				if (type == ChartSeriesType.Pie)
				{
					sfChartSeries.Style.TextOrientation = ChartTextOrientation.Center;
					sfChartSeries.ConfigItems.PieItem.LabelStyle = ChartAccumulationLabelStyle.Inside;
				}
				else
				{
					sfChartSeries.Style.TextOrientation = ChartTextOrientation.RegionUp;
				}
				return;
			case 1:
			case 5:
				if (type == ChartSeriesType.Pie)
				{
					sfChartSeries.Style.TextOrientation = ChartTextOrientation.Up;
					sfChartSeries.ConfigItems.PieItem.LabelStyle = ChartAccumulationLabelStyle.Outside;
				}
				else if (text.Contains("bar"))
				{
					sfChartSeries.Style.TextOrientation = ChartTextOrientation.Right;
				}
				else
				{
					sfChartSeries.Style.TextOrientation = ChartTextOrientation.Up;
				}
				return;
			case 4:
				if (text.Contains("bar"))
				{
					sfChartSeries.Style.TextOrientation = ChartTextOrientation.RegionLeft;
				}
				else
				{
					sfChartSeries.Style.TextOrientation = ChartTextOrientation.RegionDown;
				}
				return;
			case 6:
				sfChartSeries.Style.TextOrientation = ChartTextOrientation.Down;
				return;
			case 8:
				sfChartSeries.Style.TextOrientation = ChartTextOrientation.Right;
				return;
			case 7:
				sfChartSeries.Style.TextOrientation = ChartTextOrientation.Left;
				return;
			}
			if (text.Contains("column") || text.Contains("bar") || text.Contains("pie"))
			{
				sfChartSeries.Style.TextOrientation = ChartTextOrientation.RegionCenter;
				if (text.Contains("pie"))
				{
					sfChartSeries.ConfigItems.PieItem.LabelStyle = ChartAccumulationLabelStyle.Inside;
				}
			}
			else
			{
				sfChartSeries.Style.TextOrientation = ChartTextOrientation.Center;
			}
		}
		else
		{
			switch (type)
			{
			case ChartSeriesType.Column:
			case ChartSeriesType.Radar:
				sfChartSeries.Style.TextOrientation = ChartTextOrientation.Up;
				break;
			case ChartSeriesType.Line:
			case ChartSeriesType.Spline:
			case ChartSeriesType.Scatter:
			case ChartSeriesType.Bar:
			case ChartSeriesType.HiLo:
			case ChartSeriesType.Candle:
				sfChartSeries.Style.TextOrientation = ChartTextOrientation.Right;
				break;
			case ChartSeriesType.StackingBar:
			case ChartSeriesType.Area:
			case ChartSeriesType.StackingArea:
			case ChartSeriesType.StackingColumn:
			case ChartSeriesType.StackingArea100:
			case ChartSeriesType.StackingBar100:
			case ChartSeriesType.StackingColumn100:
			case ChartSeriesType.ColumnRange:
				sfChartSeries.Style.TextOrientation = ChartTextOrientation.RegionCenter;
				break;
			case ChartSeriesType.Bubble:
				sfChartSeries.Style.TextOrientation = ChartTextOrientation.Center;
				break;
			case ChartSeriesType.Pie:
				sfChartSeries.Style.TextOrientation = ChartTextOrientation.Center;
				sfChartSeries.ConfigItems.PieItem.LabelStyle = ChartAccumulationLabelStyle.Inside;
				break;
			case ChartSeriesType.RotatedSpline:
			case ChartSeriesType.Gantt:
			case ChartSeriesType.RangeArea:
			case ChartSeriesType.SplineArea:
			case ChartSeriesType.StackingLine100:
			case ChartSeriesType.Funnel:
			case ChartSeriesType.Pyramid:
			case ChartSeriesType.HiLoOpenClose:
			case ChartSeriesType.StepLine:
			case ChartSeriesType.StepArea:
			case ChartSeriesType.Kagi:
			case ChartSeriesType.Renko:
			case ChartSeriesType.Polar:
				break;
			}
		}
	}

	private ChartTextOrientation GetLabelPosition(ChartSeries sfChartSeries, ChartDataLabelsImpl label)
	{
		bool flag = true;
		ChartSeriesType type = sfChartSeries.Type;
		if (type == ChartSeriesType.Area || type == ChartSeriesType.Radar || type == ChartSeriesType.StackingArea || type == ChartSeriesType.StackingArea100 || (type == ChartSeriesType.Pie && sfChartSeries.ConfigItems.PieItem.DoughnutCoeficient != 0f))
		{
			flag = false;
		}
		string text = type.ToString().ToLower();
		int position = (int)label.Position;
		if (position != 0 && position != 10 && flag)
		{
			switch (position)
			{
			case 2:
			case 9:
				if (type == ChartSeriesType.Pie)
				{
					return ChartTextOrientation.Center;
				}
				return ChartTextOrientation.RegionUp;
			case 1:
			case 5:
				if (type == ChartSeriesType.Pie)
				{
					return ChartTextOrientation.Up;
				}
				if (text.Contains("bar"))
				{
					return ChartTextOrientation.Right;
				}
				return ChartTextOrientation.Up;
			case 4:
				if (text.Contains("bar"))
				{
					return ChartTextOrientation.RegionLeft;
				}
				return ChartTextOrientation.RegionDown;
			case 6:
				return ChartTextOrientation.Down;
			case 8:
				return ChartTextOrientation.Right;
			case 7:
				return ChartTextOrientation.Left;
			default:
				if (text.Contains("column") || text.Contains("bar") || text.Contains("pie"))
				{
					return ChartTextOrientation.RegionCenter;
				}
				return ChartTextOrientation.Center;
			}
		}
		switch (type)
		{
		case ChartSeriesType.Column:
		case ChartSeriesType.Radar:
			return ChartTextOrientation.Up;
		case ChartSeriesType.Line:
		case ChartSeriesType.Spline:
		case ChartSeriesType.Scatter:
		case ChartSeriesType.Bar:
		case ChartSeriesType.HiLo:
		case ChartSeriesType.Candle:
			return ChartTextOrientation.Right;
		case ChartSeriesType.StackingBar:
		case ChartSeriesType.Area:
		case ChartSeriesType.StackingArea:
		case ChartSeriesType.StackingColumn:
		case ChartSeriesType.StackingArea100:
		case ChartSeriesType.StackingBar100:
		case ChartSeriesType.StackingColumn100:
		case ChartSeriesType.ColumnRange:
			return ChartTextOrientation.RegionCenter;
		case ChartSeriesType.Bubble:
			return ChartTextOrientation.Center;
		case ChartSeriesType.Pie:
			return ChartTextOrientation.Center;
		default:
			return sfChartSeries.Style.TextOrientation;
		}
	}

	internal static ChartSymbolShape GetMarkerSymbolShape(int markerTypeInInt)
	{
		ChartSymbolShape chartSymbolShape = ChartSymbolShape.None;
		return markerTypeInInt switch
		{
			1 => ChartSymbolShape.Square, 
			2 => ChartSymbolShape.Diamond, 
			3 => ChartSymbolShape.Triangle, 
			4 => ChartSymbolShape.Cross, 
			5 => ChartSymbolShape.ExcelStar, 
			6 => ChartSymbolShape.DowJonesLine, 
			7 => ChartSymbolShape.HorizLine, 
			8 => ChartSymbolShape.Circle, 
			9 => ChartSymbolShape.Plus, 
			_ => ChartSymbolShape.None, 
		};
	}

	internal void SetGapWidthandOverlap(ChartSeries seriesBase, ChartSerieImpl serie)
	{
		float overlap = float.NaN;
		if (!SetGapWidthOnSingleSeries(seriesBase, serie, out overlap, IsChart3D))
		{
			int gapWidth = serie.SerieFormat.CommonSerieOptions.GapWidth;
			if (IsChart3D && gapWidth > 0 && gapWidth < 200)
			{
				seriesBase.ParentChart.Spacing = (float)gapWidth * 0.25f;
			}
			else
			{
				seriesBase.ParentChart.Spacing = (float)gapWidth * 0.14f;
			}
		}
		if (!IsChart3D && !float.IsNaN(overlap))
		{
			overlap = ((!(overlap <= 0f)) ? 20f : (overlap * -1f / 2.5f));
			float num = 0.5f + (float)serie.SerieFormat.CommonSerieOptions.GapWidth / 1000f;
			seriesBase.ParentChart.SpacingBetweenPoints = overlap * num;
		}
	}

	private void UpdateSpacing(ChartSeries seriesBase, ChartSerieImpl serie)
	{
		double num = serie.SerieFormat.CommonSerieOptions.GapWidth;
		if (serie.ParentChart.ChartType == OfficeChartType.Pareto || serie.ParentChart.ChartType == OfficeChartType.Histogram)
		{
			num += 3.0;
		}
		if (num <= 50.0)
		{
			seriesBase.ParentChart.Spacing = (float)num * 0.7f;
		}
		else if (num > 50.0 && num <= 80.0)
		{
			seriesBase.ParentChart.Spacing = (float)num * 0.6f;
		}
		else if (num > 80.0 && num <= 90.0)
		{
			seriesBase.ParentChart.Spacing = (float)num * 0.55f;
		}
		else if (num > 90.0 && num <= 115.0)
		{
			seriesBase.ParentChart.Spacing = (float)num * 0.5f;
		}
		else if (num > 115.0 && num <= 135.0)
		{
			seriesBase.ParentChart.Spacing = (float)num * 0.44f;
		}
		else if (num > 135.0 && num <= 160.0)
		{
			seriesBase.ParentChart.Spacing = (float)num * 0.39f;
		}
		else if (num > 160.0 && num <= 200.0)
		{
			seriesBase.ParentChart.Spacing = (float)num * 0.35f;
		}
		else if (num > 200.0 && num <= 250.0)
		{
			seriesBase.ParentChart.Spacing = (float)num * 0.31f;
		}
		else if (num > 250.0 && num <= 300.0)
		{
			seriesBase.ParentChart.Spacing = (float)num * 0.27f;
		}
		else if (num > 300.0 && num <= 400.0)
		{
			seriesBase.ParentChart.Spacing = (float)num * 0.23f;
		}
		else if (num > 400.0 && num <= 450.0)
		{
			seriesBase.ParentChart.Spacing = (float)num * 0.18f;
		}
		else
		{
			seriesBase.ParentChart.Spacing = (float)num * 0.165f;
		}
	}

	internal bool IsBarChartAxis(ChartAxisImpl chartAxisImpl)
	{
		IList<ChartFormatImpl> chartFormats = (chartAxisImpl.Parent as ChartParentAxisImpl).ChartFormats;
		if (chartFormats != null)
		{
			return chartFormats.Count((ChartFormatImpl x) => x.FormatRecordType == TBIFFRecord.ChartBar && x.IsHorizontalBar) > 0;
		}
		return false;
	}

	private bool SetGapWidthOnSingleSeries(ChartSeries seriesBase, ChartSerieImpl serie, out float overlap, bool isChart3D)
	{
		OfficeChartType serieType = serie.SerieType;
		overlap = float.NaN;
		double num = (isChart3D ? 100 : serie.SerieFormat.CommonSerieOptions.Overlap);
		bool num2 = serie.ParentChart.IsStacked || (serie.ParentChart.ChartType == OfficeChartType.Combination_Chart && serieType.ToString().Contains("Stacked"));
		bool flag = IsChartEx || (serie.ParentChart.IsClustered && serie.ParentChart.Series.Count == 1) || (!isChart3D && serie.ParentSeries.Count > 1 && num == 100.0 && serie.SerieFormat.CommonSerieOptions.GapWidth == 0) || (serie.ParentChart.ChartType == OfficeChartType.Combination_Chart && serie.ParentSeries.Count((IOfficeChartSerie x) => x.SerieType == serieType && x.UsePrimaryAxis == serie.UsePrimaryAxis) == 1);
		_ = serie.SerieFormat.CommonSerieOptions.GapWidth;
		if (num2 || flag || (isChart3D && !serie.ParentChart.IsClustered))
		{
			UpdateSpacing(seriesBase, serie);
			return true;
		}
		overlap = (float)num;
		return false;
	}

	internal void SfChartTrendLine(ChartSerieImpl serieImpl, ChartSeries sfSerie)
	{
		if (serieImpl.TrendLines.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < serieImpl.TrendLines.Count; i++)
		{
			ChartTrendLineImpl chartTrendLineImpl = serieImpl.TrendLines[i] as ChartTrendLineImpl;
			Trendline trendline = new Trendline();
			if (chartTrendLineImpl.Type == OfficeTrendLineType.Moving_Average)
			{
				trendline.Type = TrendlineType.MovingAverage;
			}
			else
			{
				trendline.Type = (TrendlineType)Enum.Parse(typeof(TrendlineType), chartTrendLineImpl.Type.ToString());
			}
			trendline.Name = chartTrendLineImpl.Name;
			trendline.PolynomialOrder = chartTrendLineImpl.Order;
			DashStyle dashStyle = DashStyle.Solid;
			if (GetStrokeDashArrayValues(chartTrendLineImpl.Border.LinePattern, out dashStyle))
			{
				trendline.Style = dashStyle;
			}
			trendline.ForwardForecast = chartTrendLineImpl.Forward;
			trendline.BackwardForecast = chartTrendLineImpl.Backward;
			if (!chartTrendLineImpl.InterceptIsAuto && chartTrendLineImpl.Intercept == 0.0)
			{
				trendline.IsIntercept = true;
				trendline.Intercept = chartTrendLineImpl.Intercept;
			}
			ChartBorderImpl chartBorderImpl = chartTrendLineImpl.Border as ChartBorderImpl;
			BrushInfo brushInfo = null;
			brushInfo = ((!chartBorderImpl.AutoFormat || !TryAndGetColorBasedOnElement(ChartElementsEnum.OtherLines, serieImpl.ParentChart, out var color)) ? GetBrushFromBorder(chartBorderImpl) : new BrushInfo(SfColor(color)));
			trendline.Color = brushInfo.BackColor;
			if (chartBorderImpl.AutoFormat)
			{
				trendline.Width = 1f;
			}
			else
			{
				trendline.Width = GetBorderThickness(chartBorderImpl);
				if (trendline.Width != 0f && trendline.Width < 1f)
				{
					trendline.Width = 1f;
				}
			}
			sfSerie.Trendlines.Add(trendline);
		}
	}

	internal bool GetStrokeDashArrayValues(OfficeChartLinePattern linePattern, out DashStyle dashStyle)
	{
		dashStyle = DashStyle.Solid;
		switch ((int)linePattern)
		{
		case 0:
			dashStyle = DashStyle.Solid;
			break;
		case 1:
			dashStyle = DashStyle.Dash;
			break;
		case 2:
			dashStyle = DashStyle.Dot;
			break;
		case 3:
			dashStyle = DashStyle.DashDot;
			break;
		case 4:
			dashStyle = DashStyle.DashDotDot;
			break;
		case 9:
			dashStyle = DashStyle.CircleDot;
			break;
		case 10:
			dashStyle = DashStyle.LongDash;
			break;
		case 11:
			dashStyle = DashStyle.LongDashDot;
			break;
		case 12:
			dashStyle = DashStyle.LongDashDotDot;
			break;
		default:
			dashStyle = DashStyle.Solid;
			break;
		}
		return dashStyle != DashStyle.Solid;
	}

	internal void SfPloatArea(ChartControl sfChart, ChartFrameFormatImpl plotArea, ChartImpl chart)
	{
		chart.ChartType.ToString();
		if (!plotArea.IsAutomaticFormat)
		{
			sfChart.ChartArea.GridBackInterior = GetBrushFromDataFormat(plotArea);
		}
		else
		{
			double num = 1.0;
			if (TryAndGetColorBasedOnElement(ChartElementsEnum.PlotAreaFill, chart, out var color))
			{
				sfChart.ChartArea.GridBackInterior = new BrushInfo(SfColor(color, (chart.Style < 33 || (chart.Style > 100 && chart.Style < 133)) ? num : 0.0));
			}
			else
			{
				sfChart.ChartArea.GridBackInterior = new BrushInfo(SfColor(byte.MaxValue, byte.MaxValue, byte.MaxValue, num));
			}
		}
		ChartBorderImpl chartBorderImpl = plotArea.Border as ChartBorderImpl;
		if (chartBorderImpl.LinePattern != OfficeChartLinePattern.None)
		{
			double num2 = 0.0;
			if (chartBorderImpl.HasLineProperties)
			{
				num2 = GetBorderThickness(chartBorderImpl);
			}
			if (!chartBorderImpl.IsAutoLineColor || chartBorderImpl.HasGradientFill)
			{
				sfChart.ChartArea.BorderColor = GetLineColorFromBorder(chartBorderImpl);
			}
			else if (parentWorkbook.Version == OfficeVersion.Excel97to2003)
			{
				sfChart.ChartArea.BorderColor = SfColor(0, 0, 0);
			}
			else
			{
				sfChart.ChartArea.BorderColor = SfColor(byte.MaxValue, byte.MaxValue, byte.MaxValue, 1.0);
			}
			if (num2 < 1.0)
			{
				num2 = 1.0;
			}
			sfChart.ChartArea.BorderWidth = (int)Math.Round(num2);
		}
		else
		{
			sfChart.ChartArea.BorderColor = SfColor(byte.MaxValue, byte.MaxValue, byte.MaxValue, 1.0);
			sfChart.ChartArea.BorderWidth = 0;
		}
	}

	internal void SfChartArea(ChartControl sfChart, ChartFrameFormatImpl chartArea, ChartImpl chart)
	{
		Color color;
		if (!chartArea.IsAutomaticFormat)
		{
			sfChart.ChartArea.BackInterior = GetBrushFromDataFormat(chartArea);
		}
		else if (TryAndGetColorBasedOnElement(ChartElementsEnum.ChartAreaFill, chart, out color))
		{
			double num = 1.0;
			sfChart.ChartArea.BackInterior = new BrushInfo(SfColor(color, (chart.Style < 41 || (chart.Style > 100 && chart.Style < 141)) ? num : 0.0));
		}
		else
		{
			sfChart.ChartArea.BackInterior = new BrushInfo(SfColor(byte.MaxValue, byte.MaxValue, byte.MaxValue, 1.0));
		}
		ChartBorderImpl chartBorderImpl = chartArea.Border as ChartBorderImpl;
		if (chartBorderImpl.LinePattern != OfficeChartLinePattern.None)
		{
			double num2 = 0.0;
			BrushInfo brushInfo = null;
			if (chartBorderImpl.HasLineProperties)
			{
				num2 = GetBorderThickness(chartBorderImpl);
			}
			if (num2 < 1.0)
			{
				num2 = 1.0;
			}
			if (!chartBorderImpl.IsAutoLineColor || chartBorderImpl.HasGradientFill)
			{
				brushInfo = GetBrushFromBorder(chartBorderImpl);
				sfChart.Border.ForeColor = brushInfo.BackColor;
			}
			else
			{
				if (TryAndGetColorBasedOnElement(ChartElementsEnum.ChartAreaLine, chart, out color))
				{
					sfChart.Border.ForeColor = SfColor(color);
				}
				else
				{
					sfChart.Border.ForeColor = SfColor(byte.MaxValue, byte.MaxValue, byte.MaxValue, 1.0);
				}
				num2 = 0.0;
			}
			sfChart.Border.Width = (float)Math.Round(num2);
		}
		else
		{
			sfChart.Border.ForeColor = SfColor(byte.MaxValue, byte.MaxValue, byte.MaxValue, 1.0);
			sfChart.Border.Width = 0f;
		}
	}

	internal void SfChartArea3D(ChartControl sfChart, ChartFrameFormatImpl chartArea, ChartImpl chart)
	{
		Color color;
		if (!chartArea.IsAutomaticFormat)
		{
			sfChart.ChartArea.BackInterior = GetBrushFromDataFormat(chartArea);
		}
		else if (TryAndGetColorBasedOnElement(ChartElementsEnum.ChartAreaFill, chart, out color))
		{
			double num = 1.0;
			sfChart.ChartArea.BackInterior = new BrushInfo(SfColor(color, (chart.Style < 41 || (chart.Style > 100 && chart.Style < 141)) ? num : 0.0));
		}
		else
		{
			sfChart.ChartArea.BackInterior = new BrushInfo(SfColor(byte.MaxValue, byte.MaxValue, byte.MaxValue, 1.0));
		}
		ChartBorderImpl chartBorderImpl = chartArea.Border as ChartBorderImpl;
		if (chartBorderImpl.LinePattern != OfficeChartLinePattern.None)
		{
			double num2 = 0.0;
			if (chartBorderImpl.HasLineProperties)
			{
				num2 = GetBorderThickness(chartBorderImpl);
			}
			if (!chartBorderImpl.IsAutoLineColor || chartBorderImpl.HasGradientFill)
			{
				sfChart.Border.ForeColor = GetLineColorFromBorder(chartBorderImpl);
				if (num2 < 1.0)
				{
					num2 = 1.0;
				}
			}
			else
			{
				if (TryAndGetColorBasedOnElement(ChartElementsEnum.ChartAreaLine, chart, out color))
				{
					sfChart.Border.ForeColor = SfColor(color);
				}
				else
				{
					sfChart.Border.ForeColor = SfColor(byte.MaxValue, byte.MaxValue, byte.MaxValue, 1.0);
				}
				num2 = 0.0;
			}
			sfChart.Border.Width = (float)Math.Round(num2);
		}
		else
		{
			sfChart.Border.ForeColor = SfColor(byte.MaxValue, byte.MaxValue, byte.MaxValue, 1.0);
			sfChart.Border.Width = 0f;
		}
	}

	internal void SfRotation3D(ChartImpl chartImpl, ChartControl sfChart)
	{
		if (!chartImpl.IsChartPie)
		{
			int rotation = chartImpl.Rotation;
			if (rotation <= 90)
			{
				sfChart.Rotation = rotation;
			}
			else if (rotation > 90 && rotation < 180)
			{
				sfChart.Rotation = 180 - rotation;
			}
			else if (rotation == 180)
			{
				sfChart.Rotation = 0f;
			}
			else
			{
				sfChart.Rotation = rotation;
			}
			sfChart.Tilt = chartImpl.Elevation;
			sfChart.Depth = chartImpl.DepthPercent / 5;
			if (chartImpl.Elevation < 0)
			{
				sfChart.PrimaryYAxis.IsVisible = false;
			}
		}
		else
		{
			sfChart.RealMode3D = true;
			sfChart.Depth = 30f;
			sfChart.Rotation = 0f;
			sfChart.Tilt = chartImpl.Elevation - 90;
		}
	}

	internal void SfWall(ChartControl sfChart3D, ChartImpl chart)
	{
		sfChart3D.ChartArea.GridBackInterior = GetWallBrush(chart, chart.BackWall as ChartWallOrFloorImpl, isFloor: false);
		sfChart3D.ChartArea.GridVerticalInterior = GetWallBrush(chart, chart.SideWall as ChartWallOrFloorImpl, isFloor: false);
		sfChart3D.ChartArea.GridHorizontalInterior = GetWallBrush(chart, chart.Floor as ChartWallOrFloorImpl, isFloor: true);
	}

	internal void SfTextBlock(ChartTitle sfTextArea, ChartTextAreaImpl textArea)
	{
		FontName = textArea.FontName;
		Color color = ((!textArea.IsAutoColor || !TryAndGetColorBasedOnElement(ChartElementsEnum.ChartAreaFill, textArea.FindParent(typeof(ChartImpl)) as ChartImpl, out color) || color.R != color.G || color.B != color.R || color.R != 0) ? textArea.Font.RGBColor : Color.FromArgb(0, 255, 255, 255));
		if (FontName.StartsWith("+"))
		{
			FontName = "Calibri";
		}
		sfTextArea.Font = GetDrawingFont(FontName, (float)textArea.Size, textArea);
		if (color.ToArgb() != Color.Empty.ToArgb())
		{
			sfTextArea.ForeColor = SfColor(color);
		}
	}

	internal bool SetTransformAndBackGround(ChartTitle sfTextArea, ChartTextAreaImpl textArea, ChartAxisImpl axis)
	{
		if (!(textArea.FrameFormat as ChartFrameFormatImpl).IsAutomaticFormat && textArea.FrameFormat.Interior.Pattern != 0)
		{
			sfTextArea.BackInterior = GetBrushFromDataFormat(textArea.FrameFormat);
		}
		return false;
	}

	internal void SfChartTitle(IOfficeChart xlsioChart, ChartControl chart, out RectangleF manualRect)
	{
		ChartTextAreaImpl chartTextAreaImpl = xlsioChart.ChartTitleArea as ChartTextAreaImpl;
		ChartLayoutImpl chartLayoutImpl = chartTextAreaImpl.Layout as ChartLayoutImpl;
		ChartTitle title = chart.Title;
		SetTitleBorder(chartTextAreaImpl.FrameFormat.Border as ChartBorderImpl, title);
		manualRect = new RectangleF(-1f, -1f, -1f, -1f);
		if (chartLayoutImpl.IsManualLayout)
		{
			ChartManualLayoutImpl chartManualLayoutImpl = chartLayoutImpl.ManualLayout as ChartManualLayoutImpl;
			if (chartManualLayoutImpl.FlagOptions != 0 && (chartManualLayoutImpl.LeftMode == LayoutModes.edge || chartManualLayoutImpl.WidthMode != LayoutModes.edge) && (chartManualLayoutImpl.TopMode == LayoutModes.edge || chartManualLayoutImpl.HeightMode != LayoutModes.edge))
			{
				manualRect = CalculateManualLayout(chartManualLayoutImpl, out var _);
			}
			title.IsManualLayout = chartLayoutImpl.IsManualLayout;
			title.rectangleF = manualRect;
		}
		bool flag = (xlsioChart.Series.Count == 1 || xlsioChart.ChartType.ToString().Contains("Pie")) && !(xlsioChart.Series[0] as ChartSerieImpl).IsDefaultName;
		SetTransformAndBackGround(title, chartTextAreaImpl, null);
		if (chartTextAreaImpl.RichText != null && chartTextAreaImpl.RichText.FormattingRuns.Length > 1)
		{
			SetTextBlockInlines(chartTextAreaImpl, title);
			return;
		}
		SfTextBlock(title, chartTextAreaImpl);
		title.Text = ((xlsioChart.ChartTitle != null) ? xlsioChart.ChartTitle : (flag ? GetSerieName(xlsioChart.Series[0] as ChartSerieImpl) : "Chart Title"));
	}

	internal void SetTextBlockInlines(ChartTextAreaImpl textArea, ChartTitle title)
	{
		_ = textArea.RichText.FormattingRuns;
		_ = parentWorkbook.InnerFonts;
		string text = textArea.Text;
		title.Alignment = ChartAlignment.Center;
		title.Text = text;
		Color color = ((!textArea.IsAutoColor || !TryAndGetColorBasedOnElement(ChartElementsEnum.ChartAreaFill, textArea.FindParent(typeof(ChartImpl)) as ChartImpl, out color) || color.R != color.G || color.B != color.R || color.R != 0) ? textArea.Font.RGBColor : Color.FromArgb(0, 255, 255, 255));
		string fontName = textArea.FontName;
		title.Font = GetDrawingFont(textArea);
		if (CheckUnicode(title.Text))
		{
			fontName = SwitchFonts(title.Text, textArea.Font.FontName);
			title.Font = GetDrawingFont(fontName, (float)textArea.Font.Size, textArea);
		}
		title.ForeColor = SfColor(color);
	}

	internal void SetTitleBorder(ChartBorderImpl border, ChartTitle chartTitle)
	{
		Color.FromArgb(0, 0, 0, 0);
		double num = 0.0;
		BrushInfo brushInfo = null;
		LineInfo lineInfo = new LineInfo();
		if (border.LinePattern != OfficeChartLinePattern.None)
		{
			chartTitle.ShowBorder = true;
			if (border.HasLineProperties)
			{
				num = GetBorderThickness(border);
			}
			brushInfo = ((border.IsAutoLineColor && !border.HasGradientFill) ? new BrushInfo(SfColor(byte.MaxValue, byte.MaxValue, byte.MaxValue, 1.0)) : GetBrushFromBorder(border));
		}
		else
		{
			brushInfo = new BrushInfo(SfColor(byte.MaxValue, byte.MaxValue, byte.MaxValue, 1.0));
		}
		if (brushInfo.BackColor != Color.FromArgb(0, 0, 0, 0))
		{
			if (num != 0.0)
			{
				num = ((num < 1.0) ? 1.0 : num);
			}
			lineInfo.Width = (float)num;
			lineInfo.ForeColor = brushInfo.BackColor;
		}
		chartTitle.Border = lineInfo;
	}

	internal void SfLegend(ChartControl sfchart, ChartImpl xlsioChart, ChartLegend leg, out int[] sortedLegendOrders, bool isPlotAreaManual, out ChartLegend emptyLegend)
	{
		IOfficeChartLegend legend = xlsioChart.Legend;
		if (!IsChart3D && !IsChartEx && sfchart.Series.Count > 1)
		{
			sfchart.Series.All((ChartSeries x) => x.Type == ChartSeriesType.Pie);
		}
		else
			_ = 0;
		emptyLegend = null;
		ChartLayoutImpl chartLayoutImpl = xlsioChart.Legend.Layout as ChartLayoutImpl;
		ChartLayoutImpl chartLayoutImpl2 = (xlsioChart.HasPlotArea ? (xlsioChart.PlotArea.Layout as ChartLayoutImpl) : null);
		ChartManualLayoutImpl chartManualLayoutImpl = ((chartLayoutImpl2 != null) ? (chartLayoutImpl2.ManualLayout as ChartManualLayoutImpl) : null);
		RectangleF rectangleF = new RectangleF(-1f, -1f, 0f, 0f);
		bool flag = false;
		if (chartLayoutImpl.IsManualLayout)
		{
			ChartManualLayoutImpl chartManualLayoutImpl2 = chartLayoutImpl.ManualLayout as ChartManualLayoutImpl;
			if (chartManualLayoutImpl2.FlagOptions != 0 && (chartManualLayoutImpl2.LeftMode == LayoutModes.edge || chartManualLayoutImpl2.WidthMode != LayoutModes.edge) && (chartManualLayoutImpl2.TopMode == LayoutModes.edge || chartManualLayoutImpl2.HeightMode != LayoutModes.edge))
			{
				rectangleF = CalculateManualLayout(chartManualLayoutImpl2, out var _);
				flag = true;
			}
		}
		if (!flag && xlsioChart.HasPlotArea && chartLayoutImpl2 != null && chartLayoutImpl2.IsManualLayout && (chartManualLayoutImpl.FlagOptions == 0 || chartManualLayoutImpl.FlagOptions == 16 || (chartManualLayoutImpl.LeftMode != LayoutModes.edge && chartManualLayoutImpl.WidthMode == LayoutModes.edge) || (chartManualLayoutImpl.TopMode != LayoutModes.edge && chartManualLayoutImpl.HeightMode == LayoutModes.edge)))
		{
			if (IsChartEx)
			{
				if (!legend.IncludeInLayout)
				{
					leg.IsLegendOverlapping = true;
				}
			}
			else if (legend.IncludeInLayout)
			{
				leg.IsLegendOverlapping = true;
			}
		}
		else
		{
			leg.IsLegendOverlapping = true;
		}
		int num = ((xlsioChart.Series.Count > 0) ? xlsioChart.Legend.LegendEntries.Count : 0);
		leg.RepresentationType = ChartLegendRepresentationType.SeriesType;
		SetLegendPosition(xlsioChart, leg);
		if (xlsioChart.Legend.IsVerticalLegend)
		{
			leg.Orientation = ChartOrientation.Vertical;
		}
		else
		{
			OfficeLegendPosition position = legend.Position;
			if (position == OfficeLegendPosition.Bottom || position == OfficeLegendPosition.Top)
			{
				leg.Spacing = 10;
			}
		}
		if (!(xlsioChart.Legend.FrameFormat as ChartFrameFormatImpl).IsAutomaticFormat && xlsioChart.Legend.FrameFormat.Interior.Pattern != 0)
		{
			leg.BackInterior = GetBrushFromDataFormat(xlsioChart.Legend.FrameFormat);
		}
		double num2 = ((!(xlsioChart.Legend as ChartLegendImpl).IsDefaultTextSettings || xlsioChart.DefaultTextProperty == null) ? xlsioChart.Legend.TextArea.Size : xlsioChart.Font.Size);
		ChartLegendItem[] items = leg.Items;
		for (int i = 0; (i < num) & (i < items.Length); i++)
		{
			items[i].RepresentationSize = new Size((int)num2, (int)num2);
		}
		if (xlsioChart.Legend.TextArea.RGBColor.ToArgb() != Color.Empty.ToArgb())
		{
			leg.ForeColor = SfColor(xlsioChart.Legend.TextArea.RGBColor);
		}
		FontName = xlsioChart.Legend.TextArea.FontName;
		if (FontName.StartsWith("+"))
		{
			bool flag2 = FontName.Contains("+mn-");
			if (flag2 ? (parentWorkbook.MinorFonts != null) : (parentWorkbook.MajorFonts != null))
			{
				string text = FontName.Replace(flag2 ? "+mn-" : "+mj-", "");
				if (flag2 ? parentWorkbook.MinorFonts.ContainsKey(text) : parentWorkbook.MajorFonts.ContainsKey(text))
				{
					FontName = (flag2 ? parentWorkbook.MinorFonts[text] : parentWorkbook.MajorFonts[text]).FontName;
				}
				else if (flag2 && text == "lt" && parentWorkbook.MinorFonts.ContainsKey("latin"))
				{
					FontName = parentWorkbook.MinorFonts["latin"].FontName;
				}
			}
			if (FontName.StartsWith("+"))
			{
				FontName = "Calibri";
			}
		}
		leg.Font = GetDrawingFont(FontName, (float)num2, xlsioChart.Legend.TextArea);
		ChartBorderImpl chartBorderImpl = xlsioChart.Legend.FrameFormat.Border as ChartBorderImpl;
		if (chartBorderImpl.LinePattern != OfficeChartLinePattern.None)
		{
			if (chartBorderImpl.HasLineProperties)
			{
				leg.ShowBorder = true;
				double num3 = GetBorderThickness(chartBorderImpl);
				leg.Border.Width = (float)((num3 < 1.0) ? 1.0 : num3);
			}
			if (chartBorderImpl.IsAutoLineColor && !chartBorderImpl.HasGradientFill && parentWorkbook.Version == OfficeVersion.Excel97to2003)
			{
				leg.ShowBorder = true;
				leg.Border.ForeColor = SfColor(0, 0, 0);
				leg.Border.Width = 1f;
			}
			else
			{
				leg.Border.ForeColor = SfColor(chartBorderImpl.LineColor);
			}
		}
		else
		{
			leg.Border.ForeColor = SfColor(byte.MaxValue, byte.MaxValue, byte.MaxValue, 1.0);
		}
		string text2 = xlsioChart.ChartType.ToString();
		if (text2.Contains("Line"))
		{
			leg.ItemsSize = new Size((int)num2 * 2, (int)num2);
		}
		else if (!text2.Contains("Combination"))
		{
			leg.ItemsSize = new Size((int)num2 / 2, (int)num2 / 2);
		}
		if (IsChartEx)
		{
			if (xlsioChart.Legend.IncludeInLayout)
			{
				leg.Position = ChartDock.Bottom;
			}
		}
		else if (!isPlotAreaManual)
		{
			_ = xlsioChart.Legend.IncludeInLayout;
		}
		sortedLegendOrders = ((sfchart.Series.Count > 1) ? GetOrderOfLegendItems(sfchart, xlsioChart, leg) : null);
		OfficeChartType chartType = xlsioChart.ChartType;
		if (xlsioChart != null && xlsioChart.Legend.IsVerticalLegend && !IsSeriesReverseOrder && chartType != OfficeChartType.Column_Stacked && chartType != OfficeChartType.Column_Stacked_100 && chartType != OfficeChartType.Column_Stacked_3D && chartType != OfficeChartType.Column_Stacked_100_3D && chartType != OfficeChartType.Line_Stacked && chartType != OfficeChartType.Line_Stacked_100 && chartType != OfficeChartType.Line_Markers_Stacked && chartType != OfficeChartType.Line_Markers_Stacked_100 && chartType != OfficeChartType.Bar_Clustered && chartType != OfficeChartType.Bar_Clustered_3D && chartType != OfficeChartType.Area_Stacked && chartType != OfficeChartType.Area_Stacked_100 && chartType != OfficeChartType.Area_Stacked_3D)
		{
			_ = 36;
		}
		Dictionary<int, ChartLegendEntryImpl> hashEntries = (legend.LegendEntries as ChartLegendEntriesColl).HashEntries;
		int num4 = hashEntries.Count;
		if (hashEntries.Count > 0)
		{
			List<IOfficeChartSerie> list = xlsioChart.Series.OrderByType();
			IEnumerable<IGrouping<int, IOfficeChartSerie>> enumerable = from x in xlsioChart.Series
				where !x.IsFiltered
				group x by (x as ChartSerieImpl).ChartGroup;
			if (xlsioChart.ChartSerieGroupsBeforesorting != null)
			{
				enumerable = xlsioChart.ChartSerieGroupsBeforesorting;
			}
			foreach (KeyValuePair<int, ChartLegendEntryImpl> item in hashEntries)
			{
				if (!item.Value.IsDeleted)
				{
					continue;
				}
				int num5 = 0;
				foreach (IGrouping<int, IOfficeChartSerie> item2 in enumerable)
				{
					int num6 = item2.Count();
					num5 += num6;
					if (item.Key >= num5)
					{
						continue;
					}
					int num7 = ((!IsSeriesReverseOrder) ? (item.Key - (num5 - num6)) : (num5 - item.Key - 1 - (num5 - num6)));
					IOfficeChartSerie officeChartSerie = item2.ElementAt(num7);
					if (officeChartSerie == null)
					{
						break;
					}
					for (int j = 0; j < list.Count; j++)
					{
						if (list[j].Name == officeChartSerie.Name)
						{
							num7 = j;
							break;
						}
					}
					if (HasNewSeries && num7 >= NewseriesIndex)
					{
						num7++;
					}
					if (sfchart != null && num7 < sfchart.Series.Count)
					{
						sfchart.Series[num7].LegendItem.Visible = false;
						sfchart.Legend.Items[num7].Visible = false;
					}
					num4--;
					break;
				}
			}
			HasNewSeries = false;
			NewseriesIndex = 0;
		}
		if (legend.LegendEntries.Count > 0)
		{
			for (int k = 0; k < legend.LegendEntries.Count; k++)
			{
				if (k < items.Count() && legend.LegendEntries[k].TextArea != null)
				{
					Color rGBColor = legend.LegendEntries[k].TextArea.RGBColor;
					if (!legend.LegendEntries[k].TextArea.IsAutoColor && rGBColor.ToArgb() != Color.Empty.ToArgb() && rGBColor.ToArgb() != Color.FromArgb(rGBColor.A, 51, 51, 51).ToArgb())
					{
						items[k].TextColor = SfColor(rGBColor);
						items[k].IsLegendTextColor = true;
					}
				}
			}
		}
		if (IsLegendManualLayout && rectangleF.Width > 0f && rectangleF.Height > 0f)
		{
			return;
		}
		bool flag3 = sfchart != null && (leg.Position == ChartDock.Left || leg.Position == ChartDock.Right);
		if ((leg.Position != 0 && leg.Position != ChartDock.Right) || flag3)
		{
			double serieWidth = 0.0;
			double totalWidth = 0.0;
			int totalCount = 0;
			int iconWidth = 0;
			CalculateLegendSize(leg, out serieWidth, out var serieHeight, out totalWidth, out totalCount, out iconWidth);
			totalWidth = ((leg.Orientation != ChartOrientation.Vertical) ? (totalWidth + (double)(totalCount * iconWidth)) : ((serieWidth + (double)leg.Spacing) * (double)totalCount));
			if ((double)ChartWidth < totalWidth)
			{
				leg.RowsCount = (int)Math.Ceiling(totalWidth / (double)ChartWidth + 1.0);
			}
			if (leg.RowsCount > 1)
			{
				leg.ItemsSize = new SizeF((float)serieWidth, (float)serieHeight);
			}
		}
		else if (leg.Alignment != 0 && num4 > 1 && (leg.Position == ChartDock.Left || leg.Position == ChartDock.Right) && (double)num4 * ((double)leg.Font.GetHeight() + 3.0) > 0.7 * (double)ChartHeight)
		{
			leg.Alignment = ChartAlignment.Near;
		}
	}

	internal Stream GenerateStreamFromXamlContent(StringBuilder sb)
	{
		MemoryStream memoryStream = new MemoryStream();
		StreamWriter streamWriter = new StreamWriter(memoryStream);
		streamWriter.Write(sb.ToString());
		streamWriter.Flush();
		memoryStream.Position = 0L;
		return memoryStream;
	}

	private void CalculateLegendSize(ChartLegend legend, out double serieWidth, out double serieHeight, out double totalWidth, out int totalCount, out int iconWidth)
	{
		totalWidth = 0.0;
		serieWidth = 0.0;
		serieHeight = 0.0;
		totalCount = 0;
		iconWidth = 0;
		Graphics g = Graphics.FromImage(new Bitmap(1, 1));
		ChartLegendItem[] items = legend.Items;
		foreach (ChartLegendItem chartLegendItem in items)
		{
			if (chartLegendItem.Visible)
			{
				SizeF sizeF = chartLegendItem.Measure(g);
				serieWidth = Math.Max(sizeF.Width, serieWidth);
				serieHeight = Math.Max(sizeF.Height, serieHeight);
				totalWidth += serieWidth;
				iconWidth = Math.Max(chartLegendItem.ItemStyle.RepresentationSize.Width, iconWidth);
				totalCount++;
			}
		}
	}

	private void SetLegendPosition(IOfficeChart xlsioChart, ChartLegend leg)
	{
		switch ((int)xlsioChart.Legend.Position)
		{
		case 0:
			leg.Position = ChartDock.Bottom;
			leg.Alignment = ChartAlignment.Center;
			break;
		case 4:
			leg.Position = ChartDock.Left;
			break;
		case 2:
			leg.Position = ChartDock.Top;
			break;
		case 3:
			leg.Position = ChartDock.Right;
			break;
		case 1:
			leg.Alignment = ChartAlignment.Far;
			leg.Orientation = ChartOrientation.Vertical;
			leg.Position = ChartDock.Top;
			break;
		}
	}

	private int[] GetOrderOfLegendItems(ChartControl sfchart, IOfficeChart chart, ChartLegend legend)
	{
		int[] array = null;
		string text = chart.ChartType.ToString();
		int count = sfchart.Series.Count;
		bool flag = false;
		if (chart.Legend.Position == OfficeLegendPosition.Corner || chart.Legend.Position == OfficeLegendPosition.Right || chart.Legend.Position == OfficeLegendPosition.Left)
		{
			flag = true;
		}
		if (text.Contains("Bar") && text.Contains("Clustered"))
		{
			array = new int[count];
			for (int i = 0; i < count; i++)
			{
				array[i] = count - 1 - i;
			}
		}
		else
		{
			bool num;
			if (!IsLegendManualLayout)
			{
				if (legend.Position == ChartDock.Left)
				{
					goto IL_00c2;
				}
				num = legend.Position == ChartDock.Right;
			}
			else
			{
				num = chart.Legend.IsVerticalLegend && flag;
			}
			if (num)
			{
				goto IL_00c2;
			}
		}
		goto IL_0162;
		IL_00c2:
		bool num2 = sfchart != null;
		array = new int[count];
		if (num2)
		{
			for (int j = 0; j < count; j++)
			{
				array[j] = count - 1 - j;
			}
		}
		else
		{
			IList<ChartSeries> seriesList = null;
			IList<ChartSeries> seriesList2 = null;
			IList<ChartSeries> seriesList3 = null;
			IList<ChartSeries> seriesList4 = null;
			int index = 0;
			UpdateOrderArray(seriesList, sfchart, index, out index, array);
			UpdateOrderArray(seriesList3, sfchart, index, out index, array);
			UpdateOrderArray(seriesList2, sfchart, index, out index, array);
			UpdateOrderArray(seriesList4, sfchart, index, out index, array);
			if (index < count)
			{
				for (int k = 0; k < count; k++)
				{
					if (!array.Contains(k))
					{
						array[index] = k;
						index++;
					}
					if (index >= count)
					{
						break;
					}
				}
			}
		}
		goto IL_0162;
		IL_0162:
		return array;
	}

	private void UpdateOrderArray(IList<ChartSeries> seriesList, ChartControl sfchart, int i, out int index, int[] orderResult)
	{
		if (seriesList != null && seriesList.Count > 0)
		{
			for (int num = seriesList.Count - 1; num >= 0; num--)
			{
				orderResult[i] = sfchart.Series.IndexOf(seriesList[num]);
				i++;
			}
		}
		index = i;
	}

	internal RectangleF CalculateManualLayout(ChartManualLayoutImpl layoutImpl, out bool isInnerLayout)
	{
		isInnerLayout = false;
		if (IsChartEx)
		{
			return new RectangleF(-1f, -1f, -1f, -1f);
		}
		float num = -1f;
		float num2 = -1f;
		float num3 = -1f;
		float num4 = -1f;
		if (((uint)layoutImpl.FlagOptions & (true ? 1u : 0u)) != 0)
		{
			if (layoutImpl.TopMode == LayoutModes.edge || layoutImpl.LeftMode == LayoutModes.factor)
			{
				num2 = (float)Math.Floor((double)ChartHeight * layoutImpl.Top);
			}
			else if (-0.5 <= layoutImpl.Top && layoutImpl.Top <= 0.5)
			{
				num2 = (float)Math.Floor((double)ChartHeight * 0.9 * (0.5 + layoutImpl.Top));
			}
		}
		if ((layoutImpl.FlagOptions & 2u) != 0)
		{
			if (layoutImpl.LeftMode == LayoutModes.edge)
			{
				num = (float)Math.Floor((double)ChartWidth * layoutImpl.Left);
			}
			else if (layoutImpl.LeftMode == LayoutModes.factor)
			{
				num = (float)Math.Floor((double)ChartWidth * (layoutImpl.Left * 5.0));
				if (num > (float)ChartWidth)
				{
					num = (float)Math.Floor((double)ChartWidth * (layoutImpl.Left * 2.4));
				}
			}
			else if (-1.0 <= layoutImpl.Left && layoutImpl.Left <= 0.0)
			{
				num = (float)Math.Floor((double)ChartWidth * 0.8 * (2.0 + layoutImpl.Left));
			}
		}
		if ((layoutImpl.FlagOptions & 8u) != 0)
		{
			num3 = (float)Math.Floor((double)ChartWidth * layoutImpl.Width);
			if (layoutImpl.WidthMode == LayoutModes.edge)
			{
				num3 -= num;
			}
			if (num3 == 0f)
			{
				num3 = -1f;
			}
		}
		else
		{
			num3 = 0f;
		}
		if ((layoutImpl.FlagOptions & 4u) != 0)
		{
			num4 = (float)Math.Floor((double)ChartHeight * layoutImpl.Height);
			if (layoutImpl.HeightMode == LayoutModes.edge)
			{
				num4 -= num2;
			}
			if (num4 == 0f)
			{
				num4 = -1f;
			}
		}
		else
		{
			num4 = 0f;
		}
		if ((layoutImpl.FlagOptions & 0x10u) != 0 && layoutImpl.LayoutTarget == LayoutTargets.inner)
		{
			isInnerLayout = true;
		}
		if (num < 0f || num2 < 0f || num3 < 0f || num4 < 0f)
		{
			return new RectangleF(-1f, -1f, -1f, -1f);
		}
		return new RectangleF(num, num2, num3, num4);
	}

	private void TryAndUpdateLegendItemsInWaterFall(ChartControl sfchart, ChartImpl xlsioChart)
	{
	}
}
