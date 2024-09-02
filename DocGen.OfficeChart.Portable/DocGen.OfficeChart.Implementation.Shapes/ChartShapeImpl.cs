using System;
using System.Collections;
using System.Collections.Generic;
using DocGen.Drawing;
using DocGen.OfficeChart.Implementation.Charts;
using DocGen.OfficeChart.Parser;
using DocGen.OfficeChart.Parser.Biff_Records;
using DocGen.OfficeChart.Parser.Biff_Records.MsoDrawing;
using DocGen.OfficeChart.Parser.Biff_Records.ObjRecords;

namespace DocGen.OfficeChart.Implementation.Shapes;

internal class ChartShapeImpl : ShapeImpl, IOfficeChartShape, IShape, IParentApplication, IOfficeChart
{
	private const int DEF_SHAPE_INSTANCE = 201;

	private const int DEF_SHAPE_VERSION = 2;

	private const int DEF_OPTIONS_VERSION = 3;

	private const int DEF_OPTIONS_INSTANCE = 8;

	private const uint DEF_LOCK_GROUPING_VALUE = 17039620u;

	private const uint DEF_LINECOLOR = 134217805u;

	private const uint DEF_NOLINEDRAWDASH = 524296u;

	private const uint DEF_SHADOWOBSCURED = 131072u;

	private const uint DEF_FORECOLOR = 134217806u;

	private const uint DEF_BACKCOLOR = 134217805u;

	private ChartImpl m_chart;

	private int m_iTopRow;

	private int m_iBottomRow;

	private int m_iLeftColumn;

	private int m_iRightColumn;

	private WorksheetBaseImpl m_worksheet;

	private int m_offsetX;

	private int m_offsetY;

	private int m_extentsX;

	private int m_extentsY;

	private ChartCategoryCollection m_categories;

	public ChartImpl ChartObject => m_chart;

	internal int OffsetX
	{
		get
		{
			return m_offsetX;
		}
		set
		{
			m_offsetX = value;
		}
	}

	internal int OffsetY
	{
		get
		{
			return m_offsetY;
		}
		set
		{
			m_offsetY = value;
		}
	}

	internal int ExtentsX
	{
		get
		{
			return m_extentsX;
		}
		set
		{
			m_extentsX = value;
		}
	}

	internal int ExtentsY
	{
		get
		{
			return m_extentsY;
		}
		set
		{
			m_extentsY = value;
		}
	}

	public IOfficeChartData ChartData => m_chart.ChartData;

	public int Rotation
	{
		get
		{
			return m_chart.Rotation;
		}
		set
		{
			m_chart.Rotation = value;
		}
	}

	public OfficeSeriesNameLevel SeriesNameLevel
	{
		get
		{
			return m_chart.SeriesNameLevel;
		}
		set
		{
			m_chart.SeriesNameLevel = value;
		}
	}

	public int Style
	{
		get
		{
			return m_chart.Style;
		}
		set
		{
			m_chart.Style = value;
		}
	}

	public OfficeCategoriesLabelLevel CategoryLabelLevel
	{
		get
		{
			return m_chart.CategoryLabelLevel;
		}
		set
		{
			m_chart.CategoryLabelLevel = value;
		}
	}

	public IOfficeChartCategories Categories => m_chart.Categories;

	public int Elevation
	{
		get
		{
			return m_chart.Elevation;
		}
		set
		{
			m_chart.Elevation = value;
		}
	}

	public int Perspective
	{
		get
		{
			return m_chart.Perspective;
		}
		set
		{
			m_chart.Perspective = value;
		}
	}

	public int HeightPercent
	{
		get
		{
			return m_chart.HeightPercent;
		}
		set
		{
			m_chart.HeightPercent = value;
		}
	}

	public int DepthPercent
	{
		get
		{
			return m_chart.DepthPercent;
		}
		set
		{
			m_chart.DepthPercent = value;
		}
	}

	public int GapDepth
	{
		get
		{
			return m_chart.GapDepth;
		}
		set
		{
			m_chart.GapDepth = value;
		}
	}

	public bool RightAngleAxes
	{
		get
		{
			return m_chart.RightAngleAxes;
		}
		set
		{
			m_chart.RightAngleAxes = value;
		}
	}

	public bool AutoScaling
	{
		get
		{
			return m_chart.AutoScaling;
		}
		set
		{
			m_chart.AutoScaling = value;
		}
	}

	public bool WallsAndGridlines2D
	{
		get
		{
			return m_chart.WallsAndGridlines2D;
		}
		set
		{
			m_chart.WallsAndGridlines2D = value;
		}
	}

	public IShapes Shapes => m_chart.Shapes;

	public OfficeChartType PivotChartType
	{
		get
		{
			return m_chart.PivotChartType;
		}
		set
		{
			m_chart.PivotChartType = value;
		}
	}

	public bool ShowAllFieldButtons
	{
		get
		{
			return m_chart.ShowAllFieldButtons;
		}
		set
		{
			m_chart.ShowAllFieldButtons = value;
		}
	}

	public bool ShowValueFieldButtons
	{
		get
		{
			return m_chart.ShowValueFieldButtons;
		}
		set
		{
			m_chart.ShowValueFieldButtons = value;
		}
	}

	public bool ShowAxisFieldButtons
	{
		get
		{
			return m_chart.ShowAxisFieldButtons;
		}
		set
		{
			m_chart.ShowAxisFieldButtons = value;
		}
	}

	public bool ShowLegendFieldButtons
	{
		get
		{
			return m_chart.ShowLegendFieldButtons;
		}
		set
		{
			m_chart.ShowLegendFieldButtons = value;
		}
	}

	public bool ShowReportFilterFieldButtons
	{
		get
		{
			return m_chart.ShowReportFilterFieldButtons;
		}
		set
		{
			m_chart.ShowReportFilterFieldButtons = value;
		}
	}

	public OfficeChartType ChartType
	{
		get
		{
			return m_chart.ChartType;
		}
		set
		{
			m_chart.ChartType = value;
		}
	}

	public IOfficeDataRange DataRange
	{
		get
		{
			return m_chart.DataRange;
		}
		set
		{
		}
	}

	public bool IsSeriesInRows
	{
		get
		{
			return m_chart.IsSeriesInRows;
		}
		set
		{
			m_chart.IsSeriesInRows = value;
		}
	}

	public string ChartTitle
	{
		get
		{
			return m_chart.ChartTitle;
		}
		set
		{
			m_chart.ChartTitle = value;
		}
	}

	public IOfficeChartTextArea ChartTitleArea => m_chart.ChartTitleArea;

	public string CategoryAxisTitle
	{
		get
		{
			return m_chart.CategoryAxisTitle;
		}
		set
		{
			m_chart.CategoryAxisTitle = value;
		}
	}

	public string ValueAxisTitle
	{
		get
		{
			return m_chart.ValueAxisTitle;
		}
		set
		{
			m_chart.ValueAxisTitle = value;
		}
	}

	public string SecondaryCategoryAxisTitle
	{
		get
		{
			return m_chart.SecondaryCategoryAxisTitle;
		}
		set
		{
			m_chart.SecondaryCategoryAxisTitle = value;
		}
	}

	public string SecondaryValueAxisTitle
	{
		get
		{
			return m_chart.SecondaryValueAxisTitle;
		}
		set
		{
			m_chart.SecondaryValueAxisTitle = value;
		}
	}

	public string SeriesAxisTitle
	{
		get
		{
			return m_chart.SeriesAxisTitle;
		}
		set
		{
			m_chart.SeriesAxisTitle = value;
		}
	}

	public IOfficeChartPageSetup PageSetup => m_chart.PageSetup;

	public double XPos
	{
		get
		{
			return m_chart.XPos;
		}
		set
		{
			m_chart.XPos = value;
		}
	}

	public double YPos
	{
		get
		{
			return m_chart.YPos;
		}
		set
		{
			m_chart.YPos = value;
		}
	}

	double IOfficeChart.Width
	{
		get
		{
			return m_chart.Width;
		}
		set
		{
			m_chart.Width = value;
		}
	}

	double IOfficeChart.Height
	{
		get
		{
			return m_chart.Height;
		}
		set
		{
			m_chart.Height = value;
		}
	}

	public IOfficeChartSeries Series => m_chart.Series;

	public IOfficeChartCategoryAxis PrimaryCategoryAxis => m_chart.PrimaryCategoryAxis;

	public IOfficeChartValueAxis PrimaryValueAxis => m_chart.PrimaryValueAxis;

	public IOfficeChartSeriesAxis PrimarySerieAxis => m_chart.PrimarySerieAxis;

	public IOfficeChartCategoryAxis SecondaryCategoryAxis => m_chart.SecondaryCategoryAxis;

	public IOfficeChartValueAxis SecondaryValueAxis => m_chart.SecondaryValueAxis;

	public IOfficeChartFrameFormat ChartArea => m_chart.ChartArea;

	public IOfficeChartFrameFormat PlotArea => m_chart.PlotArea;

	public ChartFormatCollection PrimaryFormats => m_chart.PrimaryFormats;

	public ChartFormatCollection SecondaryFormats => m_chart.SecondaryFormats;

	public IOfficeChartShapes Charts
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	public OfficeKnownColors TabColor
	{
		get
		{
			throw new NotSupportedException();
		}
		set
		{
			throw new NotSupportedException();
		}
	}

	public bool IsRightToLeft
	{
		get
		{
			return m_chart.IsRightToLeft;
		}
		set
		{
			m_chart.IsRightToLeft = value;
		}
	}

	public Color TabColorRGB
	{
		get
		{
			throw new NotSupportedException();
		}
		set
		{
			throw new NotSupportedException();
		}
	}

	public IOfficeChartWallOrFloor Walls => m_chart.Walls;

	public IOfficeChartWallOrFloor SideWall => m_chart.SideWall;

	public IOfficeChartWallOrFloor BackWall => m_chart.Walls;

	public IOfficeChartWallOrFloor Floor => m_chart.Floor;

	public IOfficeChartDataTable DataTable => m_chart.DataTable;

	public bool IsSelected
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	public bool HasDataTable
	{
		get
		{
			return m_chart.HasDataTable;
		}
		set
		{
			m_chart.HasDataTable = value;
		}
	}

	public bool HasLegend
	{
		get
		{
			return m_chart.HasLegend;
		}
		set
		{
			m_chart.HasLegend = value;
		}
	}

	public bool HasTitle
	{
		get
		{
			return m_chart.HasTitle;
		}
		set
		{
			m_chart.HasTitle = value;
		}
	}

	public IOfficeChartLegend Legend => m_chart.Legend;

	public bool HasPlotArea
	{
		get
		{
			return m_chart.HasPlotArea;
		}
		set
		{
			m_chart.HasPlotArea = value;
		}
	}

	public int TabIndex
	{
		get
		{
			throw new NotSupportedException("This property is not supported for embedded charts.");
		}
	}

	public OfficeWorksheetVisibility Visibility
	{
		get
		{
			throw new NotSupportedException();
		}
		set
		{
			throw new NotSupportedException();
		}
	}

	public OfficeChartPlotEmpty DisplayBlanksAs
	{
		get
		{
			return m_chart.DisplayBlanksAs;
		}
		set
		{
			m_chart.DisplayBlanksAs = value;
		}
	}

	public bool PlotVisibleOnly
	{
		get
		{
			return m_chart.PlotVisibleOnly;
		}
		set
		{
			m_chart.PlotVisibleOnly = value;
		}
	}

	public bool SizeWithWindow
	{
		get
		{
			return m_chart.SizeWithWindow;
		}
		set
		{
			m_chart.SizeWithWindow = value;
		}
	}

	public ITextBoxes TextBoxes => m_chart.TextBoxes;

	public string CodeName => m_chart.CodeName;

	public bool ProtectContents
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	public bool ProtectDrawingObjects
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	public bool ProtectScenarios
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	public OfficeSheetProtection Protection
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	public bool IsPasswordProtected
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	public ChartShapeImpl(IApplication application, object parent, ChartShapeImpl instance, Dictionary<string, string> hashNewNames, Dictionary<int, int> dicFontIndexes)
		: base(application, parent, instance)
	{
		m_chart = instance.m_chart.Clone(hashNewNames, this, dicFontIndexes);
		m_bIsDisposed = instance.m_bIsDisposed;
		m_iBottomRow = instance.m_iBottomRow;
		m_iLeftColumn = instance.m_iLeftColumn;
		m_iRightColumn = instance.m_iRightColumn;
		m_iTopRow = instance.m_iTopRow;
	}

	public ChartShapeImpl(IApplication application, object parent)
		: base(application, parent)
	{
		m_chart = new ChartImpl(application, this);
		base.ShapeType = OfficeShapeType.Chart;
		BottomRow = 20;
		RightColumn = 10;
		m_bSupportOptions = false;
	}

	[CLSCompliant(false)]
	public ChartShapeImpl(IApplication application, object parent, MsofbtSpContainer container, OfficeParseOptions options)
		: base(application, parent, container, options)
	{
		base.ShapeType = OfficeShapeType.Chart;
		m_bSupportOptions = false;
	}

	public void Refresh()
	{
		m_chart.Refresh();
	}

	public void Activate()
	{
		throw new NotSupportedException();
	}

	public void Select()
	{
		throw new NotSupportedException();
	}

	public void Unselect()
	{
		throw new NotSupportedException();
	}

	public void Protect(string password)
	{
		throw new NotSupportedException();
	}

	public void Protect(string password, OfficeSheetProtection options)
	{
		throw new NotSupportedException();
	}

	public void Unprotect(string password)
	{
		throw new NotSupportedException();
	}

	public override IShape Clone(object parent, Dictionary<string, string> hashNewNames, Dictionary<int, int> dicFontIndexes, bool addToCollections)
	{
		ChartShapeImpl chartShapeImpl = new ChartShapeImpl(base.Application, parent, this, hashNewNames, dicFontIndexes);
		WorksheetBaseImpl worksheetBaseImpl = CommonObject.FindParent(chartShapeImpl.Parent, typeof(WorksheetBaseImpl), bSubTypes: true) as WorksheetBaseImpl;
		if (addToCollections)
		{
			worksheetBaseImpl.InnerShapes.AddShape(chartShapeImpl);
		}
		return chartShapeImpl;
	}

	public override void UpdateFormula(int iCurIndex, int iSourceIndex, Rectangle sourceRect, int iDestIndex, Rectangle destRect)
	{
		m_chart.UpdateFormula(iCurIndex, iSourceIndex, sourceRect, iDestIndex, destRect);
	}

	public override void RegisterInSubCollection()
	{
		m_shapes.WorksheetBase.InnerCharts.InnerAddChart(this);
	}

	protected override void OnPrepareForSerialization()
	{
		if (m_shape == null)
		{
			m_shape = (MsofbtSp)MsoFactory.GetRecord(MsoRecords.msofbtSp);
		}
		m_shape.Version = 2;
		m_shape.Instance = 201;
		m_shape.IsHaveAnchor = true;
		m_shape.IsHaveSpt = true;
	}

	[CLSCompliant(false)]
	protected override void ParseClientData(MsofbtClientData clientData, OfficeParseOptions options)
	{
		base.ParseClientData(clientData, options);
		int iPos = 1;
		BiffRecordRaw[] additionalData = clientData.AdditionalData;
		m_chart = new ChartImpl(base.Application, this, additionalData, ref iPos, options);
		if ((options & OfficeParseOptions.DoNotParseCharts) == 0)
		{
			m_chart.Parse();
		}
	}

	[CLSCompliant(false)]
	protected override void SerializeShape(MsofbtSpgrContainer spgrContainer)
	{
		if (spgrContainer == null)
		{
			throw new ArgumentNullException("spgrContainer");
		}
		MsofbtSpContainer msofbtSpContainer = (MsofbtSpContainer)MsoFactory.GetRecord(MsoRecords.msofbtSpContainer);
		MsofbtClientAnchor msofbtClientAnchor = (MsofbtClientAnchor)MsoFactory.GetRecord(MsoRecords.msofbtClientAnchor);
		MsofbtClientData msofbtClientData = (MsofbtClientData)MsoFactory.GetRecord(MsoRecords.msofbtClientData);
		OffsetArrayList records = new OffsetArrayList();
		ftCmo ftCmo = null;
		if (base.Obj == null)
		{
			OBJRecord oBJRecord = (OBJRecord)BiffRecordFactory.GetRecord(TBIFFRecord.OBJ);
			ftCmo = new ftCmo();
			ftCmo.ObjectType = TObjType.otChart;
			ftCmo.Printable = true;
			ftEnd record = new ftEnd();
			oBJRecord.AddSubRecord(ftCmo);
			oBJRecord.AddSubRecord(record);
			SetObject(oBJRecord);
		}
		else
		{
			ftCmo = base.Obj.RecordsList[0] as ftCmo;
		}
		ftCmo.ID = ((base.OldObjId != 0) ? ((ushort)base.OldObjId) : ((ushort)base.ParentWorkbook.CurrentObjectId));
		msofbtClientData.AddRecord(base.Obj);
		m_chart.EMUWidth = ApplicationImpl.ConvertFromPixel(((IShape)this).Width, MeasureUnits.Point);
		m_chart.EMUHeight = ApplicationImpl.ConvertFromPixel(((IShape)this).Height, MeasureUnits.Point);
		m_chart.Serialize(records);
		msofbtClientData.AddRecordRange(records);
		if (base.ClientAnchor == null)
		{
			msofbtClientAnchor.Options = 3;
			msofbtClientAnchor.LeftColumn = m_iLeftColumn;
			msofbtClientAnchor.RightColumn = m_iRightColumn;
			msofbtClientAnchor.TopRow = m_iTopRow;
			msofbtClientAnchor.BottomRow = m_iBottomRow;
			msofbtClientAnchor.LeftOffset = 0;
			msofbtClientAnchor.RightOffset = 0;
			msofbtClientAnchor.TopOffset = 0;
			msofbtClientAnchor.BottomOffset = 0;
		}
		else
		{
			msofbtClientAnchor = base.ClientAnchor;
		}
		msofbtSpContainer.AddItem(m_shape);
		MsofbtOPT msofbtOPT = SerializeOptions(msofbtSpContainer);
		msofbtOPT.Version = 3;
		msofbtOPT.Instance = 8;
		if (msofbtOPT.Properties.Length != 0)
		{
			msofbtSpContainer.AddItem(msofbtOPT);
		}
		msofbtSpContainer.AddItem(msofbtClientAnchor);
		msofbtSpContainer.AddItem(msofbtClientData);
		spgrContainer.AddItem(msofbtSpContainer);
	}

	[CLSCompliant(false)]
	public override void ParseClientAnchor(MsofbtClientAnchor clientAnchor)
	{
		base.ParseClientAnchor(clientAnchor);
		m_iBottomRow = clientAnchor.BottomRow;
		m_iTopRow = clientAnchor.TopRow;
		m_iLeftColumn = clientAnchor.LeftColumn;
		m_iRightColumn = clientAnchor.RightColumn;
	}

	[CLSCompliant(false)]
	protected override MsofbtOPT SerializeOptions(MsoBase parent)
	{
		if (m_bUpdateLineFill || m_options == null)
		{
			MsofbtOPT msofbtOPT = base.SerializeOptions(parent);
			SerializeSizeTextToFit(msofbtOPT);
			SerializeOptionSorted(msofbtOPT, MsoOptions.ForeColor, 134217806u);
			SerializeOptionSorted(msofbtOPT, MsoOptions.BackColor, 134217805u);
			SerializeHitTest(msofbtOPT);
			SerializeOptionSorted(msofbtOPT, MsoOptions.LineColor, 134217805u);
			SerializeOptionSorted(msofbtOPT, MsoOptions.NoLineDrawDash, 524296u);
			SerializeOptionSorted(msofbtOPT, MsoOptions.ShadowObscured, 131072u);
			SerializeShapeName(msofbtOPT);
			return msofbtOPT;
		}
		return m_options;
	}

	[CLSCompliant(false)]
	protected override MsofbtOPT CreateDefaultOptions()
	{
		MsofbtOPT msofbtOPT = base.CreateDefaultOptions();
		msofbtOPT.Version = 3;
		msofbtOPT.Instance = 8;
		SerializeOption(msofbtOPT, MsoOptions.LockAgainstGrouping, 17039620u);
		return msofbtOPT;
	}

	protected override void SetParents()
	{
		base.SetParents();
		m_worksheet = m_shapes.WorksheetBase;
		m_worksheet.InnerCharts.InnerAddChart(this);
	}

	public static implicit operator WorksheetBaseImpl(ChartShapeImpl chartShape)
	{
		return chartShape.ChartObject;
	}

	public void SetChartData(object[][] data)
	{
		ChartData.SetChartData(data);
	}

	public void SetDataRange(object[][] data, int rowIndex, int columnIndex)
	{
		ChartData.SetDataRange(data, rowIndex, columnIndex);
	}

	public void SetDataRange(IEnumerable enumerable, int rowIndex, int columnIndex)
	{
		ChartData.SetDataRange(enumerable, rowIndex, columnIndex);
	}
}
