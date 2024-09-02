using System;
using System.Runtime.Serialization;
using DocGen.Drawing;
using DocGen.Styles;

namespace DocGen.Chart;

[Serializable]
[StaticDataField("sd")]
internal class ChartSymbolInfoStore : StyleInfoStore
{
	private static StaticData sd = new StaticData(typeof(ChartSymbolInfoStore), typeof(ChartSymbolInfo), sortProperties: true);

	public static StyleInfoProperty ShapeProperty = sd.CreateStyleInfoProperty(typeof(ChartSymbolShape), "Shape");

	public static StyleInfoProperty ImageIndexProperty = sd.CreateStyleInfoProperty(typeof(int), "ImageIndex");

	public static StyleInfoProperty ColorProperty = sd.CreateStyleInfoProperty(typeof(Color), "Color");

	public static StyleInfoProperty HighlightColorProperty = sd.CreateStyleInfoProperty(typeof(Color), "HighlightColor");

	public static StyleInfoProperty DimmedColorProperty = sd.CreateStyleInfoProperty(typeof(Color), "DimmedColor");

	public static StyleInfoProperty SizeProperty = sd.CreateStyleInfoProperty(typeof(Size), "Size");

	public static StyleInfoProperty OffsetProperty = sd.CreateStyleInfoProperty(typeof(Size), "Offset");

	public static StyleInfoProperty MarkerProperty = sd.CreateStyleInfoProperty(typeof(ChartMarker), "Marker");

	public static StyleInfoProperty BorderProperty = sd.CreateStyleInfoProperty(typeof(ChartLineInfo), "Border");

	protected override StaticData StaticDataStore => sd;

	public ChartSymbolInfoStore()
	{
	}

	private ChartSymbolInfoStore(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	internal static ChartSymbolInfoStore InitializeStaticData()
	{
		if (sd == null)
		{
			sd = new StaticData(typeof(ChartSymbolInfoStore), typeof(ChartSymbolInfo), sortProperties: true);
			ShapeProperty = sd.CreateStyleInfoProperty(typeof(ChartSymbolShape), "Shape");
			ImageIndexProperty = sd.CreateStyleInfoProperty(typeof(int), "ImageIndex");
			ColorProperty = sd.CreateStyleInfoProperty(typeof(Color), "Color");
			HighlightColorProperty = sd.CreateStyleInfoProperty(typeof(Color), "HighlightColor");
			DimmedColorProperty = sd.CreateStyleInfoProperty(typeof(Color), "DimmedColor");
			SizeProperty = sd.CreateStyleInfoProperty(typeof(Size), "Size");
			OffsetProperty = sd.CreateStyleInfoProperty(typeof(Size), "Offset");
			MarkerProperty = sd.CreateStyleInfoProperty(typeof(ChartMarker), "Marker");
			BorderProperty = sd.CreateStyleInfoProperty(typeof(ChartLineInfo), "Border");
		}
		return new ChartSymbolInfoStore();
	}

	public new void Dispose()
	{
		if (sd != null)
		{
			sd.Dispose();
			sd = null;
		}
		if (ShapeProperty != null)
		{
			ShapeProperty.Dispose();
			ShapeProperty = null;
		}
		if (ImageIndexProperty != null)
		{
			ImageIndexProperty.Dispose();
			ImageIndexProperty = null;
		}
		if (ColorProperty != null)
		{
			ColorProperty.Dispose();
			ColorProperty = null;
		}
		if (HighlightColorProperty != null)
		{
			HighlightColorProperty.Dispose();
			HighlightColorProperty = null;
		}
		if (DimmedColorProperty != null)
		{
			DimmedColorProperty.Dispose();
			DimmedColorProperty = null;
		}
		if (SizeProperty != null)
		{
			SizeProperty.Dispose();
			SizeProperty = null;
		}
		if (OffsetProperty != null)
		{
			OffsetProperty.Dispose();
			OffsetProperty = null;
		}
		if (MarkerProperty != null)
		{
			MarkerProperty.Dispose();
			MarkerProperty = null;
		}
		if (BorderProperty != null)
		{
			BorderProperty.Dispose();
			BorderProperty = null;
		}
		base.Dispose();
	}

	public override object Clone()
	{
		StyleInfoStore styleInfoStore = new ChartSymbolInfoStore();
		CopyTo(styleInfoStore);
		return styleInfoStore;
	}
}
