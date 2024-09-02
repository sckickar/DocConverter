using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Xml;
using DocGen.Compression;
using DocGen.Drawing;
using DocGen.OfficeChart.Implementation.Collections;
using DocGen.OfficeChart.Implementation.Shapes;
using DocGen.OfficeChart.Implementation.XmlSerialization;

namespace DocGen.OfficeChart.Implementation.XmlReaders.Shapes;

internal abstract class VmlTextBoxBaseParser : ShapeParser
{
	internal const int GradientHorizontal = 0;

	internal const int GradientVertical = -90;

	internal const int GradientDiagonalUp = -135;

	internal const int GradientDiagonalDownCornerCenter = -45;

	private const byte IndexedColor = 1;

	private const byte NamedColor = 2;

	private const byte HexColor = 3;

	internal const int GradientVariant_1 = 100;

	internal const int GradientVariant_3 = 50;

	internal const int GradientVariant_4 = -50;

	internal const string PatternPrefix = "pat_";

	private const string LineStylePrefix = "LINE_";

	private const string ResourcePatternPrefix = "Patt";

	private const string DefaultFillStyle = "solid";

	public static Dictionary<string, OfficeShapeLineStyle> m_excelShapeLineStyle;

	public static Dictionary<string, OfficeShapeDashLineStyle> m_excelDashLineStyle;

	private bool m_isGradientShadingRadial;

	private bool IsGradientShadingRadial
	{
		get
		{
			return m_isGradientShadingRadial;
		}
		set
		{
			m_isGradientShadingRadial = value;
		}
	}

	public static void InitShapeLineStyle()
	{
		if (m_excelShapeLineStyle == null)
		{
			m_excelShapeLineStyle = new Dictionary<string, OfficeShapeLineStyle>();
			lock (m_excelShapeLineStyle)
			{
				m_excelShapeLineStyle.Add("single", OfficeShapeLineStyle.Line_Single);
				m_excelShapeLineStyle.Add("thinThin", OfficeShapeLineStyle.Line_Thin_Thin);
				m_excelShapeLineStyle.Add("thinThick", OfficeShapeLineStyle.Line_Thin_Thick);
				m_excelShapeLineStyle.Add("thickThin", OfficeShapeLineStyle.Line_Thick_Thin);
				m_excelShapeLineStyle.Add("thickBetweenThin", OfficeShapeLineStyle.Line_Thick_Between_Thin);
			}
		}
	}

	public static void InitDashLineStyle()
	{
		m_excelDashLineStyle = new Dictionary<string, OfficeShapeDashLineStyle>();
		m_excelDashLineStyle.Add("solid", OfficeShapeDashLineStyle.Solid);
		m_excelDashLineStyle.Add("1 1", OfficeShapeDashLineStyle.Dotted_Round);
		m_excelDashLineStyle.Add("squareDot", OfficeShapeDashLineStyle.Dotted);
		m_excelDashLineStyle.Add("dash", OfficeShapeDashLineStyle.Dashed);
		m_excelDashLineStyle.Add("dashDot", OfficeShapeDashLineStyle.Dash_Dot);
		m_excelDashLineStyle.Add("longDash", OfficeShapeDashLineStyle.Medium_Dashed);
		m_excelDashLineStyle.Add("longDashDot", OfficeShapeDashLineStyle.Medium_Dash_Dot);
		m_excelDashLineStyle.Add("longDashDotDot", OfficeShapeDashLineStyle.Dash_Dot_Dot);
	}

	public override bool ParseShape(XmlReader reader, ShapeImpl defaultShape, RelationCollection relations, string parentItemPath)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (defaultShape == null)
		{
			throw new ArgumentNullException("defaultShape");
		}
		TextBoxShapeBase textBoxShapeBase = (TextBoxShapeBase)defaultShape.Clone(defaultShape.Parent, null, null, addToCollections: false);
		ParseShapeId(reader, textBoxShapeBase);
		if (reader.MoveToAttribute("style"))
		{
			ParseStyle(reader, textBoxShapeBase);
		}
		if (reader.MoveToAttribute("filled"))
		{
			textBoxShapeBase.HasFill = false;
		}
		else
		{
			textBoxShapeBase.HasFill = true;
		}
		if (reader.MoveToAttribute("stroked"))
		{
			textBoxShapeBase.HasLineFormat = reader.Value != "f";
		}
		if (textBoxShapeBase.HasLineFormat)
		{
			if (reader.MoveToAttribute("strokecolor"))
			{
				ChartColor chartColor = ExtractColor(reader.Value);
				textBoxShapeBase.Line.BackColor = chartColor.GetRGB(textBoxShapeBase.Workbook);
				textBoxShapeBase.Line.DashStyle = OfficeShapeDashLineStyle.Solid;
				textBoxShapeBase.Line.HasPattern = false;
				textBoxShapeBase.Line.Style = OfficeShapeLineStyle.Line_Single;
				textBoxShapeBase.Line.Weight = 0.5;
			}
			if (reader.MoveToAttribute("strokeweight"))
			{
				string value = reader.Value;
				if (value.Contains("pt"))
				{
					textBoxShapeBase.Line.Weight = Convert.ToDouble(value.Split('p')[0]);
				}
				else if (value.Contains("mm"))
				{
					textBoxShapeBase.Line.Weight = Convert.ToDouble(value.Split('m')[0]);
				}
			}
		}
		if (textBoxShapeBase.HasFill && reader.MoveToAttribute("fillcolor"))
		{
			ChartColor chartColor3 = (textBoxShapeBase.ColorObject = ExtractColor(reader.Value));
			textBoxShapeBase.FillColor = chartColor3.GetRGB(textBoxShapeBase.Workbook);
			textBoxShapeBase.Fill.ForeColor = textBoxShapeBase.FillColor;
		}
		if (reader.MoveToAttribute("alt"))
		{
			textBoxShapeBase.AlternativeText = reader.Value.Split('#')[0];
		}
		if (reader.MoveToAttribute("spid", "urn:schemas-microsoft-com:office:office") && reader.MoveToAttribute("id"))
		{
			textBoxShapeBase.Name = reader.Value;
		}
		reader.MoveToElement();
		reader.Read();
		bool flag = true;
		bool flag2 = false;
		string shapeType = null;
		while (reader.NodeType != XmlNodeType.EndElement && flag)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				switch (reader.LocalName)
				{
				case "ClientData":
					flag2 = true;
					flag = ParseClientData(reader, textBoxShapeBase, out shapeType);
					continue;
				case "textbox":
					ParseTextBox(reader, textBoxShapeBase);
					continue;
				case "fill":
					ParseFillStyle(reader, textBoxShapeBase, relations, parentItemPath);
					continue;
				case "stroke":
					if (textBoxShapeBase.HasLineFormat)
					{
						ParseLine(reader, textBoxShapeBase, relations, parentItemPath);
						continue;
					}
					break;
				}
				reader.Skip();
			}
			else
			{
				reader.Skip();
			}
		}
		reader.Read();
		if (flag && flag2 && shapeType != "Shape")
		{
			RegisterShape(textBoxShapeBase);
		}
		return flag;
	}

	public static void ParseShapeId(XmlReader reader, ShapeImpl shape)
	{
		string text = null;
		int num = 0;
		if (reader.MoveToAttribute("spid", "urn:schemas-microsoft-com:office:office"))
		{
			string value = reader.Value;
			num = value.IndexOf("_s");
			if (num >= 0)
			{
				text = value;
			}
		}
		if (text == null && reader.MoveToAttribute("id"))
		{
			string value2 = reader.Value;
			num = value2.IndexOf("_s");
			if (num >= 0)
			{
				text = value2;
			}
		}
		if (text == null || !int.TryParse(text.Substring(num + 2), out var result))
		{
			return;
		}
		ShapesCollection innerShapes = shape.Worksheet.InnerShapes;
		if (innerShapes.StartId == 0)
		{
			innerShapes.StartId = result;
		}
		ShapeImpl shapeImpl = innerShapes.GetShapeById(result) as ShapeImpl;
		shape.ShapeId = result;
		if (shapeImpl != null && shapeImpl.EnableAlternateContent)
		{
			shape.EnableAlternateContent = true;
			shape.XmlDataStream = shapeImpl.XmlDataStream;
			if (shapeImpl.Name != null && shapeImpl.Name.Length > 0)
			{
				shape.Name = shapeImpl.Name;
			}
			shapeImpl.Remove();
		}
	}

	private void ParseTextBox(XmlReader reader, TextBoxShapeBase textBox)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (textBox == null)
		{
			throw new ArgumentNullException("textBox");
		}
		if (reader.LocalName != "textbox")
		{
			throw new XmlException("Unexcpected xml tag.");
		}
		if (reader.MoveToAttribute("style"))
		{
			string value = reader.Value;
			Dictionary<string, string> dictProperties = SplitStyle(value);
			ParseTextDirection(textBox, dictProperties);
		}
		reader.MoveToElement();
		if (!reader.IsEmptyElement)
		{
			reader.Read();
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
					if (reader.LocalName == "div")
					{
						ParseDiv(reader, textBox);
					}
					else
					{
						reader.Skip();
					}
				}
				else
				{
					reader.Skip();
				}
			}
		}
		reader.Skip();
	}

	private void ParseDiv(XmlReader reader, TextBoxShapeBase textBox)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (textBox == null)
		{
			throw new ArgumentNullException("textBox");
		}
		if (!reader.IsEmptyElement)
		{
			reader.Read();
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
					if (reader.LocalName == "font")
					{
						ParseFormattingRun(reader, textBox);
					}
					else
					{
						reader.Skip();
					}
				}
				else
				{
					reader.Skip();
				}
			}
		}
		reader.Read();
	}

	private void ParseFormattingRun(XmlReader reader, TextBoxShapeBase textBox)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (textBox == null)
		{
			throw new ArgumentNullException("textBox");
		}
		Stream stream = ShapeParser.ReadNodeAsStream(reader);
		stream.Position = 0L;
		XmlReader fontReader = UtilityMethods.CreateReader(stream);
		bool flag = CheckFontElement(fontReader);
		stream.Position = 0L;
		fontReader = UtilityMethods.CreateReader(stream);
		IOfficeFont officeFont = textBox.Workbook.CreateFont();
		if (fontReader.MoveToAttribute("face"))
		{
			officeFont.FontName = fontReader.Value;
		}
		if (fontReader.MoveToAttribute("size"))
		{
			officeFont.Size = (double)XmlConvertExtension.ToInt32(fontReader.Value) / 20.0;
		}
		fontReader.MoveToElement();
		string empty = string.Empty;
		if (flag)
		{
			empty = fontReader.ReadElementContentAsString();
			IRichTextString richText = textBox.RichText;
			_ = richText.Text.Length;
			richText.Append(empty, officeFont);
		}
		else
		{
			fontReader.Skip();
		}
	}

	private bool CheckFontElement(XmlReader fontReader)
	{
		if (fontReader == null)
		{
			throw new ArgumentNullException();
		}
		if (fontReader.LocalName != "font")
		{
			throw new XmlException();
		}
		fontReader.Read();
		fontReader.Read();
		if (fontReader.LocalName == "font" && fontReader.NodeType == XmlNodeType.EndElement)
		{
			return true;
		}
		return false;
	}

	protected virtual void RegisterShape(TextBoxShapeBase textBox)
	{
		if (textBox == null)
		{
			throw new ArgumentNullException("comment");
		}
		((WorksheetImpl)textBox.Worksheet).InnerShapes.AddShape(textBox);
	}

	private bool ParseClientData(XmlReader reader, TextBoxShapeBase textBox, out string shapeType)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (textBox == null)
		{
			throw new ArgumentNullException("textBox");
		}
		if (reader.LocalName != "ClientData")
		{
			throw new XmlException("Unexpected xml token");
		}
		shapeType = null;
		if (reader.MoveToAttribute("ObjectType"))
		{
			shapeType = reader.Value;
			if (shapeType == "Pict")
			{
				return false;
			}
		}
		reader.Read();
		textBox.IsMoveWithCell = true;
		textBox.IsSizeWithCell = true;
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				switch (reader.LocalName)
				{
				case "MoveWithCells":
					textBox.IsMoveWithCell = !ParseBoolOrEmpty(reader, defaultValue: true);
					break;
				case "SizeWithCells":
					textBox.IsSizeWithCell = !ParseBoolOrEmpty(reader, defaultValue: true);
					break;
				case "Anchor":
					ParseAnchor(reader, textBox);
					break;
				case "TextHAlign":
					textBox.HAlignment = (OfficeCommentHAlign)Enum.Parse(typeof(OfficeCommentHAlign), reader.ReadElementContentAsString(), ignoreCase: false);
					break;
				case "TextVAlign":
					textBox.VAlignment = (OfficeCommentVAlign)Enum.Parse(typeof(OfficeCommentVAlign), reader.ReadElementContentAsString(), ignoreCase: false);
					break;
				case "LockText":
				{
					string text = reader.ReadElementContentAsString();
					textBox.IsTextLocked = XmlConvertExtension.ToBoolean(text.ToLower());
					break;
				}
				default:
					ParseUnknownClientDataTag(reader, textBox);
					break;
				}
			}
			reader.Read();
		}
		reader.Read();
		return true;
	}

	protected virtual void ParseUnknownClientDataTag(XmlReader reader, TextBoxShapeBase textBox)
	{
		reader.Skip();
	}

	public static bool ParseBoolOrEmpty(XmlReader reader, bool defaultValue)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		bool result = defaultValue;
		if (!reader.IsEmptyElement)
		{
			string text = reader.ReadElementContentAsString();
			if (text.Length != 0)
			{
				result = bool.Parse(text);
			}
		}
		else
		{
			reader.Read();
		}
		return result;
	}

	private void ParseFillStyle(XmlReader reader, TextBoxShapeBase textBox, RelationCollection relations, string parentItemPath)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (textBox == null)
		{
			throw new ArgumentNullException("textBox");
		}
		OfficeFillType officeFillType = (reader.MoveToAttribute("type") ? GetExcelFillType(reader.Value) : GetExcelFillType("solid"));
		if (reader.MoveToAttribute("opacity"))
		{
			textBox.Fill.Transparency = 1.0 - ExtractOpacity(reader.Value);
		}
		if (reader.MoveToAttribute("color"))
		{
			ChartColor chartColor = ExtractColor(reader.Value);
			textBox.FillColor = chartColor.GetRGB(textBox.Workbook);
			textBox.ColorObject = chartColor;
			if (textBox.FillColor == ColorExtension.Empty)
			{
				textBox.HasFill = false;
			}
			textBox.Fill.ForeColor = textBox.FillColor;
		}
		switch (officeFillType)
		{
		case OfficeFillType.SolidColor:
			ParseSolidFill(reader, textBox);
			break;
		case OfficeFillType.Gradient:
			ParseGradientFill(reader, textBox);
			break;
		case OfficeFillType.Texture:
			ParseTextureFill(reader, textBox, relations);
			break;
		case OfficeFillType.Pattern:
			ParsePatternFill(reader, textBox, relations);
			break;
		case OfficeFillType.Picture:
			ParsePictureFill(reader, textBox, relations);
			break;
		case OfficeFillType.UnknownGradient:
		case (OfficeFillType)5:
		case (OfficeFillType)6:
			break;
		}
	}

	private void ParseSolidFill(XmlReader reader, TextBoxShapeBase textBox)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (textBox == null)
		{
			throw new ArgumentNullException("textBox");
		}
		textBox.Fill.FillType = OfficeFillType.SolidColor;
		if (reader.MoveToAttribute("opacity"))
		{
			textBox.Fill.Transparency = 1.0 - ExtractOpacity(reader.Value);
		}
		reader.MoveToElement();
		reader.Skip();
	}

	private void ParseGradientFill(XmlReader reader, TextBoxShapeBase textBox)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (textBox == null)
		{
			throw new ArgumentNullException("textBox");
		}
		textBox.Fill.FillType = OfficeFillType.Gradient;
		string color = null;
		textBox.Fill.GradientColorType = ExtractGradientColorType(reader, out color);
		switch (textBox.Fill.GradientColorType)
		{
		case OfficeGradientColor.OneColor:
			textBox.Fill.BackColor = textBox.FillColor;
			textBox.Fill.GradientDegree = ExtractDegree(color);
			break;
		case OfficeGradientColor.TwoColor:
		{
			ChartColor chartColor = ExtractColor(color);
			textBox.Fill.BackColor = textBox.FillColor;
			textBox.Fill.ForeColor = chartColor.GetRGB(textBox.Workbook);
			break;
		}
		case OfficeGradientColor.Preset:
			textBox.Fill.PresetGradient(ExtractPreset(color));
			break;
		}
		ParseGradientCommon(reader, textBox);
	}

	private void ParseTextureFill(XmlReader reader, TextBoxShapeBase textBox, RelationCollection relations)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (textBox == null)
		{
			throw new ArgumentNullException("textBox");
		}
		if (relations == null)
		{
			throw new ArgumentNullException("relation collection");
		}
		string text = "image";
		if (reader.MoveToAttribute("title", "urn:schemas-microsoft-com:office:office"))
		{
			text = reader.Value;
		}
		textBox.Fill.FillType = OfficeFillType.Texture;
		textBox.Fill.Texture = ExtractTexture(text);
		if (textBox.Fill.Texture == OfficeTexture.User_Defined && reader.MoveToAttribute("relid", "urn:schemas-microsoft-com:office:office"))
		{
			FileDataHolder dataHolder = textBox.ParentWorkbook.DataHolder;
			string value = reader.Value;
			Relation relation = relations[value];
			string itemPath = relations.ItemPath;
			itemPath = itemPath[..itemPath.LastIndexOf('/')];
			itemPath = itemPath[..itemPath.LastIndexOf('/')];
			itemPath = FileDataHolder.CombinePath(itemPath, relation.Target);
			Image image = dataHolder.GetImage(itemPath);
			textBox.Fill.UserTexture(image, text);
		}
		else
		{
			textBox.Fill.PresetTextured(ExtractTexture(text));
		}
	}

	private void ParsePatternFill(XmlReader reader, TextBoxShapeBase textBox, RelationCollection relations)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (textBox == null)
		{
			throw new ArgumentNullException("textBox");
		}
		if (relations == null)
		{
			throw new ArgumentNullException("relation collection");
		}
		string title = "image";
		if (reader.MoveToAttribute("title", "urn:schemas-microsoft-com:office:office"))
		{
			title = reader.Value;
		}
		if (reader.MoveToAttribute("color2"))
		{
			ChartColor chartColor = ExtractColor(reader.Value);
			textBox.Fill.BackColor = textBox.FillColor;
			textBox.Fill.ForeColor = chartColor.GetRGB(textBox.Workbook);
			textBox.Fill.FillType = OfficeFillType.Pattern;
			textBox.Fill.Pattern = ExtractPattern(title);
		}
		if (reader.MoveToAttribute("relid", "urn:schemas-microsoft-com:office:office"))
		{
			FileDataHolder dataHolder = textBox.ParentWorkbook.DataHolder;
			string value = reader.Value;
			Relation relation = relations[value];
			string itemPath = relations.ItemPath;
			itemPath = itemPath[..itemPath.LastIndexOf('/')];
			itemPath = itemPath[..itemPath.LastIndexOf('/')];
			itemPath = FileDataHolder.CombinePath(itemPath, relation.Target);
			dataHolder.GetImage(itemPath);
			textBox.Fill.Patterned(textBox.Fill.Pattern);
		}
	}

	private void ParsePictureFill(XmlReader reader, TextBoxShapeBase textBox, RelationCollection relations)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (textBox == null)
		{
			throw new ArgumentNullException("textBox");
		}
		if (relations == null)
		{
			throw new ArgumentNullException("relation collection");
		}
		string name = "image";
		if (reader.MoveToAttribute("title", "urn:schemas-microsoft-com:office:office"))
		{
			name = reader.Value;
		}
		if (reader.MoveToAttribute("relid", "urn:schemas-microsoft-com:office:office"))
		{
			FileDataHolder dataHolder = textBox.ParentWorkbook.DataHolder;
			string value = reader.Value;
			Relation relation = relations[value];
			string itemPath = relations.ItemPath;
			itemPath = itemPath[..itemPath.LastIndexOf('/')];
			itemPath = itemPath[..itemPath.LastIndexOf('/')];
			itemPath = FileDataHolder.CombinePath(itemPath, relation.Target);
			Image image = dataHolder.GetImage(itemPath);
			textBox.Fill.UserPicture(image, name);
		}
	}

	private void ParseGradientCommon(XmlReader reader, TextBoxShapeBase textBox)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (textBox == null)
		{
			throw new ArgumentNullException("textBox");
		}
		int num = 0;
		if (reader.MoveToAttribute("opacity"))
		{
			textBox.Fill.TransparencyFrom = ExtractOpacity(reader.Value);
		}
		if (reader.MoveToAttribute("o:opacity2"))
		{
			textBox.Fill.TransparencyTo = ExtractOpacity(reader.Value);
		}
		if (reader.MoveToAttribute("focus"))
		{
			textBox.Fill.GradientVariant = ExtractShadingVariant(reader.Value);
		}
		if (reader.MoveToAttribute("angle"))
		{
			num = reader.ReadContentAsInt();
		}
		switch (num)
		{
		case 0:
			textBox.Fill.GradientStyle = OfficeGradientStyle.Horizontal;
			break;
		case -90:
			textBox.Fill.GradientStyle = OfficeGradientStyle.Vertical;
			break;
		case -135:
			textBox.Fill.GradientStyle = OfficeGradientStyle.DiagonalUp;
			break;
		case -45:
			if (IsGradientShadingRadial)
			{
				reader.Read();
				if (reader.NodeType == XmlNodeType.EndElement || reader.NodeType == XmlNodeType.Whitespace)
				{
					textBox.Fill.GradientStyle = OfficeGradientStyle.FromCenter;
				}
				else
				{
					textBox.Fill.GradientStyle = OfficeGradientStyle.FromCorner;
				}
			}
			else
			{
				textBox.Fill.GradientStyle = OfficeGradientStyle.DiagonalDown;
			}
			break;
		}
	}

	private void ParseLine(XmlReader reader, TextBoxShapeBase textBox, RelationCollection relations, string parentItemPath)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (textBox == null)
		{
			throw new ArgumentNullException("textBox");
		}
		if (reader.MoveToAttribute("filltype"))
		{
			textBox.Line.HasPattern = true;
			ParsePatternLine(reader, textBox, relations, parentItemPath);
			return;
		}
		if (reader.MoveToAttribute("dashstyle"))
		{
			textBox.Line.DashStyle = ExtractDashStyle(reader.Value);
		}
		if (reader.MoveToAttribute("linestyle"))
		{
			textBox.Line.Style = ExtractLineStyle(reader.Value);
		}
		reader.MoveToElement();
		reader.Skip();
	}

	private void ParsePatternLine(XmlReader reader, TextBoxShapeBase textBox, RelationCollection relations, string parentItemPath)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (textBox == null)
		{
			throw new ArgumentNullException("textBox");
		}
		if (relations == null)
		{
			throw new ArgumentNullException("relation collection");
		}
		if (parentItemPath == null)
		{
			throw new ArgumentNullException("resource path");
		}
		if (reader.MoveToAttribute("relid", "urn:schemas-microsoft-com:office:office"))
		{
			FileDataHolder dataHolder = textBox.ParentWorkbook.DataHolder;
			string value = reader.Value;
			Relation relation = relations[value];
			string itemPath = relations.ItemPath;
			itemPath = itemPath[..itemPath.LastIndexOf('/')];
			itemPath = itemPath[..itemPath.LastIndexOf('/')];
			itemPath = FileDataHolder.CombinePath(itemPath, relation.Target);
			dataHolder.GetImage(itemPath);
			ExtractLinePattern(dataHolder.GetData(relation, parentItemPath, removeItem: false).Length);
		}
	}

	private void ParseStyle(XmlReader reader, TextBoxShapeBase textBox)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (textBox == null)
		{
			throw new ArgumentNullException("textBox");
		}
		string value = reader.Value;
		textBox.StyleProperties = SplitStyle(value);
		ParseStyle(textBox, textBox.StyleProperties);
	}

	protected virtual void ParseStyle(TextBoxShapeBase textBox, Dictionary<string, string> styleProperties)
	{
		if (styleProperties.TryGetValue("visibility", out var value) && value == "hidden")
		{
			textBox.IsShapeVisible = false;
		}
	}

	private void ParseTextDirection(TextBoxShapeBase textBox, Dictionary<string, string> dictProperties)
	{
		if (textBox == null)
		{
			throw new ArgumentNullException("textBox");
		}
		if (dictProperties == null)
		{
			throw new ArgumentNullException("dictProperties");
		}
		dictProperties.TryGetValue("mso-layout-flow-alt", out var value);
		dictProperties.TryGetValue("layout-flow", out var value2);
		if (value != null || value2 == "vertical")
		{
			OfficeTextRotation textRotation = ((value == "top-to-bottom") ? OfficeTextRotation.TopToBottom : ((!(value == "bottom-to-top")) ? OfficeTextRotation.Clockwise : OfficeTextRotation.CounterClockwise));
			textBox.TextRotation = textRotation;
		}
	}

	private OfficeFillType GetExcelFillType(string officeFillType)
	{
		Dictionary<string, OfficeFillType> obj = new Dictionary<string, OfficeFillType>
		{
			{
				"gradient",
				OfficeFillType.Gradient
			},
			{
				"gradientRadial",
				OfficeFillType.Gradient
			},
			{
				"pattern",
				OfficeFillType.Pattern
			},
			{
				"frame",
				OfficeFillType.Picture
			},
			{
				"tile",
				OfficeFillType.Texture
			},
			{
				"solid",
				OfficeFillType.SolidColor
			}
		};
		if (officeFillType.Equals("gradientRadial"))
		{
			IsGradientShadingRadial = true;
		}
		return obj[officeFillType];
	}

	private byte GetColorType(string color)
	{
		if (color.Contains('['.ToString()))
		{
			return 1;
		}
		if (color.Contains('#'.ToString()))
		{
			return 3;
		}
		return 2;
	}

	private ChartColor ExtractColor(string color)
	{
		switch (GetColorType(color))
		{
		case 1:
			return new ChartColor(ColorType.Indexed, Convert.ToInt32(color.Split('[')[1].Split(']')[0]));
		case 3:
			if (IsHexString(color))
			{
				return new ChartColor(ColorType.RGB, int.Parse(RemoveCharUnSafeAt(color, isLast: false), NumberStyles.HexNumber, null));
			}
			return new ChartColor(ShapeFillImpl.DEF_COMENT_PARSE_COLOR)
			{
				HexColor = color
			};
		case 2:
			return new ChartColor(ColorExtension.FromName(color));
		default:
			return new ChartColor(ColorType.RGB, 1);
		}
	}

	private bool IsHexString(string color)
	{
		color = color.Trim('#');
		char[] array = color.ToCharArray();
		foreach (char c in array)
		{
			if ((c < '0' || c > '9') && (c < 'a' || c > 'f') && (c < 'A' || c > 'F'))
			{
				return false;
			}
		}
		return true;
	}

	private double ExtractOpacity(string opacity)
	{
		if (opacity.EndsWith("f"))
		{
			opacity = RemoveCharUnSafeAt(opacity, isLast: true);
			return Convert.ToDouble(opacity) / 65536.0;
		}
		return Convert.ToDouble(opacity);
	}

	private string RemoveCharUnSafeAt(string source, bool isLast)
	{
		if (!isLast)
		{
			return source.Remove(0, 1);
		}
		return source.Remove(source.Length - 1);
	}

	private OfficeGradientColor ExtractGradientColorType(XmlReader reader, out string color)
	{
		if (reader.MoveToAttribute("colors"))
		{
			color = reader.Value;
			return OfficeGradientColor.Preset;
		}
		if (reader.MoveToAttribute("color2"))
		{
			color = reader.Value;
			if (color.StartsWith("fill"))
			{
				return OfficeGradientColor.OneColor;
			}
			return OfficeGradientColor.TwoColor;
		}
		color = "fill darken(0)";
		return OfficeGradientColor.OneColor;
	}

	private double ExtractDegree(string degree)
	{
		if (degree == null)
		{
			throw new ArgumentNullException("degree");
		}
		double num = XmlConvertExtension.ToDouble(degree.Split('(')[1].Split(')')[0]);
		if (degree.Contains("fill lighten"))
		{
			return num / 255.0;
		}
		return (num - 0.5) / 255.0;
	}

	private OfficeGradientPreset ExtractPreset(string preset)
	{
		OfficeGradientPreset[] array = new OfficeGradientPreset[24]
		{
			OfficeGradientPreset.Grad_Early_Sunset,
			OfficeGradientPreset.Grad_Late_Sunset,
			OfficeGradientPreset.Grad_Nightfall,
			OfficeGradientPreset.Grad_Daybreak,
			OfficeGradientPreset.Grad_Horizon,
			OfficeGradientPreset.Grad_Desert,
			OfficeGradientPreset.Grad_Ocean,
			OfficeGradientPreset.Grad_Calm_Water,
			OfficeGradientPreset.Grad_Fire,
			OfficeGradientPreset.Grad_Fog,
			OfficeGradientPreset.Grad_Moss,
			OfficeGradientPreset.Grad_Peacock,
			OfficeGradientPreset.Grad_Wheat,
			OfficeGradientPreset.Grad_Parchment,
			OfficeGradientPreset.Grad_Mahogany,
			OfficeGradientPreset.Grad_Rainbow,
			OfficeGradientPreset.Grad_RainbowII,
			OfficeGradientPreset.Grad_Gold,
			OfficeGradientPreset.Grad_GoldII,
			OfficeGradientPreset.Grad_Brass,
			OfficeGradientPreset.Grad_Chrome,
			OfficeGradientPreset.Grad_ChromeII,
			OfficeGradientPreset.Grad_Silver,
			OfficeGradientPreset.Grad_Sapphire
		};
		ResourceManager resourceManager = new ResourceManager("DocGen.OfficeChart.VMLPresetGradientFills", typeof(VmlTextBoxBaseParser).GetTypeInfo().Assembly);
		int i = 0;
		for (int num = array.Length; i < num; i++)
		{
			if (resourceManager.GetString(array[i].ToString()).Equals(preset))
			{
				return array[i];
			}
		}
		throw new IndexOutOfRangeException("Presets");
	}

	private OfficePattern ExtractLinePattern(long length)
	{
		throw new NotImplementedException("pattern");
	}

	private OfficeGradientVariants ExtractShadingVariant(string focus)
	{
		return Convert.ToInt32(RemoveCharUnSafeAt(focus, isLast: true)) switch
		{
			100 => OfficeGradientVariants.ShadingVariants_1, 
			50 => OfficeGradientVariants.ShadingVariants_3, 
			-50 => OfficeGradientVariants.ShadingVariants_4, 
			_ => OfficeGradientVariants.ShadingVariants_2, 
		};
	}

	private OfficeTexture ExtractTexture(string title)
	{
		title = title.Replace(' ', '_');
		try
		{
			return (OfficeTexture)Enum.Parse(typeof(OfficeTexture), title, ignoreCase: true);
		}
		catch (Exception)
		{
			return OfficeTexture.User_Defined;
		}
	}

	private OfficeGradientPattern ExtractPattern(string title)
	{
		title = "pat_" + title.Replace(' ', '_');
		try
		{
			return (OfficeGradientPattern)Enum.Parse(typeof(OfficeGradientPattern), title, ignoreCase: true);
		}
		catch (Exception)
		{
			return OfficeGradientPattern.Pat_10_Percent;
		}
	}

	private OfficeShapeDashLineStyle ExtractDashStyle(string dashStyle)
	{
		if (m_excelDashLineStyle == null)
		{
			InitDashLineStyle();
		}
		if (!char.IsDigit(dashStyle[0]))
		{
			return m_excelDashLineStyle[dashStyle];
		}
		return OfficeShapeDashLineStyle.Dotted_Round;
	}

	private OfficeShapeLineStyle ExtractLineStyle(string lineStyle)
	{
		if (m_excelShapeLineStyle == null)
		{
			InitShapeLineStyle();
		}
		return m_excelShapeLineStyle[lineStyle];
	}
}
