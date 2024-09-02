using System;
using System.Runtime.Serialization;
using DocGen.Drawing;
using DocGen.Styles;

namespace DocGen.Chart;

[Serializable]
[StaticDataField("sd")]
internal class ChartLineInfoStore : StyleInfoStore
{
	private static StaticData sd = new StaticData(typeof(ChartLineInfoStore), typeof(ChartLineInfo), sortProperties: true);

	public static StyleInfoProperty ColorProperty = sd.CreateStyleInfoProperty(typeof(Color), "Color");

	public static StyleInfoProperty WidthProperty = sd.CreateStyleInfoProperty(typeof(float), "Width");

	public static StyleInfoProperty AlignmentProperty = sd.CreateStyleInfoProperty(typeof(PenAlignment), "Alignment");

	public static StyleInfoProperty DashStyleProperty = sd.CreateStyleInfoProperty(typeof(DashStyle), "DashStyle");

	public static StyleInfoProperty DashPatternProperty = sd.CreateStyleInfoProperty(typeof(float[]), "DashPattern");

	internal static StyleInfoProperty DashCapProperty = sd.CreateStyleInfoProperty(typeof(DashCap), "DashCap");

	protected override StaticData StaticDataStore => sd;

	internal static ChartLineInfoStore InitializeStaticData()
	{
		if (sd == null)
		{
			sd = new StaticData(typeof(ChartLineInfoStore), typeof(ChartLineInfo), sortProperties: true);
			ColorProperty = sd.CreateStyleInfoProperty(typeof(Color), "Color");
			WidthProperty = sd.CreateStyleInfoProperty(typeof(float), "Width");
			AlignmentProperty = sd.CreateStyleInfoProperty(typeof(PenAlignment), "Alignment");
			DashStyleProperty = sd.CreateStyleInfoProperty(typeof(DashStyle), "DashStyle");
			DashPatternProperty = sd.CreateStyleInfoProperty(typeof(float[]), "DashPattern");
		}
		return new ChartLineInfoStore();
	}

	public ChartLineInfoStore()
	{
	}

	private ChartLineInfoStore(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public new void Dispose()
	{
		if (sd != null)
		{
			sd.Dispose();
			sd = null;
		}
		if (AlignmentProperty != null)
		{
			AlignmentProperty.Dispose();
			AlignmentProperty = null;
		}
		if (ColorProperty != null)
		{
			ColorProperty.Dispose();
			ColorProperty = null;
		}
		if (WidthProperty != null)
		{
			WidthProperty.Dispose();
			WidthProperty = null;
		}
		if (DashPatternProperty != null)
		{
			DashPatternProperty.Dispose();
			DashPatternProperty = null;
		}
		if (DashStyleProperty != null)
		{
			DashStyleProperty.Dispose();
			DashStyleProperty = null;
		}
		base.Dispose();
	}

	public override object Clone()
	{
		StyleInfoStore styleInfoStore = new ChartLineInfoStore();
		CopyTo(styleInfoStore);
		return styleInfoStore;
	}
}
