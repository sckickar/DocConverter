using System;
using System.IO;

namespace DocGen.DocIO.ReaderWriter.Biff_Records.Structures;

[CLSCompliant(false)]
internal class DOP97
{
	private ushort m_adt;

	private DopTypography m_doptypography;

	private Dogrid m_dogrid;

	private ushort m_flagsA = 13394;

	private Asumyi m_asumyi;

	private uint m_cChWS;

	private uint m_cChWSWithSubdocs;

	private uint m_grfDocEvents;

	private uint m_flagsM;

	private uint m_cpMaxListCacheMainDoc;

	private ushort m_ilfoLastBulletMain;

	private ushort m_ilfoLastNumberMain;

	private uint m_cDBC;

	private uint m_cDBCWithSubdocs;

	private ushort m_nfcFtnRef;

	private ushort m_nfcEdnRef = 2;

	private ushort m_hpsZoomFontPag;

	private ushort m_dywDispPag;

	private DOPDescriptor m_dopBase;

	internal ushort Adt
	{
		get
		{
			return m_adt;
		}
		set
		{
			m_adt = value;
		}
	}

	internal DopTypography DopTypography
	{
		get
		{
			if (m_doptypography == null)
			{
				m_doptypography = new DopTypography();
			}
			return m_doptypography;
		}
	}

	internal Dogrid Dogrid
	{
		get
		{
			if (m_dogrid == null)
			{
				m_dogrid = new Dogrid();
			}
			return m_dogrid;
		}
	}

	internal byte LvlDop
	{
		get
		{
			return (byte)((m_flagsA & 0x1E) >> 1);
		}
		set
		{
			m_flagsA = (ushort)((m_flagsA & 0xFFE1u) | (uint)(value << 1));
		}
	}

	internal bool GramAllDone
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

	internal bool GramAllClean
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

	internal bool SubsetFonts
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

	internal bool HtmlDoc
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

	internal bool DiskLvcInvalid
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

	internal bool SnapBorder
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

	internal bool IncludeHeader
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

	internal bool IncludeFooter
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

	internal Asumyi Asumyi
	{
		get
		{
			if (m_asumyi == null)
			{
				m_asumyi = new Asumyi();
			}
			return m_asumyi;
		}
	}

	internal uint CChWS
	{
		get
		{
			return m_cChWS;
		}
		set
		{
			m_cChWS = value;
		}
	}

	internal uint CChWSWithSubdocs
	{
		get
		{
			return m_cChWSWithSubdocs;
		}
		set
		{
			m_cChWSWithSubdocs = value;
		}
	}

	internal uint GrfDocEvents
	{
		get
		{
			return m_grfDocEvents;
		}
		set
		{
			m_grfDocEvents = value;
		}
	}

	internal bool VirusPrompted
	{
		get
		{
			return (m_flagsM & 1) != 0;
		}
		set
		{
			m_flagsM = (uint)((m_flagsM & 0xFFFFFFFEu) | (value ? 1 : 0));
		}
	}

	internal bool VirusLoadSafe
	{
		get
		{
			return (m_flagsM & 2) >> 1 != 0;
		}
		set
		{
			m_flagsM = (uint)((m_flagsM & 0xFFFFFFFDu) | (int)((value ? 1u : 0u) << 1));
		}
	}

	internal uint KeyVirusSession30
	{
		get
		{
			return (m_flagsM & 0xFFFFFFFCu) >> 2;
		}
		set
		{
			m_flagsM = (m_flagsM & 3u) | (value << 2);
		}
	}

	internal uint CpMaxListCacheMainDoc
	{
		get
		{
			return m_cpMaxListCacheMainDoc;
		}
		set
		{
			m_cpMaxListCacheMainDoc = value;
		}
	}

	internal ushort IlfoLastBulletMain
	{
		get
		{
			return m_ilfoLastBulletMain;
		}
		set
		{
			m_ilfoLastBulletMain = value;
		}
	}

	internal ushort IlfoLastNumberMain
	{
		get
		{
			return m_ilfoLastNumberMain;
		}
		set
		{
			m_ilfoLastNumberMain = value;
		}
	}

	internal uint CDBC
	{
		get
		{
			return m_cDBC;
		}
		set
		{
			m_cDBC = value;
		}
	}

	internal uint CDBCWithSubdocs
	{
		get
		{
			return m_cDBCWithSubdocs;
		}
		set
		{
			m_cDBCWithSubdocs = value;
		}
	}

	internal ushort NfcFtnRef
	{
		get
		{
			return m_nfcFtnRef;
		}
		set
		{
			m_nfcFtnRef = value;
		}
	}

	internal ushort NfcEdnRef
	{
		get
		{
			return m_nfcEdnRef;
		}
		set
		{
			m_nfcEdnRef = value;
		}
	}

	internal ushort HpsZoomFontPag
	{
		get
		{
			return m_hpsZoomFontPag;
		}
		set
		{
			m_hpsZoomFontPag = value;
		}
	}

	internal ushort DywDispPag
	{
		get
		{
			return m_dywDispPag;
		}
		set
		{
			m_dywDispPag = value;
		}
	}

	internal DOP97(DOPDescriptor dopBase)
	{
		m_dopBase = dopBase;
	}

	internal void Parse(Stream stream)
	{
		m_adt = BaseWordRecord.ReadUInt16(stream);
		DopTypography.Parse(stream);
		Dogrid.Parse(stream);
		m_flagsA = BaseWordRecord.ReadUInt16(stream);
		BaseWordRecord.ReadUInt16(stream);
		Asumyi.Parse(stream);
		m_cChWS = BaseWordRecord.ReadUInt32(stream);
		m_cChWSWithSubdocs = BaseWordRecord.ReadUInt32(stream);
		m_grfDocEvents = BaseWordRecord.ReadUInt32(stream);
		m_flagsM = BaseWordRecord.ReadUInt32(stream);
		byte[] array = new byte[30];
		stream.Read(array, 0, array.Length);
		m_cpMaxListCacheMainDoc = BaseWordRecord.ReadUInt32(stream);
		m_ilfoLastBulletMain = BaseWordRecord.ReadUInt16(stream);
		m_ilfoLastNumberMain = BaseWordRecord.ReadUInt16(stream);
		m_cDBC = BaseWordRecord.ReadUInt32(stream);
		m_cDBCWithSubdocs = BaseWordRecord.ReadUInt32(stream);
		BaseWordRecord.ReadUInt32(stream);
		m_nfcFtnRef = BaseWordRecord.ReadUInt16(stream);
		m_nfcEdnRef = BaseWordRecord.ReadUInt16(stream);
		m_hpsZoomFontPag = BaseWordRecord.ReadUInt16(stream);
		m_dywDispPag = BaseWordRecord.ReadUInt16(stream);
	}

	internal void Write(Stream stream)
	{
		BaseWordRecord.WriteUInt16(stream, m_adt);
		DopTypography.Write(stream);
		Dogrid.Write(stream);
		BaseWordRecord.WriteUInt16(stream, m_flagsA);
		BaseWordRecord.WriteUInt16(stream, 0);
		Asumyi.Write(stream);
		BaseWordRecord.WriteUInt32(stream, m_cChWS);
		BaseWordRecord.WriteUInt32(stream, m_cChWSWithSubdocs);
		BaseWordRecord.WriteUInt32(stream, m_grfDocEvents);
		BaseWordRecord.WriteUInt32(stream, m_flagsM);
		byte[] array = new byte[30];
		stream.Write(array, 0, array.Length);
		BaseWordRecord.WriteUInt32(stream, m_cpMaxListCacheMainDoc);
		BaseWordRecord.WriteUInt16(stream, m_ilfoLastBulletMain);
		BaseWordRecord.WriteUInt16(stream, m_ilfoLastNumberMain);
		BaseWordRecord.WriteUInt32(stream, m_cDBC);
		BaseWordRecord.WriteUInt32(stream, m_cDBCWithSubdocs);
		BaseWordRecord.WriteUInt32(stream, 0u);
		BaseWordRecord.WriteUInt16(stream, m_nfcFtnRef);
		BaseWordRecord.WriteUInt16(stream, m_nfcEdnRef);
		BaseWordRecord.WriteUInt16(stream, m_hpsZoomFontPag);
		BaseWordRecord.WriteUInt16(stream, m_dywDispPag);
	}
}
