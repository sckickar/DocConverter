using System;
using System.IO;

namespace DocGen.DocIO.ReaderWriter.Biff_Records.Structures;

[CLSCompliant(false)]
internal class DOP2003
{
	private ushort m_flagsA;

	private byte m_flagsO = 50;

	private uint m_dxaPageLock;

	private uint m_dyaPageLock;

	private uint m_pctFontLock;

	private byte m_grfitbid;

	private ushort m_ilfoMacAtCleanup;

	private DOPDescriptor m_dopBase;

	internal bool TreatLockAtnAsReadOnly
	{
		get
		{
			return (m_flagsA & 1) != 0;
		}
		set
		{
			m_flagsA = (ushort)((m_flagsA & 0xFFFEu) | (value ? 1u : 0u));
		}
	}

	internal bool StyleLock
	{
		get
		{
			return (m_flagsA & 2) >> 1 != 0;
		}
		set
		{
			m_flagsA = (ushort)((m_flagsA & 0xFFFDu) | ((value ? 1u : 0u) << 1));
		}
	}

	internal bool AutoFmtOverride
	{
		get
		{
			return (m_flagsA & 4) >> 2 != 0;
		}
		set
		{
			m_flagsA = (ushort)((m_flagsA & 0xFFFBu) | ((value ? 1u : 0u) << 2));
		}
	}

	internal bool RemoveWordML
	{
		get
		{
			return (m_flagsA & 8) >> 3 != 0;
		}
		set
		{
			m_flagsA = (ushort)((m_flagsA & 0xFFF7u) | ((value ? 1u : 0u) << 3));
		}
	}

	internal bool ApplyCustomXForm
	{
		get
		{
			return (m_flagsA & 0x10) >> 4 != 0;
		}
		set
		{
			m_flagsA = (ushort)((m_flagsA & 0xFFEFu) | ((value ? 1u : 0u) << 4));
		}
	}

	internal bool StyleLockEnforced
	{
		get
		{
			return (m_flagsA & 0x20) >> 5 != 0;
		}
		set
		{
			m_flagsA = (ushort)((m_flagsA & 0xFFDFu) | ((value ? 1u : 0u) << 5));
		}
	}

	internal bool FakeLockAtn
	{
		get
		{
			return (m_flagsA & 0x40) >> 6 != 0;
		}
		set
		{
			m_flagsA = (ushort)((m_flagsA & 0xFFBFu) | ((value ? 1u : 0u) << 6));
		}
	}

	internal bool IgnoreMixedContent
	{
		get
		{
			return (m_flagsA & 0x80) >> 7 != 0;
		}
		set
		{
			m_flagsA = (ushort)((m_flagsA & 0xFF7Fu) | ((value ? 1u : 0u) << 7));
		}
	}

	internal bool ShowPlaceholderText
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

	internal bool Word97Doc
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

	internal bool StyleLockTheme
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

	internal bool StyleLockQFSet
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

	internal bool ReadingModeInkLockDown
	{
		get
		{
			return (m_flagsO & 1) != 0;
		}
		set
		{
			m_flagsO = (byte)((m_flagsO & 0xFEu) | (value ? 1u : 0u));
		}
	}

	internal bool AcetateShowInkAtn
	{
		get
		{
			return (m_flagsO & 2) >> 1 != 0;
		}
		set
		{
			m_flagsO = (byte)((m_flagsO & 0xFDu) | ((value ? 1u : 0u) << 1));
		}
	}

	internal bool FilterDttm
	{
		get
		{
			return (m_flagsO & 4) >> 2 != 0;
		}
		set
		{
			m_flagsO = (byte)((m_flagsO & 0xFBu) | ((value ? 1u : 0u) << 2));
		}
	}

	internal bool EnforceDocProt
	{
		get
		{
			return (m_flagsO & 8) >> 3 != 0;
		}
		set
		{
			m_flagsO = (byte)((m_flagsO & 0xF7u) | ((value ? 1u : 0u) << 3));
		}
	}

	internal byte DocProtCur
	{
		get
		{
			return (byte)((m_flagsO & 0x70) >> 4);
		}
		set
		{
			m_flagsO = (byte)((m_flagsO & 0x8Fu) | (uint)(value << 4));
		}
	}

	internal bool DispBkSpSaved
	{
		get
		{
			return (m_flagsO & 0x80) >> 7 != 0;
		}
		set
		{
			m_flagsO = (byte)((m_flagsO & 0x7Fu) | ((value ? 1u : 0u) << 7));
		}
	}

	internal uint DxaPageLock
	{
		get
		{
			return m_dxaPageLock;
		}
		set
		{
			m_dxaPageLock = value;
		}
	}

	internal uint DyaPageLock
	{
		get
		{
			return m_dyaPageLock;
		}
		set
		{
			m_dyaPageLock = value;
		}
	}

	internal uint PctFontLock
	{
		get
		{
			return m_pctFontLock;
		}
		set
		{
			m_pctFontLock = value;
		}
	}

	internal byte Grfitbid
	{
		get
		{
			return m_grfitbid;
		}
		set
		{
			m_grfitbid = value;
		}
	}

	internal ushort IlfoMacAtCleanup
	{
		get
		{
			return m_ilfoMacAtCleanup;
		}
		set
		{
			m_ilfoMacAtCleanup = value;
		}
	}

	internal DOP2003(DOPDescriptor dopBase)
	{
		m_dopBase = dopBase;
	}

	internal void Parse(Stream stream)
	{
		m_flagsA = BaseWordRecord.ReadUInt16(stream);
		BaseWordRecord.ReadUInt16(stream);
		m_flagsO = (byte)stream.ReadByte();
		stream.ReadByte();
		m_dxaPageLock = BaseWordRecord.ReadUInt32(stream);
		m_dyaPageLock = BaseWordRecord.ReadUInt32(stream);
		m_pctFontLock = BaseWordRecord.ReadUInt32(stream);
		m_grfitbid = (byte)stream.ReadByte();
		stream.ReadByte();
		m_ilfoMacAtCleanup = BaseWordRecord.ReadUInt16(stream);
	}

	internal void Write(Stream stream)
	{
		BaseWordRecord.WriteUInt16(stream, m_flagsA);
		BaseWordRecord.WriteUInt16(stream, 0);
		stream.WriteByte(m_flagsO);
		stream.WriteByte(0);
		BaseWordRecord.WriteUInt32(stream, m_dxaPageLock);
		BaseWordRecord.WriteUInt32(stream, m_dyaPageLock);
		BaseWordRecord.WriteUInt32(stream, m_pctFontLock);
		stream.WriteByte(m_grfitbid);
		stream.WriteByte(0);
		BaseWordRecord.WriteUInt16(stream, m_ilfoMacAtCleanup);
	}
}
