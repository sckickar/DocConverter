using System;
using System.Collections;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Schema;
using DocGen.Chart.Drawing;
using DocGen.Drawing;
using DocGen.Styles;

namespace DocGen.Chart;

[Serializable]
internal class ChartStyleInfoStore : StyleInfoStore
{
	private static StaticData sd;

	internal static StyleInfoProperty TextColorProperty;

	internal static StyleInfoProperty TextShapeProperty;

	internal static StyleInfoProperty BaseStyleProperty;

	internal static StyleInfoProperty AltTagFormatProperty;

	internal static StyleInfoProperty FontProperty;

	internal static StyleInfoProperty InteriorProperty;

	internal static StyleInfoProperty TextProperty;

	internal static StyleInfoProperty ToolTipProperty;

	internal static StyleInfoProperty ImagesProperty;

	internal static StyleInfoProperty ImageIndexProperty;

	internal static StyleInfoProperty SymbolProperty;

	internal static StyleInfoProperty CalloutProperty;

	internal static StyleInfoProperty SystemProperty;

	internal static StyleInfoProperty NameProperty;

	internal static StyleInfoProperty TextOrientationProperty;

	internal static StyleInfoProperty DisplayShadowProperty;

	internal static StyleInfoProperty ShadowOffsetProperty;

	internal static StyleInfoProperty ShadowInteriorProperty;

	internal static StyleInfoProperty HighlightInteriorProperty;

	internal static StyleInfoProperty DimmedInteriorProperty;

	internal static StyleInfoProperty HighlightOnMouseOverProperty;

	internal static StyleInfoProperty HitTestRadiusProperty;

	internal static StyleInfoProperty LabelProperty;

	internal static StyleInfoProperty PointWidthProperty;

	internal static StyleInfoProperty TextOffsetProperty;

	internal static StyleInfoProperty BorderProperty;

	internal static StyleInfoProperty DisplayTextProperty;

	internal static StyleInfoProperty DrawTextShapeProperty;

	internal static StyleInfoProperty TextFormatProperty;

	internal static StyleInfoProperty FormatProperty;

	internal static StyleInfoProperty ToolTipFormatProperty;

	internal static StyleInfoProperty ElementBordersProperty;

	internal static StyleInfoProperty RelatedPointsProperty;

	internal static StyleInfoProperty UrlProperty;

	internal static StaticData StaticData => sd;

	protected override StaticData StaticDataStore => sd;

	static ChartStyleInfoStore()
	{
		sd = new StaticData(typeof(ChartStyleInfoStore), typeof(ChartStyleInfo), sortProperties: false);
		TextColorProperty = sd.CreateStyleInfoProperty(typeof(Color), "TextColor");
		TextShapeProperty = sd.CreateStyleInfoProperty(typeof(ChartCustomShapeInfo), "TextShapeProperty");
		BaseStyleProperty = sd.CreateStyleInfoProperty(typeof(string), "BaseStyle");
		AltTagFormatProperty = sd.CreateStyleInfoProperty(typeof(string), "AltTagFormat");
		FontProperty = sd.CreateStyleInfoProperty(typeof(ChartFontInfo), "Font");
		InteriorProperty = sd.CreateStyleInfoProperty(typeof(BrushInfo), "Interior");
		TextProperty = sd.CreateStyleInfoProperty(typeof(string), "Text");
		ToolTipProperty = sd.CreateStyleInfoProperty(typeof(string), "ToolTip");
		ImagesProperty = sd.CreateStyleInfoProperty(typeof(ChartImageCollection), "Images");
		ImageIndexProperty = sd.CreateStyleInfoProperty(typeof(int), "ImageIndex");
		SymbolProperty = sd.CreateStyleInfoProperty(typeof(ChartSymbolInfo), "Symbol");
		CalloutProperty = sd.CreateStyleInfoProperty(typeof(ChartCalloutInfo), "Callout");
		SystemProperty = sd.CreateStyleInfoProperty(typeof(bool), "System");
		NameProperty = sd.CreateStyleInfoProperty(typeof(string), "Name");
		TextOrientationProperty = sd.CreateStyleInfoProperty(typeof(ChartTextOrientation), "TextOrientation");
		DisplayShadowProperty = sd.CreateStyleInfoProperty(typeof(bool), "DisplayShadow");
		ShadowOffsetProperty = sd.CreateStyleInfoProperty(typeof(Size), "ShadowOffset");
		ShadowInteriorProperty = sd.CreateStyleInfoProperty(typeof(BrushInfo), "ShadowInterior");
		HighlightInteriorProperty = sd.CreateStyleInfoProperty(typeof(BrushInfo), "HighlightInterior");
		DimmedInteriorProperty = sd.CreateStyleInfoProperty(typeof(BrushInfo), "DimmedInterior");
		HighlightOnMouseOverProperty = sd.CreateStyleInfoProperty(typeof(bool), "HighlightOnMouseOver");
		HitTestRadiusProperty = sd.CreateStyleInfoProperty(typeof(float), "HitTestRadius");
		LabelProperty = sd.CreateStyleInfoProperty(typeof(string), "Label");
		PointWidthProperty = sd.CreateStyleInfoProperty(typeof(float), "PointWidth");
		TextOffsetProperty = sd.CreateStyleInfoProperty(typeof(float), "TextOffset");
		BorderProperty = sd.CreateStyleInfoProperty(typeof(ChartLineInfo), "Border");
		DisplayTextProperty = sd.CreateStyleInfoProperty(typeof(bool), "DisplayText");
		DrawTextShapeProperty = sd.CreateStyleInfoProperty(typeof(bool), "DrawTextShapeProperty");
		TextFormatProperty = sd.CreateStyleInfoProperty(typeof(string), "TextFormat");
		FormatProperty = sd.CreateStyleInfoProperty(typeof(StringFormat), "Format");
		ToolTipFormatProperty = sd.CreateStyleInfoProperty(typeof(string), "ToolTipFormat");
		ElementBordersProperty = sd.CreateStyleInfoProperty(typeof(ChartBordersInfo), "ElementBorders");
		RelatedPointsProperty = sd.CreateStyleInfoProperty(typeof(ChartRelatedPointInfo), "RelatedPoints");
		UrlProperty = sd.CreateStyleInfoProperty(typeof(string), "Url");
		FontProperty.CreateObject = ChartFontInfo.CreateObject;
	}

	public ChartStyleInfoStore()
	{
	}

	protected ChartStyleInfoStore(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		if (sd.IsEmpty)
		{
			new ChartStyleInfo();
		}
	}

	internal static ChartStyleInfoStore InitializeStaticVariables()
	{
		if (sd == null)
		{
			sd = new StaticData(typeof(ChartStyleInfoStore), typeof(ChartStyleInfo), sortProperties: false);
			TextColorProperty = sd.CreateStyleInfoProperty(typeof(Color), "TextColor");
			TextShapeProperty = sd.CreateStyleInfoProperty(typeof(ChartCustomShapeInfo), "TextShapeProperty");
			BaseStyleProperty = sd.CreateStyleInfoProperty(typeof(string), "BaseStyle");
			AltTagFormatProperty = sd.CreateStyleInfoProperty(typeof(string), "AltTagFormat");
			FontProperty = sd.CreateStyleInfoProperty(typeof(ChartFontInfo), "Font");
			InteriorProperty = sd.CreateStyleInfoProperty(typeof(BrushInfo), "Interior");
			TextProperty = sd.CreateStyleInfoProperty(typeof(string), "Text");
			ToolTipProperty = sd.CreateStyleInfoProperty(typeof(string), "ToolTip");
			ImagesProperty = sd.CreateStyleInfoProperty(typeof(ChartImageCollection), "Images");
			ImageIndexProperty = sd.CreateStyleInfoProperty(typeof(int), "ImageIndex");
			SymbolProperty = sd.CreateStyleInfoProperty(typeof(ChartSymbolInfo), "Symbol");
			CalloutProperty = sd.CreateStyleInfoProperty(typeof(ChartCalloutInfo), "Callout");
			SystemProperty = sd.CreateStyleInfoProperty(typeof(bool), "System");
			NameProperty = sd.CreateStyleInfoProperty(typeof(string), "Name");
			TextOrientationProperty = sd.CreateStyleInfoProperty(typeof(ChartTextOrientation), "TextOrientation");
			DisplayShadowProperty = sd.CreateStyleInfoProperty(typeof(bool), "DisplayShadow");
			ShadowOffsetProperty = sd.CreateStyleInfoProperty(typeof(Size), "ShadowOffset");
			ShadowInteriorProperty = sd.CreateStyleInfoProperty(typeof(BrushInfo), "ShadowInterior");
			HighlightInteriorProperty = sd.CreateStyleInfoProperty(typeof(BrushInfo), "HighlightInterior");
			DimmedInteriorProperty = sd.CreateStyleInfoProperty(typeof(BrushInfo), "DimmedInterior");
			HighlightOnMouseOverProperty = sd.CreateStyleInfoProperty(typeof(bool), "HighlightOnMouseOver");
			HitTestRadiusProperty = sd.CreateStyleInfoProperty(typeof(float), "HitTestRadius");
			LabelProperty = sd.CreateStyleInfoProperty(typeof(string), "Label");
			PointWidthProperty = sd.CreateStyleInfoProperty(typeof(float), "PointWidth");
			TextOffsetProperty = sd.CreateStyleInfoProperty(typeof(float), "TextOffset");
			BorderProperty = sd.CreateStyleInfoProperty(typeof(ChartLineInfo), "Border");
			DisplayTextProperty = sd.CreateStyleInfoProperty(typeof(bool), "DisplayText");
			DrawTextShapeProperty = sd.CreateStyleInfoProperty(typeof(bool), "DrawTextShapeProperty");
			TextFormatProperty = sd.CreateStyleInfoProperty(typeof(string), "TextFormat");
			FormatProperty = sd.CreateStyleInfoProperty(typeof(StringFormat), "Format");
			ToolTipFormatProperty = sd.CreateStyleInfoProperty(typeof(string), "ToolTipFormat");
			ElementBordersProperty = sd.CreateStyleInfoProperty(typeof(ChartBordersInfo), "ElementBorders");
			RelatedPointsProperty = sd.CreateStyleInfoProperty(typeof(ChartRelatedPointInfo), "RelatedPoints");
			UrlProperty = sd.CreateStyleInfoProperty(typeof(string), "Url");
		}
		return new ChartStyleInfoStore();
	}

	public new void Dispose()
	{
		if (TextProperty != null)
		{
			TextProperty.Dispose();
			TextProperty = null;
		}
		if (TextColorProperty != null)
		{
			TextColorProperty.Dispose();
			TextColorProperty = null;
		}
		if (TextShapeProperty != null)
		{
			TextShapeProperty.Dispose();
			TextShapeProperty = null;
		}
		if (BaseStyleProperty != null)
		{
			BaseStyleProperty.Dispose();
			BaseStyleProperty = null;
		}
		if (AltTagFormatProperty != null)
		{
			AltTagFormatProperty.Dispose();
			AltTagFormatProperty = null;
		}
		if (FontProperty != null)
		{
			FontProperty.Dispose();
			FontProperty = null;
		}
		if (InteriorProperty != null)
		{
			InteriorProperty.Dispose();
			InteriorProperty = null;
		}
		if (ToolTipProperty != null)
		{
			ToolTipProperty.Dispose();
			ToolTipProperty = null;
		}
		if (ImagesProperty != null)
		{
			ImagesProperty.Dispose();
			ImagesProperty = null;
		}
		if (ImageIndexProperty != null)
		{
			ImageIndexProperty.Dispose();
			ImageIndexProperty = null;
		}
		if (SymbolProperty != null)
		{
			SymbolProperty.Dispose();
			SymbolProperty = null;
		}
		if (CalloutProperty != null)
		{
			CalloutProperty.Dispose();
			CalloutProperty = null;
		}
		if (SystemProperty != null)
		{
			SystemProperty.Dispose();
			SystemProperty = null;
		}
		if (NameProperty != null)
		{
			NameProperty.Dispose();
			NameProperty = null;
		}
		if (TextOrientationProperty != null)
		{
			TextOrientationProperty.Dispose();
			TextOrientationProperty = null;
		}
		if (DisplayShadowProperty != null)
		{
			DisplayShadowProperty.Dispose();
			DisplayShadowProperty = null;
		}
		if (ShadowOffsetProperty != null)
		{
			ShadowOffsetProperty.Dispose();
			ShadowOffsetProperty = null;
		}
		if (ShadowInteriorProperty != null)
		{
			ShadowInteriorProperty.Dispose();
			ShadowInteriorProperty = null;
		}
		if (HighlightInteriorProperty != null)
		{
			HighlightInteriorProperty.Dispose();
			HighlightInteriorProperty = null;
		}
		if (DimmedInteriorProperty != null)
		{
			DimmedInteriorProperty.Dispose();
			DimmedInteriorProperty = null;
		}
		if (HighlightOnMouseOverProperty != null)
		{
			HighlightOnMouseOverProperty.Dispose();
			HighlightOnMouseOverProperty = null;
		}
		if (HitTestRadiusProperty != null)
		{
			HitTestRadiusProperty.Dispose();
			HitTestRadiusProperty = null;
		}
		if (LabelProperty != null)
		{
			LabelProperty.Dispose();
			LabelProperty = null;
		}
		if (PointWidthProperty != null)
		{
			PointWidthProperty.Dispose();
			PointWidthProperty = null;
		}
		if (TextOffsetProperty != null)
		{
			TextOffsetProperty.Dispose();
			TextOffsetProperty = null;
		}
		if (BorderProperty != null)
		{
			BorderProperty.Dispose();
			BorderProperty = null;
		}
		if (DisplayTextProperty != null)
		{
			DisplayTextProperty.Dispose();
			DisplayTextProperty = null;
		}
		if (DrawTextShapeProperty != null)
		{
			DrawTextShapeProperty.Dispose();
			DrawTextShapeProperty = null;
		}
		if (TextFormatProperty != null)
		{
			TextFormatProperty.Dispose();
			TextFormatProperty = null;
		}
		if (FormatProperty != null)
		{
			FormatProperty.Dispose();
			FormatProperty = null;
		}
		if (ToolTipFormatProperty != null)
		{
			ToolTipFormatProperty.Dispose();
			ToolTipFormatProperty = null;
		}
		if (RelatedPointsProperty != null)
		{
			RelatedPointsProperty.Dispose();
			RelatedPointsProperty = null;
		}
		if (ElementBordersProperty != null)
		{
			ElementBordersProperty.Dispose();
			ElementBordersProperty = null;
		}
		if (UrlProperty != null)
		{
			UrlProperty.Dispose();
			UrlProperty = null;
		}
		if (sd != null)
		{
			sd.Dispose();
			sd = null;
		}
		base.Dispose();
	}

	public override object Clone()
	{
		StyleInfoStore styleInfoStore = new ChartStyleInfoStore();
		CopyTo(styleInfoStore);
		return styleInfoStore;
	}

	public new static XmlSchema GetSchema()
	{
		XmlSchema xmlSchema = new XmlSchema();
		StyleInfoProperty[] array = new StyleInfoProperty[30]
		{
			TextColorProperty, BaseStyleProperty, FontProperty, InteriorProperty, TextProperty, ToolTipProperty, ImagesProperty, ImageIndexProperty, SymbolProperty, CalloutProperty,
			SystemProperty, NameProperty, TextOrientationProperty, DisplayShadowProperty, ShadowOffsetProperty, ShadowInteriorProperty, HighlightInteriorProperty, DimmedInteriorProperty, HighlightOnMouseOverProperty, HitTestRadiusProperty,
			LabelProperty, PointWidthProperty, TextOffsetProperty, BorderProperty, DisplayTextProperty, TextFormatProperty, FormatProperty, ToolTipFormatProperty, ElementBordersProperty, RelatedPointsProperty
		};
		StyleInfoProperty[] array2 = new StyleInfoProperty[5]
		{
			ChartLineInfoStore.AlignmentProperty,
			ChartLineInfoStore.ColorProperty,
			ChartLineInfoStore.DashPatternProperty,
			ChartLineInfoStore.DashStyleProperty,
			ChartLineInfoStore.WidthProperty
		};
		StyleInfoProperty[] array3 = new StyleInfoProperty[9]
		{
			ChartFontInfoStore.BoldProperty,
			ChartFontInfoStore.FacenameProperty,
			ChartFontInfoStore.FontFamilyTemplateProperty,
			ChartFontInfoStore.ItalicProperty,
			ChartFontInfoStore.OrientationProperty,
			ChartFontInfoStore.SizeProperty,
			ChartFontInfoStore.StrikeoutProperty,
			ChartFontInfoStore.UnderlineProperty,
			ChartFontInfoStore.UnitProperty
		};
		StyleInfoProperty[] array4 = new StyleInfoProperty[8]
		{
			ChartSymbolInfoStore.ColorProperty,
			ChartSymbolInfoStore.HighlightColorProperty,
			ChartSymbolInfoStore.DimmedColorProperty,
			ChartSymbolInfoStore.ImageIndexProperty,
			ChartSymbolInfoStore.MarkerProperty,
			ChartSymbolInfoStore.OffsetProperty,
			ChartSymbolInfoStore.ShapeProperty,
			ChartSymbolInfoStore.SizeProperty
		};
		StyleInfoProperty[] array5 = new StyleInfoProperty[2]
		{
			ChartBordersInfoStore.InnerProperty,
			ChartBordersInfoStore.OuterProperty
		};
		StyleInfoProperty[] array6 = new StyleInfoProperty[9]
		{
			ChartRelatedPointInfoStore.AlignmentProperty,
			ChartRelatedPointInfoStore.BorderProperty,
			ChartRelatedPointInfoStore.ColorProperty,
			ChartRelatedPointInfoStore.DashPatternProperty,
			ChartRelatedPointInfoStore.DashStyleProperty,
			ChartRelatedPointInfoStore.EndSymbolProperty,
			ChartRelatedPointInfoStore.PointsProperty,
			ChartRelatedPointInfoStore.StartSymbolProperty,
			ChartRelatedPointInfoStore.WidthProperty
		};
		ArrayList arrayList = new ArrayList();
		Hashtable hashtable = new Hashtable(100);
		StyleInfoProperty[][] array7 = new StyleInfoProperty[6][] { array, array2, array3, array4, array5, array6 };
		for (int i = 0; i < array7.Length; i++)
		{
			for (int j = 0; j < array7[i].Length; j++)
			{
				if (!arrayList.Contains(array7[i][j]) && !hashtable.Contains(array7[i][j].PropertyName))
				{
					arrayList.Add(array7[i][j]);
					hashtable.Add(array7[i][j].PropertyName, array7[i][j].PropertyName);
				}
			}
		}
		XmlSchemaElement xmlSchemaElement = new XmlSchemaElement();
		xmlSchema.Items.Add(xmlSchemaElement);
		xmlSchemaElement.Name = typeof(ChartStyleInfo).Name;
		XmlSchemaComplexType xmlSchemaComplexType = (XmlSchemaComplexType)(xmlSchemaElement.SchemaType = new XmlSchemaComplexType());
		XmlSchemaAll xmlSchemaAll = new XmlSchemaAll();
		xmlSchemaAll.MinOccurs = 0m;
		xmlSchemaComplexType.Particle = xmlSchemaAll;
		for (int k = 0; k < array.Length; k++)
		{
			XmlSchemaElement xmlSchemaElement2 = new XmlSchemaElement();
			xmlSchemaElement2.Name = array[k].PropertyName;
			xmlSchemaElement2.MinOccurs = 0m;
			xmlSchemaElement2.SchemaTypeName = new XmlQualifiedName(array[k].PropertyName);
			xmlSchemaAll.Items.Add(xmlSchemaElement2);
		}
		XmlSchemaAll xmlSchemaAll2 = new XmlSchemaAll();
		xmlSchemaAll2.MinOccurs = 0m;
		for (int l = 0; l < array2.Length; l++)
		{
			XmlSchemaElement xmlSchemaElement3 = new XmlSchemaElement();
			xmlSchemaElement3.Name = array2[l].PropertyName;
			xmlSchemaElement3.MinOccurs = 0m;
			xmlSchemaElement3.SchemaTypeName = new XmlQualifiedName(array2[l].PropertyName);
			xmlSchemaAll2.Items.Add(xmlSchemaElement3);
		}
		XmlSchemaAll xmlSchemaAll3 = new XmlSchemaAll();
		xmlSchemaAll3.MinOccurs = 0m;
		for (int m = 0; m < array3.Length; m++)
		{
			XmlSchemaElement xmlSchemaElement4 = new XmlSchemaElement();
			xmlSchemaElement4.Name = array3[m].PropertyName;
			xmlSchemaElement4.MinOccurs = 0m;
			xmlSchemaElement4.SchemaTypeName = new XmlQualifiedName(array3[m].PropertyName);
			xmlSchemaAll3.Items.Add(xmlSchemaElement4);
		}
		XmlSchemaAll xmlSchemaAll4 = new XmlSchemaAll();
		xmlSchemaAll4.MinOccurs = 0m;
		for (int n = 0; n < array4.Length; n++)
		{
			XmlSchemaElement xmlSchemaElement5 = new XmlSchemaElement();
			xmlSchemaElement5.Name = array4[n].PropertyName;
			xmlSchemaElement5.MinOccurs = 0m;
			xmlSchemaElement5.SchemaTypeName = new XmlQualifiedName(array4[n].PropertyName);
			xmlSchemaAll4.Items.Add(xmlSchemaElement5);
		}
		XmlSchemaAll xmlSchemaAll5 = new XmlSchemaAll();
		xmlSchemaAll5.MinOccurs = 0m;
		for (int num = 0; num < array5.Length; num++)
		{
			XmlSchemaElement xmlSchemaElement6 = new XmlSchemaElement();
			xmlSchemaElement6.Name = array5[num].PropertyName;
			xmlSchemaElement6.MinOccurs = 0m;
			xmlSchemaElement6.SchemaTypeName = new XmlQualifiedName(array5[num].PropertyName);
			xmlSchemaAll5.Items.Add(xmlSchemaElement6);
		}
		XmlSchemaAll xmlSchemaAll6 = new XmlSchemaAll();
		xmlSchemaAll6.MinOccurs = 0m;
		for (int num2 = 0; num2 < array6.Length; num2++)
		{
			XmlSchemaElement xmlSchemaElement7 = new XmlSchemaElement();
			xmlSchemaElement7.Name = array6[num2].PropertyName;
			xmlSchemaElement7.MinOccurs = 0m;
			xmlSchemaElement7.SchemaTypeName = new XmlQualifiedName(array6[num2].PropertyName);
			xmlSchemaAll6.Items.Add(xmlSchemaElement7);
		}
		for (int num3 = 0; num3 < arrayList.Count; num3++)
		{
			StyleInfoProperty styleInfoProperty = (StyleInfoProperty)arrayList[num3];
			if (styleInfoProperty.IsExpandable)
			{
				XmlSchemaComplexType xmlSchemaComplexType2 = new XmlSchemaComplexType();
				xmlSchemaComplexType2.Name = styleInfoProperty.PropertyName;
				if (styleInfoProperty.PropertyType == typeof(ChartLineInfo))
				{
					xmlSchemaComplexType2.Particle = xmlSchemaAll2;
				}
				else if (styleInfoProperty.PropertyType == typeof(ChartFontInfo))
				{
					xmlSchemaComplexType2.Particle = xmlSchemaAll3;
				}
				else if (styleInfoProperty.PropertyType == typeof(ChartSymbolInfo))
				{
					xmlSchemaComplexType2.Particle = xmlSchemaAll4;
				}
				else if (styleInfoProperty.PropertyType == typeof(ChartBordersInfo))
				{
					xmlSchemaComplexType2.Particle = xmlSchemaAll5;
				}
				else if (styleInfoProperty.PropertyType == typeof(ChartRelatedPointInfoStore))
				{
					xmlSchemaComplexType2.Particle = xmlSchemaAll6;
				}
				else
				{
					xmlSchemaComplexType2.Particle = xmlSchemaAll;
				}
				xmlSchema.Items.Add(xmlSchemaComplexType2);
			}
			else if (styleInfoProperty.SerializeXmlBehavior != SerializeXmlBehavior.Skip)
			{
				if (styleInfoProperty.SerializeXmlBehavior == SerializeXmlBehavior.SerializeAsString || styleInfoProperty.ObjectStoreKey == -1 || styleInfoProperty.PropertyType == typeof(Color) || styleInfoProperty.PropertyType == typeof(string) || styleInfoProperty.PropertyType.IsPrimitive || styleInfoProperty.PropertyType == typeof(Type))
				{
					XmlSchemaSimpleType xmlSchemaSimpleType = new XmlSchemaSimpleType();
					xmlSchemaSimpleType.Name = styleInfoProperty.PropertyName;
					XmlSchemaSimpleTypeRestriction xmlSchemaSimpleTypeRestriction = new XmlSchemaSimpleTypeRestriction();
					xmlSchemaSimpleTypeRestriction.BaseTypeName = new XmlQualifiedName("string", "http://www.w3.org/2001/XMLSchema");
					xmlSchemaSimpleType.Content = xmlSchemaSimpleTypeRestriction;
					xmlSchema.Items.Add(xmlSchemaSimpleType);
				}
				else
				{
					XmlSchemaComplexType xmlSchemaComplexType3 = new XmlSchemaComplexType();
					xmlSchemaComplexType3.Name = styleInfoProperty.PropertyName;
					XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
					XmlSchemaAny xmlSchemaAny = new XmlSchemaAny();
					xmlSchemaAny.MinOccurs = 0m;
					xmlSchemaAny.ProcessContents = XmlSchemaContentProcessing.Skip;
					xmlSchemaSequence.Items.Add(xmlSchemaAny);
					xmlSchemaComplexType3.Particle = xmlSchemaSequence;
					xmlSchema.Items.Add(xmlSchemaComplexType3);
				}
			}
		}
		return xmlSchema;
	}
}
