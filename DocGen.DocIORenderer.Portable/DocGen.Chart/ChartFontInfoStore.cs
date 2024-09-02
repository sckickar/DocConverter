using System;
using System.Runtime.Serialization;
using DocGen.Drawing;
using DocGen.Drawing.SkiaSharpHelper;
using DocGen.Styles;

namespace DocGen.Chart;

[Serializable]
[StaticDataField("sd")]
internal class ChartFontInfoStore : StyleInfoStore
{
	private const int MaxOrientation = 360;

	private const int MaxSize = 512;

	private const int MaxUnit = 6;

	private static StaticData sd;

	public static StyleInfoProperty FacenameProperty;

	public static StyleInfoProperty SizeProperty;

	public static StyleInfoProperty BoldProperty;

	public static StyleInfoProperty ItalicProperty;

	public static StyleInfoProperty UnderlineProperty;

	public static StyleInfoProperty StrikeoutProperty;

	public static StyleInfoProperty OrientationProperty;

	public static StyleInfoProperty UnitProperty;

	public static StyleInfoProperty FontFamilyTemplateProperty;

	protected override StaticData StaticDataStore => sd;

	internal static ChartFontInfoStore InitializeStaticVariables()
	{
		if (sd == null)
		{
			sd = new StaticData(typeof(ChartFontInfoStore), typeof(ChartFontInfo), sortProperties: true);
			FacenameProperty = sd.CreateStyleInfoProperty(typeof(string), "Facename");
			SizeProperty = sd.CreateStyleInfoProperty(typeof(short), "Size", 512, makeBitValue: true);
			BoldProperty = sd.CreateStyleInfoProperty(typeof(bool), "Bold", 1, makeBitValue: true);
			ItalicProperty = sd.CreateStyleInfoProperty(typeof(bool), "Italic", 1, makeBitValue: true);
			UnderlineProperty = sd.CreateStyleInfoProperty(typeof(bool), "Underline", 1, makeBitValue: true);
			StrikeoutProperty = sd.CreateStyleInfoProperty(typeof(bool), "Strikeout", 1, makeBitValue: true);
			OrientationProperty = sd.CreateStyleInfoProperty(typeof(short), "Orientation", 360, makeBitValue: true);
			UnitProperty = sd.CreateStyleInfoProperty(typeof(GraphicsUnit), "Unit", 6, makeBitValue: true);
			FontFamilyTemplateProperty = sd.CreateStyleInfoProperty(typeof(FontFamily), "FontFamilyTemplate");
		}
		return new ChartFontInfoStore();
	}

	public ChartFontInfoStore()
	{
	}

	private ChartFontInfoStore(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public override object Clone()
	{
		StyleInfoStore styleInfoStore = new ChartFontInfoStore();
		CopyTo(styleInfoStore);
		return styleInfoStore;
	}

	public new void Dispose()
	{
		if (sd != null)
		{
			sd.Dispose();
			sd = null;
		}
		if (FacenameProperty != null)
		{
			FacenameProperty.Dispose();
			FacenameProperty = null;
		}
		if (SizeProperty != null)
		{
			SizeProperty.Dispose();
			SizeProperty = null;
		}
		if (BoldProperty != null)
		{
			BoldProperty.Dispose();
			BoldProperty = null;
		}
		if (ItalicProperty != null)
		{
			ItalicProperty.Dispose();
			ItalicProperty = null;
		}
		if (UnderlineProperty != null)
		{
			UnderlineProperty.Dispose();
			UnderlineProperty = null;
		}
		if (OrientationProperty != null)
		{
			OrientationProperty.Dispose();
			OrientationProperty = null;
		}
		if (FontFamilyTemplateProperty != null)
		{
			FontFamilyTemplateProperty.Dispose();
			FontFamilyTemplateProperty = null;
		}
		if (UnitProperty != null)
		{
			UnitProperty.Dispose();
			UnitProperty = null;
		}
		if (StrikeoutProperty != null)
		{
			StrikeoutProperty.Dispose();
			StrikeoutProperty = null;
		}
		base.Dispose();
	}

	static ChartFontInfoStore()
	{
		sd = new StaticData(typeof(ChartFontInfoStore), typeof(ChartFontInfo), sortProperties: true);
		FacenameProperty = sd.CreateStyleInfoProperty(typeof(string), "Facename");
		SizeProperty = sd.CreateStyleInfoProperty(typeof(short), "Size", 512, makeBitValue: true);
		BoldProperty = sd.CreateStyleInfoProperty(typeof(bool), "Bold", 1, makeBitValue: true);
		ItalicProperty = sd.CreateStyleInfoProperty(typeof(bool), "Italic", 1, makeBitValue: true);
		UnderlineProperty = sd.CreateStyleInfoProperty(typeof(bool), "Underline", 1, makeBitValue: true);
		StrikeoutProperty = sd.CreateStyleInfoProperty(typeof(bool), "Strikeout", 1, makeBitValue: true);
		OrientationProperty = sd.CreateStyleInfoProperty(typeof(short), "Orientation", 360, makeBitValue: true);
		UnitProperty = sd.CreateStyleInfoProperty(typeof(GraphicsUnit), "Unit", 6, makeBitValue: true);
		FontFamilyTemplateProperty = sd.CreateStyleInfoProperty(typeof(FontFamily), "FontFamilyTemplate");
		SizeProperty.Format += SizeToString;
		SizeProperty.Parse += SizeStringToInt;
		SizeProperty.WriteXml += SizeProperty_WriteXml;
		SizeProperty.ReadXml += SizeProperty_ReadXml;
	}

	private static void SizeToString(object sender, StyleInfoPropertyConvertEventArgs cevent)
	{
		if (!cevent.Handled && !(cevent.DesiredType != typeof(string)))
		{
			cevent.Value = ((float)(short)cevent.Value / 4f).ToString();
			cevent.Handled = true;
		}
	}

	private static void SizeStringToInt(object sender, StyleInfoPropertyConvertEventArgs cevent)
	{
		if (!cevent.Handled)
		{
			if (cevent.Value != null)
			{
				cevent.Value = (short)(float.Parse(cevent.Value.ToString()) * 4f);
			}
			cevent.Handled = true;
		}
	}

	private static void SizeProperty_WriteXml(object sender, StyleInfoPropertyWriteXmlEventArgs e)
	{
		StyleInfoProperty sip = e.Sip;
		e.Writer.WriteStartElement(sip.PropertyName);
		e.Writer.WriteString(sip.FormatValue(e.Store.GetValue(sip)));
		e.Writer.WriteEndElement();
		e.Handled = true;
	}

	private static void SizeProperty_ReadXml(object sender, StyleInfoPropertyReadXmlEventArgs e)
	{
		string s = e.Reader.ReadString();
		object value = e.Sip.ParseValue(s);
		e.Store.SetValue(e.Sip, value);
		e.Reader.Read();
		e.Handled = true;
	}
}
