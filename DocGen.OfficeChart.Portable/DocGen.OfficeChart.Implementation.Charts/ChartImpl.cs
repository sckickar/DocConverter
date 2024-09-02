using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DocGen.Drawing;
using DocGen.OfficeChart.Implementation.Shapes;
using DocGen.OfficeChart.Implementation.XmlReaders;
using DocGen.OfficeChart.Implementation.XmlSerialization;
using DocGen.OfficeChart.Interfaces;
using DocGen.OfficeChart.Parser;
using DocGen.OfficeChart.Parser.Biff_Records;
using DocGen.OfficeChart.Parser.Biff_Records.Charts;

namespace DocGen.OfficeChart.Implementation.Charts;

internal class ChartImpl : WorksheetBaseImpl, IOfficeChart, ISerializableNamedObject, INamedObject, IParseable, ICloneParent
{
	internal const string DefaultChartTitle = "Chart Title";

	internal const string DefaultTitleFontName = "Calibri";

	internal const double DefaultTitleFontSize = 18.6;

	public const string DEF_FIRST_SERIE_NAME = "Serie1";

	public const OfficeChartType DEFAULT_CHART_TYPE = OfficeChartType.Column_Clustered;

	public const string PREFIX_3D = "_3D";

	public const string PREFIX_BAR = "_Bar";

	public const string PREFIX_CLUSTERED = "_Clustered";

	public const string PREFIX_CONTOUR = "_Contour";

	public const string PREFIX_EXPLODED = "_Exploded";

	public const string PREFIX_LINE = "_Line";

	public const string PREFIX_MARKERS = "_Markers";

	public const string PREFIX_NOCOLOR = "_NoColor";

	public const string PREFIX_SHOW_PERCENT = "_100";

	public const string PREFIX_SMOOTHEDLINE = "_SmoothedLine";

	public const string PREFIX_STACKED = "_Stacked";

	public const string START_AREA = "Area";

	public const string START_BAR = "Bar";

	public const string START_BUBBLE = "Bubble";

	public const string START_COLUMN = "Column";

	public const string START_CONE = "Cone";

	public const string START_CYLINDER = "Cylinder";

	public const string START_DOUGHNUT = "Doughnut";

	public const string START_LINE = "Line";

	public const string START_PIE = "Pie";

	public const string START_PYRAMID = "Pyramid";

	public const string START_RADAR = "Radar";

	public const string START_SCATTER = "Scatter";

	public const string START_SURFACE = "Surface";

	private const int DEF_PRIMARY_INDEX = 0;

	public const int DEF_SI_VALUE = 1;

	public const int DEF_SI_CATEGORY = 2;

	public const int DEF_SI_BUBBLE = 3;

	private const int DEF_SECONDARY_INDEX = 1;

	private const int MaximumFontCount = 506;

	internal const int DefaultPlotAreaX = 328;

	internal const int DefaultPlotAreaY = 243;

	internal const int DefaultPlotAreaXLength = 3125;

	internal const int DefaultPlotAreaYLength = 3283;

	public static readonly string[] DEF_LEGEND_NEED_DATA_POINT = new string[3] { "Pie", "Doughnut", "Surface" };

	public static readonly OfficeChartType[] DEF_SUPPORT_SERIES_AXIS = new OfficeChartType[10]
	{
		OfficeChartType.Surface_3D,
		OfficeChartType.Surface_Contour,
		OfficeChartType.Surface_NoColor_Contour,
		OfficeChartType.Surface_NoColor_3D,
		OfficeChartType.Column_3D,
		OfficeChartType.Line_3D,
		OfficeChartType.Area_3D,
		OfficeChartType.Pyramid_Clustered_3D,
		OfficeChartType.Cone_Clustered_3D,
		OfficeChartType.Cylinder_Clustered_3D
	};

	public static readonly OfficeChartType[] DEF_UNSUPPORT_PIVOT_CHART = new OfficeChartType[11]
	{
		OfficeChartType.Scatter_Line,
		OfficeChartType.Scatter_Line_Markers,
		OfficeChartType.Scatter_Markers,
		OfficeChartType.Scatter_SmoothedLine,
		OfficeChartType.Scatter_SmoothedLine_Markers,
		OfficeChartType.Stock_HighLowClose,
		OfficeChartType.Stock_OpenHighLowClose,
		OfficeChartType.Stock_VolumeHighLowClose,
		OfficeChartType.Stock_VolumeOpenHighLowClose,
		OfficeChartType.Bubble,
		OfficeChartType.Bubble_3D
	};

	public static readonly string[] DEF_SUPPORT_DATA_TABLE = new string[8] { "Column", "Bar", "Line", "Area", "Cylinder", "Cone", "Pyramid", "Stock" };

	public static readonly string[] DEF_SUPPORT_ERROR_BARS = new string[6] { "Column", "Bar", "Line", "Area", "Scatter", "Bubble" };

	public static readonly OfficeChartType[] DEF_SUPPORT_TREND_LINES = new OfficeChartType[16]
	{
		OfficeChartType.Column_Clustered,
		OfficeChartType.Bar_Clustered,
		OfficeChartType.Line,
		OfficeChartType.Line_Markers,
		OfficeChartType.Scatter_Line,
		OfficeChartType.Scatter_Line_Markers,
		OfficeChartType.Scatter_Markers,
		OfficeChartType.Scatter_SmoothedLine,
		OfficeChartType.Scatter_SmoothedLine_Markers,
		OfficeChartType.Stock_HighLowClose,
		OfficeChartType.Stock_OpenHighLowClose,
		OfficeChartType.Stock_VolumeHighLowClose,
		OfficeChartType.Stock_VolumeOpenHighLowClose,
		OfficeChartType.Area,
		OfficeChartType.Bubble,
		OfficeChartType.Bubble_3D
	};

	public static readonly OfficeChartType[] DEF_WALLS_OR_FLOOR_TYPES = new OfficeChartType[34]
	{
		OfficeChartType.Column_3D,
		OfficeChartType.Column_Clustered_3D,
		OfficeChartType.Column_Stacked_100_3D,
		OfficeChartType.Column_Stacked_3D,
		OfficeChartType.Bar_Clustered_3D,
		OfficeChartType.Bar_Stacked_3D,
		OfficeChartType.Bar_Stacked_100_3D,
		OfficeChartType.Line_3D,
		OfficeChartType.Area_3D,
		OfficeChartType.Area_Stacked_3D,
		OfficeChartType.Area_Stacked_100_3D,
		OfficeChartType.Cylinder_Clustered,
		OfficeChartType.Cylinder_Stacked,
		OfficeChartType.Cylinder_Stacked_100,
		OfficeChartType.Cylinder_Bar_Clustered,
		OfficeChartType.Cylinder_Bar_Stacked,
		OfficeChartType.Cylinder_Bar_Stacked_100,
		OfficeChartType.Cylinder_Clustered_3D,
		OfficeChartType.Cone_Clustered,
		OfficeChartType.Cone_Stacked,
		OfficeChartType.Cone_Stacked_100,
		OfficeChartType.Cone_Bar_Clustered,
		OfficeChartType.Cone_Bar_Stacked,
		OfficeChartType.Cone_Bar_Stacked_100,
		OfficeChartType.Cone_Clustered_3D,
		OfficeChartType.Pyramid_Clustered,
		OfficeChartType.Pyramid_Stacked,
		OfficeChartType.Pyramid_Stacked_100,
		OfficeChartType.Pyramid_Bar_Clustered,
		OfficeChartType.Pyramid_Bar_Stacked,
		OfficeChartType.Pyramid_Bar_Stacked_100,
		OfficeChartType.Pyramid_Clustered_3D,
		OfficeChartType.Surface_3D,
		OfficeChartType.Surface_NoColor_3D
	};

	private static readonly OfficeAxisType[] DEF_SECONDARY_AXES_TYPES = new OfficeAxisType[2]
	{
		OfficeAxisType.Category,
		OfficeAxisType.Value
	};

	public static readonly OfficeChartType[] DEF_NOT_3D = new OfficeChartType[17]
	{
		OfficeChartType.Scatter_Markers,
		OfficeChartType.Scatter_SmoothedLine_Markers,
		OfficeChartType.Scatter_SmoothedLine,
		OfficeChartType.Scatter_Line_Markers,
		OfficeChartType.Scatter_Line,
		OfficeChartType.Doughnut,
		OfficeChartType.Doughnut_Exploded,
		OfficeChartType.Radar,
		OfficeChartType.Radar_Markers,
		OfficeChartType.Radar_Filled,
		OfficeChartType.Bubble,
		OfficeChartType.Bubble_3D,
		OfficeChartType.Stock_HighLowClose,
		OfficeChartType.Stock_OpenHighLowClose,
		OfficeChartType.Stock_VolumeHighLowClose,
		OfficeChartType.Stock_VolumeOpenHighLowClose,
		OfficeChartType.Combination_Chart
	};

	public static readonly OfficeChartType[] DEF_CHANGE_SERIE = new OfficeChartType[31]
	{
		OfficeChartType.Column_Clustered,
		OfficeChartType.Column_Stacked,
		OfficeChartType.Column_Stacked_100,
		OfficeChartType.Bar_Clustered,
		OfficeChartType.Bar_Stacked,
		OfficeChartType.Bar_Stacked_100,
		OfficeChartType.Line,
		OfficeChartType.Line_Stacked,
		OfficeChartType.Line_Stacked_100,
		OfficeChartType.Line_Markers,
		OfficeChartType.Line_Markers_Stacked,
		OfficeChartType.Line_Markers_Stacked_100,
		OfficeChartType.Pie,
		OfficeChartType.PieOfPie,
		OfficeChartType.Pie_Exploded,
		OfficeChartType.Pie_Bar,
		OfficeChartType.Scatter_Markers,
		OfficeChartType.Scatter_SmoothedLine_Markers,
		OfficeChartType.Scatter_SmoothedLine,
		OfficeChartType.Scatter_Line_Markers,
		OfficeChartType.Scatter_Line,
		OfficeChartType.Area,
		OfficeChartType.Area_Stacked,
		OfficeChartType.Area_Stacked_100,
		OfficeChartType.Doughnut,
		OfficeChartType.Doughnut_Exploded,
		OfficeChartType.Radar,
		OfficeChartType.Radar_Markers,
		OfficeChartType.Radar_Filled,
		OfficeChartType.Bubble,
		OfficeChartType.Bubble_3D
	};

	public static readonly OfficeChartType[] DEF_NOT_SUPPORT_GRIDLINES = new OfficeChartType[8]
	{
		OfficeChartType.Doughnut,
		OfficeChartType.Doughnut_Exploded,
		OfficeChartType.Pie_Bar,
		OfficeChartType.Pie_Exploded,
		OfficeChartType.PieOfPie,
		OfficeChartType.Pie,
		OfficeChartType.Pie_3D,
		OfficeChartType.Pie_Exploded_3D
	};

	public static readonly OfficeChartType[] DEF_NEED_SECONDARY_AXIS = new OfficeChartType[11]
	{
		OfficeChartType.Doughnut,
		OfficeChartType.Doughnut_Exploded,
		OfficeChartType.Pie_Bar,
		OfficeChartType.Pie_Exploded,
		OfficeChartType.PieOfPie,
		OfficeChartType.Pie,
		OfficeChartType.Radar,
		OfficeChartType.Radar_Filled,
		OfficeChartType.Bar_Clustered,
		OfficeChartType.Bar_Stacked,
		OfficeChartType.Bar_Stacked_100
	};

	public static readonly OfficeChartType[] DEF_COMBINATION_CHART = new OfficeChartType[5]
	{
		OfficeChartType.Stock_HighLowClose,
		OfficeChartType.Stock_OpenHighLowClose,
		OfficeChartType.Stock_VolumeHighLowClose,
		OfficeChartType.Stock_VolumeOpenHighLowClose,
		OfficeChartType.Combination_Chart
	};

	public static readonly string[] DEF_PRIORITY_START_TYPES = new string[8] { "Pie", "Doughnut", "Radar", "Area", "Column", "Bar", "Line", "Scatter" };

	public static readonly OfficeChartType[] DEF_CHANGE_INTIMATE = new OfficeChartType[16]
	{
		OfficeChartType.Radar,
		OfficeChartType.Radar_Markers,
		OfficeChartType.Radar_Filled,
		OfficeChartType.Scatter_Markers,
		OfficeChartType.Scatter_SmoothedLine_Markers,
		OfficeChartType.Scatter_SmoothedLine,
		OfficeChartType.Scatter_Line_Markers,
		OfficeChartType.Scatter_Line,
		OfficeChartType.Line,
		OfficeChartType.Line_Stacked,
		OfficeChartType.Line_Stacked_100,
		OfficeChartType.Line_Markers,
		OfficeChartType.Line_Markers_Stacked,
		OfficeChartType.Line_Markers_Stacked_100,
		OfficeChartType.Bubble,
		OfficeChartType.Bubble_3D
	};

	public static readonly OfficeChartType[] DEF_DONT_NEED_PLOT = new OfficeChartType[13]
	{
		OfficeChartType.Doughnut,
		OfficeChartType.Doughnut_Exploded,
		OfficeChartType.Pie_Bar,
		OfficeChartType.Pie_Exploded,
		OfficeChartType.PieOfPie,
		OfficeChartType.Pie,
		OfficeChartType.Pie_3D,
		OfficeChartType.Pie_Exploded_3D,
		OfficeChartType.Radar,
		OfficeChartType.Radar_Markers,
		OfficeChartType.Radar_Filled,
		OfficeChartType.Surface_Contour,
		OfficeChartType.Surface_NoColor_Contour
	};

	public static readonly OfficeChartType[] DEF_NEED_VIEW_3D = new OfficeChartType[4]
	{
		OfficeChartType.Surface_3D,
		OfficeChartType.Surface_Contour,
		OfficeChartType.Surface_NoColor_3D,
		OfficeChartType.Surface_NoColor_Contour
	};

	public static readonly OfficeChartType[] CHARTS_100 = new OfficeChartType[14]
	{
		OfficeChartType.Column_Stacked_100,
		OfficeChartType.Column_Stacked_100_3D,
		OfficeChartType.Bar_Stacked_100,
		OfficeChartType.Bar_Stacked_100_3D,
		OfficeChartType.Line_Stacked_100,
		OfficeChartType.Line_Markers_Stacked_100,
		OfficeChartType.Area_Stacked_100,
		OfficeChartType.Area_Stacked_100_3D,
		OfficeChartType.Cylinder_Stacked_100,
		OfficeChartType.Cylinder_Bar_Stacked_100,
		OfficeChartType.Cone_Stacked_100,
		OfficeChartType.Cone_Bar_Stacked_100,
		OfficeChartType.Pyramid_Stacked_100,
		OfficeChartType.Pyramid_Bar_Stacked_100
	};

	public static readonly OfficeChartType[] STACKEDCHARTS = new OfficeChartType[28]
	{
		OfficeChartType.Column_Stacked,
		OfficeChartType.Column_Stacked_3D,
		OfficeChartType.Bar_Stacked,
		OfficeChartType.Bar_Stacked_3D,
		OfficeChartType.Line_Stacked,
		OfficeChartType.Line_Markers_Stacked,
		OfficeChartType.Area_Stacked,
		OfficeChartType.Area_Stacked_3D,
		OfficeChartType.Cylinder_Stacked,
		OfficeChartType.Cylinder_Bar_Stacked,
		OfficeChartType.Cone_Stacked,
		OfficeChartType.Cone_Bar_Stacked,
		OfficeChartType.Pyramid_Stacked,
		OfficeChartType.Pyramid_Bar_Stacked,
		OfficeChartType.Column_Stacked_100,
		OfficeChartType.Column_Stacked_100_3D,
		OfficeChartType.Bar_Stacked_100,
		OfficeChartType.Bar_Stacked_100_3D,
		OfficeChartType.Line_Stacked_100,
		OfficeChartType.Line_Markers_Stacked_100,
		OfficeChartType.Area_Stacked_100,
		OfficeChartType.Area_Stacked_100_3D,
		OfficeChartType.Cylinder_Stacked_100,
		OfficeChartType.Cylinder_Bar_Stacked_100,
		OfficeChartType.Cone_Stacked_100,
		OfficeChartType.Cone_Bar_Stacked_100,
		OfficeChartType.Pyramid_Stacked_100,
		OfficeChartType.Pyramid_Bar_Stacked_100
	};

	public static readonly OfficeChartType[] CHARTS3D = new OfficeChartType[39]
	{
		OfficeChartType.Column_Clustered_3D,
		OfficeChartType.Column_Stacked_3D,
		OfficeChartType.Column_Stacked_100_3D,
		OfficeChartType.Column_3D,
		OfficeChartType.Bar_Clustered_3D,
		OfficeChartType.Bar_Stacked_3D,
		OfficeChartType.Bar_Stacked_100_3D,
		OfficeChartType.Bubble_3D,
		OfficeChartType.Line_3D,
		OfficeChartType.Pie_3D,
		OfficeChartType.Pie_Exploded_3D,
		OfficeChartType.Area_3D,
		OfficeChartType.Area_Stacked_3D,
		OfficeChartType.Area_Stacked_100_3D,
		OfficeChartType.Surface_3D,
		OfficeChartType.Surface_NoColor_3D,
		OfficeChartType.Surface_Contour,
		OfficeChartType.Surface_NoColor_Contour,
		OfficeChartType.Cylinder_Clustered,
		OfficeChartType.Cylinder_Stacked,
		OfficeChartType.Cylinder_Stacked_100,
		OfficeChartType.Cylinder_Bar_Clustered,
		OfficeChartType.Cylinder_Bar_Stacked,
		OfficeChartType.Cylinder_Bar_Stacked_100,
		OfficeChartType.Cylinder_Clustered_3D,
		OfficeChartType.Cone_Clustered,
		OfficeChartType.Cone_Stacked,
		OfficeChartType.Cone_Stacked_100,
		OfficeChartType.Cone_Bar_Clustered,
		OfficeChartType.Cone_Bar_Stacked,
		OfficeChartType.Cone_Bar_Stacked_100,
		OfficeChartType.Cone_Clustered_3D,
		OfficeChartType.Pyramid_Clustered,
		OfficeChartType.Pyramid_Stacked,
		OfficeChartType.Pyramid_Stacked_100,
		OfficeChartType.Pyramid_Bar_Clustered,
		OfficeChartType.Pyramid_Bar_Stacked,
		OfficeChartType.Pyramid_Bar_Stacked_100,
		OfficeChartType.Pyramid_Clustered_3D
	};

	public static readonly OfficeChartType[] CHARTS_LINE = new OfficeChartType[7]
	{
		OfficeChartType.Line,
		OfficeChartType.Line_3D,
		OfficeChartType.Line_Markers,
		OfficeChartType.Line_Markers_Stacked,
		OfficeChartType.Line_Markers_Stacked_100,
		OfficeChartType.Line_Stacked,
		OfficeChartType.Line_Stacked_100
	};

	public static readonly OfficeChartType[] CHARTS_BUBBLE = new OfficeChartType[2]
	{
		OfficeChartType.Bubble,
		OfficeChartType.Bubble_3D
	};

	public static readonly OfficeChartType[] NO_CATEGORY_AXIS = new OfficeChartType[8]
	{
		OfficeChartType.Doughnut,
		OfficeChartType.Doughnut_Exploded,
		OfficeChartType.Pie,
		OfficeChartType.Pie_3D,
		OfficeChartType.Pie_Bar,
		OfficeChartType.Pie_Exploded,
		OfficeChartType.Pie_Exploded_3D,
		OfficeChartType.PieOfPie
	};

	public static readonly OfficeChartType[] CHARTS_VARYCOLOR = new OfficeChartType[8]
	{
		OfficeChartType.Doughnut,
		OfficeChartType.Doughnut_Exploded,
		OfficeChartType.Pie,
		OfficeChartType.Pie_3D,
		OfficeChartType.Pie_Bar,
		OfficeChartType.Pie_Exploded,
		OfficeChartType.Pie_Exploded_3D,
		OfficeChartType.PieOfPie
	};

	public static readonly OfficeChartType[] CHARTS_EXPLODED = new OfficeChartType[3]
	{
		OfficeChartType.Doughnut_Exploded,
		OfficeChartType.Pie_Exploded,
		OfficeChartType.Pie_Exploded_3D
	};

	private static readonly OfficeChartType[] CHART_SERIES_LINES = new OfficeChartType[2]
	{
		OfficeChartType.PieOfPie,
		OfficeChartType.Pie_Bar
	};

	public static readonly OfficeChartType[] CHARTS_SCATTER = new OfficeChartType[5]
	{
		OfficeChartType.Scatter_Markers,
		OfficeChartType.Scatter_Line_Markers,
		OfficeChartType.Scatter_Line,
		OfficeChartType.Scatter_SmoothedLine_Markers,
		OfficeChartType.Scatter_SmoothedLine
	};

	public static readonly OfficeChartType[] CHARTS_SMOOTHED_LINE = new OfficeChartType[2]
	{
		OfficeChartType.Scatter_SmoothedLine_Markers,
		OfficeChartType.Scatter_SmoothedLine
	};

	public static readonly OfficeChartType[] CHARTS_STOCK = new OfficeChartType[4]
	{
		OfficeChartType.Stock_HighLowClose,
		OfficeChartType.Stock_OpenHighLowClose,
		OfficeChartType.Stock_VolumeHighLowClose,
		OfficeChartType.Stock_VolumeOpenHighLowClose
	};

	public static readonly OfficeChartType[] CHARTS_PERSPECTIVE = new OfficeChartType[10]
	{
		OfficeChartType.Area_3D,
		OfficeChartType.Column_3D,
		OfficeChartType.Cone_Clustered_3D,
		OfficeChartType.Cylinder_Clustered_3D,
		OfficeChartType.Line_3D,
		OfficeChartType.Pyramid_Clustered_3D,
		OfficeChartType.Surface_3D,
		OfficeChartType.Surface_NoColor_3D,
		OfficeChartType.Surface_Contour,
		OfficeChartType.Surface_NoColor_Contour
	};

	public static readonly OfficeChartType[] CHARTS_CLUSTERED = new OfficeChartType[10]
	{
		OfficeChartType.Bar_Clustered,
		OfficeChartType.Bar_Clustered_3D,
		OfficeChartType.Column_Clustered,
		OfficeChartType.Column_Clustered_3D,
		OfficeChartType.Cone_Clustered,
		OfficeChartType.Cone_Bar_Clustered,
		OfficeChartType.Cylinder_Clustered,
		OfficeChartType.Cylinder_Bar_Clustered,
		OfficeChartType.Pyramid_Clustered,
		OfficeChartType.Pyramid_Bar_Clustered
	};

	public static readonly OfficeChartType[] CHARTS_WITH_PLOT_AREA = new OfficeChartType[26]
	{
		OfficeChartType.Column_Clustered,
		OfficeChartType.Scatter_Line_Markers,
		OfficeChartType.Scatter_SmoothedLine,
		OfficeChartType.Scatter_SmoothedLine_Markers,
		OfficeChartType.Line_Markers_Stacked_100,
		OfficeChartType.Stock_VolumeOpenHighLowClose,
		OfficeChartType.Line_Markers_Stacked,
		OfficeChartType.Stock_VolumeHighLowClose,
		OfficeChartType.Column_Stacked_100,
		OfficeChartType.Stock_OpenHighLowClose,
		OfficeChartType.Scatter_Line_Markers,
		OfficeChartType.Area_Stacked_100,
		OfficeChartType.Line_Stacked_100,
		OfficeChartType.Line_Markers,
		OfficeChartType.Stock_HighLowClose,
		OfficeChartType.Column_Stacked,
		OfficeChartType.Bar_Clustered,
		OfficeChartType.Bar_Stacked_100,
		OfficeChartType.Area_Stacked,
		OfficeChartType.Line_Stacked,
		OfficeChartType.Bar_Stacked,
		OfficeChartType.Bubble_3D,
		OfficeChartType.Scatter_Markers,
		OfficeChartType.Bubble,
		OfficeChartType.Area,
		OfficeChartType.Line
	};

	public static readonly OfficeLegendPosition[] LEGEND_VERTICAL = new OfficeLegendPosition[3]
	{
		OfficeLegendPosition.Right,
		OfficeLegendPosition.Corner,
		OfficeLegendPosition.Left
	};

	private static readonly byte[][] DEF_UNKNOWN_SERIE_LABEL = new byte[14][]
	{
		new byte[20]
		{
			80, 8, 0, 0, 10, 10, 3, 0, 80, 8,
			90, 8, 97, 8, 97, 8, 106, 8, 107, 8
		},
		new byte[12]
		{
			82, 8, 0, 0, 13, 0, 0, 0, 0, 0,
			0, 0
		},
		new byte[12]
		{
			82, 8, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0
		},
		new byte[12]
		{
			82, 8, 0, 0, 5, 0, 0, 0, 0, 0,
			0, 0
		},
		new byte[12]
		{
			106, 8, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0
		},
		new byte[12]
		{
			84, 8, 0, 0, 18, 0, 0, 0, 0, 0,
			0, 0
		},
		new byte[12]
		{
			81, 8, 0, 0, 36, 16, 2, 0, 0, 0,
			0, 0
		},
		new byte[40]
		{
			81, 8, 0, 0, 37, 16, 32, 0, 2, 2,
			1, 0, 0, 0, 0, 0, 169, 254, 255, 255,
			187, 254, 255, 255, 0, 0, 0, 0, 0, 0,
			0, 0, 177, 0, 77, 0, 80, 40, 0, 0
		},
		new byte[12]
		{
			81, 8, 0, 0, 51, 16, 0, 0, 0, 0,
			0, 0
		},
		new byte[28]
		{
			81, 8, 0, 0, 79, 16, 20, 0, 2, 0,
			2, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0
		},
		new byte[16]
		{
			81, 8, 0, 0, 81, 16, 8, 0, 0, 1,
			0, 0, 0, 0, 0, 0
		},
		new byte[14]
		{
			81, 8, 0, 0, 39, 16, 6, 0, 4, 0,
			0, 0, 0, 0
		},
		new byte[12]
		{
			81, 8, 0, 0, 52, 16, 0, 0, 0, 0,
			0, 0
		},
		new byte[12]
		{
			85, 8, 0, 0, 18, 0, 0, 0, 0, 0,
			0, 0
		}
	};

	public static readonly OfficeChartType[] DEF_SPECIAL_DATA_LABELS = new OfficeChartType[7]
	{
		OfficeChartType.Radar_Filled,
		OfficeChartType.Area,
		OfficeChartType.Area_3D,
		OfficeChartType.Area_Stacked,
		OfficeChartType.Area_Stacked_100,
		OfficeChartType.Area_Stacked_100_3D,
		OfficeChartType.Area_Stacked_3D
	};

	public static readonly OfficeChartType[] DEF_CHART_PERCENTAGE = new OfficeChartType[8]
	{
		OfficeChartType.Pie,
		OfficeChartType.Pie_3D,
		OfficeChartType.Pie_Bar,
		OfficeChartType.Pie_Exploded,
		OfficeChartType.Pie_Exploded_3D,
		OfficeChartType.PieOfPie,
		OfficeChartType.Doughnut,
		OfficeChartType.Doughnut_Exploded
	};

	private bool m_isThemeOverridePresent;

	private Dictionary<string, string> m_colorMapOverrideDictionary;

	private int m_linechartCount;

	private bool m_bParseDataOnDemand;

	private int m_iOverlap;

	private int m_gapWidth;

	private bool m_isStringRef;

	private bool m_bShowGapWidth;

	private bool m_bIsSecondaryAxis;

	private bool m_bInWorksheet;

	internal OfficeChartType m_chartType;

	private OfficeChartType m_pivotChartType;

	private IRange m_dataRange;

	private bool m_bSeriesInRows;

	private bool m_bHasDataTable;

	private ChartPageSetupImpl m_pageSetup;

	private double m_dXPos;

	private double m_dYPos;

	private double m_dWidth;

	private double m_dHeight;

	private List<ChartFbiRecord> m_arrFonts;

	private ChartSeriesCollection m_series;

	private ChartCategoryCollection m_categories;

	private ChartDataTableImpl m_dataTable;

	private ChartShtpropsRecord m_chartProperties;

	private ChartPlotGrowthRecord m_plotGrowth;

	private ChartPosRecord m_plotAreaBoundingBox;

	private ChartFrameFormatImpl m_chartArea;

	private ChartFrameFormatImpl m_plotAreaFrame;

	private TypedSortedListEx<int, List<BiffRecordRaw>> m_lstDefaultText = new TypedSortedListEx<int, List<BiffRecordRaw>>();

	private ChartTextAreaImpl m_title;

	private ChartParentAxisImpl m_primaryParentAxis;

	private ChartParentAxisImpl m_secondaryParentAxis;

	private ChartLegendImpl m_legend;

	private bool m_bHasLegend;

	private ChartWallOrFloorImpl m_walls;

	private ChartWallOrFloorImpl m_sidewall;

	private ChartWallOrFloorImpl m_floor;

	private ChartPlotAreaImpl m_plotArea;

	private bool m_bTypeChanging;

	private OfficeChartType m_destinationType;

	private List<BiffRecordRaw> m_trendList = new List<BiffRecordRaw>();

	private List<BiffRecordRaw> m_pivotList;

	private WindowZoomRecord m_chartChartZoom;

	private RelationCollection m_relations;

	private int m_iStyle2007;

	private Stream m_pivotFormatsStream;

	private Stream m_highlightStream;

	private bool m_bZoomToFit;

	private Dictionary<int, List<BiffRecordRaw>> m_dictReparseErrorBars;

	private Stream m_bandFormats;

	private string m_preservedPivotSource;

	private int m_formatId;

	private bool m_showAllFieldButtons = true;

	private bool m_showAxisFieldButtons = true;

	private bool m_showValueFieldButtons = true;

	private bool m_showLegendFieldButtons = true;

	private bool m_showReportFilterFieldButtons = true;

	private Stream m_alternateContent;

	internal bool m_bHasChartTitle;

	internal bool m_bIsPrimarySecondaryCategory;

	internal bool m_bIsPrimarySecondaryValue;

	private Stream m_defaultTextProperty;

	private FontWrapper m_font;

	private bool? m_hasAutoTitle;

	private List<int> m_axisIds;

	private ChartPlotAreaLayoutRecord m_plotAreaLayout;

	private bool[] category;

	private bool[] series;

	private OfficeSeriesNameLevel m_seriesNameLevel;

	private OfficeCategoriesLabelLevel m_categoriesLabelLevel;

	private bool m_showPlotVisible;

	private string m_radarStyle;

	internal bool m_bIsRadarTypeChanged;

	internal bool IsAddCopied;

	private object[] m_categoryLabelValues;

	private ChartData _chartData;

	private string m_formula;

	internal MemoryStream m_themeOverrideStream;

	internal List<Color> m_themeColors;

	private bool m_isChartCleared;

	private int m_activeSheetIndex;

	internal MemoryStream m_colorMapOverrideStream;

	internal bool m_isChartStyleSkipped;

	internal bool m_isChartColorStyleSkipped;

	private Dictionary<string, Stream> m_relationPreservedStreamCollections;

	internal Dictionary<int, ChartDataPointsCollection> CommonDataPointsCollection;

	private bool m_hasExternalWorkbook;

	private Dictionary<string, Stream> m_chartIteams;

	private bool m_isChartExternalRelation;

	private bool m_isChartParsed;

	internal string m_showDlbSOverMax = "";

	private ushort m_chartExTitlePosition;

	private bool m_chartTitleIncludeInLayout;

	private bool? m_isAutoUpdate;

	private string m_chartExRelationId;

	private IEnumerable<IGrouping<int, IOfficeChartSerie>> m_chartSerieGroupsBeforesorting;

	internal bool IsStock;

	internal bool HasExternalWorkbook
	{
		get
		{
			return m_hasExternalWorkbook;
		}
		set
		{
			m_hasExternalWorkbook = value;
		}
	}

	internal Dictionary<string, Stream> ChartIteams
	{
		get
		{
			if (m_chartIteams == null)
			{
				m_chartIteams = new Dictionary<string, Stream>();
			}
			return m_chartIteams;
		}
		set
		{
			m_chartIteams = value;
		}
	}

	internal bool IsStringRef
	{
		get
		{
			return m_isStringRef;
		}
		set
		{
			m_isStringRef = value;
		}
	}

	internal int OverLap
	{
		get
		{
			return m_iOverlap;
		}
		set
		{
			m_iOverlap = value;
		}
	}

	internal int GapWidth
	{
		get
		{
			return m_gapWidth;
		}
		set
		{
			m_gapWidth = value;
		}
	}

	internal bool ShowGapWidth
	{
		get
		{
			return m_bShowGapWidth;
		}
		set
		{
			m_bShowGapWidth = value;
		}
	}

	public int Rotation
	{
		get
		{
			return ChartFormat.Rotation;
		}
		set
		{
			ChartFormat.Rotation = value;
		}
	}

	public int Elevation
	{
		get
		{
			return ChartFormat.Elevation;
		}
		set
		{
			ChartFormat.Elevation = value;
		}
	}

	public int Perspective
	{
		get
		{
			return ChartFormat.Perspective;
		}
		set
		{
			ChartFormat.Perspective = value;
		}
	}

	internal OfficeChartType PivotChartType
	{
		get
		{
			return m_pivotChartType;
		}
		set
		{
			if (Series.Count != 0)
			{
				ChartType = value;
			}
			m_pivotChartType = value;
			CreateNecessaryAxes(bPrimary: true);
		}
	}

	public string PreservedPivotSource
	{
		get
		{
			return m_preservedPivotSource;
		}
		set
		{
			m_preservedPivotSource = value;
		}
	}

	public int FormatId
	{
		get
		{
			return m_formatId;
		}
		set
		{
			m_formatId = value;
		}
	}

	internal bool ShowAllFieldButtons
	{
		get
		{
			return m_showAllFieldButtons;
		}
		set
		{
			m_showAllFieldButtons = value;
		}
	}

	internal bool ShowValueFieldButtons
	{
		get
		{
			return m_showValueFieldButtons;
		}
		set
		{
			m_showValueFieldButtons = value;
		}
	}

	internal bool ShowAxisFieldButtons
	{
		get
		{
			return m_showAxisFieldButtons;
		}
		set
		{
			m_showAxisFieldButtons = value;
		}
	}

	internal bool ShowLegendFieldButtons
	{
		get
		{
			return m_showLegendFieldButtons;
		}
		set
		{
			m_showLegendFieldButtons = value;
		}
	}

	internal bool ShowReportFilterFieldButtons
	{
		get
		{
			return m_showReportFilterFieldButtons;
		}
		set
		{
			m_showReportFilterFieldButtons = value;
		}
	}

	public int HeightPercent
	{
		get
		{
			return ChartFormat.HeightPercent;
		}
		set
		{
			ChartFormat.HeightPercent = value;
		}
	}

	public int DepthPercent
	{
		get
		{
			return ChartFormat.DepthPercent;
		}
		set
		{
			ChartFormat.DepthPercent = value;
		}
	}

	public int GapDepth
	{
		get
		{
			return ChartFormat.GapDepth;
		}
		set
		{
			ChartFormat.GapDepth = value;
		}
	}

	public bool RightAngleAxes
	{
		get
		{
			return ChartFormat.RightAngleAxes;
		}
		set
		{
			ChartFormat.RightAngleAxes = value;
		}
	}

	public bool AutoScaling
	{
		get
		{
			return ChartFormat.AutoScaling;
		}
		set
		{
			ChartFormat.AutoScaling = value;
		}
	}

	public bool WallsAndGridlines2D
	{
		get
		{
			return ChartFormat.WallsAndGridlines2D;
		}
		set
		{
			ChartFormat.WallsAndGridlines2D = value;
		}
	}

	public OfficeChartType ChartType
	{
		get
		{
			DetectChartType();
			return m_chartType;
		}
		set
		{
			if (!m_book.IsWorkbookOpening)
			{
				m_radarStyle = null;
			}
			ChangeChartType(value, isSeriesCreation: false);
		}
	}

	public OfficeSeriesNameLevel SeriesNameLevel
	{
		get
		{
			return m_seriesNameLevel;
		}
		set
		{
			m_seriesNameLevel = value;
		}
	}

	public OfficeCategoriesLabelLevel CategoryLabelLevel
	{
		get
		{
			return m_categoriesLabelLevel;
		}
		set
		{
			m_categoriesLabelLevel = value;
		}
	}

	public IOfficeDataRange DataRange
	{
		get
		{
			return new ChartDataRange(this)
			{
				Range = DataIRange
			};
		}
		set
		{
			int firstRow = value.FirstRow;
			int firstColumn = value.FirstColumn;
			int lastRow = value.LastRow;
			int lastColumn = value.LastColumn;
			DataIRange = base.Workbook.Worksheets[0][firstRow, firstColumn, lastRow, lastColumn];
		}
	}

	public IRange DataIRange
	{
		get
		{
			if (m_dataRange == null)
			{
				m_dataRange = DetectDataRange();
			}
			return m_dataRange;
		}
		set
		{
			if (m_dataRange != value)
			{
				OfficeChartType chartType = ChartType;
				m_dataRange = value;
				OnDataRangeChanged(chartType);
				ChartSerieImpl chartSerieImpl = (ChartSerieImpl)m_series[0];
				UpdateSeries(m_series);
				Categories.Clear();
				UpdateCategory(m_series, fromDataRange: true);
				if (chartSerieImpl.NumRefFormula != null || chartSerieImpl.StrRefFormula != null)
				{
					chartSerieImpl.NumRefFormula = null;
					chartSerieImpl.StrRefFormula = null;
				}
			}
		}
	}

	public bool IsSeriesInRows
	{
		get
		{
			return m_bSeriesInRows;
		}
		set
		{
			int count = m_series.Count;
			if (DataRange == null && count != 0)
			{
				throw new NotSupportedException("This property supported only in chart where can detect data range.");
			}
			if (m_bSeriesInRows != value)
			{
				m_bSeriesInRows = value;
				if (count != 0)
				{
					GetFilter();
					OnSeriesInRowsChanged();
					m_categories.Clear();
					Setfilter();
					UpdateCategory(m_series, fromDataRange: false);
				}
			}
		}
	}

	public string ChartTitle
	{
		get
		{
			if (!base.ParentWorkbook.Saving && m_title != null && m_title.Text == "Chart Title")
			{
				return GetChartTitle();
			}
			if (HasTitle)
			{
				return ChartTitleArea.Text;
			}
			return null;
		}
		set
		{
			ChartTitleArea.Text = value;
			if (value == null || value == string.Empty)
			{
				HasAutoTitle = true;
				HasTitle = false;
			}
		}
	}

	public IOfficeChartTextArea ChartTitleArea
	{
		get
		{
			if (m_title == null)
			{
				CreateChartTitle();
			}
			return m_title;
		}
	}

	public IOfficeFont ChartTitleFont => ChartTitleArea;

	public string CategoryAxisTitle
	{
		get
		{
			return PrimaryCategoryAxis.Title;
		}
		set
		{
			PrimaryCategoryAxis.Title = value;
		}
	}

	internal bool IsPrimarySecondaryCategory
	{
		get
		{
			return m_bIsPrimarySecondaryCategory;
		}
		set
		{
			m_bIsPrimarySecondaryCategory = value;
		}
	}

	internal bool IsPrimarySecondaryValue
	{
		get
		{
			return m_bIsPrimarySecondaryValue;
		}
		set
		{
			m_bIsPrimarySecondaryValue = value;
		}
	}

	public string ValueAxisTitle
	{
		get
		{
			return PrimaryValueAxis.Title;
		}
		set
		{
			PrimaryValueAxis.Title = value;
		}
	}

	public string SecondaryCategoryAxisTitle
	{
		get
		{
			return SecondaryCategoryAxis.Title;
		}
		set
		{
			SecondaryCategoryAxis.Title = value;
		}
	}

	public string SecondaryValueAxisTitle
	{
		get
		{
			return SecondaryValueAxis.Title;
		}
		set
		{
			SecondaryValueAxis.Title = value;
		}
	}

	public string SeriesAxisTitle
	{
		get
		{
			return PrimarySerieAxis.Title;
		}
		set
		{
			PrimarySerieAxis.Title = value;
		}
	}

	public IOfficeChartCategoryAxis PrimaryCategoryAxis => m_primaryParentAxis.CategoryAxis;

	public IOfficeChartValueAxis PrimaryValueAxis => m_primaryParentAxis.ValueAxis;

	public IOfficeChartSeriesAxis PrimarySerieAxis
	{
		get
		{
			if (!IsSeriesAxisAvail && !Loading)
			{
				throw new NotSupportedException("Series axis doesnot exist in current chart type.");
			}
			return m_primaryParentAxis.SeriesAxis;
		}
	}

	public IOfficeChartCategoryAxis SecondaryCategoryAxis => m_secondaryParentAxis.CategoryAxis;

	public IOfficeChartValueAxis SecondaryValueAxis => m_secondaryParentAxis.ValueAxis;

	public IOfficeChartPageSetup PageSetup => m_pageSetup;

	public double XPos
	{
		get
		{
			return m_dXPos;
		}
		set
		{
			m_dXPos = value;
		}
	}

	public double YPos
	{
		get
		{
			return m_dYPos;
		}
		set
		{
			m_dYPos = value;
		}
	}

	public double Width
	{
		get
		{
			return (int)Math.Round(ApplicationImpl.ConvertToPixels(EMUWidth, MeasureUnits.EMU));
		}
		set
		{
			m_dWidth = Math.Round(ApplicationImpl.ConvertFromPixel(value, MeasureUnits.EMU));
		}
	}

	public double Height
	{
		get
		{
			return (int)Math.Round(ApplicationImpl.ConvertToPixels(EMUHeight, MeasureUnits.EMU));
		}
		set
		{
			m_dHeight = Math.Round(ApplicationImpl.ConvertFromPixel(value, MeasureUnits.EMU));
		}
	}

	internal double EMUHeight
	{
		get
		{
			return m_dHeight;
		}
		set
		{
			m_dHeight = value;
		}
	}

	internal double EMUWidth
	{
		get
		{
			return m_dWidth;
		}
		set
		{
			m_dWidth = value;
		}
	}

	public IOfficeChartSeries Series => m_series;

	public IOfficeChartCategories Categories => m_categories;

	public ChartFormatCollection PrimaryFormats => m_primaryParentAxis.ChartFormats;

	public ChartFormatCollection SecondaryFormats => m_secondaryParentAxis.ChartFormats;

	public IOfficeChartFrameFormat ChartArea
	{
		get
		{
			if (m_chartArea == null)
			{
				m_chartArea = new ChartFrameFormatImpl(base.Application, this);
				m_chartArea.Interior.ForegroundColorIndex = OfficeKnownColors.WhiteCustom;
			}
			return m_chartArea;
		}
	}

	public bool HasChartArea
	{
		get
		{
			return m_chartArea != null;
		}
		set
		{
			if (value != HasChartArea)
			{
				m_chartArea = (value ? new ChartFrameFormatImpl(base.Application, this) : null);
			}
		}
	}

	public bool HasPlotArea
	{
		get
		{
			return m_plotArea != null;
		}
		set
		{
			if (value != HasPlotArea)
			{
				m_plotArea = (value ? new ChartPlotAreaImpl(base.Application, this, ChartType) : null);
			}
		}
	}

	public IOfficeChartFrameFormat PlotArea
	{
		get
		{
			return m_plotArea;
		}
		set
		{
			m_plotArea = (ChartPlotAreaImpl)value;
		}
	}

	public ChartParentAxisImpl PrimaryParentAxis => m_primaryParentAxis;

	public ChartParentAxisImpl SecondaryParentAxis => m_secondaryParentAxis;

	public IOfficeChartWallOrFloor Walls
	{
		get
		{
			if (!m_book.IsWorkbookOpening && Array.IndexOf(DEF_WALLS_OR_FLOOR_TYPES, ChartType) == -1)
			{
				throw new ApplicationException("Walls are not supported in this chart type");
			}
			if (m_walls == null)
			{
				m_walls = new ChartWallOrFloorImpl(base.Application, this, bWalls: true);
			}
			return m_walls;
		}
		set
		{
			m_walls = (ChartWallOrFloorImpl)value;
		}
	}

	public IOfficeChartWallOrFloor SideWall
	{
		get
		{
			if (!m_book.IsWorkbookOpening && Array.IndexOf(DEF_WALLS_OR_FLOOR_TYPES, ChartType) == -1)
			{
				throw new ApplicationException("Walls are not supported in this chart type");
			}
			if (m_sidewall == null)
			{
				m_sidewall = new ChartWallOrFloorImpl(base.Application, this, bWalls: true);
			}
			return m_sidewall;
		}
		set
		{
			m_sidewall = (ChartWallOrFloorImpl)value;
		}
	}

	public IOfficeChartWallOrFloor BackWall
	{
		get
		{
			return Walls;
		}
		set
		{
			Walls = (ChartWallOrFloorImpl)value;
		}
	}

	public IOfficeChartWallOrFloor Floor
	{
		get
		{
			if (!m_book.IsWorkbookOpening && !SupportWallsAndFloor)
			{
				throw new ApplicationException("Floor is not supported by this chart type.");
			}
			if (m_floor == null)
			{
				m_floor = new ChartWallOrFloorImpl(base.Application, this, bWalls: false);
			}
			return m_floor;
		}
		set
		{
			m_floor = (ChartWallOrFloorImpl)value;
		}
	}

	public IOfficeChartDataTable DataTable => m_dataTable;

	public bool HasDataTable
	{
		get
		{
			return m_bHasDataTable;
		}
		set
		{
			if (m_bHasDataTable != value)
			{
				if (value)
				{
					CheckSupportDataTable();
				}
				m_bHasDataTable = value;
				m_dataTable = (value ? new ChartDataTableImpl(base.Application, this) : null);
			}
		}
	}

	public IOfficeChartLegend Legend => m_legend;

	public bool HasLegend
	{
		get
		{
			return m_bHasLegend;
		}
		set
		{
			if (m_bHasLegend != value)
			{
				m_bHasLegend = value;
				m_legend = (value ? new ChartLegendImpl(base.Application, this) : null);
			}
		}
	}

	public bool HasTitle
	{
		get
		{
			return m_bHasChartTitle;
		}
		set
		{
			if (m_bHasChartTitle != value)
			{
				m_bHasChartTitle = value;
				if (m_title != null)
				{
					if (value && m_title.Text == null)
					{
						m_title.Text = "Chart Title";
					}
					else if (!value)
					{
						m_title.Text = null;
					}
				}
				else
				{
					CreateChartTitle();
				}
			}
			if (!base.ParentWorkbook.IsWorkbookOpening)
			{
				m_hasAutoTitle = !value;
			}
		}
	}

	public OfficeChartPlotEmpty DisplayBlanksAs
	{
		get
		{
			return m_chartProperties.PlotBlank;
		}
		set
		{
			m_chartProperties.PlotBlank = value;
		}
	}

	public bool PlotVisibleOnly
	{
		get
		{
			return m_chartProperties.IsPlotVisOnly;
		}
		set
		{
			m_chartProperties.IsPlotVisOnly = value;
		}
	}

	public bool ShowPlotVisible
	{
		get
		{
			return m_showPlotVisible;
		}
		set
		{
			m_showPlotVisible = value;
		}
	}

	public bool SizeWithWindow
	{
		get
		{
			if (!m_bInWorksheet)
			{
				return !m_chartProperties.IsNotSizeWith;
			}
			return true;
		}
		set
		{
			if (!m_bInWorksheet)
			{
				m_chartProperties.IsNotSizeWith = !value;
			}
		}
	}

	public bool SupportWallsAndFloor => Array.IndexOf(DEF_WALLS_OR_FLOOR_TYPES, ChartType) >= 0;

	public override bool ProtectDrawingObjects => (InnerProtection & OfficeSheetProtection.Objects) != 0;

	public override bool ProtectScenarios => (InnerProtection & OfficeSheetProtection.Scenarios) != 0;

	public override OfficeSheetProtection Protection => base.Protection & ~OfficeSheetProtection.Scenarios;

	public ChartPlotAreaLayoutRecord PlotAreaLayout
	{
		get
		{
			if (m_plotAreaLayout == null)
			{
				m_plotAreaLayout = (ChartPlotAreaLayoutRecord)BiffRecordFactory.GetRecord(TBIFFRecord.PlotAreaLayout);
			}
			return m_plotAreaLayout;
		}
	}

	public object[] CategoryLabelValues
	{
		get
		{
			return m_categoryLabelValues;
		}
		set
		{
			m_categoryLabelValues = value;
		}
	}

	internal bool IsChartCleared
	{
		get
		{
			return m_isChartCleared;
		}
		set
		{
			m_isChartCleared = value;
		}
	}

	public IOfficeChartData ChartData => _chartData ?? (_chartData = new ChartData(this));

	public string CategoryFormula
	{
		get
		{
			return m_formula;
		}
		set
		{
			if (value.Length == 0 || value == null)
			{
				throw new ArgumentNullException("value");
			}
			m_formula = value;
		}
	}

	internal bool IsChartParsed
	{
		get
		{
			return m_isChartParsed;
		}
		set
		{
			m_isChartParsed = value;
		}
	}

	internal ushort ChartExTitlePosition
	{
		get
		{
			return m_chartExTitlePosition;
		}
		set
		{
			m_chartExTitlePosition = value;
		}
	}

	internal bool ChartTitleIncludeInLayout
	{
		get
		{
			return m_chartTitleIncludeInLayout;
		}
		set
		{
			m_chartTitleIncludeInLayout = value;
		}
	}

	internal bool? AutoUpdate
	{
		get
		{
			return m_isAutoUpdate;
		}
		set
		{
			m_isAutoUpdate = value;
		}
	}

	internal string ChartExRelationId
	{
		get
		{
			return m_chartExRelationId;
		}
		set
		{
			m_chartExRelationId = value;
		}
	}

	internal bool IsTreeMapOrSunBurst
	{
		get
		{
			if (ChartType != OfficeChartType.SunBurst)
			{
				return ChartType == OfficeChartType.TreeMap;
			}
			return true;
		}
	}

	internal bool IsHistogramOrPareto
	{
		get
		{
			if (ChartType != OfficeChartType.Histogram)
			{
				return ChartType == OfficeChartType.Pareto;
			}
			return true;
		}
	}

	internal bool IsThemeOverridePresent
	{
		get
		{
			return m_isThemeOverridePresent;
		}
		set
		{
			m_isThemeOverridePresent = value;
		}
	}

	internal Dictionary<string, string> ColorMapOverrideDictionary
	{
		get
		{
			if (m_colorMapOverrideDictionary == null)
			{
				m_colorMapOverrideDictionary = new Dictionary<string, string>();
			}
			return m_colorMapOverrideDictionary;
		}
		set
		{
			m_colorMapOverrideDictionary = value;
		}
	}

	internal bool IsChartExternalRelation
	{
		get
		{
			return m_isChartExternalRelation;
		}
		set
		{
			m_isChartExternalRelation = value;
		}
	}

	internal int LineChartCount
	{
		get
		{
			return m_linechartCount;
		}
		set
		{
			m_linechartCount = value;
		}
	}

	public override OfficeKnownColors TabColor
	{
		get
		{
			return base.TabColor;
		}
		set
		{
			if (m_bInWorksheet)
			{
				throw new NotSupportedException();
			}
			base.TabColor = value;
		}
	}

	internal int ActiveSheetIndex
	{
		get
		{
			return m_activeSheetIndex;
		}
		set
		{
			m_activeSheetIndex = value;
		}
	}

	public bool IsCategoryAxisAvail => Array.IndexOf(NO_CATEGORY_AXIS, ChartType) == -1;

	public bool IsValueAxisAvail => Array.IndexOf(NO_CATEGORY_AXIS, ChartType) == -1;

	public bool IsSeriesAxisAvail
	{
		get
		{
			OfficeChartType chartType = ChartType;
			return Array.IndexOf(DEF_SUPPORT_SERIES_AXIS, chartType) != -1;
		}
	}

	public bool IsStacked => GetIsStacked(ChartType);

	public bool IsChart_100 => GetIs100(ChartType);

	public bool IsChart3D => Array.IndexOf(CHARTS3D, ChartType) != -1;

	public bool IsPivotChart3D => Array.IndexOf(DEF_NEED_VIEW_3D, PivotChartType) != -1;

	public bool IsChartLine => Array.IndexOf(CHARTS_LINE, ChartType) != -1;

	public bool NeedDataFormat
	{
		get
		{
			if (IsChart3D || IsChartLine || IsChartExploded || IsChartScatter || IsChartStock || ChartType == OfficeChartType.Bubble_3D || ChartType == OfficeChartType.Radar)
			{
				return ChartType != OfficeChartType.Surface_NoColor_Contour;
			}
			return false;
		}
	}

	public bool NeedMarkerFormat
	{
		get
		{
			if (!IsChartPyramid && !IsChartCone)
			{
				return IsChartCylinder;
			}
			return true;
		}
	}

	public bool IsChartBar => ChartType.ToString().IndexOf("Bar") != -1;

	public bool IsChartPyramid => ChartType.ToString().IndexOf("Pyramid") != -1;

	public bool IsChartCone => ChartType.ToString().IndexOf("Cone") != -1;

	public bool IsChartCylinder => ChartType.ToString().IndexOf("Cylinder") != -1;

	public bool IsChartBubble => Array.IndexOf(CHARTS_BUBBLE, ChartType) != -1;

	public bool IsChartDoughnut => ChartType.ToString().IndexOf("Doughnut") != -1;

	public bool IsChartVaryColor => Array.IndexOf(CHARTS_VARYCOLOR, ChartType) != -1;

	public bool IsChartExploded => Array.IndexOf(CHARTS_EXPLODED, ChartType) != -1;

	public bool IsSeriesLines => CanChartHaveSeriesLines;

	public bool CanChartHaveSeriesLines => Array.IndexOf(CHART_SERIES_LINES, ChartType) != -1;

	public bool IsChartScatter => Array.IndexOf(CHARTS_SCATTER, ChartType) != -1;

	public OfficeChartLinePattern DefaultLinePattern
	{
		get
		{
			if (ChartType == OfficeChartType.Scatter_Markers || IsChartStock)
			{
				return OfficeChartLinePattern.None;
			}
			return OfficeChartLinePattern.Solid;
		}
	}

	public bool IsChartSmoothedLine => Array.IndexOf(CHARTS_SMOOTHED_LINE, ChartType) != -1;

	public bool IsChartStock => Array.IndexOf(CHARTS_STOCK, ChartType) != -1;

	public bool NeedDropBar
	{
		get
		{
			if (ChartType != OfficeChartType.Stock_OpenHighLowClose)
			{
				return ChartType == OfficeChartType.Stock_VolumeOpenHighLowClose;
			}
			return true;
		}
	}

	public bool IsChartVolume
	{
		get
		{
			if (ChartType != OfficeChartType.Stock_VolumeHighLowClose)
			{
				return ChartType == OfficeChartType.Stock_VolumeOpenHighLowClose;
			}
			return true;
		}
	}

	public bool IsPerspective => Array.IndexOf(CHARTS_PERSPECTIVE, ChartType) != -1;

	public bool IsClustered => GetIsClustered(ChartType);

	public bool NoPlotArea
	{
		get
		{
			if (!IsChartRadar && !IsChartPie && !IsChartDoughnut)
			{
				return ChartType == OfficeChartType.Surface_NoColor_Contour;
			}
			return true;
		}
	}

	public bool IsChartRadar => ChartType.ToString().StartsWith("Radar");

	public bool IsChartPie => GetIsChartPie(ChartType);

	public bool IsChartWalls => false;

	public bool IsChartFloor
	{
		get
		{
			if (OfficeChartType.Surface_NoColor_Contour != ChartType)
			{
				return OfficeChartType.Surface_Contour == ChartType;
			}
			return true;
		}
	}

	internal List<int> SerializedAxisIds
	{
		get
		{
			if (m_axisIds == null)
			{
				m_axisIds = new List<int>();
			}
			return m_axisIds;
		}
	}

	public bool IsSecondaryCategoryAxisAvail => SecondaryCategoryAxis != null;

	public bool IsSecondaryValueAxisAvail => SecondaryValueAxis != null;

	public bool IsSecondaryAxes
	{
		get
		{
			if (!IsSecondaryValueAxisAvail && !IsSecondaryCategoryAxisAvail)
			{
				return m_bIsSecondaryAxis;
			}
			return true;
		}
		set
		{
			m_bIsSecondaryAxis = value;
		}
	}

	public bool IsSpecialDataLabels => Array.IndexOf(DEF_SPECIAL_DATA_LABELS, ChartType) != -1;

	public bool CanChartPercentageLabel => Array.IndexOf(DEF_CHART_PERCENTAGE, ChartType) != -1;

	public bool CanChartBubbleLabel => IsChartBubble;

	public bool IsManuallyFormatted
	{
		get
		{
			return m_chartProperties.IsManSerAlloc;
		}
		set
		{
			m_chartProperties.IsManSerAlloc = value;
		}
	}

	private ChartPlotGrowthRecord PlotGrowth
	{
		get
		{
			if (m_plotGrowth == null)
			{
				m_plotGrowth = (ChartPlotGrowthRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartPlotGrowth);
			}
			return m_plotGrowth;
		}
	}

	private ChartPosRecord PlotAreaBoundingBox
	{
		get
		{
			if (m_plotAreaBoundingBox == null)
			{
				m_plotAreaBoundingBox = (ChartPosRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartPos);
				m_plotAreaBoundingBox.BottomRight = 2;
				m_plotAreaBoundingBox.TopLeft = 2;
			}
			return m_plotAreaBoundingBox;
		}
	}

	public WorkbookImpl InnerWorkbook => m_book;

	public ChartFrameFormatImpl InnerChartArea => ChartArea as ChartFrameFormatImpl;

	public ChartFrameFormatImpl InnerPlotArea
	{
		get
		{
			if (m_plotAreaFrame == null)
			{
				m_plotAreaFrame = new ChartFrameFormatImpl(base.Application, this);
			}
			return m_plotAreaFrame;
		}
	}

	public string ChartStartType => ChartFormatImpl.GetStartSerieType(ChartType);

	public override PageSetupBaseImpl PageSetupBase => m_pageSetup;

	[CLSCompliant(false)]
	public ChartShtpropsRecord ChartProperties => m_chartProperties;

	public bool Loading => m_book.IsWorkbookOpening;

	internal ChartFormatImpl ChartFormat
	{
		get
		{
			if (m_series.Count == 0)
			{
				throw new ApplicationException("cannot get format.");
			}
			return ((ChartSerieImpl)m_series[0]).GetCommonSerieFormat();
		}
	}

	public bool TypeChanging
	{
		get
		{
			return m_bTypeChanging;
		}
		set
		{
			m_bTypeChanging = value;
		}
	}

	public OfficeChartType DestinationType
	{
		get
		{
			return m_destinationType;
		}
		set
		{
			m_destinationType = value;
		}
	}

	public RelationCollection Relations
	{
		get
		{
			if (m_relations == null)
			{
				m_relations = new RelationCollection();
			}
			return m_relations;
		}
	}

	public int Style
	{
		get
		{
			return m_iStyle2007;
		}
		set
		{
			m_iStyle2007 = value;
			if (!base.ParentWorkbook.IsWorkbookOpening && !IsChartExSerieType(ChartType) && m_relationPreservedStreamCollections != null && m_relationPreservedStreamCollections.Count > 0)
			{
				if (m_relationPreservedStreamCollections.ContainsKey("http://schemas.microsoft.com/office/2011/relationships/chartColorStyle"))
				{
					m_relationPreservedStreamCollections.Remove("http://schemas.microsoft.com/office/2011/relationships/chartColorStyle");
					m_isChartColorStyleSkipped = false;
				}
				if (m_relationPreservedStreamCollections.ContainsKey("http://schemas.microsoft.com/office/2011/relationships/chartStyle"))
				{
					m_relationPreservedStreamCollections.Remove("http://schemas.microsoft.com/office/2011/relationships/chartStyle");
					m_isChartStyleSkipped = false;
				}
			}
		}
	}

	public bool HasFloor => m_floor != null;

	public bool HasWalls => m_walls != null;

	public Stream PivotFormatsStream
	{
		get
		{
			return m_pivotFormatsStream;
		}
		set
		{
			m_pivotFormatsStream = value;
		}
	}

	internal Stream HighlightStream
	{
		get
		{
			return m_highlightStream;
		}
		set
		{
			m_highlightStream = value;
		}
	}

	public bool ZoomToFit
	{
		get
		{
			return SizeWithWindow;
		}
		set
		{
			SizeWithWindow = value;
		}
	}

	protected override OfficeSheetProtection DefaultProtectionOptions => OfficeSheetProtection.Objects | OfficeSheetProtection.Scenarios | OfficeSheetProtection.Content;

	public bool IsEmbeded => m_bInWorksheet;

	public int DefaultTextIndex
	{
		get
		{
			int result = 0;
			if (m_lstDefaultText != null && m_lstDefaultText.Count > 0)
			{
				List<BiffRecordRaw> byIndex = m_lstDefaultText.GetByIndex(0);
				if (byIndex != null)
				{
					foreach (BiffRecordRaw item in byIndex)
					{
						if (item.TypeCode == TBIFFRecord.ChartFontx)
						{
							result = ((ChartFontxRecord)item).FontIndex;
							break;
						}
					}
				}
			}
			return result;
		}
	}

	public Stream PreservedBandFormats
	{
		get
		{
			return m_bandFormats;
		}
		set
		{
			m_bandFormats = value;
		}
	}

	public bool HasTitleInternal
	{
		get
		{
			bool result = false;
			if (m_title != null && HasTitle && (m_title.Text != null || m_title.FontIndex == 0 || !m_title.TextRecord.IsAutoColor || !m_title.TextRecord.IsAutoMode))
			{
				result = true;
			}
			return result;
		}
	}

	internal Stream AlternateContent
	{
		get
		{
			return m_alternateContent;
		}
		set
		{
			m_alternateContent = value;
		}
	}

	internal Stream DefaultTextProperty
	{
		get
		{
			return m_defaultTextProperty;
		}
		set
		{
			m_defaultTextProperty = value;
		}
	}

	public IOfficeFont Font
	{
		get
		{
			if (m_font == null)
			{
				FontImpl font = (FontImpl)base.ParentWorkbook.InnerFonts[0];
				m_font = new FontWrapper(font);
			}
			return m_font;
		}
	}

	internal bool? HasAutoTitle
	{
		get
		{
			return m_hasAutoTitle;
		}
		set
		{
			m_hasAutoTitle = value;
		}
	}

	internal override bool ParseDataOnDemand
	{
		get
		{
			return m_bParseDataOnDemand;
		}
		set
		{
			m_bParseDataOnDemand = value;
		}
	}

	internal string RadarStyle
	{
		get
		{
			return m_radarStyle;
		}
		set
		{
			m_radarStyle = value;
		}
	}

	internal Dictionary<string, Stream> RelationPreservedStreamCollection
	{
		get
		{
			if (m_relationPreservedStreamCollections == null)
			{
				m_relationPreservedStreamCollections = new Dictionary<string, Stream>(3);
			}
			return m_relationPreservedStreamCollections;
		}
	}

	internal IEnumerable<IGrouping<int, IOfficeChartSerie>> ChartSerieGroupsBeforesorting
	{
		get
		{
			return m_chartSerieGroupsBeforesorting;
		}
		set
		{
			m_chartSerieGroupsBeforesorting = value;
		}
	}

	internal ChartImpl()
	{
		IApplication excel = new ExcelEngine().Excel;
		excel.DefaultVersion = OfficeVersion.Excel2010;
		WorksheetImpl workSheet = (excel.Workbooks.Create(1) as WorkbookImpl).Worksheets[0] as WorksheetImpl;
		SetParent(excel, workSheet);
	}

	protected override void SetParent(IApplication application, WorksheetImpl workSheet)
	{
		base.SetParent(application, workSheet);
		SetParents();
	}

	internal void SetDefaultProperties()
	{
		m_bInWorksheet = FindParent(typeof(WorksheetBaseImpl), bSubTypes: true) is WorksheetBaseImpl;
		if (!m_book.IsWorkbookOpening)
		{
			m_bHasChartTitle = true;
			CreateChartTitle();
		}
		ChartFormatImpl chartFormatImpl = new ChartFormatImpl(base.Application, PrimaryFormats);
		PrimaryFormats.Add(chartFormatImpl, bCanReplace: true);
		chartFormatImpl.ChangeChartType(OfficeChartType.Column_Clustered, isSeriesCreation: false);
		if (!m_book.IsWorkbookOpening)
		{
			HasLegend = true;
			PrimaryValueAxis.HasMajorGridLines = true;
			m_chartProperties = (ChartShtpropsRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartShtprops);
			m_walls = new ChartWallOrFloorImpl(base.Application, this, bWalls: true);
			m_floor = new ChartWallOrFloorImpl(base.Application, this, bWalls: false);
			m_sidewall = new ChartWallOrFloorImpl(base.Application, this, bWalls: true);
			m_plotArea = new ChartPlotAreaImpl(base.Application, this, ChartType);
		}
		else if (m_book.Version != 0)
		{
			m_chartProperties = (ChartShtpropsRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartShtprops);
		}
		InitializeFrames();
	}

	public void Refresh()
	{
		Refresh(UpdateChartData: false);
	}

	internal void Refresh(bool UpdateChartData)
	{
		DetectChartType();
		foreach (ChartSerieImpl item in Series)
		{
			if (UpdateChartData)
			{
				(base.Workbook.ActiveSheet as WorksheetImpl).Calculate();
			}
			string text = null;
			if (((ChartDataRange)item.NameRange).Range != null)
			{
				text = ((ChartDataRange)item.NameRange).Range.AddressGlobal;
			}
			if (text != null)
			{
				item.NameRangeIRange.Value2 = base.Workbook.ActiveSheet.Range[text].Value2;
			}
			IRange[] cells = item.ValuesIRange.Cells;
			for (int i = 0; i < cells.Length; i++)
			{
				RangeImpl rangeImpl = (RangeImpl)cells[i];
				text = rangeImpl.AddressGlobal;
				if (text != null)
				{
					rangeImpl.Value2 = base.Workbook.ActiveSheet.Range[text].Value2;
				}
			}
		}
		foreach (ChartCategory category in Categories)
		{
			if (category.CategoryLabelIRange == null)
			{
				continue;
			}
			IRange[] cells = category.CategoryLabelIRange.Cells;
			for (int i = 0; i < cells.Length; i++)
			{
				RangeImpl rangeImpl2 = (RangeImpl)cells[i];
				string addressGlobal = rangeImpl2.AddressGlobal;
				if (addressGlobal != null)
				{
					rangeImpl2.Value2 = base.Workbook.ActiveSheet.Range[addressGlobal].Value2;
				}
			}
		}
		CategoryLabelValues = null;
		foreach (ChartSerieImpl item2 in Series)
		{
			item2.EnteredDirectlyValues = null;
		}
	}

	protected internal ChartImpl(IApplication application, object parent)
		: base(application, parent)
	{
		SetParents();
		m_bInWorksheet = FindParent(typeof(WorksheetBaseImpl), bSubTypes: true) is WorksheetBaseImpl;
		if (!m_book.IsWorkbookOpening)
		{
			m_bHasChartTitle = true;
			CreateChartTitle();
		}
		ChartFormatImpl chartFormatImpl = new ChartFormatImpl(base.Application, PrimaryFormats);
		PrimaryFormats.Add(chartFormatImpl, bCanReplace: true);
		chartFormatImpl.ChangeChartType(OfficeChartType.Column_Clustered, isSeriesCreation: false);
		if (!m_book.IsWorkbookOpening)
		{
			HasLegend = true;
			PrimaryValueAxis.HasMajorGridLines = true;
			m_chartProperties = (ChartShtpropsRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartShtprops);
			m_walls = new ChartWallOrFloorImpl(application, this, bWalls: true);
			m_floor = new ChartWallOrFloorImpl(application, this, bWalls: false);
			m_sidewall = new ChartWallOrFloorImpl(application, this, bWalls: true);
			m_plotArea = new ChartPlotAreaImpl(application, this, ChartType);
		}
		else if (m_book.Version != 0)
		{
			m_chartProperties = (ChartShtpropsRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartShtprops);
		}
		InitializeFrames();
	}

	[CLSCompliant(false)]
	protected internal ChartImpl(IApplication application, object parent, IList data, ref int iPos, OfficeParseOptions options)
		: this(application, parent)
	{
		m_arrRecords.Clear();
		m_parseOptions = options;
		GetChartRecords(data, ref iPos, m_arrRecords, options);
	}

	private void SetParents()
	{
		m_book = FindParent(typeof(WorkbookImpl)) as WorkbookImpl;
		if (m_book == null)
		{
			throw new ArgumentNullException("Can't find parent workbook.");
		}
	}

	private void CreateChartTitle()
	{
		m_title = new ChartTextAreaImpl(base.Application, this, ExcelObjectTextLink.Chart);
		if (m_bHasChartTitle)
		{
			m_title.Text = "Chart Title";
		}
		else
		{
			m_title.Text = null;
		}
		m_title.FontName = "Calibri";
		m_title.Size = 18.6;
		m_title.FrameFormat.Interior.UseAutomaticFormat = true;
		if (m_book != null && !m_book.IsWorkbookOpening)
		{
			m_title.Bold = true;
		}
	}

	public override void Parse()
	{
		KeepRecord = true;
		base.Parse();
	}

	private void Parse(IList data, ref int iPos, OfficeParseOptions options)
	{
	}

	protected internal override void ParseData(Dictionary<int, int> dictUpdatedSSTIndexes)
	{
		SetDefaultValues();
		if ((m_parseOptions & OfficeParseOptions.DoNotParseCharts) != 0)
		{
			return;
		}
		if (m_dataHolder == null)
		{
			m_pivotList = new List<BiffRecordRaw>();
			int iPos = 0;
			bool flag = false;
			BiffRecordRaw biffRecordRaw = m_arrRecords[iPos];
			int num = 0;
			Dictionary<int, int> newSeriesIndex = new Dictionary<int, int>();
			while (iPos < m_arrRecords.Count && !flag)
			{
				biffRecordRaw = m_arrRecords[iPos];
				if (biffRecordRaw.TypeCode == TBIFFRecord.BOF)
				{
					num++;
					iPos++;
				}
				else if (biffRecordRaw.TypeCode == TBIFFRecord.EOF)
				{
					num--;
					iPos++;
					if (num == 0)
					{
						flag = true;
					}
				}
				else if (num == 1)
				{
					ParseOrdinaryRecord(biffRecordRaw, ref iPos, newSeriesIndex);
				}
				else
				{
					iPos++;
				}
			}
			PrepareProtection();
			ReparseErrorBars(newSeriesIndex);
		}
		else
		{
			m_dataHolder.ParseChartsheetData(this);
		}
		base.IsParsed = true;
	}

	private void ReparseErrorBars(Dictionary<int, int> newSeriesIndex)
	{
		if (m_dictReparseErrorBars != null && m_dictReparseErrorBars.Count > 0)
		{
			foreach (KeyValuePair<int, List<BiffRecordRaw>> dictReparseErrorBar in m_dictReparseErrorBars)
			{
				int num = dictReparseErrorBar.Key;
				if (newSeriesIndex.ContainsKey(num))
				{
					num = newSeriesIndex[num];
				}
				List<BiffRecordRaw> value = dictReparseErrorBar.Value;
				((ChartSerieImpl)m_series[num]).ParseErrorBars(value);
			}
		}
		m_dictReparseErrorBars = null;
	}

	private void ParseOrdinaryRecord(BiffRecordRaw record, ref int iPos, Dictionary<int, int> newSeriesIndex)
	{
		switch (record.TypeCode)
		{
		case TBIFFRecord.Header:
			m_pageSetup = new ChartPageSetupImpl(base.Application, this, m_arrRecords, ref iPos);
			break;
		case TBIFFRecord.ChartFbi:
			ParseFonts(m_arrRecords, ref iPos);
			break;
		case (TBIFFRecord)2128:
		case (TBIFFRecord)2136:
		case (TBIFFRecord)2137:
			m_pivotList.Add(record);
			iPos++;
			break;
		case TBIFFRecord.ChartChart:
			ParseChart(m_arrRecords, ref iPos, newSeriesIndex);
			break;
		case TBIFFRecord.Dimensions:
			ParseDimensions((DimensionsRecord)m_arrRecords[iPos++]);
			break;
		case TBIFFRecord.ChartSiIndex:
			ParseSiIndex(m_arrRecords, ref iPos);
			break;
		case TBIFFRecord.WindowTwo:
			ParseWindowTwo((WindowTwoRecord)m_arrRecords[iPos++]);
			break;
		case TBIFFRecord.WindowZoom:
			ParseWindowZoom((WindowZoomRecord)m_arrRecords[iPos++]);
			break;
		case TBIFFRecord.CodeName:
			m_strCodeName = ((CodeNameRecord)m_arrRecords[iPos]).CodeName;
			iPos++;
			break;
		case TBIFFRecord.Protect:
			ParseProtect((ProtectRecord)record);
			iPos++;
			break;
		case TBIFFRecord.Password:
			ParsePassword((PasswordRecord)record);
			iPos++;
			break;
		case TBIFFRecord.ObjectProtect:
			ParseObjectProtect((ObjectProtectRecord)record);
			iPos++;
			break;
		case TBIFFRecord.ScenProtect:
			ParseScenProtect((ScenProtectRecord)record);
			iPos++;
			break;
		default:
			iPos++;
			break;
		}
	}

	private void GetChartRecords(IList data, ref int iPos, List<BiffRecordRaw> records, OfficeParseOptions options)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		if (iPos < 0 || iPos >= data.Count)
		{
			throw new ArgumentOutOfRangeException("iPos", "Value cannot be less than 0 and greater than data.Count - 1");
		}
		BiffRecordRaw biffRecordRaw = (BiffRecordRaw)data[iPos];
		if (biffRecordRaw.TypeCode != TBIFFRecord.BOF)
		{
			throw new ArgumentOutOfRangeException("BOF record was expected.");
		}
		records.Add(biffRecordRaw);
		iPos++;
		biffRecordRaw = (BiffRecordRaw)data[iPos];
		bool flag = false;
		if (flag)
		{
			_ = ((FontImpl)((FindParent(typeof(WorkbookImpl)) as WorkbookImpl) ?? throw new ArgumentNullException("Can't find parent workbook")).InnerFonts[0]).Record;
		}
		while (biffRecordRaw.TypeCode != TBIFFRecord.EOF)
		{
			if (!flag || (flag && biffRecordRaw.TypeCode != TBIFFRecord.ChartFbi))
			{
				records.Add(biffRecordRaw);
			}
			if (biffRecordRaw.TypeCode == TBIFFRecord.MSODrawing && m_iMsoStartIndex < 0)
			{
				m_iMsoStartIndex = records.Count - 1;
			}
			iPos++;
			biffRecordRaw = (BiffRecordRaw)data[iPos];
		}
		records.Add((BiffRecordRaw)data[iPos]);
		iPos++;
	}

	private void ParseFonts(IList data, ref int iPos)
	{
		BiffRecordRaw biffRecordRaw = (BiffRecordRaw)data[iPos];
		if (biffRecordRaw.TypeCode != TBIFFRecord.ChartFbi)
		{
			throw new ArgumentOutOfRangeException("ChartFbi record was expected.");
		}
		while (biffRecordRaw.TypeCode == TBIFFRecord.ChartFbi)
		{
			m_arrFonts.Add((ChartFbiRecord)biffRecordRaw);
			iPos++;
			biffRecordRaw = (BiffRecordRaw)data[iPos];
		}
	}

	private void ParseChart(IList<BiffRecordRaw> data, ref int iPos, Dictionary<int, int> newSeriesIndex)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		BiffRecordRaw biffRecordRaw = data[iPos];
		biffRecordRaw.CheckTypeCode(TBIFFRecord.ChartChart);
		FillDataFromChartRecord((ChartChartRecord)biffRecordRaw);
		iPos++;
		biffRecordRaw = data[iPos];
		biffRecordRaw.CheckTypeCode(TBIFFRecord.Begin);
		iPos++;
		int num = 0;
		int count = data.Count;
		m_series.TrendIndex = 0;
		biffRecordRaw = data[iPos];
		List<ChartTextAreaImpl> list = null;
		while (biffRecordRaw.TypeCode != TBIFFRecord.End || num != 0)
		{
			switch (biffRecordRaw.TypeCode)
			{
			case TBIFFRecord.ChartPlotGrowth:
				ParsePlotGrowth((ChartPlotGrowthRecord)data[iPos++]);
				break;
			case TBIFFRecord.ChartSeries:
				ParseSeriesOrErrorBars(data, ref iPos, newSeriesIndex);
				break;
			case TBIFFRecord.ChartShtprops:
				ParseSheetProperties(data, ref iPos);
				break;
			case TBIFFRecord.ChartDefaultText:
				ParseDefaultText(data, ref iPos);
				break;
			case TBIFFRecord.ChartText:
			{
				ChartTextAreaImpl textArea2 = ParseText(data, ref iPos);
				List<ChartTextAreaImpl> list2 = AssignTextArea(textArea2, newSeriesIndex);
				if (list == null)
				{
					list = list2;
				}
				else
				{
					list.AddRange(list2);
				}
				break;
			}
			case TBIFFRecord.ChartAxesUsed:
				ParseAxesUsed(data, ref iPos);
				break;
			case TBIFFRecord.ChartAxisParent:
				ParseAxisParent(data, ref iPos);
				break;
			case TBIFFRecord.ChartDat:
				ParseDataTable(data, ref iPos);
				break;
			case TBIFFRecord.ChartFrame:
				InnerChartArea.Parse(data, ref iPos);
				break;
			case TBIFFRecord.ChartWrapper:
				if (((ChartWrapperRecord)biffRecordRaw).Record.TypeCode == TBIFFRecord.ChartText)
				{
					ChartWrappedTextAreaImpl textArea = new ChartWrappedTextAreaImpl(base.Application, this, data, ref iPos);
					AssignTextArea(textArea, newSeriesIndex);
				}
				else
				{
					iPos++;
				}
				break;
			case TBIFFRecord.Begin:
				num++;
				iPos++;
				break;
			case TBIFFRecord.End:
				num--;
				iPos++;
				break;
			case TBIFFRecord.WindowZoom:
				m_chartChartZoom = (WindowZoomRecord)biffRecordRaw;
				iPos++;
				break;
			case TBIFFRecord.PlotAreaLayout:
				m_plotAreaLayout = (ChartPlotAreaLayoutRecord)biffRecordRaw;
				iPos++;
				break;
			default:
				iPos++;
				break;
			}
			if (iPos == count)
			{
				break;
			}
			biffRecordRaw = data[iPos];
		}
		iPos++;
		if (m_chartProperties == null)
		{
			m_chartProperties = (ChartShtpropsRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartShtprops);
		}
		UpdateChartTitle();
		DetectIsInRowOnParsing();
		ChangePrimaryAxis(isParsing: true);
		DetectChartType();
		ReparseTrendLegends();
		count = list?.Count ?? 0;
		if (DataRange != null)
		{
			IRange serieRange = null;
			IsSeriesInRows = DetectIsInRow((Series[0].Values as ChartDataRange).Range);
			GetSerieOrAxisRange((DataRange as ChartDataRange).Range, IsSeriesInRows, out serieRange);
			GetSerieOrAxisRange(serieRange, !IsSeriesInRows, out serieRange);
			int count2 = serieRange.Count / (Series[0].Values as ChartDataRange).Range.Count;
			for (int i = 0; i < (Series[0].Values as ChartDataRange).Range.Count; i++)
			{
				IRange categoryRange = GetCategoryRange(serieRange, out serieRange, count2, IsSeriesInRows);
				if (Categories.Count > 0)
				{
					(Categories[i] as ChartCategory).CategoryLabelIRange = (Series[0].CategoryLabels as ChartDataRange).Range;
					(Categories[i] as ChartCategory).ValuesIRange = categoryRange;
					if (Categories[0].CategoryLabel != null)
					{
						(Categories[i] as ChartCategory).Name = (Categories[0].CategoryLabel as ChartDataRange).Range.Cells[i].Text;
						continue;
					}
					(Categories[i] as ChartCategory).Name = (i + 1).ToString();
					if (Legend != null && Legend.LegendEntries != null && Legend.LegendEntries[i].TextArea != null)
					{
						Legend.LegendEntries[i].TextArea.Text = (Categories[i] as ChartCategory).Name;
					}
				}
				else if (Legend != null && Legend.LegendEntries != null && Legend.LegendEntries[i].TextArea != null)
				{
					Legend.LegendEntries[i].TextArea.Text = (i + 1).ToString();
				}
			}
		}
		else if (Series != null && Series.Count > 0 && Series[0].Values != null)
		{
			for (int j = 0; j < (Series[0].Values as ChartDataRange).Range.Count; j++)
			{
				if (j < Categories.Count)
				{
					(Categories[j] as ChartCategory).CategoryLabelIRange = (Series[0].CategoryLabels as ChartDataRange).Range;
					(Categories[j] as ChartCategory).ValuesIRange = (Series[0].Values as ChartDataRange).Range;
					if (Categories[0].CategoryLabel != null && Categories[0].CategoryLabel.GetType() != typeof(ExternalRange) && (Categories[0].CategoryLabel as ChartDataRange).Range.Worksheet != null && j < (Categories[0].CategoryLabel as ChartDataRange).Range.Cells.Length)
					{
						(Categories[j] as ChartCategory).Name = (Categories[0].CategoryLabel as ChartDataRange).Range.Cells[j].Text;
					}
				}
			}
		}
		for (int k = 0; k < count; k++)
		{
			ChartTextAreaImpl area = list[k];
			m_series.AssignTrendDataLabel(area);
		}
		if (Series.Count > 0 && Series[0].SerieType == OfficeChartType.Bubble)
		{
			CheckIsBubble3D();
		}
	}

	private void FillDataFromChartRecord(ChartChartRecord chart)
	{
		XPos = FixedPointToDouble(chart.X);
		YPos = FixedPointToDouble(chart.Y);
		EMUWidth = FixedPointToDouble(chart.Width);
		EMUHeight = FixedPointToDouble(chart.Height);
	}

	private void ParsePlotGrowth(ChartPlotGrowthRecord plotGrowth)
	{
		if (plotGrowth == null)
		{
			throw new ArgumentNullException("plotGrowth");
		}
		m_plotGrowth = plotGrowth;
	}

	private void ParseSiIndex(IList<BiffRecordRaw> data, ref int iPos)
	{
		m_series.ParseSiIndex(data, ref iPos);
	}

	private void ParseSeriesOrErrorBars(IList<BiffRecordRaw> data, ref int iPos, Dictionary<int, int> newSeriesIndexes)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		List<BiffRecordRaw> list = new List<BiffRecordRaw>();
		int serieIndex = 0;
		bool bIsErrorBars = false;
		if (AddRecords(data, list, ref iPos, ref serieIndex, ref bIsErrorBars))
		{
			int iPos2 = 0;
			ChartSerieImpl serieToAdd = new ChartSerieImpl(base.Application, m_series, list, ref iPos2);
			m_series.Add(serieToAdd);
			newSeriesIndexes[serieIndex] = m_series.Count - 1;
		}
		else if (bIsErrorBars)
		{
			if (newSeriesIndexes.ContainsKey(serieIndex))
			{
				serieIndex = newSeriesIndexes[serieIndex];
			}
			ChartSerieImpl chartSerieImpl = (ChartSerieImpl)m_series[serieIndex];
			if (chartSerieImpl != null)
			{
				chartSerieImpl.ParseErrorBars(list);
				return;
			}
			if (m_dictReparseErrorBars == null)
			{
				m_dictReparseErrorBars = new Dictionary<int, List<BiffRecordRaw>>();
			}
			m_dictReparseErrorBars.Add(serieIndex, list);
		}
		else
		{
			int i = 0;
			for (int count = list.Count; i < count; i++)
			{
				m_trendList.Add(list[i]);
			}
		}
	}

	private void ParseSheetProperties(IList<BiffRecordRaw> data, ref int iPos)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		BiffRecordRaw biffRecordRaw = data[iPos];
		biffRecordRaw.CheckTypeCode(TBIFFRecord.ChartShtprops);
		m_chartProperties = (ChartShtpropsRecord)biffRecordRaw.Clone();
		iPos++;
	}

	private void ParseDefaultText(IList<BiffRecordRaw> data, ref int iPos)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		BiffRecordRaw biffRecordRaw = data[iPos];
		biffRecordRaw.CheckTypeCode(TBIFFRecord.ChartDefaultText);
		int textCharacteristics = (int)((ChartDefaultTextRecord)biffRecordRaw.Clone()).TextCharacteristics;
		iPos++;
		List<BiffRecordRaw> textData = GetTextData(data, ref iPos);
		m_lstDefaultText[textCharacteristics] = textData;
	}

	private List<BiffRecordRaw> GetTextData(IList<BiffRecordRaw> data, ref int iPos)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		if (iPos < 0 || iPos > data.Count)
		{
			throw new ArgumentOutOfRangeException("iPos", "Value cannot be less than 0 and greater than data.Length");
		}
		BiffRecordRaw biffRecordRaw = data[iPos];
		biffRecordRaw.CheckTypeCode(TBIFFRecord.ChartText);
		List<BiffRecordRaw> list = new List<BiffRecordRaw>();
		list.Add(biffRecordRaw);
		iPos++;
		biffRecordRaw = data[iPos];
		biffRecordRaw.CheckTypeCode(TBIFFRecord.Begin);
		list.Add(biffRecordRaw);
		do
		{
			iPos++;
			biffRecordRaw = data[iPos];
			list.Add(biffRecordRaw);
		}
		while (biffRecordRaw.TypeCode != TBIFFRecord.End);
		iPos++;
		return list;
	}

	private ChartTextAreaImpl ParseText(IList<BiffRecordRaw> data, ref int iPos)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		data[iPos].CheckTypeCode(TBIFFRecord.ChartText);
		return new ChartTextAreaImpl(base.Application, this, data, ref iPos);
	}

	private void ParseAxesUsed(IList<BiffRecordRaw> data, ref int iPos)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		data[iPos].CheckTypeCode(TBIFFRecord.ChartAxesUsed);
		m_primaryParentAxis.Formats.PrimaryFormats.Clear();
		m_primaryParentAxis.Formats.SecondaryFormats.Clear();
		iPos++;
	}

	private void ParseAxisParent(IList<BiffRecordRaw> data, ref int iPos)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		BiffRecordRaw biffRecordRaw = data[iPos];
		biffRecordRaw.CheckTypeCode(TBIFFRecord.ChartAxisParent);
		switch (((ChartAxisParentRecord)biffRecordRaw.Clone()).AxesIndex)
		{
		case 0:
			m_primaryParentAxis.Parse(data, ref iPos);
			break;
		case 1:
			m_secondaryParentAxis.Parse(data, ref iPos);
			break;
		default:
			throw new ArgumentOutOfRangeException("Axes index must be 0 or 1.");
		}
	}

	private void ParseDataTable(IList<BiffRecordRaw> data, ref int iPos)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		data[iPos].CheckTypeCode(TBIFFRecord.ChartDat);
		m_bHasDataTable = true;
		m_dataTable = new ChartDataTableImpl(base.Application, this, data, ref iPos);
	}

	private List<ChartTextAreaImpl> AssignTextArea(ChartTextAreaImpl textArea, Dictionary<int, int> newSeriesIndexes)
	{
		if (textArea == null)
		{
			throw new ArgumentNullException("textArea");
		}
		ChartObjectLinkRecord objectLink = textArea.ObjectLink;
		if (objectLink == null)
		{
			throw new ArgumentNullException("objectLink");
		}
		List<ChartTextAreaImpl> list = null;
		switch (objectLink.LinkObject)
		{
		case ExcelObjectTextLink.Chart:
			m_title = textArea;
			break;
		case ExcelObjectTextLink.XAxis:
			CategoryAxisTitle = textArea.Text;
			break;
		case ExcelObjectTextLink.YAxis:
			ValueAxisTitle = textArea.Text;
			break;
		case ExcelObjectTextLink.ZAxis:
			SeriesAxisTitle = textArea.Text;
			break;
		case ExcelObjectTextLink.DataLabel:
		{
			int num = objectLink.SeriesNumber;
			int dataPointNumber = objectLink.DataPointNumber;
			if (newSeriesIndexes.ContainsKey(num))
			{
				num = newSeriesIndexes[num];
			}
			if (num >= m_series.Count)
			{
				if (list == null)
				{
					list = new List<ChartTextAreaImpl>();
				}
				list.Add(textArea);
			}
			else
			{
				((ChartDataPointImpl)((ChartSerieImpl)m_series[num]).DataPoints[dataPointNumber]).SetDataLabels(textArea);
			}
			break;
		}
		}
		return list;
	}

	private void ParsePlotArea(IList<BiffRecordRaw> data, ref int iPos)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		data[iPos].CheckTypeCode(TBIFFRecord.ChartPlotArea);
		iPos++;
		data[iPos].CheckTypeCode(TBIFFRecord.ChartFrame);
		InnerPlotArea.Parse(data, ref iPos);
	}

	private void ParseChartFrame(IList<BiffRecordRaw> data, ref int iPos)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		data[iPos].CheckTypeCode(TBIFFRecord.ChartFrame);
		iPos++;
		int num = 0;
		if (data[iPos].TypeCode != TBIFFRecord.Begin)
		{
			return;
		}
		num++;
		iPos++;
		while (num != 0)
		{
			switch (data[iPos].TypeCode)
			{
			case TBIFFRecord.Begin:
				num++;
				break;
			case TBIFFRecord.End:
				num--;
				break;
			}
			iPos++;
		}
	}

	public void DetectChartType()
	{
		if (Series.Count != 0)
		{
			m_chartType = m_primaryParentAxis.Formats.DetectChartType(m_series);
		}
	}

	private void SetDefaultValues()
	{
		m_plotAreaFrame = null;
		m_chartArea = null;
	}

	private IOfficeFont ParseFontx(ChartFontxRecord fontx)
	{
		if (fontx == null)
		{
			throw new ArgumentNullException("fontx");
		}
		int fontIndex = fontx.FontIndex;
		return new FontWrapper(m_book.InnerFonts[fontIndex] as FontImpl);
	}

	public void ParseLegend(IList<BiffRecordRaw> data, ref int iPos)
	{
		if (data == null)
		{
			throw new ArgumentException("data");
		}
		HasLegend = true;
		m_legend.Parse(data, ref iPos);
	}

	private bool AddRecords(IList<BiffRecordRaw> list, IList<BiffRecordRaw> holder, ref int iPos, ref int serieIndex, ref bool bIsErrorBars)
	{
		if (list == null)
		{
			throw new ArgumentNullException("list");
		}
		if (holder == null)
		{
			throw new ArgumentNullException("holder");
		}
		bool result = true;
		bIsErrorBars = false;
		BiffRecordRaw biffRecordRaw = list[iPos];
		biffRecordRaw.CheckTypeCode(TBIFFRecord.ChartSeries);
		iPos++;
		holder.Add(biffRecordRaw);
		biffRecordRaw = list[iPos];
		biffRecordRaw.CheckTypeCode(TBIFFRecord.Begin);
		iPos++;
		holder.Add(biffRecordRaw);
		int num = 1;
		while (num > 0)
		{
			biffRecordRaw = list[iPos];
			holder.Add(biffRecordRaw);
			switch (biffRecordRaw.TypeCode)
			{
			case TBIFFRecord.Begin:
				num++;
				break;
			case TBIFFRecord.End:
				num--;
				break;
			case TBIFFRecord.ChartSerAuxErrBar:
				result = false;
				bIsErrorBars = true;
				break;
			case TBIFFRecord.ChartSerAuxTrend:
				result = false;
				break;
			case TBIFFRecord.ChartSerParent:
				serieIndex = ((ChartSerParentRecord)biffRecordRaw).Series - 1;
				break;
			case TBIFFRecord.ChartDataFormat:
			{
				ChartDataFormatRecord chartDataFormatRecord = (ChartDataFormatRecord)biffRecordRaw;
				serieIndex = chartDataFormatRecord.SeriesIndex;
				break;
			}
			}
			iPos++;
		}
		return result;
	}

	private void ReparseTrendLegends()
	{
		ChartLegendEntriesColl chartLegendEntriesColl = (HasLegend ? ((ChartLegendEntriesColl)m_legend.LegendEntries) : null);
		int i = 0;
		for (int count = m_trendList.Count; i < count; i++)
		{
			int num = FindSeriesIndex(m_trendList, i);
			ChartTrendLineCollection chartTrendLineCollection = (ChartTrendLineCollection)((ChartSerieImpl)m_series[num]).TrendLines;
			ChartLegendEntryImpl entry;
			ChartTrendLineImpl trend = new ChartTrendLineImpl(base.Application, chartTrendLineCollection, m_trendList, ref i, out entry);
			chartTrendLineCollection.Add(trend);
			if (chartLegendEntriesColl != null && entry != null)
			{
				int legendEntryOffset = m_series.GetLegendEntryOffset(num);
				chartLegendEntriesColl.UpdateEntries(legendEntryOffset, 1);
				chartLegendEntriesColl.Add(legendEntryOffset, entry);
			}
		}
	}

	private int FindSeriesIndex(List<BiffRecordRaw> m_trendList, int i)
	{
		int result = -1;
		int count = m_trendList.Count;
		while (i < count)
		{
			BiffRecordRaw biffRecordRaw = m_trendList[i];
			if (biffRecordRaw.TypeCode == TBIFFRecord.ChartSerParent)
			{
				result = ((ChartSerParentRecord)biffRecordRaw).Series - 1;
				break;
			}
			i++;
		}
		return result;
	}

	[CLSCompliant(false)]
	public override void Serialize(OffsetArrayList records)
	{
		if (records == null)
		{
			throw new ArgumentNullException("records");
		}
		bool flag = IsChartExSerieType(ChartType);
		_ = m_arrRecords.Count;
		m_bof.Type = BOFRecord.TType.TYPE_CHART;
		m_bof.IsNested = FindParent(typeof(WorksheetImpl)) != null;
		if (m_arrRecords.Count > 0)
		{
			records.AddList(m_arrRecords);
			return;
		}
		records.Add(m_bof);
		SerializeHeaderFooterPictures(records);
		m_pageSetup.Serialize(records);
		SerializeFonts(records);
		if (m_bInWorksheet)
		{
			records.Add(BiffRecordFactory.GetRecord(TBIFFRecord.Protect));
		}
		else
		{
			SerializeProtection(records, bContentNotNecessary: true);
		}
		if (!flag)
		{
			SerializeMsoDrawings(records);
		}
		if (m_pivotList != null && m_pivotList.Count > 0)
		{
			records.AddRange(m_pivotList);
		}
		records.Add(BiffRecordFactory.GetRecord(TBIFFRecord.ChartUnits));
		SerializeChart(records, flag);
		if (!flag)
		{
			DimensionsRecord dimensionsRecord = (DimensionsRecord)BiffRecordFactory.GetRecord(TBIFFRecord.Dimensions);
			dimensionsRecord.LastColumn = (ushort)(m_series.TrendErrorBarIndex + 1);
			IRange range = ((m_series.Count > 0) ? (m_series[0].Values as ChartDataRange).Range : null);
			dimensionsRecord.LastRow = ((range != null) ? (m_series[0].Values as ChartDataRange).Range.Count : 0);
			records.Add(dimensionsRecord);
			SerializeChartSiIndexes(records);
		}
		if (!m_bInWorksheet)
		{
			SerializeWindowTwo(records);
			SerializeWindowZoom(records);
			SerializeSheetLayout(records);
		}
		SerializeMacrosSupport(records);
		records.Add(BiffRecordFactory.GetRecord(TBIFFRecord.EOF));
	}

	private void SerializeFonts(OffsetArrayList records)
	{
		records.AddList(m_arrFonts);
	}

	private void SerializeChart(OffsetArrayList records, bool isChartEx)
	{
		ChartChartRecord chartChartRecord = (ChartChartRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartChart);
		chartChartRecord.X = DoubleToFixedPoint(XPos);
		chartChartRecord.Y = DoubleToFixedPoint(YPos);
		chartChartRecord.Width = DoubleToFixedPoint(EMUWidth);
		chartChartRecord.Height = DoubleToFixedPoint(EMUHeight);
		if (chartChartRecord.Width == 0)
		{
			chartChartRecord.Width = 48027384;
			chartChartRecord.Height = 29506896;
		}
		records.Add(chartChartRecord);
		records.Add(BiffRecordFactory.GetRecord(TBIFFRecord.Begin));
		if (m_chartChartZoom != null)
		{
			records.Add(m_chartChartZoom);
		}
		else
		{
			WindowZoomRecord windowZoomRecord = (WindowZoomRecord)BiffRecordFactory.GetRecord(TBIFFRecord.WindowZoom);
			windowZoomRecord.NumMagnification = 1;
			windowZoomRecord.DenumMagnification = 1;
			records.Add(windowZoomRecord);
		}
		records.Add((BiffRecordRaw)PlotGrowth.Clone());
		if (m_chartArea != null)
		{
			m_chartArea.Serialize(records);
		}
		if (!isChartEx)
		{
			m_series.Serialize(records);
		}
		SerializeSheetProperties(records);
		SerializeDefaultText(records);
		SerializeAxes(records);
		if (m_plotAreaLayout != null)
		{
			records.Add((ChartPlotAreaLayoutRecord)m_plotAreaLayout.Clone());
		}
		if (!isChartEx)
		{
			records.AddRange(m_series.TrendLabels);
			SerializeDataTable(records);
			if (HasTitleInternal)
			{
				m_title.Serialize(records);
			}
			SerializeDataLabels(records);
		}
		records.Add(BiffRecordFactory.GetRecord(TBIFFRecord.End));
	}

	private void SerializeDefaultText(OffsetArrayList records)
	{
		if (records == null)
		{
			throw new ArgumentNullException("records");
		}
		int i = 0;
		for (int count = m_lstDefaultText.Count; i < count; i++)
		{
			int key = m_lstDefaultText.GetKey(i);
			List<BiffRecordRaw> byIndex = m_lstDefaultText.GetByIndex(i);
			ChartDefaultTextRecord chartDefaultTextRecord = (ChartDefaultTextRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartDefaultText);
			chartDefaultTextRecord.TextCharacteristics = (ChartDefaultTextRecord.TextDefaults)key;
			records.Add(chartDefaultTextRecord);
			records.AddList(byIndex);
		}
	}

	private void SerializeAxes(OffsetArrayList records)
	{
		ChartAxesUsedRecord chartAxesUsedRecord = (ChartAxesUsedRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartAxesUsed);
		int count = SecondaryFormats.Count;
		chartAxesUsedRecord.NumberOfAxes = (ushort)((count <= 0) ? 1u : 2u);
		records.Add(chartAxesUsedRecord);
		m_primaryParentAxis.Serialize(records);
		if (count > 0)
		{
			m_secondaryParentAxis.Serialize(records);
		}
	}

	private void SerializeSheetProperties(OffsetArrayList records)
	{
		records.Add((BiffRecordRaw)m_chartProperties.Clone());
	}

	private void SerializeChartSiIndexes(OffsetArrayList records)
	{
		if (records == null)
		{
			throw new ArgumentNullException("records");
		}
		ChartSiIndexRecord chartSiIndexRecord = (ChartSiIndexRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartSiIndex);
		chartSiIndexRecord.NumIndex = 2;
		records.Add(chartSiIndexRecord);
		SerializeChartSiMembers(records, 2);
		InsertSeriesLabels(records);
		chartSiIndexRecord = (ChartSiIndexRecord)chartSiIndexRecord.Clone();
		chartSiIndexRecord.NumIndex = 1;
		records.Add(chartSiIndexRecord);
		SerializeChartSiMembers(records, 1);
		chartSiIndexRecord = (ChartSiIndexRecord)chartSiIndexRecord.Clone();
		chartSiIndexRecord.NumIndex = 3;
		records.Add(chartSiIndexRecord);
		SerializeChartSiMembers(records, 3);
	}

	private void InsertSeriesLabels(OffsetArrayList records)
	{
	}

	private void InsertSeriesValues(OffsetArrayList records)
	{
		NumberRecord numberRecord = (NumberRecord)BiffRecordFactory.GetRecord(TBIFFRecord.Number);
		for (int i = 0; i < Series.Count; i++)
		{
			ChartSerieImpl obj = (ChartSerieImpl)Series[i];
			int num = 0;
			IRange[] cells = obj.ValuesIRange.Cells;
			foreach (IRange range in cells)
			{
				numberRecord = (NumberRecord)numberRecord.Clone();
				numberRecord.Row = (ushort)num;
				numberRecord.Column = (ushort)i;
				numberRecord.Value = range.Number;
				numberRecord.ExtendedFormatIndex = 0;
				records.Add(numberRecord);
				num++;
			}
		}
	}

	private void SerializeDataTable(OffsetArrayList records)
	{
		if (HasDataTable)
		{
			m_dataTable.Serialize(records);
		}
	}

	private void SerializeDataLabels(OffsetArrayList records)
	{
		m_series.SerializeDataLabels(records);
	}

	private void SerializeSeriesList(OffsetArrayList records)
	{
		if (IsChartVolume)
		{
			ChartSeriesListRecord chartSeriesListRecord = (ChartSeriesListRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartSeriesList);
			if (ChartType == OfficeChartType.Stock_VolumeHighLowClose)
			{
				chartSeriesListRecord.Series = new ushort[4] { 1, 2, 3, 4 };
			}
			else if (ChartType == OfficeChartType.Stock_VolumeOpenHighLowClose)
			{
				chartSeriesListRecord.Series = new ushort[5] { 1, 2, 3, 4, 5 };
			}
			records.Add(chartSeriesListRecord);
		}
	}

	private void SerializeChartSiMembers(OffsetArrayList records, int siIndex)
	{
		if (records == null)
		{
			throw new ArgumentNullException("records");
		}
		if (siIndex > 3 || siIndex < 1)
		{
			throw new ArgumentOutOfRangeException("siIndex");
		}
		List<BiffRecordRaw> enteredRecords = m_series.GetEnteredRecords(siIndex);
		if (enteredRecords != null && enteredRecords.Count > 0)
		{
			records.AddList(enteredRecords);
		}
	}

	[CLSCompliant(false)]
	public void SerializeLegend(OffsetArrayList records)
	{
		if (m_legend != null)
		{
			m_legend.Serialize(records);
		}
	}

	[CLSCompliant(false)]
	public void SerializeWalls(OffsetArrayList records)
	{
		if (records == null)
		{
			throw new ArgumentNullException("records");
		}
		if (m_walls != null)
		{
			m_walls.Serialize(records);
		}
	}

	[CLSCompliant(false)]
	public void SerializeFloor(OffsetArrayList records)
	{
		if (records == null)
		{
			throw new ArgumentNullException("records");
		}
		if (m_floor != null)
		{
			m_floor.Serialize(records);
		}
	}

	[CLSCompliant(false)]
	public void SerializePlotArea(OffsetArrayList records)
	{
		if (records == null)
		{
			throw new ArgumentNullException("records");
		}
		if (m_plotArea != null)
		{
			m_plotArea.Serialize(records);
		}
	}

	private void DetectAndUpdateDataRangeForChartEx(OfficeChartType type)
	{
		MigrantRangeImpl tempRange = new MigrantRangeImpl(base.Application, m_dataRange.Worksheet);
		IRange serieRange = null;
		IRange outputSerieNameRange = null;
		IRange outputAxisRange = null;
		bool flag = false;
		bool flag2 = false;
		if (m_dataRange.Columns.Length == 1 && m_dataRange.Rows.Length == 1)
		{
			serieRange = m_dataRange;
		}
		else
		{
			int rangeForChartEx = GetRangeForChartEx(tempRange, m_dataRange.LastRow, m_dataRange.Row, m_dataRange.LastColumn, isChangeRow: true);
			int rangeForChartEx2 = GetRangeForChartEx(tempRange, m_dataRange.LastColumn, m_dataRange.Column, m_dataRange.LastRow, isChangeRow: false);
			flag2 = m_dataRange[m_dataRange.Row, m_dataRange.Column].IsBlank;
			if (flag2)
			{
				outputSerieNameRange = GetSerieRangeByBlank(m_dataRange, out serieRange, isChartEx: true);
			}
			flag = GetSeriesRangesForChartEx(rangeForChartEx, rangeForChartEx2, flag2, serieRange, outputSerieNameRange, out serieRange, out outputSerieNameRange, out outputAxisRange);
		}
		int iIndex = 0;
		if (flag2 && outputSerieNameRange != null && outputAxisRange != null)
		{
			iIndex = ((!flag) ? (outputAxisRange.LastColumn - outputAxisRange.Column + 1) : 0);
			outputAxisRange = (flag ? outputAxisRange[outputAxisRange.Row, outputSerieNameRange.LastColumn + 1, outputAxisRange.LastRow, outputAxisRange.LastColumn] : outputAxisRange);
		}
		if (!ValidateSerieRangeForChartType(serieRange, type, flag))
		{
			throw new ApplicationException("Can't set data range.");
		}
		UpdateSeriesByDataRange(serieRange, outputSerieNameRange, outputAxisRange, ChartFormatImpl.GetStartSerieType(type), iIndex, flag);
		PrimaryCategoryAxis.CategoryLabels = (IOfficeDataRange)outputAxisRange;
		tempRange = null;
	}

	private bool GetSeriesRangesForChartEx(int reqRowsCount, int reqColsCount, bool isAnySpace, IRange inputSerieValue, IRange inputSerieNameRange, out IRange outputSerieValue, out IRange outputSerieNameRange, out IRange outputAxisRange)
	{
		bool result = false;
		outputSerieNameRange = inputSerieNameRange;
		outputSerieValue = inputSerieValue;
		outputAxisRange = null;
		if (isAnySpace || (reqColsCount < m_dataRange.Columns.Length && reqRowsCount < m_dataRange.Rows.Length))
		{
			if (reqRowsCount > reqColsCount)
			{
				if (outputSerieValue != null)
				{
					if (outputSerieNameRange != null && m_dataRange.Rows.Length != 1 && m_dataRange.Columns.Length != 1)
					{
						outputAxisRange = CheckForBlankAndAssignAxis(m_dataRange, outputSerieNameRange, out outputSerieNameRange, isSeriesInRows: false);
					}
					outputSerieValue = GetSerieRanges(outputSerieValue, outputSerieNameRange, outputAxisRange, m_dataRange.Columns.Length, m_dataRange.Rows.Length, isSeriesInRows: false);
				}
				else
				{
					outputSerieValue = m_dataRange[m_dataRange.LastRow - reqRowsCount + 1, m_dataRange.LastColumn - reqColsCount + 1, m_dataRange.LastRow, m_dataRange.LastColumn];
					outputAxisRange = m_dataRange[outputSerieValue.Row, m_dataRange.Column, m_dataRange.LastRow, outputSerieValue.Column - 1];
					outputSerieNameRange = m_dataRange[m_dataRange.Row, outputSerieValue.Column, outputSerieValue.Row - 1, m_dataRange.LastColumn];
				}
			}
			else
			{
				result = true;
				if (outputSerieValue != null)
				{
					if (outputSerieNameRange != null && m_dataRange.Rows.Length != 1 && m_dataRange.Columns.Length != 1)
					{
						outputSerieNameRange = CheckForBlankAndAssignAxis(m_dataRange, outputSerieNameRange, out outputAxisRange, isSeriesInRows: false);
					}
					outputSerieValue = GetSerieRanges(outputSerieValue, outputSerieNameRange, outputAxisRange, m_dataRange.Columns.Length, m_dataRange.Rows.Length, isSeriesInRows: true);
				}
				else
				{
					outputSerieValue = m_dataRange[m_dataRange.LastRow - reqRowsCount + 1, m_dataRange.LastColumn - reqColsCount + 1, m_dataRange.LastRow, m_dataRange.LastColumn];
					outputSerieNameRange = m_dataRange[outputSerieValue.Row, m_dataRange.Column, m_dataRange.LastRow, outputSerieValue.Column - 1];
					outputAxisRange = m_dataRange[m_dataRange.Row, outputSerieValue.Column, outputSerieValue.Row - 1, m_dataRange.LastColumn];
				}
			}
		}
		else if (m_dataRange.Rows.Length == 1)
		{
			outputSerieNameRange = m_dataRange[m_dataRange.Row, m_dataRange.Column, m_dataRange.LastRow, m_dataRange.LastColumn - reqColsCount];
			outputSerieValue = m_dataRange[m_dataRange.Row, outputSerieNameRange.LastColumn + 1, m_dataRange.LastRow, m_dataRange.LastColumn];
			result = true;
		}
		else if (m_dataRange.Columns.Length == 1)
		{
			outputSerieNameRange = m_dataRange[m_dataRange.Row, m_dataRange.Column, m_dataRange.LastRow - reqRowsCount, m_dataRange.LastColumn];
			outputSerieValue = m_dataRange[outputSerieNameRange.LastColumn + 1, m_dataRange.Column, m_dataRange.LastRow, m_dataRange.LastColumn];
		}
		else if (reqColsCount != m_dataRange.Columns.Length && reqRowsCount == m_dataRange.Rows.Length)
		{
			outputAxisRange = m_dataRange[m_dataRange.Row, m_dataRange.Column, m_dataRange.LastRow, m_dataRange.LastColumn - reqColsCount];
			outputSerieValue = m_dataRange[m_dataRange.Row, outputAxisRange.LastColumn + 1, m_dataRange.LastRow, m_dataRange.LastColumn];
		}
		else if (reqColsCount == m_dataRange.Columns.Length && reqRowsCount != m_dataRange.Rows.Length)
		{
			outputAxisRange = m_dataRange[m_dataRange.Row, m_dataRange.Column, m_dataRange.LastRow - reqRowsCount, m_dataRange.LastColumn];
			outputSerieValue = m_dataRange[outputAxisRange.LastRow + 1, m_dataRange.Column, m_dataRange.LastRow, m_dataRange.LastColumn];
			result = true;
		}
		else
		{
			outputSerieValue = m_dataRange;
		}
		return result;
	}

	private int GetRangeForChartEx(MigrantRangeImpl tempRange, int lastIndex, int index, int constantValue, bool isChangeRow)
	{
		bool flag = false;
		int num = 0;
		int num2 = 0;
		bool flag2 = false;
		for (int num3 = lastIndex; num3 >= index; num3--)
		{
			if (isChangeRow)
			{
				tempRange.ResetRowColumn(num3, constantValue);
			}
			else
			{
				tempRange.ResetRowColumn(constantValue, num3);
			}
			if (tempRange.HasNumber || tempRange.HasFormula)
			{
				num++;
				if (flag)
				{
					num += num2;
					flag = false;
					num2 = 0;
				}
				flag2 = true;
			}
			else
			{
				if (!tempRange.IsBlank)
				{
					if (flag && IsTreeMapOrSunBurst)
					{
						flag = false;
						num2 = 0;
					}
					if (num3 == lastIndex)
					{
						num++;
					}
					break;
				}
				num2++;
				flag = true;
			}
		}
		if (flag)
		{
			num = ((!IsTreeMapOrSunBurst) ? (num + num2) : ((!flag2) ? 1 : num));
		}
		return num;
	}

	public void UpdateCategory(ChartSeriesCollection series, bool fromDataRange)
	{
		IRange serieRange = null;
		if (series.Count == 0 || series[0].Values == null)
		{
			return;
		}
		int count = (series[0].Values as ChartDataRange).Range.Count;
		ChartImpl parentChart = (series[0] as ChartSerieImpl).ParentChart;
		IRange range = (parentChart.Series[0].CategoryLabels as ChartDataRange).Range;
		parentChart.DetectIsInRowOnParsing();
		if (parentChart.DataRange == null)
		{
			return;
		}
		parentChart.IsSeriesInRows = DetectIsInRow((parentChart.Series[0].Values as ChartDataRange).Range);
		IRange range2 = null;
		IRange range3 = null;
		if (m_dataRange[m_dataRange.Row, m_dataRange.Column].IsBlank)
		{
			range2 = GetSerieRangeByBlank(m_dataRange, out serieRange, isChartEx: false);
			if (range2 != null && m_dataRange.Rows.Length != 1 && m_dataRange.Columns.Length != 1)
			{
				range3 = CheckForBlankAndAssignAxis(m_dataRange, range2, out range2, m_bSeriesInRows);
			}
			if (range2 != null && (m_bSeriesInRows ? (m_dataRange.Rows.Length != 1) : (m_dataRange.Rows.Length == 1)))
			{
				IRange range4 = range2;
				range2 = range3;
				range3 = range4;
			}
			serieRange = GetSerieRanges(serieRange, range2, range3, m_dataRange.Columns.Length, m_dataRange.Rows.Length, m_bSeriesInRows);
		}
		else
		{
			GetSerieOrAxisRange(DataIRange, IsSeriesInRows, out serieRange);
			GetSerieOrAxisRange(serieRange, !IsSeriesInRows, out serieRange);
		}
		int count2 = serieRange.Count / count;
		for (int i = 0; i < count; i++)
		{
			IRange categoryRange = GetCategoryRange(serieRange, out serieRange, count2, parentChart.IsSeriesInRows);
			if (fromDataRange)
			{
				(parentChart.Categories as ChartCategoryCollection).Add();
			}
			if (parentChart.Categories.Count > i)
			{
				(parentChart.Categories[i] as ChartCategory).CategoryLabelIRange = range;
				(parentChart.Categories[i] as ChartCategory).ValuesIRange = categoryRange;
				if ((parentChart.Categories[0].CategoryLabel as ChartDataRange).Range != null)
				{
					(parentChart.Categories[i] as ChartCategory).Name = (parentChart.Categories[0].CategoryLabel as ChartDataRange).Range.Cells[i].Text;
				}
			}
		}
	}

	public void UpdateSeries(ChartSeriesCollection series)
	{
		for (int i = 0; i < series.Count; i++)
		{
			series[i].IsFiltered = false;
		}
	}

	public static IRange GetCategoryRange(IRange Chartvalues, out IRange values, int count, bool bIsInRow)
	{
		int row = Chartvalues.Row;
		int lastRow = Chartvalues.LastRow;
		int column = Chartvalues.Column;
		int lastColumn = Chartvalues.LastColumn;
		IRange range = null;
		if (Chartvalues.Count == count)
		{
			values = Chartvalues;
			return Chartvalues;
		}
		range = (bIsInRow ? Chartvalues[row, column, lastRow, column] : Chartvalues[row, column, row, lastColumn]);
		if (row == lastRow && column == lastColumn)
		{
			values = range;
		}
		else if (row == lastRow)
		{
			values = (bIsInRow ? Chartvalues[row, column, lastRow, lastColumn] : Chartvalues[row, column, lastRow, lastColumn]);
		}
		else
		{
			int num = ((bIsInRow ? (column != lastColumn) : (row != lastRow)) ? 1 : 0);
			values = (bIsInRow ? Chartvalues[row, column + num, lastRow, lastColumn] : Chartvalues[row + num, column, lastRow, lastColumn]);
		}
		return range;
	}

	private void GetFilter()
	{
		if (IsChartBubble)
		{
			int num = 0;
			foreach (IOfficeChartSerie item in m_series)
			{
				num += ((item.Bubbles == null) ? 1 : 2);
			}
			category = new bool[num];
		}
		else
		{
			category = new bool[m_series.Count];
		}
		series = new bool[(((IOfficeChartCategories)m_categories).Count != 0) ? ((IOfficeChartCategories)m_categories).Count : ((m_series[0].CategoryLabels != null) ? (m_series[0].CategoryLabels as ChartDataRange).Range.Count : 0)];
		for (int i = 0; i < m_series.Count; i++)
		{
			category[i] = ((IOfficeChartSeries)m_series)[i].IsFiltered;
		}
		for (int j = 0; j < ((IOfficeChartCategories)m_categories).Count; j++)
		{
			series[j] = ((IOfficeChartCategories)m_categories)[j].IsFiltered;
		}
		ChartImpl parentChart = (m_series[0] as ChartSerieImpl).ParentChart;
		if (parentChart != null)
		{
			OfficeSeriesNameLevel seriesNameLevel = parentChart.m_seriesNameLevel;
			OfficeCategoriesLabelLevel categoryLabelLevel = parentChart.CategoryLabelLevel;
			if (seriesNameLevel == OfficeSeriesNameLevel.SeriesNameLevelAll)
			{
				parentChart.CategoryLabelLevel = OfficeCategoriesLabelLevel.CategoriesLabelLevelAll;
			}
			else
			{
				parentChart.CategoryLabelLevel = OfficeCategoriesLabelLevel.CategoriesLabelLevelNone;
			}
			if (categoryLabelLevel == OfficeCategoriesLabelLevel.CategoriesLabelLevelAll)
			{
				parentChart.SeriesNameLevel = OfficeSeriesNameLevel.SeriesNameLevelAll;
			}
			else
			{
				parentChart.SeriesNameLevel = OfficeSeriesNameLevel.SeriesNameLevelNone;
			}
		}
	}

	private void Setfilter()
	{
		for (int i = 0; i < m_series.Count; i++)
		{
			((IOfficeChartSeries)m_series)[i].IsFiltered = series[i];
		}
		for (int j = 0; j < category.Length; j++)
		{
			m_categories.Add();
			((IOfficeChartCategories)m_categories)[j].IsFiltered = category[j];
		}
	}

	internal string GetChartTitle()
	{
		if (m_series.Count == 0)
		{
			return "Chart Title";
		}
		string result = "";
		ChartSerieImpl chartSerieImpl = m_series[0] as ChartSerieImpl;
		OfficeChartType chartType = ChartType;
		if (HasTitleInternal)
		{
			result = ((m_series.Count == 1) ? ((!chartSerieImpl.IsDefaultName && !IsChartExSerieType(chartType)) ? chartSerieImpl.Name : "Chart Title") : ((!chartType.ToString().Contains("Pie") || chartSerieImpl.IsDefaultName) ? "Chart Title" : chartSerieImpl.Name));
		}
		return result;
	}

	public static bool GetIsClustered(OfficeChartType chartType)
	{
		return Array.IndexOf(CHARTS_CLUSTERED, chartType) >= 0;
	}

	public static bool GetIs100(OfficeChartType chartType)
	{
		return Array.IndexOf(CHARTS_100, chartType) >= 0;
	}

	public static bool GetIsStacked(OfficeChartType chartType)
	{
		return Array.IndexOf(STACKEDCHARTS, chartType) >= 0;
	}

	public static bool GetIsChartPie(OfficeChartType chartType)
	{
		return chartType.ToString().StartsWith("Pie");
	}

	public void CreateNecessaryAxes(bool bPrimary)
	{
		if (bPrimary)
		{
			if (IsCategoryAxisAvail && m_primaryParentAxis.CategoryAxis == null)
			{
				m_primaryParentAxis.CategoryAxis = new ChartCategoryAxisImpl(base.Application, m_primaryParentAxis, OfficeAxisType.Category);
			}
			if (IsValueAxisAvail && m_primaryParentAxis.ValueAxis == null)
			{
				m_primaryParentAxis.ValueAxis = new ChartValueAxisImpl(base.Application, m_primaryParentAxis, OfficeAxisType.Value);
			}
			if (IsSeriesAxisAvail && m_primaryParentAxis.SeriesAxis == null)
			{
				m_primaryParentAxis.SeriesAxis = new ChartSeriesAxisImpl(base.Application, m_primaryParentAxis, OfficeAxisType.Serie);
			}
		}
		else if (m_secondaryParentAxis.CategoryAxis == null)
		{
			m_secondaryParentAxis.CategoryAxis = new ChartCategoryAxisImpl(base.Application, m_secondaryParentAxis, OfficeAxisType.Category, bIsPrimary: false);
			m_secondaryParentAxis.ValueAxis = new ChartValueAxisImpl(base.Application, m_secondaryParentAxis, OfficeAxisType.Value, bIsPrimary: false);
		}
	}

	protected override void InitializeCollections()
	{
		base.InitializeCollections();
		m_arrFonts = new List<ChartFbiRecord>();
		m_series = new ChartSeriesCollection(base.Application, this);
		m_pageSetup = new ChartPageSetupImpl(base.Application, this);
		m_categories = new ChartCategoryCollection(base.Application, this);
		m_primaryParentAxis = new ChartParentAxisImpl(base.Application, this);
		m_secondaryParentAxis = new ChartParentAxisImpl(base.Application, this, isPrimary: false);
		m_primaryParentAxis.CreatePrimaryFormats();
		m_secondaryParentAxis.UpdateSecondaryAxis(bCreateAxis: false);
		InitializeDefaultText();
	}

	private void CheckSupportDataTable()
	{
		if (ChartType != OfficeChartType.Combination_Chart)
		{
			CheckDataTablePossibility(ChartFormatImpl.GetStartSerieType(ChartType), bThrowException: true);
			return;
		}
		int i = 0;
		for (int count = Series.Count; i < count; i++)
		{
			CheckDataTablePossibility(ChartFormatImpl.GetStartSerieType((Series[i] as ChartSerieImpl).SerieType), bThrowException: true);
		}
	}

	public static bool CheckDataTablePossibility(string startType, bool bThrowException)
	{
		bool num = Array.IndexOf(DEF_SUPPORT_DATA_TABLE, startType) != -1;
		if (!num && bThrowException)
		{
			throw new NotSupportedException("Data table does not suported in this chart type");
		}
		return num;
	}

	private void InitializeDefaultText()
	{
		List<BiffRecordRaw> list = new List<BiffRecordRaw>();
		ChartTextRecord chartTextRecord = (ChartTextRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartText);
		chartTextRecord.IsAutoText = true;
		chartTextRecord.IsGenerated = true;
		chartTextRecord.HorzAlign = ExcelChartHorzAlignment.Center;
		chartTextRecord.VertAlign = ExcelChartVertAlignment.Center;
		list.Add(chartTextRecord);
		list.Add(BiffRecordFactory.GetRecord(TBIFFRecord.Begin));
		ChartPosRecord chartPosRecord = (ChartPosRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartPos);
		chartPosRecord.TopLeft = 2;
		chartPosRecord.BottomRight = 2;
		list.Add(chartPosRecord);
		ChartFontxRecord chartFontxRecord = (ChartFontxRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartFontx);
		list.Add(chartFontxRecord);
		ChartAIRecord chartAIRecord = (ChartAIRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartAI);
		chartAIRecord.Reference = ChartAIRecord.ReferenceType.EnteredDirectly;
		list.Add(chartAIRecord);
		list.Add(BiffRecordFactory.GetRecord(TBIFFRecord.End));
		m_lstDefaultText.Add(2, list);
		list = new List<BiffRecordRaw>();
		list.Add((BiffRecordRaw)chartTextRecord.Clone());
		list.Add(BiffRecordFactory.GetRecord(TBIFFRecord.Begin));
		list.Add((BiffRecordRaw)chartPosRecord.Clone());
		chartFontxRecord = (ChartFontxRecord)chartFontxRecord.Clone();
		list.Add(chartFontxRecord);
		list.Add((BiffRecordRaw)chartAIRecord.Clone());
		list.Add(BiffRecordFactory.GetRecord(TBIFFRecord.End));
		m_lstDefaultText.Add(3, list);
	}

	internal void OnDataRangeChanged(OfficeChartType type)
	{
		if (m_dataRange == null)
		{
			m_series.Clear();
			return;
		}
		IRange range = null;
		IRange range2 = null;
		IRange serieRange;
		if (m_dataRange[m_dataRange.Row, m_dataRange.Column].IsBlank)
		{
			range = GetSerieRangeByBlank(m_dataRange, out serieRange, isChartEx: false);
			if (range != null && m_dataRange.Rows.Length != 1 && m_dataRange.Columns.Length != 1)
			{
				range2 = CheckForBlankAndAssignAxis(m_dataRange, range, out range, m_bSeriesInRows);
			}
			if (range != null && (m_bSeriesInRows ? (m_dataRange.Rows.Length != 1) : (m_dataRange.Rows.Length == 1)))
			{
				IRange range3 = range;
				range = range2;
				range2 = range3;
			}
			serieRange = GetSerieRanges(serieRange, range, range2, m_dataRange.Columns.Length, m_dataRange.Rows.Length, m_bSeriesInRows);
		}
		else
		{
			range = GetSerieOrAxisRange(m_dataRange, m_bSeriesInRows, out serieRange);
			range2 = GetSerieOrAxisRange(serieRange, !m_bSeriesInRows, out serieRange);
		}
		if (!ValidateSerieRangeForChartType(serieRange, type, m_bSeriesInRows))
		{
			throw new ApplicationException("Cann't set data range.");
		}
		((ChartCategoryAxisImpl)PrimaryCategoryAxis).CategoryLabelsIRange = range2;
		int iIndex = 0;
		if (range != null && range2 != null)
		{
			iIndex = (m_bSeriesInRows ? (range2.LastRow - range2.Row + 1) : (range2.LastColumn - range2.Column + 1));
		}
		UpdateSeriesByDataRange(serieRange, range, range2, ChartFormatImpl.GetStartSerieType(type), iIndex, m_bSeriesInRows);
	}

	private IRange GetSerieRangeByBlank(IRange range, out IRange serieRange, bool isChartEx)
	{
		IRange range2 = null;
		int num = 0;
		int num2 = 0;
		int num3 = range.LastRow - range.Row + 1;
		int num4 = range.LastColumn - range.Column + 1;
		if (num3 == 1 && num4 == 1)
		{
			serieRange = range;
			return null;
		}
		MigrantRangeImpl migrantRangeImpl = new MigrantRangeImpl(base.Application, range.Worksheet);
		for (int i = 0; i < num3; i++)
		{
			int num5 = 0;
			_ = range.Row;
			for (int j = 0; j < num4; j++)
			{
				migrantRangeImpl.ResetRowColumn(range.Row + i, range.Column + j);
				if (!migrantRangeImpl.IsBlank)
				{
					break;
				}
				num5++;
			}
			if (i == 0)
			{
				num = num5;
			}
			else if (isChartEx)
			{
				if (num5 < num)
				{
					break;
				}
			}
			else if (num5 != num)
			{
				break;
			}
			num2++;
		}
		if (num4 == 1 || num3 == 1)
		{
			if (num4 == num && num3 == num2)
			{
				num2 = 1;
				num = 1;
			}
			range2 = ((num4 == 1) ? range[range.Row, range.Column, range.Row + num2 - 1, range.Column] : range[range.Row, range.Column, range.Row, range.Column + num - 1]);
			serieRange = ((num3 == 1) ? range[range2.Row, range2.LastColumn + 1, range.LastRow, range.LastColumn] : range[range2.LastRow + 1, range2.LastColumn, range.LastRow, range.LastColumn]);
		}
		else
		{
			if (num == num4)
			{
				num = 1;
			}
			if (num2 == num3)
			{
				num2 = 1;
			}
			range2 = range[range.Row, range.Column + num, range.Row + num2 - 1, range.LastColumn];
			serieRange = range[range2.LastRow + 1, range.Column, range.LastRow, range.LastColumn];
		}
		migrantRangeImpl = null;
		return range2;
	}

	private IRange CheckForBlankAndAssignAxis(IRange dataRange, IRange nameRangeInput, out IRange nameRangeOutput, bool isSeriesInRows)
	{
		int num = dataRange.LastColumn - dataRange.Column - (nameRangeInput.LastColumn - nameRangeInput.Column);
		int num2 = dataRange.LastColumn - dataRange.Column + 1;
		if (num != num2)
		{
			if (isSeriesInRows)
			{
				nameRangeOutput = nameRangeInput;
				return dataRange[nameRangeOutput.Row, dataRange.Column, dataRange.LastRow, nameRangeOutput.Column - 1];
			}
			nameRangeOutput = dataRange[dataRange.Row, dataRange.Column, nameRangeInput.LastRow, nameRangeInput.LastColumn];
			return dataRange[nameRangeOutput.LastRow + 1, dataRange.Column, dataRange.LastRow, dataRange.Column + num - 1];
		}
		nameRangeOutput = nameRangeInput;
		return null;
	}

	private IRange GetSerieRanges(IRange inputRange, IRange serieNameRange, IRange axisRange, int columnCount, int rowCount, bool isSeriesInRows)
	{
		inputRange = ((columnCount == 1 && (isSeriesInRows ? (axisRange != null) : (serieNameRange != null))) ? (isSeriesInRows ? inputRange[axisRange.LastRow + 1, inputRange.Column, inputRange.LastRow, inputRange.LastColumn] : inputRange[serieNameRange.LastRow + 1, inputRange.Column, inputRange.LastRow, inputRange.LastColumn]) : ((rowCount != 1 || !(isSeriesInRows ? (serieNameRange != null) : (axisRange != null))) ? (isSeriesInRows ? inputRange[inputRange.Row, serieNameRange.LastColumn + 1, inputRange.LastRow, inputRange.LastColumn] : inputRange[inputRange.Row, axisRange.LastColumn + 1, inputRange.LastRow, inputRange.LastColumn]) : (isSeriesInRows ? inputRange[inputRange.Row, serieNameRange.LastColumn + 1, inputRange.LastRow, inputRange.LastColumn] : inputRange[inputRange.Row, axisRange.LastColumn + 1, inputRange.LastRow, inputRange.LastColumn])));
		return inputRange;
	}

	private void AddDefaultRowSerie()
	{
		int i = m_dataRange.Row;
		for (int lastRow = m_dataRange.LastRow; i <= lastRow; i++)
		{
			ChartSerieImpl obj = (ChartSerieImpl)Series.Add();
			obj.ValuesIRange = m_dataRange[i, m_dataRange.Column, i, m_dataRange.LastColumn];
			obj.ValueRangeChanged += serie_ValueRangeChanged;
		}
	}

	private void AddDefaultColumnSerie()
	{
		int row = m_dataRange.Row;
		int lastRow = m_dataRange.LastRow;
		int lastColumn = m_dataRange.LastColumn;
		for (int i = m_dataRange.Column; i <= lastColumn; i++)
		{
			ChartSerieImpl obj = (ChartSerieImpl)Series.Add();
			obj.ValuesIRange = m_dataRange[row, i, lastRow, i];
			obj.ValueRangeChanged += serie_ValueRangeChanged;
		}
	}

	private void AddBubbleRowSerie()
	{
		int lastRow = m_dataRange.LastRow;
		int lastColumn = m_dataRange.LastColumn;
		int column = m_dataRange.Column;
		for (int i = m_dataRange.Row; i <= lastRow; i += 2)
		{
			ChartSerieImpl obj = (ChartSerieImpl)Series.Add();
			obj.BubblesIRange = m_dataRange[i, column, i, lastColumn];
			obj.ValuesIRange = m_dataRange[i + 1, column, i + 1, lastColumn];
			obj.ValueRangeChanged += serie_ValueRangeChanged;
		}
	}

	private void AddBubbleColumnSerie()
	{
		int row = m_dataRange.Row;
		int lastRow = m_dataRange.LastRow;
		int lastColumn = m_dataRange.LastColumn;
		for (int i = m_dataRange.Column; i <= lastColumn; i++)
		{
			ChartSerieImpl obj = (ChartSerieImpl)Series.Add();
			obj.BubblesIRange = m_dataRange[row, i, lastRow, i];
			obj.ValuesIRange = m_dataRange[row, i + 1, lastRow, i + 1];
			obj.ValueRangeChanged += serie_ValueRangeChanged;
		}
	}

	private void AddScatterRowSerie()
	{
		int row = m_dataRange.Row;
		int column = m_dataRange.Column;
		int lastRow = m_dataRange.LastRow;
		int lastColumn = m_dataRange.LastColumn;
		for (int i = row + 1; i <= lastRow; i += 2)
		{
			ChartSerieImpl obj = (ChartSerieImpl)Series.Add();
			obj.CategoryLabelsIRange = m_dataRange[row, column, row, lastColumn];
			obj.ValuesIRange = m_dataRange[i, column, i, lastColumn];
			obj.ValueRangeChanged += serie_ValueRangeChanged;
		}
	}

	private void AddScatterColumnSerie()
	{
		int row = m_dataRange.Row;
		int column = m_dataRange.Column;
		int lastRow = m_dataRange.LastRow;
		int lastColumn = m_dataRange.LastColumn;
		for (int i = column + 1; i <= lastColumn; i++)
		{
			ChartSerieImpl obj = (ChartSerieImpl)Series.Add();
			obj.CategoryLabelsIRange = m_dataRange[row, column, lastRow, column];
			obj.ValuesIRange = m_dataRange[row, i, lastRow, i];
			obj.ValueRangeChanged += serie_ValueRangeChanged;
		}
	}

	private void AddStockHLCRowSerie(int count)
	{
		int row = m_dataRange.Row;
		int column = m_dataRange.Column;
		int lastRow = m_dataRange.LastRow;
		_ = m_dataRange.LastColumn;
		if (lastRow - row != count - 1)
		{
			throw new ArgumentOutOfRangeException("There should be " + count + " rows for this chart type.");
		}
		ChartSerieImpl chartSerieImpl;
		for (int i = 0; i < count; i++)
		{
			chartSerieImpl = (ChartSerieImpl)Series.Add();
			chartSerieImpl.ValuesIRange = m_dataRange[row + i, column, lastRow + i, column];
			chartSerieImpl.ValueRangeChanged += serie_ValueRangeChanged;
		}
		chartSerieImpl = (ChartSerieImpl)Series[count - 1];
		SetStockSerieFormat(chartSerieImpl);
	}

	private void AddStockHLCColumnSerie(int count)
	{
		int row = m_dataRange.Row;
		int column = m_dataRange.Column;
		int lastRow = m_dataRange.LastRow;
		if (m_dataRange.LastColumn - column != count - 1)
		{
			throw new ArgumentOutOfRangeException("There should be " + count + " columns for this chart type.");
		}
		ChartSerieImpl chartSerieImpl;
		for (int i = 0; i < count; i++)
		{
			chartSerieImpl = (ChartSerieImpl)Series.Add();
			chartSerieImpl.ValuesIRange = m_dataRange[row, column + i, lastRow, column + i];
			chartSerieImpl.ValueRangeChanged += serie_ValueRangeChanged;
		}
		chartSerieImpl = (ChartSerieImpl)Series[count - 1];
		SetStockSerieFormat(chartSerieImpl);
	}

	private void AddStockVolumeRowSerie(int count)
	{
		int row = m_dataRange.Row;
		int column = m_dataRange.Column;
		int lastRow = m_dataRange.LastRow;
		_ = m_dataRange.LastColumn;
		if (lastRow - row != count - 1)
		{
			throw new ArgumentOutOfRangeException("There should be " + count + " rows for this chart type.");
		}
		ChartSerieImpl obj = (ChartSerieImpl)m_series.Add();
		obj.ValuesIRange = m_dataRange[row, column, lastRow, column];
		obj.Number = count - 1;
		obj.ValueRangeChanged += serie_ValueRangeChanged;
		for (int i = 1; i < count; i++)
		{
			ChartSerieImpl obj2 = (ChartSerieImpl)m_series.Add();
			obj2.ValuesIRange = m_dataRange[row + i, column, lastRow + i, column];
			obj2.Number = i - 1;
			obj2.ChartGroup = 1;
			obj2.ValueRangeChanged += serie_ValueRangeChanged;
		}
		if (count == 4)
		{
			SetStockSerieFormat((ChartSerieImpl)m_series[3]);
		}
		SetVolumeSecondaryAxisFormat();
	}

	private void AddStockVolumeColumnSerie(int count)
	{
		int row = m_dataRange.Row;
		int column = m_dataRange.Column;
		int lastRow = m_dataRange.LastRow;
		if (m_dataRange.LastColumn - column != count - 1)
		{
			throw new ArgumentOutOfRangeException("There should be " + count + " columns for this chart type.");
		}
		ChartSerieImpl obj = (ChartSerieImpl)m_series.Add();
		obj.ValuesIRange = m_dataRange[row, column, lastRow, column];
		obj.Number = count - 1;
		obj.ValueRangeChanged += serie_ValueRangeChanged;
		for (int i = 1; i < count; i++)
		{
			ChartSerieImpl obj2 = (ChartSerieImpl)m_series.Add();
			obj2.ValuesIRange = m_dataRange[row, column + i, lastRow, column + i];
			obj2.Number = i - 1;
			obj2.ChartGroup = 1;
			obj2.ValueRangeChanged += serie_ValueRangeChanged;
		}
		if (count == 4)
		{
			SetStockSerieFormat((ChartSerieImpl)m_series[3]);
		}
		SetVolumeSecondaryAxisFormat();
	}

	private void SetVolumeSecondaryAxisFormat()
	{
		SecondaryCategoryAxis.IsMaxCross = true;
		_ = (ChartCategoryAxisImpl)SecondaryCategoryAxis;
		m_chartProperties.IsManSerAlloc = true;
	}

	private void SetStockSerieFormat(ChartSerieImpl serie)
	{
		if (serie == null)
		{
			throw new ArgumentNullException("serie");
		}
		ChartSerieDataFormatImpl obj = (ChartSerieDataFormatImpl)((ChartDataPointImpl)serie.DataPoints.DefaultDataPoint).DataFormat;
		IOfficeChartBorder lineProperties = obj.LineProperties;
		lineProperties.LinePattern = OfficeChartLinePattern.None;
		lineProperties.LineWeight = OfficeChartLineWeight.Hairline;
		obj.PieFormat.Percent = 0;
		ChartMarkerFormatRecord markerFormat = obj.MarkerFormat;
		markerFormat.MarkerType = OfficeChartMarkerType.DowJones;
		markerFormat.LineSize = 60;
	}

	private void OnSeriesInRowsChanged()
	{
		if (m_dataRange != null)
		{
			OnDataRangeChanged(ChartType);
		}
	}

	private void UpdateSeriesInBubleChart()
	{
		ChartSeriesCollection chartSeriesCollection = m_series;
		m_series = new ChartSeriesCollection(base.Application, m_series.Parent);
		int i = 0;
		for (int count = chartSeriesCollection.Count; i < count; i++)
		{
			ChartSerieImpl obj = (ChartSerieImpl)m_series.Add();
			ChartSerieImpl chartSerieImpl = (ChartSerieImpl)chartSeriesCollection[i];
			obj.ValuesIRange = chartSerieImpl.ValuesIRange;
			IRange bubblesIRange = chartSerieImpl.BubblesIRange;
			if (bubblesIRange != null)
			{
				ChartSerieImpl chartSerieImpl2 = new ChartSerieImpl(base.Application, m_series);
				chartSerieImpl2.ValuesIRange = bubblesIRange;
				m_series.Add(chartSerieImpl2);
			}
		}
	}

	private void OnChartTypeChanged(OfficeChartType type, bool isSeriesCreation)
	{
		if (type == OfficeChartType.Combination_Chart)
		{
			throw new ArgumentException("Cannot change chart type.");
		}
		HasDataTable = false;
		m_series.ClearErrorBarsAndTrends();
		if (ChartStartType == "Bubble")
		{
			UpdateSeriesInBubleChart();
		}
		m_primaryParentAxis.Formats.Clear();
		bool flag = Loading || IsChartExSerieType(type) || IsChartExSerieType(m_chartType) || Array.IndexOf(DEF_NEED_VIEW_3D, type) != -1;
		m_series.ClearSeriesForChangeChartType(!flag, type);
		if (Array.IndexOf(CHARTS_STOCK, type) != -1)
		{
			ChangeChartStockType(type);
			return;
		}
		ChartFormatImpl chartFormatImpl = new ChartFormatImpl(base.Application, PrimaryFormats);
		PrimaryFormats.Add(chartFormatImpl, bCanReplace: false);
		chartFormatImpl.ChangeChartType(type, isSeriesCreation);
		UpdateChartMembersOnTypeChanging(type, isChartExChanges: false);
	}

	private void UpdateSurfaceTickRecord()
	{
		((ChartAxisImpl)PrimaryCategoryAxis).UpdateTickRecord(OfficeTickLabelPosition.TickLabelPosition_Low);
		((ChartAxisImpl)PrimaryValueAxis).UpdateTickRecord(OfficeTickLabelPosition.TickLabelPosition_None);
		((ChartAxisImpl)PrimarySerieAxis).UpdateTickRecord(OfficeTickLabelPosition.TickLabelPosition_Low);
	}

	private void UpdateRadarTickRecord()
	{
		((ChartAxisImpl)PrimaryCategoryAxis).UpdateTickRecord(OfficeTickLabelPosition.TickLabelPosition_NextToAxis);
		((ChartAxisImpl)PrimaryValueAxis).UpdateTickRecord(OfficeTickLabelPosition.TickLabelPosition_NextToAxis);
	}

	private void UpdateChartMembersOnTypeChanging(OfficeChartType type, bool isChartExChanges)
	{
		if (m_book.IsWorkbookOpening || isChartExChanges)
		{
			return;
		}
		if (!m_bHasLegend)
		{
			m_bHasLegend = true;
			m_legend = new ChartLegendImpl(base.Application, this);
		}
		m_primaryParentAxis.ClearGridLines();
		SetToDefaultGridlines(type);
		m_sidewall = new ChartWallOrFloorImpl(base.Application, this, bWalls: true);
		m_walls = new ChartWallOrFloorImpl(base.Application, this, bWalls: true);
		m_floor = new ChartWallOrFloorImpl(base.Application, this, bWalls: false);
		if (!isChartExChanges)
		{
			m_plotArea = new ChartPlotAreaImpl(base.Application, this, type);
		}
		if (Array.IndexOf(CHARTS3D, ChartType) != -1)
		{
			((ChartAxisImpl)PrimaryCategoryAxis).UpdateTickRecord(OfficeTickLabelPosition.TickLabelPosition_Low);
		}
		if (Array.IndexOf(DEF_SUPPORT_SERIES_AXIS, type) != -1)
		{
			ChartAxisImpl chartAxisImpl = m_primaryParentAxis.SeriesAxis;
			if (chartAxisImpl == null)
			{
				ChartSeriesAxisImpl chartSeriesAxisImpl2 = (m_primaryParentAxis.SeriesAxis = new ChartSeriesAxisImpl(base.Application, m_primaryParentAxis, OfficeAxisType.Serie));
				chartAxisImpl = chartSeriesAxisImpl2;
			}
			chartAxisImpl.UpdateTickRecord(OfficeTickLabelPosition.TickLabelPosition_Low);
		}
		else
		{
			m_primaryParentAxis.SeriesAxis = null;
		}
		switch (type)
		{
		case OfficeChartType.Surface_Contour:
		case OfficeChartType.Surface_NoColor_Contour:
			UpdateSurfaceTickRecord();
			if (type == OfficeChartType.Surface_NoColor_Contour)
			{
				m_sidewall.Interior.Pattern = OfficePattern.None;
				m_walls.Interior.Pattern = OfficePattern.None;
				m_floor.Interior.Pattern = OfficePattern.None;
			}
			break;
		case OfficeChartType.Radar:
		case OfficeChartType.Radar_Markers:
		case OfficeChartType.Radar_Filled:
			UpdateRadarTickRecord();
			break;
		default:
			((ChartAxisImpl)PrimaryValueAxis).UpdateTickRecord(OfficeTickLabelPosition.TickLabelPosition_Low);
			break;
		}
	}

	internal static bool IsChartExSerieType(OfficeChartType type)
	{
		if ((uint)(type - 74) <= 6u)
		{
			return true;
		}
		return false;
	}

	internal void ChangeToChartExType(OfficeChartType oldChartType, OfficeChartType type, bool isSeriesCreation)
	{
		m_series.ClearErrorBarsAndTrends();
		bool flag = (oldChartType == OfficeChartType.Pareto || oldChartType == OfficeChartType.Histogram) && type != OfficeChartType.Histogram && type != OfficeChartType.Pareto;
		if (isSeriesCreation && type != oldChartType && !flag)
		{
			m_series.ClearSeriesForChangeChartType();
		}
		HasDataTable = false;
		PrimaryParentAxis.Formats.Clear();
		ChartFormatImpl chartFormatImpl = new ChartFormatImpl(base.Application, PrimaryFormats);
		PrimaryFormats.Add(chartFormatImpl, bCanReplace: false);
		chartFormatImpl.ChangeChartType(type, isSeriesCreation);
		UpdateChartMembersOnTypeChanging(type, isChartExChanges: true);
		if (!Loading)
		{
			switch (type)
			{
			case OfficeChartType.Funnel:
				chartFormatImpl.GapWidth = 6;
				break;
			case OfficeChartType.BoxAndWhisker:
				chartFormatImpl.GapWidth = 50;
				break;
			case OfficeChartType.Pareto:
			case OfficeChartType.Histogram:
				chartFormatImpl.GapWidth = 0;
				break;
			}
		}
		else
		{
			chartFormatImpl.GapWidth = 100;
		}
		m_chartType = type;
		if (IsChartExSerieType(oldChartType))
		{
			PrimaryCategoryAxis.MajorTickMark = OfficeTickMark.TickMark_None;
			PrimaryValueAxis.MajorTickMark = OfficeTickMark.TickMark_None;
		}
		if (type == OfficeChartType.Pareto)
		{
			for (int i = 0; i < m_series.Count; i++)
			{
				(m_series[i] as ChartSerieImpl).ParetoLineFormat = new ChartFrameFormatImpl(base.Application, m_series[i]);
			}
			m_secondaryParentAxis.UpdateSecondaryAxis(bCreateAxis: true);
			chartFormatImpl = new ChartFormatImpl(base.Application, SecondaryFormats);
			chartFormatImpl.DrawingZOrder = 1;
			SecondaryFormats.Add(chartFormatImpl);
			SecondaryFormats.IsParetoFormat = true;
			if (!IsChartExSerieType(oldChartType))
			{
				SecondaryValueAxis.MajorTickMark = OfficeTickMark.TickMark_None;
			}
		}
		else if (oldChartType == OfficeChartType.Pareto)
		{
			for (int j = 0; j < m_series.Count; j++)
			{
				(m_series[j] as ChartSerieImpl).ParetoLineFormat = null;
			}
			SecondaryFormats.IsParetoFormat = false;
		}
		if (flag)
		{
			for (int k = 0; k < m_series.Count; k++)
			{
				(m_series[k].SerieFormat as ChartSerieDataFormatImpl).HistogramAxisFormatProperty = null;
			}
		}
		if (Loading)
		{
			return;
		}
		if (!isSeriesCreation && (type == OfficeChartType.TreeMap || type == OfficeChartType.SunBurst))
		{
			for (int l = 0; l < m_series.Count; l++)
			{
				ChartDataPointImpl chartDataPointImpl = m_series[l].DataPoints.DefaultDataPoint as ChartDataPointImpl;
				if (!chartDataPointImpl.HasDataLabels)
				{
					chartDataPointImpl.DataLabels.IsCategoryName = true;
					chartDataPointImpl.DataLabels.Delimiter = ",";
					chartDataPointImpl.DataLabels.Position = OfficeDataLabelPosition.Inside;
				}
			}
		}
		else if (isSeriesCreation && (type == OfficeChartType.Funnel || type == OfficeChartType.WaterFall))
		{
			for (int m = 0; m < m_series.Count; m++)
			{
				ChartDataPointImpl chartDataPointImpl2 = m_series[m].DataPoints.DefaultDataPoint as ChartDataPointImpl;
				if (!chartDataPointImpl2.HasDataLabels)
				{
					chartDataPointImpl2.DataLabels.IsValue = true;
					chartDataPointImpl2.DataLabels.Delimiter = ",";
					chartDataPointImpl2.DataLabels.Position = OfficeDataLabelPosition.Inside;
				}
			}
		}
		if (m_bHasLegend && m_legend != null)
		{
			m_legend.IncludeInLayout = false;
			m_legend.Position = OfficeLegendPosition.Top;
		}
	}

	private void ChangeChartStockType(OfficeChartType type)
	{
		int count = m_series.Count;
		switch (type)
		{
		case OfficeChartType.Stock_HighLowClose:
		{
			if (count != 3)
			{
				throw new ArgumentException("Cannot change serie type.");
			}
			ChartFormatImpl chartFormatImpl = new ChartFormatImpl(base.Application, PrimaryFormats);
			PrimaryFormats.Add(chartFormatImpl);
			chartFormatImpl.ChangeChartStockHigh_Low_CloseType();
			break;
		}
		case OfficeChartType.Stock_OpenHighLowClose:
		{
			if (count != 4)
			{
				throw new ArgumentException("Cannot change serie type.");
			}
			ChartFormatImpl chartFormatImpl = new ChartFormatImpl(base.Application, PrimaryFormats);
			PrimaryFormats.Add(chartFormatImpl);
			chartFormatImpl.ChangeChartStockOpen_High_Low_CloseType();
			break;
		}
		case OfficeChartType.Stock_VolumeHighLowClose:
		{
			if (count != 4)
			{
				throw new ArgumentException("Cannot change serie type.");
			}
			ChartFormatImpl chartFormatImpl = new ChartFormatImpl(base.Application, PrimaryFormats);
			PrimaryFormats.Add(chartFormatImpl);
			chartFormatImpl.ChangeChartStockVolume_High_Low_CloseTypeFirst();
			chartFormatImpl = new ChartFormatImpl(base.Application, SecondaryFormats);
			chartFormatImpl.DrawingZOrder = 1;
			SecondaryFormats.Add(chartFormatImpl);
			chartFormatImpl.ChangeChartStockVolume_High_Low_CloseTypeSecond();
			break;
		}
		case OfficeChartType.Stock_VolumeOpenHighLowClose:
		{
			if (count != 5)
			{
				throw new ArgumentException("Cannot change serie type.");
			}
			IsManuallyFormatted = true;
			m_secondaryParentAxis.UpdateSecondaryAxis(bCreateAxis: true);
			ChartFormatImpl chartFormatImpl = new ChartFormatImpl(base.Application, PrimaryFormats);
			PrimaryFormats.Add(chartFormatImpl);
			chartFormatImpl.ChangeChartStockVolume_High_Low_CloseTypeFirst();
			chartFormatImpl = new ChartFormatImpl(base.Application, SecondaryFormats);
			chartFormatImpl.DrawingZOrder = 1;
			SecondaryFormats.Add(chartFormatImpl);
			chartFormatImpl.ChangeChartStockVolume_Open_High_Low_CloseType();
			SecondaryCategoryAxis.IsMaxCross = true;
			break;
		}
		default:
			throw new ArgumentException("type");
		}
		IsStock = true;
	}

	private void UpdateSeriesByDataRange(IRange serieValue, IRange serieNameRange, IRange axisRange, string strType, int iIndex, bool isSeriesInRows)
	{
		bool isClearNameRange = serieNameRange == null;
		if (strType == "Bubble")
		{
			int num = 0;
			int num2 = 0;
			int count = m_series.Count;
			while (num2 < count)
			{
				IRange valuesIRange = (isSeriesInRows ? serieValue[serieValue.Row + num, serieValue.Column, serieValue.Row + num, serieValue.LastColumn] : serieValue[serieValue.Row, serieValue.Column + num, serieValue.LastRow, serieValue.Column + num]);
				if (isSeriesInRows ? (serieValue.Rows.Length >= num + 2) : (serieValue.Columns.Length >= num + 2))
				{
					Series[num2].Bubbles = (isSeriesInRows ? ((IOfficeDataRange)serieValue[serieValue.Row + num + 1, serieValue.Column, serieValue.Row + num + 1, serieValue.LastColumn]) : ((IOfficeDataRange)serieValue[serieValue.Row, serieValue.Column + num + 1, serieValue.LastRow, serieValue.Column + num + 1]));
				}
				ChartSerieImpl chartSerieImpl = (ChartSerieImpl)Series[num2];
				chartSerieImpl.SetDefaultName(m_series.GetDefSerieName(num2), isClearNameRange);
				int num3 = iIndex;
				chartSerieImpl.ValuesIRange = valuesIRange;
				if (serieNameRange != null)
				{
					num3 += (isSeriesInRows ? serieNameRange.Row : serieNameRange.Column);
					string text = (isSeriesInRows ? serieValue[num3 + num, serieNameRange.Column, num3 + num, serieNameRange.LastColumn].AddressGlobal : serieValue[serieNameRange.Row, num3 + num, serieNameRange.LastRow, num3 + num].AddressGlobal);
					chartSerieImpl.Name = "=" + text;
				}
				num2++;
				num += 2;
			}
			return;
		}
		if (axisRange != null && !axisRange.IsBlank && strType == "Pareto")
		{
			PrimaryCategoryAxis.IsBinningByCategory = true;
		}
		int i = 0;
		for (int count2 = m_series.Count; i < count2; i++)
		{
			IRange valuesIRange2 = (isSeriesInRows ? serieValue[serieValue.Row + i, serieValue.Column, serieValue.Row + i, serieValue.LastColumn] : serieValue[serieValue.Row, serieValue.Column + i, serieValue.LastRow, serieValue.Column + i]);
			ChartSerieImpl chartSerieImpl2 = (ChartSerieImpl)Series[i];
			chartSerieImpl2.BubblesIRange = null;
			chartSerieImpl2.SetDefaultName(m_series.GetDefSerieName(i), isClearNameRange);
			int num4 = iIndex;
			chartSerieImpl2.ValuesIRange = valuesIRange2;
			if (serieNameRange != null)
			{
				num4 += (isSeriesInRows ? serieNameRange.Row : serieNameRange.Column);
				string text2 = (isSeriesInRows ? serieValue[num4 + i, serieNameRange.Column, num4 + i, serieNameRange.LastColumn].AddressGlobal : serieValue[serieNameRange.Row, num4 + i, serieNameRange.LastRow, num4 + i].AddressGlobal);
				chartSerieImpl2.Name = "=" + text2;
			}
		}
	}

	private Chart3DRecord GetChart3D()
	{
		Chart3DRecord chart3DRecord = (Chart3DRecord)BiffRecordFactory.GetRecord(TBIFFRecord.Chart3D);
		chart3DRecord.IsPerspective = IsPerspective;
		chart3DRecord.IsClustered = IsClustered;
		switch (ChartType)
		{
		case OfficeChartType.Surface_Contour:
		case OfficeChartType.Surface_NoColor_Contour:
			chart3DRecord.RotationAngle = 0;
			chart3DRecord.ElevationAngle = 90;
			chart3DRecord.DistanceFromEye = 0;
			break;
		case OfficeChartType.Pie_3D:
		case OfficeChartType.Pie_Exploded_3D:
			chart3DRecord.RotationAngle = 0;
			chart3DRecord.IsAutoScaled = false;
			chart3DRecord.Is2DWalls = false;
			break;
		}
		return chart3DRecord;
	}

	private void serie_ValueRangeChanged(object sender, ValueChangedEventArgs e)
	{
		m_dataRange = null;
	}

	private void InitializeFrames()
	{
		m_chartArea = new ChartFrameFormatImpl(base.Application, this);
		m_chartArea.Interior.ForegroundColorIndex = OfficeKnownColors.WhiteCustom;
		m_plotAreaFrame = new ChartFrameFormatImpl(base.Application, this, bAutoSize: true, bIsInteriorGrey: false, bSetDefaults: true);
	}

	public void RemoveFormat(IOfficeChartFormat formatToRemove)
	{
		if (formatToRemove == null)
		{
			throw new ArgumentNullException("formatToRemove");
		}
		m_primaryParentAxis.Formats.Remove((ChartFormatImpl)formatToRemove);
	}

	public void UpdateChartTitle()
	{
		if (m_title == null)
		{
			return;
		}
		ChartTextAreaImpl chartTextAreaImpl = ChartTitleArea as ChartTextAreaImpl;
		if (ChartTitle != null || !chartTextAreaImpl.TextRecord.IsAutoMode)
		{
			return;
		}
		int count = Series.Count;
		if (count <= 0)
		{
			return;
		}
		ChartSerieImpl chartSerieImpl = (ChartSerieImpl)Series[0];
		if (!chartSerieImpl.IsDefaultName)
		{
			string text = ((count != 1) ? GetChartTypeStart() : null);
			if (count == 1 || text == "Pie" || text == "Doughnut")
			{
				string parseSerieNotDefaultText = chartSerieImpl.ParseSerieNotDefaultText;
				ChartTitle = parseSerieNotDefaultText;
			}
		}
	}

	private string GetChartTypeStart()
	{
		if (m_series.Count == 0)
		{
			return null;
		}
		string text = (m_series[0] as ChartSerieImpl).DetectSerieTypeStart();
		int i = 1;
		for (int count = m_series.Count; i < count; i++)
		{
			if ((m_series[i] as ChartSerieImpl).DetectSerieTypeStart() != text)
			{
				text = null;
				break;
			}
		}
		return text;
	}

	internal IRange DetectDataRange()
	{
		if (m_series.Count == 0)
		{
			return null;
		}
		ChartSerieImpl obj = (ChartSerieImpl)Series[0];
		IRange valuesIRange = obj.ValuesIRange;
		IRange serieNameRange = obj.GetSerieNameRange();
		IRange bubblesIRange = obj.BubblesIRange;
		if (valuesIRange == null || valuesIRange.Worksheet == null)
		{
			return null;
		}
		IWorksheet worksheet = valuesIRange.Worksheet;
		string name = worksheet.Name;
		IRange seriesValuesRange = GetSeriesValuesRange(valuesIRange, bubblesIRange, worksheet, name);
		IRange range = (PrimaryCategoryAxis.CategoryLabels as ChartDataRange).Range;
		if (range != null && range.Worksheet != null && name != range.Worksheet.Name)
		{
			return null;
		}
		if (seriesValuesRange == null)
		{
			return null;
		}
		if (!GetSerieNameValuesRange(serieNameRange, bubblesIRange, worksheet, name, out var result))
		{
			return null;
		}
		Rectangle rec = RangeImpl.GetRectangeOfRange(seriesValuesRange, bThrowExcONNullRange: true);
		if (result != null && !GetDataRangeRec(result, ref rec, !m_bSeriesInRows))
		{
			return null;
		}
		if (range != null && !GetDataRangeRec(range, ref rec, m_bSeriesInRows))
		{
			return null;
		}
		return worksheet[rec.Top, rec.Left, rec.Bottom, rec.Right];
	}

	private bool DetectIsInRow(IRange range)
	{
		if (range == null)
		{
			return true;
		}
		int num = range.LastRow - range.Row;
		int num2 = range.LastColumn - range.Column;
		return num <= num2;
	}

	public IRange GetSerieOrAxisRange(IRange range, bool bIsInRow, out IRange serieRange)
	{
		if (range == null)
		{
			throw new ArgumentNullException("range");
		}
		int num = (bIsInRow ? range.Row : range.Column);
		int num2 = (bIsInRow ? range.LastRow : range.LastColumn);
		int num3 = (bIsInRow ? range.Column : range.Row);
		int num4 = (bIsInRow ? range.LastColumn : range.LastRow);
		int num5 = -1;
		bool flag = false;
		for (int i = num3; i < num4; i++)
		{
			if (flag)
			{
				break;
			}
			IRange range2 = (bIsInRow ? range[num2, i] : range[i, num2]);
			flag = range2.HasNumber || range2.IsBlank || range2.HasFormula;
			if (!flag)
			{
				num5 = i;
			}
		}
		if (num5 == -1)
		{
			serieRange = range;
			return null;
		}
		IRange range3 = (bIsInRow ? range[num, num3, num2, num5] : range[num3, num, num5, num2]);
		serieRange = (bIsInRow ? range[range.Row, range3.LastColumn + 1, range.LastRow, range.LastColumn] : range[range3.LastRow + 1, range.Column, range.LastRow, range.LastColumn]);
		return range3;
	}

	private bool ValidateSerieRangeForChartType(IRange serieValue, OfficeChartType type, bool isSeriesInRows)
	{
		if (serieValue == null)
		{
			throw new ArgumentNullException("serieValue");
		}
		string startSerieType = ChartFormatImpl.GetStartSerieType(type);
		int num = (m_bSeriesInRows ? (serieValue.LastRow - serieValue.Row + 1) : (serieValue.LastColumn - serieValue.Column + 1));
		if (num < 2 && (startSerieType == "Bubble" || startSerieType == "Surface"))
		{
			return false;
		}
		if (type == OfficeChartType.Stock_HighLowClose && num != 3)
		{
			return false;
		}
		if (type == OfficeChartType.Stock_OpenHighLowClose || (type == OfficeChartType.Stock_VolumeHighLowClose && num != 4))
		{
			return false;
		}
		if (type == OfficeChartType.Stock_VolumeOpenHighLowClose && num != 5)
		{
			return false;
		}
		if (startSerieType == "Bubble")
		{
			num = num / 2 + num % 2;
		}
		int count = m_series.Count;
		bool flag = count > num;
		int num2 = (flag ? num : count);
		int num3 = (flag ? count : num);
		for (int i = num2; i < num3; i++)
		{
			if (flag)
			{
				m_series.RemoveAt(num3 - i + num2 - 1);
			}
			else
			{
				m_series.Add();
			}
		}
		return true;
	}

	private bool CompareSeriesValues(Rectangle rec, IRange range, int i, string strSheetName)
	{
		if ((strSheetName == null || strSheetName.Length == 0) && !(range.Worksheet is ExternWorksheetImpl))
		{
			throw new ArgumentNullException("strSheetName");
		}
		if (range == null)
		{
			return false;
		}
		if (range.Worksheet != null && range.Worksheet.Name != strSheetName)
		{
			return false;
		}
		if (!m_bSeriesInRows)
		{
			if (range.Row == rec.Top && range.LastRow == rec.Bottom && range.Column == rec.Left + i)
			{
				return range.LastColumn == rec.Right + i;
			}
			return false;
		}
		if (range.Row == rec.Top + i && range.LastRow == rec.Bottom + i && range.Column == rec.Left)
		{
			return range.LastColumn == rec.Right;
		}
		return false;
	}

	internal IRange GetSeriesValuesRange(IRange lastRange, IRange buble, IWorksheet sheet, string strSheetName)
	{
		if (lastRange == null)
		{
			throw new ArgumentNullException("lastRange");
		}
		if (sheet == null)
		{
			throw new ArgumentNullException("sheet");
		}
		Rectangle rectangeOfRange = RangeImpl.GetRectangeOfRange(lastRange, bThrowExcONNullRange: true);
		bool flag = ChartStartType == "Bubble";
		int count = Series.Count;
		int num = 0;
		if (flag && buble != null)
		{
			if (!CompareSeriesValues(rectangeOfRange, buble, 1, strSheetName))
			{
				return null;
			}
			if (count == 1)
			{
				num++;
			}
		}
		for (int i = 1; i < count; i++)
		{
			ChartSerieImpl chartSerieImpl = (ChartSerieImpl)Series[i];
			lastRange = chartSerieImpl.ValuesIRange;
			if (!CompareSeriesValues(rectangeOfRange, lastRange, (flag && chartSerieImpl.Bubbles != null) ? (i * 2) : i, strSheetName))
			{
				return null;
			}
			if (!flag)
			{
				continue;
			}
			buble = chartSerieImpl.BubblesIRange;
			if (count - i > 1 && !CompareSeriesValues(rectangeOfRange, lastRange, i * 2 + i, strSheetName))
			{
				return null;
			}
			if (count == i && buble != null)
			{
				if (!CompareSeriesValues(rectangeOfRange, lastRange, i * 2 + i, strSheetName))
				{
					return null;
				}
				num++;
			}
		}
		if (!m_bSeriesInRows)
		{
			return sheet[rectangeOfRange.Top, rectangeOfRange.Left, lastRange.LastRow, lastRange.LastColumn + num];
		}
		return sheet[rectangeOfRange.Top, rectangeOfRange.Left, lastRange.LastRow + num, lastRange.LastColumn];
	}

	private bool GetSerieNameValuesRange(IRange lastRange, IRange bubles, IWorksheet sheet, string strSheetName, out IRange result)
	{
		bool flag = lastRange == null;
		Rectangle rec = new Rectangle(0, 0, 0, 0);
		bool flag2 = ChartStartType == "Bubble";
		result = null;
		int count = Series.Count;
		int num = 0;
		if (flag2 && bubles != null && count == 1)
		{
			num++;
		}
		if (!flag)
		{
			rec = RangeImpl.GetRectangeOfRange(lastRange, bThrowExcONNullRange: true);
		}
		for (int i = 1; i < count; i++)
		{
			ChartSerieImpl chartSerieImpl = (ChartSerieImpl)Series[i];
			lastRange = chartSerieImpl.GetSerieNameRange();
			if (flag != (lastRange == null))
			{
				return false;
			}
			if (flag2 && count - i > 1 && chartSerieImpl.BubblesIRange != null)
			{
				num++;
			}
			if (lastRange != null && !CompareSeriesValues(rec, lastRange, (flag2 && chartSerieImpl.Bubbles != null) ? (i * 2) : i, strSheetName))
			{
				return false;
			}
		}
		if (flag)
		{
			return true;
		}
		result = (m_bSeriesInRows ? sheet[rec.Top, rec.Left, lastRange.LastRow + num, lastRange.LastColumn] : sheet[rec.Top, rec.Left, lastRange.LastRow, lastRange.LastColumn + num]);
		return true;
	}

	private bool GetDataRangeRec(IRange range, ref Rectangle rec, bool inRow)
	{
		if (range == null)
		{
			throw new ArgumentNullException("values");
		}
		bool num;
		if (range != null)
		{
			if (!inRow)
			{
				if (rec.Bottom == range.LastRow)
				{
					num = rec.Left == range.Column + 1;
					goto IL_005b;
				}
			}
			else if (rec.Right == range.LastColumn)
			{
				num = rec.Top == range.LastRow + 1;
				goto IL_005b;
			}
			goto IL_005d;
		}
		goto IL_0098;
		IL_005b:
		if (!num)
		{
			goto IL_005d;
		}
		if (inRow)
		{
			rec.Y = range.Row;
			rec.Height++;
		}
		else
		{
			rec.X = range.Column;
			rec.Width++;
		}
		goto IL_0098;
		IL_005d:
		return false;
		IL_0098:
		return true;
	}

	public void DetectIsInRowOnParsing()
	{
		if (m_series.Count == 0)
		{
			m_bSeriesInRows = false;
			return;
		}
		IRange range = (m_series[0].Values as ChartDataRange).Range;
		if (range == null)
		{
			m_bSeriesInRows = false;
		}
		else
		{
			m_bSeriesInRows = DetectIsInRow(range);
		}
	}

	internal void ChangeChartType(OfficeChartType newChartType, bool isSeriesCreation)
	{
		if (ChartType != newChartType)
		{
			if (IsChartStock)
			{
				IsStock = false;
			}
			TypeChanging = true;
			DestinationType = newChartType;
			UpdateChartType(ChartType, newChartType, isSeriesCreation);
			m_chartType = newChartType;
			TypeChanging = false;
		}
	}

	protected override OfficeSheetProtection PrepareProtectionOptions(OfficeSheetProtection options)
	{
		return options |= OfficeSheetProtection.Scenarios;
	}

	private void UpdateChartType(OfficeChartType oldChartType, OfficeChartType newChartType, bool isSeriesCreation)
	{
		if (IsChartExSerieType(newChartType))
		{
			bool flag = !IsChartExSerieType(oldChartType);
			ChangeToChartExType(oldChartType, newChartType, isSeriesCreation);
			foreach (IOfficeChartSerie item in Series)
			{
				ChartSerieImpl chartSerieImpl = item as ChartSerieImpl;
				chartSerieImpl.Number = -1;
				if (flag)
				{
					chartSerieImpl.UpdateChartExSerieRangesMembers(isValues: true);
					chartSerieImpl.UpdateChartExSerieRangesMembers(isValues: false);
					if (chartSerieImpl.SerieFormat.HasInterior)
					{
						(chartSerieImpl.SerieFormat.Interior as ChartInteriorImpl).UseAutomaticFormat = true;
					}
					if (chartSerieImpl.SerieFormat.HasLineProperties)
					{
						chartSerieImpl.SerieFormat.LineProperties.AutoFormat = true;
					}
				}
			}
			ClearDependentStreams();
			return;
		}
		if (IsChartExSerieType(oldChartType))
		{
			int num = 0;
			foreach (IOfficeChartSerie item2 in Series)
			{
				(item2 as ChartSerieImpl).Number = num;
				if (item2.IsFiltered)
				{
					item2.IsFiltered = false;
				}
				num++;
			}
			ClearDependentStreams();
		}
		OnChartTypeChanged(newChartType, isSeriesCreation);
	}

	private void ClearDependentStreams()
	{
		m_defaultTextProperty = null;
		m_colorMapOverrideStream = null;
		if (m_primaryParentAxis.CategoryAxis != null)
		{
			m_primaryParentAxis.CategoryAxis.TextStream = null;
		}
		if (m_primaryParentAxis.ValueAxis != null)
		{
			m_primaryParentAxis.ValueAxis.TextStream = null;
		}
		if (m_secondaryParentAxis.CategoryAxis != null)
		{
			m_secondaryParentAxis.CategoryAxis.TextStream = null;
		}
		if (m_secondaryParentAxis.ValueAxis != null)
		{
			m_secondaryParentAxis.ValueAxis.TextStream = null;
		}
	}

	private new object Clone(object parent)
	{
		return Clone(null, parent, null);
	}

	internal IOfficeChart Clone()
	{
		WorksheetImpl parent = (base.Workbook.Clone() as WorkbookImpl).Worksheets[0] as WorksheetImpl;
		return Clone(parent) as IOfficeChart;
	}

	internal ChartImpl Clone(Dictionary<string, string> hashNewNames, object parent, Dictionary<int, int> dicFontIndexes)
	{
		ChartImpl chartImpl = (ChartImpl)base.Clone(parent);
		chartImpl.SetParent(parent);
		chartImpl.FindParents();
		chartImpl.IsStringRef = IsStringRef;
		if (m_colorMapOverrideDictionary != null)
		{
			chartImpl.m_colorMapOverrideDictionary = new Dictionary<string, string>();
			foreach (KeyValuePair<string, string> item in m_colorMapOverrideDictionary)
			{
				chartImpl.m_colorMapOverrideDictionary.Add(item.Key, item.Value);
			}
		}
		if (_chartData != null)
		{
			chartImpl._chartData = _chartData.Clone(chartImpl);
		}
		chartImpl.m_arrFonts = CloneUtils.CloneCloneable(m_arrFonts);
		if (m_arrFonts != null)
		{
			chartImpl.UpdateChartFbiIndexes(dicFontIndexes);
		}
		if (m_arrRecords != null)
		{
			chartImpl.m_arrRecords = CloneUtils.CloneCloneable(m_arrRecords);
		}
		if (m_chartProperties != null)
		{
			chartImpl.m_chartProperties = (ChartShtpropsRecord)m_chartProperties.Clone();
		}
		if (m_plotArea != null)
		{
			chartImpl.m_plotArea = (ChartPlotAreaImpl)m_plotArea.Clone(chartImpl);
		}
		if (m_dataTable != null)
		{
			chartImpl.m_dataTable = m_dataTable.Clone(chartImpl);
		}
		if (m_chartArea != null)
		{
			chartImpl.m_chartArea = m_chartArea.Clone(chartImpl);
		}
		if (m_lstDefaultText != null)
		{
			chartImpl.m_lstDefaultText = m_lstDefaultText.CloneAll();
			chartImpl.UpdateChartFontXIndexes(dicFontIndexes);
		}
		if (m_pageSetup != null)
		{
			chartImpl.m_pageSetup = m_pageSetup.Clone(chartImpl);
		}
		m_plotAreaBoundingBox = (ChartPosRecord)CloneUtils.CloneCloneable(m_plotAreaBoundingBox);
		if (m_plotAreaFrame != null)
		{
			chartImpl.m_plotAreaFrame = m_plotAreaFrame.Clone(chartImpl);
		}
		m_plotGrowth = (ChartPlotGrowthRecord)CloneUtils.CloneCloneable(m_plotGrowth);
		if (m_series != null)
		{
			chartImpl.m_series = m_series.Clone(chartImpl, hashNewNames, dicFontIndexes);
		}
		if (m_title != null)
		{
			chartImpl.m_title = (ChartTextAreaImpl)m_title.Clone(chartImpl, dicFontIndexes, hashNewNames);
		}
		if (m_primaryParentAxis != null)
		{
			chartImpl.m_primaryParentAxis = m_primaryParentAxis.Clone(chartImpl, dicFontIndexes, hashNewNames);
		}
		if (m_secondaryParentAxis != null)
		{
			chartImpl.m_secondaryParentAxis = m_secondaryParentAxis.Clone(chartImpl, dicFontIndexes, hashNewNames);
		}
		if (m_legend != null)
		{
			chartImpl.m_legend = m_legend.Clone(chartImpl, dicFontIndexes, hashNewNames);
		}
		if (m_walls != null)
		{
			chartImpl.m_walls = (ChartWallOrFloorImpl)m_walls.Clone(chartImpl);
		}
		if (m_themeColors != null)
		{
			chartImpl.m_themeColors = CloneUtils.CloneCloneable(m_themeColors);
		}
		if (m_themeOverrideStream != null)
		{
			m_themeOverrideStream.Position = 0L;
			chartImpl.m_themeOverrideStream = (MemoryStream)CloneUtils.CloneStream(m_themeOverrideStream);
		}
		chartImpl.m_categoriesLabelLevel = m_categoriesLabelLevel;
		chartImpl.m_seriesNameLevel = m_seriesNameLevel;
		if (m_font != null)
		{
			chartImpl.m_font = (FontWrapper)m_font.Clone(chartImpl);
		}
		if (m_defaultTextProperty != null)
		{
			m_defaultTextProperty.Position = 0L;
			chartImpl.m_defaultTextProperty = CloneUtils.CloneStream(m_defaultTextProperty);
		}
		if (m_bandFormats != null)
		{
			m_bandFormats.Position = 0L;
			chartImpl.m_bandFormats = CloneUtils.CloneStream(m_bandFormats);
		}
		if (m_pivotFormatsStream != null)
		{
			m_pivotFormatsStream.Position = 0L;
			chartImpl.m_pivotFormatsStream = CloneUtils.CloneStream(m_pivotFormatsStream);
		}
		if (m_alternateContent != null)
		{
			m_alternateContent.Position = 0L;
			chartImpl.m_alternateContent = CloneUtils.CloneStream(m_alternateContent);
		}
		if (m_relations != null)
		{
			chartImpl.m_relations = m_relations.Clone();
		}
		if (m_trendList != null)
		{
			chartImpl.m_trendList = CloneUtils.CloneCloneable(m_trendList);
		}
		if (m_pivotList != null)
		{
			chartImpl.m_pivotList = CloneUtils.CloneCloneable(m_pivotList);
		}
		if (m_sidewall != null)
		{
			chartImpl.m_sidewall = (ChartWallOrFloorImpl)m_sidewall.Clone(chartImpl);
		}
		if (m_floor != null)
		{
			chartImpl.m_floor = (ChartWallOrFloorImpl)m_floor.Clone(chartImpl);
		}
		if (m_chartChartZoom != null)
		{
			chartImpl.m_chartChartZoom = m_chartChartZoom;
		}
		chartImpl.m_chartType = m_chartType;
		chartImpl.m_pivotChartType = m_pivotChartType;
		if (m_dataRange != null)
		{
			chartImpl.m_dataRange = m_dataRange;
		}
		chartImpl.m_destinationType = m_destinationType;
		if (m_dictReparseErrorBars != null)
		{
			chartImpl.m_dictReparseErrorBars = CloneUtils.CloneHash(m_dictReparseErrorBars);
		}
		if (m_plotAreaLayout != null)
		{
			chartImpl.m_plotAreaLayout = (ChartPlotAreaLayoutRecord)m_plotAreaLayout.Clone();
		}
		if (m_categoryLabelValues != null)
		{
			chartImpl.m_categoryLabelValues = CloneUtils.CloneArray(m_categoryLabelValues);
		}
		chartImpl.m_isChartStyleSkipped = m_isChartStyleSkipped;
		chartImpl.m_isChartColorStyleSkipped = m_isChartColorStyleSkipped;
		if (m_relationPreservedStreamCollections != null && m_relationPreservedStreamCollections.Count > 0)
		{
			chartImpl.m_relationPreservedStreamCollections = new Dictionary<string, Stream>();
			foreach (KeyValuePair<string, Stream> relationPreservedStreamCollection in m_relationPreservedStreamCollections)
			{
				relationPreservedStreamCollection.Value.Position = 0L;
				Stream value = CloneUtils.CloneStream(relationPreservedStreamCollection.Value);
				chartImpl.m_relationPreservedStreamCollections.Add(relationPreservedStreamCollection.Key, value);
			}
		}
		if (CommonDataPointsCollection != null && CommonDataPointsCollection.Count > 0)
		{
			chartImpl.CommonDataPointsCollection = new Dictionary<int, ChartDataPointsCollection>();
			foreach (int key in CommonDataPointsCollection.Keys)
			{
				ChartDataPointsCollection value2 = (ChartDataPointsCollection)CommonDataPointsCollection[key].Clone(chartImpl, chartImpl.m_book, dicFontIndexes, hashNewNames);
				chartImpl.CommonDataPointsCollection.Add(key, value2);
			}
		}
		return chartImpl;
	}

	public void ChangePrimaryAxis(bool isParsing)
	{
		if (!isParsing || (SecondaryFormats.Count != 0 && PrimaryFormats.Count <= 1 && PrimaryFormats.NeedSecondaryAxis))
		{
			PrimaryParentAxis.Formats.ChangeCollections();
		}
	}

	public void UpdateChartFbiIndexes(IDictionary dicFontIndexes)
	{
		if (dicFontIndexes != null && m_arrFonts != null)
		{
			int i = 0;
			for (int count = m_arrFonts.Count; i < count; i++)
			{
				ChartFbiRecord chartFbiRecord = m_arrFonts[i];
				int fontIndex = chartFbiRecord.FontIndex;
				int num = (int)dicFontIndexes[fontIndex];
				chartFbiRecord.FontIndex = (ushort)num;
			}
		}
	}

	public void UpdateChartFontXIndexes(IDictionary dicFontIndexes)
	{
		if (dicFontIndexes == null || m_lstDefaultText == null)
		{
			return;
		}
		int i = 0;
		for (int count = m_lstDefaultText.Count; i < count; i++)
		{
			List<BiffRecordRaw> byIndex = m_lstDefaultText.GetByIndex(i);
			if (byIndex == null)
			{
				continue;
			}
			int j = 0;
			for (int count2 = byIndex.Count; j < count2; j++)
			{
				if (byIndex[j] is ChartFontxRecord chartFontxRecord)
				{
					int fontIndex = chartFontxRecord.FontIndex;
					if (dicFontIndexes.Contains(fontIndex))
					{
						int num = (int)dicFontIndexes[fontIndex];
						chartFontxRecord.FontIndex = (ushort)num;
					}
				}
			}
		}
	}

	public bool CheckForSupportGridLine()
	{
		OfficeChartType chartType = ChartType;
		ChartSeriesCollection chartSeriesCollection = m_series;
		if (chartType == OfficeChartType.Combination_Chart)
		{
			int i = 0;
			for (int count = chartSeriesCollection.Count; i < count; i++)
			{
				IOfficeChartSerie officeChartSerie = chartSeriesCollection[i];
				if (Array.IndexOf(DEF_NOT_SUPPORT_GRIDLINES, officeChartSerie.SerieType) != -1)
				{
					return false;
				}
			}
			return true;
		}
		return Array.IndexOf(DEF_NOT_SUPPORT_GRIDLINES, chartType) == -1;
	}

	internal void CheckIsBubble3D()
	{
		bool flag = ChartType == OfficeChartType.Combination_Chart;
		int num = PrimaryFormats.Count;
		int num2 = (flag ? SecondaryFormats.Count : 0);
		for (int i = 0; i < Series.Count; i++)
		{
			if (num <= 0 && (!flag || num2 <= 0))
			{
				break;
			}
			int chartGroup = (Series[i] as ChartSerieImpl).ChartGroup;
			if (PrimaryFormats.ContainsIndex(chartGroup))
			{
				PrimaryFormats[chartGroup].SerieDataFormat.Is3DBubbles = Series[i].SerieFormat.Is3DBubbles;
				num--;
			}
			else if (flag && SecondaryFormats.ContainsIndex(chartGroup))
			{
				SecondaryFormats[chartGroup].SerieDataFormat.Is3DBubbles = Series[i].SerieFormat.Is3DBubbles;
				num2--;
				if (num2 == 0)
				{
					flag = false;
				}
			}
		}
	}

	public void SetToDefaultGridlines(OfficeChartType type)
	{
		IOfficeChartAxis seriesAxis = m_primaryParentAxis.SeriesAxis;
		if (seriesAxis != null)
		{
			seriesAxis.HasMinorGridLines = false;
			seriesAxis.HasMajorGridLines = false;
		}
		seriesAxis = m_primaryParentAxis.CategoryAxis;
		if (seriesAxis != null)
		{
			seriesAxis.HasMinorGridLines = false;
			seriesAxis.HasMajorGridLines = false;
		}
		seriesAxis = m_primaryParentAxis.ValueAxis;
		if (seriesAxis != null)
		{
			seriesAxis.HasMinorGridLines = false;
			seriesAxis.HasMajorGridLines = false;
		}
		if (Array.IndexOf(DEF_NOT_SUPPORT_GRIDLINES, type) == -1)
		{
			seriesAxis.HasMajorGridLines = true;
		}
	}

	public override void UpdateFormula(int iCurIndex, int iSourceIndex, Rectangle sourceRect, int iDestIndex, Rectangle destRect)
	{
		m_series.UpdateFormula(iCurIndex, iSourceIndex, sourceRect, iDestIndex, destRect);
	}

	public override void MarkUsedReferences(bool[] usedItems)
	{
		if (m_series != null)
		{
			m_series.MarkUsedReferences(usedItems);
		}
		if (m_primaryParentAxis != null)
		{
			m_primaryParentAxis.MarkUsedReferences(usedItems);
		}
		if (m_secondaryParentAxis != null)
		{
			m_secondaryParentAxis.MarkUsedReferences(usedItems);
		}
		if (m_title != null)
		{
			m_title.MarkUsedReferences(usedItems);
		}
	}

	public override void UpdateReferenceIndexes(int[] arrUpdatedIndexes)
	{
		if (m_series != null)
		{
			m_series.UpdateReferenceIndexes(arrUpdatedIndexes);
		}
		if (m_primaryParentAxis != null)
		{
			m_primaryParentAxis.UpdateReferenceIndexes(arrUpdatedIndexes);
		}
		if (m_secondaryParentAxis != null)
		{
			m_secondaryParentAxis.UpdateReferenceIndexes(arrUpdatedIndexes);
		}
		if (m_title != null)
		{
			m_title.UpdateReferenceIndexes(arrUpdatedIndexes);
		}
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

	public void Close()
	{
		if (m_legend != null)
		{
			m_legend.Clear();
			m_legend = null;
		}
		if (_chartData != null)
		{
			_chartData.Clear();
			_chartData = null;
		}
		if (m_plotArea != null)
		{
			m_plotArea.Clear();
			m_plotArea = null;
		}
		if (m_chartArea != null)
		{
			m_chartArea.Clear();
			m_chartArea = null;
		}
		if (m_categories != null)
		{
			m_categories.Clear();
			m_categories = null;
		}
		if (m_series != null)
		{
			m_series.Clear();
			m_series = null;
		}
		if (m_colorMapOverrideDictionary != null)
		{
			m_colorMapOverrideDictionary.Clear();
			m_colorMapOverrideDictionary = null;
		}
		m_relationPreservedStreamCollections = null;
		m_themeOverrideStream = null;
		m_defaultTextProperty = null;
		m_bandFormats = null;
		m_pivotFormatsStream = null;
		m_alternateContent = null;
		base.ParentWorkbook.Close();
	}

	internal void RemoveSecondaryAxes()
	{
		if (IsSecondaryAxes)
		{
			if (IsCategoryAxisAvail)
			{
				m_secondaryParentAxis.RemoveAxis(isCategory: true);
			}
			if (IsValueAxisAvail)
			{
				m_secondaryParentAxis.RemoveAxis(isCategory: false);
			}
		}
	}

	public void SaveAsImage(Stream imageAsstream)
	{
		(base.Application.ChartToImageConverter ?? throw new ArgumentException("IApplication.ChartToImageConverter must be instantiated")).SaveAsImage(this, imageAsstream);
	}

	public static int DoubleToFixedPoint(double value)
	{
		ushort num = (ushort)value;
		double num2 = (value - (double)(int)num) * 100000.0;
		if (num2 > 65535.0)
		{
			num2 /= 10.0;
		}
		byte[] bytes = BitConverter.GetBytes(num);
		byte[] bytes2 = BitConverter.GetBytes((ushort)num2);
		byte[] array = new byte[4];
		bytes.CopyTo(array, 2);
		bytes2.CopyTo(array, 0);
		return BitConverter.ToInt32(array, 0);
	}

	public static double FixedPointToDouble(int value)
	{
		byte[] bytes = BitConverter.GetBytes(value);
		int num = BitConverter.ToUInt16(bytes, 0);
		int value2 = BitConverter.ToUInt16(bytes, 2);
		int num2 = ((num != 0) ? ((int)Math.Log10(num) + 1) : 0);
		return ((double)Math.Abs(value2) + (double)num / Math.Pow(10.0, num2)) * (double)Math.Sign(value2);
	}

	internal ChartSeriesAxisImpl CreatePrimarySeriesAxis()
	{
		return m_primaryParentAxis.SeriesAxis = new ChartSeriesAxisImpl(base.Application, m_primaryParentAxis, OfficeAxisType.Serie);
	}

	internal Color GetChartColor(int index, int totalCount, bool isBinary, bool isColorPalette)
	{
		Color empty = Color.Empty;
		if (isBinary)
		{
			if (isColorPalette)
			{
				int num = 56;
				index %= num;
				if (index < 31)
				{
					return WorkbookImpl.DEF_PALETTE[index + 32];
				}
				index %= 31;
				return WorkbookImpl.DEF_PALETTE[index + 8];
			}
			ChartColor chartColor = null;
			if (index <= 30)
			{
				chartColor = new ChartColor((OfficeKnownColors)(index + 24));
			}
			else
			{
				index -= 30;
				chartColor = new ChartColor((OfficeKnownColors)(index % 55 + 7));
			}
			return chartColor.GetRGB(base.ParentWorkbook);
		}
		int num2 = 6;
		double num3 = -0.5;
		int color_Index = index % num2 + 4;
		empty = GetChartThemeColorByColorIndex(color_Index);
		int num4 = (int)Math.Ceiling((double)totalCount / (double)num2);
		double num5 = 1.0 / (double)num4;
		num5 = ((!(num5 >= 0.5)) ? (num3 + num5 * (double)(index / num2 + 1)) : ((index / num2 <= 0) ? 0.0 : 0.25));
		return Excel2007Parser.ConvertColorByTint(empty, num5);
	}

	internal Color GetChartThemeColorByColorIndex(int color_Index)
	{
		Color empty = Color.Empty;
		if (m_themeOverrideStream == null && m_themeColors == null)
		{
			if ((base.ParentWorkbook.IsCreated && base.ParentWorkbook.Version == OfficeVersion.Excel2013) || (!base.ParentWorkbook.m_isThemeColorsParsed && base.ParentWorkbook.DefaultThemeVersion == "153222"))
			{
				return base.ParentWorkbook.GetThemeColor2013(color_Index);
			}
			return base.ParentWorkbook.GetThemeColor(color_Index);
		}
		if (m_themeColors != null)
		{
			return m_themeColors[color_Index];
		}
		m_themeColors = base.ParentWorkbook.DataHolder.Parser.ParseThemeOverideColors(this);
		if (m_themeColors == null)
		{
			return base.ParentWorkbook.GetThemeColor(color_Index);
		}
		return m_themeColors[color_Index];
	}

	internal static bool TryAndModifyToValidFormula(string value)
	{
		if (value == null || value == "")
		{
			return false;
		}
		bool flag = value.StartsWith("=");
		if (value.Length > (flag ? 1 : 0))
		{
			value = (flag ? value.Substring(1, value.Length - 1) : value);
			if (double.TryParse(value, out var _))
			{
				return false;
			}
			if (value.Contains(" "))
			{
				MatchCollection matchCollection = Regex.Matches(value, "( \\d+)", RegexOptions.None);
				MatchCollection matchCollection2 = Regex.Matches(value, "('[^']*')", RegexOptions.None);
				if (matchCollection.Count == 0)
				{
					return true;
				}
				if (matchCollection2.Count == 0)
				{
					return false;
				}
				for (int i = 0; i < matchCollection.Count; i++)
				{
					for (int j = 0; j < matchCollection2.Count && (matchCollection2[j].Index >= matchCollection[i].Index || matchCollection[i].Index >= matchCollection2[j].Index + matchCollection2[j].Length); j++)
					{
						if (j == matchCollection2.Count - 1)
						{
							return false;
						}
					}
				}
			}
		}
		return true;
	}

	internal StringBuilder GetChartAsString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append((base.HasCodeName ? "1" : "0") + ";");
		stringBuilder.Append((ProtectContents ? "1" : "0") + ";");
		stringBuilder.Append((ProtectDrawingObjects ? "1" : "0") + ";");
		stringBuilder.Append((ProtectScenarios ? "1" : "0") + ";");
		stringBuilder.Append((base.IsPasswordProtected ? "1" : "0") + ";");
		stringBuilder.Append((base.IsParsed ? "1" : "0") + ";");
		stringBuilder.Append((base.IsParsing ? "1" : "0") + ";");
		stringBuilder.Append((base.IsSkipParsing ? "1" : "0") + ";");
		stringBuilder.Append((base.IsSupported ? "1" : "0") + ";");
		stringBuilder.Append((base.DefaultGridlineColor ? "1" : "0") + ";");
		stringBuilder.Append((base.IsRightToLeft ? "1" : "0") + ";");
		stringBuilder.Append((base.IsSelected ? "1" : "0") + ";");
		stringBuilder.Append((base.UnknownVmlShapes ? "1" : "0") + ";");
		stringBuilder.Append((ContainsProtection ? "1" : "0") + ";");
		stringBuilder.Append((base.IsTransitionEvaluation ? "1" : "0") + ";");
		stringBuilder.Append((base.ParseOnDemand ? "1" : "0") + ";");
		stringBuilder.Append((base.HasTabColorRGB ? "1" : "0") + ";");
		stringBuilder.Append((ParseDataOnDemand ? "1" : "0") + ";");
		stringBuilder.Append((HasExternalWorkbook ? "1" : "0") + ";");
		stringBuilder.Append((ShowGapWidth ? "1" : "0") + ";");
		stringBuilder.Append((ShowAllFieldButtons ? "1" : "0") + ";");
		stringBuilder.Append((ShowValueFieldButtons ? "1" : "0") + ";");
		stringBuilder.Append((ShowAxisFieldButtons ? "1" : "0") + ";");
		stringBuilder.Append((ShowLegendFieldButtons ? "1" : "0") + ";");
		stringBuilder.Append((ShowReportFilterFieldButtons ? "1" : "0") + ";");
		if (Series != null && Series.Count != 0)
		{
			stringBuilder.Append((RightAngleAxes ? "1" : "0") + ";");
		}
		if (Series != null && Series.Count != 0)
		{
			stringBuilder.Append((AutoScaling ? "1" : "0") + ";");
		}
		if (Series != null && Series.Count != 0)
		{
			stringBuilder.Append((WallsAndGridlines2D ? "1" : "0") + ";");
		}
		stringBuilder.Append((IsSeriesInRows ? "1" : "0") + ";");
		stringBuilder.Append((IsPrimarySecondaryCategory ? "1" : "0") + ";");
		stringBuilder.Append((IsPrimarySecondaryValue ? "1" : "0") + ";");
		stringBuilder.Append((HasChartArea ? "1" : "0") + ";");
		stringBuilder.Append((HasPlotArea ? "1" : "0") + ";");
		stringBuilder.Append((HasDataTable ? "1" : "0") + ";");
		stringBuilder.Append((HasLegend ? "1" : "0") + ";");
		stringBuilder.Append((HasTitle ? "1" : "0") + ";");
		if (m_chartProperties != null)
		{
			stringBuilder.Append((PlotVisibleOnly ? "1" : "0") + ";");
		}
		stringBuilder.Append((ShowPlotVisible ? "1" : "0") + ";");
		if (m_chartProperties != null)
		{
			stringBuilder.Append((SizeWithWindow ? "1" : "0") + ";");
		}
		if (Series != null && Series.Count != 0)
		{
			stringBuilder.Append((SupportWallsAndFloor ? "1" : "0") + ";");
		}
		stringBuilder.Append((ProtectDrawingObjects ? "1" : "0") + ";");
		stringBuilder.Append((ProtectScenarios ? "1" : "0") + ";");
		stringBuilder.Append((IsChartCleared ? "1" : "0") + ";");
		stringBuilder.Append((ChartTitleIncludeInLayout ? "1" : "0") + ";");
		if (Series != null && Series.Count != 0)
		{
			stringBuilder.Append((IsTreeMapOrSunBurst ? "1" : "0") + ";");
		}
		if (Series != null && Series.Count != 0)
		{
			stringBuilder.Append((IsHistogramOrPareto ? "1" : "0") + ";");
		}
		stringBuilder.Append((IsChartExternalRelation ? "1" : "0") + ";");
		if (Series != null && Series.Count != 0)
		{
			stringBuilder.Append((IsCategoryAxisAvail ? "1" : "0") + ";");
		}
		if (Series != null && Series.Count != 0)
		{
			stringBuilder.Append((IsValueAxisAvail ? "1" : "0") + ";");
		}
		if (Series != null && Series.Count != 0)
		{
			stringBuilder.Append((IsSeriesAxisAvail ? "1" : "0") + ";");
		}
		if (Series != null && Series.Count != 0)
		{
			stringBuilder.Append((IsStacked ? "1" : "0") + ";");
		}
		if (Series != null && Series.Count != 0)
		{
			stringBuilder.Append((IsChart_100 ? "1" : "0") + ";");
		}
		if (Series != null && Series.Count != 0)
		{
			stringBuilder.Append((IsChart3D ? "1" : "0") + ";");
		}
		stringBuilder.Append((IsPivotChart3D ? "1" : "0") + ";");
		if (Series != null && Series.Count != 0)
		{
			stringBuilder.Append((IsChartLine ? "1" : "0") + ";");
		}
		if (Series != null && Series.Count != 0)
		{
			stringBuilder.Append((NeedDataFormat ? "1" : "0") + ";");
		}
		if (Series != null && Series.Count != 0)
		{
			stringBuilder.Append((NeedMarkerFormat ? "1" : "0") + ";");
		}
		if (Series != null && Series.Count != 0)
		{
			stringBuilder.Append((IsChartCone ? "1" : "0") + ";");
		}
		if (Series != null && Series.Count != 0)
		{
			stringBuilder.Append((IsChartBar ? "1" : "0") + ";");
		}
		if (Series != null && Series.Count != 0)
		{
			stringBuilder.Append((IsChartPyramid ? "1" : "0") + ";");
		}
		if (Series != null && Series.Count != 0)
		{
			stringBuilder.Append((IsChartCylinder ? "1" : "0") + ";");
		}
		if (Series != null && Series.Count != 0)
		{
			stringBuilder.Append((IsChartDoughnut ? "1" : "0") + ";");
		}
		if (Series != null && Series.Count != 0)
		{
			stringBuilder.Append((IsChartBubble ? "1" : "0") + ";");
		}
		if (Series != null && Series.Count != 0)
		{
			stringBuilder.Append((IsChartVaryColor ? "1" : "0") + ";");
		}
		if (Series != null && Series.Count != 0)
		{
			stringBuilder.Append((IsChartExploded ? "1" : "0") + ";");
		}
		if (Series != null && Series.Count != 0)
		{
			stringBuilder.Append((IsSeriesLines ? "1" : "0") + ";");
		}
		if (Series != null && Series.Count != 0)
		{
			stringBuilder.Append((CanChartHaveSeriesLines ? "1" : "0") + ";");
		}
		if (Series != null && Series.Count != 0)
		{
			stringBuilder.Append((IsChartScatter ? "1" : "0") + ";");
		}
		if (Series != null && Series.Count != 0)
		{
			stringBuilder.Append((IsChartSmoothedLine ? "1" : "0") + ";");
		}
		if (Series != null && Series.Count != 0)
		{
			stringBuilder.Append((IsChartStock ? "1" : "0") + ";");
		}
		if (Series != null && Series.Count != 0)
		{
			stringBuilder.Append((NeedDropBar ? "1" : "0") + ";");
		}
		if (Series != null && Series.Count != 0)
		{
			stringBuilder.Append((IsChartVolume ? "1" : "0") + ";");
		}
		if (Series != null && Series.Count != 0)
		{
			stringBuilder.Append((IsPerspective ? "1" : "0") + ";");
		}
		if (Series != null && Series.Count != 0)
		{
			stringBuilder.Append((IsClustered ? "1" : "0") + ";");
		}
		if (Series != null && Series.Count != 0)
		{
			stringBuilder.Append((NoPlotArea ? "1" : "0") + ";");
		}
		if (Series != null && Series.Count != 0)
		{
			stringBuilder.Append((IsChartRadar ? "1" : "0") + ";");
		}
		if (Series != null && Series.Count != 0)
		{
			stringBuilder.Append((IsChartPie ? "1" : "0") + ";");
		}
		stringBuilder.Append((IsChartWalls ? "1" : "0") + ";");
		if (Series != null && Series.Count != 0)
		{
			stringBuilder.Append((IsChartFloor ? "1" : "0") + ";");
		}
		if (Series != null && Series.Count != 0)
		{
			stringBuilder.Append((IsSecondaryCategoryAxisAvail ? "1" : "0") + ";");
		}
		if (Series != null && Series.Count != 0)
		{
			stringBuilder.Append((IsSecondaryValueAxisAvail ? "1" : "0") + ";");
		}
		if (Series != null && Series.Count != 0)
		{
			stringBuilder.Append((IsSecondaryAxes ? "1" : "0") + ";");
		}
		if (Series != null && Series.Count != 0)
		{
			stringBuilder.Append((IsSpecialDataLabels ? "1" : "0") + ";");
		}
		if (Series != null && Series.Count != 0)
		{
			stringBuilder.Append((CanChartPercentageLabel ? "1" : "0") + ";");
		}
		if (Series != null && Series.Count != 0)
		{
			stringBuilder.Append((CanChartBubbleLabel ? "1" : "0") + ";");
		}
		if (Series != null && Series.Count != 0)
		{
			stringBuilder.Append((IsManuallyFormatted ? "1" : "0") + ";");
		}
		if (Series != null && Series.Count != 0)
		{
			stringBuilder.Append((Loading ? "1" : "0") + ";");
		}
		stringBuilder.Append((TypeChanging ? "1" : "0") + ";");
		stringBuilder.Append((HasFloor ? "1" : "0") + ";");
		stringBuilder.Append((HasWalls ? "1" : "0") + ";");
		if (m_chartProperties != null)
		{
			stringBuilder.Append((ZoomToFit ? "1" : "0") + ";");
		}
		stringBuilder.Append((IsEmbeded ? "1" : "0") + ";");
		stringBuilder.Append((HasTitleInternal ? "1" : "0") + ";");
		stringBuilder.Append((ParseDataOnDemand ? "1" : "0") + ";");
		stringBuilder.Append(base.Name + ";");
		stringBuilder.Append(base.CodeName + ";");
		stringBuilder.Append(base.AlgorithmName + ";");
		if (m_arrRecords.Count != 0)
		{
			stringBuilder.Append(FirstRow + ";");
		}
		if (m_arrRecords.Count != 0)
		{
			stringBuilder.Append(FirstColumn + ";");
		}
		if (m_arrRecords.Count != 0)
		{
			stringBuilder.Append(LastRow + ";");
		}
		if (m_arrRecords.Count != 0)
		{
			stringBuilder.Append(LastColumn + ";");
		}
		stringBuilder.Append(base.Zoom + ";");
		stringBuilder.Append(base.Index + ";");
		stringBuilder.Append(base.TopVisibleRow + ";");
		stringBuilder.Append(base.LeftVisibleColumn + ";");
		if (m_shapes != null)
		{
			stringBuilder.Append(base.VmlShapesCount + ";");
		}
		stringBuilder.Append(base.SpinCount + ";");
		stringBuilder.Append(base.RealIndex + ";");
		stringBuilder.Append(base.ReferenceCount + ";");
		stringBuilder.Append(OverLap + ";");
		stringBuilder.Append(GapWidth + ";");
		if (Series != null && Series.Count != 0)
		{
			stringBuilder.Append(Rotation + ";");
		}
		if (Series != null && Series.Count != 0)
		{
			stringBuilder.Append(Elevation + ";");
		}
		if (Series != null && Series.Count != 0)
		{
			stringBuilder.Append(Perspective + ";");
		}
		stringBuilder.Append(FormatId + ";");
		stringBuilder.Append(PreservedPivotSource + ";");
		if (Series != null && Series.Count != 0)
		{
			stringBuilder.Append(HeightPercent + ";");
		}
		if (Series != null && Series.Count != 0)
		{
			stringBuilder.Append(DepthPercent + ";");
		}
		if (Series != null && Series.Count != 0)
		{
			stringBuilder.Append(GapDepth + ";");
		}
		if (base.ParentWorkbook != null && m_title != null)
		{
			stringBuilder.Append(ChartTitle + ";");
		}
		if (m_primaryParentAxis != null)
		{
			stringBuilder.Append(CategoryAxisTitle + ";");
		}
		if (m_primaryParentAxis != null)
		{
			stringBuilder.Append(ValueAxisTitle + ";");
		}
		if (m_secondaryParentAxis != null && SecondaryCategoryAxis != null)
		{
			stringBuilder.Append(SecondaryCategoryAxisTitle + ";");
		}
		if (m_secondaryParentAxis != null && SecondaryValueAxis != null)
		{
			stringBuilder.Append(SecondaryValueAxisTitle + ";");
		}
		if (Series != null && Series.Count != 0 && PrimarySerieAxis != null)
		{
			stringBuilder.Append(SeriesAxisTitle + ";");
		}
		stringBuilder.Append(XPos + ";");
		stringBuilder.Append(YPos + ";");
		stringBuilder.Append(Width + ";");
		stringBuilder.Append(Height + ";");
		stringBuilder.Append(EMUHeight + ";");
		stringBuilder.Append(EMUWidth + ";");
		stringBuilder.Append(CategoryFormula + ";");
		stringBuilder.Append(ChartExTitlePosition + ";");
		stringBuilder.Append(ChartExRelationId + ";");
		stringBuilder.Append(LineChartCount + ";");
		stringBuilder.Append(ActiveSheetIndex + ";");
		if (Series != null && Series.Count != 0)
		{
			stringBuilder.Append(ChartStartType + ";");
		}
		stringBuilder.Append(Style + ";");
		stringBuilder.Append(DefaultTextIndex + ";");
		stringBuilder.Append(RadarStyle + ";");
		stringBuilder.Append(EMUHeight + ";");
		stringBuilder.Append(EMUHeight + ";");
		stringBuilder.Append((int)TabColor + ";");
		stringBuilder.Append((int)base.GridLineColor + ";");
		stringBuilder.Append((int)InnerProtection + ";");
		stringBuilder.Append((int)UnprotectedOptions + ";");
		stringBuilder.Append((int)base.Visibility + ";");
		stringBuilder.Append((int)DefaultProtectionOptions + ";");
		stringBuilder.Append((int)PivotChartType + ";");
		if (Series != null && Series.Count != 0)
		{
			stringBuilder.Append((int)ChartType + ";");
		}
		stringBuilder.Append((int)SeriesNameLevel + ";");
		stringBuilder.Append((int)CategoryLabelLevel + ";");
		if (m_chartProperties != null)
		{
			stringBuilder.Append((int)DisplayBlanksAs + ";");
		}
		if (Series != null && Series.Count != 0)
		{
			stringBuilder.Append((int)DefaultLinePattern + ";");
		}
		stringBuilder.Append((int)DestinationType + ";");
		stringBuilder.Append((int)Protection + ";");
		if (base.Workbook != null && DataRange != null)
		{
			stringBuilder.Append(GetDataRangeAsString(DataRange as ChartDataRange));
		}
		if (base.Application != null && ChartTitleArea != null)
		{
			stringBuilder.Append(GetChartTextAreaAsString(ChartTitleArea as ChartTextAreaImpl));
		}
		if (base.Application != null && ChartTitleFont != null)
		{
			stringBuilder.Append(GetChartTextAreaAsString(ChartTitleFont as ChartTextAreaImpl));
		}
		if (m_primaryParentAxis != null && PrimaryCategoryAxis != null)
		{
			stringBuilder.Append(GetChartCategoryAxisAsString(PrimaryCategoryAxis as ChartCategoryAxisImpl));
		}
		if (m_secondaryParentAxis != null && SecondaryCategoryAxis != null)
		{
			stringBuilder.Append(GetChartCategoryAxisAsString(SecondaryCategoryAxis as ChartCategoryAxisImpl));
		}
		if (m_primaryParentAxis != null && PrimaryValueAxis != null)
		{
			stringBuilder.Append(GetChartValueAxisAsString(PrimaryValueAxis as ChartValueAxisImpl));
		}
		if (m_secondaryParentAxis != null && SecondaryValueAxis != null)
		{
			stringBuilder.Append(GetChartValueAxisAsString(SecondaryValueAxis as ChartValueAxisImpl));
		}
		if (Series != null && Series.Count != 0 && PrimarySerieAxis != null)
		{
			stringBuilder.Append(GetChartSeriesAxisAsString(PrimarySerieAxis as ChartSeriesAxisImpl));
		}
		if (Series != null && Series.Count != 0)
		{
			stringBuilder.Append(GetChartSeriesAsString(Series as ChartSeriesCollection));
		}
		if (Categories != null)
		{
			stringBuilder.Append(GetChartCategoriesAsString(Categories as ChartCategoryCollection));
		}
		if (m_primaryParentAxis != null && PrimaryFormats != null)
		{
			stringBuilder.Append(GetChartFormatCollectionAsString(PrimaryFormats));
		}
		if (m_secondaryParentAxis != null && SecondaryFormats != null)
		{
			stringBuilder.Append(GetChartFormatCollectionAsString(SecondaryFormats));
		}
		if (Legend != null)
		{
			stringBuilder.Append(GetChartLegendAsString(Legend as ChartLegendImpl));
		}
		if (base.Application != null && ChartArea != null)
		{
			stringBuilder.Append(GetChartFrameFormatImplAsString(ChartArea as ChartFrameFormatImpl));
		}
		if (PlotArea != null)
		{
			stringBuilder.Append(GetChartFrameFormatAsString(PlotArea as ChartPlotAreaImpl));
		}
		if (m_book != null && m_book.IsWorkbookOpening && Array.IndexOf(DEF_WALLS_OR_FLOOR_TYPES, ChartType) == -1 && Walls != null)
		{
			stringBuilder.Append(GetChartWallOrFloorAsString(Walls as ChartWallOrFloorImpl));
		}
		if (m_book != null && m_book.IsWorkbookOpening && SupportWallsAndFloor && Floor != null)
		{
			stringBuilder.Append(GetChartWallOrFloorAsString(Floor as ChartWallOrFloorImpl));
		}
		if (m_book != null && m_book.IsWorkbookOpening && Array.IndexOf(DEF_WALLS_OR_FLOOR_TYPES, ChartType) == -1 && SideWall != null)
		{
			stringBuilder.Append(GetChartWallOrFloorAsString(SideWall as ChartWallOrFloorImpl));
		}
		if (m_book != null && m_book.IsWorkbookOpening && Array.IndexOf(DEF_WALLS_OR_FLOOR_TYPES, ChartType) == -1 && BackWall != null)
		{
			stringBuilder.Append(GetChartWallOrFloorAsString(BackWall as ChartWallOrFloorImpl));
		}
		if (DataTable != null)
		{
			stringBuilder.Append(GetChartDataTableAsString(DataTable as ChartDataTableImpl));
		}
		if (PlotAreaLayout != null)
		{
			stringBuilder.Append(GetChartPlotAreaLayoutRecordAsString(PlotAreaLayout));
		}
		if (CategoryLabelValues != null)
		{
			stringBuilder.Append(CategoryLabelValues.Count() + ";");
		}
		if (AutoUpdate.HasValue)
		{
			stringBuilder.Append(AutoUpdate.Value ? "1" : "0;");
		}
		foreach (int serializedAxisId in SerializedAxisIds)
		{
			stringBuilder.Append(serializedAxisId + ";");
		}
		if (PlotGrowth != null)
		{
			stringBuilder.Append(GetPlotGrowthRecordAsString(PlotGrowth));
		}
		if (PlotAreaBoundingBox != null)
		{
			stringBuilder.Append(GetChartPosRecordAsString(PlotAreaBoundingBox));
		}
		if (base.Application != null && InnerChartArea != null)
		{
			stringBuilder.Append(GetChartFrameFormatImplAsString(InnerChartArea));
		}
		if (base.Application != null && InnerPlotArea != null)
		{
			stringBuilder.Append(GetChartFrameFormatImplAsString(InnerPlotArea));
		}
		if (ChartProperties != null)
		{
			stringBuilder.Append(GetChartShtpropsRecordAsString(ChartProperties));
		}
		if (Series != null && Series.Count != 0 && ChartFormat != null)
		{
			stringBuilder.Append(GetChartFormatAsString(ChartFormat));
		}
		if (base.ParentWorkbook != null && Font != null)
		{
			stringBuilder.Append(GetFontWrapperAsString(Font as FontWrapper));
		}
		if (HasAutoTitle.HasValue)
		{
			stringBuilder.Append(HasAutoTitle.Value ? "1" : "0;");
		}
		return stringBuilder;
	}

	internal StringBuilder GetBiffStorageAsString(IBiffStorage biffStorage)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append((biffStorage.NeedDataArray ? "1" : "0") + ";");
		stringBuilder.Append(biffStorage.RecordCode + ";");
		stringBuilder.Append(biffStorage.StreamPos + ";");
		stringBuilder.Append(biffStorage.TypeCode.ToString() + ";");
		return stringBuilder;
	}

	internal StringBuilder GetChartSerieAsString(ChartSerieImpl serieImpl)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append((serieImpl.Reversed ? "1" : "0") + ";");
		stringBuilder.Append((serieImpl.IsValidValueRange ? "1" : "0") + ";");
		stringBuilder.Append((serieImpl.IsValidCategoryRange ? "1" : "0") + ";");
		stringBuilder.Append((serieImpl.UsePrimaryAxis ? "1" : "0") + ";");
		stringBuilder.Append((serieImpl.HasColumnShape ? "1" : "0") + ";");
		stringBuilder.Append((serieImpl.HasErrorBarsY ? "1" : "0") + ";");
		stringBuilder.Append((serieImpl.HasErrorBarsX ? "1" : "0") + ";");
		stringBuilder.Append((serieImpl.ShowGapWidth ? "1" : "0") + ";");
		stringBuilder.Append((serieImpl.HasLeaderLines ? "1" : "0") + ";");
		stringBuilder.Append((serieImpl.IsFiltered ? "1" : "0") + ";");
		stringBuilder.Append((serieImpl.IsDefaultName ? "1" : "0") + ";");
		stringBuilder.Append((serieImpl.IsPie ? "1" : "0") + ";");
		stringBuilder.Append((serieImpl.InvertIfNegative ? "1" : "0") + ";");
		stringBuilder.Append((serieImpl.IsParetoLineHidden ? "1" : "0") + ";");
		stringBuilder.Append((serieImpl.IsSeriesHidden ? "1" : "0") + ";");
		stringBuilder.Append((serieImpl.IsRowWiseCategory ? "1" : "0") + ";");
		stringBuilder.Append((serieImpl.IsRowWiseSeries ? "1" : "0") + ";");
		stringBuilder.Append((int)serieImpl.SerieType + ";");
		stringBuilder.Append(serieImpl.SerieName + ";");
		stringBuilder.Append(serieImpl.ExistingOrder + ";");
		stringBuilder.Append(serieImpl.Name + ";");
		stringBuilder.Append(serieImpl.RealIndex + ";");
		stringBuilder.Append(serieImpl.Grouping + ";");
		stringBuilder.Append(serieImpl.GapWidth + ";");
		stringBuilder.Append(serieImpl.Overlap + ";");
		stringBuilder.Append(serieImpl.FormatCode + ";");
		stringBuilder.Append(serieImpl.PointCount + ";");
		stringBuilder.Append(serieImpl.Index + ";");
		stringBuilder.Append(serieImpl.Number + ";");
		stringBuilder.Append(serieImpl.ChartGroup + ";");
		stringBuilder.Append(serieImpl.PointNumber + ";");
		stringBuilder.Append(serieImpl.FilteredCategory + ";");
		stringBuilder.Append(serieImpl.FilteredValue + ";");
		stringBuilder.Append(serieImpl.StartType + ";");
		stringBuilder.Append(serieImpl.ParseSerieNotDefaultText + ";");
		stringBuilder.Append(serieImpl.NameOrFormula + ";");
		stringBuilder.Append(serieImpl.StrRefFormula + ";");
		stringBuilder.Append(serieImpl.NumRefFormula + ";");
		stringBuilder.Append(serieImpl.MulLvlStrRefFormula + ";");
		stringBuilder.Append(serieImpl.ParetoLineFormatIndex + ";");
		stringBuilder.Append(serieImpl.CategoriesFormatCode + ";");
		return stringBuilder;
	}

	internal StringBuilder GetChartLegendEntryImplAsString(ChartLegendEntryImpl entryImpl)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append((entryImpl.IsFormatted ? "1" : "0") + ";");
		stringBuilder.Append((entryImpl.IsDeleted ? "1" : "0") + ";");
		stringBuilder.Append(entryImpl.LegendEntityIndex + ";");
		stringBuilder.Append(entryImpl.Index + ";");
		if (entryImpl.TextArea != null)
		{
			stringBuilder.Append(GetChartTextAreaAsString(entryImpl.TextArea as ChartTextAreaImpl));
		}
		return stringBuilder;
	}

	internal StringBuilder GetChartLegendRecordAsString(ChartLegendRecord legendRecord)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append((legendRecord.NeedInfill ? "1" : "0") + ";");
		stringBuilder.Append((legendRecord.NeedDataArray ? "1" : "0") + ";");
		stringBuilder.Append((legendRecord.IsAllowShortData ? "1" : "0") + ";");
		stringBuilder.Append((legendRecord.NeedDecoding ? "1" : "0") + ";");
		stringBuilder.Append((int)legendRecord.TypeCode + ";");
		stringBuilder.Append(legendRecord.Data?.ToString() + ";");
		stringBuilder.Append(legendRecord.RecordCode + ";");
		stringBuilder.Append(legendRecord.Length + ";");
		stringBuilder.Append(legendRecord.StreamPos + ";");
		stringBuilder.Append(legendRecord.MinimumRecordSize + ";");
		stringBuilder.Append(legendRecord.MaximumRecordSize + ";");
		stringBuilder.Append(legendRecord.MaximumMemorySize + ";");
		stringBuilder.Append(legendRecord.StartDecodingOffset + ";");
		stringBuilder.Append((legendRecord.AutoPosition ? "1" : "0") + ";");
		stringBuilder.Append((legendRecord.AutoSeries ? "1" : "0") + ";");
		stringBuilder.Append((legendRecord.AutoPositionX ? "1" : "0") + ";");
		stringBuilder.Append((legendRecord.AutoPositionY ? "1" : "0") + ";");
		stringBuilder.Append((legendRecord.IsVerticalLegend ? "1" : "0") + ";");
		stringBuilder.Append(legendRecord.ContainsDataTable ? "1" : "0;");
		stringBuilder.Append(legendRecord.X + ";");
		stringBuilder.Append(legendRecord.Y + ";");
		stringBuilder.Append(legendRecord.Width + ";");
		stringBuilder.Append(legendRecord.Height + ";");
		stringBuilder.Append((int)legendRecord.Position + ";");
		stringBuilder.Append((int)legendRecord.Spacing + ";");
		return stringBuilder;
	}

	internal StringBuilder GetChartAttachedLabelLayoutRecordAsString(ChartAttachedLabelLayoutRecord layoutRecord)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append((layoutRecord.NeedInfill ? "1" : "0") + ";");
		stringBuilder.Append((layoutRecord.NeedDataArray ? "1" : "0") + ";");
		stringBuilder.Append((layoutRecord.IsAllowShortData ? "1" : "0") + ";");
		stringBuilder.Append((layoutRecord.NeedDecoding ? "1" : "0") + ";");
		stringBuilder.Append((int)layoutRecord.TypeCode + ";");
		stringBuilder.Append(layoutRecord.Data?.ToString() + ";");
		stringBuilder.Append(layoutRecord.RecordCode + ";");
		stringBuilder.Append(layoutRecord.Length + ";");
		stringBuilder.Append(layoutRecord.StreamPos + ";");
		stringBuilder.Append(layoutRecord.MinimumRecordSize + ";");
		stringBuilder.Append(layoutRecord.MaximumRecordSize + ";");
		stringBuilder.Append(layoutRecord.MaximumMemorySize + ";");
		stringBuilder.Append(layoutRecord.StartDecodingOffset + ";");
		stringBuilder.Append(layoutRecord.X + ";");
		stringBuilder.Append(layoutRecord.Y + ";");
		stringBuilder.Append(layoutRecord.Dx + ";");
		stringBuilder.Append(layoutRecord.Dy + ";");
		stringBuilder.Append((int)layoutRecord.WXMode + ";");
		stringBuilder.Append((int)layoutRecord.WYMode + ";");
		stringBuilder.Append((int)layoutRecord.WWidthMode + ";");
		stringBuilder.Append((int)layoutRecord.WHeightMode + ";");
		return stringBuilder;
	}

	internal StringBuilder GetChartManualLayoutAsString(ChartManualLayoutImpl layoutImpl)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append((int)layoutImpl.LayoutTarget + ";");
		stringBuilder.Append((int)layoutImpl.LeftMode + ";");
		stringBuilder.Append((int)layoutImpl.TopMode + ";");
		stringBuilder.Append((int)layoutImpl.WidthMode + ";");
		stringBuilder.Append((int)layoutImpl.HeightMode + " ;");
		stringBuilder.Append(layoutImpl.Left + ";");
		stringBuilder.Append(layoutImpl.Top + " ;");
		stringBuilder.Append(layoutImpl.Width + " ;");
		stringBuilder.Append(layoutImpl.Height + " ;");
		stringBuilder.Append(layoutImpl.dX + " ;");
		stringBuilder.Append(layoutImpl.dY + " ;");
		stringBuilder.Append(layoutImpl.xTL + " ;");
		stringBuilder.Append(layoutImpl.yTL + " ;");
		stringBuilder.Append(layoutImpl.xBR + " ;");
		stringBuilder.Append(layoutImpl.yBR + " ;");
		stringBuilder.Append(layoutImpl.FlagOptions + " ;");
		return stringBuilder;
	}

	internal StringBuilder GetChartLegendEntriesAsString(ChartLegendEntriesColl entriesColl)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(entriesColl.Count + ";");
		if (entriesColl.HashEntries != null)
		{
			foreach (KeyValuePair<int, ChartLegendEntryImpl> hashEntry in entriesColl.HashEntries)
			{
				stringBuilder.Append(GetChartLegendEntryImplAsString(hashEntry.Value));
			}
		}
		return stringBuilder;
	}

	internal StringBuilder GetChartPosRecordAsString(ChartPosRecord posRecord)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append((posRecord.NeedInfill ? "1" : "0") + ";");
		stringBuilder.Append((posRecord.NeedDataArray ? "1" : "0") + ";");
		stringBuilder.Append((posRecord.IsAllowShortData ? "1" : "0") + ";");
		stringBuilder.Append((posRecord.NeedDecoding ? "1" : "0") + ";");
		stringBuilder.Append((int)posRecord.TypeCode + ";");
		stringBuilder.Append(posRecord.RecordCode + ";");
		stringBuilder.Append(posRecord.Length + ";");
		stringBuilder.Append(posRecord.StreamPos + ";");
		stringBuilder.Append(posRecord.MinimumRecordSize + ";");
		stringBuilder.Append(posRecord.MaximumRecordSize + ";");
		stringBuilder.Append(posRecord.MaximumMemorySize + ";");
		stringBuilder.Append(posRecord.StartDecodingOffset + ";");
		stringBuilder.Append(posRecord.TopLeft + ";");
		stringBuilder.Append(posRecord.BottomRight + ";");
		stringBuilder.Append(posRecord.X1 + ";");
		stringBuilder.Append(posRecord.X2 + ";");
		stringBuilder.Append(posRecord.Y1 + ";");
		stringBuilder.Append(posRecord.Y2 + ";");
		stringBuilder.Append(posRecord.MinimumRecordSize + ";");
		stringBuilder.Append(posRecord.MaximumRecordSize + ";");
		stringBuilder.Append(posRecord.Data?.ToString() + ";");
		return stringBuilder;
	}

	internal StringBuilder GetChartMarkerFormatRecordAsString(ChartMarkerFormatRecord formatRecord)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append((formatRecord.IsAutoColor ? "1" : "0") + ";");
		stringBuilder.Append((formatRecord.IsNotShowInt ? "1" : "0") + ";");
		stringBuilder.Append((formatRecord.IsNotShowBrd ? "1" : "0") + ";");
		stringBuilder.Append((formatRecord.HasLineProperties ? "1" : "0") + ";");
		stringBuilder.Append(formatRecord.ForeColor + ";");
		stringBuilder.Append(formatRecord.BackColor + ";");
		stringBuilder.Append(formatRecord.Options + ";");
		stringBuilder.Append(formatRecord.BorderColorIndex + ";");
		stringBuilder.Append(formatRecord.FillColorIndex + ";");
		stringBuilder.Append(formatRecord.LineSize + ";");
		stringBuilder.Append(formatRecord.MinimumRecordSize + ";");
		stringBuilder.Append(formatRecord.MaximumRecordSize + ";");
		stringBuilder.Append(formatRecord.FlagOptions + ";");
		stringBuilder.Append((int)formatRecord.MarkerType + ";");
		return stringBuilder;
	}

	internal StringBuilder GetGradientStopsAsString(GradientStops gradientStops)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append((gradientStops.IsDoubled ? "1" : "0") + ";");
		stringBuilder.Append(gradientStops.Angle + ";");
		stringBuilder.Append((int)gradientStops.GradientType + ";");
		_ = gradientStops.FillToRect;
		stringBuilder.Append(GetRectangleAsString(gradientStops.FillToRect));
		_ = gradientStops.TileRect;
		stringBuilder.Append(GetRectangleAsString(gradientStops.TileRect));
		return stringBuilder;
	}

	internal StringBuilder GetRectangleAsString(Rectangle rect)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(rect.X + ";");
		stringBuilder.Append(rect.Y + ";");
		stringBuilder.Append(rect.Width + ";");
		stringBuilder.Append(rect.Height + ";");
		return stringBuilder;
	}

	internal StringBuilder GetChartInteriorAsString(ChartInteriorImpl interiorImpl)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append((interiorImpl.UseAutomaticFormat ? "1" : "0") + ";");
		stringBuilder.Append((interiorImpl.SwapColorsOnNegative ? "1" : "0") + ";");
		stringBuilder.Append((int)interiorImpl.Pattern + ";");
		stringBuilder.Append((int)interiorImpl.ForegroundColorIndex + ";");
		stringBuilder.Append((int)interiorImpl.BackgroundColorIndex + ";");
		if (interiorImpl.ForegroundColorObject != null)
		{
			stringBuilder.Append(GetChartColorAsString(interiorImpl.ForegroundColorObject));
		}
		if (interiorImpl.BackgroundColorObject != null)
		{
			stringBuilder.Append(GetChartColorAsString(interiorImpl.BackgroundColorObject));
		}
		_ = interiorImpl.ForegroundColor;
		stringBuilder.Append(interiorImpl.ForegroundColor.ToArgb() + ";");
		_ = interiorImpl.BackgroundColor;
		stringBuilder.Append(interiorImpl.BackgroundColor.ToArgb() + ";");
		return stringBuilder;
	}

	internal StringBuilder GetShadowAsString(ShadowImpl shadowImpl)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append((shadowImpl.HasCustomShadowStyle ? "1" : "0") + ";");
		stringBuilder.Append((int)shadowImpl.ShadowOuterPresets + ";");
		stringBuilder.Append((int)shadowImpl.ShadowInnerPresets + ";");
		stringBuilder.Append((int)shadowImpl.ShadowPerspectivePresets + ";");
		stringBuilder.Append(shadowImpl.Transparency + ";");
		stringBuilder.Append(shadowImpl.Size + ";");
		stringBuilder.Append(shadowImpl.Blur + ";");
		stringBuilder.Append(shadowImpl.Angle + ";");
		stringBuilder.Append(shadowImpl.Distance + ";");
		if (shadowImpl.ShadowFormat != null)
		{
			stringBuilder.Append(GetChartMarkerFormatRecordAsString(shadowImpl.ShadowFormat));
		}
		if (shadowImpl.ColorObject != null)
		{
			stringBuilder.Append(GetChartColorAsString(shadowImpl.ColorObject));
		}
		_ = shadowImpl.ShadowColor;
		stringBuilder.Append(shadowImpl.ShadowColor.ToArgb() + ";");
		return stringBuilder;
	}

	internal StringBuilder GetThreeDFormatAsString(ThreeDFormatImpl formatImpl)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append((formatImpl.IsBevelTopWidthSet ? "1" : "0") + ";");
		stringBuilder.Append((formatImpl.IsBevelBottomWidthSet ? "1" : "0") + ";");
		stringBuilder.Append((formatImpl.IsBevelTopHeightSet ? "1" : "0") + ";");
		stringBuilder.Append((formatImpl.IsBevelBottomHeightSet ? "1" : "0") + ";");
		stringBuilder.Append((int)formatImpl.BevelTop + ";");
		stringBuilder.Append((int)formatImpl.BevelBottom + ";");
		stringBuilder.Append((int)formatImpl.Material + ";");
		stringBuilder.Append((int)formatImpl.Lighting + ";");
		stringBuilder.Append(formatImpl.BevelTopHeight + ";");
		stringBuilder.Append(formatImpl.BevelBottomHeight + ";");
		stringBuilder.Append(formatImpl.BevelBottomWidth + ";");
		stringBuilder.Append(formatImpl.PresetShape + ";");
		return stringBuilder;
	}

	internal StringBuilder GetFillAsString(ShapeFillImpl fillImpl)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append((fillImpl.Tile ? "1" : "0") + ";");
		stringBuilder.Append((fillImpl.IsGradientSupported ? "1" : "0") + ";");
		stringBuilder.Append((fillImpl.Visible ? "1" : "0") + ";");
		stringBuilder.Append((int)fillImpl.FillType + ";");
		if (fillImpl.FillType == OfficeFillType.Gradient)
		{
			stringBuilder.Append((int)fillImpl.GradientStyle + ";");
			stringBuilder.Append((int)fillImpl.GradientVariant + ";");
			stringBuilder.Append((int)fillImpl.GradientColorType + ";");
			stringBuilder.Append(fillImpl.GradientDegree + ";");
			stringBuilder.Append((int)fillImpl.PresetGradientType + ";");
			stringBuilder.Append(fillImpl.TransparencyTo + ";");
		}
		if (fillImpl.FillType == OfficeFillType.Pattern)
		{
			stringBuilder.Append((int)fillImpl.Pattern + ";");
		}
		if (fillImpl.FillType == OfficeFillType.Texture)
		{
			stringBuilder.Append((int)fillImpl.Texture + ";");
		}
		stringBuilder.Append((int)fillImpl.BackColorIndex + ";");
		stringBuilder.Append((int)fillImpl.ForeColorIndex + ";");
		stringBuilder.Append(fillImpl.TransparencyColor + ";");
		if (fillImpl.FillType == OfficeFillType.SolidColor)
		{
			stringBuilder.Append(fillImpl.Transparency + ";");
		}
		if (fillImpl.FillType == OfficeFillType.Texture && fillImpl.m_gradTexture == OfficeTexture.User_Defined && fillImpl.FillType == OfficeFillType.Picture)
		{
			stringBuilder.Append(fillImpl.PictureName + ";");
		}
		stringBuilder.Append(fillImpl.TextureVerticalScale + ";");
		stringBuilder.Append(fillImpl.TextureHorizontalScale + ";");
		stringBuilder.Append(fillImpl.TextureOffsetX + ";");
		stringBuilder.Append(fillImpl.TextureOffsetY + ";");
		stringBuilder.Append(fillImpl.Alignment + ";");
		stringBuilder.Append(fillImpl.TileFlipping + ";");
		if (fillImpl.GradientStops != null)
		{
			stringBuilder.Append(GetGradientStopsAsString(fillImpl.GradientStops));
		}
		if (fillImpl.PreservedGradient != null)
		{
			stringBuilder.Append(GetGradientStopsAsString(fillImpl.PreservedGradient));
		}
		_ = fillImpl.FillRect;
		stringBuilder.Append(GetRectangleAsString(fillImpl.FillRect));
		_ = fillImpl.SourceRect;
		stringBuilder.Append(GetRectangleAsString(fillImpl.SourceRect));
		_ = fillImpl.BackColor;
		stringBuilder.Append(fillImpl.BackColor.ToArgb() + ";");
		_ = fillImpl.ForeColor;
		stringBuilder.Append(fillImpl.ForeColor.ToArgb() + ";");
		if (fillImpl.BackColorObject != null)
		{
			stringBuilder.Append(GetChartColorAsString(fillImpl.BackColorObject));
		}
		if (fillImpl.ForeColorObject != null)
		{
			stringBuilder.Append(GetChartColorAsString(fillImpl.ForeColorObject));
		}
		return stringBuilder;
	}

	internal StringBuilder GetChartSeriesAsString(ChartSeriesCollection chartSeriesCollection)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append((chartSeriesCollection.QuietMode ? "1" : "0") + ";");
		stringBuilder.Append((chartSeriesCollection.IsReadOnly ? "1" : "0") + ";");
		stringBuilder.Append(chartSeriesCollection.Capacity + ";");
		stringBuilder.Append(chartSeriesCollection.Count + ";");
		stringBuilder.Append(chartSeriesCollection.TrendErrorBarIndex + ";");
		stringBuilder.Append(chartSeriesCollection.TrendIndex + ";");
		if (chartSeriesCollection.AdditionOrder != null && chartSeriesCollection.AdditionOrder.Count > 0)
		{
			foreach (IOfficeChartSerie item in chartSeriesCollection.AdditionOrder)
			{
				stringBuilder.Append(GetChartSerieAsString(item as ChartSerieImpl));
			}
		}
		if (chartSeriesCollection.TrendErrorList != null && chartSeriesCollection.TrendErrorList.Count > 0)
		{
			foreach (IBiffStorage trendError in chartSeriesCollection.TrendErrorList)
			{
				stringBuilder.Append(GetBiffStorageAsString(trendError));
			}
		}
		if (chartSeriesCollection.TrendLabels != null && chartSeriesCollection.TrendLabels.Count > 0)
		{
			foreach (IBiffStorage trendLabel in chartSeriesCollection.TrendLabels)
			{
				stringBuilder.Append(GetBiffStorageAsString(trendLabel));
			}
		}
		return stringBuilder;
	}

	internal StringBuilder GetChartCategoriesAsString(ChartCategoryCollection categoriesCollection)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append((categoriesCollection.QuietMode ? "1" : "0") + ";");
		stringBuilder.Append((categoriesCollection.IsReadOnly ? "1" : "0") + ";");
		stringBuilder.Append(categoriesCollection.Capacity + ";");
		stringBuilder.Append(categoriesCollection.Count + ";");
		return stringBuilder;
	}

	internal StringBuilder GetChartDataTableAsString(ChartDataTableImpl dataTableImpl)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append((dataTableImpl.HasHorzBorder ? "1" : "0") + ";");
		stringBuilder.Append((dataTableImpl.HasVertBorder ? "1" : "0") + ";");
		stringBuilder.Append((dataTableImpl.HasBorders ? "1" : "0") + ";");
		stringBuilder.Append((dataTableImpl.ShowSeriesKeys ? "1" : "0") + ";");
		if (dataTableImpl.TextArea != null)
		{
			stringBuilder.Append(GetChartTextAreaAsString(dataTableImpl.TextArea as ChartTextAreaImpl));
		}
		return stringBuilder;
	}

	internal StringBuilder GetChartWallOrFloorAsString(ChartWallOrFloorImpl wallOrFloorImpl)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append((wallOrFloorImpl.HasLineProperties ? "1" : "0") + ";");
		stringBuilder.Append((wallOrFloorImpl.HasShadowProperties ? "1" : "0") + ";");
		stringBuilder.Append((wallOrFloorImpl.Has3dProperties ? "1" : "0") + ";");
		stringBuilder.Append((wallOrFloorImpl.HasInterior ? "1" : "0") + ";");
		stringBuilder.Append((wallOrFloorImpl.HasShapeProperties ? "1" : "0") + ";");
		stringBuilder.Append((wallOrFloorImpl.IsAutomaticFormat ? "1" : "0") + ";");
		stringBuilder.Append((wallOrFloorImpl.Visible ? "1" : "0") + ";");
		stringBuilder.Append(wallOrFloorImpl.Thickness + ";");
		stringBuilder.Append((int)wallOrFloorImpl.PictureUnit + ";");
		stringBuilder.Append((int)wallOrFloorImpl.Pattern + ";");
		stringBuilder.Append((int)wallOrFloorImpl.AxisLineType + ";");
		if (wallOrFloorImpl.Interior != null)
		{
			stringBuilder.Append(GetChartInteriorAsString(wallOrFloorImpl.Interior as ChartInteriorImpl));
		}
		if (wallOrFloorImpl.ThreeD != null)
		{
			stringBuilder.Append(GetThreeDFormatAsString(wallOrFloorImpl.ThreeD as ThreeDFormatImpl));
		}
		if (wallOrFloorImpl.Shadow != null)
		{
			stringBuilder.Append(GetShadowAsString(wallOrFloorImpl.Shadow as ShadowImpl));
		}
		if (wallOrFloorImpl.Fill != null)
		{
			stringBuilder.Append(GetFillAsString(wallOrFloorImpl.Fill as ShapeFillImpl));
		}
		if (wallOrFloorImpl.ForeGroundColorObject != null)
		{
			stringBuilder.Append(GetChartColorAsString(wallOrFloorImpl.ForeGroundColorObject));
		}
		if (wallOrFloorImpl.BackGroundColorObject != null)
		{
			stringBuilder.Append(GetChartColorAsString(wallOrFloorImpl.BackGroundColorObject));
		}
		return stringBuilder;
	}

	internal StringBuilder GetChartFrameFormatAsString(ChartPlotAreaImpl chartPlotArea)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append((chartPlotArea.HasInterior ? "1" : "0") + ";");
		stringBuilder.Append((chartPlotArea.HasLineProperties ? "1" : "0") + ";");
		stringBuilder.Append((chartPlotArea.HasShadowProperties ? "1" : "0") + ";");
		stringBuilder.Append((chartPlotArea.Has3dProperties ? "1" : "0") + ";");
		stringBuilder.Append((chartPlotArea.IsAutoSize ? "1" : "0") + ";");
		stringBuilder.Append((chartPlotArea.IsAutoPosition ? "1" : "0") + ";");
		stringBuilder.Append((chartPlotArea.IsBorderCornersRound ? "1" : "0") + ";");
		stringBuilder.Append((chartPlotArea.IsAutomaticFormat ? "1" : "0") + ";");
		stringBuilder.Append((chartPlotArea.Visible ? "1" : "0") + ";");
		stringBuilder.Append((int)chartPlotArea.RectangleStyle + ";");
		stringBuilder.Append((int)chartPlotArea.Pattern + ";");
		if (chartPlotArea.Layout != null)
		{
			stringBuilder.Append(GetChartLayoutAsString(chartPlotArea.Layout as ChartLayoutImpl));
		}
		if (chartPlotArea.Border != null)
		{
			stringBuilder.Append(GetChartBorderAsString(chartPlotArea.Border as ChartBorderImpl));
		}
		if (chartPlotArea.Interior != null)
		{
			stringBuilder.Append(GetChartInteriorAsString(chartPlotArea.Interior as ChartInteriorImpl));
		}
		if (chartPlotArea.ThreeD != null)
		{
			stringBuilder.Append(GetThreeDFormatAsString(chartPlotArea.ThreeD as ThreeDFormatImpl));
		}
		if (chartPlotArea.Shadow != null)
		{
			stringBuilder.Append(GetShadowAsString(chartPlotArea.Shadow as ShadowImpl));
		}
		if (chartPlotArea.Fill != null)
		{
			stringBuilder.Append(GetFillAsString(chartPlotArea.Fill as ShapeFillImpl));
		}
		if (chartPlotArea.LineProperties != null)
		{
			stringBuilder.Append(GetChartBorderAsString(chartPlotArea.LineProperties as ChartBorderImpl));
		}
		if (chartPlotArea.ForeGroundColorObject != null)
		{
			stringBuilder.Append(GetChartColorAsString(chartPlotArea.ForeGroundColorObject));
		}
		if (chartPlotArea.BackGroundColorObject != null)
		{
			stringBuilder.Append(GetChartColorAsString(chartPlotArea.BackGroundColorObject));
		}
		return stringBuilder;
	}

	internal StringBuilder GetChartTextAreaAsString(ChartTextAreaImpl chartTextArea)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append((chartTextArea.Bold ? "1" : "0") + ";");
		stringBuilder.Append((chartTextArea.Italic ? "1" : "0") + ";");
		stringBuilder.Append((chartTextArea.MacOSOutlineFont ? "1" : "0") + ";");
		stringBuilder.Append((chartTextArea.MacOSShadow ? "1" : "0") + ";");
		stringBuilder.Append((chartTextArea.Strikethrough ? "1" : "0") + ";");
		stringBuilder.Append((chartTextArea.Subscript ? "1" : "0") + ";");
		stringBuilder.Append((chartTextArea.Superscript ? "1" : "0") + ";");
		stringBuilder.Append((chartTextArea.IsBaselineWithPercentage ? "1" : "0") + ";");
		stringBuilder.Append((chartTextArea.HasTextRotation ? "1" : "0") + ";");
		stringBuilder.Append((chartTextArea.ContainDataLabels ? "1" : "0") + ";");
		stringBuilder.Append((chartTextArea.IsAutoMode ? "1" : "0") + ";");
		stringBuilder.Append((chartTextArea.IsTrend ? "1" : "0") + ";");
		stringBuilder.Append((chartTextArea.IsAutoColor ? "1" : "0") + ";");
		stringBuilder.Append((chartTextArea.Overlay ? "1" : "0") + ";");
		stringBuilder.Append((chartTextArea.ShowTextProperties ? "1" : "0") + ";");
		stringBuilder.Append((chartTextArea.ShowSizeProperties ? "1" : "0") + ";");
		stringBuilder.Append((chartTextArea.ShowBoldProperties ? "1" : "0") + ";");
		stringBuilder.Append((chartTextArea.HasText ? "1" : "0") + ";");
		stringBuilder.Append((chartTextArea.IsFormula ? "1" : "0") + ";");
		stringBuilder.Append((chartTextArea.IsTextParsed ? "1" : "0") + ";");
		stringBuilder.Append((chartTextArea.IsValueFromCells ? "1" : "0") + ";");
		stringBuilder.Append((chartTextArea.IsSeriesName ? "1" : "0") + ";");
		stringBuilder.Append((chartTextArea.IsCategoryName ? "1" : "0") + ";");
		stringBuilder.Append((chartTextArea.IsValue ? "1" : "0") + ";");
		stringBuilder.Append((chartTextArea.IsPercentage ? "1" : "0") + ";");
		stringBuilder.Append((chartTextArea.IsBubbleSize ? "1" : "0") + ";");
		stringBuilder.Append((chartTextArea.IsCategoryName ? "1" : "0") + ";");
		stringBuilder.Append((chartTextArea.IsLegendKey ? "1" : "0") + ";");
		stringBuilder.Append((chartTextArea.IsShowLabelPercent ? "1" : "0") + ";");
		stringBuilder.Append((int)chartTextArea.Color + ";");
		stringBuilder.Append((int)chartTextArea.Underline + ";");
		stringBuilder.Append((int)chartTextArea.VerticalAlignment + ";");
		stringBuilder.Append((int)chartTextArea.TextRotation + ";");
		stringBuilder.Append((int)chartTextArea.BackgroundMode + ";");
		stringBuilder.Append((int)chartTextArea.ParagraphType + ";");
		stringBuilder.Append((int)chartTextArea.Position + ";");
		stringBuilder.Append(chartTextArea.Size + ";");
		stringBuilder.Append(chartTextArea.Baseline + ";");
		stringBuilder.Append(chartTextArea.FontName + ";");
		stringBuilder.Append(chartTextArea.Text + ";");
		stringBuilder.Append(chartTextArea.TextRotationAngle + ";");
		stringBuilder.Append(chartTextArea.NumberFormat + ";");
		stringBuilder.Append(chartTextArea.NumberFormatIndex + ";");
		stringBuilder.Append(chartTextArea.FontIndex + ";");
		stringBuilder.Append(chartTextArea.Delimiter + ";");
		stringBuilder.Append(chartTextArea.NumberFormatIndex + ";");
		_ = chartTextArea.RGBColor;
		stringBuilder.Append(chartTextArea.RGBColor.ToArgb() + ";");
		if (chartTextArea.RichText != null)
		{
			stringBuilder.Append(GetChartRichTextStringAsString(chartTextArea.RichText as ChartRichTextString));
		}
		if (chartTextArea.FrameFormat != null)
		{
			stringBuilder.Append(GetChartFrameFormatImplAsString(chartTextArea.FrameFormat as ChartFrameFormatImpl));
		}
		if (chartTextArea.ObjectLink != null)
		{
			stringBuilder.Append(GetChartObjectLinkRecordAsString(chartTextArea.ObjectLink));
		}
		if (chartTextArea.TextRecord != null)
		{
			stringBuilder.Append(GetChartTextRecordAsString(chartTextArea.TextRecord));
		}
		if (chartTextArea.ChartAI != null)
		{
			stringBuilder.Append(GetChartAIRecordAsString(chartTextArea.ChartAI));
		}
		if (chartTextArea.ChartAlRuns != null)
		{
			stringBuilder.Append(GetChartAlrunsRecordAsString(chartTextArea.ChartAlRuns));
		}
		if (chartTextArea.Layout != null)
		{
			stringBuilder.Append(GetChartLayoutAsString(chartTextArea.Layout as ChartLayoutImpl));
		}
		if (chartTextArea.ParentWorkbook != null)
		{
			stringBuilder.Append(GetWorkbookImplAsString(chartTextArea.ParentWorkbook));
		}
		if (chartTextArea.StringCache != null)
		{
			string[] stringCache = chartTextArea.StringCache;
			foreach (string text in stringCache)
			{
				stringBuilder.Append(text + ";");
			}
		}
		if (chartTextArea.ValueFromCellsRange != null)
		{
			stringBuilder.Append(GetDataRangeAsString(chartTextArea.ValueFromCellsRange as ChartDataRange));
		}
		if (chartTextArea.AttachedLabelLayout != null)
		{
			stringBuilder.Append(GetChartAttachedLabelLayoutRecordAsString(chartTextArea.AttachedLabelLayout));
		}
		if (chartTextArea.ColorObject != null)
		{
			stringBuilder.Append(GetChartColorAsString(chartTextArea.ColorObject));
		}
		if (chartTextArea.Font != null)
		{
			stringBuilder.Append(GetFontAsString(chartTextArea.Font));
		}
		return stringBuilder;
	}

	internal StringBuilder GetDataRangeAsString(ChartDataRange dataRange)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (m_dataRange != null)
		{
			for (int i = dataRange.FirstRow; i <= dataRange.LastRow; i++)
			{
				for (int j = dataRange.FirstColumn; j <= dataRange.LastColumn; j++)
				{
					stringBuilder.Append(dataRange.GetValue(i, j).ToString() + ";");
				}
			}
			stringBuilder.Append(dataRange.Count + ";");
		}
		if (dataRange.SheetImpl != null)
		{
			stringBuilder.Append(GetWorksheetImplAsString(dataRange.SheetImpl));
		}
		return stringBuilder;
	}

	internal StringBuilder GetWorksheetImplAsString(WorksheetImpl worksheetImpl)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(worksheetImpl.HasSheetCalculation ? "1" : "0;");
		stringBuilder.Append(worksheetImpl.HasAlternateContent ? "1" : "0;");
		stringBuilder.Append(worksheetImpl.UseRangesCache ? "1" : "0;");
		stringBuilder.Append(worksheetImpl.IsVisible ? "1" : "0;");
		stringBuilder.Append(worksheetImpl.IsZeroHeight ? "1" : "0;");
		stringBuilder.Append(worksheetImpl.IsThickBottom ? "1" : "0;");
		stringBuilder.Append(worksheetImpl.IsThickTop ? "1" : "0;");
		stringBuilder.Append(worksheetImpl.CustomHeight ? "1" : "0;");
		stringBuilder.Append(worksheetImpl.HasMergedCells ? "1" : "0;");
		stringBuilder.Append(worksheetImpl.ParseDataOnDemand ? "1" : "0;");
		stringBuilder.Append(worksheetImpl.DisplayPageBreaks ? "1" : "0;");
		stringBuilder.Append(worksheetImpl.IsDisplayZeros ? "1" : "0;");
		stringBuilder.Append(worksheetImpl.IsGridLinesVisible ? "1" : "0;");
		stringBuilder.Append(worksheetImpl.IsRowColumnHeadersVisible ? "1" : "0;");
		stringBuilder.Append(worksheetImpl.IsStringsPreserved ? "1" : "0;");
		stringBuilder.Append(worksheetImpl.IsFreezePanes ? "1" : "0;");
		stringBuilder.Append(worksheetImpl.StandardHeightFlag ? "1" : "0;");
		stringBuilder.Append(worksheetImpl.IsEmpty ? "1" : "0;");
		stringBuilder.Append(worksheetImpl.UsedRangeIncludesFormatting ? "1" : "0;");
		stringBuilder.Append(worksheetImpl.UsedRangeIncludesCF ? "1" : "0;");
		stringBuilder.Append(worksheetImpl.ProtectContents ? "1" : "0;");
		stringBuilder.Append(worksheetImpl.IsImporting ? "1" : "0;");
		stringBuilder.Append(worksheetImpl.ContainsProtection ? "1" : "0;");
		stringBuilder.Append((int)worksheetImpl.View + ";");
		stringBuilder.Append((int)worksheetImpl.Version + ";");
		stringBuilder.Append((int)worksheetImpl.Type + ";");
		stringBuilder.Append(worksheetImpl.ArchiveItemName + ";");
		stringBuilder.Append(worksheetImpl.QuotedName + ";");
		stringBuilder.Append(worksheetImpl.DefaultColumnWidth + ";");
		stringBuilder.Append(worksheetImpl.VerticalSplit + ";");
		stringBuilder.Append(worksheetImpl.HorizontalSplit + ";");
		stringBuilder.Append(worksheetImpl.FirstVisibleRow + ";");
		stringBuilder.Append(worksheetImpl.MaxColumnWidth + ";");
		stringBuilder.Append(worksheetImpl.FirstVisibleColumn + ";");
		stringBuilder.Append(worksheetImpl.SelectionCount + ";");
		stringBuilder.Append(worksheetImpl.DefaultRowHeight + ";");
		stringBuilder.Append(worksheetImpl.BaseColumnWidth + ";");
		stringBuilder.Append(worksheetImpl.OutlineLevelColumn + ";");
		stringBuilder.Append(worksheetImpl.OutlineLevelRow + ";");
		stringBuilder.Append(worksheetImpl.RowsOutlineLevel + ";");
		stringBuilder.Append(worksheetImpl.ColumnsOutlineLevel + ";");
		stringBuilder.Append(worksheetImpl.ActivePane + ";");
		stringBuilder.Append(worksheetImpl.StandardHeight + ";");
		stringBuilder.Append(worksheetImpl.StandardWidth + ";");
		return stringBuilder;
	}

	internal StringBuilder GetChartShtpropsRecordAsString(ChartShtpropsRecord chartShtpropsRecord)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(chartShtpropsRecord.NeedInfill ? "1" : "0;");
		stringBuilder.Append(chartShtpropsRecord.NeedDataArray ? "1" : "0;");
		stringBuilder.Append(chartShtpropsRecord.IsAllowShortData ? "1" : "0;");
		stringBuilder.Append(chartShtpropsRecord.NeedDecoding ? "1" : "0;");
		stringBuilder.Append(chartShtpropsRecord.RecordCode + ";");
		stringBuilder.Append(chartShtpropsRecord.Length + ";");
		stringBuilder.Append(chartShtpropsRecord.StreamPos + ";");
		stringBuilder.Append(chartShtpropsRecord.MinimumRecordSize + ";");
		stringBuilder.Append(chartShtpropsRecord.MaximumRecordSize + ";");
		stringBuilder.Append(chartShtpropsRecord.MaximumMemorySize + ";");
		stringBuilder.Append(chartShtpropsRecord.StartDecodingOffset + ";");
		stringBuilder.Append(chartShtpropsRecord.IsManSerAlloc ? "1" : "0;");
		stringBuilder.Append(chartShtpropsRecord.IsPlotVisOnly ? "1" : "0;");
		stringBuilder.Append(chartShtpropsRecord.IsNotSizeWith ? "1" : "0;");
		stringBuilder.Append(chartShtpropsRecord.IsManPlotArea ? "1" : "0;");
		stringBuilder.Append(chartShtpropsRecord.IsAlwaysAutoPlotArea ? "1" : "0;");
		stringBuilder.Append(chartShtpropsRecord.Flags + ";");
		stringBuilder.Append(chartShtpropsRecord.Reserved + ";");
		stringBuilder.Append(chartShtpropsRecord.MinimumRecordSize + ";");
		stringBuilder.Append(chartShtpropsRecord.MaximumRecordSize + ";");
		stringBuilder.Append((int)chartShtpropsRecord.PlotBlank + ";");
		return stringBuilder;
	}

	internal StringBuilder GetChartFormatCollectionAsString(ChartFormatCollection chartFormatCollection)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(chartFormatCollection.QuietMode ? "1" : "0;");
		stringBuilder.Append(chartFormatCollection.IsReadOnly ? "1" : "0;");
		stringBuilder.Append(chartFormatCollection.Capacity + ";");
		stringBuilder.Append(chartFormatCollection.Count + ";");
		foreach (ChartFormatImpl item in chartFormatCollection)
		{
			stringBuilder.Append(GetChartFormatAsString(item));
		}
		stringBuilder.Append(chartFormatCollection.IsPrimary ? "1" : "0;");
		stringBuilder.Append(chartFormatCollection.NeedSecondaryAxis ? "1" : "0;");
		stringBuilder.Append(chartFormatCollection.IsParetoFormat ? "1" : "0;");
		stringBuilder.Append(chartFormatCollection.IsBarChartAxes ? "1" : "0;");
		stringBuilder.Append(chartFormatCollection.IsPercentStackedAxis ? "1" : "0;");
		return stringBuilder;
	}

	internal StringBuilder GetChartFormatAsString(ChartFormatImpl chartFormat)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(chartFormat.IsVeryColor ? "1" : "0;");
		stringBuilder.Append(chartFormat.IsVaryColor ? "1" : "0;");
		if (chartFormat.SerieFormat.TypeCode == TBIFFRecord.ChartBar)
		{
			stringBuilder.Append(chartFormat.IsHorizontalBar ? "1" : "0;");
			stringBuilder.Append(chartFormat.StackValuesBar ? "1" : "0;");
			stringBuilder.Append(chartFormat.ShowAsPercentsBar ? "1" : "0;");
			stringBuilder.Append(chartFormat.HasShadowBar ? "1" : "0;");
		}
		if (chartFormat.SerieFormat.TypeCode == TBIFFRecord.ChartLine)
		{
			stringBuilder.Append(chartFormat.StackValuesLine ? "1" : "0;");
			stringBuilder.Append(chartFormat.ShowAsPercentsLine ? "1" : "0;");
			stringBuilder.Append(chartFormat.HasShadowLine ? "1" : "0;");
		}
		if (chartFormat.SerieFormat.TypeCode == TBIFFRecord.ChartPie)
		{
			stringBuilder.Append(chartFormat.HasShadowPie ? "1" : "0;");
			stringBuilder.Append(chartFormat.ShowLeaderLines ? "1" : "0;");
			stringBuilder.Append(chartFormat.FirstSliceAngle + ";");
			stringBuilder.Append(chartFormat.DoughnutHoleSize + ";");
		}
		if (chartFormat.SerieFormat.TypeCode == TBIFFRecord.ChartScatter)
		{
			stringBuilder.Append(chartFormat.IsBubbles ? "1" : "0;");
			stringBuilder.Append(chartFormat.ShowNegativeBubbles ? "1" : "0;");
			stringBuilder.Append(chartFormat.HasShadowScatter ? "1" : "0;");
			stringBuilder.Append((int)chartFormat.SizeRepresents + ";");
			stringBuilder.Append(chartFormat.BubbleScale + ";");
		}
		if (chartFormat.SerieFormat.TypeCode == TBIFFRecord.ChartArea)
		{
			stringBuilder.Append(chartFormat.IsStacked ? "1" : "0;");
			stringBuilder.Append(chartFormat.IsCategoryBrokenDown ? "1" : "0;");
			stringBuilder.Append(chartFormat.IsAreaShadowed ? "1" : "0;");
		}
		if (chartFormat.SerieFormat.TypeCode == TBIFFRecord.ChartSurface)
		{
			stringBuilder.Append(chartFormat.IsFillSurface ? "1" : "0;");
			stringBuilder.Append(chartFormat.Is3DPhongShade ? "1" : "0;");
		}
		if (chartFormat.SerieFormat.TypeCode == TBIFFRecord.ChartRadar)
		{
			stringBuilder.Append(chartFormat.HasShadowRadar ? "1" : "0;");
		}
		if (chartFormat.SerieFormat.TypeCode == TBIFFRecord.ChartRadarArea)
		{
			stringBuilder.Append(chartFormat.HasRadarAxisLabels ? "1" : "0;");
		}
		if (chartFormat.SerieFormat.TypeCode == TBIFFRecord.ChartBoppop)
		{
			stringBuilder.Append(chartFormat.UseDefaultSplitValue ? "1" : "0;");
			stringBuilder.Append(chartFormat.HasShadowBoppop ? "1" : "0;");
			stringBuilder.Append((int)chartFormat.PieChartType + ";");
			stringBuilder.Append((int)chartFormat.SplitType + ";");
			stringBuilder.Append(chartFormat.SplitValue + ";");
			stringBuilder.Append(chartFormat.SplitPercent + ";");
			stringBuilder.Append(chartFormat.PieSecondSize + ";");
			stringBuilder.Append(chartFormat.Gap + ";");
			stringBuilder.Append(chartFormat.NumSplitValue + ";");
			stringBuilder.Append(chartFormat.GapWidth + ";");
		}
		stringBuilder.Append(chartFormat.IsSeriesName ? "1" : "0;");
		stringBuilder.Append(chartFormat.IsCategoryName ? "1" : "0;");
		stringBuilder.Append(chartFormat.IsValue ? "1" : "0;");
		stringBuilder.Append(chartFormat.IsPercentage ? "1" : "0;");
		stringBuilder.Append(chartFormat.IsBubbleSize ? "1" : "0;");
		stringBuilder.Append(chartFormat.IsDefaultRotation ? "1" : "0;");
		stringBuilder.Append(chartFormat.IsDefaultElevation ? "1" : "0;");
		stringBuilder.Append(chartFormat.RightAngleAxes ? "1" : "0;");
		stringBuilder.Append(chartFormat.IsClustered ? "1" : "0;");
		stringBuilder.Append(chartFormat.IsChartExType ? "1" : "0;");
		stringBuilder.Append(chartFormat.AutoScaling ? "1" : "0;");
		stringBuilder.Append(chartFormat.WallsAndGridlines2D ? "1" : "0;");
		stringBuilder.Append(chartFormat.IsPrimaryAxis ? "1" : "0;");
		stringBuilder.Append(chartFormat.IsChartChartLine ? "1" : "0;");
		stringBuilder.Append(chartFormat.IsChartLineFormat ? "1" : "0;");
		stringBuilder.Append(chartFormat.IsDropBar ? "1" : "0;");
		stringBuilder.Append(chartFormat.Is3D ? "1" : "0;");
		stringBuilder.Append(chartFormat.IsMarker ? "1" : "0;");
		stringBuilder.Append(chartFormat.IsLine ? "1" : "0;");
		stringBuilder.Append(chartFormat.IsSmoothed ? "1" : "0;");
		stringBuilder.Append(chartFormat.HasDropLines ? "1" : "0;");
		stringBuilder.Append(chartFormat.HasHighLowLines ? "1" : "0;");
		stringBuilder.Append(chartFormat.HasSeriesLines ? "1" : "0;");
		stringBuilder.Append((int)chartFormat.LineStyle + ";");
		stringBuilder.Append((int)chartFormat.DropLineStyle + ";");
		stringBuilder.Append((int)chartFormat.FormatRecordType + ";");
		stringBuilder.Append(chartFormat.Delimiter + ";");
		if (!chartFormat.Is3D)
		{
			stringBuilder.Append(chartFormat.Overlap + ";");
		}
		stringBuilder.Append(chartFormat.DelimiterLength + ";");
		stringBuilder.Append(chartFormat.Rotation + ";");
		stringBuilder.Append(chartFormat.Elevation + ";");
		stringBuilder.Append(chartFormat.Perspective + ";");
		stringBuilder.Append(chartFormat.HeightPercent + ";");
		stringBuilder.Append(chartFormat.DepthPercent + ";");
		stringBuilder.Append(chartFormat.GapDepth + ";");
		stringBuilder.Append(chartFormat.DrawingZOrder + ";");
		return stringBuilder;
	}

	internal StringBuilder GetChartFrameFormatImplAsString(ChartFrameFormatImpl chartFrameFormatImpl)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(chartFrameFormatImpl.HasInterior ? "1" : "0;");
		stringBuilder.Append(chartFrameFormatImpl.HasLineProperties ? "1" : "0;");
		stringBuilder.Append(chartFrameFormatImpl.HasShadowProperties ? "1" : "0;");
		stringBuilder.Append(chartFrameFormatImpl.Has3dProperties ? "1" : "0;");
		stringBuilder.Append(chartFrameFormatImpl.IsAutoSize ? "1" : "0;");
		stringBuilder.Append(chartFrameFormatImpl.IsAutoPosition ? "1" : "0;");
		stringBuilder.Append(chartFrameFormatImpl.IsBorderCornersRound ? "1" : "0;");
		stringBuilder.Append(chartFrameFormatImpl.IsAutomaticFormat ? "1" : "0;");
		stringBuilder.Append(chartFrameFormatImpl.Visible ? "1" : "0;");
		stringBuilder.Append((int)chartFrameFormatImpl.RectangleStyle + ";");
		stringBuilder.Append((int)chartFrameFormatImpl.Pattern + ";");
		if (chartFrameFormatImpl.Workbook != null)
		{
			stringBuilder.Append(GetWorkbookImplAsString(chartFrameFormatImpl.Workbook));
		}
		if (chartFrameFormatImpl.Layout != null)
		{
			stringBuilder.Append(GetChartLayoutAsString(chartFrameFormatImpl.Layout as ChartLayoutImpl));
		}
		if (chartFrameFormatImpl.Border != null)
		{
			stringBuilder.Append(GetChartBorderAsString(chartFrameFormatImpl.Border as ChartBorderImpl));
		}
		if (chartFrameFormatImpl.Interior != null)
		{
			stringBuilder.Append(GetChartInteriorAsString(chartFrameFormatImpl.Interior as ChartInteriorImpl));
		}
		if (chartFrameFormatImpl.ThreeD != null)
		{
			stringBuilder.Append(GetThreeDFormatAsString(chartFrameFormatImpl.ThreeD as ThreeDFormatImpl));
		}
		if (chartFrameFormatImpl.Fill != null)
		{
			stringBuilder.Append(GetFillAsString(chartFrameFormatImpl.Fill as ShapeFillImpl));
		}
		if (chartFrameFormatImpl.Shadow != null)
		{
			stringBuilder.Append(GetShadowAsString(chartFrameFormatImpl.Shadow as ShadowImpl));
		}
		if (chartFrameFormatImpl.LineProperties != null)
		{
			stringBuilder.Append(GetChartBorderAsString(chartFrameFormatImpl.LineProperties as ChartBorderImpl));
		}
		if (chartFrameFormatImpl.ForeGroundColorObject != null)
		{
			stringBuilder.Append(GetChartColorAsString(chartFrameFormatImpl.ForeGroundColorObject));
		}
		if (chartFrameFormatImpl.BackGroundColorObject != null)
		{
			stringBuilder.Append(GetChartColorAsString(chartFrameFormatImpl.BackGroundColorObject));
		}
		return stringBuilder;
	}

	internal StringBuilder GetChartLegendAsString(ChartLegendImpl chartLegend)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(chartLegend.IncludeInLayout ? "1" : "0;");
		stringBuilder.Append(chartLegend.IsVerticalLegend ? "1" : "0;");
		stringBuilder.Append(chartLegend.IsDefaultTextSettings ? "1" : "0;");
		stringBuilder.Append(chartLegend.IsChartTextArea ? "1" : "0;");
		stringBuilder.Append(chartLegend.ContainsDataTable ? "1" : "0;");
		stringBuilder.Append(chartLegend.AutoPosition ? "1" : "0;");
		stringBuilder.Append(chartLegend.AutoSeries ? "1" : "0;");
		stringBuilder.Append(chartLegend.AutoPositionX ? "1" : "0;");
		stringBuilder.Append(chartLegend.AutoPositionY ? "1" : "0;");
		stringBuilder.Append(chartLegend.X + ";");
		stringBuilder.Append(chartLegend.Y + ";");
		stringBuilder.Append(chartLegend.Width + ";");
		stringBuilder.Append(chartLegend.Height + ";");
		stringBuilder.Append(chartLegend.ChartExPosition + ";");
		stringBuilder.Append((int)chartLegend.Position + ";");
		stringBuilder.Append((int)chartLegend.Spacing + ";");
		stringBuilder.Append((int)chartLegend.ParagraphType + ";");
		if (chartLegend.TextArea != null)
		{
			stringBuilder.Append(GetChartTextAreaAsString(chartLegend.TextArea as ChartTextAreaImpl));
		}
		if (chartLegend.FrameFormat != null)
		{
			stringBuilder.Append(GetChartFrameFormatImplAsString(chartLegend.FrameFormat as ChartFrameFormatImpl));
		}
		if (chartLegend.LegendEntries != null)
		{
			stringBuilder.Append(GetChartLegendEntriesAsString(chartLegend.LegendEntries as ChartLegendEntriesColl));
		}
		if (chartLegend.Layout != null)
		{
			stringBuilder.Append(GetChartLayoutAsString(chartLegend.Layout as ChartLayoutImpl));
		}
		if (chartLegend.AttachedLabelLayout != null)
		{
			stringBuilder.Append(GetChartAttachedLabelLayoutRecordAsString(chartLegend.AttachedLabelLayout));
		}
		if (chartLegend.LegendRecord != null)
		{
			stringBuilder.Append(GetChartLegendRecordAsString(chartLegend.LegendRecord));
		}
		if (chartLegend.PositionRecord != null)
		{
			stringBuilder.Append(GetChartPosRecordAsString(chartLegend.PositionRecord));
		}
		return stringBuilder;
	}

	internal StringBuilder GetChartValueAxisAsString(ChartValueAxisImpl chartValueAxisImpl)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(chartValueAxisImpl.IsWrapText ? "1" : "0;");
		stringBuilder.Append(chartValueAxisImpl.IsPrimary ? "1" : "0;");
		stringBuilder.Append(chartValueAxisImpl.IsAutoTextRotation ? "1" : "0;");
		stringBuilder.Append(chartValueAxisImpl.IsChartFont ? "1" : "0;");
		stringBuilder.Append(chartValueAxisImpl.HasMinorGridLines ? "1" : "0;");
		stringBuilder.Append(chartValueAxisImpl.HasMajorGridLines ? "1" : "0;");
		stringBuilder.Append(chartValueAxisImpl.isNumber ? "1" : "0;");
		stringBuilder.Append(chartValueAxisImpl.Visible ? "1" : "0;");
		stringBuilder.Append(chartValueAxisImpl.ReversePlotOrder ? "1" : "0;");
		stringBuilder.Append(chartValueAxisImpl.Deleted ? "1" : "0;");
		stringBuilder.Append(chartValueAxisImpl.AutoTickLabelSpacing ? "1" : "0;");
		stringBuilder.Append(chartValueAxisImpl.AutoTickMarkSpacing ? "1" : "0;");
		stringBuilder.Append(chartValueAxisImpl.HasShadowProperties ? "1" : "0;");
		stringBuilder.Append(chartValueAxisImpl.Has3dProperties ? "1" : "0;");
		stringBuilder.Append(chartValueAxisImpl.IsSourceLinked ? "1" : "0;");
		stringBuilder.Append(chartValueAxisImpl.HasAxisTitle ? "1" : "0;");
		stringBuilder.Append(chartValueAxisImpl.IsDefaultTextSettings ? "1" : "0;");
		stringBuilder.Append(chartValueAxisImpl.IsAutoMin ? "1" : "0;");
		stringBuilder.Append(chartValueAxisImpl.IsAutoMax ? "1" : "0;");
		stringBuilder.Append(chartValueAxisImpl.AutoTickLabelSpacing ? "1" : "0;");
		stringBuilder.Append(chartValueAxisImpl.IsAutoMajor ? "1" : "0;");
		stringBuilder.Append(chartValueAxisImpl.IsAutoMinor ? "1" : "0;");
		stringBuilder.Append(chartValueAxisImpl.IsAutoCross ? "1" : "0;");
		stringBuilder.Append(chartValueAxisImpl.IsChangeAutoCross ? "1" : "0;");
		stringBuilder.Append(chartValueAxisImpl.IsChangeAutoCrossInLoading ? "1" : "0;");
		stringBuilder.Append(chartValueAxisImpl.IsLogScale ? "1" : "0;");
		stringBuilder.Append(chartValueAxisImpl.ReversePlotOrder ? "1" : "0;");
		stringBuilder.Append(chartValueAxisImpl.IsMaxCross ? "1" : "0;");
		stringBuilder.Append(chartValueAxisImpl.HasDisplayUnitLabel ? "1" : "0;");
		stringBuilder.Append((int)chartValueAxisImpl.AxisType + ";");
		stringBuilder.Append((int)chartValueAxisImpl.MinorTickMark + ";");
		stringBuilder.Append((int)chartValueAxisImpl.MajorTickMark + ";");
		stringBuilder.Append((int)chartValueAxisImpl.ParagraphType + ";");
		stringBuilder.Append((int)chartValueAxisImpl.LabelTextAlign + ";");
		stringBuilder.Append((int)chartValueAxisImpl.TickLabelPosition + ";");
		stringBuilder.Append((int)chartValueAxisImpl.Alignment + ";");
		stringBuilder.Append((int)chartValueAxisImpl.DisplayUnit + ";");
		stringBuilder.Append(chartValueAxisImpl.Title + ";");
		stringBuilder.Append(chartValueAxisImpl.TextRotationAngle + ";");
		stringBuilder.Append(chartValueAxisImpl.NumberFormatIndex + ";");
		stringBuilder.Append(chartValueAxisImpl.NumberFormat + ";");
		stringBuilder.Append(chartValueAxisImpl.AxisId + ";");
		stringBuilder.Append(chartValueAxisImpl.MinimumValue + ";");
		stringBuilder.Append(chartValueAxisImpl.MaximumValue + ";");
		stringBuilder.Append(chartValueAxisImpl.MajorUnit + ";");
		stringBuilder.Append(chartValueAxisImpl.MinorUnit + ";");
		stringBuilder.Append(chartValueAxisImpl.CrossValue + ";");
		stringBuilder.Append(chartValueAxisImpl.CrossesAt + ";");
		stringBuilder.Append(chartValueAxisImpl.DisplayUnitCustom + ";");
		if (chartValueAxisImpl.TextStream != null)
		{
			stringBuilder.Append(chartValueAxisImpl.TextStream.Length + ";");
		}
		if (chartValueAxisImpl.DisplayUnitLabel != null)
		{
			stringBuilder.Append(GetChartTextAreaAsString(chartValueAxisImpl.DisplayUnitLabel as ChartTextAreaImpl));
		}
		if (chartValueAxisImpl.FrameFormat != null)
		{
			stringBuilder.Append(GetChartFrameFormatImplAsString(chartValueAxisImpl.FrameFormat as ChartFrameFormatImpl));
		}
		if (chartValueAxisImpl.AxisPosition.HasValue)
		{
			stringBuilder.Append(chartValueAxisImpl.AxisPosition.HasValue ? chartValueAxisImpl.AxisPosition.Value.ToString() : "null;");
		}
		if (chartValueAxisImpl.Chart3DProperties != null)
		{
			stringBuilder.Append(GetThreeDFormatAsString(chartValueAxisImpl.Chart3DProperties as ThreeDFormatImpl));
		}
		if (chartValueAxisImpl.Chart3DOptions != null)
		{
			stringBuilder.Append(GetThreeDFormatAsString(chartValueAxisImpl.Chart3DOptions as ThreeDFormatImpl));
		}
		if (chartValueAxisImpl.ShadowProperties != null)
		{
			stringBuilder.Append(GetShadowAsString(chartValueAxisImpl.ShadowProperties as ShadowImpl));
		}
		if (chartValueAxisImpl.Shadow != null)
		{
			stringBuilder.Append(GetShadowAsString(chartValueAxisImpl.Shadow as ShadowImpl));
		}
		if (chartValueAxisImpl.Border != null)
		{
			stringBuilder.Append(GetChartBorderAsString(chartValueAxisImpl.Border as ChartBorderImpl));
		}
		if (chartValueAxisImpl.MinorGridLines != null)
		{
			stringBuilder.Append(GetChartGridLineAsString(chartValueAxisImpl.MinorGridLines as ChartGridLineImpl));
		}
		if (chartValueAxisImpl.MajorGridLines != null)
		{
			stringBuilder.Append(GetChartGridLineAsString(chartValueAxisImpl.MajorGridLines as ChartGridLineImpl));
		}
		if (chartValueAxisImpl.Font != null)
		{
			stringBuilder.Append(GetFontWrapperAsString(chartValueAxisImpl.Font as FontWrapper));
		}
		if (chartValueAxisImpl.TitleArea != null)
		{
			stringBuilder.Append(GetChartTextAreaAsString(chartValueAxisImpl.TitleArea as ChartTextAreaImpl));
		}
		return stringBuilder;
	}

	internal StringBuilder GetChartCategoryAxisAsString(ChartCategoryAxisImpl chartCategoryAxisImpl)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(chartCategoryAxisImpl.IsAutoMin ? "1" : "0;");
		stringBuilder.Append(chartCategoryAxisImpl.IsAutoMax ? "1" : "0;");
		stringBuilder.Append(chartCategoryAxisImpl.AutoTickLabelSpacing ? "1" : "0;");
		stringBuilder.Append(chartCategoryAxisImpl.IsAutoMajor ? "1" : "0;");
		stringBuilder.Append(chartCategoryAxisImpl.IsAutoMinor ? "1" : "0;");
		stringBuilder.Append(chartCategoryAxisImpl.IsAutoCross ? "1" : "0;");
		stringBuilder.Append(chartCategoryAxisImpl.IsChangeAutoCross ? "1" : "0;");
		stringBuilder.Append(chartCategoryAxisImpl.IsChangeAutoCrossInLoading ? "1" : "0;");
		stringBuilder.Append(chartCategoryAxisImpl.IsLogScale ? "1" : "0;");
		stringBuilder.Append(chartCategoryAxisImpl.ReversePlotOrder ? "1" : "0;");
		stringBuilder.Append(chartCategoryAxisImpl.IsMaxCross ? "1" : "0;");
		stringBuilder.Append(chartCategoryAxisImpl.HasDisplayUnitLabel ? "1" : "0;");
		stringBuilder.Append(chartCategoryAxisImpl.IsWrapText ? "1" : "0;");
		stringBuilder.Append(chartCategoryAxisImpl.IsPrimary ? "1" : "0;");
		stringBuilder.Append(chartCategoryAxisImpl.IsAutoTextRotation ? "1" : "0;");
		stringBuilder.Append(chartCategoryAxisImpl.IsChartFont ? "1" : "0;");
		stringBuilder.Append(chartCategoryAxisImpl.HasMinorGridLines ? "1" : "0;");
		stringBuilder.Append(chartCategoryAxisImpl.HasMajorGridLines ? "1" : "0;");
		stringBuilder.Append(chartCategoryAxisImpl.isNumber ? "1" : "0;");
		stringBuilder.Append(chartCategoryAxisImpl.Visible ? "1" : "0;");
		stringBuilder.Append(chartCategoryAxisImpl.Deleted ? "1" : "0;");
		stringBuilder.Append(chartCategoryAxisImpl.AutoTickMarkSpacing ? "1" : "0;");
		stringBuilder.Append(chartCategoryAxisImpl.HasShadowProperties ? "1" : "0;");
		stringBuilder.Append(chartCategoryAxisImpl.Has3dProperties ? "1" : "0;");
		stringBuilder.Append(chartCategoryAxisImpl.IsSourceLinked ? "1" : "0;");
		stringBuilder.Append(chartCategoryAxisImpl.HasAxisTitle ? "1" : "0;");
		stringBuilder.Append(chartCategoryAxisImpl.IsDefaultTextSettings ? "1" : "0;");
		stringBuilder.Append(chartCategoryAxisImpl.IsMaxCross ? "1" : "0;");
		stringBuilder.Append(chartCategoryAxisImpl.ChangeDateTimeAxisValue ? "1" : "0;");
		stringBuilder.Append(chartCategoryAxisImpl.HasAutoTickLabelSpacing ? "1" : "0;");
		stringBuilder.Append(chartCategoryAxisImpl.AutoTickLabelSpacing ? "1" : "0;");
		stringBuilder.Append(chartCategoryAxisImpl.IsBetween ? "1" : "0;");
		stringBuilder.Append(chartCategoryAxisImpl.ReversePlotOrder ? "1" : "0;");
		stringBuilder.Append(chartCategoryAxisImpl.BaseUnitIsAuto ? "1" : "0;");
		stringBuilder.Append(chartCategoryAxisImpl.MajorUnitScaleIsAuto ? "1" : "0;");
		stringBuilder.Append(chartCategoryAxisImpl.MinorUnitScaleIsAuto ? "1" : "0;");
		stringBuilder.Append(chartCategoryAxisImpl.IsAutoMajor ? "1" : "0;");
		stringBuilder.Append(chartCategoryAxisImpl.IsAutoMinor ? "1" : "0;");
		stringBuilder.Append(chartCategoryAxisImpl.IsAutoCross ? "1" : "0;");
		stringBuilder.Append(chartCategoryAxisImpl.IsAutoMax ? "1" : "0;");
		stringBuilder.Append(chartCategoryAxisImpl.IsAutoMin ? "1" : "0;");
		stringBuilder.Append(chartCategoryAxisImpl.NoMultiLevelLabel ? "1" : "0;");
		stringBuilder.Append(chartCategoryAxisImpl.IsBinningByCategory ? "1" : "0;");
		stringBuilder.Append(chartCategoryAxisImpl.HasAutomaticBins ? "1" : "0;");
		stringBuilder.Append(chartCategoryAxisImpl.IsChartBubbleOrScatter ? "1" : "0;");
		stringBuilder.Append((int)chartCategoryAxisImpl.AxisType + ";");
		stringBuilder.Append((int)chartCategoryAxisImpl.DisplayUnit + ";");
		stringBuilder.Append((int)chartCategoryAxisImpl.MinorTickMark + ";");
		stringBuilder.Append((int)chartCategoryAxisImpl.MajorTickMark + ";");
		stringBuilder.Append((int)chartCategoryAxisImpl.TickLabelPosition + ";");
		stringBuilder.Append((int)chartCategoryAxisImpl.Alignment + ";");
		stringBuilder.Append((int)chartCategoryAxisImpl.ParagraphType + ";");
		stringBuilder.Append((int)chartCategoryAxisImpl.LabelTextAlign + ";");
		stringBuilder.Append((int)chartCategoryAxisImpl.CategoryType + ";");
		stringBuilder.Append((int)chartCategoryAxisImpl.BaseUnit + ";");
		stringBuilder.Append((int)chartCategoryAxisImpl.MajorUnitScale + ";");
		stringBuilder.Append((int)chartCategoryAxisImpl.MinorUnitScale + ";");
		stringBuilder.Append(chartCategoryAxisImpl.MinimumValue + ";");
		stringBuilder.Append(chartCategoryAxisImpl.MaximumValue + ";");
		stringBuilder.Append(chartCategoryAxisImpl.MajorUnit + ";");
		stringBuilder.Append(chartCategoryAxisImpl.MinorUnit + ";");
		stringBuilder.Append(chartCategoryAxisImpl.CrossValue + ";");
		stringBuilder.Append(chartCategoryAxisImpl.CrossesAt + ";");
		stringBuilder.Append(chartCategoryAxisImpl.DisplayUnitCustom + ";");
		stringBuilder.Append(chartCategoryAxisImpl.Title + ";");
		stringBuilder.Append(chartCategoryAxisImpl.TextRotationAngle + ";");
		stringBuilder.Append(chartCategoryAxisImpl.NumberFormatIndex + ";");
		stringBuilder.Append(chartCategoryAxisImpl.NumberFormat + ";");
		stringBuilder.Append(chartCategoryAxisImpl.AxisId + ";");
		stringBuilder.Append(chartCategoryAxisImpl.LabelFrequency + ";");
		stringBuilder.Append(chartCategoryAxisImpl.TickLabelSpacing + ";");
		stringBuilder.Append(chartCategoryAxisImpl.TickMarksFrequency + ";");
		stringBuilder.Append(chartCategoryAxisImpl.TickMarkSpacing + ";");
		stringBuilder.Append(chartCategoryAxisImpl.Offset + ";");
		stringBuilder.Append(chartCategoryAxisImpl.NumberOfBins + ";");
		stringBuilder.Append(chartCategoryAxisImpl.BinWidth + ";");
		stringBuilder.Append(chartCategoryAxisImpl.UnderflowBinValue + ";");
		stringBuilder.Append(chartCategoryAxisImpl.OverflowBinValue + ";");
		if (chartCategoryAxisImpl.TextStream != null)
		{
			stringBuilder.Append(chartCategoryAxisImpl.TextStream.Length + ";");
		}
		if (chartCategoryAxisImpl.HistogramAxisFormatProperty != null)
		{
			stringBuilder.Append(GetHistogramAxisFormatAsString(chartCategoryAxisImpl.HistogramAxisFormatProperty));
		}
		if (chartCategoryAxisImpl.FrameFormat != null)
		{
			stringBuilder.Append(GetChartFrameFormatImplAsString(chartCategoryAxisImpl.FrameFormat as ChartFrameFormatImpl));
		}
		if (chartCategoryAxisImpl.AxisPosition.HasValue)
		{
			stringBuilder.Append(chartCategoryAxisImpl.AxisPosition.HasValue ? chartCategoryAxisImpl.AxisPosition.Value.ToString() : "null;");
		}
		if (chartCategoryAxisImpl.Chart3DProperties != null)
		{
			stringBuilder.Append(GetThreeDFormatAsString(chartCategoryAxisImpl.Chart3DProperties as ThreeDFormatImpl));
		}
		if (chartCategoryAxisImpl.Chart3DOptions != null)
		{
			stringBuilder.Append(GetThreeDFormatAsString(chartCategoryAxisImpl.Chart3DOptions as ThreeDFormatImpl));
		}
		if (chartCategoryAxisImpl.ShadowProperties != null)
		{
			stringBuilder.Append(GetShadowAsString(chartCategoryAxisImpl.ShadowProperties as ShadowImpl));
		}
		if (chartCategoryAxisImpl.Shadow != null)
		{
			stringBuilder.Append(GetShadowAsString(chartCategoryAxisImpl.Shadow as ShadowImpl));
		}
		if (chartCategoryAxisImpl.Border != null)
		{
			stringBuilder.Append(GetChartBorderAsString(chartCategoryAxisImpl.Border as ChartBorderImpl));
		}
		if (chartCategoryAxisImpl.MinorGridLines != null)
		{
			stringBuilder.Append(GetChartGridLineAsString(chartCategoryAxisImpl.MinorGridLines as ChartGridLineImpl));
		}
		if (chartCategoryAxisImpl.MajorGridLines != null)
		{
			stringBuilder.Append(GetChartGridLineAsString(chartCategoryAxisImpl.MajorGridLines as ChartGridLineImpl));
		}
		if (chartCategoryAxisImpl.Font != null)
		{
			stringBuilder.Append(GetFontWrapperAsString(chartCategoryAxisImpl.Font as FontWrapper));
		}
		if (chartCategoryAxisImpl.TitleArea != null)
		{
			stringBuilder.Append(GetChartTextAreaAsString(chartCategoryAxisImpl.TitleArea as ChartTextAreaImpl));
		}
		if (chartCategoryAxisImpl.DisplayUnitLabel != null)
		{
			stringBuilder.Append(GetChartTextAreaAsString(chartCategoryAxisImpl.DisplayUnitLabel as ChartTextAreaImpl));
		}
		return stringBuilder;
	}

	private StringBuilder GetFontWrapperAsString(FontWrapper fontWrapper)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(fontWrapper.Bold ? "1" : "0;");
		stringBuilder.Append(fontWrapper.Italic ? "1" : "0;");
		stringBuilder.Append(fontWrapper.MacOSOutlineFont ? "1" : "0;");
		stringBuilder.Append(fontWrapper.MacOSShadow ? "1" : "0;");
		stringBuilder.Append(fontWrapper.Strikethrough ? "1" : "0;");
		stringBuilder.Append(fontWrapper.Subscript ? "1" : "0;");
		stringBuilder.Append(fontWrapper.Superscript ? "1" : "0;");
		stringBuilder.Append(fontWrapper.IsAutoColor ? "1" : "0;");
		stringBuilder.Append(fontWrapper.IsReadOnly ? "1" : "0;");
		stringBuilder.Append(fontWrapper.IsDirectAccess ? "1" : "0;");
		stringBuilder.Append((int)fontWrapper.Color + ";");
		stringBuilder.Append((int)fontWrapper.Underline + ";");
		stringBuilder.Append((int)fontWrapper.VerticalAlignment + ";");
		stringBuilder.Append(fontWrapper.Size + ";");
		stringBuilder.Append(fontWrapper.Baseline + ";");
		stringBuilder.Append(fontWrapper.FontName + ";");
		stringBuilder.Append(fontWrapper.CharSet + ";");
		stringBuilder.Append(fontWrapper.FontIndex + ";");
		stringBuilder.Append(fontWrapper.Index + ";");
		_ = fontWrapper.RGBColor;
		stringBuilder.Append(fontWrapper.RGBColor.ToArgb() + ";");
		if (fontWrapper.Wrapped != null)
		{
			stringBuilder.Append(GetFontAsString(fontWrapper.Wrapped)?.ToString() + ";");
		}
		if (fontWrapper.Font != null)
		{
			stringBuilder.Append(GetFontAsString(fontWrapper.Font)?.ToString() + ";");
		}
		if (fontWrapper.ColorObject != null)
		{
			stringBuilder.Append(GetChartColorAsString(fontWrapper.ColorObject));
		}
		if (fontWrapper.Workbook != null)
		{
			stringBuilder.Append(GetWorkbookImplAsString(fontWrapper.Workbook));
		}
		return stringBuilder;
	}

	internal StringBuilder GetChartSeriesAxisAsString(ChartSeriesAxisImpl chartSeriesAxisImpl)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(chartSeriesAxisImpl.IsWrapText ? "1" : "0;");
		stringBuilder.Append(chartSeriesAxisImpl.IsPrimary ? "1" : "0;");
		stringBuilder.Append(chartSeriesAxisImpl.IsAutoTextRotation ? "1" : "0;");
		stringBuilder.Append(chartSeriesAxisImpl.IsChartFont ? "1" : "0;");
		stringBuilder.Append(chartSeriesAxisImpl.HasMinorGridLines ? "1" : "0;");
		stringBuilder.Append(chartSeriesAxisImpl.HasMajorGridLines ? "1" : "0;");
		stringBuilder.Append(chartSeriesAxisImpl.isNumber ? "1" : "0;");
		stringBuilder.Append(chartSeriesAxisImpl.Visible ? "1" : "0;");
		stringBuilder.Append(chartSeriesAxisImpl.ReversePlotOrder ? "1" : "0;");
		stringBuilder.Append(chartSeriesAxisImpl.Deleted ? "1" : "0;");
		stringBuilder.Append(chartSeriesAxisImpl.AutoTickLabelSpacing ? "1" : "0;");
		stringBuilder.Append(chartSeriesAxisImpl.AutoTickMarkSpacing ? "1" : "0;");
		stringBuilder.Append(chartSeriesAxisImpl.HasShadowProperties ? "1" : "0;");
		stringBuilder.Append(chartSeriesAxisImpl.Has3dProperties ? "1" : "0;");
		stringBuilder.Append(chartSeriesAxisImpl.IsSourceLinked ? "1" : "0;");
		stringBuilder.Append(chartSeriesAxisImpl.HasAxisTitle ? "1" : "0;");
		stringBuilder.Append(chartSeriesAxisImpl.IsDefaultTextSettings ? "1" : "0;");
		stringBuilder.Append(chartSeriesAxisImpl.IsBetween ? "1" : "0;");
		stringBuilder.Append(chartSeriesAxisImpl.IsLogScale ? "1" : "0;");
		stringBuilder.Append((int)chartSeriesAxisImpl.AxisType + ";");
		stringBuilder.Append((int)chartSeriesAxisImpl.MinorTickMark + ";");
		stringBuilder.Append((int)chartSeriesAxisImpl.MajorTickMark + ";");
		stringBuilder.Append((int)chartSeriesAxisImpl.TickLabelPosition + ";");
		stringBuilder.Append((int)chartSeriesAxisImpl.Alignment + ";");
		stringBuilder.Append((int)chartSeriesAxisImpl.ParagraphType + ";");
		stringBuilder.Append((int)chartSeriesAxisImpl.LabelTextAlign + ";");
		stringBuilder.Append(chartSeriesAxisImpl.Title + ";");
		stringBuilder.Append(chartSeriesAxisImpl.TextRotationAngle + ";");
		stringBuilder.Append(chartSeriesAxisImpl.NumberFormatIndex + ";");
		stringBuilder.Append(chartSeriesAxisImpl.NumberFormat + ";");
		stringBuilder.Append(chartSeriesAxisImpl.AxisId + ";");
		stringBuilder.Append(chartSeriesAxisImpl.LabelFrequency + ";");
		stringBuilder.Append(chartSeriesAxisImpl.TickLabelSpacing + ";");
		stringBuilder.Append(chartSeriesAxisImpl.TickMarksFrequency + ";");
		stringBuilder.Append(chartSeriesAxisImpl.TickMarkSpacing + ";");
		stringBuilder.Append(chartSeriesAxisImpl.CrossesAt + ";");
		stringBuilder.Append(chartSeriesAxisImpl.MaximumValue + ";");
		stringBuilder.Append(chartSeriesAxisImpl.MinimumValue + ";");
		if (chartSeriesAxisImpl.TextStream != null)
		{
			stringBuilder.Append(chartSeriesAxisImpl.TextStream.Length + ";");
		}
		if (chartSeriesAxisImpl.AxisPosition.HasValue)
		{
			stringBuilder.Append(chartSeriesAxisImpl.AxisPosition.HasValue ? chartSeriesAxisImpl.AxisPosition.Value.ToString() : "null;");
		}
		if (chartSeriesAxisImpl.FrameFormat != null)
		{
			stringBuilder.Append(GetChartFrameFormatAsString(chartSeriesAxisImpl.FrameFormat as ChartPlotAreaImpl));
		}
		if (chartSeriesAxisImpl.Chart3DOptions != null)
		{
			stringBuilder.Append(GetThreeDFormatAsString(chartSeriesAxisImpl.Chart3DOptions as ThreeDFormatImpl));
		}
		if (chartSeriesAxisImpl.Chart3DProperties != null)
		{
			stringBuilder.Append(GetThreeDFormatAsString(chartSeriesAxisImpl.Chart3DProperties as ThreeDFormatImpl));
		}
		if (chartSeriesAxisImpl.Shadow != null)
		{
			stringBuilder.Append(GetShadowAsString(chartSeriesAxisImpl.Shadow as ShadowImpl));
		}
		if (chartSeriesAxisImpl.ShadowProperties != null)
		{
			stringBuilder.Append(GetShadowAsString(chartSeriesAxisImpl.ShadowProperties as ShadowImpl));
		}
		if (chartSeriesAxisImpl.ParentChart != null)
		{
			stringBuilder.Append(GetChartAsString());
		}
		if (chartSeriesAxisImpl.Border != null)
		{
			stringBuilder.Append(GetChartBorderAsString(chartSeriesAxisImpl.Border as ChartBorderImpl));
		}
		if (chartSeriesAxisImpl.MinorGridLines != null)
		{
			stringBuilder.Append(GetChartGridLineAsString(chartSeriesAxisImpl.MinorGridLines as ChartGridLineImpl));
		}
		if (chartSeriesAxisImpl.MajorGridLines != null)
		{
			stringBuilder.Append(GetChartGridLineAsString(chartSeriesAxisImpl.MajorGridLines as ChartGridLineImpl));
		}
		if (chartSeriesAxisImpl.Font != null)
		{
			stringBuilder.Append(GetFontAsString(chartSeriesAxisImpl.Font as FontImpl));
		}
		if (chartSeriesAxisImpl.TitleArea != null)
		{
			stringBuilder.Append(GetChartTextAreaAsString(chartSeriesAxisImpl.TitleArea as ChartTextAreaImpl));
		}
		return stringBuilder;
	}

	internal StringBuilder GetHistogramAxisFormatAsString(HistogramAxisFormat histogramAxisFormat)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(histogramAxisFormat.HasAutomaticBins ? "1" : "0;");
		stringBuilder.Append(histogramAxisFormat.IsBinningByCategory ? "1" : "0;");
		stringBuilder.Append(histogramAxisFormat.IsIntervalClosedinLeft ? "1" : "0;");
		stringBuilder.Append(histogramAxisFormat.IsAutomaticFlowValue ? "1" : "0;");
		stringBuilder.Append(histogramAxisFormat.IsNotAutomaticUnderFlowValue ? "1" : "0;");
		stringBuilder.Append(histogramAxisFormat.IsNotAutomaticOverFlowValue ? "1" : "0;");
		stringBuilder.Append(histogramAxisFormat.BinWidth + ";");
		stringBuilder.Append(histogramAxisFormat.NumberOfBins + ";");
		stringBuilder.Append(histogramAxisFormat.OverflowBinValue + ";");
		stringBuilder.Append(histogramAxisFormat.UnderflowBinValue + ";");
		stringBuilder.Append(histogramAxisFormat.FlagOptions + ";");
		return stringBuilder;
	}

	internal StringBuilder GetFontAsString(FontImpl fontImpl)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(fontImpl.Bold ? "1" : "0;");
		stringBuilder.Append(fontImpl.Italic ? "1" : "0;");
		stringBuilder.Append(fontImpl.MacOSOutlineFont ? "1" : "0;");
		stringBuilder.Append(fontImpl.MacOSShadow ? "1" : "0;");
		stringBuilder.Append(fontImpl.Strikethrough ? "1" : "0;");
		stringBuilder.Append(fontImpl.Subscript ? "1" : "0;");
		stringBuilder.Append(fontImpl.Superscript ? "1" : "0;");
		stringBuilder.Append(fontImpl.IsAutoColor ? "1" : "0;");
		stringBuilder.Append(fontImpl.HasLatin ? "1" : "0;");
		stringBuilder.Append(fontImpl.HasComplexScripts ? "1" : "0;");
		stringBuilder.Append(fontImpl.HasEastAsianFont ? "1" : "0;");
		stringBuilder.Append(fontImpl.HasParagrapAlign ? "1" : "0;");
		stringBuilder.Append((int)fontImpl.Color + ";");
		stringBuilder.Append((int)fontImpl.Underline + ";");
		stringBuilder.Append((int)fontImpl.VerticalAlignment + ";");
		stringBuilder.Append((int)fontImpl.ParaAlign + ";");
		stringBuilder.Append(fontImpl.Size + ";");
		stringBuilder.Append(fontImpl.FontName + ";");
		stringBuilder.Append(fontImpl.BaseLine + ";");
		stringBuilder.Append(fontImpl.Index + ";");
		stringBuilder.Append(fontImpl.Language + ";");
		stringBuilder.Append(fontImpl.ActualFontName + ";");
		stringBuilder.Append(fontImpl.CharSet + ";");
		stringBuilder.Append(fontImpl.Family + ";");
		return stringBuilder;
	}

	internal StringBuilder GetChartRichTextStringAsString(ChartRichTextString chartRichTextString)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(chartRichTextString.Text + ";");
		return stringBuilder;
	}

	internal StringBuilder GetChartLayoutAsString(ChartLayoutImpl chartLayoutImpl)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(chartLayoutImpl.IsManualLayout ? "1" : "0;");
		stringBuilder.Append((int)chartLayoutImpl.LayoutTarget + ";");
		stringBuilder.Append((int)chartLayoutImpl.LeftMode + ";");
		stringBuilder.Append((int)chartLayoutImpl.TopMode + ";");
		stringBuilder.Append((int)chartLayoutImpl.WidthMode + ";");
		stringBuilder.Append((int)chartLayoutImpl.HeightMode + ";");
		stringBuilder.Append(chartLayoutImpl.Left + ";");
		stringBuilder.Append(chartLayoutImpl.Top + ";");
		stringBuilder.Append(chartLayoutImpl.Width + ";");
		stringBuilder.Append(chartLayoutImpl.Height + ";");
		if (chartLayoutImpl.ManualLayout != null)
		{
			stringBuilder.Append(GetChartManualLayoutAsString(chartLayoutImpl.ManualLayout as ChartManualLayoutImpl));
		}
		return stringBuilder;
	}

	internal StringBuilder GetWorkbookImplAsString(WorkbookImpl workbookImpl)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(workbookImpl.HidePivotFieldList ? "1" : "0;");
		stringBuilder.Append(workbookImpl.Date1904 ? "1" : "0;");
		stringBuilder.Append(workbookImpl.PrecisionAsDisplayed ? "1" : "0;");
		stringBuilder.Append(workbookImpl.IsCellProtection ? "1" : "0;");
		stringBuilder.Append(workbookImpl.IsWindowProtection ? "1" : "0;");
		stringBuilder.Append(workbookImpl.ReadOnly ? "1" : "0;");
		stringBuilder.Append(workbookImpl.Saved ? "1" : "0;");
		stringBuilder.Append(workbookImpl.HasMacros ? "1" : "0;");
		stringBuilder.Append(workbookImpl.ThrowOnUnknownNames ? "1" : "0;");
		stringBuilder.Append(workbookImpl.IsHScrollBarVisible ? "1" : "0;");
		stringBuilder.Append(workbookImpl.IsVScrollBarVisible ? "1" : "0;");
		stringBuilder.Append(workbookImpl.DisableMacrosStart ? "1" : "0;");
		stringBuilder.Append(workbookImpl.HasStandardFont ? "1" : "0;");
		stringBuilder.Append(workbookImpl.Allow3DRangesInDataValidation ? "1" : "0;");
		stringBuilder.Append(workbookImpl.IsRightToLeft ? "1" : "0;");
		stringBuilder.Append(workbookImpl.DisplayWorkbookTabs ? "1" : "0;");
		stringBuilder.Append(workbookImpl.DetectDateTimeInValue ? "1" : "0;");
		stringBuilder.Append(workbookImpl.UseFastStringSearching ? "1" : "0;");
		stringBuilder.Append(workbookImpl.ReadOnlyRecommended ? "1" : "0;");
		stringBuilder.Append(workbookImpl.EnabledCalcEngine ? "1" : "0;");
		stringBuilder.Append(workbookImpl.IsWorkbookOpening ? "1" : "0;");
		stringBuilder.Append(workbookImpl.Saving ? "1" : "0;");
		stringBuilder.Append(workbookImpl.HasInlineStrings ? "1" : "0;");
		stringBuilder.Append(workbookImpl.HasDuplicatedNames ? "1" : "0;");
		stringBuilder.Append(workbookImpl.InternalSaved ? "1" : "0;");
		stringBuilder.Append(workbookImpl.IsConverted ? "1" : "0;");
		stringBuilder.Append(workbookImpl.IsCreated ? "1" : "0;");
		stringBuilder.Append(workbookImpl.IsLoaded ? "1" : "0;");
		stringBuilder.Append(workbookImpl.CheckCompability ? "1" : "0;");
		stringBuilder.Append(workbookImpl.HasApostrophe ? "1" : "0;");
		stringBuilder.Append(workbookImpl.ParseOnDemand ? "1" : "0;");
		stringBuilder.Append(workbookImpl.IsCellModified ? "1" : "0;");
		stringBuilder.Append(workbookImpl.IsEqualColor ? "1" : "0;");
		stringBuilder.Append((int)workbookImpl.Options + ";");
		stringBuilder.Append((int)workbookImpl.Version + ";");
		stringBuilder.Append(workbookImpl.ActiveSheetIndex + ";");
		stringBuilder.Append(workbookImpl.Author + ";");
		stringBuilder.Append(workbookImpl.CodeName + ";");
		stringBuilder.Append(workbookImpl.DefaultThemeVersion + ";");
		stringBuilder.Append(workbookImpl.DisplayedTab + ";");
		stringBuilder.Append(workbookImpl.StandardFontSize + ";");
		stringBuilder.Append(workbookImpl.StandardFont + ";");
		stringBuilder.Append(workbookImpl.RowSeparator + ";");
		stringBuilder.Append(workbookImpl.ArgumentsSeparator + ";");
		stringBuilder.Append(workbookImpl.PasswordToOpen + ";");
		stringBuilder.Append(workbookImpl.MaxRowCount + ";");
		stringBuilder.Append(workbookImpl.MaxColumnCount + ";");
		stringBuilder.Append(workbookImpl.MaxXFCount + ";");
		stringBuilder.Append(workbookImpl.MaxIndent + ";");
		stringBuilder.Append(workbookImpl.MaxImportColumns + ";");
		stringBuilder.Append(workbookImpl.BookCFPriorityCount + ";");
		stringBuilder.Append(workbookImpl.LastPivotTableIndex + ";");
		stringBuilder.Append(workbookImpl.FullFileName + ";");
		stringBuilder.Append(workbookImpl.ObjectCount + ";");
		stringBuilder.Append(workbookImpl.CurrentObjectId + ";");
		stringBuilder.Append(workbookImpl.CurrentHeaderId + ";");
		stringBuilder.Append(workbookImpl.FirstCharSize + ";");
		stringBuilder.Append(workbookImpl.SecondCharSize + ";");
		stringBuilder.Append(workbookImpl.BeginVersion + ";");
		stringBuilder.Append(workbookImpl.DefaultXFIndex + ";");
		stringBuilder.Append(workbookImpl.MaxTableIndex + ";");
		stringBuilder.Append(workbookImpl.AlgorithmName + ";");
		stringBuilder.Append(workbookImpl.SpinCount + ";");
		stringBuilder.Append(workbookImpl.StandardRowHeight + ";");
		stringBuilder.Append(workbookImpl.StandardRowHeightInPixels + ";");
		return stringBuilder;
	}

	internal StringBuilder GetChartAIRecordAsString(ChartAIRecord chartAIRecord)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(chartAIRecord.NeedInfill ? "1" : "0;");
		stringBuilder.Append(chartAIRecord.NeedDataArray ? "1" : "0;");
		stringBuilder.Append(chartAIRecord.IsAllowShortData ? "1" : "0;");
		stringBuilder.Append(chartAIRecord.NeedDecoding ? "1" : "0;");
		stringBuilder.Append(chartAIRecord.IsCustomNumberFormat ? "1" : "0;");
		stringBuilder.Append((int)chartAIRecord.IndexIdentifier + ";");
		stringBuilder.Append((int)chartAIRecord.Reference + ";");
		stringBuilder.Append((int)chartAIRecord.TypeCode + ";");
		stringBuilder.Append(chartAIRecord.RecordCode + ";");
		stringBuilder.Append(chartAIRecord.Length + ";");
		stringBuilder.Append(chartAIRecord.Data?.ToString() + ";");
		stringBuilder.Append(chartAIRecord.StreamPos + ";");
		stringBuilder.Append(chartAIRecord.MinimumRecordSize + ";");
		stringBuilder.Append(chartAIRecord.MaximumRecordSize + ";");
		stringBuilder.Append(chartAIRecord.MaximumMemorySize + ";");
		stringBuilder.Append(chartAIRecord.StartDecodingOffset + ";");
		stringBuilder.Append(chartAIRecord.Options + ";");
		stringBuilder.Append(chartAIRecord.NumberFormatIndex + ";");
		stringBuilder.Append(chartAIRecord.FormulaSize + ";");
		return stringBuilder;
	}

	internal StringBuilder GetChartAlrunsRecordAsString(ChartAlrunsRecord chartAlrunsRecord)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(chartAlrunsRecord.NeedInfill ? "1" : "0;");
		stringBuilder.Append(chartAlrunsRecord.NeedDataArray ? "1" : "0;");
		stringBuilder.Append(chartAlrunsRecord.IsAllowShortData ? "1" : "0;");
		stringBuilder.Append(chartAlrunsRecord.NeedDecoding ? "1" : "0;");
		stringBuilder.Append((int)chartAlrunsRecord.TypeCode + ";");
		stringBuilder.Append(chartAlrunsRecord.RecordCode + ";");
		stringBuilder.Append(chartAlrunsRecord.Length + ";");
		stringBuilder.Append(chartAlrunsRecord.StreamPos + ";");
		stringBuilder.Append(chartAlrunsRecord.MinimumRecordSize + ";");
		stringBuilder.Append(chartAlrunsRecord.MaximumRecordSize + ";");
		stringBuilder.Append(chartAlrunsRecord.MaximumMemorySize + ";");
		stringBuilder.Append(chartAlrunsRecord.StartDecodingOffset + ";");
		stringBuilder.Append(chartAlrunsRecord.Quantity + ";");
		return stringBuilder;
	}

	internal StringBuilder GetChartObjectLinkRecordAsString(ChartObjectLinkRecord chartObjectLinkRecord)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(chartObjectLinkRecord.NeedInfill ? "1" : "0;");
		stringBuilder.Append(chartObjectLinkRecord.NeedDataArray ? "1" : "0;");
		stringBuilder.Append(chartObjectLinkRecord.IsAllowShortData ? "1" : "0;");
		stringBuilder.Append(chartObjectLinkRecord.NeedDecoding ? "1" : "0;");
		stringBuilder.Append((int)chartObjectLinkRecord.TypeCode + ";");
		stringBuilder.Append((int)chartObjectLinkRecord.LinkObject + ";");
		stringBuilder.Append(chartObjectLinkRecord.RecordCode + ";");
		stringBuilder.Append(chartObjectLinkRecord.Length + ";");
		stringBuilder.Append(chartObjectLinkRecord.StreamPos + ";");
		stringBuilder.Append(chartObjectLinkRecord.MinimumRecordSize + ";");
		stringBuilder.Append(chartObjectLinkRecord.MaximumRecordSize + ";");
		stringBuilder.Append(chartObjectLinkRecord.MaximumMemorySize + ";");
		stringBuilder.Append(chartObjectLinkRecord.StartDecodingOffset + ";");
		stringBuilder.Append(chartObjectLinkRecord.SeriesNumber + ";");
		stringBuilder.Append(chartObjectLinkRecord.DataPointNumber + ";");
		return stringBuilder;
	}

	internal StringBuilder GetChartColorAsString(ChartColor chartColor)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append((chartColor.IsSchemeColor ? "1" : "0") + ";");
		stringBuilder.Append(chartColor.Value + ";");
		stringBuilder.Append(chartColor.Tint + ";");
		stringBuilder.Append(chartColor.Saturation + ";");
		stringBuilder.Append(chartColor.Luminance + ";");
		stringBuilder.Append(chartColor.LuminanceOffSet + ";");
		stringBuilder.Append(chartColor.SchemaName + ";");
		stringBuilder.Append(chartColor.HexColor + ";");
		stringBuilder.Append((int)chartColor.ColorType + ";");
		return stringBuilder;
	}

	internal StringBuilder GetChartTextRecordAsString(ChartTextRecord chartTextRecord)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(chartTextRecord.NeedInfill ? "1" : "0;");
		stringBuilder.Append(chartTextRecord.NeedDataArray ? "1" : "0;");
		stringBuilder.Append(chartTextRecord.IsAllowShortData ? "1" : "0;");
		stringBuilder.Append(chartTextRecord.NeedDecoding ? "1" : "0;");
		stringBuilder.Append(chartTextRecord.IsAutoColor ? "1" : "0;");
		stringBuilder.Append(chartTextRecord.IsShowKey ? "1" : "0;");
		stringBuilder.Append(chartTextRecord.IsShowValue ? "1" : "0;");
		stringBuilder.Append(chartTextRecord.IsVertical ? "1" : "0;");
		stringBuilder.Append(chartTextRecord.IsAutoText ? "1" : "0;");
		stringBuilder.Append(chartTextRecord.IsGenerated ? "1" : "0;");
		stringBuilder.Append(chartTextRecord.IsDeleted ? "1" : "0;");
		stringBuilder.Append(chartTextRecord.IsAutoMode ? "1" : "0;");
		stringBuilder.Append(chartTextRecord.IsShowLabelPercent ? "1" : "0;");
		stringBuilder.Append(chartTextRecord.IsShowPercent ? "1" : "0;");
		stringBuilder.Append(chartTextRecord.IsShowBubbleSizes ? "1" : "0;");
		stringBuilder.Append(chartTextRecord.IsShowLabel ? "1" : "0;");
		stringBuilder.Append((int)chartTextRecord.HorzAlign + ";");
		stringBuilder.Append((int)chartTextRecord.VertAlign + ";");
		stringBuilder.Append((int)chartTextRecord.BackgroundMode + ";");
		stringBuilder.Append((int)chartTextRecord.ColorIndex + ";");
		stringBuilder.Append((int)chartTextRecord.Rotation + ";");
		stringBuilder.Append((int)chartTextRecord.DataLabelPlacement + ";");
		stringBuilder.Append(chartTextRecord.TextColor + ";");
		stringBuilder.Append(chartTextRecord.XPos + ";");
		stringBuilder.Append(chartTextRecord.YPos + ";");
		stringBuilder.Append(chartTextRecord.XSize + ";");
		stringBuilder.Append(chartTextRecord.YSize + ";");
		stringBuilder.Append(chartTextRecord.Options + ";");
		stringBuilder.Append(chartTextRecord.Options2 + ";");
		stringBuilder.Append(chartTextRecord.TextRotation + ";");
		stringBuilder.Append(chartTextRecord.MinimumRecordSize + ";");
		stringBuilder.Append(chartTextRecord.MaximumRecordSize + ";");
		return stringBuilder;
	}

	internal StringBuilder GetChartBorderAsString(ChartBorderImpl chartBorderImpl)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(chartBorderImpl.HasGradientFill ? "1" : "0;");
		stringBuilder.Append(chartBorderImpl.HasLineProperties ? "1" : "0;");
		stringBuilder.Append(chartBorderImpl.AutoFormat ? "1" : "0;");
		stringBuilder.Append(chartBorderImpl.DrawTickLabels ? "1" : "0;");
		stringBuilder.Append(chartBorderImpl.IsAutoLineColor ? "1" : "0;");
		stringBuilder.Append((int)chartBorderImpl.BeginArrowType + ";");
		stringBuilder.Append((int)chartBorderImpl.EndArrowType + ";");
		stringBuilder.Append((int)chartBorderImpl.BeginArrowSize + ";");
		stringBuilder.Append((int)chartBorderImpl.EndArrowSize + ";");
		stringBuilder.Append((int)chartBorderImpl.LinePattern + ";");
		stringBuilder.Append((int)chartBorderImpl.LineWeight + ";");
		stringBuilder.Append((int)chartBorderImpl.JoinType + ";");
		stringBuilder.Append((int)chartBorderImpl.ColorIndex + ";");
		stringBuilder.Append(chartBorderImpl.Transparency + ";");
		stringBuilder.Append(chartBorderImpl.LineWeightString + ";");
		stringBuilder.Append(chartBorderImpl.LineColor.ToArgb() + ";");
		if (chartBorderImpl.Color != null)
		{
			stringBuilder.Append(GetChartColorAsString(chartBorderImpl.Color));
		}
		if (chartBorderImpl.Fill != null)
		{
			stringBuilder.Append(GetFillAsString(chartBorderImpl.Fill as ShapeFillImpl));
		}
		return stringBuilder;
	}

	internal StringBuilder GetChartGridLineAsString(ChartGridLineImpl chartGridLineImpl)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(chartGridLineImpl.HasLineProperties ? "1" : "0;");
		stringBuilder.Append(chartGridLineImpl.HasShadowProperties ? "1" : "0;");
		stringBuilder.Append(chartGridLineImpl.Has3dProperties ? "1" : "0;");
		stringBuilder.Append(chartGridLineImpl.HasInterior ? "1" : "0;");
		stringBuilder.Append((int)chartGridLineImpl.AxisLineType + ";");
		if (chartGridLineImpl.ThreeD != null)
		{
			stringBuilder.Append(GetThreeDFormatAsString(chartGridLineImpl.ThreeD as ThreeDFormatImpl));
		}
		if (chartGridLineImpl.Border != null)
		{
			stringBuilder.Append(GetChartBorderAsString(chartGridLineImpl.Border as ChartBorderImpl));
		}
		if (chartGridLineImpl.LineProperties != null)
		{
			stringBuilder.Append(GetChartBorderAsString(chartGridLineImpl.LineProperties as ChartBorderImpl));
		}
		if (chartGridLineImpl.Shadow != null)
		{
			stringBuilder.Append(GetShadowAsString(chartGridLineImpl.Shadow as ShadowImpl));
		}
		if (chartGridLineImpl.Interior != null)
		{
			stringBuilder.Append(GetChartInteriorAsString(chartGridLineImpl.Interior as ChartInteriorImpl));
		}
		if (chartGridLineImpl.Fill != null)
		{
			stringBuilder.Append(GetFillAsString(chartGridLineImpl.Fill as ShapeFillImpl));
		}
		return stringBuilder;
	}

	internal StringBuilder GetPlotGrowthRecordAsString(ChartPlotGrowthRecord chartPlotGrowthRecord)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(chartPlotGrowthRecord.NeedInfill ? "1" : "0;");
		stringBuilder.Append(chartPlotGrowthRecord.NeedDataArray ? "1" : "0;");
		stringBuilder.Append(chartPlotGrowthRecord.IsAllowShortData ? "1" : "0;");
		stringBuilder.Append(chartPlotGrowthRecord.NeedDecoding ? "1" : "0;");
		stringBuilder.Append(chartPlotGrowthRecord.RecordCode + ";");
		stringBuilder.Append(chartPlotGrowthRecord.Length + ";");
		stringBuilder.Append(chartPlotGrowthRecord.StreamPos + ";");
		stringBuilder.Append(chartPlotGrowthRecord.MinimumRecordSize + ";");
		stringBuilder.Append(chartPlotGrowthRecord.MaximumRecordSize + ";");
		stringBuilder.Append(chartPlotGrowthRecord.MaximumMemorySize + ";");
		stringBuilder.Append(chartPlotGrowthRecord.StartDecodingOffset + ";");
		stringBuilder.Append(chartPlotGrowthRecord.HorzGrowth + ";");
		stringBuilder.Append(chartPlotGrowthRecord.VertGrowth + ";");
		return stringBuilder;
	}

	internal StringBuilder GetChartPlotAreaLayoutRecordAsString(ChartPlotAreaLayoutRecord chartPlotAreaLayoutRecord)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(chartPlotAreaLayoutRecord.NeedInfill ? "1" : "0;");
		stringBuilder.Append(chartPlotAreaLayoutRecord.NeedDataArray ? "1" : "0;");
		stringBuilder.Append(chartPlotAreaLayoutRecord.IsAllowShortData ? "1" : "0;");
		stringBuilder.Append(chartPlotAreaLayoutRecord.NeedDecoding ? "1" : "0;");
		stringBuilder.Append((int)chartPlotAreaLayoutRecord.WXMode + ";");
		stringBuilder.Append((int)chartPlotAreaLayoutRecord.WYMode + ";");
		stringBuilder.Append((int)chartPlotAreaLayoutRecord.WWidthMode + ";");
		stringBuilder.Append((int)chartPlotAreaLayoutRecord.WHeightMode + ";");
		stringBuilder.Append(chartPlotAreaLayoutRecord.RecordCode + ";");
		stringBuilder.Append(chartPlotAreaLayoutRecord.Length + ";");
		stringBuilder.Append(chartPlotAreaLayoutRecord.StreamPos + ";");
		stringBuilder.Append(chartPlotAreaLayoutRecord.MinimumRecordSize + ";");
		stringBuilder.Append(chartPlotAreaLayoutRecord.MaximumRecordSize + ";");
		stringBuilder.Append(chartPlotAreaLayoutRecord.MaximumMemorySize + ";");
		stringBuilder.Append(chartPlotAreaLayoutRecord.StartDecodingOffset + ";");
		stringBuilder.Append(chartPlotAreaLayoutRecord.xTL + ";");
		stringBuilder.Append(chartPlotAreaLayoutRecord.yTL + ";");
		stringBuilder.Append(chartPlotAreaLayoutRecord.xBR + ";");
		stringBuilder.Append(chartPlotAreaLayoutRecord.yBR + ";");
		stringBuilder.Append(chartPlotAreaLayoutRecord.X + ";");
		stringBuilder.Append(chartPlotAreaLayoutRecord.Y + ";");
		stringBuilder.Append(chartPlotAreaLayoutRecord.Dx + ";");
		stringBuilder.Append(chartPlotAreaLayoutRecord.Dy + ";");
		return stringBuilder;
	}

	internal bool Compare(ChartImpl chart)
	{
		if ((chart == null && this != null) || (chart != null && this == null))
		{
			return false;
		}
		if (chart != null && this != null)
		{
			if (chart.HasCodeName != base.HasCodeName || chart.ProtectContents != ProtectContents || chart.ProtectDrawingObjects != ProtectDrawingObjects || chart.ProtectScenarios != ProtectScenarios || chart.IsPasswordProtected != base.IsPasswordProtected || chart.IsParsed != base.IsParsed || chart.IsParsing != base.IsParsing || chart.IsSkipParsing != base.IsSkipParsing || chart.IsSupported != base.IsSupported || chart.DefaultGridlineColor != base.DefaultGridlineColor || chart.IsRightToLeft != base.IsRightToLeft || chart.IsSelected != base.IsSelected || chart.UnknownVmlShapes != base.UnknownVmlShapes || chart.ContainsProtection != ContainsProtection || chart.IsTransitionEvaluation != base.IsTransitionEvaluation || chart.ParseOnDemand != base.ParseOnDemand || chart.HasTabColorRGB != base.HasTabColorRGB || chart.ParseDataOnDemand != ParseDataOnDemand || chart.HasExternalWorkbook != HasExternalWorkbook || chart.ShowGapWidth != ShowGapWidth || chart.ShowAllFieldButtons != ShowAllFieldButtons || chart.ShowValueFieldButtons != ShowValueFieldButtons || chart.ShowAxisFieldButtons != ShowAxisFieldButtons || chart.ShowLegendFieldButtons != ShowLegendFieldButtons || chart.ShowReportFilterFieldButtons != ShowReportFilterFieldButtons || chart.IsSeriesInRows != IsSeriesInRows || chart.IsPrimarySecondaryCategory != IsPrimarySecondaryCategory || chart.IsPrimarySecondaryValue != IsPrimarySecondaryValue || chart.HasChartArea != HasChartArea || chart.HasPlotArea != HasPlotArea || chart.HasDataTable != HasDataTable || chart.HasLegend != HasLegend || chart.HasTitle != HasTitle || chart.ShowPlotVisible != ShowPlotVisible || chart.ProtectDrawingObjects != ProtectDrawingObjects || chart.ProtectScenarios != ProtectScenarios || chart.IsChartCleared != IsChartCleared || chart.ChartTitleIncludeInLayout != ChartTitleIncludeInLayout || chart.IsChartExternalRelation != IsChartExternalRelation || chart.IsPivotChart3D != IsPivotChart3D || chart.IsChartWalls != IsChartWalls || chart.TypeChanging != TypeChanging || chart.HasFloor != HasFloor || chart.HasWalls != HasWalls || chart.IsEmbeded != IsEmbeded || chart.HasTitleInternal != HasTitleInternal || chart.ParseDataOnDemand != ParseDataOnDemand || chart.Name != base.Name || chart.CodeName != base.CodeName || chart.AlgorithmName != base.AlgorithmName || chart.Zoom != base.Zoom || chart.Index != base.Index || chart.TopVisibleRow != base.TopVisibleRow || chart.LeftVisibleColumn != base.LeftVisibleColumn || chart.SpinCount != base.SpinCount || chart.RealIndex != base.RealIndex || chart.ReferenceCount != base.ReferenceCount || chart.OverLap != OverLap || chart.GapWidth != GapWidth || chart.FormatId != FormatId || chart.PreservedPivotSource != PreservedPivotSource || chart.XPos != XPos || chart.YPos != YPos || chart.Width != Width || chart.Height != Height || chart.EMUHeight != EMUHeight || chart.EMUWidth != EMUWidth || chart.CategoryFormula != CategoryFormula || chart.ChartExTitlePosition != ChartExTitlePosition || chart.ChartExRelationId != ChartExRelationId || chart.LineChartCount != LineChartCount || chart.ActiveSheetIndex != ActiveSheetIndex || chart.Style != Style || chart.DefaultTextIndex != DefaultTextIndex || chart.RadarStyle != RadarStyle || chart.EMUHeight != EMUHeight || chart.EMUHeight != EMUHeight || chart.TabColor != TabColor || chart.GridLineColor != base.GridLineColor || chart.InnerProtection != InnerProtection || chart.UnprotectedOptions != UnprotectedOptions || chart.Visibility != base.Visibility || chart.DefaultProtectionOptions != DefaultProtectionOptions || chart.PivotChartType != PivotChartType || chart.SeriesNameLevel != SeriesNameLevel || chart.CategoryLabelLevel != CategoryLabelLevel || chart.DestinationType != DestinationType || chart.Protection != Protection || chart.CategoryLabelValues.Count() != CategoryLabelValues.Count() || chart.AutoUpdate != AutoUpdate || chart.HasAutoTitle != HasAutoTitle)
			{
				return false;
			}
			if ((chart.DataRange == null && DataRange != null) || (chart.DataRange != null && DataRange == null) || (chart.ChartArea == null && ChartArea != null) || (chart.ChartArea != null && ChartArea == null) || (chart.PlotArea == null && PlotArea != null) || (chart.PlotArea != null && PlotArea == null) || (chart.ChartTitleArea == null && ChartTitleArea != null) || (chart.ChartTitleArea != null && ChartTitleArea == null) || (chart.DataTable == null && DataTable != null) || (chart.DataTable != null && DataTable == null) || (chart.InnerChartArea == null && InnerChartArea != null) || (chart.InnerChartArea != null && InnerChartArea == null) || (chart.InnerPlotArea == null && InnerPlotArea != null) || (chart.InnerPlotArea != null && InnerPlotArea == null) || (chart.PlotAreaBoundingBox == null && PlotAreaBoundingBox != null) || (chart.PlotAreaBoundingBox != null && PlotAreaBoundingBox == null) || (chart.Categories == null && Categories != null) || (chart.Categories != null && Categories == null) || (chart.ChartTitleFont == null && ChartTitleFont != null) || (chart.ChartTitleFont != null && ChartTitleFont == null) || (chart.Font == null && Font != null) || (chart.Font != null && Font == null) || (chart.PrimaryCategoryAxis == null && PrimaryCategoryAxis != null) || (chart.PrimaryCategoryAxis != null && PrimaryCategoryAxis == null) || (chart.SecondaryCategoryAxis == null && SecondaryCategoryAxis != null) || (chart.SecondaryCategoryAxis != null && SecondaryCategoryAxis == null) || (chart.PrimaryValueAxis == null && PrimaryValueAxis != null) || (chart.PrimaryValueAxis != null && PrimaryValueAxis == null) || (chart.SecondaryValueAxis == null && SecondaryValueAxis != null) || (chart.SecondaryValueAxis != null && SecondaryValueAxis == null) || (chart.Series == null && Series != null) || (chart.Series != null && Series == null) || (chart.PrimaryFormats == null && PrimaryFormats != null) || (chart.PrimaryFormats != null && PrimaryFormats == null) || (chart.SecondaryFormats == null && SecondaryFormats != null) || (chart.SecondaryFormats != null && SecondaryFormats == null) || (chart.Legend == null && Legend != null) || (chart.Legend != null && Legend == null) || (chart.PlotGrowth == null && PlotGrowth != null) || (chart.PlotGrowth != null && PlotGrowth == null) || (chart.PlotAreaBoundingBox == null && PlotAreaBoundingBox != null) || (chart.PlotAreaBoundingBox != null && PlotAreaBoundingBox == null) || (chart.ChartProperties == null && ChartProperties != null) || (chart.ChartProperties != null && ChartProperties == null) || (chart.ChartFormat == null && ChartFormat != null) || (chart.ChartFormat != null && ChartFormat == null) || (chart.PlotAreaLayout == null && PlotAreaLayout != null) || (chart.PlotAreaLayout != null && PlotAreaLayout == null) || (chart.SerializedAxisIds != null && SerializedAxisIds == null) || (chart.SerializedAxisIds == null && SerializedAxisIds != null) || (chart.Workbook != null && base.Workbook == null && chart.Workbook == null && base.Workbook != null) || (chart.Application != null && base.Application == null && chart.Application == null && base.Application != null) || (PrimaryParentAxis != null && chart.PrimaryParentAxis == null) || (PrimaryParentAxis == null && chart.PrimaryParentAxis != null) || (chart.ParentWorkbook != null && base.ParentWorkbook == null) || (chart.ParentWorkbook == null && base.ParentWorkbook != null) || (chart.m_secondaryParentAxis != null && m_secondaryParentAxis == null) || (chart.m_secondaryParentAxis == null && m_secondaryParentAxis != null))
			{
				return false;
			}
			if (chart.Series != null && Series != null && Series.Count != 0 && chart.Series.Count != 0)
			{
				if (chart.RightAngleAxes != RightAngleAxes || chart.AutoScaling != AutoScaling || chart.WallsAndGridlines2D != WallsAndGridlines2D || chart.IsSeriesInRows != IsSeriesInRows || chart.IsPrimarySecondaryCategory != IsPrimarySecondaryCategory || chart.IsPrimarySecondaryValue != IsPrimarySecondaryValue || chart.SupportWallsAndFloor != SupportWallsAndFloor || chart.IsTreeMapOrSunBurst != IsTreeMapOrSunBurst || chart.IsHistogramOrPareto != IsHistogramOrPareto || chart.IsCategoryAxisAvail != IsCategoryAxisAvail || chart.IsValueAxisAvail != IsValueAxisAvail || chart.IsSeriesAxisAvail != IsSeriesAxisAvail || chart.IsStacked != IsStacked || chart.IsChart_100 != IsChart_100 || chart.IsChart3D != IsChart3D || chart.IsChartLine != IsChartLine || chart.NeedDataFormat != NeedDataFormat || chart.NeedMarkerFormat != NeedMarkerFormat || chart.IsChartCone != IsChartCone || chart.IsChartBar != IsChartBar || chart.IsChartPyramid != IsChartPyramid || chart.IsChartDoughnut != IsChartDoughnut || chart.IsChartBubble != IsChartBubble || chart.IsChartVaryColor != IsChartVaryColor || chart.IsChartExploded != IsChartExploded || chart.IsSeriesLines != IsSeriesLines || chart.CanChartHaveSeriesLines != CanChartHaveSeriesLines || chart.IsChartScatter != IsChartScatter || chart.IsChartSmoothedLine != IsChartSmoothedLine || chart.IsChartStock != IsChartStock || chart.NeedDropBar != NeedDropBar || chart.IsChartVolume != IsChartVolume || chart.IsPerspective != IsPerspective || chart.IsClustered != IsClustered || chart.NoPlotArea != NoPlotArea || chart.IsChartRadar != IsChartRadar || chart.IsChartPie != IsChartPie || chart.IsChartWalls != IsChartWalls || chart.IsChartFloor != IsChartFloor || chart.IsSecondaryCategoryAxisAvail != IsSecondaryCategoryAxisAvail || chart.IsSecondaryValueAxisAvail != IsSecondaryValueAxisAvail || chart.IsSecondaryAxes != IsSecondaryAxes || chart.IsSpecialDataLabels != IsSpecialDataLabels || chart.CanChartPercentageLabel != CanChartPercentageLabel || chart.CanChartBubbleLabel != CanChartBubbleLabel || chart.IsManuallyFormatted != IsManuallyFormatted || chart.Loading != Loading || chart.DefaultLinePattern != DefaultLinePattern || chart.Rotation != Rotation || chart.Elevation != Elevation || chart.Perspective != Perspective || chart.PreservedPivotSource != PreservedPivotSource || chart.HeightPercent != HeightPercent || chart.DepthPercent != DepthPercent || chart.GapDepth != GapDepth || chart.CategoryAxisTitle != CategoryAxisTitle || chart.ValueAxisTitle != ValueAxisTitle || chart.ChartStartType != ChartStartType || chart.VmlShapesCount != base.VmlShapesCount || chart.IsSeriesAxisAvail != IsSeriesAxisAvail || chart.Loading != Loading)
				{
					return false;
				}
				if ((chart.m_iFirstRow != -1 || chart.m_iLastRow != -1) && (chart.FirstRow != FirstRow || chart.FirstColumn != FirstColumn || chart.LastRow != LastRow || chart.LastColumn != LastColumn))
				{
					return false;
				}
				if (chart.SecondaryCategoryAxis != null && SecondaryCategoryAxis != null && chart.m_secondaryParentAxis != null && m_secondaryParentAxis != null && (chart.SecondaryCategoryAxisTitle != SecondaryCategoryAxisTitle || chart.SecondaryValueAxisTitle != SecondaryValueAxisTitle))
				{
					return false;
				}
				if (IsSeriesAxisAvail && Loading && chart.IsSeriesAxisAvail && chart.Loading)
				{
					if ((chart.PrimarySerieAxis == null && PrimarySerieAxis != null) || (chart.PrimarySerieAxis != null && PrimarySerieAxis == null))
					{
						return false;
					}
					if (PrimarySerieAxis != null && chart.PrimarySerieAxis != null)
					{
						if (chart.SeriesAxisTitle != SeriesAxisTitle)
						{
							return false;
						}
						if (!CompareChartSeriesAxis(chart.PrimarySerieAxis as ChartSeriesAxisImpl, PrimarySerieAxis as ChartSeriesAxisImpl))
						{
							return false;
						}
					}
				}
				if (!CompareChartSeries(chart.Series as ChartSeriesCollection, Series as ChartSeriesCollection) || !CompareChartFormatAsString(chart.ChartFormat, ChartFormat))
				{
					return false;
				}
			}
			if (chart.SerializedAxisIds != null && SerializedAxisIds != null && chart.SerializedAxisIds.Count == SerializedAxisIds.Count)
			{
				for (int i = 0; i < SerializedAxisIds.Count; i++)
				{
					if (chart.SerializedAxisIds[i] != SerializedAxisIds[i])
					{
						return false;
					}
				}
			}
			if (chart.ChartProperties != null && ChartProperties != null && (chart.PlotVisibleOnly != PlotVisibleOnly || chart.SizeWithWindow != SizeWithWindow || chart.ZoomToFit != ZoomToFit || chart.DisplayBlanksAs != DisplayBlanksAs))
			{
				return false;
			}
			if (m_book != null && chart.m_book != null)
			{
				if (m_book.IsWorkbookOpening && Array.IndexOf(DEF_WALLS_OR_FLOOR_TYPES, ChartType) == -1 && chart.m_book.IsWorkbookOpening && Array.IndexOf(DEF_WALLS_OR_FLOOR_TYPES, ChartType) == -1)
				{
					if ((chart.Walls == null && Walls != null) || (chart.Walls != null && Walls == null) || (chart.SideWall == null && SideWall != null) || (chart.SideWall != null && SideWall == null) || (chart.BackWall == null && BackWall != null) || (chart.BackWall != null && BackWall == null))
					{
						return false;
					}
					if (!CompareWallOrFloor(chart.Walls as ChartWallOrFloorImpl, Walls as ChartWallOrFloorImpl) || !CompareWallOrFloor(chart.SideWall as ChartWallOrFloorImpl, SideWall as ChartWallOrFloorImpl) || !CompareWallOrFloor(chart.BackWall as ChartWallOrFloorImpl, BackWall as ChartWallOrFloorImpl))
					{
						return false;
					}
				}
				if (m_book.IsWorkbookOpening && chart.m_book.IsWorkbookOpening && SupportWallsAndFloor && chart.SupportWallsAndFloor)
				{
					if ((chart.Floor == null && Floor != null) || (chart.Floor != null && Floor == null))
					{
						return false;
					}
					if (!CompareWallOrFloor(chart.Floor as ChartWallOrFloorImpl, Floor as ChartWallOrFloorImpl))
					{
						return false;
					}
				}
			}
			if (chart.Workbook != null && base.Workbook != null && !CompareDataRange(chart.DataRange as ChartDataRange, DataRange as ChartDataRange))
			{
				return false;
			}
			if (chart.PrimaryParentAxis != null && PrimaryParentAxis != null && (!CompareChartCategoryAxis(chart.PrimaryCategoryAxis as ChartCategoryAxisImpl, PrimaryCategoryAxis as ChartCategoryAxisImpl) || !CompareChartValueAxis(chart.PrimaryValueAxis as ChartValueAxisImpl, PrimaryValueAxis as ChartValueAxisImpl) || !CompareChartFormatCollectionAsString(chart.PrimaryFormats, PrimaryFormats)))
			{
				return false;
			}
			if (chart.SecondaryParentAxis != null && SecondaryParentAxis != null && (!CompareChartCategoryAxis(chart.SecondaryCategoryAxis as ChartCategoryAxisImpl, SecondaryCategoryAxis as ChartCategoryAxisImpl) || !CompareChartValueAxis(chart.SecondaryValueAxis as ChartValueAxisImpl, SecondaryValueAxis as ChartValueAxisImpl) || !CompareChartFormatCollectionAsString(chart.SecondaryFormats, SecondaryFormats)))
			{
				return false;
			}
			if (chart.Application != null && base.Application != null && (!CompareChartTextArea(chart.ChartTitleArea as ChartTextAreaImpl, ChartTitleArea as ChartTextAreaImpl) || !CompareChartTextArea(chart.ChartTitleFont as ChartTextAreaImpl, ChartTitleFont as ChartTextAreaImpl) || !CompareChartFrameFormatImpl(chart.ChartArea as ChartFrameFormatImpl, ChartArea as ChartFrameFormatImpl) || !CompareChartFrameFormatImpl(chart.InnerChartArea, InnerChartArea) || !CompareChartFrameFormatImpl(chart.InnerPlotArea, InnerPlotArea)))
			{
				return false;
			}
			if (chart.ParentWorkbook != null && base.ParentWorkbook != null && !CompareFontWrapper(chart.Font as FontWrapper, Font as FontWrapper))
			{
				return false;
			}
			if (!CompareChartFrameFormat(chart.PlotArea as ChartPlotAreaImpl, PlotArea as ChartPlotAreaImpl) || !CompareChartDataTable(chart.DataTable, DataTable) || !CompareChartPosRecord(chart.PlotAreaBoundingBox, PlotAreaBoundingBox) || !CompareChartCategories(chart.Categories as ChartCategoryCollection, Categories as ChartCategoryCollection) || !CompareChartLegend(chart.Legend as ChartLegendImpl, Legend as ChartLegendImpl) || !ComparePlotGrowthRecord(chart.PlotGrowth, PlotGrowth) || !CompareChartPosRecord(chart.PlotAreaBoundingBox, PlotAreaBoundingBox) || !CompareChartShtpropsRecordAsString(chart.ChartProperties, ChartProperties) || !CompareChartPlotAreaLayoutRecord(chart.PlotAreaLayout, PlotAreaLayout))
			{
				return false;
			}
		}
		return true;
	}

	internal bool CompareDataRange(ChartDataRange dataRange1, ChartDataRange dataRange2)
	{
		if (dataRange1 != null && dataRange2 != null)
		{
			if (dataRange1.LastRow - dataRange1.FirstRow != dataRange2.LastRow - dataRange2.FirstRow || dataRange1.LastColumn - dataRange1.FirstColumn != dataRange2.LastColumn - dataRange2.FirstColumn || dataRange1.Count != dataRange2.Count)
			{
				return false;
			}
			int num = dataRange1.FirstRow;
			int num2 = dataRange2.FirstRow;
			while (num <= dataRange1.LastRow && num2 <= dataRange2.LastRow)
			{
				int num3 = dataRange1.FirstColumn;
				int num4 = dataRange2.FirstColumn;
				while (num3 <= dataRange1.LastColumn && num4 <= dataRange2.LastColumn)
				{
					dataRange1.GetValue(num, num3);
					dataRange2.GetValue(num, num3);
					if (dataRange1.GetValue(num, num3).ToString() != dataRange2.GetValue(num2, num4).ToString())
					{
						return false;
					}
					num3++;
					num4++;
				}
				num++;
				num2++;
			}
			if ((dataRange1.SheetImpl != null && dataRange2.SheetImpl == null) || (dataRange1.SheetImpl == null && dataRange2.SheetImpl != null))
			{
				return false;
			}
			if (!CompareWorksheetImpl(dataRange1.SheetImpl, dataRange2.SheetImpl))
			{
				return false;
			}
		}
		return true;
	}

	internal bool CompareWorksheetImpl(WorksheetImpl sheet1, WorksheetImpl sheet2)
	{
		if (sheet1 != null && sheet2 != null && (sheet1.HasSheetCalculation != sheet2.HasSheetCalculation || sheet1.HasAlternateContent != sheet2.HasAlternateContent || sheet1.UseRangesCache != sheet2.UseRangesCache || sheet1.IsVisible != sheet2.IsVisible || sheet1.IsZeroHeight != sheet2.IsZeroHeight || sheet1.IsThickBottom != sheet2.IsThickBottom || sheet1.IsThickTop != sheet2.IsThickTop || sheet1.CustomHeight != sheet2.CustomHeight || sheet1.HasMergedCells != sheet2.HasMergedCells || sheet1.ParseDataOnDemand != sheet2.ParseDataOnDemand || sheet1.DisplayPageBreaks != sheet2.DisplayPageBreaks || sheet1.IsDisplayZeros != sheet2.IsDisplayZeros || sheet1.IsGridLinesVisible != sheet2.IsGridLinesVisible || sheet1.IsRowColumnHeadersVisible != sheet2.IsRowColumnHeadersVisible || sheet1.IsStringsPreserved != sheet2.IsStringsPreserved || sheet1.IsFreezePanes != sheet2.IsFreezePanes || sheet1.StandardHeightFlag != sheet2.StandardHeightFlag || sheet1.IsEmpty != sheet2.IsEmpty || sheet1.UsedRangeIncludesFormatting != sheet2.UsedRangeIncludesFormatting || sheet1.UsedRangeIncludesCF != sheet2.UsedRangeIncludesCF || sheet1.ProtectContents != sheet2.ProtectContents || sheet1.IsImporting != sheet2.IsImporting || sheet1.ContainsProtection != sheet2.ContainsProtection || sheet1.View != sheet2.View || sheet1.Version != sheet2.Version || sheet1.Type != sheet2.Type || sheet1.ArchiveItemName != sheet2.ArchiveItemName || sheet1.QuotedName != sheet2.QuotedName || sheet1.DefaultColumnWidth != sheet2.DefaultColumnWidth || sheet1.VerticalSplit != sheet2.VerticalSplit || sheet1.HorizontalSplit != sheet2.HorizontalSplit || sheet1.FirstVisibleRow != sheet2.FirstVisibleRow || sheet1.MaxColumnWidth != sheet2.MaxColumnWidth || sheet1.FirstVisibleColumn != sheet2.FirstVisibleColumn || sheet1.SelectionCount != sheet2.SelectionCount || sheet1.DefaultRowHeight != sheet2.DefaultRowHeight || sheet1.BaseColumnWidth != sheet2.BaseColumnWidth || sheet1.OutlineLevelColumn != sheet2.OutlineLevelColumn || sheet1.OutlineLevelRow != sheet2.OutlineLevelRow || sheet1.RowsOutlineLevel != sheet2.RowsOutlineLevel || sheet1.ColumnsOutlineLevel != sheet2.ColumnsOutlineLevel || sheet1.ActivePane != sheet2.ActivePane || sheet1.StandardHeight != sheet2.StandardHeight || sheet1.StandardWidth != sheet2.StandardWidth))
		{
			return false;
		}
		return true;
	}

	internal bool CompareChartTextArea(ChartTextAreaImpl chartTextArea1, ChartTextAreaImpl chartTextArea2)
	{
		if (chartTextArea1 != null && chartTextArea2 != null)
		{
			if (chartTextArea1.Bold != chartTextArea2.Bold || chartTextArea1.Italic != chartTextArea2.Italic || chartTextArea1.MacOSOutlineFont != chartTextArea2.MacOSOutlineFont || chartTextArea1.MacOSShadow != chartTextArea2.MacOSShadow || chartTextArea1.Strikethrough != chartTextArea2.Strikethrough || chartTextArea1.Subscript != chartTextArea2.Subscript || chartTextArea1.Superscript != chartTextArea2.Superscript || chartTextArea1.IsBaselineWithPercentage != chartTextArea2.IsBaselineWithPercentage || chartTextArea1.HasTextRotation != chartTextArea2.HasTextRotation || chartTextArea1.ContainDataLabels != chartTextArea2.ContainDataLabels || chartTextArea1.IsAutoMode != chartTextArea2.IsAutoMode || chartTextArea1.IsTrend != chartTextArea2.IsTrend || chartTextArea1.IsAutoColor != chartTextArea2.IsAutoColor || chartTextArea1.Overlay != chartTextArea2.Overlay || chartTextArea1.ShowTextProperties != chartTextArea2.ShowTextProperties || chartTextArea1.ShowSizeProperties != chartTextArea2.ShowSizeProperties || chartTextArea1.ShowBoldProperties != chartTextArea2.ShowBoldProperties || chartTextArea1.HasText != chartTextArea2.HasText || chartTextArea1.IsFormula != chartTextArea2.IsFormula || chartTextArea1.IsTextParsed != chartTextArea2.IsTextParsed || chartTextArea1.IsValueFromCells != chartTextArea2.IsValueFromCells || chartTextArea1.IsSeriesName != chartTextArea2.IsSeriesName || chartTextArea1.IsCategoryName != chartTextArea2.IsCategoryName || chartTextArea1.IsValue != chartTextArea2.IsValue || chartTextArea1.IsPercentage != chartTextArea2.IsPercentage || chartTextArea1.IsBubbleSize != chartTextArea2.IsBubbleSize || chartTextArea1.IsLegendKey != chartTextArea2.IsLegendKey || chartTextArea1.IsShowLabelPercent != chartTextArea2.IsShowLabelPercent || chartTextArea1.Color != chartTextArea2.Color || chartTextArea1.Underline != chartTextArea2.Underline || chartTextArea1.VerticalAlignment != chartTextArea2.VerticalAlignment || chartTextArea1.TextRotation != chartTextArea2.TextRotation || chartTextArea1.BackgroundMode != chartTextArea2.BackgroundMode || chartTextArea1.ParagraphType != chartTextArea2.ParagraphType || chartTextArea1.Position != chartTextArea2.Position || chartTextArea1.Size != chartTextArea2.Size || chartTextArea1.Baseline != chartTextArea2.Baseline || chartTextArea1.FontName != chartTextArea2.FontName || chartTextArea1.Text != chartTextArea2.Text || chartTextArea1.TextRotationAngle != chartTextArea2.TextRotationAngle || chartTextArea1.NumberFormat != chartTextArea2.NumberFormat || chartTextArea1.NumberFormatIndex != chartTextArea2.NumberFormatIndex || chartTextArea1.FontIndex != chartTextArea2.FontIndex || chartTextArea1.Delimiter != chartTextArea2.Delimiter || chartTextArea1.NumberFormat != chartTextArea2.NumberFormat || chartTextArea1.ReferenceCount != chartTextArea2.ReferenceCount || chartTextArea1.RGBColor != chartTextArea2.RGBColor || chartTextArea1.StringCache != chartTextArea2.StringCache)
			{
				return false;
			}
			if ((chartTextArea1.RichText != null && chartTextArea2.RichText == null) || (chartTextArea1.RichText == null && chartTextArea2.RichText != null) || (chartTextArea1.FrameFormat != null && chartTextArea2.FrameFormat == null) || (chartTextArea1.FrameFormat == null && chartTextArea2.FrameFormat != null) || (chartTextArea1.Layout != null && chartTextArea2.Layout == null) || (chartTextArea1.Layout == null && chartTextArea2.Layout != null) || (chartTextArea1.ObjectLink != null && chartTextArea2.ObjectLink == null) || (chartTextArea1.ObjectLink == null && chartTextArea2.ObjectLink != null) || (chartTextArea1.TextRecord != null && chartTextArea2.TextRecord == null) || (chartTextArea1.TextRecord == null && chartTextArea2.TextRecord != null) || (chartTextArea1.ChartAI != null && chartTextArea2.ChartAI == null) || (chartTextArea1.ChartAI == null && chartTextArea2.ChartAI != null) || (chartTextArea1.ChartAlRuns != null && chartTextArea2.ChartAlRuns == null) || (chartTextArea1.ChartAlRuns == null && chartTextArea2.ChartAlRuns != null) || (chartTextArea1.ParentWorkbook != null && chartTextArea2.ParentWorkbook == null) || (chartTextArea1.ParentWorkbook == null && chartTextArea2.ParentWorkbook != null) || (chartTextArea1.ValueFromCellsRange != null && chartTextArea2.ValueFromCellsRange == null) || (chartTextArea1.ValueFromCellsRange == null && chartTextArea2.ValueFromCellsRange != null) || (chartTextArea1.AttachedLabelLayout != null && chartTextArea2.AttachedLabelLayout == null) || (chartTextArea1.AttachedLabelLayout == null && chartTextArea2.AttachedLabelLayout != null) || (chartTextArea1.ColorObject != null && chartTextArea2.ColorObject == null) || (chartTextArea1.ColorObject == null && chartTextArea2.ColorObject != null) || (chartTextArea1.Font != null && chartTextArea2.Font == null) || (chartTextArea1.Font == null && chartTextArea2.Font != null))
			{
				return false;
			}
			if (!CompareChartRichText(chartTextArea1.RichText as ChartRichTextString, chartTextArea2.RichText as ChartRichTextString) || !CompareChartFrameFormat(chartTextArea1.FrameFormat as ChartPlotAreaImpl, chartTextArea2.FrameFormat as ChartPlotAreaImpl) || !CompareChartObjectLinkRecord(chartTextArea1.ObjectLink, chartTextArea2.ObjectLink) || !CompareChartTextRecord(chartTextArea1.TextRecord, chartTextArea2.TextRecord) || !CompareChartAIRecord(chartTextArea1.ChartAI, chartTextArea2.ChartAI) || !CompareChartAlrunsRecord(chartTextArea1.ChartAlRuns, chartTextArea2.ChartAlRuns) || !CompareChartLayout(chartTextArea1.Layout as ChartLayoutImpl, chartTextArea2.Layout as ChartLayoutImpl) || !CompareWorkBookImpl(chartTextArea1.ParentWorkbook, chartTextArea2.ParentWorkbook) || !CompareDataRange(chartTextArea1.ValueFromCellsRange as ChartDataRange, chartTextArea2.ValueFromCellsRange as ChartDataRange) || !CompareChartAttachedLabelLayoutRecord(chartTextArea1.AttachedLabelLayout, chartTextArea2.AttachedLabelLayout) || !CompareChartColor(chartTextArea1.ColorObject, chartTextArea2.ColorObject) || !CompareFont(chartTextArea1.Font, chartTextArea2.Font))
			{
				return false;
			}
		}
		return true;
	}

	internal bool CompareChartFrameFormat(ChartPlotAreaImpl chartAreaFrame1, ChartPlotAreaImpl chartAreaFrame2)
	{
		if (chartAreaFrame1 != null && chartAreaFrame2 != null)
		{
			if (chartAreaFrame1.HasInterior != chartAreaFrame2.HasInterior || chartAreaFrame1.HasLineProperties != chartAreaFrame2.HasLineProperties || chartAreaFrame1.HasShadowProperties != chartAreaFrame2.HasShadowProperties || chartAreaFrame1.Has3dProperties != chartAreaFrame2.Has3dProperties || chartAreaFrame1.IsAutoSize != chartAreaFrame2.IsAutoSize || chartAreaFrame1.IsAutoPosition != chartAreaFrame2.IsAutoPosition || chartAreaFrame1.IsBorderCornersRound != chartAreaFrame2.IsBorderCornersRound || chartAreaFrame1.IsAutomaticFormat != chartAreaFrame2.IsAutomaticFormat || chartAreaFrame1.Visible != chartAreaFrame2.Visible || chartAreaFrame1.RectangleStyle != chartAreaFrame2.RectangleStyle || chartAreaFrame1.Pattern != chartAreaFrame2.Pattern)
			{
				return false;
			}
			if ((chartAreaFrame1.Layout != null && chartAreaFrame2.Layout == null) || (chartAreaFrame1.Layout == null && chartAreaFrame2.Layout != null) || (chartAreaFrame1.Border != null && chartAreaFrame2.Border == null) || (chartAreaFrame1.Border == null && chartAreaFrame2.Border != null) || (chartAreaFrame1.Interior != null && chartAreaFrame2.Interior == null) || (chartAreaFrame1.Interior == null && chartAreaFrame2.Interior != null) || (chartAreaFrame1.ThreeD != null && chartAreaFrame2.ThreeD == null) || (chartAreaFrame1.ThreeD == null && chartAreaFrame2.ThreeD != null) || (chartAreaFrame1.Shadow != null && chartAreaFrame2.Shadow == null) || (chartAreaFrame1.Shadow == null && chartAreaFrame2.Shadow != null) || (chartAreaFrame1.Fill != null && chartAreaFrame2.Fill == null) || (chartAreaFrame1.Fill == null && chartAreaFrame2.Fill != null) || (chartAreaFrame1.LineProperties != null && chartAreaFrame2.LineProperties == null) || (chartAreaFrame1.LineProperties == null && chartAreaFrame2.LineProperties != null) || (chartAreaFrame1.ForeGroundColorObject != null && chartAreaFrame2.ForeGroundColorObject == null) || (chartAreaFrame1.ForeGroundColorObject == null && chartAreaFrame2.ForeGroundColorObject != null) || (chartAreaFrame1.BackGroundColorObject != null && chartAreaFrame2.BackGroundColorObject == null) || (chartAreaFrame1.BackGroundColorObject == null && chartAreaFrame2.BackGroundColorObject != null))
			{
				return false;
			}
			if (!CompareChartLayout(chartAreaFrame1.Layout as ChartLayoutImpl, chartAreaFrame2.Layout as ChartLayoutImpl) || !CompareChartBorder(chartAreaFrame1.Border as ChartBorderImpl, chartAreaFrame2.Border as ChartBorderImpl) || !CompareChartInterior(chartAreaFrame1.Interior as ChartInteriorImpl, chartAreaFrame2.Interior as ChartInteriorImpl) || !CompareThreeDFormat(chartAreaFrame1.ThreeD as ThreeDFormatImpl, chartAreaFrame2.ThreeD as ThreeDFormatImpl) || !CompareShadow(chartAreaFrame1.Shadow as ShadowImpl, chartAreaFrame2.Shadow as ShadowImpl) || !CompareFill(chartAreaFrame1.Fill as ShapeFillImpl, chartAreaFrame2.Fill as ShapeFillImpl) || !CompareChartBorder(chartAreaFrame1.LineProperties as ChartBorderImpl, chartAreaFrame2.LineProperties as ChartBorderImpl) || !CompareChartColor(chartAreaFrame1.ForeGroundColorObject, chartAreaFrame2.ForeGroundColorObject) || !CompareChartColor(chartAreaFrame1.BackGroundColorObject, chartAreaFrame2.BackGroundColorObject))
			{
				return false;
			}
		}
		return true;
	}

	internal bool CompareWallOrFloor(ChartWallOrFloorImpl chartWall1, ChartWallOrFloorImpl chartWall2)
	{
		if (chartWall1 != null && chartWall2 != null)
		{
			if (chartWall1.HasInterior != chartWall2.HasInterior || chartWall1.HasLineProperties != chartWall2.HasLineProperties || chartWall1.HasShadowProperties != chartWall2.HasShadowProperties || chartWall1.Has3dProperties != chartWall2.Has3dProperties || chartWall1.HasShapeProperties != chartWall2.HasShapeProperties || chartWall1.IsAutomaticFormat != chartWall2.IsAutomaticFormat || chartWall1.Visible != chartWall2.Visible || chartWall1.Thickness != chartWall2.Thickness || chartWall1.PictureUnit != chartWall2.PictureUnit || chartWall1.Pattern != chartWall2.Pattern || chartWall1.AxisLineType != chartWall2.AxisLineType)
			{
				return false;
			}
			if ((chartWall1.Interior != null && chartWall2.Interior == null) || (chartWall1.Interior == null && chartWall2.Interior != null) || (chartWall1.ThreeD != null && chartWall2.ThreeD == null) || (chartWall1.ThreeD == null && chartWall2.ThreeD != null) || (chartWall1.Shadow != null && chartWall2.Shadow == null) || (chartWall1.Shadow == null && chartWall2.Shadow != null) || (chartWall1.Fill != null && chartWall2.Fill == null) || (chartWall1.Fill == null && chartWall2.Fill != null) || (chartWall1.ForeGroundColorObject != null && chartWall2.ForeGroundColorObject == null) || (chartWall1.ForeGroundColorObject == null && chartWall2.ForeGroundColorObject != null) || (chartWall1.BackGroundColorObject != null && chartWall2.BackGroundColorObject == null) || (chartWall1.BackGroundColorObject == null && chartWall2.BackGroundColorObject != null))
			{
				return false;
			}
			if (!CompareChartInterior(chartWall1.Interior as ChartInteriorImpl, chartWall2.Interior as ChartInteriorImpl) || !CompareThreeDFormat(chartWall1.ThreeD as ThreeDFormatImpl, chartWall2.ThreeD as ThreeDFormatImpl) || !CompareShadow(chartWall1.Shadow as ShadowImpl, chartWall2.Shadow as ShadowImpl) || !CompareFill(chartWall1.Fill as ShapeFillImpl, chartWall2.Fill as ShapeFillImpl) || !CompareChartColor(chartWall1.ForeGroundColorObject, chartWall2.ForeGroundColorObject) || !CompareChartColor(chartWall1.BackGroundColorObject, chartWall2.BackGroundColorObject))
			{
				return false;
			}
		}
		return true;
	}

	internal bool CompareChartSeries(ChartSeriesCollection chartSeries1, ChartSeriesCollection chartSeries2)
	{
		if (chartSeries1 != null && chartSeries2 != null)
		{
			if (chartSeries1.QuietMode != chartSeries2.QuietMode || chartSeries1.IsReadOnly != chartSeries2.IsReadOnly || chartSeries1.Capacity != chartSeries2.Capacity || chartSeries1.Count != chartSeries2.Count || chartSeries1.TrendErrorBarIndex != chartSeries2.TrendErrorBarIndex || chartSeries1.TrendIndex != chartSeries2.TrendIndex)
			{
				return false;
			}
			if ((chartSeries1.AdditionOrder != null && chartSeries2.AdditionOrder == null) || (chartSeries1.AdditionOrder == null && chartSeries2.AdditionOrder != null) || (chartSeries1.TrendErrorList == null && chartSeries2.TrendErrorList != null) || (chartSeries1.TrendErrorList != null && chartSeries2.TrendErrorList == null) || (chartSeries1.TrendLabels != null && chartSeries2.TrendLabels == null) || (chartSeries1.TrendLabels == null && chartSeries2.TrendLabels != null))
			{
				return false;
			}
			if (chartSeries1.AdditionOrder.Count != chartSeries2.AdditionOrder.Count)
			{
				return false;
			}
			for (int i = 0; i < chartSeries1.AdditionOrder.Count; i++)
			{
				if (!CompareChartSerie(chartSeries1.AdditionOrder[i] as ChartSerieImpl, chartSeries2.AdditionOrder[i] as ChartSerieImpl))
				{
					return false;
				}
			}
			if (chartSeries1.TrendErrorList.Count != chartSeries2.TrendErrorList.Count)
			{
				return false;
			}
			for (int j = 0; j < chartSeries1.TrendErrorList.Count; j++)
			{
				if (!CompareBiffStorage(chartSeries1.TrendErrorList[j], chartSeries2.TrendErrorList[j]))
				{
					return false;
				}
			}
			if (chartSeries1.TrendLabels.Count != chartSeries2.TrendLabels.Count)
			{
				return false;
			}
			for (int k = 0; k < chartSeries1.TrendLabels.Count; k++)
			{
				if (!CompareBiffStorage(chartSeries1.TrendLabels[k], chartSeries2.TrendLabels[k]))
				{
					return false;
				}
			}
		}
		return true;
	}

	internal bool CompareChartDataTable(IOfficeChartDataTable dataTable1, IOfficeChartDataTable dataTable2)
	{
		if (dataTable1 is ChartDataTableImpl && dataTable2 is ChartDataTableImpl)
		{
			ChartDataTableImpl chartDataTableImpl = dataTable1 as ChartDataTableImpl;
			ChartDataTableImpl chartDataTableImpl2 = dataTable2 as ChartDataTableImpl;
			if (chartDataTableImpl.HasHorzBorder != chartDataTableImpl2.HasHorzBorder || chartDataTableImpl.HasVertBorder != chartDataTableImpl2.HasVertBorder || chartDataTableImpl.HasBorders != chartDataTableImpl2.HasBorders || chartDataTableImpl.ShowSeriesKeys != chartDataTableImpl2.ShowSeriesKeys)
			{
				return false;
			}
			if ((dataTable1.TextArea != null && dataTable2.TextArea == null) || (dataTable1.TextArea == null && dataTable2.TextArea != null))
			{
				return false;
			}
			if (!CompareChartTextArea(dataTable1.TextArea as ChartTextAreaImpl, dataTable2.TextArea as ChartTextAreaImpl))
			{
				return false;
			}
		}
		return true;
	}

	internal bool CompareChartPosRecord(ChartPosRecord chartPosRecord1, ChartPosRecord chartPosRecord2)
	{
		if (chartPosRecord1.NeedInfill != chartPosRecord2.NeedInfill || chartPosRecord1.NeedDataArray != chartPosRecord2.NeedDataArray || chartPosRecord1.IsAllowShortData != chartPosRecord2.IsAllowShortData || chartPosRecord1.NeedDecoding != chartPosRecord2.NeedDecoding || chartPosRecord1.TypeCode != chartPosRecord2.TypeCode || chartPosRecord1.RecordCode != chartPosRecord2.RecordCode || chartPosRecord1.Length != chartPosRecord2.Length || chartPosRecord1.StreamPos != chartPosRecord2.StreamPos || chartPosRecord1.MinimumRecordSize != chartPosRecord2.MinimumRecordSize || chartPosRecord1.MaximumRecordSize != chartPosRecord2.MaximumRecordSize || chartPosRecord1.MaximumMemorySize != chartPosRecord2.MaximumMemorySize || chartPosRecord1.StartDecodingOffset != chartPosRecord2.StartDecodingOffset || chartPosRecord1.TopLeft != chartPosRecord2.TopLeft || chartPosRecord1.BottomRight != chartPosRecord2.BottomRight || chartPosRecord1.X1 != chartPosRecord2.X1 || chartPosRecord1.X2 != chartPosRecord2.X2 || chartPosRecord1.Y1 != chartPosRecord2.Y1 || chartPosRecord1.Y2 != chartPosRecord2.Y2 || chartPosRecord1.MinimumRecordSize != chartPosRecord2.MinimumRecordSize || chartPosRecord1.MaximumRecordSize != chartPosRecord2.MaximumRecordSize)
		{
			return false;
		}
		if ((chartPosRecord1.Data != null && chartPosRecord2.Data == null) || (chartPosRecord1.Data == null && chartPosRecord2.Data != null))
		{
			return false;
		}
		if (chartPosRecord1 != null && chartPosRecord1.Data.ToString() != chartPosRecord2.Data.ToString())
		{
			return false;
		}
		return true;
	}

	internal bool CompareChartCategories(ChartCategoryCollection category1, ChartCategoryCollection category2)
	{
		if (category1 != null && category2 != null && (category1.QuietMode != category2.QuietMode || category1.IsReadOnly != category2.IsReadOnly || category1.Capacity != category2.Capacity || category1.Count != category2.Count))
		{
			return false;
		}
		return true;
	}

	internal bool CompareFont(FontImpl font1, FontImpl font2)
	{
		if (font1 != null && font2 != null && (font1.Bold != font2.Bold || font1.Italic != font2.Italic || font1.MacOSOutlineFont != font2.MacOSOutlineFont || font1.MacOSShadow != font2.MacOSShadow || font1.Strikethrough != font2.Strikethrough || font1.Subscript != font2.Subscript || font1.Superscript != font2.Superscript || font1.IsAutoColor != font2.IsAutoColor || font1.HasLatin != font2.HasLatin || font1.HasComplexScripts != font2.HasComplexScripts || font1.HasEastAsianFont != font2.HasEastAsianFont || font1.HasParagrapAlign != font2.HasParagrapAlign || font1.Color != font2.Color || font1.Underline != font2.Underline || font1.VerticalAlignment != font2.VerticalAlignment || font1.ParaAlign != font2.ParaAlign || font1.Size != font2.Size || font1.FontName != font2.FontName || font1.BaseLine != font2.BaseLine || font1.Index != font2.Index || font1.Language != font2.Language || font1.ActualFontName != font2.ActualFontName || font1.CharSet != font2.CharSet || font1.Family != font2.Family))
		{
			return false;
		}
		return true;
	}

	internal bool CompareChartCategoryAxis(ChartCategoryAxisImpl categoryAxis1, ChartCategoryAxisImpl categoryAxis2)
	{
		if (categoryAxis1 != null && categoryAxis2 != null)
		{
			if (categoryAxis1.IsAutoMin != categoryAxis2.IsAutoMin || categoryAxis1.IsAutoMax != categoryAxis2.IsAutoMax || categoryAxis1.AutoTickLabelSpacing != categoryAxis2.AutoTickLabelSpacing || categoryAxis1.IsAutoMajor != categoryAxis2.IsAutoMajor || categoryAxis1.IsAutoMinor != categoryAxis2.IsAutoMinor || categoryAxis1.IsAutoCross != categoryAxis2.IsAutoCross || categoryAxis1.IsChangeAutoCross != categoryAxis2.IsChangeAutoCross || categoryAxis1.IsChangeAutoCrossInLoading != categoryAxis2.IsChangeAutoCrossInLoading || categoryAxis1.IsLogScale != categoryAxis2.IsLogScale || categoryAxis1.ReversePlotOrder != categoryAxis2.ReversePlotOrder || categoryAxis1.IsMaxCross != categoryAxis2.IsMaxCross || categoryAxis1.HasDisplayUnitLabel != categoryAxis2.HasDisplayUnitLabel || categoryAxis1.IsWrapText != categoryAxis2.IsWrapText || categoryAxis1.IsPrimary != categoryAxis2.IsPrimary || categoryAxis1.IsAutoTextRotation != categoryAxis2.IsAutoTextRotation || categoryAxis1.IsChartFont != categoryAxis2.IsChartFont || categoryAxis1.HasMinorGridLines != categoryAxis2.HasMinorGridLines || categoryAxis1.HasMajorGridLines != categoryAxis2.HasMajorGridLines || categoryAxis1.isNumber != categoryAxis2.isNumber || categoryAxis1.Visible != categoryAxis2.Visible || categoryAxis1.Deleted != categoryAxis2.Deleted || categoryAxis1.AutoTickMarkSpacing != categoryAxis2.AutoTickMarkSpacing || categoryAxis1.HasShadowProperties != categoryAxis2.HasShadowProperties || categoryAxis1.Has3dProperties != categoryAxis2.Has3dProperties || categoryAxis1.IsSourceLinked != categoryAxis2.IsSourceLinked || categoryAxis1.HasAxisTitle != categoryAxis2.HasAxisTitle || categoryAxis1.IsDefaultTextSettings != categoryAxis2.IsDefaultTextSettings || categoryAxis1.ChangeDateTimeAxisValue != categoryAxis2.ChangeDateTimeAxisValue || categoryAxis1.HasAutoTickLabelSpacing != categoryAxis2.HasAutoTickLabelSpacing || categoryAxis1.AutoTickLabelSpacing != categoryAxis2.AutoTickLabelSpacing || categoryAxis1.IsBetween != categoryAxis2.IsBetween || categoryAxis1.ReversePlotOrder != categoryAxis2.ReversePlotOrder || categoryAxis1.BaseUnitIsAuto != categoryAxis2.BaseUnitIsAuto || categoryAxis1.MajorUnitScaleIsAuto != categoryAxis2.MajorUnitScaleIsAuto || categoryAxis1.MinorUnitScaleIsAuto != categoryAxis2.MinorUnitScaleIsAuto || categoryAxis1.IsAutoMajor != categoryAxis2.IsAutoMajor || categoryAxis1.IsAutoMinor != categoryAxis2.IsAutoMinor || categoryAxis1.IsAutoCross != categoryAxis2.IsAutoCross || categoryAxis1.IsAutoMax != categoryAxis2.IsAutoMax || categoryAxis1.IsAutoMin != categoryAxis2.IsAutoMin || categoryAxis1.NoMultiLevelLabel != categoryAxis2.NoMultiLevelLabel || categoryAxis1.IsBinningByCategory != categoryAxis2.IsBinningByCategory || categoryAxis1.HasAutomaticBins != categoryAxis2.HasAutomaticBins || categoryAxis1.IsChartBubbleOrScatter != categoryAxis2.IsChartBubbleOrScatter || categoryAxis1.AxisType != categoryAxis2.AxisType || categoryAxis1.DisplayUnit != categoryAxis2.DisplayUnit || categoryAxis1.MinorTickMark != categoryAxis2.MinorTickMark || categoryAxis1.MajorTickMark != categoryAxis2.MajorTickMark || categoryAxis1.TickLabelPosition != categoryAxis2.TickLabelPosition || categoryAxis1.Alignment != categoryAxis2.Alignment || categoryAxis1.ParagraphType != categoryAxis2.ParagraphType || categoryAxis1.LabelTextAlign != categoryAxis2.LabelTextAlign || categoryAxis1.CategoryType != categoryAxis2.CategoryType || categoryAxis1.BaseUnit != categoryAxis2.BaseUnit || categoryAxis1.MajorUnitScale != categoryAxis2.MajorUnitScale || categoryAxis1.MinorUnitScale != categoryAxis2.MinorUnitScale || categoryAxis1.MinimumValue != categoryAxis2.MinimumValue || categoryAxis1.MaximumValue != categoryAxis2.MaximumValue || categoryAxis1.MajorUnit != categoryAxis2.MajorUnit || categoryAxis1.MinorUnit != categoryAxis2.MinorUnit || categoryAxis1.CrossValue != categoryAxis2.CrossValue || categoryAxis1.CrossesAt != categoryAxis2.CrossesAt || categoryAxis1.DisplayUnitCustom != categoryAxis2.DisplayUnitCustom || categoryAxis1.Title != categoryAxis2.Title || categoryAxis1.TextRotationAngle != categoryAxis2.TextRotationAngle || categoryAxis1.NumberFormatIndex != categoryAxis2.NumberFormatIndex || categoryAxis1.NumberFormat != categoryAxis2.NumberFormat || categoryAxis1.AxisId != categoryAxis2.AxisId || categoryAxis1.LabelFrequency != categoryAxis2.LabelFrequency || categoryAxis1.TickLabelSpacing != categoryAxis2.TickLabelSpacing || categoryAxis1.TickMarksFrequency != categoryAxis2.TickMarksFrequency || categoryAxis1.TickMarkSpacing != categoryAxis2.TickMarkSpacing || categoryAxis1.Offset != categoryAxis2.Offset || categoryAxis1.NumberOfBins != categoryAxis2.NumberOfBins || categoryAxis1.BinWidth != categoryAxis2.BinWidth || categoryAxis1.UnderflowBinValue != categoryAxis2.UnderflowBinValue || categoryAxis1.OverflowBinValue != categoryAxis2.OverflowBinValue || categoryAxis1.AxisPosition != categoryAxis2.AxisPosition)
			{
				return false;
			}
			if ((categoryAxis1.HistogramAxisFormatProperty != null && categoryAxis2.HistogramAxisFormatProperty == null) || (categoryAxis1.HistogramAxisFormatProperty == null && categoryAxis2.HistogramAxisFormatProperty != null) || (categoryAxis1.CategoryLabels != null && categoryAxis2.CategoryLabels == null) || (categoryAxis1.CategoryLabels == null && categoryAxis2.CategoryLabels != null) || (categoryAxis1.FrameFormat != null && categoryAxis2.FrameFormat == null) || (categoryAxis1.FrameFormat == null && categoryAxis2.FrameFormat != null) || (categoryAxis1.Chart3DOptions != null && categoryAxis2.Chart3DOptions == null) || (categoryAxis1.Chart3DOptions == null && categoryAxis2.Chart3DOptions != null) || (categoryAxis1.Chart3DProperties != null && categoryAxis2.Chart3DProperties == null) || (categoryAxis1.Chart3DProperties == null && categoryAxis2.Chart3DProperties != null) || (categoryAxis1.Shadow != null && categoryAxis2.Shadow == null) || (categoryAxis1.Shadow == null && categoryAxis2.Shadow != null) || (categoryAxis1.ShadowProperties != null && categoryAxis2.Shadow == null) || (categoryAxis1.ShadowProperties == null && categoryAxis2.Shadow != null) || (categoryAxis1.Border != null && categoryAxis2.Border == null) || (categoryAxis1.Border == null && categoryAxis2.Border != null) || (categoryAxis1.MinorGridLines != null && categoryAxis2.MinorGridLines == null) || (categoryAxis1.MinorGridLines == null && categoryAxis2.MinorGridLines != null) || (categoryAxis1.MajorGridLines != null && categoryAxis2.MajorGridLines == null) || (categoryAxis1.MajorGridLines == null && categoryAxis2.MajorGridLines != null) || (categoryAxis1.Font != null && categoryAxis2.Font == null) || (categoryAxis1.Font == null && categoryAxis2.Font != null) || (categoryAxis1.TitleArea != null && categoryAxis2.TitleArea == null) || (categoryAxis1.TitleArea == null && categoryAxis2.TitleArea != null) || (categoryAxis1.DisplayUnitLabel != null && categoryAxis2.DisplayUnitLabel == null) || (categoryAxis1.DisplayUnitLabel == null && categoryAxis2.DisplayUnitLabel != null) || (categoryAxis1.ParentChart != null && categoryAxis2.ParentChart == null) || (categoryAxis1.ParentChart == null && categoryAxis2.ParentChart != null) || (categoryAxis1.TextStream != null && categoryAxis2.TextStream == null) || (categoryAxis1.TextStream == null && categoryAxis2.TextStream != null))
			{
				return false;
			}
			if (categoryAxis1.TextStream != null && categoryAxis2.TextStream != null && categoryAxis1.TextStream.Length != categoryAxis2.TextStream.Length)
			{
				return false;
			}
			if (!CompareHistogramAxisFormat(categoryAxis1.HistogramAxisFormatProperty, categoryAxis2.HistogramAxisFormatProperty) || !CompareDataRange(categoryAxis1.CategoryLabels as ChartDataRange, categoryAxis2.CategoryLabels as ChartDataRange) || !CompareChartFrameFormat(categoryAxis1.FrameFormat as ChartPlotAreaImpl, categoryAxis2.FrameFormat as ChartPlotAreaImpl) || !CompareThreeDFormat(categoryAxis1.Chart3DOptions as ThreeDFormatImpl, categoryAxis2.Chart3DOptions as ThreeDFormatImpl) || !CompareThreeDFormat(categoryAxis1.Chart3DProperties as ThreeDFormatImpl, categoryAxis2.Chart3DProperties as ThreeDFormatImpl) || !CompareShadow(categoryAxis1.Shadow as ShadowImpl, categoryAxis2.Shadow as ShadowImpl) || !CompareShadow(categoryAxis1.ShadowProperties as ShadowImpl, categoryAxis2.ShadowProperties as ShadowImpl) || !CompareChartBorder(categoryAxis1.Border as ChartBorderImpl, categoryAxis2.Border as ChartBorderImpl) || !CompareChartGridLine(categoryAxis1.MinorGridLines as ChartGridLineImpl, categoryAxis2.MinorGridLines as ChartGridLineImpl) || !CompareChartGridLine(categoryAxis1.MajorGridLines as ChartGridLineImpl, categoryAxis2.MajorGridLines as ChartGridLineImpl) || !CompareFont(categoryAxis1.Font as FontImpl, categoryAxis2.Font as FontImpl) || !CompareChartTextArea(categoryAxis1.TitleArea as ChartTextAreaImpl, categoryAxis2.TitleArea as ChartTextAreaImpl) || !CompareChartTextArea(categoryAxis1.DisplayUnitLabel as ChartTextAreaImpl, categoryAxis2.DisplayUnitLabel as ChartTextAreaImpl) || !CompareFill(categoryAxis1.FrameFormat.Fill as ShapeFillImpl, categoryAxis2.FrameFormat.Fill as ShapeFillImpl))
			{
				return false;
			}
		}
		return true;
	}

	internal bool CompareChartSeriesAxis(ChartSeriesAxisImpl seriesAxis1, ChartSeriesAxisImpl seriesAxis2)
	{
		if (seriesAxis1 != null && seriesAxis2 != null)
		{
			if (seriesAxis1.IsWrapText != seriesAxis2.IsWrapText || seriesAxis1.IsPrimary != seriesAxis2.IsPrimary || seriesAxis1.IsAutoTextRotation != seriesAxis2.IsAutoTextRotation || seriesAxis1.IsChartFont != seriesAxis2.IsChartFont || seriesAxis1.HasMinorGridLines != seriesAxis2.HasMinorGridLines || seriesAxis1.HasMajorGridLines != seriesAxis2.HasMajorGridLines || seriesAxis1.isNumber != seriesAxis2.isNumber || seriesAxis1.Visible != seriesAxis2.Visible || seriesAxis1.ReversePlotOrder != seriesAxis2.ReversePlotOrder || seriesAxis1.Deleted != seriesAxis2.Deleted || seriesAxis1.AutoTickLabelSpacing != seriesAxis2.AutoTickLabelSpacing || seriesAxis1.AutoTickMarkSpacing != seriesAxis2.AutoTickMarkSpacing || seriesAxis1.HasShadowProperties != seriesAxis2.HasShadowProperties || seriesAxis1.Has3dProperties != seriesAxis2.Has3dProperties || seriesAxis1.IsSourceLinked != seriesAxis2.IsSourceLinked || seriesAxis1.HasAxisTitle != seriesAxis2.HasAxisTitle || seriesAxis1.IsDefaultTextSettings != seriesAxis2.IsDefaultTextSettings || seriesAxis1.IsBetween != seriesAxis2.IsBetween || seriesAxis1.IsLogScale != seriesAxis2.IsLogScale || seriesAxis1.AxisType != seriesAxis2.AxisType || seriesAxis1.MinorTickMark != seriesAxis2.MinorTickMark || seriesAxis1.MajorTickMark != seriesAxis2.MajorTickMark || seriesAxis1.TickLabelPosition != seriesAxis2.TickLabelPosition || seriesAxis1.Alignment != seriesAxis2.Alignment || seriesAxis1.ParagraphType != seriesAxis2.ParagraphType || seriesAxis1.LabelTextAlign != seriesAxis2.LabelTextAlign || seriesAxis1.Title != seriesAxis2.Title || seriesAxis1.TextRotationAngle != seriesAxis2.TextRotationAngle || seriesAxis1.NumberFormatIndex != seriesAxis2.NumberFormatIndex || seriesAxis1.NumberFormat != seriesAxis2.NumberFormat || seriesAxis1.AxisId != seriesAxis2.AxisId || seriesAxis1.LabelFrequency != seriesAxis2.LabelFrequency || seriesAxis1.TickLabelSpacing != seriesAxis2.TickLabelSpacing || seriesAxis1.TickMarksFrequency != seriesAxis2.TickMarksFrequency || seriesAxis1.TickMarkSpacing != seriesAxis2.TickMarkSpacing || seriesAxis1.CrossesAt != seriesAxis2.CrossesAt || seriesAxis1.MaximumValue != seriesAxis2.MaximumValue || seriesAxis1.MinimumValue != seriesAxis2.MinimumValue || seriesAxis1.AxisPosition != seriesAxis2.AxisPosition)
			{
				return false;
			}
			if ((seriesAxis1.FrameFormat != null && seriesAxis2.FrameFormat == null) || (seriesAxis1.FrameFormat == null && seriesAxis2.FrameFormat != null) || (seriesAxis1.Chart3DOptions != null && seriesAxis2.Chart3DOptions == null) || (seriesAxis1.Chart3DOptions == null && seriesAxis2.Chart3DOptions != null) || (seriesAxis1.Chart3DProperties != null && seriesAxis2.Chart3DProperties == null) || (seriesAxis1.Chart3DProperties == null && seriesAxis2.Chart3DProperties != null) || (seriesAxis1.Shadow != null && seriesAxis2.Shadow == null) || (seriesAxis1.Shadow == null && seriesAxis2.Shadow != null) || (seriesAxis1.ShadowProperties != null && seriesAxis2.ShadowProperties == null) || (seriesAxis1.ShadowProperties == null && seriesAxis2.ShadowProperties != null) || (seriesAxis1.Border != null && seriesAxis2.Border == null) || (seriesAxis1.Border == null && seriesAxis2.Border != null) || (seriesAxis1.MinorGridLines != null && seriesAxis2.MinorGridLines == null) || (seriesAxis1.MinorGridLines == null && seriesAxis2.MinorGridLines != null) || (seriesAxis1.MajorGridLines != null && seriesAxis2.MajorGridLines == null) || (seriesAxis1.MajorGridLines == null && seriesAxis2.MajorGridLines != null) || (seriesAxis1.Font != null && seriesAxis2.Font == null) || (seriesAxis1.Font == null && seriesAxis2.Font != null) || (seriesAxis1.TitleArea != null && seriesAxis2.TitleArea == null) || (seriesAxis1.TitleArea == null && seriesAxis2.TitleArea != null) || (seriesAxis1.ParentChart != null && seriesAxis2.ParentChart == null) || (seriesAxis1.ParentChart == null && seriesAxis2.ParentChart != null) || (seriesAxis1.AxisPosition.HasValue && !seriesAxis2.AxisPosition.HasValue) || (!seriesAxis1.AxisPosition.HasValue && seriesAxis2.AxisPosition.HasValue))
			{
				return false;
			}
			if (seriesAxis1.AxisPosition.HasValue)
			{
				if (seriesAxis1.AxisPosition.HasValue != seriesAxis2.AxisPosition.HasValue)
				{
					return false;
				}
				if (seriesAxis1.AxisPosition.HasValue && seriesAxis1.AxisPosition.Value != seriesAxis2.AxisPosition.Value)
				{
					return false;
				}
			}
			if (!CompareChartFrameFormat(seriesAxis1.FrameFormat as ChartPlotAreaImpl, seriesAxis2.FrameFormat as ChartPlotAreaImpl) || !CompareThreeDFormat(seriesAxis1.Chart3DOptions as ThreeDFormatImpl, seriesAxis2.Chart3DOptions as ThreeDFormatImpl) || !CompareThreeDFormat(seriesAxis1.Chart3DProperties as ThreeDFormatImpl, seriesAxis2.Chart3DProperties as ThreeDFormatImpl) || !CompareShadow(seriesAxis1.Shadow as ShadowImpl, seriesAxis2.Shadow as ShadowImpl) || !seriesAxis2.ParentChart.Compare(seriesAxis1.ParentChart) || !CompareShadow(seriesAxis1.ShadowProperties as ShadowImpl, seriesAxis2.ShadowProperties as ShadowImpl) || !CompareChartBorder(seriesAxis1.Border as ChartBorderImpl, seriesAxis2.Border as ChartBorderImpl) || !CompareChartGridLine(seriesAxis1.MinorGridLines as ChartGridLineImpl, seriesAxis2.MinorGridLines as ChartGridLineImpl) || !CompareChartGridLine(seriesAxis1.MajorGridLines as ChartGridLineImpl, seriesAxis2.MajorGridLines as ChartGridLineImpl) || !CompareFont(seriesAxis1.Font as FontImpl, seriesAxis2.Font as FontImpl) || !CompareChartTextArea(seriesAxis1.TitleArea as ChartTextAreaImpl, seriesAxis2.TitleArea as ChartTextAreaImpl))
			{
				return false;
			}
		}
		return true;
	}

	internal bool CompareChartValueAxis(ChartValueAxisImpl valueAxis1, ChartValueAxisImpl valueAxis2)
	{
		if (valueAxis1 != null && valueAxis2 != null)
		{
			if (valueAxis1.IsWrapText != valueAxis2.IsWrapText || valueAxis1.IsPrimary != valueAxis2.IsPrimary || valueAxis1.IsAutoTextRotation != valueAxis2.IsAutoTextRotation || valueAxis1.IsChartFont != valueAxis2.IsChartFont || valueAxis1.HasMinorGridLines != valueAxis2.HasMinorGridLines || valueAxis1.HasMajorGridLines != valueAxis2.HasMajorGridLines || valueAxis1.isNumber != valueAxis2.isNumber || valueAxis1.Visible != valueAxis2.Visible || valueAxis1.ReversePlotOrder != valueAxis2.ReversePlotOrder || valueAxis1.Deleted != valueAxis2.Deleted || valueAxis1.AutoTickLabelSpacing != valueAxis2.AutoTickLabelSpacing || valueAxis1.AutoTickMarkSpacing != valueAxis2.AutoTickMarkSpacing || valueAxis1.HasShadowProperties != valueAxis2.HasShadowProperties || valueAxis1.Has3dProperties != valueAxis2.Has3dProperties || valueAxis1.IsSourceLinked != valueAxis2.IsSourceLinked || valueAxis1.HasAxisTitle != valueAxis2.HasAxisTitle || valueAxis1.IsDefaultTextSettings != valueAxis2.IsDefaultTextSettings || valueAxis1.IsAutoMin != valueAxis2.IsAutoMin || valueAxis1.IsAutoMax != valueAxis2.IsAutoMax || valueAxis1.AutoTickLabelSpacing != valueAxis2.AutoTickLabelSpacing || valueAxis1.IsAutoMajor != valueAxis2.IsAutoMajor || valueAxis1.IsAutoMinor != valueAxis2.IsAutoMinor || valueAxis1.IsAutoCross != valueAxis2.IsAutoCross || valueAxis1.IsChangeAutoCross != valueAxis2.IsChangeAutoCross || valueAxis1.IsChangeAutoCrossInLoading != valueAxis2.IsChangeAutoCrossInLoading || valueAxis1.IsLogScale != valueAxis2.IsLogScale || valueAxis1.ReversePlotOrder != valueAxis2.ReversePlotOrder || valueAxis1.IsMaxCross != valueAxis2.IsMaxCross || valueAxis1.HasDisplayUnitLabel != valueAxis2.HasDisplayUnitLabel || valueAxis1.AxisType != valueAxis2.AxisType || valueAxis1.MinorTickMark != valueAxis2.MinorTickMark || valueAxis1.MajorTickMark != valueAxis2.MajorTickMark || valueAxis1.ParagraphType != valueAxis2.ParagraphType || valueAxis1.LabelTextAlign != valueAxis2.LabelTextAlign || valueAxis1.TickLabelPosition != valueAxis2.TickLabelPosition || valueAxis1.Alignment != valueAxis2.Alignment || valueAxis1.DisplayUnit != valueAxis2.DisplayUnit || valueAxis1.Title != valueAxis2.Title || valueAxis1.TextRotationAngle != valueAxis2.TextRotationAngle || valueAxis1.NumberFormatIndex != valueAxis2.NumberFormatIndex || valueAxis1.NumberFormat != valueAxis2.NumberFormat || valueAxis1.AxisId != valueAxis2.AxisId || valueAxis1.MinimumValue != valueAxis2.MinimumValue || valueAxis1.MaximumValue != valueAxis2.MaximumValue || valueAxis1.MajorUnit != valueAxis2.MajorUnit || valueAxis1.MinorUnit != valueAxis2.MinorUnit || valueAxis1.CrossValue != valueAxis2.CrossValue || valueAxis1.CrossesAt != valueAxis2.CrossesAt || valueAxis1.DisplayUnitCustom != valueAxis2.DisplayUnitCustom || valueAxis1.AxisPosition != valueAxis2.AxisPosition)
			{
				return true;
			}
			if ((valueAxis1.DisplayUnitLabel != null && valueAxis2.DisplayUnitLabel == null) || (valueAxis1.DisplayUnitLabel == null && valueAxis2.DisplayUnitLabel != null) || (valueAxis1.FrameFormat != null && valueAxis2.FrameFormat == null) || (valueAxis1.FrameFormat == null && valueAxis2.FrameFormat != null) || (valueAxis1.Chart3DOptions != null && valueAxis2.Chart3DOptions == null) || (valueAxis1.Chart3DOptions == null && valueAxis2.Chart3DOptions != null) || (valueAxis1.Chart3DProperties != null && valueAxis2.Chart3DProperties == null) || (valueAxis1.Chart3DProperties == null && valueAxis2.Chart3DProperties != null) || (valueAxis1.Shadow != null && valueAxis2.Shadow == null) || (valueAxis1.Shadow == null && valueAxis2.Shadow != null) || (valueAxis1.ShadowProperties != null && valueAxis2.ShadowProperties == null) || (valueAxis1.ShadowProperties == null && valueAxis2.ShadowProperties != null) || (valueAxis1.Border != null && valueAxis2.Border == null) || (valueAxis1.Border == null && valueAxis2.Border != null) || (valueAxis1.MinorGridLines != null && valueAxis2.MinorGridLines == null) || (valueAxis1.MinorGridLines == null && valueAxis2.MinorGridLines != null) || (valueAxis1.MajorGridLines != null && valueAxis2.MajorGridLines == null) || (valueAxis1.MajorGridLines == null && valueAxis2.MajorGridLines != null) || (valueAxis1.Font != null && valueAxis2.Font == null) || (valueAxis1.Font == null && valueAxis2.Font != null) || (valueAxis1.TitleArea != null && valueAxis2.TitleArea == null) || (valueAxis1.TitleArea == null && valueAxis2.TitleArea != null) || (valueAxis1.AxisPosition.HasValue && !valueAxis2.AxisPosition.HasValue) || (!valueAxis1.AxisPosition.HasValue && valueAxis2.AxisPosition.HasValue) || (valueAxis1.TextStream != null && valueAxis2.TextStream == null) || (valueAxis1.TextStream == null && valueAxis2.TextStream != null))
			{
				return false;
			}
			if (valueAxis1.TextStream != null && valueAxis2.TextStream != null && valueAxis1.TextStream.Length != valueAxis2.TextStream.Length)
			{
				return false;
			}
			if (valueAxis1.AxisPosition.HasValue && valueAxis2.AxisPosition.HasValue && valueAxis1.AxisPosition.Value != valueAxis2.AxisPosition.Value)
			{
				return false;
			}
			if (!CompareChartTextArea(valueAxis1.DisplayUnitLabel as ChartTextAreaImpl, valueAxis2.DisplayUnitLabel as ChartTextAreaImpl) || !CompareChartFrameFormat(valueAxis1.FrameFormat as ChartPlotAreaImpl, valueAxis2.FrameFormat as ChartPlotAreaImpl) || !CompareThreeDFormat(valueAxis1.Chart3DOptions as ThreeDFormatImpl, valueAxis2.Chart3DOptions as ThreeDFormatImpl) || !CompareThreeDFormat(valueAxis1.Chart3DProperties as ThreeDFormatImpl, valueAxis2.Chart3DProperties as ThreeDFormatImpl) || !CompareShadow(valueAxis1.Shadow as ShadowImpl, valueAxis2.Shadow as ShadowImpl) || !CompareShadow(valueAxis1.ShadowProperties as ShadowImpl, valueAxis2.ShadowProperties as ShadowImpl) || !CompareChartBorder(valueAxis1.Border as ChartBorderImpl, valueAxis2.Border as ChartBorderImpl) || !CompareChartGridLine(valueAxis1.MinorGridLines as ChartGridLineImpl, valueAxis2.MinorGridLines as ChartGridLineImpl) || !CompareChartGridLine(valueAxis1.MajorGridLines as ChartGridLineImpl, valueAxis2.MajorGridLines as ChartGridLineImpl) || !CompareFont(valueAxis1.Font as FontImpl, valueAxis2.Font as FontImpl) || !CompareChartTextArea(valueAxis1.TitleArea as ChartTextAreaImpl, valueAxis2.TitleArea as ChartTextAreaImpl))
			{
				return false;
			}
		}
		return true;
	}

	internal bool CompareChartBorder(ChartBorderImpl chartBorder1, ChartBorderImpl chartBorder2)
	{
		if (chartBorder1 != null && chartBorder2 != null)
		{
			if (chartBorder1.HasGradientFill != chartBorder2.HasGradientFill || chartBorder1.HasLineProperties != chartBorder2.HasLineProperties || chartBorder1.AutoFormat != chartBorder2.AutoFormat || chartBorder1.DrawTickLabels != chartBorder2.DrawTickLabels || chartBorder1.IsAutoLineColor != chartBorder2.IsAutoLineColor || chartBorder1.BeginArrowType != chartBorder2.BeginArrowType || chartBorder1.EndArrowType != chartBorder2.EndArrowType || chartBorder1.BeginArrowSize != chartBorder2.BeginArrowSize || chartBorder1.EndArrowSize != chartBorder2.EndArrowSize || chartBorder1.LinePattern != chartBorder2.LinePattern || chartBorder1.LineWeight != chartBorder2.LineWeight || chartBorder1.JoinType != chartBorder2.JoinType || chartBorder1.ColorIndex != chartBorder2.ColorIndex || chartBorder1.Transparency != chartBorder2.Transparency || chartBorder1.LineWeightString != chartBorder2.LineWeightString || chartBorder1.LineColor.ToArgb() != chartBorder2.LineColor.ToArgb())
			{
				return false;
			}
			if ((chartBorder1.Color != null && chartBorder2.Color == null) || (chartBorder1.Color == null && chartBorder2.Color != null) || (chartBorder1.Fill != null && chartBorder2.Fill == null) || (chartBorder1.Fill == null && chartBorder2.Fill != null))
			{
				return false;
			}
			if (!CompareChartColor(chartBorder1.Color, chartBorder2.Color) || !CompareFill(chartBorder1.Fill as ShapeFillImpl, chartBorder2.Fill as ShapeFillImpl))
			{
				return false;
			}
		}
		return true;
	}

	internal bool CompareChartTextRecord(ChartTextRecord textRecord1, ChartTextRecord textRecord2)
	{
		if (textRecord1 != null && textRecord2 != null && (textRecord1.NeedInfill != textRecord2.NeedInfill || textRecord1.NeedDataArray != textRecord2.NeedDataArray || textRecord1.IsAllowShortData != textRecord2.IsAllowShortData || textRecord1.NeedDecoding != textRecord2.NeedDecoding || textRecord1.IsAutoColor != textRecord2.IsAutoColor || textRecord1.IsShowKey != textRecord2.IsShowKey || textRecord1.IsShowValue != textRecord2.IsShowValue || textRecord1.IsVertical != textRecord2.IsVertical || textRecord1.IsAutoText != textRecord2.IsAutoText || textRecord1.IsGenerated != textRecord2.IsGenerated || textRecord1.IsDeleted != textRecord2.IsDeleted || textRecord1.IsAutoMode != textRecord2.IsAutoMode || textRecord1.IsShowLabelPercent != textRecord2.IsShowLabelPercent || textRecord1.IsShowPercent != textRecord2.IsShowPercent || textRecord1.IsShowBubbleSizes != textRecord2.IsShowBubbleSizes || textRecord1.IsShowLabel != textRecord2.IsShowLabel || textRecord1.HorzAlign != textRecord2.HorzAlign || textRecord1.VertAlign != textRecord2.VertAlign || textRecord1.BackgroundMode != textRecord2.BackgroundMode || textRecord1.ColorIndex != textRecord2.ColorIndex || textRecord1.Rotation != textRecord2.Rotation || textRecord1.DataLabelPlacement != textRecord2.DataLabelPlacement || textRecord1.TextColor != textRecord2.TextColor || textRecord1.XPos != textRecord2.XPos || textRecord1.YPos != textRecord2.YPos || textRecord1.XSize != textRecord2.XSize || textRecord1.YSize != textRecord2.YSize || textRecord1.Options != textRecord2.Options || textRecord1.Options2 != textRecord2.Options2 || textRecord1.TextRotation != textRecord2.TextRotation || textRecord1.MinimumRecordSize != textRecord2.MinimumRecordSize || textRecord1.MaximumRecordSize != textRecord2.MaximumRecordSize))
		{
			return false;
		}
		return true;
	}

	internal bool CompareChartColor(ChartColor color1, ChartColor color2)
	{
		if ((object)color1 != null && (object)color2 != null && (color1.IsSchemeColor != color2.IsSchemeColor || color1.Value != color2.Value || color1.Tint != color2.Tint || color1.Saturation != color2.Saturation || color1.Luminance != color2.Luminance || color1.LuminanceOffSet != color2.LuminanceOffSet || color1.SchemaName != color2.SchemaName || color1.HexColor != color2.HexColor || color1.ColorType != color2.ColorType))
		{
			return false;
		}
		return true;
	}

	internal bool CompareChartObjectLinkRecord(ChartObjectLinkRecord chartObjectLinkRecord1, ChartObjectLinkRecord chartObjectLinkRecord2)
	{
		if (chartObjectLinkRecord1 != null && chartObjectLinkRecord1 != null && (chartObjectLinkRecord1.NeedInfill != chartObjectLinkRecord2.NeedInfill || chartObjectLinkRecord1.NeedDataArray != chartObjectLinkRecord2.NeedDataArray || chartObjectLinkRecord1.IsAllowShortData != chartObjectLinkRecord2.IsAllowShortData || chartObjectLinkRecord1.NeedDecoding != chartObjectLinkRecord2.NeedDecoding || chartObjectLinkRecord1.TypeCode != chartObjectLinkRecord2.TypeCode || chartObjectLinkRecord1.LinkObject != chartObjectLinkRecord2.LinkObject || chartObjectLinkRecord1.RecordCode != chartObjectLinkRecord2.RecordCode || chartObjectLinkRecord1.Length != chartObjectLinkRecord2.Length || chartObjectLinkRecord1.StreamPos != chartObjectLinkRecord2.StreamPos || chartObjectLinkRecord1.MinimumRecordSize != chartObjectLinkRecord2.MinimumRecordSize || chartObjectLinkRecord1.MaximumRecordSize != chartObjectLinkRecord2.MaximumRecordSize || chartObjectLinkRecord1.MaximumMemorySize != chartObjectLinkRecord2.MaximumMemorySize || chartObjectLinkRecord1.StartDecodingOffset != chartObjectLinkRecord2.StartDecodingOffset || chartObjectLinkRecord1.SeriesNumber != chartObjectLinkRecord2.SeriesNumber || chartObjectLinkRecord1.DataPointNumber != chartObjectLinkRecord2.DataPointNumber))
		{
			return false;
		}
		return true;
	}

	internal bool CompareChartAlrunsRecord(ChartAlrunsRecord chartAlrunsRecord1, ChartAlrunsRecord chartAlrunsRecord2)
	{
		if (chartAlrunsRecord1 != null && chartAlrunsRecord2 != null && (chartAlrunsRecord1.NeedInfill != chartAlrunsRecord2.NeedInfill || chartAlrunsRecord1.NeedDataArray != chartAlrunsRecord2.NeedDataArray || chartAlrunsRecord1.IsAllowShortData != chartAlrunsRecord2.IsAllowShortData || chartAlrunsRecord1.NeedDecoding != chartAlrunsRecord2.NeedDecoding || chartAlrunsRecord1.TypeCode != chartAlrunsRecord2.TypeCode || chartAlrunsRecord1.RecordCode != chartAlrunsRecord2.RecordCode || chartAlrunsRecord1.Length != chartAlrunsRecord2.Length || chartAlrunsRecord1.StreamPos != chartAlrunsRecord2.StreamPos || chartAlrunsRecord1.MinimumRecordSize != chartAlrunsRecord2.MinimumRecordSize || chartAlrunsRecord1.MaximumRecordSize != chartAlrunsRecord2.MaximumRecordSize || chartAlrunsRecord1.MaximumMemorySize != chartAlrunsRecord2.MaximumMemorySize || chartAlrunsRecord1.StartDecodingOffset != chartAlrunsRecord2.StartDecodingOffset || chartAlrunsRecord1.Quantity != chartAlrunsRecord2.Quantity))
		{
			return false;
		}
		return true;
	}

	internal bool CompareChartAttachedLabelLayoutRecord(ChartAttachedLabelLayoutRecord LabelLayoutRecord1, ChartAttachedLabelLayoutRecord LabelLayoutRecord2)
	{
		if (LabelLayoutRecord1 != null && LabelLayoutRecord2 != null)
		{
			if (LabelLayoutRecord1.NeedInfill != LabelLayoutRecord2.NeedInfill || LabelLayoutRecord1.NeedDataArray != LabelLayoutRecord2.NeedDataArray || LabelLayoutRecord1.IsAllowShortData != LabelLayoutRecord2.IsAllowShortData || LabelLayoutRecord1.NeedDecoding != LabelLayoutRecord2.NeedDecoding || LabelLayoutRecord1.TypeCode != LabelLayoutRecord2.TypeCode || LabelLayoutRecord1.WXMode != LabelLayoutRecord2.WXMode || LabelLayoutRecord1.WYMode != LabelLayoutRecord2.WYMode || LabelLayoutRecord1.WWidthMode != LabelLayoutRecord2.WWidthMode || LabelLayoutRecord1.WHeightMode != LabelLayoutRecord2.WHeightMode || LabelLayoutRecord1.RecordCode != LabelLayoutRecord2.RecordCode || LabelLayoutRecord1.Length != LabelLayoutRecord2.Length || LabelLayoutRecord1.StreamPos != LabelLayoutRecord2.StreamPos || LabelLayoutRecord1.MinimumRecordSize != LabelLayoutRecord2.MinimumRecordSize || LabelLayoutRecord1.MaximumRecordSize != LabelLayoutRecord2.MaximumRecordSize || LabelLayoutRecord1.MaximumMemorySize != LabelLayoutRecord2.MaximumMemorySize || LabelLayoutRecord1.StartDecodingOffset != LabelLayoutRecord2.StartDecodingOffset || LabelLayoutRecord1.X != LabelLayoutRecord2.X || LabelLayoutRecord1.Y != LabelLayoutRecord2.Y || LabelLayoutRecord1.Dx != LabelLayoutRecord2.Dx || LabelLayoutRecord1.Dy != LabelLayoutRecord2.Dy)
			{
				return false;
			}
			if ((LabelLayoutRecord1.Data != null && LabelLayoutRecord2.Data == null) || (LabelLayoutRecord1.Data == null && LabelLayoutRecord2.Data != null))
			{
				return false;
			}
			if (LabelLayoutRecord1.Data != null && LabelLayoutRecord2.Data != null && LabelLayoutRecord1.Data.ToString() != LabelLayoutRecord2.Data.ToString())
			{
				return false;
			}
		}
		return true;
	}

	internal bool CompareChartAIRecord(ChartAIRecord chartAIRecord1, ChartAIRecord chartAIRecord2)
	{
		if (chartAIRecord1 != null && chartAIRecord2 != null)
		{
			if (chartAIRecord1.NeedInfill != chartAIRecord2.NeedInfill || chartAIRecord1.NeedDataArray != chartAIRecord2.NeedDataArray || chartAIRecord1.IsAllowShortData != chartAIRecord2.IsAllowShortData || chartAIRecord1.NeedDecoding != chartAIRecord2.NeedDecoding || chartAIRecord1.IsCustomNumberFormat != chartAIRecord2.IsCustomNumberFormat || chartAIRecord1.IndexIdentifier != chartAIRecord2.IndexIdentifier || chartAIRecord1.Reference != chartAIRecord2.Reference || chartAIRecord1.TypeCode != chartAIRecord2.TypeCode || chartAIRecord1.RecordCode != chartAIRecord2.RecordCode || chartAIRecord1.Length != chartAIRecord2.Length || chartAIRecord1.StreamPos != chartAIRecord2.StreamPos || chartAIRecord1.MinimumRecordSize != chartAIRecord2.MinimumRecordSize || chartAIRecord1.MaximumRecordSize != chartAIRecord2.MaximumRecordSize || chartAIRecord1.MaximumMemorySize != chartAIRecord2.MaximumMemorySize || chartAIRecord1.StartDecodingOffset != chartAIRecord2.StartDecodingOffset || chartAIRecord1.Options != chartAIRecord2.Options || chartAIRecord1.NumberFormatIndex != chartAIRecord2.NumberFormatIndex || chartAIRecord1.FormulaSize != chartAIRecord2.FormulaSize)
			{
				return false;
			}
			if ((chartAIRecord1.Data != null && chartAIRecord2.Data == null) || (chartAIRecord1.Data == null && chartAIRecord2.Data != null))
			{
				return false;
			}
			if (chartAIRecord1.Data != null && chartAIRecord2.Data != null && chartAIRecord1.Data.ToString() != chartAIRecord2.Data.ToString())
			{
				return false;
			}
		}
		return true;
	}

	internal bool CompareWorkBookImpl(WorkbookImpl workbook1, WorkbookImpl workbook2)
	{
		if (workbook1 != null && workbook2 != null && (workbook1.HidePivotFieldList != workbook2.HidePivotFieldList || workbook1.Date1904 != workbook2.Date1904 || workbook1.PrecisionAsDisplayed != workbook2.PrecisionAsDisplayed || workbook1.IsCellProtection != workbook2.IsCellProtection || workbook1.IsWindowProtection != workbook2.IsWindowProtection || workbook1.ReadOnly != workbook2.ReadOnly || workbook1.Saved != workbook2.Saved || workbook1.HasMacros != workbook2.HasMacros || workbook1.ThrowOnUnknownNames != workbook2.ThrowOnUnknownNames || workbook1.IsHScrollBarVisible != workbook2.IsHScrollBarVisible || workbook1.IsVScrollBarVisible != workbook2.IsVScrollBarVisible || workbook1.DisableMacrosStart != workbook2.DisableMacrosStart || workbook1.HasStandardFont != workbook2.HasStandardFont || workbook1.Allow3DRangesInDataValidation != workbook2.Allow3DRangesInDataValidation || workbook1.IsRightToLeft != workbook2.IsRightToLeft || workbook1.DisplayWorkbookTabs != workbook2.DisplayWorkbookTabs || workbook1.DetectDateTimeInValue != workbook2.DetectDateTimeInValue || workbook1.UseFastStringSearching != workbook2.UseFastStringSearching || workbook1.ReadOnlyRecommended != workbook2.ReadOnlyRecommended || workbook1.EnabledCalcEngine != workbook2.EnabledCalcEngine || workbook1.IsWorkbookOpening != workbook2.IsWorkbookOpening || workbook1.Saving != workbook2.Saving || workbook1.HasInlineStrings != workbook2.HasInlineStrings || workbook1.HasDuplicatedNames != workbook2.HasDuplicatedNames || workbook1.InternalSaved != workbook2.InternalSaved || workbook1.IsConverted != workbook2.IsConverted || workbook1.IsCreated != workbook2.IsCreated || workbook1.IsLoaded != workbook2.IsLoaded || workbook1.CheckCompability != workbook2.CheckCompability || workbook1.HasApostrophe != workbook2.HasApostrophe || workbook1.ParseOnDemand != workbook2.ParseOnDemand || workbook1.IsCellModified != workbook2.IsCellModified || workbook1.IsEqualColor != workbook2.IsEqualColor || workbook1.Options != workbook2.Options || workbook1.Version != workbook2.Version || workbook1.ActiveSheetIndex != workbook2.ActiveSheetIndex || workbook1.Author != workbook2.Author || workbook1.CodeName != workbook2.CodeName || workbook1.DefaultThemeVersion != workbook2.DefaultThemeVersion || workbook1.DisplayedTab != workbook2.DisplayedTab || workbook1.StandardFontSize != workbook2.StandardFontSize || workbook1.StandardFont != workbook2.StandardFont || workbook1.RowSeparator != workbook2.RowSeparator || workbook1.ArgumentsSeparator != workbook2.ArgumentsSeparator || workbook1.PasswordToOpen != workbook2.PasswordToOpen || workbook1.MaxRowCount != workbook2.MaxRowCount || workbook1.MaxColumnCount != workbook2.MaxColumnCount || workbook1.MaxXFCount != workbook2.MaxXFCount || workbook1.MaxIndent != workbook2.MaxIndent || workbook1.MaxImportColumns != workbook2.MaxImportColumns || workbook1.BookCFPriorityCount != workbook2.BookCFPriorityCount || workbook1.LastPivotTableIndex != workbook2.LastPivotTableIndex || workbook1.FullFileName != workbook2.FullFileName || workbook1.ObjectCount != workbook2.ObjectCount || workbook1.MaxDigitWidth != workbook2.MaxDigitWidth || workbook1.CurrentObjectId != workbook2.CurrentObjectId || workbook1.CurrentHeaderId != workbook2.CurrentHeaderId || workbook1.FirstCharSize != workbook2.FirstCharSize || workbook1.SecondCharSize != workbook2.SecondCharSize || workbook1.BeginVersion != workbook2.BeginVersion || workbook1.DefaultXFIndex != workbook2.DefaultXFIndex || workbook1.MaxTableIndex != workbook2.MaxTableIndex || workbook1.AlgorithmName != workbook2.AlgorithmName || workbook1.SpinCount != workbook2.SpinCount || workbook1.StandardRowHeight != workbook2.StandardRowHeight || workbook1.StandardRowHeightInPixels != workbook2.StandardRowHeightInPixels))
		{
			return false;
		}
		return true;
	}

	internal bool CompareChartLayout(ChartLayoutImpl chartLayout1, ChartLayoutImpl chartLayout2)
	{
		if (chartLayout1 != null && chartLayout2 != null)
		{
			if (chartLayout1.IsManualLayout != chartLayout2.IsManualLayout || chartLayout1.LayoutTarget != chartLayout2.LayoutTarget || chartLayout1.LeftMode != chartLayout2.LeftMode || chartLayout1.TopMode != chartLayout2.TopMode || chartLayout1.WidthMode != chartLayout2.WidthMode || chartLayout1.HeightMode != chartLayout2.HeightMode || chartLayout1.Left != chartLayout2.Left || chartLayout1.Top != chartLayout2.Top || chartLayout1.Width != chartLayout2.Width || chartLayout1.Height != chartLayout2.Height)
			{
				return false;
			}
			if ((chartLayout1.ManualLayout != null && chartLayout2.ManualLayout == null) || (chartLayout1.ManualLayout == null && chartLayout2.ManualLayout != null))
			{
				return false;
			}
			if (!CompareChartManualLayout(chartLayout1.ManualLayout as ChartManualLayoutImpl, chartLayout2.ManualLayout as ChartManualLayoutImpl))
			{
				return false;
			}
		}
		return true;
	}

	internal bool CompareChartRichText(ChartRichTextString chartRichTextString1, ChartRichTextString chartRichTextString2)
	{
		if (chartRichTextString1 != null && chartRichTextString2 != null && chartRichTextString1.Text != chartRichTextString2.Text)
		{
			return false;
		}
		return true;
	}

	internal bool CompareFill(ShapeFillImpl shapeFill1, ShapeFillImpl shapeFill2)
	{
		if (shapeFill1 != null && shapeFill2 != null)
		{
			if (shapeFill1.Tile != shapeFill2.Tile || shapeFill1.IsGradientSupported != shapeFill2.IsGradientSupported || shapeFill1.Visible != shapeFill2.Visible || shapeFill1.FillType != shapeFill2.FillType || shapeFill1.BackColorIndex != shapeFill2.BackColorIndex || shapeFill1.ForeColorIndex != shapeFill2.ForeColorIndex || shapeFill1.TransparencyColor != shapeFill2.TransparencyColor || shapeFill1.TextureVerticalScale != shapeFill2.TextureVerticalScale || shapeFill1.TextureHorizontalScale != shapeFill2.TextureHorizontalScale || shapeFill1.TextureOffsetX != shapeFill2.TextureOffsetX || shapeFill1.TextureOffsetY != shapeFill2.TextureOffsetY || shapeFill1.Alignment != shapeFill2.Alignment || shapeFill1.TileFlipping != shapeFill2.TileFlipping || shapeFill1.BackColor != shapeFill2.BackColor || shapeFill1.ForeColor != shapeFill2.ForeColor)
			{
				return false;
			}
			if (shapeFill1.FillType == OfficeFillType.Gradient && (shapeFill1.GradientStyle != shapeFill2.GradientStyle || shapeFill1.GradientVariant != shapeFill2.GradientVariant || shapeFill1.GradientColorType != shapeFill2.GradientColorType || shapeFill1.PresetGradientType != shapeFill2.PresetGradientType || shapeFill1.GradientDegree != shapeFill2.GradientDegree || shapeFill1.TransparencyTo != shapeFill2.TransparencyTo))
			{
				return false;
			}
			if (shapeFill2.FillType == OfficeFillType.Pattern && shapeFill1.Pattern != shapeFill2.Pattern)
			{
				return false;
			}
			if (shapeFill2.FillType == OfficeFillType.Texture && shapeFill1.Texture != shapeFill2.Texture)
			{
				return false;
			}
			if (shapeFill2.FillType == OfficeFillType.SolidColor && shapeFill1.Transparency != shapeFill2.Transparency)
			{
				return false;
			}
			if (shapeFill2.FillType == OfficeFillType.Picture && shapeFill1.PictureName != shapeFill2.PictureName)
			{
				return false;
			}
			if ((shapeFill1.GradientStops == null || shapeFill2.GradientStops != null) && (shapeFill1.GradientStops != null || shapeFill2.GradientStops == null) && (shapeFill1.PreservedGradient == null || shapeFill2.PreservedGradient != null) && (shapeFill1.PreservedGradient != null || shapeFill2.PreservedGradient == null))
			{
				_ = shapeFill1.FillRect;
				_ = shapeFill2.FillRect;
				_ = shapeFill1.FillRect;
				_ = shapeFill1.SourceRect;
				_ = shapeFill2.SourceRect;
				_ = shapeFill1.SourceRect;
				if ((!(shapeFill1.BackColorObject != null) || !(shapeFill2.BackColorObject == null)) && (!(shapeFill1.BackColorObject == null) || !(shapeFill2.BackColorObject != null)) && (!(shapeFill1.ForeColorObject != null) || !(shapeFill2.ForeColorObject == null)) && (!(shapeFill1.ForeColorObject == null) || !(shapeFill2.ForeColorObject != null)))
				{
					if (!CompareGradientStops(shapeFill1.GradientStops, shapeFill2.GradientStops) || !CompareGradientStops(shapeFill2.PreservedGradient, shapeFill2.PreservedGradient) || !CompareRectangle(shapeFill1.FillRect, shapeFill2.FillRect) || !CompareRectangle(shapeFill1.SourceRect, shapeFill2.SourceRect) || !CompareChartColor(shapeFill1.BackColorObject, shapeFill2.BackColorObject) || !CompareChartColor(shapeFill2.ForeColorObject, shapeFill2.ForeColorObject))
					{
						return false;
					}
					goto IL_0332;
				}
			}
			return false;
		}
		goto IL_0332;
		IL_0332:
		return true;
	}

	internal bool CompareThreeDFormat(ThreeDFormatImpl formatImpl1, ThreeDFormatImpl formatImpl2)
	{
		if (formatImpl1 != null && formatImpl2 != null && (formatImpl1.IsBevelTopWidthSet != formatImpl2.IsBevelTopWidthSet || formatImpl1.IsBevelBottomWidthSet != formatImpl2.IsBevelBottomWidthSet || formatImpl1.IsBevelTopHeightSet != formatImpl2.IsBevelTopHeightSet || formatImpl1.IsBevelBottomHeightSet != formatImpl2.IsBevelBottomHeightSet || formatImpl1.BevelTop != formatImpl2.BevelTop || formatImpl1.BevelBottom != formatImpl2.BevelBottom || formatImpl1.Material != formatImpl2.Material || formatImpl1.Lighting != formatImpl2.Lighting || formatImpl1.BevelTopHeight != formatImpl2.BevelTopHeight || formatImpl1.BevelBottomHeight != formatImpl2.BevelBottomHeight || formatImpl1.BevelBottomWidth != formatImpl2.BevelBottomWidth || formatImpl1.PresetShape != formatImpl2.PresetShape))
		{
			return false;
		}
		return true;
	}

	internal bool CompareShadow(ShadowImpl shadow1, ShadowImpl shadow2)
	{
		if (shadow1 != null && shadow2 != null)
		{
			if (shadow1.HasCustomShadowStyle != shadow2.HasCustomShadowStyle || shadow1.ShadowOuterPresets != shadow2.ShadowOuterPresets || shadow1.ShadowInnerPresets != shadow2.ShadowInnerPresets || shadow1.ShadowPerspectivePresets != shadow2.ShadowPerspectivePresets || shadow1.Transparency != shadow2.Transparency || shadow1.Size != shadow2.Size || shadow1.Blur != shadow2.Blur || shadow1.Angle != shadow2.Angle || shadow1.Distance != shadow2.Distance)
			{
				return false;
			}
			if ((shadow1.ShadowFormat != null && shadow2.ShadowFormat == null) || (shadow1.ShadowFormat == null && shadow2.ShadowFormat != null) || (shadow1.ColorObject != null && shadow2.ColorObject == null) || (shadow1.ColorObject == null && shadow2.ColorObject != null))
			{
				return false;
			}
			if (!CompareChartMarkerFormatRecord(shadow1.ShadowFormat, shadow2.ShadowFormat) || !CompareChartColor(shadow1.ColorObject, shadow2.ColorObject))
			{
				return false;
			}
		}
		return true;
	}

	internal bool CompareChartInterior(ChartInteriorImpl interior1, ChartInteriorImpl interior2)
	{
		if (interior1 != null && interior2 != null)
		{
			if (interior1.UseAutomaticFormat != interior2.UseAutomaticFormat || interior1.SwapColorsOnNegative != interior2.SwapColorsOnNegative || interior1.Pattern != interior2.Pattern || interior1.ForegroundColorIndex != interior2.ForegroundColorIndex || interior1.BackgroundColorIndex != interior2.BackgroundColorIndex || interior1.ForegroundColor != interior2.ForegroundColor || interior2.BackgroundColor != interior2.BackgroundColor)
			{
				return false;
			}
			if ((interior1.ForegroundColorObject != null && interior2.ForegroundColorObject == null) || (interior1.ForegroundColorObject == null && interior2.ForegroundColorObject != null) || (interior1.BackgroundColorObject != null && interior2.BackgroundColorObject == null) || (interior1.BackgroundColorObject == null && interior2.BackgroundColorObject != null))
			{
				return false;
			}
			if (!CompareChartColor(interior1.ForegroundColorObject, interior2.ForegroundColorObject) || !CompareChartColor(interior1.BackgroundColorObject, interior2.BackgroundColorObject))
			{
				return false;
			}
		}
		return true;
	}

	internal bool CompareGradientStops(GradientStops gradientStops1, GradientStops gradientStops2)
	{
		if (gradientStops1 != null && gradientStops2 != null)
		{
			if (gradientStops1.IsDoubled != gradientStops2.IsDoubled || gradientStops1.Angle != gradientStops2.Angle || gradientStops1.GradientType != gradientStops2.GradientType)
			{
				return false;
			}
			_ = gradientStops1.FillToRect;
			_ = gradientStops2.FillToRect;
			_ = gradientStops1.FillToRect;
			_ = gradientStops1.TileRect;
			_ = gradientStops2.TileRect;
			_ = gradientStops1.TileRect;
			if (!CompareRectangle(gradientStops1.FillToRect, gradientStops2.FillToRect) || !CompareRectangle(gradientStops2.TileRect, gradientStops2.TileRect))
			{
				return false;
			}
		}
		return true;
	}

	internal bool CompareHistogramAxisFormat(HistogramAxisFormat histogramAxisFormat1, HistogramAxisFormat histogramAxisFormat2)
	{
		if (histogramAxisFormat1 != null && histogramAxisFormat2 != null && (histogramAxisFormat1.HasAutomaticBins != histogramAxisFormat2.HasAutomaticBins || histogramAxisFormat1.IsBinningByCategory != histogramAxisFormat2.IsBinningByCategory || histogramAxisFormat1.IsIntervalClosedinLeft != histogramAxisFormat2.IsIntervalClosedinLeft || histogramAxisFormat1.IsAutomaticFlowValue != histogramAxisFormat2.IsAutomaticFlowValue || histogramAxisFormat1.IsNotAutomaticUnderFlowValue != histogramAxisFormat2.IsNotAutomaticUnderFlowValue || histogramAxisFormat1.IsNotAutomaticOverFlowValue != histogramAxisFormat2.IsNotAutomaticOverFlowValue || histogramAxisFormat1.BinWidth != histogramAxisFormat2.BinWidth || histogramAxisFormat1.NumberOfBins != histogramAxisFormat2.NumberOfBins || histogramAxisFormat1.OverflowBinValue != histogramAxisFormat2.OverflowBinValue || histogramAxisFormat1.UnderflowBinValue != histogramAxisFormat2.UnderflowBinValue || histogramAxisFormat1.FlagOptions != histogramAxisFormat2.FlagOptions))
		{
			return false;
		}
		return true;
	}

	internal bool CompareChartMarkerFormatRecord(ChartMarkerFormatRecord formatRecord1, ChartMarkerFormatRecord formatRecord2)
	{
		if (formatRecord1 != null && formatRecord2 != null && (formatRecord1.IsAutoColor != formatRecord2.IsAutoColor || formatRecord1.IsNotShowInt != formatRecord2.IsNotShowInt || formatRecord1.IsNotShowBrd != formatRecord2.IsNotShowBrd || formatRecord1.HasLineProperties != formatRecord2.HasLineProperties || formatRecord1.ForeColor != formatRecord2.ForeColor || formatRecord1.BackColor != formatRecord2.BackColor || formatRecord1.Options != formatRecord2.Options || formatRecord1.BorderColorIndex != formatRecord2.BorderColorIndex || formatRecord1.FillColorIndex != formatRecord2.FillColorIndex || formatRecord1.LineSize != formatRecord2.LineSize || formatRecord1.MinimumRecordSize != formatRecord2.MinimumRecordSize || formatRecord1.MaximumRecordSize != formatRecord2.MaximumRecordSize || formatRecord1.FlagOptions != formatRecord2.FlagOptions || formatRecord1.MarkerType != formatRecord2.MarkerType))
		{
			return false;
		}
		return true;
	}

	internal bool CompareChartLegendEntries(ChartLegendEntriesColl entriesColl1, ChartLegendEntriesColl entriesColl2)
	{
		if (entriesColl1 != null && entriesColl2 != null)
		{
			if (entriesColl1.Count != entriesColl2.Count)
			{
				return false;
			}
			if ((entriesColl1.HashEntries != null && entriesColl2.HashEntries == null) || (entriesColl1.HashEntries == null && entriesColl2.HashEntries != null) || entriesColl1.HashEntries.Count != entriesColl2.HashEntries.Count)
			{
				return false;
			}
			for (int i = 0; i < entriesColl1.HashEntries.Count; i++)
			{
				ChartLegendEntryImpl chartLegendEntryImpl = entriesColl1.HashEntries[i];
				ChartLegendEntryImpl chartLegendEntryImpl2 = entriesColl2.HashEntries[i];
				if (chartLegendEntryImpl != chartLegendEntryImpl2)
				{
					return false;
				}
			}
		}
		return true;
	}

	internal bool CompareChartManualLayout(ChartManualLayoutImpl layoutImpl1, ChartManualLayoutImpl layoutImpl2)
	{
		if (layoutImpl1 != null && layoutImpl2 != null && (layoutImpl1.LayoutTarget != layoutImpl2.LayoutTarget || layoutImpl1.LeftMode != layoutImpl2.LeftMode || layoutImpl1.TopMode != layoutImpl2.TopMode || layoutImpl1.WidthMode != layoutImpl2.WidthMode || layoutImpl1.HeightMode != layoutImpl2.HeightMode || layoutImpl1.Left != layoutImpl2.Left || layoutImpl1.Top != layoutImpl2.Top || layoutImpl1.Width != layoutImpl2.Width || layoutImpl1.Height != layoutImpl2.Height || layoutImpl1.dX != layoutImpl2.dX || layoutImpl1.dY != layoutImpl2.dY || layoutImpl1.xTL != layoutImpl2.xTL || layoutImpl1.yTL != layoutImpl2.yTL || layoutImpl1.xBR != layoutImpl2.xBR || layoutImpl1.yBR != layoutImpl2.yBR || layoutImpl1.FlagOptions != layoutImpl2.FlagOptions))
		{
			return false;
		}
		return true;
	}

	internal bool CompareChartSerie(ChartSerieImpl serie1, ChartSerieImpl serie2)
	{
		if (serie1 != null && serie2 != null)
		{
			if (serie1.Reversed != serie2.Reversed || serie1.IsValidValueRange != serie2.IsValidValueRange || serie1.IsValidCategoryRange != serie2.IsValidCategoryRange || serie1.UsePrimaryAxis != serie2.UsePrimaryAxis || serie1.HasColumnShape != serie2.HasColumnShape || serie1.HasErrorBarsY != serie2.HasErrorBarsY || serie1.HasErrorBarsX != serie2.HasErrorBarsX || serie1.ShowGapWidth != serie2.ShowGapWidth || serie1.HasLeaderLines != serie2.HasLeaderLines || serie1.IsFiltered != serie2.IsFiltered || serie1.IsDefaultName != serie2.IsDefaultName || serie1.IsPie != serie2.IsPie || serie1.InvertIfNegative != serie2.InvertIfNegative || serie1.IsParetoLineHidden != serie2.IsParetoLineHidden || serie1.IsSeriesHidden != serie2.IsSeriesHidden || serie1.IsRowWiseCategory != serie2.IsRowWiseCategory || serie1.IsRowWiseSeries != serie2.IsRowWiseSeries || serie1.SerieType != serie2.SerieType || serie1.SerieName != serie2.SerieName || serie1.ExistingOrder != serie2.ExistingOrder || serie1.Name != serie2.Name || serie1.RealIndex != serie2.RealIndex || serie1.Grouping != serie2.Grouping || serie1.GapWidth != serie2.GapWidth || serie1.Overlap != serie2.Overlap || serie1.FormatCode != serie2.FormatCode || serie1.PointCount != serie2.PointCount || serie1.Index != serie2.Index || serie1.Number != serie2.Number || serie1.ChartGroup != serie2.ChartGroup || serie1.PointNumber != serie2.PointNumber || serie1.FilteredCategory != serie2.FilteredCategory || serie1.FilteredValue != serie2.FilteredValue || serie1.StartType != serie2.StartType || serie1.ParseSerieNotDefaultText != serie2.ParseSerieNotDefaultText || serie1.NameOrFormula != serie2.NameOrFormula || serie1.StrRefFormula != serie2.StrRefFormula || serie1.NumRefFormula != serie2.NumRefFormula || serie1.MulLvlStrRefFormula != serie2.MulLvlStrRefFormula || serie1.ParetoLineFormatIndex != serie2.ParetoLineFormatIndex || serie1.CategoriesFormatCode != serie2.CategoriesFormatCode)
			{
				return false;
			}
			if ((serie1.SerieFormat != null && serie2.SerieFormat == null) || (serie1.SerieFormat == null && serie2.SerieFormat != null))
			{
				return false;
			}
			if (serie1.SerieFormat != null && serie2.SerieFormat != null)
			{
				if ((serie1.SerieFormat.Fill != null && serie2.SerieFormat.Fill == null) || (serie1.SerieFormat.Fill == null && serie2.SerieFormat.Fill != null) || (serie1.SerieFormat.LineProperties != null && serie2.SerieFormat.LineProperties == null) || (serie1.SerieFormat.LineProperties == null && serie2.SerieFormat.LineProperties != null))
				{
					return false;
				}
				if (!CompareFill(serie1.SerieFormat.Fill as ShapeFillImpl, serie2.SerieFormat.Fill as ShapeFillImpl) || !CompareChartBorder(serie1.SerieFormat.LineProperties as ChartBorderImpl, serie2.SerieFormat.Fill as ChartBorderImpl))
				{
					return false;
				}
			}
		}
		return true;
	}

	internal bool CompareBiffStorage(IBiffStorage storage1, IBiffStorage storage2)
	{
		if (storage1 != null && storage2 != null && (storage1.NeedDataArray != storage2.NeedDataArray || storage1.RecordCode != storage2.RecordCode || storage1.StreamPos != storage2.StreamPos || storage1.TypeCode != storage2.TypeCode))
		{
			return false;
		}
		return true;
	}

	internal bool CompareChartShtpropsRecordAsString(ChartShtpropsRecord chartShtpropsRecord1, ChartShtpropsRecord chartShtpropsRecord2)
	{
		if (chartShtpropsRecord1 != null && chartShtpropsRecord2 != null && (chartShtpropsRecord1.NeedInfill != chartShtpropsRecord2.NeedInfill || chartShtpropsRecord1.NeedDataArray != chartShtpropsRecord2.NeedDataArray || chartShtpropsRecord1.IsAllowShortData != chartShtpropsRecord2.IsAllowShortData || chartShtpropsRecord1.NeedDecoding != chartShtpropsRecord2.NeedDecoding || chartShtpropsRecord1.RecordCode != chartShtpropsRecord2.RecordCode || chartShtpropsRecord1.Length != chartShtpropsRecord2.Length || chartShtpropsRecord1.StreamPos != chartShtpropsRecord2.StreamPos || chartShtpropsRecord1.MinimumRecordSize != chartShtpropsRecord2.MinimumRecordSize || chartShtpropsRecord1.MaximumRecordSize != chartShtpropsRecord2.MaximumRecordSize || chartShtpropsRecord1.MaximumMemorySize != chartShtpropsRecord2.MaximumMemorySize || chartShtpropsRecord1.StartDecodingOffset != chartShtpropsRecord2.StartDecodingOffset || chartShtpropsRecord1.IsManSerAlloc != chartShtpropsRecord2.IsManSerAlloc || chartShtpropsRecord1.IsPlotVisOnly != chartShtpropsRecord2.IsPlotVisOnly || chartShtpropsRecord1.IsNotSizeWith != chartShtpropsRecord2.IsNotSizeWith || chartShtpropsRecord1.IsManPlotArea != chartShtpropsRecord2.IsManPlotArea || chartShtpropsRecord1.IsAlwaysAutoPlotArea != chartShtpropsRecord2.IsAlwaysAutoPlotArea || chartShtpropsRecord1.Flags != chartShtpropsRecord2.Flags || chartShtpropsRecord1.Reserved != chartShtpropsRecord2.Reserved || chartShtpropsRecord1.MinimumRecordSize != chartShtpropsRecord2.MinimumRecordSize || chartShtpropsRecord1.MaximumRecordSize != chartShtpropsRecord2.MaximumRecordSize || chartShtpropsRecord1.PlotBlank != chartShtpropsRecord2.PlotBlank))
		{
			return false;
		}
		return true;
	}

	internal bool CompareChartFormatCollectionAsString(ChartFormatCollection chartFormatCollection1, ChartFormatCollection chartFormatCollection2)
	{
		if (chartFormatCollection1 != null && chartFormatCollection2 != null)
		{
			if (chartFormatCollection1.QuietMode != chartFormatCollection2.QuietMode || chartFormatCollection1.IsReadOnly != chartFormatCollection2.IsReadOnly || chartFormatCollection1.Capacity != chartFormatCollection2.Capacity || chartFormatCollection1.Count != chartFormatCollection2.Count || chartFormatCollection1.IsPrimary != chartFormatCollection2.IsPrimary || chartFormatCollection1.NeedSecondaryAxis != chartFormatCollection2.NeedSecondaryAxis || chartFormatCollection1.IsParetoFormat != chartFormatCollection2.IsParetoFormat || chartFormatCollection1.IsBarChartAxes != chartFormatCollection2.IsBarChartAxes || chartFormatCollection1.IsPercentStackedAxis != chartFormatCollection2.IsPercentStackedAxis || chartFormatCollection1.Count != chartFormatCollection2.Count)
			{
				return false;
			}
			for (int i = 0; i < chartFormatCollection1.Count; i++)
			{
				if ((chartFormatCollection1[i] != null && chartFormatCollection2[i] == null) || (chartFormatCollection1[i] == null && chartFormatCollection2[i] != null))
				{
					return false;
				}
				if (!CompareChartFormatAsString(chartFormatCollection1[i], chartFormatCollection2[i]))
				{
					return false;
				}
			}
		}
		return true;
	}

	internal bool CompareChartFormatAsString(ChartFormatImpl chartFormat1, ChartFormatImpl chartFormat2)
	{
		if ((object)chartFormat1 != null && (object)chartFormat2 != null)
		{
			if (chartFormat1.IsVeryColor != chartFormat2.IsVeryColor || chartFormat1.IsVaryColor != chartFormat2.IsVaryColor || chartFormat1.IsSeriesName != chartFormat2.IsSeriesName || chartFormat1.IsCategoryName != chartFormat2.IsCategoryName || chartFormat1.IsValue != chartFormat2.IsValue || chartFormat1.IsPercentage != chartFormat2.IsPercentage || chartFormat1.IsBubbleSize != chartFormat2.IsBubbleSize || chartFormat1.RightAngleAxes != chartFormat2.RightAngleAxes || chartFormat1.IsChartExType != chartFormat2.IsChartExType || chartFormat1.AutoScaling != chartFormat2.AutoScaling || chartFormat1.WallsAndGridlines2D != chartFormat2.WallsAndGridlines2D || chartFormat1.IsPrimaryAxis != chartFormat2.IsPrimaryAxis || chartFormat1.IsChartChartLine != chartFormat2.IsChartChartLine || chartFormat1.IsChartLineFormat != chartFormat2.IsChartLineFormat || chartFormat1.IsDropBar != chartFormat2.IsDropBar || chartFormat1.Is3D != chartFormat2.Is3D || chartFormat1.IsMarker != chartFormat2.IsMarker || chartFormat1.IsLine != chartFormat2.IsLine || chartFormat1.IsSmoothed != chartFormat2.IsSmoothed || chartFormat1.HasDropLines != chartFormat2.HasDropLines || chartFormat1.HasHighLowLines != chartFormat2.HasHighLowLines || chartFormat1.HasSeriesLines != chartFormat2.HasSeriesLines || chartFormat1.LineStyle != chartFormat2.LineStyle || chartFormat1.DropLineStyle != chartFormat2.DropLineStyle || chartFormat1.FormatRecordType != chartFormat2.FormatRecordType || chartFormat1.Delimiter != chartFormat2.Delimiter || chartFormat1.Is3D != chartFormat2.Is3D || chartFormat1.DelimiterLength != chartFormat2.DelimiterLength || chartFormat1.Rotation != chartFormat2.Rotation || chartFormat1.Elevation != chartFormat2.Elevation || chartFormat1.Perspective != chartFormat2.Perspective || chartFormat1.HeightPercent != chartFormat2.HeightPercent || chartFormat1.DepthPercent != chartFormat2.DepthPercent || chartFormat1.GapDepth != chartFormat2.GapDepth || chartFormat1.DrawingZOrder != chartFormat2.DrawingZOrder || chartFormat1.SerieFormat.TypeCode != chartFormat2.SerieFormat.TypeCode)
			{
				return false;
			}
			if (chartFormat1.SerieFormat.TypeCode == TBIFFRecord.ChartBar)
			{
				if (chartFormat1.IsHorizontalBar != chartFormat2.IsHorizontalBar || chartFormat1.StackValuesBar != chartFormat2.StackValuesBar || chartFormat1.ShowAsPercentsBar != chartFormat2.ShowAsPercentsBar || chartFormat1.HasShadowBar != chartFormat2.HasShadowBar)
				{
					return false;
				}
				if (!chartFormat1.Is3D && !chartFormat2.Is3D && chartFormat1.Overlap != chartFormat2.Overlap)
				{
					return false;
				}
			}
			if (chartFormat1.SerieFormat.TypeCode == TBIFFRecord.ChartLine && (chartFormat1.StackValuesLine != chartFormat2.StackValuesLine || chartFormat1.ShowAsPercentsLine != chartFormat2.ShowAsPercentsLine || chartFormat1.HasShadowLine != chartFormat2.HasShadowLine))
			{
				return false;
			}
			if (chartFormat1.SerieFormat.TypeCode == TBIFFRecord.ChartPie && (chartFormat1.HasShadowPie != chartFormat2.HasShadowPie || chartFormat1.ShowLeaderLines != chartFormat2.ShowLeaderLines || chartFormat1.FirstSliceAngle != chartFormat2.FirstSliceAngle || chartFormat1.DoughnutHoleSize != chartFormat2.DoughnutHoleSize))
			{
				return false;
			}
			if (chartFormat1.SerieFormat.TypeCode == TBIFFRecord.ChartScatter && (chartFormat1.IsBubbles != chartFormat2.IsBubbles || chartFormat1.ShowNegativeBubbles != chartFormat2.ShowNegativeBubbles || chartFormat1.HasShadowScatter != chartFormat2.HasShadowScatter || chartFormat1.SizeRepresents != chartFormat2.SizeRepresents || chartFormat1.BubbleScale != chartFormat2.BubbleScale))
			{
				return false;
			}
			if (chartFormat1.SerieFormat.TypeCode == TBIFFRecord.ChartArea && (chartFormat1.IsStacked != chartFormat2.IsStacked || chartFormat1.IsCategoryBrokenDown != chartFormat2.IsCategoryBrokenDown || chartFormat1.IsAreaShadowed != chartFormat2.IsAreaShadowed))
			{
				return false;
			}
			if (chartFormat1.SerieFormat.TypeCode == TBIFFRecord.ChartSurface && (chartFormat1.IsFillSurface != chartFormat2.IsFillSurface || chartFormat1.Is3DPhongShade != chartFormat2.Is3DPhongShade))
			{
				return false;
			}
			if (chartFormat1.SerieFormat.TypeCode == TBIFFRecord.ChartRadar && chartFormat1.HasShadowRadar != chartFormat2.HasShadowRadar)
			{
				return false;
			}
			if (chartFormat1.SerieFormat.TypeCode == TBIFFRecord.ChartRadarArea && chartFormat1.HasRadarAxisLabels != chartFormat2.HasRadarAxisLabels)
			{
				return false;
			}
			if (chartFormat1.SerieFormat.TypeCode == TBIFFRecord.ChartBoppop && (chartFormat1.UseDefaultSplitValue != chartFormat2.UseDefaultSplitValue || chartFormat1.HasShadowBoppop != chartFormat2.HasShadowBoppop || chartFormat1.PieChartType != chartFormat2.PieChartType || chartFormat1.SplitType != chartFormat2.SplitType || chartFormat1.SplitValue != chartFormat2.SplitValue || chartFormat1.SplitPercent != chartFormat2.SplitPercent || chartFormat1.GapWidth != chartFormat2.GapWidth || chartFormat1.PieSecondSize != chartFormat2.PieSecondSize || chartFormat1.Gap != chartFormat2.Gap || chartFormat1.NumSplitValue != chartFormat2.NumSplitValue))
			{
				return false;
			}
			if (chartFormat1.Is3D && (chartFormat1.IsDefaultRotation != chartFormat2.IsDefaultRotation || chartFormat1.IsDefaultElevation != chartFormat2.IsDefaultElevation || chartFormat1.IsClustered != chartFormat2.IsClustered))
			{
				return false;
			}
		}
		return true;
	}

	internal bool CompareChartFrameFormatImpl(ChartFrameFormatImpl chartFrameFormatImpl1, ChartFrameFormatImpl chartFrameFormatImpl2)
	{
		if (chartFrameFormatImpl1 != null && chartFrameFormatImpl2 != null)
		{
			if (chartFrameFormatImpl1.HasInterior != chartFrameFormatImpl2.HasInterior || chartFrameFormatImpl1.HasLineProperties != chartFrameFormatImpl2.HasLineProperties || chartFrameFormatImpl1.HasShadowProperties != chartFrameFormatImpl2.HasShadowProperties || chartFrameFormatImpl1.Has3dProperties != chartFrameFormatImpl2.Has3dProperties || chartFrameFormatImpl1.IsAutoSize != chartFrameFormatImpl2.IsAutoSize || chartFrameFormatImpl1.IsAutoPosition != chartFrameFormatImpl2.IsAutoPosition || chartFrameFormatImpl1.IsBorderCornersRound != chartFrameFormatImpl2.IsBorderCornersRound || chartFrameFormatImpl1.IsAutomaticFormat != chartFrameFormatImpl2.IsAutomaticFormat || chartFrameFormatImpl1.Visible != chartFrameFormatImpl2.Visible || chartFrameFormatImpl1.RectangleStyle != chartFrameFormatImpl2.RectangleStyle || chartFrameFormatImpl1.Pattern != chartFrameFormatImpl2.Pattern)
			{
				return false;
			}
			if ((chartFrameFormatImpl1.Workbook != null && chartFrameFormatImpl2.Workbook == null) || (chartFrameFormatImpl1.Workbook == null && chartFrameFormatImpl2.Workbook != null) || (chartFrameFormatImpl1.Layout != null && chartFrameFormatImpl2.Layout == null) || (chartFrameFormatImpl1.Layout == null && chartFrameFormatImpl2.Layout != null) || (chartFrameFormatImpl1.Border != null && chartFrameFormatImpl2.Border == null) || (chartFrameFormatImpl1.Border == null && chartFrameFormatImpl2.Border != null) || (chartFrameFormatImpl1.Interior != null && chartFrameFormatImpl2.Interior == null) || (chartFrameFormatImpl1.Interior == null && chartFrameFormatImpl2.Interior != null) || (chartFrameFormatImpl1.ThreeD != null && chartFrameFormatImpl2.ThreeD == null) || (chartFrameFormatImpl1.ThreeD == null && chartFrameFormatImpl2.ThreeD != null) || (chartFrameFormatImpl1.Shadow != null && chartFrameFormatImpl2.Shadow == null) || (chartFrameFormatImpl1.Shadow == null && chartFrameFormatImpl2.Shadow != null) || (chartFrameFormatImpl1.LineProperties != null && chartFrameFormatImpl2.LineProperties == null) || (chartFrameFormatImpl1.LineProperties == null && chartFrameFormatImpl2.LineProperties != null) || (chartFrameFormatImpl1.ForeGroundColorObject != null && chartFrameFormatImpl2.ForeGroundColorObject == null) || (chartFrameFormatImpl1.ForeGroundColorObject == null && chartFrameFormatImpl2.ForeGroundColorObject != null) || (chartFrameFormatImpl1.BackGroundColorObject != null && chartFrameFormatImpl2.BackGroundColorObject == null) || (chartFrameFormatImpl1.BackGroundColorObject == null && chartFrameFormatImpl2.BackGroundColorObject != null) || (chartFrameFormatImpl1.Fill != null && chartFrameFormatImpl2.Fill == null) || (chartFrameFormatImpl1.Fill == null && chartFrameFormatImpl2.Fill != null))
			{
				return false;
			}
			if (!CompareWorkBookImpl(chartFrameFormatImpl1.Workbook, chartFrameFormatImpl2.Workbook) || !CompareChartLayout(chartFrameFormatImpl1.Layout as ChartLayoutImpl, chartFrameFormatImpl2.Layout as ChartLayoutImpl) || !CompareChartBorder(chartFrameFormatImpl1.Border as ChartBorderImpl, chartFrameFormatImpl2.Border as ChartBorderImpl) || !CompareChartInterior(chartFrameFormatImpl1.Interior as ChartInteriorImpl, chartFrameFormatImpl2.Interior as ChartInteriorImpl) || !CompareThreeDFormat(chartFrameFormatImpl1.ThreeD as ThreeDFormatImpl, chartFrameFormatImpl2.ThreeD as ThreeDFormatImpl) || !CompareShadow(chartFrameFormatImpl1.Shadow as ShadowImpl, chartFrameFormatImpl2.Shadow as ShadowImpl) || !CompareChartBorder(chartFrameFormatImpl1.LineProperties as ChartBorderImpl, chartFrameFormatImpl2.LineProperties as ChartBorderImpl) || !CompareChartColor(chartFrameFormatImpl1.ForeGroundColorObject, chartFrameFormatImpl2.ForeGroundColorObject) || !CompareChartColor(chartFrameFormatImpl1.BackGroundColorObject, chartFrameFormatImpl2.BackGroundColorObject) || !CompareFill(chartFrameFormatImpl1.Fill as ShapeFillImpl, chartFrameFormatImpl2.Fill as ShapeFillImpl))
			{
				return false;
			}
		}
		return true;
	}

	internal bool CompareChartGridLine(ChartGridLineImpl gridLine1, ChartGridLineImpl gridLine2)
	{
		if (gridLine1 != null && gridLine2 != null)
		{
			if (gridLine1.HasLineProperties != gridLine2.HasLineProperties || gridLine1.HasShadowProperties != gridLine2.HasShadowProperties || gridLine1.Has3dProperties != gridLine2.Has3dProperties || gridLine1.HasInterior != gridLine2.HasInterior || gridLine1.AxisLineType != gridLine2.AxisLineType)
			{
				return false;
			}
			if ((gridLine1.Border != null && gridLine2.Border == null) || (gridLine1.Border == null && gridLine2.Border != null) || (gridLine1.ThreeD != null && gridLine2.ThreeD == null) || (gridLine1.ThreeD == null && gridLine2.ThreeD != null) || (gridLine1.Shadow != null && gridLine2.Shadow == null) || (gridLine1.Shadow == null && gridLine2.Shadow != null) || (gridLine1.LineProperties != null && gridLine2.LineProperties == null) || (gridLine1.LineProperties == null && gridLine2.LineProperties != null))
			{
				return false;
			}
			if (gridLine1.HasInterior && !CompareChartInterior(gridLine1.Interior as ChartInteriorImpl, gridLine2.Interior as ChartInteriorImpl))
			{
				return false;
			}
			if (!CompareChartBorder(gridLine1.Border as ChartBorderImpl, gridLine2.Border as ChartBorderImpl) || !CompareThreeDFormat(gridLine1.ThreeD as ThreeDFormatImpl, gridLine2.ThreeD as ThreeDFormatImpl) || !CompareShadow(gridLine1.Shadow as ShadowImpl, gridLine2.Shadow as ShadowImpl) || !CompareChartBorder(gridLine1.LineProperties as ChartBorderImpl, gridLine2.LineProperties as ChartBorderImpl))
			{
				return false;
			}
		}
		return true;
	}

	internal bool ComparePlotGrowthRecord(ChartPlotGrowthRecord chartPlot1, ChartPlotGrowthRecord chartPlot2)
	{
		if (chartPlot1 != null && chartPlot2 != null && (chartPlot1.NeedInfill != chartPlot2.NeedInfill || chartPlot1.NeedDataArray != chartPlot2.NeedDataArray || chartPlot1.IsAllowShortData != chartPlot2.IsAllowShortData || chartPlot1.NeedDecoding != chartPlot2.NeedDecoding || chartPlot1.RecordCode != chartPlot2.RecordCode || chartPlot1.Length != chartPlot2.Length || chartPlot1.StreamPos != chartPlot2.StreamPos || chartPlot1.MinimumRecordSize != chartPlot2.MinimumRecordSize || chartPlot1.MaximumRecordSize != chartPlot2.MaximumRecordSize || chartPlot1.MaximumMemorySize != chartPlot2.MaximumMemorySize || chartPlot1.StartDecodingOffset != chartPlot2.StartDecodingOffset || chartPlot1.HorzGrowth != chartPlot2.HorzGrowth || chartPlot1.VertGrowth != chartPlot2.VertGrowth))
		{
			return false;
		}
		return true;
	}

	internal bool CompareChartPlotAreaLayoutRecord(ChartPlotAreaLayoutRecord plotArea1, ChartPlotAreaLayoutRecord plotArea2)
	{
		if (plotArea1 != null && plotArea2 != null && (plotArea1.NeedInfill != plotArea2.NeedInfill || plotArea1.NeedDataArray != plotArea2.NeedDataArray || plotArea1.IsAllowShortData != plotArea2.IsAllowShortData || plotArea1.NeedDecoding != plotArea2.NeedDecoding || plotArea1.WXMode != plotArea2.WXMode || plotArea1.WYMode != plotArea2.WYMode || plotArea1.WWidthMode != plotArea2.WWidthMode || plotArea1.WHeightMode != plotArea2.WHeightMode || plotArea1.RecordCode != plotArea2.RecordCode || plotArea1.Length != plotArea2.Length || plotArea1.StreamPos != plotArea2.StreamPos || plotArea1.MinimumRecordSize != plotArea2.MinimumRecordSize || plotArea1.MaximumRecordSize != plotArea2.MaximumRecordSize || plotArea1.MaximumMemorySize != plotArea2.MaximumMemorySize || plotArea1.StartDecodingOffset != plotArea2.StartDecodingOffset || plotArea1.xTL != plotArea2.xTL || plotArea1.yTL != plotArea2.yTL || plotArea1.xBR != plotArea2.xBR || plotArea1.yBR != plotArea2.yBR || plotArea1.X != plotArea2.X || plotArea1.Y != plotArea2.Y || plotArea1.Dx != plotArea2.Dx || plotArea1.Dy != plotArea2.Dy))
		{
			return false;
		}
		return true;
	}

	internal bool CompareChartLegend(ChartLegendImpl legend1, ChartLegendImpl legend2)
	{
		if (legend1 != null && legend2 != null)
		{
			if (legend1.IncludeInLayout != legend2.IncludeInLayout || legend1.IsVerticalLegend != legend2.IsVerticalLegend || legend1.IsDefaultTextSettings != legend2.IsDefaultTextSettings || legend1.IsChartTextArea != legend2.IsChartTextArea || legend1.ContainsDataTable != legend2.ContainsDataTable || legend1.AutoPosition != legend2.AutoPosition || legend1.AutoSeries != legend2.AutoSeries || legend1.AutoPositionX != legend2.AutoPositionX || legend1.AutoPositionY != legend2.AutoPositionY || legend1.X != legend2.X || legend1.Y != legend2.Y || legend1.Width != legend2.Width || legend1.Height != legend2.Height || legend1.ChartExPosition != legend2.ChartExPosition || legend1.Position != legend2.Position || legend1.Spacing != legend2.Spacing || legend1.ParagraphType != legend2.ParagraphType)
			{
				return false;
			}
			if ((legend1.TextArea != null && legend2.TextArea == null) || (legend1.TextArea == null && legend2.TextArea != null) || (legend1.FrameFormat != null && legend2.FrameFormat == null) || (legend1.FrameFormat == null && legend2.FrameFormat != null) || (legend1.LegendEntries != null && legend2.LegendEntries == null) || (legend1.LegendEntries == null && legend2.LegendEntries != null) || (legend1.Layout != null && legend2.Layout == null) || (legend1.Layout == null && legend2.Layout != null) || (legend1.AttachedLabelLayout != null && legend2.AttachedLabelLayout == null) || (legend1.AttachedLabelLayout == null && legend2.AttachedLabelLayout != null) || (legend1.LegendRecord != null && legend2.LegendRecord == null) || (legend1.LegendRecord == null && legend2.LegendRecord != null) || (legend1.PositionRecord != null && legend2.PositionRecord == null) || (legend1.PositionRecord == null && legend2.PositionRecord != null))
			{
				return false;
			}
			if (!CompareChartTextArea(legend1.TextArea as ChartTextAreaImpl, legend2.TextArea as ChartTextAreaImpl) || !CompareChartFrameFormatImpl(legend1.FrameFormat as ChartFrameFormatImpl, legend2.FrameFormat as ChartFrameFormatImpl) || !CompareChartLegendEntries(legend1.LegendEntries as ChartLegendEntriesColl, legend2.LegendEntries as ChartLegendEntriesColl) || !CompareChartLayout(legend1.Layout as ChartLayoutImpl, legend2.Layout as ChartLayoutImpl) || !CompareChartAttachedLabelLayoutRecord(legend1.AttachedLabelLayout, legend2.AttachedLabelLayout) || !CompareChartLegendRecord(legend1.LegendRecord, legend2.LegendRecord) || !CompareChartPosRecord(legend1.PositionRecord, legend2.PositionRecord))
			{
				return false;
			}
		}
		return true;
	}

	internal bool CompareChartLegendRecord(ChartLegendRecord legend1, ChartLegendRecord legend2)
	{
		if (legend1 != null && legend2 != null)
		{
			if (legend1.NeedInfill != legend2.NeedInfill || legend1.NeedDataArray != legend2.NeedDataArray || legend1.IsAllowShortData != legend2.IsAllowShortData || legend1.NeedDecoding != legend2.NeedDecoding || legend1.TypeCode != legend2.TypeCode || legend1.RecordCode != legend2.RecordCode || legend1.Length != legend2.Length || legend1.StreamPos != legend2.StreamPos || legend1.MinimumRecordSize != legend2.MinimumRecordSize || legend1.MaximumRecordSize != legend2.MaximumRecordSize || legend1.MaximumMemorySize != legend2.MaximumMemorySize || legend1.StartDecodingOffset != legend2.StartDecodingOffset || legend1.AutoPosition != legend2.AutoPosition || legend1.AutoSeries != legend2.AutoSeries || legend1.AutoPositionX != legend2.AutoPositionX || legend1.AutoPositionY != legend2.AutoPositionY || legend1.IsVerticalLegend != legend2.IsVerticalLegend || legend1.ContainsDataTable != legend2.ContainsDataTable || legend1.X != legend2.X || legend1.Y != legend2.Y || legend1.Width != legend2.Width || legend1.Height != legend2.Height || legend1.Position != legend2.Position || legend1.Spacing != legend2.Spacing)
			{
				return false;
			}
			if ((legend1.Data != null && legend2.Data == null) || (legend1.Data == null && legend2.Data != null))
			{
				return false;
			}
			if (legend1.Data != null && legend2.Data == null && legend1.Data.ToString() != legend2.Data.ToString())
			{
				return false;
			}
		}
		return true;
	}

	internal bool CompareRectangle(Rectangle rect1, Rectangle rect2)
	{
		if (rect1.Width != rect2.Width || rect1.Height != rect2.Height || rect1.X != rect2.X || rect1.Y != rect2.Y)
		{
			return false;
		}
		return true;
	}

	private bool CompareFontWrapper(FontWrapper font1, FontWrapper font2)
	{
		if (font1 != null && font2 != null)
		{
			if (font1.Bold != font2.Bold || font1.Italic != font2.Italic || font1.MacOSOutlineFont != font2.MacOSOutlineFont || font1.MacOSShadow != font2.MacOSShadow || font1.Strikethrough != font2.Strikethrough || font1.Subscript != font2.Subscript || font1.Superscript != font2.Superscript || font1.IsAutoColor != font2.IsAutoColor || font1.IsReadOnly != font2.IsReadOnly || font1.IsDirectAccess != font2.IsDirectAccess || font1.Color != font2.Color || font1.Underline != font2.Underline || font1.VerticalAlignment != font2.VerticalAlignment || font1.Size != font2.Size || font1.Baseline != font2.Baseline || font1.FontName != font2.FontName || font1.CharSet != font2.CharSet || font1.FontIndex != font2.FontIndex || font1.Index != font2.Index)
			{
				return false;
			}
			if ((font1.Wrapped != null && font2.Wrapped == null) || (font1.Wrapped == null && font2.Wrapped != null) || (font1.Font != null && font2.Font == null) || (font1.Font == null && font2.Font != null) || (font1.ColorObject != null && font2.ColorObject == null) || (font1.ColorObject == null && font2.ColorObject != null) || (font1.Workbook != null && font2.Workbook == null) || (font1.Workbook == null && font2.Workbook != null))
			{
				return false;
			}
			_ = font1.RGBColor;
			_ = font2.RGBColor;
			_ = font1.RGBColor;
			_ = font1.RGBColor;
			_ = font2.RGBColor;
			if (font1.RGBColor.ToArgb() != font2.RGBColor.ToArgb())
			{
				return false;
			}
			if (!CompareFont(font1.Wrapped, font2.Wrapped) || !CompareFont(font1.Font, font2.Font) || !CompareChartColor(font1.ColorObject, font2.ColorObject) || !CompareWorkBookImpl(font1.Workbook, font2.Workbook))
			{
				return false;
			}
		}
		return true;
	}
}
