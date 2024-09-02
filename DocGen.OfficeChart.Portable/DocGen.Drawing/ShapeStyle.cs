using System.Xml;
using DocGen.OfficeChart;
using DocGen.OfficeChart.Implementation.XmlReaders;
using DocGen.OfficeChart.Implementation.XmlReaders.Shapes;

namespace DocGen.Drawing;

internal class ShapeStyle
{
	private string attribute;

	private ShapeImplExt shape;

	private string nameSpace;

	private StyleEntryModifierEnum m_styleElementMod;

	private StyleOrFontReference m_lnRefStyleEntry;

	private double m_lineWidthScale = -1.0;

	private StyleOrFontReference m_effectRefStyleEntry;

	private StyleOrFontReference m_fillRefStyleEntry;

	private StyleOrFontReference m_fontRefstyleEntry;

	private StyleEntryShapeProperties m_shapeProperties;

	private TextSettings m_defaultParagraphRunProperties;

	private TextBodyPropertiesHolder m_textBodyProperties;

	internal StyleEntryModifierEnum StyleElementMod
	{
		get
		{
			return m_styleElementMod;
		}
		set
		{
			m_styleElementMod = value;
		}
	}

	internal StyleOrFontReference LineRefStyleEntry
	{
		get
		{
			return m_lnRefStyleEntry;
		}
		set
		{
			m_lnRefStyleEntry = value;
		}
	}

	internal double LineWidthScale
	{
		get
		{
			return m_lineWidthScale;
		}
		set
		{
			m_lineWidthScale = value;
		}
	}

	internal StyleOrFontReference EffectRefStyleEntry
	{
		get
		{
			return m_effectRefStyleEntry;
		}
		set
		{
			m_effectRefStyleEntry = value;
		}
	}

	internal StyleOrFontReference FillRefStyleEntry
	{
		get
		{
			return m_fillRefStyleEntry;
		}
		set
		{
			m_fillRefStyleEntry = value;
		}
	}

	internal StyleOrFontReference FontRefstyleEntry
	{
		get
		{
			return m_fontRefstyleEntry;
		}
		set
		{
			m_fontRefstyleEntry = value;
		}
	}

	internal StyleEntryShapeProperties ShapeProperties
	{
		get
		{
			return m_shapeProperties;
		}
		set
		{
			m_shapeProperties = value;
		}
	}

	internal TextSettings DefaultRunParagraphProperties
	{
		get
		{
			return m_defaultParagraphRunProperties;
		}
		set
		{
			m_defaultParagraphRunProperties = value;
		}
	}

	internal TextBodyPropertiesHolder TextBodyProperties
	{
		get
		{
			return m_textBodyProperties;
		}
		set
		{
			m_textBodyProperties = value;
		}
	}

	public ShapeStyle(ShapeImplExt shape, string arrtibute)
	{
		this.shape = shape;
		attribute = arrtibute;
		nameSpace = ((attribute == "xdr") ? "http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing" : "http://schemas.openxmlformats.org/drawingml/2006/chartDrawing");
	}

	internal ShapeStyle(string attributeValue, string nameSpaceValue, StyleEntryModifierEnum enumValue)
	{
		shape = null;
		attribute = attributeValue;
		nameSpace = nameSpaceValue;
		m_styleElementMod = enumValue;
	}

	private void SerializeStyleOrFontReference(XmlWriter writer, StyleOrFontReference styleEntry, string styleEntryName, bool isFontReference)
	{
		string prefix = ((shape != null) ? "a" : attribute);
		string ns = ((shape != null) ? "http://schemas.openxmlformats.org/drawingml/2006/main" : nameSpace);
		writer.WriteStartElement(prefix, styleEntryName, ns);
		if (isFontReference)
		{
			if (styleEntry.Index < 3 && styleEntry.Index > -1)
			{
				writer.WriteAttributeString("idx", ((FontCollectionIndex)styleEntry.Index).ToString());
			}
			else
			{
				writer.WriteAttributeString("idx", FontCollectionIndex.none.ToString());
			}
		}
		else
		{
			writer.WriteAttributeString("idx", styleEntry.Index.ToString());
		}
		SerializeColorSettings(writer, styleEntry.ColorModelType, styleEntry.ColorValue, styleEntry.LumModValue, styleEntry.LumOffValue1, styleEntry.LumOffValue2, styleEntry.ShadeValue);
		writer.WriteEndElement();
	}

	private void SerializeColorSettings(XmlWriter writer, ColorModel colorModelType, string colorValue, double lumModValue, double lumOffValue1, double lumOffValue2, double shadeValue)
	{
		switch (colorModelType)
		{
		case ColorModel.styleClr:
			writer.WriteStartElement("cs", colorModelType.ToString(), null);
			break;
		default:
			writer.WriteStartElement("a", colorModelType.ToString(), null);
			break;
		case ColorModel.none:
			return;
		}
		writer.WriteAttributeString("val", colorValue);
		if (lumModValue != -1.0)
		{
			writer.WriteStartElement("a", "lumMod", "http://schemas.openxmlformats.org/drawingml/2006/main");
			writer.WriteAttributeString("val", lumModValue.ToString());
			writer.WriteEndElement();
		}
		if (lumOffValue1 != -1.0)
		{
			writer.WriteStartElement("a", "lumOff", "http://schemas.openxmlformats.org/drawingml/2006/main");
			writer.WriteAttributeString("val", lumOffValue1.ToString());
			writer.WriteEndElement();
		}
		if (lumOffValue2 != -1.0)
		{
			writer.WriteStartElement("a", "lumOff", "http://schemas.openxmlformats.org/drawingml/2006/main");
			writer.WriteAttributeString("val", lumOffValue2.ToString());
			writer.WriteEndElement();
		}
		if (shadeValue != -1.0)
		{
			writer.WriteStartElement("a", "shade", "http://schemas.openxmlformats.org/drawingml/2006/main");
			writer.WriteAttributeString("val", shadeValue.ToString());
			writer.WriteEndElement();
		}
		writer.WriteEndElement();
	}

	private void SerializeShapeProperties(XmlWriter writer)
	{
		writer.WriteStartElement(attribute, "spPr", nameSpace);
		if ((m_shapeProperties.FlagOptions & 1) == 1)
		{
			if (m_shapeProperties.ShapeFillType == OfficeFillType.SolidColor)
			{
				writer.WriteStartElement("a", "solidFill", "http://schemas.openxmlformats.org/drawingml/2006/main");
				SerializeColorSettings(writer, m_shapeProperties.ShapeFillColorModelType, m_shapeProperties.ShapeFillColorValue, m_shapeProperties.ShapeFillLumModValue, m_shapeProperties.ShapeFillLumOffValue1, m_shapeProperties.ShapeFillLumOffValue2, -1.0);
				writer.WriteEndElement();
			}
			else
			{
				writer.WriteElementString("a", "noFill", "http://schemas.openxmlformats.org/drawingml/2006/main", "");
			}
		}
		if ((m_shapeProperties.FlagOptions & 2) == 2)
		{
			writer.WriteStartElement("a", "ln", "http://schemas.openxmlformats.org/drawingml/2006/main");
			if ((m_shapeProperties.FlagOptions & 4) == 4 && m_shapeProperties.BorderWeight == 0.0)
			{
				writer.WriteElementString("a", "noFill", "http://schemas.openxmlformats.org/drawingml/2006/main", "");
			}
			else
			{
				if ((m_shapeProperties.FlagOptions & 4) == 4 && m_shapeProperties.BorderWeight != -1.0)
				{
					writer.WriteAttributeString("w", m_shapeProperties.BorderWeight.ToString());
				}
				if ((m_shapeProperties.FlagOptions & 4) == 8)
				{
					writer.WriteAttributeString("cap", m_shapeProperties.LineCap.ToString());
				}
				if ((m_shapeProperties.FlagOptions & 4) == 16)
				{
					writer.WriteAttributeString("cmpd", m_shapeProperties.BorderLineStyle.ToString());
				}
				if ((m_shapeProperties.FlagOptions & 4) == 32)
				{
					writer.WriteAttributeString("algn", m_shapeProperties.IsInsetPenAlignment ? "in" : "ctr");
				}
				writer.WriteStartElement("a", "solidFill", "http://schemas.openxmlformats.org/drawingml/2006/main");
				SerializeColorSettings(writer, m_shapeProperties.BorderFillColorModelType, m_shapeProperties.BorderFillColorValue, m_shapeProperties.BorderFillLumModValue, m_shapeProperties.BorderFillLumOffValue1, m_shapeProperties.BorderFillLumOffValue2, -1.0);
				writer.WriteEndElement();
				if (m_shapeProperties.BorderIsRound)
				{
					writer.WriteElementString("a", "round", "http://schemas.openxmlformats.org/drawingml/2006/main", "");
				}
			}
			writer.WriteEndElement();
		}
		writer.WriteEndElement();
	}

	private void SerializeDefaultRPrProperties(XmlWriter writer)
	{
		writer.WriteStartElement(attribute, "defRPr", nameSpace);
		if (m_defaultParagraphRunProperties.FontSize.HasValue)
		{
			writer.WriteAttributeString("sz", (m_defaultParagraphRunProperties.FontSize * 100f).ToString());
		}
		if (m_defaultParagraphRunProperties.Bold.HasValue)
		{
			writer.WriteAttributeString("b", m_defaultParagraphRunProperties.Bold.Value ? "1" : "0");
		}
		if (m_defaultParagraphRunProperties.Baseline != -1)
		{
			writer.WriteAttributeString("baseline", m_defaultParagraphRunProperties.Baseline.ToString());
		}
		if (m_defaultParagraphRunProperties.KerningValue != -1f)
		{
			writer.WriteAttributeString("kern", (m_defaultParagraphRunProperties.KerningValue * 100f).ToString());
		}
		if (m_defaultParagraphRunProperties.SpacingValue != -1f)
		{
			writer.WriteAttributeString("spc", m_defaultParagraphRunProperties.SpacingValue.ToString());
		}
		writer.WriteEndElement();
	}

	internal void Write(XmlWriter xmlTextWriter, string parentElement)
	{
		if (shape != null)
		{
			if (shape.PreservedElements.TryGetValue("Style", out var value))
			{
				if (value != null && value.Length > 0)
				{
					value.Position = 0L;
					ShapeParser.WriteNodeFromStream(xmlTextWriter, value);
				}
			}
			else if (shape.IsCreated)
			{
				xmlTextWriter.WriteStartElement(attribute, parentElement, nameSpace);
				m_lnRefStyleEntry = new StyleOrFontReference(2, ColorModel.schemeClr, "accent1", -1.0, -1.0, -1.0, 50000.0);
				m_effectRefStyleEntry = new StyleOrFontReference(0, ColorModel.schemeClr, "accent1", -1.0, -1.0, -1.0, -1.0);
				m_fillRefStyleEntry = new StyleOrFontReference(1, ColorModel.schemeClr, "accent1", -1.0, -1.0, -1.0, -1.0);
				m_fontRefstyleEntry = new StyleOrFontReference(1, ColorModel.schemeClr, "lt1", -1.0, -1.0, -1.0, -1.0);
				SerializeStyleOrFontReference(xmlTextWriter, m_lnRefStyleEntry, "lnRef", isFontReference: false);
				SerializeStyleOrFontReference(xmlTextWriter, m_fillRefStyleEntry, "fillRef", isFontReference: false);
				SerializeStyleOrFontReference(xmlTextWriter, m_effectRefStyleEntry, "effectRef", isFontReference: false);
				SerializeStyleOrFontReference(xmlTextWriter, m_fontRefstyleEntry, "fontRef", isFontReference: true);
				xmlTextWriter.WriteEndElement();
			}
			return;
		}
		xmlTextWriter.WriteStartElement(attribute, parentElement, nameSpace);
		if (m_styleElementMod != 0)
		{
			string text = (((m_styleElementMod & StyleEntryModifierEnum.allowNoFillOverride) == StyleEntryModifierEnum.allowNoFillOverride) ? StyleEntryModifierEnum.allowNoFillOverride.ToString() : "");
			if ((m_styleElementMod & StyleEntryModifierEnum.allowNoLineOverride) == StyleEntryModifierEnum.allowNoLineOverride)
			{
				text = text + " " + StyleEntryModifierEnum.allowNoLineOverride;
			}
			xmlTextWriter.WriteAttributeString("mods", text);
		}
		SerializeStyleOrFontReference(xmlTextWriter, m_lnRefStyleEntry, "lnRef", isFontReference: false);
		if (m_lineWidthScale != -1.0)
		{
			xmlTextWriter.WriteStartElement(attribute, "lineWidthScale", nameSpace);
			xmlTextWriter.WriteAttributeString("val", m_lineWidthScale.ToString());
			xmlTextWriter.WriteEndElement();
		}
		SerializeStyleOrFontReference(xmlTextWriter, m_fillRefStyleEntry, "fillRef", isFontReference: false);
		SerializeStyleOrFontReference(xmlTextWriter, m_effectRefStyleEntry, "effectRef", isFontReference: false);
		SerializeStyleOrFontReference(xmlTextWriter, m_fontRefstyleEntry, "fontRef", isFontReference: true);
		if (m_shapeProperties != null)
		{
			SerializeShapeProperties(xmlTextWriter);
		}
		if (m_defaultParagraphRunProperties != null)
		{
			SerializeDefaultRPrProperties(xmlTextWriter);
		}
		if (m_textBodyProperties != null)
		{
			m_textBodyProperties.SerialzieTextBodyProperties(xmlTextWriter, attribute, nameSpace);
		}
		xmlTextWriter.WriteEndElement();
	}

	private void EffectRef(XmlWriter xmlTextWriter)
	{
		xmlTextWriter.WriteStartElement("a", "effectRef", "http://schemas.openxmlformats.org/drawingml/2006/main");
		xmlTextWriter.WriteAttributeString("idx", "0");
		xmlTextWriter.WriteStartElement("a", "schemeClr", null);
		xmlTextWriter.WriteAttributeString("val", "accent1");
		xmlTextWriter.WriteEndElement();
		xmlTextWriter.WriteEndElement();
	}

	private void FillRef(XmlWriter xmlTextWriter)
	{
		xmlTextWriter.WriteStartElement("a", "fillRef", "http://schemas.openxmlformats.org/drawingml/2006/main");
		xmlTextWriter.WriteAttributeString("idx", "1");
		xmlTextWriter.WriteStartElement("a", "schemeClr", null);
		xmlTextWriter.WriteAttributeString("val", "accent1");
		xmlTextWriter.WriteEndElement();
		xmlTextWriter.WriteEndElement();
	}

	private void FontRef(XmlWriter xmlTextWriter)
	{
		xmlTextWriter.WriteStartElement("a", "fontRef", "http://schemas.openxmlformats.org/drawingml/2006/main");
		xmlTextWriter.WriteAttributeString("idx", "minor");
		xmlTextWriter.WriteStartElement("a", "schemeClr", null);
		xmlTextWriter.WriteAttributeString("val", "lt1");
		xmlTextWriter.WriteEndElement();
		xmlTextWriter.WriteEndElement();
	}

	private void lnRef(XmlWriter xmlTextWriter)
	{
		xmlTextWriter.WriteStartElement("a", "lnRef", "http://schemas.openxmlformats.org/drawingml/2006/main");
		xmlTextWriter.WriteAttributeString("idx", "2");
		xmlTextWriter.WriteStartElement("a", "schemeClr", null);
		xmlTextWriter.WriteAttributeString("val", "accent1");
		xmlTextWriter.WriteStartElement("a", "shade", "http://schemas.openxmlformats.org/drawingml/2006/main");
		xmlTextWriter.WriteAttributeString("val", "50000");
		xmlTextWriter.WriteEndElement();
		xmlTextWriter.WriteEndElement();
		xmlTextWriter.WriteEndElement();
	}

	internal void Write(XmlWriter xmlTextWriter)
	{
		if (shape.PreservedElements.TryGetValue("Style", out var value))
		{
			if (value != null && value.Length > 0)
			{
				value.Position = 0L;
				ShapeParser.WriteNodeFromStream(xmlTextWriter, value);
			}
		}
		else if (shape.IsCreated)
		{
			xmlTextWriter.WriteStartElement(attribute, "style", "http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing");
			lnRef(xmlTextWriter);
			FillRef(xmlTextWriter);
			EffectRef(xmlTextWriter);
			FontRef(xmlTextWriter);
			xmlTextWriter.WriteEndElement();
		}
	}
}
