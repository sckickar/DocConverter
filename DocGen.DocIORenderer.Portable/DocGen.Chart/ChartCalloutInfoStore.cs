using System;
using DocGen.Drawing;
using DocGen.Styles;

namespace DocGen.Chart;

[Serializable]
[StaticDataField("sd")]
internal class ChartCalloutInfoStore : StyleInfoStore
{
	private static StaticData sd = new StaticData(typeof(ChartCalloutInfoStore), typeof(ChartCalloutInfo), sortProperties: true);

	internal static StyleInfoProperty EnableProperty = sd.CreateStyleInfoProperty(typeof(bool), "Enable");

	internal static StyleInfoProperty TextProperty = sd.CreateStyleInfoProperty(typeof(string), "Text");

	internal static StyleInfoProperty TextOffsetProperty = sd.CreateStyleInfoProperty(typeof(string), "TextOffset");

	internal static StyleInfoProperty OffsetXProperty = sd.CreateStyleInfoProperty(typeof(float), "OffsetX");

	internal static StyleInfoProperty OffsetYProperty = sd.CreateStyleInfoProperty(typeof(string), "OffsetY");

	internal static StyleInfoProperty FontProperty = sd.CreateStyleInfoProperty(typeof(ChartFontInfo), "Font");

	internal static StyleInfoProperty TextFormatProperty = sd.CreateStyleInfoProperty(typeof(string), "TextFormat");

	internal static StyleInfoProperty PositionProperty = sd.CreateStyleInfoProperty(typeof(LabelPosition), "Position");

	internal static StyleInfoProperty ColorProperty = sd.CreateStyleInfoProperty(typeof(Color), "Color");

	internal static StyleInfoProperty TextColorProperty = sd.CreateStyleInfoProperty(typeof(Color), "TextColor");

	internal static StyleInfoProperty BorderProperty = sd.CreateStyleInfoProperty(typeof(ChartLineInfo), "Border");

	protected override StaticData StaticDataStore => sd;

	internal static ChartCalloutInfoStore InitializeStaticData()
	{
		if (sd == null)
		{
			sd = new StaticData(typeof(ChartCalloutInfoStore), typeof(ChartCalloutInfo), sortProperties: true);
			EnableProperty = sd.CreateStyleInfoProperty(typeof(bool), "Enable");
			TextProperty = sd.CreateStyleInfoProperty(typeof(string), "Text");
			TextOffsetProperty = sd.CreateStyleInfoProperty(typeof(string), "TextOffset");
			OffsetXProperty = sd.CreateStyleInfoProperty(typeof(float), "OffsetX");
			OffsetYProperty = sd.CreateStyleInfoProperty(typeof(float), "OffsetY");
			FontProperty = sd.CreateStyleInfoProperty(typeof(ChartFontInfo), "Font");
			TextFormatProperty = sd.CreateStyleInfoProperty(typeof(string), "TextFormat");
			PositionProperty = sd.CreateStyleInfoProperty(typeof(LabelPosition), "Position");
			ColorProperty = sd.CreateStyleInfoProperty(typeof(Color), "Color");
			TextColorProperty = sd.CreateStyleInfoProperty(typeof(Color), "TextColor");
			BorderProperty = sd.CreateStyleInfoProperty(typeof(ChartLineInfo), "Border");
		}
		return new ChartCalloutInfoStore();
	}

	public new void Dispose()
	{
		if (sd != null)
		{
			sd.Dispose();
			sd = null;
		}
		if (EnableProperty != null)
		{
			EnableProperty.Dispose();
			EnableProperty = null;
		}
		if (TextProperty != null)
		{
			TextProperty.Dispose();
			TextProperty = null;
		}
		if (TextOffsetProperty != null)
		{
			TextOffsetProperty.Dispose();
			TextOffsetProperty = null;
		}
		if (OffsetXProperty != null)
		{
			OffsetXProperty.Dispose();
			OffsetXProperty = null;
		}
		if (OffsetYProperty != null)
		{
			OffsetYProperty.Dispose();
			OffsetYProperty = null;
		}
		if (FontProperty != null)
		{
			FontProperty.Dispose();
			FontProperty = null;
		}
		if (TextFormatProperty != null)
		{
			TextFormatProperty.Dispose();
			TextFormatProperty = null;
		}
		if (PositionProperty != null)
		{
			PositionProperty.Dispose();
			PositionProperty = null;
		}
		if (ColorProperty != null)
		{
			ColorProperty.Dispose();
			ColorProperty = null;
		}
		if (TextColorProperty != null)
		{
			TextColorProperty.Dispose();
			TextColorProperty = null;
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
		StyleInfoStore styleInfoStore = new ChartCalloutInfoStore();
		CopyTo(styleInfoStore);
		return styleInfoStore;
	}
}
