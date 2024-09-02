using System;
using System.Collections.Generic;
using DocGen.DocIO.DLS.XML;
using DocGen.Drawing;

namespace DocGen.DocIO.DLS;

public class TextWatermark : Watermark
{
	private string m_text = string.Empty;

	private string m_fontName = "Times New Roman";

	private float m_fontSize = 36f;

	private Color m_fontColor = Color.Gray;

	private byte m_bFlags = 1;

	private byte m_bFlags1 = 1;

	private WatermarkLayout m_layout;

	private float m_shapeHeigh = -1f;

	private float m_shapeWidth = -1f;

	private TextWrappingStyle m_wrappingStyle = TextWrappingStyle.Behind;

	private HorizontalOrigin m_horizontalOrgin;

	private float m_horizontalPosition;

	private ShapeHorizontalAlignment m_horizontalAlignement;

	private VerticalOrigin m_verticalOrgin;

	private float m_verticalPosition;

	private ShapeVerticalAlignment m_verticalAlignement;

	private int m_rotation;

	private ShapePosition m_position;

	internal Dictionary<int, object> m_propertiesHash;

	internal const byte FontSizeKey = 1;

	internal bool Visible
	{
		get
		{
			return (m_bFlags1 & 1) != 0;
		}
		set
		{
			m_bFlags1 = (byte)((m_bFlags1 & 0xFEu) | (value ? 1u : 0u));
		}
	}

	public string Text
	{
		get
		{
			return m_text;
		}
		set
		{
			m_text = value;
		}
	}

	public string FontName
	{
		get
		{
			return m_fontName;
		}
		set
		{
			m_fontName = value;
		}
	}

	public float Size
	{
		get
		{
			if (HasKey(1))
			{
				return (float)PropertiesHash[1];
			}
			return m_fontSize;
		}
		set
		{
			m_fontSize = value;
			SetKeyValue(1, value);
		}
	}

	public Color Color
	{
		get
		{
			return m_fontColor;
		}
		set
		{
			m_fontColor = value;
		}
	}

	public bool Semitransparent
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

	public WatermarkLayout Layout
	{
		get
		{
			return m_layout;
		}
		set
		{
			m_layout = value;
			if (base.Document == null || !base.Document.IsOpening)
			{
				if (m_layout == WatermarkLayout.Diagonal)
				{
					m_rotation = 315;
				}
				else
				{
					m_rotation = 0;
				}
			}
		}
	}

	public float Height
	{
		get
		{
			return m_shapeHeigh;
		}
		set
		{
			m_shapeHeigh = value;
		}
	}

	public float Width
	{
		get
		{
			return m_shapeWidth;
		}
		set
		{
			m_shapeWidth = value;
		}
	}

	internal SizeF ShapeSize => GetShapeSizeValue();

	internal TextWrappingStyle TextWrappingStyle
	{
		get
		{
			return m_wrappingStyle;
		}
		set
		{
			m_wrappingStyle = value;
		}
	}

	internal HorizontalOrigin HorizontalOrigin
	{
		get
		{
			return m_horizontalOrgin;
		}
		set
		{
			m_horizontalOrgin = value;
		}
	}

	internal VerticalOrigin VerticalOrigin
	{
		get
		{
			return m_verticalOrgin;
		}
		set
		{
			m_verticalOrgin = value;
		}
	}

	internal ShapeHorizontalAlignment HorizontalAlignment
	{
		get
		{
			return m_horizontalAlignement;
		}
		set
		{
			m_horizontalAlignement = value;
		}
	}

	internal ShapeVerticalAlignment VerticalAlignment
	{
		get
		{
			return m_verticalAlignement;
		}
		set
		{
			m_verticalAlignement = value;
		}
	}

	internal float HorizontalPosition
	{
		get
		{
			return m_horizontalPosition;
		}
		set
		{
			m_horizontalPosition = value;
		}
	}

	internal float VerticalPosition
	{
		get
		{
			return m_verticalPosition;
		}
		set
		{
			m_verticalPosition = value;
		}
	}

	internal int Rotation
	{
		get
		{
			return m_rotation;
		}
		set
		{
			m_rotation = value;
		}
	}

	internal ShapePosition Position
	{
		get
		{
			return m_position;
		}
		set
		{
			m_position = value;
		}
	}

	internal Dictionary<int, object> PropertiesHash
	{
		get
		{
			if (m_propertiesHash == null)
			{
				m_propertiesHash = new Dictionary<int, object>();
			}
			return m_propertiesHash;
		}
	}

	protected object this[int key]
	{
		get
		{
			return key;
		}
		set
		{
			PropertiesHash[key] = value;
		}
	}

	internal TextWatermark()
		: base(WatermarkType.TextWatermark)
	{
		SetDefaultValues();
	}

	internal TextWatermark(string text)
		: base(WatermarkType.TextWatermark)
	{
		m_text = text;
		SetDefaultValues();
	}

	public TextWatermark(string text, string fontName, float width, float height)
		: base(WatermarkType.TextWatermark)
	{
		m_text = text;
		m_fontName = fontName;
		m_fontSize = 1f;
		Width = width;
		Height = height;
		SetDefaultValues();
	}

	internal TextWatermark(string text, string fontName, int fontSize, WatermarkLayout layout)
		: base(WatermarkType.TextWatermark)
	{
		m_text = text;
		m_fontName = fontName;
		m_fontSize = fontSize;
		SetDefaultValues();
		Layout = layout;
	}

	internal TextWatermark(WordDocument doc)
		: base(doc, WatermarkType.TextWatermark)
	{
	}

	protected override void ReadXmlAttributes(IXDLSAttributeReader reader)
	{
		base.ReadXmlAttributes(reader);
		if (reader.HasAttribute("Text"))
		{
			m_text = reader.ReadString("Text");
		}
		if (reader.HasAttribute("TextFontName"))
		{
			m_fontName = reader.ReadString("TextFontName");
		}
		if (reader.HasAttribute("TextFontSize"))
		{
			m_fontSize = reader.ReadFloat("TextFontSize");
		}
		if (reader.HasAttribute("TextLayout"))
		{
			m_layout = (WatermarkLayout)(object)reader.ReadEnum("TextLayout", typeof(WatermarkLayout));
		}
		if (reader.HasAttribute("Semitransparent"))
		{
			Semitransparent = reader.ReadBoolean("Semitransparent");
		}
		if (reader.HasAttribute("TextFontColor"))
		{
			m_fontColor = reader.ReadColor("TextFontColor");
		}
		if (reader.HasAttribute("ShapeHeight"))
		{
			m_shapeHeigh = reader.ReadInt("ShapeHeight");
		}
		if (reader.HasAttribute("ShapeWidth"))
		{
			m_shapeWidth = reader.ReadInt("ShapeWidth");
		}
	}

	protected override void WriteXmlAttributes(IXDLSAttributeWriter writer)
	{
		base.WriteXmlAttributes(writer);
		writer.WriteValue("Text", m_text);
		writer.WriteValue("TextFontName", m_fontName);
		writer.WriteValue("TextFontSize", m_fontSize);
		writer.WriteValue("TextLayout", m_layout);
		if (m_fontColor != Color.Gray)
		{
			writer.WriteValue("TextFontColor", m_fontColor);
		}
		if (!Semitransparent)
		{
			writer.WriteValue("Semitransparent", Semitransparent);
		}
		if (m_shapeHeigh != 0f)
		{
			writer.WriteValue("ShapeHeight", m_shapeHeigh);
		}
		if (m_shapeWidth != 0f)
		{
			writer.WriteValue("ShapeWidth", m_shapeWidth);
		}
	}

	private void SetDefaultValues()
	{
		Rotation = 315;
		Position = ShapePosition.Absolute;
		HorizontalAlignment = ShapeHorizontalAlignment.Center;
		VerticalAlignment = ShapeVerticalAlignment.Center;
	}

	internal void SetDefaultSize()
	{
		SizeF shapeSize = ShapeSize;
		if (Width == -1f)
		{
			Width = (float)Math.Round(shapeSize.Width * 0.6934f, 0);
		}
		if (Height == -1f)
		{
			Height = (float)Math.Round(shapeSize.Height * 0.67f, 0);
		}
	}

	private SizeF GetShapeSizeValue()
	{
		return new SizeF(0f, 0f);
	}

	internal bool HasKey(int Key)
	{
		if (m_propertiesHash != null && m_propertiesHash.ContainsKey(Key))
		{
			return true;
		}
		return false;
	}

	internal void SetKeyValue(int propKey, object value)
	{
		this[propKey] = value;
	}
}
