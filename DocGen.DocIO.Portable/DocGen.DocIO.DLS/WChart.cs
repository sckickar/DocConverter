using System.Collections;
using System.IO;
using System.Text;
using DocGen.Compression;
using DocGen.Compression.Zip;
using DocGen.DocIO.Rendering;
using DocGen.Drawing;
using DocGen.Layouting;
using DocGen.OfficeChart;
using DocGen.OfficeChart.Implementation;
using DocGen.OfficeChart.Implementation.Charts;
using DocGen.OfficeChart.Implementation.XmlSerialization;

namespace DocGen.DocIO.DLS;

public class WChart : ShapeBase, IEntity, ILeafWidget, IWidget
{
	private IOfficeChart m_officeChart;

	private string m_externalDataPath = string.Empty;

	private string m_internalDataPath = string.Empty;

	private string m_userShapes = string.Empty;

	private bool m_isExternalRelation;

	private bool m_is2016Chart;

	public IOfficeChart OfficeChart
	{
		get
		{
			if (m_officeChart == null)
			{
				m_officeChart = new ChartImpl();
			}
			((WorkbookImpl)(m_officeChart as ChartImpl).Workbook).IsWorkbookOpening = base.Document.IsOpening;
			return m_officeChart;
		}
		internal set
		{
			m_officeChart = value;
		}
	}

	internal bool Is2016Chart
	{
		get
		{
			return m_is2016Chart;
		}
		set
		{
			m_is2016Chart = value;
		}
	}

	internal WorkbookImpl Workbook => (OfficeChart as ChartImpl).Workbook as WorkbookImpl;

	internal string InternalDataPath
	{
		get
		{
			return m_internalDataPath;
		}
		set
		{
			m_internalDataPath = value;
		}
	}

	internal string UserShapes
	{
		get
		{
			return m_userShapes;
		}
		set
		{
			m_userShapes = value;
		}
	}

	internal bool IsExternalRelation
	{
		get
		{
			return m_isExternalRelation;
		}
		set
		{
			m_isExternalRelation = value;
			(OfficeChart as ChartImpl).IsChartExternalRelation = value;
		}
	}

	public OfficeChartType ChartType
	{
		get
		{
			return OfficeChart.ChartType;
		}
		set
		{
			OfficeChart.ChartType = value;
			Is2016Chart = OfficeChart.ChartType == OfficeChartType.WaterFall;
		}
	}

	public string ExternalDataPath
	{
		get
		{
			return m_externalDataPath;
		}
		set
		{
			m_externalDataPath = value;
			IsExternalRelation = true;
		}
	}

	public IOfficeDataRange DataRange
	{
		get
		{
			return OfficeChart.DataRange;
		}
		set
		{
			OfficeChart.DataRange = value;
		}
	}

	public bool IsSeriesInRows
	{
		get
		{
			return OfficeChart.IsSeriesInRows;
		}
		set
		{
			OfficeChart.IsSeriesInRows = value;
		}
	}

	public string ChartTitle
	{
		get
		{
			return OfficeChart.ChartTitle;
		}
		set
		{
			OfficeChart.ChartTitle = value;
		}
	}

	public IOfficeChartTextArea ChartTitleArea => OfficeChart.ChartTitleArea;

	public IOfficeChartSeries Series => OfficeChart.Series;

	public IOfficeChartCategoryAxis PrimaryCategoryAxis => OfficeChart.PrimaryCategoryAxis;

	public IOfficeChartValueAxis PrimaryValueAxis => OfficeChart.PrimaryValueAxis;

	public IOfficeChartSeriesAxis PrimarySeriesAxis => OfficeChart.PrimarySerieAxis;

	public IOfficeChartCategoryAxis SecondaryCategoryAxis => OfficeChart.SecondaryCategoryAxis;

	public IOfficeChartValueAxis SecondaryValueAxis => OfficeChart.SecondaryValueAxis;

	public IOfficeChartFrameFormat ChartArea => OfficeChart.ChartArea;

	public IOfficeChartFrameFormat PlotArea => OfficeChart.PlotArea;

	public IOfficeChartWallOrFloor Walls => OfficeChart.Walls;

	public IOfficeChartWallOrFloor SideWall => OfficeChart.SideWall;

	public IOfficeChartWallOrFloor BackWall => OfficeChart.BackWall;

	public IOfficeChartWallOrFloor Floor => OfficeChart.Floor;

	public IOfficeChartDataTable DataTable => OfficeChart.DataTable;

	public bool HasDataTable
	{
		get
		{
			return OfficeChart.HasDataTable;
		}
		set
		{
			OfficeChart.HasDataTable = value;
		}
	}

	public IOfficeChartLegend Legend => OfficeChart.Legend;

	public bool HasLegend
	{
		get
		{
			return OfficeChart.HasLegend;
		}
		set
		{
			OfficeChart.HasLegend = value;
		}
	}

	public int Rotation
	{
		get
		{
			return OfficeChart.Rotation;
		}
		set
		{
			OfficeChart.Rotation = value;
		}
	}

	public int Elevation
	{
		get
		{
			return OfficeChart.Elevation;
		}
		set
		{
			OfficeChart.Elevation = value;
		}
	}

	public int Perspective
	{
		get
		{
			return OfficeChart.Perspective;
		}
		set
		{
			OfficeChart.Perspective = value;
		}
	}

	public int HeightPercent
	{
		get
		{
			return OfficeChart.HeightPercent;
		}
		set
		{
			OfficeChart.HeightPercent = value;
		}
	}

	public int DepthPercent
	{
		get
		{
			return OfficeChart.DepthPercent;
		}
		set
		{
			OfficeChart.DepthPercent = value;
		}
	}

	public int GapDepth
	{
		get
		{
			return OfficeChart.GapDepth;
		}
		set
		{
			OfficeChart.GapDepth = value;
		}
	}

	public bool RightAngleAxes
	{
		get
		{
			return OfficeChart.RightAngleAxes;
		}
		set
		{
			OfficeChart.RightAngleAxes = value;
		}
	}

	public bool AutoScaling
	{
		get
		{
			return OfficeChart.AutoScaling;
		}
		set
		{
			OfficeChart.AutoScaling = value;
		}
	}

	public bool HasPlotArea
	{
		get
		{
			return OfficeChart.HasPlotArea;
		}
		set
		{
			OfficeChart.HasPlotArea = value;
		}
	}

	public OfficeChartPlotEmpty DisplayBlanksAs
	{
		get
		{
			return OfficeChart.DisplayBlanksAs;
		}
		set
		{
			OfficeChart.DisplayBlanksAs = value;
		}
	}

	public bool PlotVisibleOnly
	{
		get
		{
			return OfficeChart.PlotVisibleOnly;
		}
		set
		{
			OfficeChart.PlotVisibleOnly = value;
		}
	}

	public IOfficeChartCategories Categories => OfficeChart.Categories;

	public OfficeSeriesNameLevel SeriesNameLevel
	{
		get
		{
			return OfficeChart.SeriesNameLevel;
		}
		set
		{
			OfficeChart.SeriesNameLevel = value;
		}
	}

	public OfficeCategoriesLabelLevel CategoryLabelLevel
	{
		get
		{
			return OfficeChart.CategoryLabelLevel;
		}
		set
		{
			OfficeChart.CategoryLabelLevel = value;
		}
	}

	public IOfficeChartData ChartData => OfficeChart.ChartData;

	public override EntityType EntityType => EntityType.Chart;

	internal override void AttachToParagraph(WParagraph paragraph, int itemPos)
	{
		base.AttachToParagraph(paragraph, itemPos);
		if (!base.DeepDetached)
		{
			base.IsCloned = false;
		}
		else
		{
			base.IsCloned = true;
		}
		if (GetTextWrappingStyle() != 0)
		{
			base.Document.FloatingItems.Add(this);
		}
	}

	internal override void Detach()
	{
		base.Detach();
		base.Document.FloatingItems.Remove(this);
	}

	protected override object CloneImpl()
	{
		WChart obj = (WChart)base.CloneImpl();
		obj.OfficeChart = (OfficeChart as ChartImpl).Clone();
		obj.IsCloned = true;
		obj.ShapeID = 0L;
		return obj;
	}

	internal override void CloneRelationsTo(WordDocument doc, OwnerHolder nextOwner)
	{
		base.CloneRelationsTo(doc, nextOwner);
	}

	internal override void Close()
	{
		if (m_officeChart != null)
		{
			(m_officeChart as ChartImpl).Close();
			m_officeChart = null;
		}
		base.Close();
	}

	public WChart(WordDocument doc)
		: base(doc)
	{
		m_officeChart = new ChartImpl();
		Workbook.IsWorkbookOpening = doc.IsOpening;
		(m_officeChart as ChartImpl).SetDefaultProperties();
		CreateDataHolder();
		base.WrapFormat.SetTextWrappingStyleValue(TextWrappingStyle.Inline);
		base.VerticalAlignment = ShapeVerticalAlignment.None;
		base.HorizontalAlignment = ShapeHorizontalAlignment.None;
		m_charFormat = new WCharacterFormat(doc, this);
	}

	internal Stream SaveAsImage()
	{
		Stream stream = new MemoryStream();
		WordDocument.RenderHelper.ConvertChartAsImage(OfficeChart, stream, new ChartRenderingOptions());
		stream.Position = 0L;
		return stream;
	}

	internal void CreateDataHolder()
	{
		if (Workbook.DataHolder == null)
		{
			Workbook.DataHolder = new FileDataHolder(Workbook);
			ZipArchiveItem item = new ZipArchive().AddItem(string.Empty, null, bControlStream: false, DocGen.Compression.FileAttributes.Archive);
			if (Workbook.ActiveSheet != null)
			{
				(Workbook.ActiveSheet as WorksheetImpl).DataHolder = new WorksheetDataHolder(Workbook.DataHolder, item);
				(Workbook.ActiveSheet as WorksheetImpl).IsParsed = true;
			}
		}
		if (Workbook.ActiveSheet != null)
		{
			(m_officeChart as ChartImpl).DataHolder = (Workbook.ActiveSheet as WorksheetImpl).DataHolder;
		}
	}

	internal void SetDataRange(int sheetNumber, string dataRange)
	{
		WorksheetImpl worksheetImpl = Workbook.Worksheets[sheetNumber - 1] as WorksheetImpl;
		(m_officeChart as ChartImpl).DataIRange = worksheetImpl.Range[dataRange];
	}

	internal void InitializeOfficeChartToImageConverter()
	{
	}

	public void SetChartData(object[][] data)
	{
		OfficeChart.SetChartData(data);
	}

	public void SetDataRange(object[][] data, int rowIndex, int columnIndex)
	{
		OfficeChart.SetDataRange(data, rowIndex, columnIndex);
	}

	public void SetDataRange(IEnumerable enumerable, int rowIndex, int columnIndex)
	{
		OfficeChart.SetDataRange(enumerable, rowIndex, columnIndex);
	}

	public void Refresh(bool updateFormula = false)
	{
		if (m_officeChart != null)
		{
			(m_officeChart as ChartImpl).Refresh(updateFormula);
		}
	}

	protected override void CreateLayoutInfo()
	{
		m_layoutInfo = new LayoutInfo(ChildrenLayoutDirection.Horizontal);
		WParagraph wParagraph = base.OwnerParagraph;
		if (base.Owner is InlineContentControl || base.Owner is XmlParagraphItem)
		{
			wParagraph = GetOwnerParagraphValue();
		}
		Entity ownerEntity = wParagraph.GetOwnerEntity();
		if ((wParagraph.IsInCell && ((IWidget)wParagraph).LayoutInfo.IsClipped) || ownerEntity is Shape || ownerEntity is WTextBox || ownerEntity is ChildShape)
		{
			m_layoutInfo.IsClipped = true;
		}
		if (base.WrapFormat.TextWrappingStyle != 0)
		{
			m_layoutInfo.IsSkipBottomAlign = true;
		}
		if (base.ParaItemCharFormat.HasValue(53))
		{
			m_layoutInfo.IsSkip = true;
		}
		if (base.IsDeleteRevision && !base.Document.RevisionOptions.ShowDeletedText)
		{
			m_layoutInfo.IsSkip = true;
		}
	}

	internal override void InitLayoutInfo(Entity entity, ref bool isLastTOCEntry)
	{
		m_layoutInfo = null;
		if (this == entity)
		{
			isLastTOCEntry = true;
		}
	}

	SizeF ILeafWidget.Measure(DrawingContext dc)
	{
		return new SizeF(base.Width, base.Height);
	}

	internal string GetChartAsString()
	{
		string s = (OfficeChart as ChartImpl).GetChartAsString().ToString();
		byte[] bytes = Encoding.UTF8.GetBytes(s);
		return base.Document.Comparison.ConvertBytesAsHash(bytes);
	}

	internal bool Compare()
	{
		return (OfficeChart as ChartImpl).Compare(OfficeChart as ChartImpl);
	}

	internal StringBuilder GetAsString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append('\u0016');
		stringBuilder.Append(GetProperties());
		stringBuilder.Append('\u0016');
		return stringBuilder;
	}

	internal StringBuilder GetProperties()
	{
		StringBuilder stringBuilder = new StringBuilder();
		string value = (base.IsRelativeVerticalPosition ? "1" : "0");
		stringBuilder.Append(value);
		value = (base.IsRelativeHorizontalPosition ? "1" : "0");
		stringBuilder.Append(value);
		value = (base.IsRelativeHeight ? "1" : "0");
		stringBuilder.Append(value);
		value = (base.IsRelativeWidth ? "1" : "0");
		stringBuilder.Append(value);
		value = (base.Visible ? "1" : "0");
		stringBuilder.Append(base.Visible);
		value = (base.IsBelowText ? "1" : "0");
		stringBuilder.Append(value);
		value = (base.LayoutInCell ? "1" : "0");
		stringBuilder.Append(value);
		value = (base.LockAnchor ? "1" : "0");
		stringBuilder.Append(value);
		stringBuilder.Append((int)base.HorizontalOrigin + ";");
		stringBuilder.Append((int)base.RelativeWidthHorizontalOrigin + ";");
		stringBuilder.Append((int)base.RelativeHeightVerticalOrigin + ";");
		stringBuilder.Append((int)base.RelativeHorizontalOrigin + ";");
		stringBuilder.Append((int)base.RelativeVerticalOrigin + ";");
		stringBuilder.Append((int)base.HorizontalAlignment + ";");
		stringBuilder.Append((int)base.VerticalOrigin + ";");
		stringBuilder.Append((int)base.VerticalAlignment + ";");
		stringBuilder.Append(base.HorizontalPosition + ";");
		stringBuilder.Append(base.RelativeHorizontalPosition + ";");
		stringBuilder.Append(base.RelativeVerticalPosition + ";");
		stringBuilder.Append(base.RelativeHeight + ";");
		stringBuilder.Append(base.RelativeWidth + ";");
		stringBuilder.Append(base.VerticalPosition + ";");
		stringBuilder.Append(base.LeftEdgeExtent + ";");
		stringBuilder.Append(base.TopEdgeExtent + ";");
		stringBuilder.Append(base.BottomEdgeExtent + ";");
		stringBuilder.Append(base.Height + ";");
		stringBuilder.Append(base.Width + ";");
		stringBuilder.Append(base.HeightScale + ";");
		stringBuilder.Append(base.WidthScale + ";");
		stringBuilder.Append(base.CoordinateXOrigin + ";");
		stringBuilder.Append(base.CoordinateYOrigin + ";");
		stringBuilder.Append(base.ZOrderPosition + ";");
		stringBuilder.Append(base.AlternativeText + ";");
		stringBuilder.Append(base.Title + ";");
		stringBuilder.Append(base.Path + ";");
		stringBuilder.Append(base.CoordinateSize + ";");
		stringBuilder.Append(InternalDataPath + ";");
		stringBuilder.Append(UserShapes + ";");
		stringBuilder.Append(ExternalDataPath + ";");
		return stringBuilder;
	}

	internal bool Compare(WChart wChart)
	{
		if (wChart == null)
		{
			return false;
		}
		if (base.IsRelativeVerticalPosition != wChart.IsRelativeVerticalPosition || base.IsRelativeHorizontalPosition != wChart.IsRelativeHorizontalPosition || base.IsRelativeHeight != wChart.IsRelativeHeight || base.IsRelativeWidth != wChart.IsRelativeWidth || base.Visible != wChart.Visible || base.IsBelowText != wChart.IsBelowText || base.LayoutInCell != wChart.LayoutInCell || base.LockAnchor != wChart.LockAnchor || base.HorizontalOrigin != wChart.HorizontalOrigin || base.RelativeWidthHorizontalOrigin != wChart.RelativeWidthHorizontalOrigin || base.RelativeHeightVerticalOrigin != wChart.RelativeHeightVerticalOrigin || base.RelativeHorizontalOrigin != wChart.RelativeHorizontalOrigin || base.RelativeVerticalOrigin != wChart.RelativeVerticalOrigin || base.HorizontalAlignment != wChart.HorizontalAlignment || base.VerticalOrigin != wChart.VerticalOrigin || base.VerticalAlignment != wChart.VerticalAlignment || base.HorizontalPosition != wChart.HorizontalPosition || base.RelativeHorizontalPosition != wChart.RelativeHorizontalPosition || base.RelativeVerticalPosition != wChart.RelativeVerticalPosition || base.RelativeHeight != wChart.RelativeHeight || base.RelativeWidth != wChart.RelativeWidth || base.VerticalPosition != wChart.VerticalPosition || base.LeftEdgeExtent != wChart.LeftEdgeExtent || base.TopEdgeExtent != wChart.TopEdgeExtent || base.BottomEdgeExtent != wChart.BottomEdgeExtent || base.Height != wChart.Height || base.Width != wChart.Width || base.HeightScale != wChart.HeightScale || base.WidthScale != wChart.WidthScale || base.CoordinateXOrigin != wChart.CoordinateXOrigin || base.CoordinateYOrigin != wChart.CoordinateYOrigin || base.ZOrderPosition != wChart.ZOrderPosition || base.AlternativeText != wChart.AlternativeText || base.Title != wChart.Title || base.Path != wChart.Path || base.CoordinateSize != wChart.CoordinateSize || InternalDataPath != wChart.InternalDataPath || UserShapes != wChart.UserShapes || ExternalDataPath != wChart.ExternalDataPath)
		{
			return false;
		}
		if ((OfficeChart != null && wChart.OfficeChart == null) || (OfficeChart == null && wChart.OfficeChart != null))
		{
			return false;
		}
		if (OfficeChart != null && wChart.OfficeChart != null && !(OfficeChart as ChartImpl).Compare(wChart.OfficeChart as ChartImpl))
		{
			return false;
		}
		return true;
	}
}
