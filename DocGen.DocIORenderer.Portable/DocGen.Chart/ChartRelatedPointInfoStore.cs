using System;
using System.Runtime.Serialization;
using DocGen.Drawing;
using DocGen.Styles;

namespace DocGen.Chart;

[Serializable]
[StaticDataField("sd")]
internal class ChartRelatedPointInfoStore : StyleInfoStore
{
	private static StaticData sd;

	public static StyleInfoProperty PointsProperty;

	public static StyleInfoProperty ColorProperty;

	public static StyleInfoProperty WidthProperty;

	public static StyleInfoProperty AlignmentProperty;

	public static StyleInfoProperty DashStyleProperty;

	public static StyleInfoProperty DashPatternProperty;

	public static StyleInfoProperty StartSymbolProperty;

	public static StyleInfoProperty EndSymbolProperty;

	public static StyleInfoProperty BorderProperty;

	protected override StaticData StaticDataStore => sd;

	public ChartRelatedPointInfoStore()
	{
	}

	private ChartRelatedPointInfoStore(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public override object Clone()
	{
		StyleInfoStore styleInfoStore = new ChartRelatedPointInfoStore();
		CopyTo(styleInfoStore);
		return styleInfoStore;
	}

	static ChartRelatedPointInfoStore()
	{
		sd = new StaticData(typeof(ChartRelatedPointInfoStore), typeof(ChartRelatedPointInfo), sortProperties: true);
		PointsProperty = sd.CreateStyleInfoProperty(typeof(int[]), "Points");
		ColorProperty = sd.CreateStyleInfoProperty(typeof(Color), "Color");
		WidthProperty = sd.CreateStyleInfoProperty(typeof(float), "Width");
		AlignmentProperty = sd.CreateStyleInfoProperty(typeof(PenAlignment), "Alignment");
		DashStyleProperty = sd.CreateStyleInfoProperty(typeof(DashStyle), "DashStyle");
		DashPatternProperty = sd.CreateStyleInfoProperty(typeof(float[]), "DashPattern");
		StartSymbolProperty = sd.CreateStyleInfoProperty(typeof(ChartRelatedPointSymbolInfo), "StartSymbol");
		EndSymbolProperty = sd.CreateStyleInfoProperty(typeof(ChartRelatedPointSymbolInfo), "EndSymbol");
		BorderProperty = sd.CreateStyleInfoProperty(typeof(ChartRelatedPointLineInfo), "Border");
	}
}
