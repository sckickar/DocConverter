using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Xml;
using DocGen.Drawing;
using DocGen.OfficeChart.Implementation.Shapes;
using DocGen.OfficeChart.Implementation.XmlReaders.Shapes;
using DocGen.OfficeChart.Parser.Biff_Records.MsoDrawing;

namespace DocGen.OfficeChart.Implementation.XmlSerialization.Shapes;

internal abstract class ShapeSerializator
{
	public const string FalseAttributeValue = "f";

	public const string TrueAttributeValue = "t";

	public abstract void Serialize(XmlWriter writer, ShapeImpl shape, WorksheetDataHolder holder, RelationCollection relations);

	public abstract void SerializeShapeType(XmlWriter writer, Type shapeType);

	protected static string GetAnchorValue(ShapeImpl shape)
	{
		MsofbtClientAnchor clientAnchor = shape.ClientAnchor;
		int leftColumn = clientAnchor.LeftColumn;
		int num = shape.OffsetInPixels(leftColumn + 1, clientAnchor.LeftOffset, isXOffset: true);
		string text = leftColumn.ToString();
		string text2 = num.ToString();
		leftColumn = clientAnchor.RightColumn;
		num = shape.OffsetInPixels(leftColumn + 1, clientAnchor.RightOffset, isXOffset: true);
		string text3 = leftColumn.ToString();
		string text4 = num.ToString();
		leftColumn = clientAnchor.TopRow;
		num = shape.OffsetInPixels(leftColumn + 1, clientAnchor.TopOffset, isXOffset: false);
		string text5 = leftColumn.ToString();
		string text6 = num.ToString();
		leftColumn = clientAnchor.BottomRow;
		num = shape.OffsetInPixels(leftColumn + 1, clientAnchor.BottomOffset, isXOffset: false);
		string text7 = leftColumn.ToString();
		string text8 = num.ToString();
		return string.Join(", ", text, text2, text5, text6, text3, text4, text7, text8);
	}

	protected void SerializeClientData(XmlWriter writer, ShapeImpl shape, string shapeType)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (shape == null)
		{
			throw new ArgumentNullException("shape");
		}
		writer.WriteStartElement("ClientData", "urn:schemas-microsoft-com:office:excel");
		writer.WriteAttributeString("ObjectType", shapeType);
		if (!shape.IsMoveWithCell)
		{
			writer.WriteElementString("MoveWithCells", "urn:schemas-microsoft-com:office:excel", null);
		}
		if (!shape.IsSizeWithCell)
		{
			writer.WriteStartElement("SizeWithCells", "urn:schemas-microsoft-com:office:excel");
			writer.WriteEndElement();
		}
		string anchorValue = GetAnchorValue(shape);
		writer.WriteElementString("Anchor", "urn:schemas-microsoft-com:office:excel", anchorValue);
		SerializeClientDataAdditional(writer, shape);
		writer.WriteEndElement();
	}

	protected virtual void SerializeClientDataAdditional(XmlWriter writer, ShapeImpl shape)
	{
	}

	[SecurityCritical]
	protected virtual void SerializeFill(XmlWriter writer, ShapeImpl shape, WorksheetDataHolder holder, RelationCollection vmlRelations)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (shape == null)
		{
			throw new ArgumentNullException("textBox");
		}
		if (shape.HasFill)
		{
			TextBoxShapeBase textBoxShapeBase = shape as TextBoxShapeBase;
			writer.WriteStartElement("fill", "urn:schemas-microsoft-com:vml");
			switch (textBoxShapeBase.Fill.FillType)
			{
			case OfficeFillType.SolidColor:
				SerializeSolidFill(writer, textBoxShapeBase);
				break;
			case OfficeFillType.Gradient:
				SerializeGradientFill(writer, textBoxShapeBase);
				break;
			case OfficeFillType.Texture:
			{
				writer.WriteAttributeString("type", "tile");
				FileDataHolder parentHolder = holder.ParentHolder;
				SerializeTextureFill(writer, textBoxShapeBase, parentHolder, vmlRelations);
				SerializeFillCommon(writer, textBoxShapeBase);
				break;
			}
			case OfficeFillType.Pattern:
			{
				FileDataHolder parentHolder = holder.ParentHolder;
				writer.WriteAttributeString("type", "pattern");
				writer.WriteAttributeString("color", GenerateHexColor(textBoxShapeBase.Fill.BackColor));
				writer.WriteAttributeString("color2", GenerateHexColor(textBoxShapeBase.Fill.ForeColor));
				SerializePatternFill(writer, textBoxShapeBase, parentHolder, vmlRelations);
				SerializeFillCommon(writer, textBoxShapeBase);
				break;
			}
			case OfficeFillType.Picture:
			{
				writer.WriteAttributeString("type", "frame");
				FileDataHolder parentHolder = holder.ParentHolder;
				SerializePictureFill(writer, textBoxShapeBase, parentHolder, vmlRelations);
				break;
			}
			}
			writer.WriteEndElement();
		}
	}

	protected virtual void SerializeSolidFill(XmlWriter writer, TextBoxShapeBase textBox)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (textBox == null)
		{
			throw new ArgumentNullException("textBox");
		}
		if (!IsEmptyColor(textBox.Fill.ForeColor))
		{
			string text = null;
			if (textBox.ColorObject == null)
			{
				return;
			}
			text = ((textBox.ColorObject.HexColor == null || !(textBox.Fill.ForeColor == ShapeFillImpl.DEF_COMENT_PARSE_COLOR)) ? GenerateHexColor(textBox.Fill.ForeColor) : textBox.ColorObject.HexColor);
			writer.WriteAttributeString("color", text);
		}
		SerializeFillCommon(writer, textBox);
	}

	protected virtual void SerializeGradientFill(XmlWriter writer, TextBoxShapeBase textBox)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (textBox == null)
		{
			throw new ArgumentNullException("textBox");
		}
		switch (textBox.Fill.GradientColorType)
		{
		case OfficeGradientColor.OneColor:
			writer.WriteAttributeString("color", GenerateHexColor(textBox.Fill.BackColor));
			writer.WriteAttributeString("color2", PrepareGradientDegree(textBox.Fill.GradientDegree));
			break;
		case OfficeGradientColor.TwoColor:
			writer.WriteAttributeString("color", GenerateHexColor(textBox.Fill.BackColor));
			writer.WriteAttributeString("color2", GenerateHexColor(textBox.Fill.ForeColor));
			break;
		case OfficeGradientColor.Preset:
			writer.WriteAttributeString("method", "none");
			writer.WriteAttributeString("colors", GetPresetString(textBox.Fill.PresetGradientType));
			break;
		}
		SerializeGradientFillCommon(writer, textBox);
	}

	[SecurityCritical]
	protected virtual void SerializeTextureFill(XmlWriter writer, TextBoxShapeBase textBox, FileDataHolder holder, RelationCollection vmlRelations)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (textBox == null)
		{
			throw new ArgumentNullException("textBox");
		}
		if (holder == null)
		{
			throw new ArgumentNullException("holder");
		}
		if (vmlRelations == null)
		{
			throw new ArgumentNullException("relations");
		}
		OfficeTexture texture = textBox.Fill.Texture;
		if (texture != OfficeTexture.User_Defined)
		{
			int num = (int)texture;
			byte[] resData = ShapeFillImpl.GetResData("Text" + num);
			byte[] array = new byte[resData.Length - 25];
			Array.Copy(resData, 25, array, 0, array.Length);
			MemoryStream memoryStream = new MemoryStream();
			ShapeFillImpl.UpdateBitMapHederToStream(memoryStream, resData);
			memoryStream.Write(array, 0, array.Length);
			Image image = Image.FromStream(memoryStream, p: true, p3: true);
			string text = texture.ToString();
			text = text.Replace('_', ' ').Trim();
			string text2 = holder.SaveImage(image, null);
			string text3 = vmlRelations.GenerateRelationId();
			vmlRelations[text3] = new Relation("/" + text2, "http://schemas.openxmlformats.org/officeDocument/2006/relationships/image");
			writer.WriteAttributeString("relid", "urn:schemas-microsoft-com:office:office", text3);
			writer.WriteAttributeString("title", "urn:schemas-microsoft-com:office:office", text);
		}
		else
		{
			Image image = textBox.Fill.Picture;
			string pictureName = textBox.Fill.PictureName;
			textBox.Fill.UserTexture(image, pictureName);
			SerializeUserPicture(writer, textBox, holder, vmlRelations);
		}
	}

	protected virtual void SerializePatternFill(XmlWriter writer, TextBoxShapeBase textBox, FileDataHolder holder, RelationCollection vmlRelations)
	{
		throw new NotImplementedException("Pattern");
	}

	[SecurityCritical]
	protected virtual void SerializePictureFill(XmlWriter writer, TextBoxShapeBase textBox, FileDataHolder holder, RelationCollection relations)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (holder == null)
		{
			throw new ArgumentNullException("holder");
		}
		if (relations == null)
		{
			throw new ArgumentNullException("relations");
		}
		if (textBox == null)
		{
			throw new ArgumentNullException("textBox");
		}
		Image picture = textBox.Fill.Picture;
		string pictureName = textBox.Fill.PictureName;
		_ = textBox.Fill.FillType;
		textBox.Fill.FillType = OfficeFillType.SolidColor;
		writer.WriteAttributeString("opacity", GetOpacityFormat(textBox.Fill.Transparency));
		textBox.Fill.UserPicture(picture, pictureName);
		SerializeUserPicture(writer, textBox, holder, relations);
	}

	[SecurityCritical]
	protected virtual void SerializeUserPicture(XmlWriter writer, TextBoxShapeBase textBox, FileDataHolder holder, RelationCollection relations)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (holder == null)
		{
			throw new ArgumentNullException("holder");
		}
		if (relations == null)
		{
			throw new ArgumentNullException("relations");
		}
		if (textBox == null)
		{
			throw new ArgumentNullException("textBox");
		}
		Image picture = textBox.Fill.Picture;
		string pictureName = textBox.Fill.PictureName;
		string text = holder.SaveImage(picture, null);
		string text2 = relations.GenerateRelationId();
		relations[text2] = new Relation("/" + text, "http://schemas.openxmlformats.org/officeDocument/2006/relationships/image");
		writer.WriteAttributeString("relid", "urn:schemas-microsoft-com:office:office", text2);
		writer.WriteAttributeString("title", "urn:schemas-microsoft-com:office:office", pictureName);
	}

	protected virtual void SerializeGradientFillCommon(XmlWriter writer, TextBoxShapeBase textBox)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (textBox == null)
		{
			throw new ArgumentNullException("textBox");
		}
		writer.WriteAttributeString("opacity", GetOpacityFormat(textBox.Fill.TransparencyFrom));
		writer.WriteAttributeString("recolor", "t");
		writer.WriteAttributeString("rotate", "t");
		writer.WriteAttributeString("opacity2", "urn:schemas-microsoft-com:office:office", GetOpacityFormat(textBox.Fill.TransparencyTo));
		double num = 0.0;
		switch (textBox.Fill.GradientStyle)
		{
		case OfficeGradientStyle.Horizontal:
			num = 0.0;
			writer.WriteAttributeString("type", "gradient");
			break;
		case OfficeGradientStyle.Vertical:
			num = -90.0;
			writer.WriteAttributeString("type", "gradient");
			writer.WriteAttributeString("angle", num.ToString());
			break;
		case OfficeGradientStyle.DiagonalUp:
			writer.WriteAttributeString("angle", (-135.0).ToString());
			writer.WriteAttributeString("type", "gradient");
			break;
		case OfficeGradientStyle.DiagonalDown:
			writer.WriteAttributeString("angle", (-45.0).ToString());
			writer.WriteAttributeString("type", "gradient");
			break;
		case OfficeGradientStyle.FromCenter:
			writer.WriteAttributeString("angle", (-45.0).ToString());
			writer.WriteAttributeString("type", "gradientRadial");
			break;
		case OfficeGradientStyle.FromCorner:
			writer.WriteAttributeString("angle", (-45.0).ToString());
			writer.WriteAttributeString("type", "gradientRadial");
			writer.WriteStartElement("fill", "urn:schemas-microsoft-com:office:office");
			writer.WriteAttributeString("ext", "urn:schemas-microsoft-com:vml", "view");
			writer.WriteAttributeString("type", "gradientCenter");
			writer.WriteEndElement();
			break;
		}
		switch (textBox.Fill.GradientVariant)
		{
		case OfficeGradientVariants.ShadingVariants_1:
			writer.WriteAttributeString("focus", 100 + "%");
			break;
		case OfficeGradientVariants.ShadingVariants_3:
			writer.WriteAttributeString("focus", 50 + "%");
			break;
		case OfficeGradientVariants.ShadingVariants_4:
			writer.WriteAttributeString("focus", -50 + "%");
			break;
		case OfficeGradientVariants.ShadingVariants_2:
			break;
		}
	}

	protected virtual void SerializeFillCommon(XmlWriter writer, TextBoxShapeBase textBox)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (textBox == null)
		{
			throw new ArgumentNullException("textBox");
		}
		OfficeFillType fillType = textBox.Fill.FillType;
		textBox.Fill.FillType = OfficeFillType.SolidColor;
		writer.WriteAttributeString("opacity", GetOpacityFormat(textBox.Fill.Transparency));
		textBox.Fill.FillType = fillType;
		writer.WriteAttributeString("recolor", "t");
		writer.WriteAttributeString("rotate", "t");
	}

	[SecurityCritical]
	protected virtual void SerializeLine(XmlWriter writer, TextBoxShapeBase textBox, FileDataHolder holder, RelationCollection relations)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (textBox == null)
		{
			throw new ArgumentNullException("textBox");
		}
		if (textBox.HasLineFormat)
		{
			writer.WriteStartElement("stroke", "urn:schemas-microsoft-com:vml");
			if (textBox.Line.HasPattern)
			{
				SerializePatternLine(writer, textBox, holder, relations);
				writer.WriteAttributeString("color2", "#" + GenerateHexColor(textBox.Line.ForeColor));
			}
			else
			{
				writer.WriteAttributeString("dashstyle", GetDashStyle(textBox.Line.DashStyle));
				writer.WriteAttributeString("linestyle", GetLineStyle(textBox.Line.Style));
				writer.WriteAttributeString("color2", "#" + GenerateHexColor(textBox.Line.ForeColor));
			}
			writer.WriteEndElement();
		}
	}

	[SecurityCritical]
	protected virtual void SerializePatternLine(XmlWriter writer, TextBoxShapeBase textBox, FileDataHolder holder, RelationCollection relations)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (textBox == null)
		{
			throw new ArgumentNullException("textBox");
		}
		if (holder == null)
		{
			throw new ArgumentNullException("holder");
		}
		if (relations == null)
		{
			throw new ArgumentNullException("relations");
		}
		OfficeGradientPattern pattern = textBox.Line.Pattern;
		int num = (int)pattern;
		byte[] resData = ShapeFillImpl.GetResData("Patt" + num);
		byte[] array = new byte[resData.Length - 25];
		Array.Copy(resData, 25, array, 0, array.Length);
		MemoryStream memoryStream = new MemoryStream();
		ShapeFillImpl.UpdateBitMapHederToStream(memoryStream, resData);
		memoryStream.Write(array, 0, array.Length);
		Image image = ApplicationImpl.CreateImage(memoryStream);
		string text = pattern.ToString();
		text = GeneratePatternName(pattern);
		string text2 = holder.SaveImage(image, null);
		string text3 = relations.GenerateRelationId();
		relations[text3] = new Relation("/" + text2, "http://schemas.openxmlformats.org/officeDocument/2006/relationships/image");
		writer.WriteAttributeString("relid", "urn:schemas-microsoft-com:office:office", text3);
		writer.WriteAttributeString("title", "urn:schemas-microsoft-com:office:office", text);
	}

	protected string PrepareGradientDegree(double degree)
	{
		string text = null;
		if (degree > 0.5)
		{
			text = "fill darken(";
			text += (int)(degree * 255.0 + 0.5);
			return text + ")";
		}
		text = "fill lighten(";
		text += (int)(degree * 255.0);
		return text + ")";
	}

	protected string GenerateHexColor(Color color)
	{
		return "#" + RemovePrecedingZeroes((color.ToArgb() & 0xFFFFFF).ToString("X6"));
	}

	protected string GetPresetString(OfficeGradientPreset OfficeGradientPreset)
	{
		return null;
	}

	protected string GeneratePatternName(OfficeGradientPattern pattern)
	{
		return pattern.ToString().Remove(0, "pat_".ToString().Length - 1).Replace('_', ' ')
			.Trim();
	}

	protected string GetOpacityFormat(double opacity)
	{
		opacity = 1.0 - opacity;
		return (int)(opacity * 65536.0) + "f";
	}

	protected string RemovePrecedingZeroes(string color)
	{
		for (int i = 0; i < color.Length; i++)
		{
			if (!color.StartsWith("0"))
			{
				break;
			}
			color = color.Remove(0, 1);
		}
		return color;
	}

	protected string GetDashStyle(OfficeShapeDashLineStyle dashStyle)
	{
		if (VmlTextBoxBaseParser.m_excelDashLineStyle == null)
		{
			VmlTextBoxBaseParser.InitDashLineStyle();
		}
		Dictionary<string, OfficeShapeDashLineStyle> excelDashLineStyle = VmlTextBoxBaseParser.m_excelDashLineStyle;
		IEnumerator<string> enumerator = excelDashLineStyle.Keys.GetEnumerator();
		IEnumerator<OfficeShapeDashLineStyle> enumerator2 = excelDashLineStyle.Values.GetEnumerator();
		while (enumerator.MoveNext())
		{
			enumerator2.MoveNext();
			if (enumerator2.Current == dashStyle)
			{
				break;
			}
		}
		return enumerator.Current;
	}

	protected string GetLineStyle(OfficeShapeLineStyle lineStyle)
	{
		VmlTextBoxBaseParser.InitShapeLineStyle();
		Dictionary<string, OfficeShapeLineStyle> excelShapeLineStyle = VmlTextBoxBaseParser.m_excelShapeLineStyle;
		IEnumerator<string> enumerator = excelShapeLineStyle.Keys.GetEnumerator();
		IEnumerator<OfficeShapeLineStyle> enumerator2 = excelShapeLineStyle.Values.GetEnumerator();
		while (enumerator.MoveNext())
		{
			enumerator2.MoveNext();
			if (enumerator2.Current == lineStyle)
			{
				break;
			}
		}
		return enumerator.Current;
	}

	public static bool IsEmptyColor(Color color)
	{
		return color == ColorExtension.Empty;
	}
}
