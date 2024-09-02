using System;
using System.IO;

namespace DocGen.DocIO.ReaderWriter.Biff_Records.Structures;

[CLSCompliant(false)]
internal class DopMth
{
	private ushort m_flagsA = 6160;

	private ushort m_ftcMath;

	private uint m_dxaLeftMargin;

	private uint m_dxaRightMargin;

	private uint m_dxaIndentWrapped = 1440u;

	internal byte Mthbrk
	{
		get
		{
			return (byte)(m_flagsA & 3u);
		}
		set
		{
			m_flagsA = (ushort)((m_flagsA & 0xFFFCu) | value);
		}
	}

	internal byte MthbrkSub
	{
		get
		{
			return (byte)((m_flagsA & 0xC) >> 2);
		}
		set
		{
			m_flagsA = (ushort)((m_flagsA & 0xFFF3u) | (uint)(value << 2));
		}
	}

	internal byte Mthbpjc
	{
		get
		{
			return (byte)((m_flagsA & 0x70) >> 4);
		}
		set
		{
			m_flagsA = (ushort)((m_flagsA & 0xFF8Fu) | (uint)(value << 4));
		}
	}

	internal bool MathSmallFrac
	{
		get
		{
			return (m_flagsA & 0x100) >> 8 != 0;
		}
		set
		{
			m_flagsA = (ushort)((m_flagsA & 0xFEFFu) | ((value ? 1u : 0u) << 8));
		}
	}

	internal bool MathIntLimUndOvr
	{
		get
		{
			return (m_flagsA & 0x200) >> 9 != 0;
		}
		set
		{
			m_flagsA = (ushort)((m_flagsA & 0xFDFFu) | ((value ? 1u : 0u) << 9));
		}
	}

	internal bool MathNaryLimUndOvr
	{
		get
		{
			return (m_flagsA & 0x400) >> 10 != 0;
		}
		set
		{
			m_flagsA = (ushort)((m_flagsA & 0xFBFFu) | ((value ? 1u : 0u) << 10));
		}
	}

	internal bool MathWrapAlignLeft
	{
		get
		{
			return (m_flagsA & 0x800) >> 11 != 0;
		}
		set
		{
			m_flagsA = (ushort)((m_flagsA & 0xF7FFu) | ((value ? 1u : 0u) << 11));
		}
	}

	internal bool MathUseDispDefaults
	{
		get
		{
			return (m_flagsA & 0x1000) >> 12 != 0;
		}
		set
		{
			m_flagsA = (ushort)((m_flagsA & 0xEFFFu) | ((value ? 1u : 0u) << 12));
		}
	}

	internal ushort FtcMath
	{
		get
		{
			return m_ftcMath;
		}
		set
		{
			m_ftcMath = value;
		}
	}

	internal uint DxaLeftMargin
	{
		get
		{
			return m_dxaLeftMargin;
		}
		set
		{
			m_dxaLeftMargin = value;
		}
	}

	internal uint DxaRightMargin
	{
		get
		{
			return m_dxaRightMargin;
		}
		set
		{
			m_dxaRightMargin = value;
		}
	}

	internal uint DxaIndentWrapped
	{
		get
		{
			return m_dxaIndentWrapped;
		}
		set
		{
			m_dxaIndentWrapped = value;
		}
	}

	internal DopMth()
	{
	}

	internal void Parse(Stream stream)
	{
		m_flagsA = BaseWordRecord.ReadUInt16(stream);
		BaseWordRecord.ReadUInt16(stream);
		m_ftcMath = BaseWordRecord.ReadUInt16(stream);
		m_dxaLeftMargin = BaseWordRecord.ReadUInt32(stream);
		m_dxaRightMargin = BaseWordRecord.ReadUInt32(stream);
		BaseWordRecord.ReadUInt32(stream);
		BaseWordRecord.ReadUInt32(stream);
		BaseWordRecord.ReadUInt32(stream);
		BaseWordRecord.ReadUInt32(stream);
		m_dxaIndentWrapped = BaseWordRecord.ReadUInt32(stream);
	}

	internal void Write(Stream stream)
	{
		BaseWordRecord.WriteUInt16(stream, m_flagsA);
		BaseWordRecord.WriteUInt16(stream, 0);
		BaseWordRecord.WriteUInt16(stream, m_ftcMath);
		BaseWordRecord.WriteUInt32(stream, m_dxaLeftMargin);
		BaseWordRecord.WriteUInt32(stream, m_dxaRightMargin);
		BaseWordRecord.WriteUInt32(stream, 120u);
		BaseWordRecord.WriteUInt32(stream, 120u);
		BaseWordRecord.WriteUInt32(stream, 0u);
		BaseWordRecord.WriteUInt32(stream, 0u);
		BaseWordRecord.WriteUInt32(stream, m_dxaIndentWrapped);
	}
}
