using DocGen.Drawing;

namespace DocGen.Pdf;

internal class Head : TableBase
{
	private ushort macStyle;

	private ushort m_flags;

	private short m_glyphDataFormat;

	private ushort m_unitsPerEm;

	private int m_id;

	private RectangleF m_bbox;

	private short m_indexFormat;

	internal override int Id => m_id;

	public ushort Flags
	{
		get
		{
			return m_flags;
		}
		private set
		{
			m_flags = value;
		}
	}

	public short GlyphDataFormat
	{
		get
		{
			return m_glyphDataFormat;
		}
		private set
		{
			m_glyphDataFormat = value;
		}
	}

	public ushort UnitsPerEm
	{
		get
		{
			return m_unitsPerEm;
		}
		private set
		{
			m_unitsPerEm = value;
		}
	}

	public RectangleF BBox
	{
		get
		{
			return m_bbox;
		}
		private set
		{
			m_bbox = value;
		}
	}

	public short IndexToLocFormat
	{
		get
		{
			return m_indexFormat;
		}
		private set
		{
			m_indexFormat = value;
		}
	}

	public bool IsBold => CheckMacStyle(0);

	public bool IsItalic => CheckMacStyle(1);

	public Head(FontFile2 fontFile)
		: base(fontFile)
	{
	}

	private bool CheckMacStyle(byte bit)
	{
		return (macStyle & (1 << (int)bit)) != 0;
	}

	public override void Read(ReadFontArray reader)
	{
		reader.getFixed();
		reader.getFixed();
		reader.getnextULong();
		reader.getnextULong();
		m_flags = reader.getnextUshort();
		m_unitsPerEm = reader.getnextUshort();
		reader.getLongDateTime();
		reader.getLongDateTime();
		m_bbox = new RectangleF(reader.getnextshort(), reader.getnextshort(), reader.getnextshort(), reader.getnextshort());
		macStyle = reader.getnextUshort();
		reader.getnextUshort();
		reader.getnextshort();
		m_indexFormat = reader.getnextshort();
		reader.getnextshort();
	}
}
