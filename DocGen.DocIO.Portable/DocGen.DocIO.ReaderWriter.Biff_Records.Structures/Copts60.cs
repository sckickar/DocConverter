using System;
using System.IO;

namespace DocGen.DocIO.ReaderWriter.Biff_Records.Structures;

[CLSCompliant(false)]
internal class Copts60
{
	private ushort m_flags = 61440;

	private DOPDescriptor m_dopBase;

	internal bool NoTabForInd
	{
		get
		{
			return (m_flags & 1) != 0;
		}
		set
		{
			m_flags = (ushort)((m_flags & 0xFFFEu) | (value ? 1u : 0u));
		}
	}

	internal bool NoSpaceRaiseLower
	{
		get
		{
			return (m_flags & 2) >> 1 != 0;
		}
		set
		{
			m_flags = (ushort)((m_flags & 0xFFFDu) | ((value ? 1u : 0u) << 1));
		}
	}

	internal bool SuppressSpBfAfterPgBrk
	{
		get
		{
			return (m_flags & 4) >> 2 != 0;
		}
		set
		{
			m_flags = (ushort)((m_flags & 0xFFFBu) | ((value ? 1u : 0u) << 2));
		}
	}

	internal bool WrapTrailSpaces
	{
		get
		{
			return (m_flags & 8) >> 3 != 0;
		}
		set
		{
			m_flags = (ushort)((m_flags & 0xFFF7u) | ((value ? 1u : 0u) << 3));
		}
	}

	internal bool MapPrintTextColor
	{
		get
		{
			return (m_flags & 0x10) >> 4 != 0;
		}
		set
		{
			m_flags = (ushort)((m_flags & 0xFFEFu) | ((value ? 1u : 0u) << 4));
		}
	}

	internal bool NoColumnBalance
	{
		get
		{
			return (m_flags & 0x20) >> 5 != 0;
		}
		set
		{
			m_flags = (ushort)((m_flags & 0xFFDFu) | ((value ? 1u : 0u) << 5));
		}
	}

	internal bool ConvMailMergeEsc
	{
		get
		{
			return (m_flags & 0x40) >> 6 != 0;
		}
		set
		{
			m_flags = (ushort)((m_flags & 0xFFBFu) | ((value ? 1u : 0u) << 6));
		}
	}

	internal bool SuppressTopSpacing
	{
		get
		{
			return (m_flags & 0x80) >> 7 != 0;
		}
		set
		{
			m_flags = (ushort)((m_flags & 0xFF7Fu) | ((value ? 1u : 0u) << 7));
		}
	}

	internal bool OrigWordTableRules
	{
		get
		{
			return (m_flags & 0x100) >> 8 != 0;
		}
		set
		{
			m_flags = (ushort)((m_flags & 0xFEFFu) | ((value ? 1u : 0u) << 8));
		}
	}

	internal bool ShowBreaksInFrames
	{
		get
		{
			return (m_flags & 0x400) >> 10 != 0;
		}
		set
		{
			m_flags = (ushort)((m_flags & 0xFBFFu) | ((value ? 1u : 0u) << 10));
		}
	}

	internal bool SwapBordersFacingPgs
	{
		get
		{
			return (m_flags & 0x800) >> 11 != 0;
		}
		set
		{
			m_flags = (ushort)((m_flags & 0xF7FFu) | ((value ? 1u : 0u) << 11));
		}
	}

	internal bool LeaveBackslashAlone
	{
		get
		{
			return (m_flags & 0x1000) >> 12 != 0;
		}
		set
		{
			m_flags = (ushort)((m_flags & 0xEFFFu) | ((value ? 1u : 0u) << 12));
		}
	}

	internal bool ExpShRtn
	{
		get
		{
			return (m_flags & 0x2000) >> 13 != 0;
		}
		set
		{
			m_flags = (ushort)((m_flags & 0xDFFFu) | ((value ? 1u : 0u) << 13));
		}
	}

	internal bool DntULTrlSpc
	{
		get
		{
			return (m_flags & 0x4000) >> 14 != 0;
		}
		set
		{
			m_flags = (ushort)((m_flags & 0xBFFFu) | ((value ? 1u : 0u) << 14));
		}
	}

	internal bool DntBlnSbDbWid
	{
		get
		{
			return (m_flags & 0x8000) >> 15 != 0;
		}
		set
		{
			m_flags = (ushort)((m_flags & 0x7FFFu) | ((value ? 1u : 0u) << 15));
		}
	}

	internal Copts60(DOPDescriptor dopBase)
	{
		m_dopBase = dopBase;
	}

	internal void Parse(Stream stream)
	{
		m_flags = BaseWordRecord.ReadUInt16(stream);
	}

	internal void Write(Stream stream)
	{
		BaseWordRecord.WriteUInt16(stream, m_flags);
	}
}
