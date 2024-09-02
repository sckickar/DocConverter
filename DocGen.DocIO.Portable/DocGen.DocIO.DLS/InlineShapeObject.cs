using System;
using System.IO;
using System.Text;
using DocGen.DocIO.DLS.XML;
using DocGen.DocIO.ReaderWriter.Biff_Records;
using DocGen.DocIO.ReaderWriter.DataStreamParser.Escher;
using DocGen.Drawing;

namespace DocGen.DocIO.DLS;

public class InlineShapeObject : ShapeObject
{
	private PICF m_inlinePictDesc;

	private MsofbtBSE m_curBSE;

	private MsofbtSpContainer m_shapeContainer;

	private int m_oleContainerId = -1;

	private byte[] m_unparsedData;

	private GradientFill m_lineGradient;

	private byte m_bFlags;

	private FillFormat m_FillFormat;

	internal bool IsHorizontalRule
	{
		get
		{
			return (m_bFlags & 2) >> 1 != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xFDu) | ((value ? 1u : 0u) << 1));
		}
	}

	internal GradientFill LineGradient
	{
		get
		{
			if (m_lineGradient == null)
			{
				m_lineGradient = new GradientFill();
			}
			return m_lineGradient;
		}
	}

	internal PICF PictureDescriptor
	{
		get
		{
			return m_inlinePictDesc;
		}
		set
		{
			m_inlinePictDesc = value;
		}
	}

	internal MsofbtSpContainer ShapeContainer
	{
		get
		{
			return m_shapeContainer;
		}
		set
		{
			m_shapeContainer = value;
		}
	}

	internal bool IsOLE
	{
		get
		{
			return (m_bFlags & 1) != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xFEu) | (value ? 1u : 0u));
		}
	}

	internal int OLEContainerId
	{
		get
		{
			return m_oleContainerId;
		}
		set
		{
			m_oleContainerId = value;
		}
	}

	internal byte[] UnparsedData
	{
		get
		{
			return m_unparsedData;
		}
		set
		{
			m_unparsedData = value;
		}
	}

	internal FillFormat FillFormat
	{
		get
		{
			if (m_FillFormat == null)
			{
				m_FillFormat = new FillFormat(this);
			}
			return m_FillFormat;
		}
		set
		{
			m_FillFormat = value;
		}
	}

	internal InlineShapeObject(IWordDocument doc)
		: base(doc)
	{
		m_curBSE = new MsofbtBSE(doc as WordDocument);
		m_shapeContainer = new MsofbtSpContainer(doc as WordDocument);
		m_inlinePictDesc = new PICF();
	}

	protected override object CloneImpl()
	{
		InlineShapeObject inlineShapeObject = (InlineShapeObject)base.CloneImpl();
		inlineShapeObject.m_inlinePictDesc = PictureDescriptor.Clone();
		if (ShapeContainer != null)
		{
			inlineShapeObject.ShapeContainer = (MsofbtSpContainer)ShapeContainer.Clone();
		}
		inlineShapeObject.IsCloned = true;
		return inlineShapeObject;
	}

	internal LineDashing GetDashStyle(BorderStyle borderStyle, ref TextBoxLineStyle lineStyle)
	{
		LineDashing result = LineDashing.Solid;
		lineStyle = TextBoxLineStyle.Simple;
		switch (borderStyle)
		{
		case BorderStyle.Dot:
		case BorderStyle.DashSmallGap:
			result = LineDashing.DotGEL;
			break;
		case BorderStyle.DashLargeGap:
			result = LineDashing.DashGEL;
			break;
		case BorderStyle.DotDash:
			result = LineDashing.DashDotGEL;
			break;
		case BorderStyle.DotDotDash:
			result = LineDashing.LongDashDotDotGEL;
			break;
		case BorderStyle.Double:
		case BorderStyle.DoubleWave:
			lineStyle = TextBoxLineStyle.Double;
			break;
		case BorderStyle.ThinThinSmallGap:
		case BorderStyle.ThickThinMediumGap:
		case BorderStyle.ThickThinLargeGap:
		case BorderStyle.Outset:
			lineStyle = TextBoxLineStyle.ThickThin;
			break;
		case BorderStyle.ThinThickSmallGap:
		case BorderStyle.ThinThickMediumGap:
		case BorderStyle.ThinThickLargeGap:
		case BorderStyle.Inset:
			lineStyle = TextBoxLineStyle.ThinThick;
			break;
		case BorderStyle.Triple:
		case BorderStyle.ThinThickThinSmallGap:
		case BorderStyle.ThickThickThinMediumGap:
		case BorderStyle.ThinThickThinLargeGap:
			lineStyle = TextBoxLineStyle.Triple;
			break;
		}
		return result;
	}

	internal BorderStyle GetBorderStyle(LineDashing dashStyle, TextBoxLineStyle lineStyle)
	{
		BorderStyle result = BorderStyle.None;
		switch (dashStyle)
		{
		case LineDashing.Dash:
		case LineDashing.DashGEL:
		case LineDashing.LongDashGEL:
			result = BorderStyle.DashLargeGap;
			break;
		case LineDashing.DashDotGEL:
			result = BorderStyle.DotDash;
			break;
		case LineDashing.Dot:
		case LineDashing.DotGEL:
			result = BorderStyle.Dot;
			break;
		case LineDashing.DashDot:
		case LineDashing.LongDashDotGEL:
			result = BorderStyle.DotDash;
			break;
		case LineDashing.DashDotDot:
		case LineDashing.LongDashDotDotGEL:
			result = BorderStyle.DotDotDash;
			break;
		default:
			result = BorderStyle.Single;
			break;
		case LineDashing.Solid:
			switch (lineStyle)
			{
			case TextBoxLineStyle.Double:
				result = BorderStyle.Double;
				break;
			case TextBoxLineStyle.ThinThick:
				result = BorderStyle.ThinThickMediumGap;
				break;
			case TextBoxLineStyle.ThickThin:
				result = BorderStyle.ThickThinMediumGap;
				break;
			case TextBoxLineStyle.Triple:
				result = BorderStyle.ThickThickThinMediumGap;
				break;
			default:
				result = BorderStyle.Single;
				break;
			case TextBoxLineStyle.Simple:
				break;
			}
			break;
		}
		return result;
	}

	internal void ConvertToInlineShape()
	{
		uint num = 0u;
		if (ShapeContainer.ShapeOptions.Properties.ContainsKey(459))
		{
			num = ShapeContainer.GetPropertyValue(459);
			ShapeContainer.ShapeOptions.Properties.Remove(459);
		}
		num = (uint)Math.Round((double)num / 12700.0 * 8.0);
		PictureDescriptor.BorderLeft.LineWidth = (byte)num;
		PictureDescriptor.BorderTop.LineWidth = (byte)num;
		PictureDescriptor.BorderRight.LineWidth = (byte)num;
		PictureDescriptor.BorderBottom.LineWidth = (byte)num;
		BorderStyle borderStyle = BorderStyle.None;
		if (ShapeContainer.ShapeOptions.Properties.ContainsKey(461))
		{
			TextBoxLineStyle propertyValue = (TextBoxLineStyle)ShapeContainer.GetPropertyValue(461);
			borderStyle = GetBorderStyle(LineDashing.Solid, propertyValue);
			if (propertyValue == TextBoxLineStyle.Simple)
			{
				borderStyle = BorderStyle.Single;
			}
			ShapeContainer.ShapeOptions.Properties.Remove(461);
		}
		if (ShapeContainer.ShapeOptions.Properties.ContainsKey(462))
		{
			LineDashing propertyValue2 = (LineDashing)ShapeContainer.GetPropertyValue(462);
			borderStyle = GetBorderStyle(propertyValue2, TextBoxLineStyle.Simple);
			if (propertyValue2 == LineDashing.Solid && borderStyle == BorderStyle.None)
			{
				borderStyle = BorderStyle.Single;
			}
			ShapeContainer.ShapeOptions.Properties.Remove(462);
		}
		if (borderStyle != 0)
		{
			PictureDescriptor.BorderLeft.BorderType = (byte)borderStyle;
			PictureDescriptor.BorderTop.BorderType = (byte)borderStyle;
			PictureDescriptor.BorderRight.BorderType = (byte)borderStyle;
			PictureDescriptor.BorderBottom.BorderType = (byte)borderStyle;
		}
		if (ShapeContainer.ShapeOptions.Properties.ContainsKey(448))
		{
			uint propertyValue3 = ShapeContainer.GetPropertyValue(448);
			int num2 = WordColor.ConvertColorToId(WordColor.ConvertRGBToColor(propertyValue3));
			PictureDescriptor.BorderLeft.LineColor = (byte)num2;
			PictureDescriptor.BorderTop.LineColor = (byte)num2;
			PictureDescriptor.BorderRight.LineColor = (byte)num2;
			PictureDescriptor.BorderBottom.LineColor = (byte)num2;
			ShapeContainer.ShapePosition.SetPropertyValue(924, propertyValue3);
			ShapeContainer.ShapePosition.SetPropertyValue(923, propertyValue3);
			ShapeContainer.ShapePosition.SetPropertyValue(926, propertyValue3);
			ShapeContainer.ShapePosition.SetPropertyValue(925, propertyValue3);
			ShapeContainer.ShapeOptions.Properties.Remove(448);
		}
		if (ShapeContainer.ShapeOptions.LineProperties.HasDefined)
		{
			ShapeContainer.ShapeOptions.Properties.Remove(511);
		}
	}

	internal void ConvertToShape()
	{
		uint value = (uint)Math.Round((double)(int)PictureDescriptor.BorderLeft.LineWidth / 8.0 * 12700.0);
		ShapeContainer.ShapeOptions.SetPropertyValue(459, value);
		Color color = PictureDescriptor.BorderLeft.LineColorExt;
		if (ShapeContainer.ShapePosition.Properties.ContainsKey(924))
		{
			color = WordColor.ConvertRGBToColor(ShapeContainer.ShapePosition.GetPropertyValue(924));
			ShapeContainer.ShapePosition.Properties.Remove(924);
		}
		ShapeContainer.ShapeOptions.SetPropertyValue(448, WordColor.ConvertColorToRGB(color));
		TextBoxLineStyle lineStyle = TextBoxLineStyle.Simple;
		LineDashing dashStyle = GetDashStyle((BorderStyle)PictureDescriptor.BorderLeft.BorderType, ref lineStyle);
		ShapeContainer.ShapeOptions.SetPropertyValue(461, (uint)lineStyle);
		ShapeContainer.ShapeOptions.SetPropertyValue(462, (uint)dashStyle);
		if (ShapeContainer.ShapePosition.Properties.ContainsKey(923))
		{
			ShapeContainer.ShapePosition.Properties.Remove(923);
		}
		if (ShapeContainer.ShapePosition.Properties.ContainsKey(926))
		{
			ShapeContainer.ShapePosition.Properties.Remove(926);
		}
		if (ShapeContainer.ShapePosition.Properties.ContainsKey(925))
		{
			ShapeContainer.ShapePosition.Properties.Remove(925);
		}
		PictureDescriptor.brcLeft = new BorderCode();
		PictureDescriptor.brcTop = new BorderCode();
		PictureDescriptor.brcRight = new BorderCode();
		PictureDescriptor.brcBottom = new BorderCode();
	}

	internal void GetEffectExtent(double borderWidth, ref long leftTop, ref long rightBottom)
	{
		int num = (int)(borderWidth / 1.5);
		if (borderWidth % 1.5 >= 1.0)
		{
			num += (int)(borderWidth % 1.5);
		}
		if (num == 0)
		{
			num = 1;
		}
		leftTop = (long)((double)num * 1.5 * 12700.0);
		rightBottom = 0L;
		if (num > 1)
		{
			rightBottom = (long)((double)(num - 1) * 1.5 * 12700.0);
		}
	}

	protected override void WriteXmlContent(IXDLSContentWriter writer)
	{
		base.WriteXmlContent(writer);
		byte[] array = null;
		if (m_inlinePictDesc != null)
		{
			MemoryStream memoryStream = new MemoryStream();
			m_inlinePictDesc.Write(memoryStream);
			array = memoryStream.ToArray();
			memoryStream.Close();
			writer.WriteChildBinaryElement("PictureDescriptor", array);
		}
		byte[] array2 = null;
		if (ShapeContainer != null)
		{
			MemoryStream memoryStream2 = new MemoryStream();
			ShapeContainer.WriteMsofbhWithRecord(memoryStream2);
			array2 = memoryStream2.ToArray();
			memoryStream2.Close();
			writer.WriteChildBinaryElement("ShapeContainer", array2);
			m_curBSE = ShapeContainer.Bse;
			if (m_curBSE != null)
			{
				MemoryStream memoryStream3 = new MemoryStream();
				m_curBSE.Write(memoryStream3);
				byte[] value = memoryStream3.ToArray();
				writer.WriteChildBinaryElement("ShapeBlip", value);
				MemoryStream memoryStream4 = new MemoryStream();
				m_curBSE.WriteMsofbhWithRecord(memoryStream4);
				byte[] value2 = memoryStream4.ToArray();
				writer.WriteChildBinaryElement("ShapeFbse", value2);
				memoryStream3.Close();
				memoryStream4.Close();
			}
		}
	}

	protected override bool ReadXmlContent(IXDLSContentReader reader)
	{
		bool result = base.ReadXmlContent(reader);
		if (reader.TagName == "PictureDescriptor")
		{
			byte[] array = reader.ReadChildBinaryElement();
			if (array.Length != 0)
			{
				MemoryStream memoryStream = new MemoryStream(array, 0, array.Length);
				BinaryReader reader2 = new BinaryReader(memoryStream);
				m_inlinePictDesc.Read(reader2);
				memoryStream.Close();
			}
		}
		if (reader.TagName == "ShapeContainer")
		{
			byte[] array2 = reader.ReadChildBinaryElement();
			if (array2.Length != 0)
			{
				MemoryStream memoryStream2 = new MemoryStream(array2, 0, array2.Length);
				ShapeContainer = new MsofbtSpContainer(base.Document);
				memoryStream2.Position = 0L;
				ShapeContainer.ReadMsofbhWithRecord(memoryStream2);
				memoryStream2.Close();
				result = true;
			}
		}
		if (reader.TagName == "ShapeBlip")
		{
			byte[] array3 = reader.ReadChildBinaryElement();
			MemoryStream stream = new MemoryStream(array3, 0, array3.Length);
			MsofbtBSE msofbtBSE = new MsofbtBSE(base.Document);
			msofbtBSE.Read(stream);
			m_curBSE = msofbtBSE;
		}
		if (reader.TagName == "ShapeFbse")
		{
			byte[] array4 = reader.ReadChildBinaryElement();
			MemoryStream stream2 = new MemoryStream(array4, 0, array4.Length);
			m_curBSE.ReadMsofbhWithRecord(stream2);
			ShapeContainer.Bse = new MsofbtBSE(base.Document);
			ShapeContainer.Bse = m_curBSE;
		}
		return result;
	}

	protected override void ReadXmlAttributes(IXDLSAttributeReader reader)
	{
		if (reader.HasAttribute("IsOLE"))
		{
			IsOLE = reader.ReadBoolean("IsOLE");
		}
		if (reader.HasAttribute("OLEContainerId"))
		{
			m_oleContainerId = reader.ReadInt("OLEContainerId");
		}
	}

	protected override void WriteXmlAttributes(IXDLSAttributeWriter writer)
	{
		writer.WriteValue("type", ParagraphItemType.InlineShapeObject);
		if (IsOLE)
		{
			writer.WriteValue("IsOLE", IsOLE);
			writer.WriteValue("OLEContainerId", OLEContainerId);
		}
	}

	internal override void CloneRelationsTo(WordDocument doc, OwnerHolder nextOwner)
	{
		base.CloneRelationsTo(doc, nextOwner);
		if (ShapeContainer != null)
		{
			ShapeContainer.CloneRelationsTo(doc);
		}
	}

	internal override void Close()
	{
		m_inlinePictDesc = null;
		if (m_curBSE != null)
		{
			m_curBSE.Close();
			m_curBSE = null;
		}
		if (m_shapeContainer != null)
		{
			m_shapeContainer.Close();
			m_shapeContainer = null;
		}
		m_unparsedData = null;
		if (m_lineGradient != null)
		{
			m_lineGradient.Close();
			m_lineGradient = null;
		}
		base.Close();
	}

	internal bool Compare(InlineShapeObject pictureShape)
	{
		if ((PictureDescriptor != null && pictureShape.PictureDescriptor == null) || (PictureDescriptor == null && pictureShape.PictureDescriptor != null) || (FillFormat != null && pictureShape.FillFormat == null) || (FillFormat == null && pictureShape.FillFormat != null) || (LineGradient != null && pictureShape.LineGradient == null) || (LineGradient == null && pictureShape.LineGradient != null) || (ShapeContainer != null && pictureShape.ShapeContainer == null) || (ShapeContainer == null && pictureShape.ShapeContainer != null))
		{
			return false;
		}
		if (PictureDescriptor != null && pictureShape.PictureDescriptor != null && !PictureDescriptor.Compare(pictureShape.PictureDescriptor))
		{
			return false;
		}
		if (FillFormat != null && pictureShape.FillFormat != null && !FillFormat.Compare(pictureShape.FillFormat))
		{
			return false;
		}
		if (LineGradient != null && pictureShape.LineGradient != null && !LineGradient.Compare(pictureShape.LineGradient))
		{
			return false;
		}
		if (ShapeContainer != null && pictureShape.ShapeContainer != null && !ShapeContainer.Compare(pictureShape.ShapeContainer))
		{
			return false;
		}
		return true;
	}

	internal StringBuilder GetAsString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (PictureDescriptor != null)
		{
			stringBuilder.Append(PictureDescriptor.GetAsString());
		}
		if (FillFormat != null)
		{
			stringBuilder.Append(FillFormat.GetAsString());
		}
		if (LineGradient != null)
		{
			stringBuilder.Append(LineGradient.GetAsString());
		}
		if (ShapeContainer != null)
		{
			stringBuilder.Append(ShapeContainer.GetAsString());
		}
		return stringBuilder;
	}
}
