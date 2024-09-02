using System;

namespace DocGen.DocIO.ReaderWriter.Biff_Records.Structures;

[CLSCompliant(false)]
internal class BorderStructure : IDataStructure
{
	private const int DEF_RECORD_SIZE = 4;

	private byte m_dptLineWidth;

	private byte m_brcType;

	private byte m_color;

	private byte m_props;

	public byte LineWidth
	{
		get
		{
			return m_dptLineWidth;
		}
		set
		{
			m_dptLineWidth = value;
		}
	}

	public byte BorderType
	{
		get
		{
			return m_brcType;
		}
		set
		{
			m_brcType = value;
		}
	}

	public byte Space
	{
		get
		{
			return (byte)(m_props & 0x1Fu);
		}
		set
		{
			byte b = value;
			m_props &= 224;
			m_props += b;
		}
	}

	public bool Shadow
	{
		get
		{
			return (byte)((byte)(m_props & 0x20) >> 5) == 1;
		}
		set
		{
			byte b = (value ? ((byte)1) : ((byte)0));
			m_props &= 223;
			b <<= 5;
			m_props += b;
		}
	}

	public byte LineColor
	{
		get
		{
			return m_color;
		}
		set
		{
			m_color = value;
		}
	}

	public bool IsClear => m_dptLineWidth == byte.MaxValue;

	internal byte Props
	{
		get
		{
			return m_props;
		}
		set
		{
			m_props = value;
		}
	}

	public int Length => 4;

	public BorderStructure(byte[] arr, int iOffset)
	{
		Parse(arr, iOffset);
	}

	public BorderStructure()
	{
	}

	internal BorderStructure Clone()
	{
		return new BorderStructure
		{
			m_brcType = m_brcType,
			m_color = m_color,
			m_dptLineWidth = m_dptLineWidth,
			m_props = m_props
		};
	}

	public void Parse(byte[] arr, int iOffset)
	{
		m_dptLineWidth = arr[iOffset];
		m_brcType = arr[iOffset + 1];
		m_color = arr[iOffset + 2];
		m_props = arr[iOffset + 3];
	}

	public int Save(byte[] arr, int iOffset)
	{
		arr[iOffset] = m_dptLineWidth;
		arr[iOffset + 1] = m_brcType;
		arr[iOffset + 2] = m_color;
		arr[iOffset + 3] = m_props;
		return 4;
	}
}
