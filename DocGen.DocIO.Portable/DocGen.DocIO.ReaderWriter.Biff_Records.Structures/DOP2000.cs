using System;
using System.IO;

namespace DocGen.DocIO.ReaderWriter.Biff_Records.Structures;

[CLSCompliant(false)]
internal class DOP2000
{
	private byte m_ilvlLastBulletMain;

	private byte m_ilvlLastNumberMain;

	private ushort m_istdClickParaType;

	private ushort m_flagsA = 12800;

	private ushort m_flagsJ = 387;

	private Copts m_copts;

	private ushort m_verCompatPre10;

	private ushort m_flagsP = 4160;

	private DOPDescriptor m_dopBase;

	internal byte IlvlLastBulletMain
	{
		get
		{
			return m_ilvlLastBulletMain;
		}
		set
		{
			m_ilvlLastBulletMain = value;
		}
	}

	internal byte IlvlLastNumberMain
	{
		get
		{
			return m_ilvlLastNumberMain;
		}
		set
		{
			m_ilvlLastNumberMain = value;
		}
	}

	internal ushort IstdClickParaType
	{
		get
		{
			return m_istdClickParaType;
		}
		set
		{
			m_istdClickParaType = value;
		}
	}

	internal bool LADAllDone
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

	internal bool EnvelopeVis
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

	internal bool MaybeTentativeListInDoc
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

	internal bool MaybeFitText
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

	internal bool FCCAllDone
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

	internal bool RelyOnCSS_WebOpt
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

	internal bool RelyOnVML_WebOpt
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

	internal bool AllowPNG_WebOpt
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

	internal byte ScreenSize_WebOpt
	{
		get
		{
			return (byte)((m_flagsA & 0xF000) >> 12);
		}
		set
		{
			m_flagsA = (ushort)((m_flagsA & 0xFFFu) | (uint)(value << 12));
		}
	}

	internal bool OrganizeInFolder_WebOpt
	{
		get
		{
			return (m_flagsJ & 1) != 0;
		}
		set
		{
			m_flagsJ = (ushort)((m_flagsJ & 0xFFFEu) | (value ? 1u : 0u));
		}
	}

	internal bool UseLongFileNames_WebOpt
	{
		get
		{
			return (m_flagsJ & 2) >> 1 != 0;
		}
		set
		{
			m_flagsJ = (ushort)((m_flagsJ & 0xFFFDu) | ((value ? 1u : 0u) << 1));
		}
	}

	internal ushort PixelsPerInch_WebOpt
	{
		get
		{
			return (ushort)((m_flagsJ & 0xFFC) >> 2);
		}
		set
		{
			m_flagsJ = (ushort)((m_flagsJ & 0xF003u) | (uint)(value << 2));
		}
	}

	internal bool WebOptionsInit
	{
		get
		{
			return (m_flagsJ & 0x1000) >> 12 != 0;
		}
		set
		{
			m_flagsJ = (ushort)((m_flagsJ & 0xEFFFu) | ((value ? 1u : 0u) << 12));
		}
	}

	internal bool MaybeFEL
	{
		get
		{
			return (m_flagsJ & 0x1000) >> 13 != 0;
		}
		set
		{
			m_flagsJ = (ushort)((m_flagsJ & 0xEFFFu) | ((value ? 1u : 0u) << 13));
		}
	}

	internal bool CharLineUnits
	{
		get
		{
			return (m_flagsJ & 0x1000) >> 14 != 0;
		}
		set
		{
			m_flagsJ = (ushort)((m_flagsJ & 0xEFFFu) | ((value ? 1u : 0u) << 14));
		}
	}

	internal Copts Copts
	{
		get
		{
			if (m_copts == null)
			{
				m_copts = new Copts(m_dopBase);
			}
			return m_copts;
		}
	}

	internal ushort VerCompatPre10
	{
		get
		{
			return m_verCompatPre10;
		}
		set
		{
			m_verCompatPre10 = value;
		}
	}

	internal bool NoMargPgvwSaved
	{
		get
		{
			return (m_flagsP & 1) != 0;
		}
		set
		{
			m_flagsP = (ushort)((m_flagsP & 0xFFFEu) | (value ? 1u : 0u));
		}
	}

	internal bool BulletProofed
	{
		get
		{
			return (m_flagsP & 0x10) >> 4 != 0;
		}
		set
		{
			m_flagsP = (ushort)((m_flagsP & 0xFFEFu) | ((value ? 1u : 0u) << 4));
		}
	}

	internal bool SaveUim
	{
		get
		{
			return (m_flagsP & 0x40) >> 6 != 0;
		}
		set
		{
			m_flagsP = (ushort)((m_flagsP & 0xFFBFu) | ((value ? 1u : 0u) << 6));
		}
	}

	internal bool FilterPrivacy
	{
		get
		{
			return (m_flagsP & 0x80) >> 7 != 0;
		}
		set
		{
			m_flagsP = (ushort)((m_flagsP & 0xFF7Fu) | ((value ? 1u : 0u) << 7));
		}
	}

	internal bool SeenRepairs
	{
		get
		{
			return (m_flagsP & 0x200) >> 9 != 0;
		}
		set
		{
			m_flagsP = (ushort)((m_flagsP & 0xFDFFu) | ((value ? 1u : 0u) << 9));
		}
	}

	internal bool HasXML
	{
		get
		{
			return (m_flagsP & 0x400) >> 10 != 0;
		}
		set
		{
			m_flagsP = (ushort)((m_flagsP & 0xFBFFu) | ((value ? 1u : 0u) << 10));
		}
	}

	internal bool ValidateXML
	{
		get
		{
			return (m_flagsP & 0x1000) >> 12 != 0;
		}
		set
		{
			m_flagsP = (ushort)((m_flagsP & 0xEFFFu) | ((value ? 1u : 0u) << 12));
		}
	}

	internal bool SaveInvalidXML
	{
		get
		{
			return (m_flagsP & 0x2000) >> 13 != 0;
		}
		set
		{
			m_flagsP = (ushort)((m_flagsP & 0xDFFFu) | ((value ? 1u : 0u) << 13));
		}
	}

	internal bool ShowXMLErrors
	{
		get
		{
			return (m_flagsP & 0x4000) >> 14 != 0;
		}
		set
		{
			m_flagsP = (ushort)((m_flagsP & 0xBFFFu) | ((value ? 1u : 0u) << 14));
		}
	}

	internal bool AlwaysMergeEmptyNamespace
	{
		get
		{
			return (m_flagsP & 0x8000) >> 15 != 0;
		}
		set
		{
			m_flagsP = (ushort)((m_flagsP & 0x7FFFu) | ((value ? 1u : 0u) << 15));
		}
	}

	internal DOP2000(DOPDescriptor dopBase)
	{
		m_dopBase = dopBase;
	}

	internal void Parse(Stream stream)
	{
		m_ilvlLastBulletMain = (byte)stream.ReadByte();
		m_ilvlLastNumberMain = (byte)stream.ReadByte();
		m_istdClickParaType = BaseWordRecord.ReadUInt16(stream);
		m_flagsA = BaseWordRecord.ReadUInt16(stream);
		m_flagsJ = BaseWordRecord.ReadUInt16(stream);
		Copts.Parse(stream);
		m_verCompatPre10 = BaseWordRecord.ReadUInt16(stream);
		m_flagsP = BaseWordRecord.ReadUInt16(stream);
	}

	internal void Write(Stream stream)
	{
		stream.WriteByte(m_ilvlLastBulletMain);
		stream.WriteByte(m_ilvlLastNumberMain);
		BaseWordRecord.WriteUInt16(stream, m_istdClickParaType);
		BaseWordRecord.WriteUInt16(stream, m_flagsA);
		BaseWordRecord.WriteUInt16(stream, m_flagsJ);
		Copts.Write(stream);
		BaseWordRecord.WriteUInt16(stream, m_verCompatPre10);
		BaseWordRecord.WriteUInt16(stream, m_flagsP);
	}
}
