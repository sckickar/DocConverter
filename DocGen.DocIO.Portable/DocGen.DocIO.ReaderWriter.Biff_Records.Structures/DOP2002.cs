using System;
using System.IO;

namespace DocGen.DocIO.ReaderWriter.Biff_Records.Structures;

[CLSCompliant(false)]
internal class DOP2002
{
	private ushort m_flagsA = 61449;

	private ushort m_istdTableDflt = 4095;

	private ushort m_verCompat;

	private ushort m_grfFmtFilter = 20516;

	private ushort m_iFolioPages;

	private int m_cpgText = 1252;

	private uint m_cpMinRMText;

	private uint m_cpMinRMFtn;

	private uint m_cpMinRMHdd;

	private uint m_cpMinRMAtn;

	private uint m_cpMinRMEdn;

	private uint m_cpMinRmTxbx;

	private uint m_cpMinRmHdrTxbx;

	private uint m_rsidRoot;

	private DOPDescriptor m_dopBase;

	internal bool DoNotEmbedSystemFont
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

	internal bool WordCompat
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

	internal bool LiveRecover
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

	internal bool EmbedFactoids
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

	internal bool FactoidXML
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

	internal bool FactoidAllDone
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

	internal bool FolioPrint
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

	internal bool ReverseFolio
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

	internal byte TextLineEnding
	{
		get
		{
			return (byte)((m_flagsA & 0x700) >> 8);
		}
		set
		{
			m_flagsA = (ushort)((m_flagsA & 0xF8FFu) | (uint)(value << 8));
		}
	}

	internal bool HideFcc
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

	internal bool AcetateShowMarkup
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

	internal bool AcetateShowAtn
	{
		get
		{
			return (m_flagsA & 0x2000) >> 13 != 0;
		}
		set
		{
			m_flagsA = (ushort)((m_flagsA & 0xDFFFu) | ((value ? 1u : 0u) << 13));
		}
	}

	internal bool AcetateShowInsDel
	{
		get
		{
			return (m_flagsA & 0x4000) >> 14 != 0;
		}
		set
		{
			m_flagsA = (ushort)((m_flagsA & 0xBFFFu) | ((value ? 1u : 0u) << 14));
		}
	}

	internal bool AcetateShowProps
	{
		get
		{
			return (m_flagsA & 0x8000) >> 15 != 0;
		}
		set
		{
			m_flagsA = (ushort)((m_flagsA & 0x7FFFu) | ((value ? 1u : 0u) << 15));
		}
	}

	internal ushort IstdTableDflt
	{
		get
		{
			return m_istdTableDflt;
		}
		set
		{
			m_istdTableDflt = value;
		}
	}

	internal ushort VerCompat
	{
		get
		{
			return m_verCompat;
		}
		set
		{
			m_verCompat = value;
		}
	}

	internal ushort GrfFmtFilter
	{
		get
		{
			return m_grfFmtFilter;
		}
		set
		{
			m_grfFmtFilter = value;
		}
	}

	internal ushort IFolioPages
	{
		get
		{
			return m_iFolioPages;
		}
		set
		{
			m_iFolioPages = value;
		}
	}

	internal int CpgText
	{
		get
		{
			return m_cpgText;
		}
		set
		{
			m_cpgText = value;
		}
	}

	internal uint CpMinRMText
	{
		get
		{
			return m_cpMinRMText;
		}
		set
		{
			m_cpMinRMText = value;
		}
	}

	internal uint CpMinRMFtn
	{
		get
		{
			return m_cpMinRMFtn;
		}
		set
		{
			m_cpMinRMFtn = value;
		}
	}

	internal uint CpMinRMHdd
	{
		get
		{
			return m_cpMinRMHdd;
		}
		set
		{
			m_cpMinRMHdd = value;
		}
	}

	internal uint CpMinRMAtn
	{
		get
		{
			return m_cpMinRMAtn;
		}
		set
		{
			m_cpMinRMAtn = value;
		}
	}

	internal uint CpMinRMEdn
	{
		get
		{
			return m_cpMinRMEdn;
		}
		set
		{
			m_cpMinRMEdn = value;
		}
	}

	internal uint CpMinRmTxbx
	{
		get
		{
			return m_cpMinRmTxbx;
		}
		set
		{
			m_cpMinRmTxbx = value;
		}
	}

	internal uint CpMinRmHdrTxbx
	{
		get
		{
			return m_cpMinRmHdrTxbx;
		}
		set
		{
			m_cpMinRmHdrTxbx = value;
		}
	}

	internal DOP2002(DOPDescriptor dopBase)
	{
		m_dopBase = dopBase;
	}

	internal void Parse(Stream stream)
	{
		BaseWordRecord.ReadInt32(stream);
		m_flagsA = BaseWordRecord.ReadUInt16(stream);
		m_istdTableDflt = BaseWordRecord.ReadUInt16(stream);
		m_verCompat = BaseWordRecord.ReadUInt16(stream);
		m_grfFmtFilter = BaseWordRecord.ReadUInt16(stream);
		m_iFolioPages = BaseWordRecord.ReadUInt16(stream);
		m_cpgText = BaseWordRecord.ReadInt32(stream);
		m_cpMinRMText = BaseWordRecord.ReadUInt32(stream);
		m_cpMinRMFtn = BaseWordRecord.ReadUInt32(stream);
		m_cpMinRMHdd = BaseWordRecord.ReadUInt32(stream);
		m_cpMinRMAtn = BaseWordRecord.ReadUInt32(stream);
		m_cpMinRMEdn = BaseWordRecord.ReadUInt32(stream);
		m_cpMinRmTxbx = BaseWordRecord.ReadUInt32(stream);
		m_cpMinRmHdrTxbx = BaseWordRecord.ReadUInt32(stream);
		m_rsidRoot = BaseWordRecord.ReadUInt32(stream);
	}

	internal void Write(Stream stream)
	{
		BaseWordRecord.WriteInt32(stream, 0);
		BaseWordRecord.WriteUInt16(stream, m_flagsA);
		BaseWordRecord.WriteUInt16(stream, m_istdTableDflt);
		BaseWordRecord.WriteUInt16(stream, m_verCompat);
		BaseWordRecord.WriteUInt16(stream, m_grfFmtFilter);
		BaseWordRecord.WriteUInt16(stream, m_iFolioPages);
		BaseWordRecord.WriteInt32(stream, m_cpgText);
		BaseWordRecord.WriteUInt32(stream, m_cpMinRMText);
		BaseWordRecord.WriteUInt32(stream, m_cpMinRMFtn);
		BaseWordRecord.WriteUInt32(stream, m_cpMinRMHdd);
		BaseWordRecord.WriteUInt32(stream, m_cpMinRMAtn);
		BaseWordRecord.WriteUInt32(stream, m_cpMinRMEdn);
		BaseWordRecord.WriteUInt32(stream, m_cpMinRmTxbx);
		BaseWordRecord.WriteUInt32(stream, m_cpMinRmHdrTxbx);
		BaseWordRecord.WriteUInt32(stream, m_rsidRoot);
	}
}
