using System;
using System.IO;
using DocGen.DocIO.DLS;
using DocGen.DocIO.ReaderWriter.Biff_Records;

namespace DocGen.DocIO.ReaderWriter.DataStreamParser.Escher;

[CLSCompliant(false)]
internal class FileShapeAddress : BaseWordRecord
{
	internal const int DEF_FSPA_LENGTH = 26;

	private int m_spid;

	private int m_xaLeft;

	private int m_yaTop;

	private int m_xaRight;

	private int m_yaBottom;

	private int m_relHrzPos;

	private int m_relVrtPos;

	private int m_wrapStyle;

	private int m_wrapType;

	private int m_cTxbx;

	private byte m_bFlags;

	internal int Spid
	{
		get
		{
			return m_spid;
		}
		set
		{
			m_spid = value;
		}
	}

	internal int XaLeft
	{
		get
		{
			return m_xaLeft;
		}
		set
		{
			m_xaLeft = value;
		}
	}

	internal int YaTop
	{
		get
		{
			return m_yaTop;
		}
		set
		{
			m_yaTop = value;
		}
	}

	internal int XaRight
	{
		get
		{
			return m_xaRight;
		}
		set
		{
			m_xaRight = value;
		}
	}

	internal int YaBottom
	{
		get
		{
			return m_yaBottom;
		}
		set
		{
			m_yaBottom = value;
		}
	}

	internal bool IsHeaderShape
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

	internal HorizontalOrigin RelHrzPos
	{
		get
		{
			return (HorizontalOrigin)m_relHrzPos;
		}
		set
		{
			m_relHrzPos = (int)value;
		}
	}

	internal VerticalOrigin RelVrtPos
	{
		get
		{
			return (VerticalOrigin)m_relVrtPos;
		}
		set
		{
			m_relVrtPos = (int)value;
		}
	}

	internal TextWrappingStyle TextWrappingStyle
	{
		get
		{
			return (TextWrappingStyle)m_wrapStyle;
		}
		set
		{
			m_wrapStyle = (int)value;
		}
	}

	internal TextWrappingType TextWrappingType
	{
		get
		{
			return (TextWrappingType)m_wrapType;
		}
		set
		{
			m_wrapType = (int)value;
		}
	}

	internal bool IsRcaSimple
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

	internal bool IsBelowText
	{
		get
		{
			return (m_bFlags & 4) >> 2 != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xFBu) | ((value ? 1u : 0u) << 2));
		}
	}

	internal bool IsAnchorLock
	{
		get
		{
			return (m_bFlags & 8) >> 3 != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xF7u) | ((value ? 1u : 0u) << 3));
		}
	}

	internal int TxbxCount
	{
		get
		{
			return m_cTxbx;
		}
		set
		{
			m_cTxbx = value;
		}
	}

	internal int Height
	{
		get
		{
			return m_yaBottom - m_yaTop;
		}
		set
		{
			m_yaBottom = m_yaTop + value;
		}
	}

	internal int Width
	{
		get
		{
			return m_xaRight - m_xaLeft;
		}
		set
		{
			m_xaRight = m_xaLeft + value;
		}
	}

	internal FileShapeAddress(Stream stream)
	{
		Read(stream);
	}

	internal FileShapeAddress()
	{
	}

	internal void Read(Stream stream)
	{
		m_spid = BaseWordRecord.ReadInt32(stream);
		m_xaLeft = BaseWordRecord.ReadInt32(stream);
		m_yaTop = BaseWordRecord.ReadInt32(stream);
		m_xaRight = BaseWordRecord.ReadInt32(stream);
		m_yaBottom = BaseWordRecord.ReadInt32(stream);
		int num = BaseWordRecord.ReadInt16(stream);
		IsHeaderShape = (num & 1) != 0;
		m_relHrzPos = (num & 6) >> 1;
		m_relVrtPos = (num & 0x18) >> 3;
		m_wrapStyle = (num & 0x1E0) >> 5;
		m_wrapType = (num & 0x1E00) >> 9;
		IsRcaSimple = (num & 0x2000) != 0;
		IsBelowText = (num & 0x4000) != 0;
		IsAnchorLock = (num & 0x8000) != 0;
		if (IsBelowText && TextWrappingStyle == TextWrappingStyle.InFrontOfText)
		{
			TextWrappingStyle = TextWrappingStyle.Behind;
		}
		m_cTxbx = BaseWordRecord.ReadInt32(stream);
	}

	internal void Write(Stream stream)
	{
		BaseWordRecord.WriteInt32(stream, m_spid);
		BaseWordRecord.WriteInt32(stream, m_xaLeft);
		BaseWordRecord.WriteInt32(stream, m_yaTop);
		BaseWordRecord.WriteInt32(stream, m_xaRight);
		BaseWordRecord.WriteInt32(stream, m_yaBottom);
		if (TextWrappingStyle == TextWrappingStyle.Behind && IsBelowText)
		{
			TextWrappingStyle = TextWrappingStyle.InFrontOfText;
		}
		int num = 0;
		num |= (IsHeaderShape ? 1 : 0);
		num |= m_relHrzPos << 1;
		num |= m_relVrtPos << 3;
		num |= m_wrapStyle << 5;
		num |= m_wrapType << 9;
		num |= (IsRcaSimple ? 8192 : 0);
		num |= (IsBelowText ? 16384 : 0);
		num |= (IsAnchorLock ? 32768 : 0);
		BaseWordRecord.WriteInt16(stream, (short)num);
		BaseWordRecord.WriteInt32(stream, m_cTxbx);
	}

	internal FileShapeAddress Clone()
	{
		return new FileShapeAddress
		{
			m_cTxbx = m_cTxbx,
			IsAnchorLock = IsAnchorLock,
			IsBelowText = IsBelowText,
			IsHeaderShape = IsHeaderShape,
			IsRcaSimple = IsRcaSimple,
			m_relHrzPos = m_relHrzPos,
			m_relVrtPos = m_relVrtPos,
			m_spid = m_spid,
			m_wrapStyle = m_wrapStyle,
			m_wrapType = m_wrapType,
			m_xaLeft = m_xaLeft,
			m_xaRight = m_xaRight,
			m_yaBottom = m_yaBottom,
			m_yaTop = m_yaTop
		};
	}
}
