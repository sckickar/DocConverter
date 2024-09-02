using System;
using System.IO;
using DocGen.DocIO.DLS;
using DocGen.DocIO.ReaderWriter.Security;

namespace DocGen.DocIO.ReaderWriter.Biff_Records.Structures;

[CLSCompliant(false)]
internal class DOPDescriptor : BaseWordRecord
{
	private const int DEF_PROTECTION_KEY = 0;

	internal const int DEF_MAX_PASSWORDLEN = 15;

	private const ushort DEF_PASSWORD_CONST = 52811;

	private byte m_nfcEdnRef = 2;

	private byte m_nfcFtnRef;

	private byte m_bepc = 3;

	private int m_nEdn = 1;

	private byte m_rncEdn;

	private int m_nFtn = 1;

	private byte m_rncFtn;

	private bool m_bFacingPage;

	private bool m_bWidowControl = true;

	private bool m_bPMHMainDoc;

	private int m_grfSuppression;

	private byte m_fpc = 1;

	private int m_unused0_7;

	private int m_grpfIhdt;

	private uint m_flagA = 411568305u;

	private Copts60 m_copts60;

	private ushort m_dxaTabs = 720;

	private int m_wSpare = 1251;

	private int m_dxaHotZ = 360;

	private int m_cConsecHypLim;

	private int m_wSpare2;

	private uint m_dttmCreated;

	private uint m_dttmRevised;

	private uint m_dttmLastPrint;

	private int m_nRevision;

	private int m_tmEdited;

	private int m_cWords;

	private int m_cCh;

	private int m_cPg;

	private int m_cParas;

	private ushort m_End = 4;

	private ushort m_epc = 4099;

	private int m_cLines;

	private int m_wordsFtnEnd;

	private int m_cChFtnEdn;

	private int m_cPgFtnEdn;

	private int m_cParasFrnEdn;

	private int m_cLinesFtnEdn;

	private uint m_lKeyProtDoc;

	private ushort m_wvkSaved = 801;

	private bool m_shadeFormData = true;

	internal byte[] m_dopLeftData;

	private DateTime m_created = DateTime.Now;

	private DateTime m_revised = DateTime.Now;

	private DateTime m_lastPrinted = DateTime.MinValue;

	private TimeSpan m_editTime = TimeSpan.MinValue;

	private DOP95 m_dop95;

	private DOP97 m_dop97;

	private DOP2000 m_dop2000;

	private DOP2002 m_dop2002;

	private DOP2003 m_dop2003;

	private DOP2007 m_dop2007;

	internal DOP95 Dop95
	{
		get
		{
			if (m_dop95 == null)
			{
				m_dop95 = new DOP95(this);
			}
			return m_dop95;
		}
	}

	internal DOP97 Dop97
	{
		get
		{
			if (m_dop97 == null)
			{
				m_dop97 = new DOP97(this);
			}
			return m_dop97;
		}
	}

	internal DOP2000 Dop2000
	{
		get
		{
			if (m_dop2000 == null)
			{
				m_dop2000 = new DOP2000(this);
			}
			return m_dop2000;
		}
	}

	internal DOP2002 Dop2002
	{
		get
		{
			if (m_dop2002 == null)
			{
				m_dop2002 = new DOP2002(this);
			}
			return m_dop2002;
		}
	}

	internal DOP2003 Dop2003
	{
		get
		{
			if (m_dop2003 == null)
			{
				m_dop2003 = new DOP2003(this);
			}
			return m_dop2003;
		}
	}

	internal DOP2007 Dop2007
	{
		get
		{
			if (m_dop2007 == null)
			{
				m_dop2007 = new DOP2007(this);
			}
			return m_dop2007;
		}
	}

	internal Copts60 Copts60
	{
		get
		{
			if (m_copts60 == null)
			{
				m_copts60 = new Copts60(this);
			}
			return m_copts60;
		}
	}

	internal byte EndnoteNumberFormat
	{
		get
		{
			return (byte)Dop97.NfcEdnRef;
		}
		set
		{
			if (Dop97.NfcEdnRef < 5)
			{
				Dop97.NfcEdnRef = value;
			}
		}
	}

	internal byte FootnoteNumberFormat
	{
		get
		{
			return (byte)Dop97.NfcFtnRef;
		}
		set
		{
			if (Dop97.NfcFtnRef < 5)
			{
				Dop97.NfcFtnRef = value;
			}
		}
	}

	internal byte EndnotePosition
	{
		get
		{
			return m_bepc;
		}
		set
		{
			m_bepc = value;
		}
	}

	internal int InitialEndnoteNumber
	{
		get
		{
			return m_nEdn;
		}
		set
		{
			m_nEdn = value;
		}
	}

	internal byte RestartIndexForEndnote
	{
		get
		{
			return m_rncEdn;
		}
		set
		{
			if (m_rncEdn < 3)
			{
				m_rncEdn = value;
			}
		}
	}

	internal int InitialFootnoteNumber
	{
		get
		{
			return m_nFtn;
		}
		set
		{
			m_nFtn = value;
		}
	}

	internal byte RestartIndexForFootnotes
	{
		get
		{
			return m_rncFtn;
		}
		set
		{
			if (value < 3)
			{
				m_rncFtn = value;
			}
		}
	}

	internal byte FootnotePosition
	{
		get
		{
			return m_fpc;
		}
		set
		{
			if (value < 3)
			{
				m_fpc = value;
			}
		}
	}

	internal ProtectionType ProtectionType
	{
		get
		{
			if (!Dop2003.EnforceDocProt || Dop2003.DocProtCur == 7)
			{
				return ProtectionType.NoProtection;
			}
			if (ProtEnabled && Dop2003.DocProtCur == 2)
			{
				return ProtectionType.AllowOnlyFormFields;
			}
			if (LockAtn)
			{
				if (Dop2003.TreatLockAtnAsReadOnly && Dop2003.DocProtCur == 3)
				{
					return ProtectionType.AllowOnlyReading;
				}
				if (Dop2003.DocProtCur == 1)
				{
					return ProtectionType.AllowOnlyComments;
				}
			}
			if (LockRev && Dop2003.DocProtCur == 0)
			{
				return ProtectionType.AllowOnlyRevisions;
			}
			return ProtectionType.NoProtection;
		}
		set
		{
			if (LockRev && Dop2003.DocProtCur == 0)
			{
				RevMarking = false;
			}
			LockAtn = false;
			ProtEnabled = false;
			LockRev = false;
			Dop2003.EnforceDocProt = false;
			Dop2003.DocProtCur = 3;
			switch (value)
			{
			case ProtectionType.AllowOnlyComments:
				Dop2003.EnforceDocProt = true;
				Dop2003.DocProtCur = 1;
				LockAtn = true;
				break;
			case ProtectionType.AllowOnlyFormFields:
				Dop2003.EnforceDocProt = true;
				Dop2003.DocProtCur = 2;
				ProtEnabled = true;
				break;
			case ProtectionType.AllowOnlyRevisions:
				Dop2003.EnforceDocProt = true;
				Dop2003.DocProtCur = 0;
				LockRev = true;
				break;
			case ProtectionType.AllowOnlyReading:
				LockAtn = true;
				Dop2003.TreatLockAtnAsReadOnly = true;
				Dop2003.EnforceDocProt = true;
				Dop2003.DocProtCur = 3;
				break;
			default:
				throw new ArgumentException("Unknown protection specified.");
			case ProtectionType.NoProtection:
				break;
			}
			if (ProtectionType != ProtectionType.NoProtection && m_lKeyProtDoc == 0)
			{
				m_lKeyProtDoc = 0u;
			}
		}
	}

	internal uint ProtectionKey => m_lKeyProtDoc;

	internal bool OddAndEvenPagesHeaderFooter
	{
		get
		{
			return m_bFacingPage;
		}
		set
		{
			m_bFacingPage = value;
		}
	}

	internal byte ViewType
	{
		get
		{
			return (byte)(m_wvkSaved & 7u);
		}
		set
		{
			m_wvkSaved &= 65528;
			m_wvkSaved = value;
		}
	}

	internal ushort ZoomPercent
	{
		get
		{
			return (ushort)((m_wvkSaved & 0xFF8) >> 3);
		}
		set
		{
			m_wvkSaved &= 61447;
			m_wvkSaved += (ushort)(value << 3);
		}
	}

	internal byte ZoomType
	{
		get
		{
			return (byte)((m_wvkSaved & 0x3000) >> 12);
		}
		set
		{
			m_wvkSaved &= 53247;
			m_wvkSaved += (ushort)(value << 12);
		}
	}

	internal ushort DefaultTabWidth
	{
		get
		{
			return m_dxaTabs;
		}
		set
		{
			m_dxaTabs = value;
		}
	}

	internal int DxaHotZ
	{
		get
		{
			return m_dxaHotZ;
		}
		set
		{
			m_dxaHotZ = value;
		}
	}

	internal int ConsecHypLim
	{
		get
		{
			return m_cConsecHypLim;
		}
		set
		{
			m_cConsecHypLim = value;
		}
	}

	internal bool SpellAllDone
	{
		get
		{
			return (m_flagA & 0x40) >> 6 != 0;
		}
		set
		{
			m_flagA = (uint)((m_flagA & 0xFFFFFFBFu) | (int)((value ? 1u : 0u) << 6));
		}
	}

	internal bool SpellAllClean
	{
		get
		{
			return (m_flagA & 0x80) >> 7 != 0;
		}
		set
		{
			m_flagA = (uint)((m_flagA & 0xFFFFFF7Fu) | (int)((value ? 1u : 0u) << 7));
		}
	}

	internal bool SpellHideErrors
	{
		get
		{
			return (m_flagA & 0x100) >> 8 != 0;
		}
		set
		{
			m_flagA = (uint)((m_flagA & 0xFFFFFEFFu) | (int)((value ? 1u : 0u) << 8));
		}
	}

	internal bool GramHideErrors
	{
		get
		{
			return (m_flagA & 0x200) >> 9 != 0;
		}
		set
		{
			m_flagA = (uint)((m_flagA & 0xFFFFFDFFu) | (int)((value ? 1u : 0u) << 9));
		}
	}

	internal bool LabelDoc
	{
		get
		{
			return (m_flagA & 0x400) >> 10 != 0;
		}
		set
		{
			m_flagA = (uint)((m_flagA & 0xFFFFFBFFu) | (int)((value ? 1u : 0u) << 10));
		}
	}

	internal bool HyphCapitals
	{
		get
		{
			return (m_flagA & 0x800) >> 11 != 0;
		}
		set
		{
			m_flagA = (uint)((m_flagA & 0xFFFFF7FFu) | (int)((value ? 1u : 0u) << 11));
		}
	}

	internal bool AutoHyphen
	{
		get
		{
			return (m_flagA & 0x1000) >> 12 != 0;
		}
		set
		{
			m_flagA = (uint)((m_flagA & 0xFFFFEFFFu) | (int)((value ? 1u : 0u) << 12));
		}
	}

	internal bool FormNoFields
	{
		get
		{
			return (m_flagA & 0x2000) >> 13 != 0;
		}
		set
		{
			m_flagA = (uint)((m_flagA & 0xFFFFDFFFu) | (int)((value ? 1u : 0u) << 13));
		}
	}

	internal bool LinkStyles
	{
		get
		{
			return (m_flagA & 0x4000) >> 14 != 0;
		}
		set
		{
			m_flagA = (uint)((m_flagA & 0xFFFFBFFFu) | (int)((value ? 1u : 0u) << 14));
		}
	}

	internal bool RevMarking
	{
		get
		{
			return (m_flagA & 0x8000) >> 15 != 0;
		}
		set
		{
			m_flagA = (uint)((m_flagA & 0xFFFF7FFFu) | (int)((value ? 1u : 0u) << 15));
		}
	}

	internal bool ExactCWords
	{
		get
		{
			return (m_flagA & 0x20000) >> 17 != 0;
		}
		set
		{
			m_flagA = (uint)((m_flagA & 0xFFFDFFFFu) | (int)((value ? 1u : 0u) << 17));
		}
	}

	internal bool PagHidden
	{
		get
		{
			return (m_flagA & 0x40000) >> 18 != 0;
		}
		set
		{
			m_flagA = (uint)((m_flagA & 0xFFFBFFFFu) | (int)((value ? 1u : 0u) << 18));
		}
	}

	internal bool PagResults
	{
		get
		{
			return (m_flagA & 0x80000) >> 19 != 0;
		}
		set
		{
			m_flagA = (uint)((m_flagA & 0xFFF7FFFFu) | (int)((value ? 1u : 0u) << 19));
		}
	}

	internal bool LockAtn
	{
		get
		{
			return (m_flagA & 0x100000) >> 20 != 0;
		}
		set
		{
			m_flagA = (uint)((m_flagA & 0xFFEFFFFFu) | (int)((value ? 1u : 0u) << 20));
		}
	}

	internal bool MirrorMargins
	{
		get
		{
			return (m_flagA & 0x200000) >> 21 != 0;
		}
		set
		{
			m_flagA = (uint)((m_flagA & 0xFFEFFFFFu) | (int)((value ? 1u : 0u) << 21));
		}
	}

	internal bool Word97Compat
	{
		get
		{
			return (m_flagA & 0x400000) >> 22 != 0;
		}
		set
		{
			m_flagA = (uint)((m_flagA & 0xFFBFFFFFu) | (int)((value ? 1u : 0u) << 22));
		}
	}

	internal bool ProtEnabled
	{
		get
		{
			return (m_flagA & 0x2000000) >> 25 != 0;
		}
		set
		{
			m_flagA = (uint)((m_flagA & 0xFDFFFFFFu) | (int)((value ? 1u : 0u) << 25));
		}
	}

	internal bool DispFormFldSel
	{
		get
		{
			return (m_flagA & 0x4000000) >> 26 != 0;
		}
		set
		{
			m_flagA = (uint)((m_flagA & 0xFBFFFFFFu) | (int)((value ? 1u : 0u) << 26));
		}
	}

	internal bool RMView
	{
		get
		{
			return (m_flagA & 0x8000000) >> 27 != 0;
		}
		set
		{
			m_flagA = (uint)((m_flagA & 0xF7FFFFFFu) | (int)((value ? 1u : 0u) << 27));
		}
	}

	internal bool RMPrint
	{
		get
		{
			return (m_flagA & 0x10000000) >> 28 != 0;
		}
		set
		{
			m_flagA = (uint)((m_flagA & 0xEFFFFFFFu) | (int)((value ? 1u : 0u) << 28));
		}
	}

	internal bool LockVbaProj
	{
		get
		{
			return (m_flagA & 0x20000000) >> 29 != 0;
		}
		set
		{
			m_flagA = (uint)((m_flagA & 0xDFFFFFFFu) | (int)((value ? 1u : 0u) << 29));
		}
	}

	internal bool LockRev
	{
		get
		{
			return (m_flagA & 0x40000000) >> 30 != 0;
		}
		set
		{
			m_flagA = (uint)((m_flagA & 0xBFFFFFFFu) | (int)((value ? 1u : 0u) << 30));
		}
	}

	internal bool EmbedFonts
	{
		get
		{
			return (m_flagA & 0x80000000u) >> 31 != 0;
		}
		set
		{
			m_flagA = (uint)((m_flagA & 0x7FFFFFFFu) | (int)((value ? 1u : 0u) << 31));
		}
	}

	internal byte[] DopInternalData
	{
		get
		{
			return m_dopLeftData;
		}
		set
		{
			m_dopLeftData = value;
		}
	}

	internal bool FormFieldShading
	{
		get
		{
			return m_shadeFormData;
		}
		set
		{
			m_shadeFormData = value;
		}
	}

	internal bool GutterAtTop
	{
		get
		{
			return (m_flagA & 8) >> 3 != 0;
		}
		set
		{
			m_flagA = (uint)((m_flagA & 0xFFFFFFF7u) | (int)((value ? 1u : 0u) << 3));
		}
	}

	internal DOPDescriptor()
	{
	}

	internal DOPDescriptor(Stream stream, int dopStart, int dopLength, bool isTemplate)
		: this()
	{
		stream.Position = dopStart;
		int num = BaseWordRecord.ReadUInt16(stream);
		m_bFacingPage = (num & 1) != 0;
		m_bWidowControl = (num & 2) != 0;
		m_bPMHMainDoc = (num & 4) != 0;
		m_grfSuppression = (num & 0x18) >> 3;
		m_fpc = (byte)((num & 0x60) >> 5);
		m_unused0_7 = (num & 0x80) >> 7;
		m_grpfIhdt = (num & 0xFF00) >> 8;
		num = BaseWordRecord.ReadUInt16(stream);
		m_rncFtn = (byte)((uint)num & 3u);
		m_nFtn = (num & 0xFFFC) >> 2;
		m_flagA = BaseWordRecord.ReadUInt32(stream);
		Copts60.Parse(stream);
		m_dxaTabs = BaseWordRecord.ReadUInt16(stream);
		m_wSpare = BaseWordRecord.ReadUInt16(stream);
		m_dxaHotZ = BaseWordRecord.ReadUInt16(stream);
		m_cConsecHypLim = BaseWordRecord.ReadUInt16(stream);
		m_wSpare2 = BaseWordRecord.ReadUInt16(stream);
		m_dttmCreated = BaseWordRecord.ReadUInt32(stream);
		if (isTemplate)
		{
			m_created = DateTime.Now;
		}
		else
		{
			m_created = ParseDateTime(m_dttmCreated);
		}
		m_dttmRevised = BaseWordRecord.ReadUInt32(stream);
		if (isTemplate)
		{
			m_revised = DateTime.Now;
		}
		else
		{
			m_revised = ParseDateTime(m_dttmRevised);
		}
		m_dttmLastPrint = BaseWordRecord.ReadUInt32(stream);
		if (isTemplate)
		{
			m_lastPrinted = DateTime.Now;
		}
		else
		{
			m_lastPrinted = ParseDateTime(m_dttmLastPrint);
		}
		m_nRevision = BaseWordRecord.ReadInt16(stream);
		m_tmEdited = BaseWordRecord.ReadInt32(stream);
		m_editTime = TimeSpan.FromMinutes(m_tmEdited);
		m_cWords = BaseWordRecord.ReadInt32(stream);
		m_cCh = BaseWordRecord.ReadInt32(stream);
		m_cPg = BaseWordRecord.ReadInt16(stream);
		m_cParas = BaseWordRecord.ReadInt32(stream);
		m_End = BaseWordRecord.ReadUInt16(stream);
		m_rncEdn = (byte)(m_End & 3u);
		m_nEdn = (m_End & 0xFFFC) >> 2;
		m_epc = BaseWordRecord.ReadUInt16(stream);
		m_nfcFtnRef = (byte)((m_epc & 0x3C) >> 2);
		m_nfcEdnRef = (byte)((m_epc & 0x3C0) >> 6);
		m_bepc = (byte)(m_epc & 3u);
		m_shadeFormData = (m_epc & 0x1000) == 4096;
		m_cLines = BaseWordRecord.ReadInt32(stream);
		m_wordsFtnEnd = BaseWordRecord.ReadInt32(stream);
		m_cChFtnEdn = BaseWordRecord.ReadInt32(stream);
		m_cPgFtnEdn = BaseWordRecord.ReadInt16(stream);
		m_cParasFrnEdn = BaseWordRecord.ReadInt32(stream);
		m_cLinesFtnEdn = BaseWordRecord.ReadInt32(stream);
		m_lKeyProtDoc = BaseWordRecord.ReadUInt32(stream);
		m_wvkSaved = BaseWordRecord.ReadUInt16(stream);
		GutterAtTop = (m_wvkSaved & 0x8000) != 0;
		if (dopLength >= 88)
		{
			Dop95.Parse(stream);
		}
		if (dopLength >= 500)
		{
			Dop97.Parse(stream);
		}
		if (dopLength >= 544)
		{
			Dop2000.Parse(stream);
		}
		if (dopLength >= 594)
		{
			Dop2002.Parse(stream);
		}
		if (dopLength >= 616)
		{
			Dop2003.Parse(stream);
		}
		else if (LockAtn)
		{
			Dop2003.EnforceDocProt = true;
			Dop2003.DocProtCur = 1;
		}
		else if (ProtEnabled)
		{
			Dop2003.EnforceDocProt = true;
			Dop2003.DocProtCur = 2;
		}
		else if (LockRev)
		{
			Dop2003.EnforceDocProt = true;
			Dop2003.DocProtCur = 0;
		}
		if (dopLength > 674)
		{
			Dop2007.Parse(stream);
		}
		if (dopLength > (int)stream.Position - dopStart)
		{
			int num2 = dopLength - ((int)stream.Position - dopStart);
			m_dopLeftData = new byte[num2];
			stream.Read(m_dopLeftData, 0, num2);
		}
		if (ProtEnabled)
		{
			if (!Dop2003.EnforceDocProt)
			{
				Dop2003.EnforceDocProt = true;
			}
			if (Dop2003.DocProtCur != 2)
			{
				Dop2003.DocProtCur = 2;
			}
		}
	}

	internal void UpdateDateTime(BuiltinDocumentProperties builtInDocumnetProperties)
	{
		m_created = new DateTime(builtInDocumnetProperties.CreateDate.Ticks);
		m_revised = new DateTime(builtInDocumnetProperties.LastSaveDate.Ticks);
		m_lastPrinted = new DateTime(builtInDocumnetProperties.LastPrinted.Ticks);
		m_editTime = builtInDocumnetProperties.TotalEditingTime;
	}

	internal uint Write(Stream stream)
	{
		m_bWidowControl = false;
		long position = stream.Position;
		int num = 0;
		num |= (m_bFacingPage ? 1 : 0);
		num |= (m_bWidowControl ? 2 : 0);
		num |= (m_bPMHMainDoc ? 4 : 0);
		num |= m_grfSuppression << 3;
		num |= m_fpc << 5;
		num |= m_unused0_7 << 7;
		num |= m_grpfIhdt << 8;
		BaseWordRecord.WriteUInt16(stream, (ushort)num);
		num = 0;
		num |= m_rncFtn;
		num |= m_nFtn << 2;
		BaseWordRecord.WriteUInt16(stream, (ushort)num);
		if (ProtectionType != ProtectionType.AllowOnlyFormFields)
		{
			DispFormFldSel = false;
		}
		if (!ProtEnabled)
		{
			FormNoFields = false;
		}
		BaseWordRecord.WriteUInt32(stream, m_flagA);
		Copts60.Write(stream);
		BaseWordRecord.WriteUInt16(stream, m_dxaTabs);
		BaseWordRecord.WriteUInt16(stream, (ushort)m_wSpare);
		BaseWordRecord.WriteUInt16(stream, (ushort)m_dxaHotZ);
		BaseWordRecord.WriteUInt16(stream, (ushort)m_cConsecHypLim);
		BaseWordRecord.WriteUInt16(stream, (ushort)m_wSpare2);
		BaseWordRecord.WriteUInt32(stream, SetDateTime(m_created));
		BaseWordRecord.WriteUInt32(stream, SetDateTime(m_revised));
		if (m_lastPrinted != DateTime.MinValue)
		{
			BaseWordRecord.WriteUInt32(stream, SetDateTime(m_lastPrinted));
		}
		else
		{
			BaseWordRecord.WriteInt32(stream, 0);
		}
		BaseWordRecord.WriteInt16(stream, (short)m_nRevision);
		if (m_editTime != TimeSpan.MinValue)
		{
			m_tmEdited = (int)m_editTime.TotalMinutes;
		}
		BaseWordRecord.WriteInt32(stream, m_tmEdited);
		BaseWordRecord.WriteInt32(stream, m_cWords);
		BaseWordRecord.WriteInt32(stream, m_cCh);
		BaseWordRecord.WriteInt16(stream, (short)m_cPg);
		BaseWordRecord.WriteInt32(stream, m_cParas);
		m_End = 0;
		m_End |= m_rncEdn;
		m_End |= (ushort)(m_nEdn << 2);
		BaseWordRecord.WriteUInt16(stream, m_End);
		m_epc = (ushort)BaseWordRecord.SetBitsByMask(m_epc, 3, m_bepc);
		m_epc = (ushort)BaseWordRecord.SetBitsByMask(m_epc, 60, m_nfcFtnRef);
		m_epc = (ushort)BaseWordRecord.SetBitsByMask(m_epc, 960, m_nfcEdnRef);
		if (m_shadeFormData)
		{
			m_epc |= 4096;
		}
		else
		{
			m_epc = (ushort)BaseWordRecord.SetBitsByMask(m_epc, 4096, 0);
		}
		BaseWordRecord.WriteUInt16(stream, m_epc);
		BaseWordRecord.WriteInt32(stream, m_cLines);
		BaseWordRecord.WriteInt32(stream, m_wordsFtnEnd);
		BaseWordRecord.WriteInt32(stream, m_cChFtnEdn);
		BaseWordRecord.WriteInt16(stream, (short)m_cPgFtnEdn);
		BaseWordRecord.WriteInt32(stream, m_cParasFrnEdn);
		BaseWordRecord.WriteInt32(stream, m_cLinesFtnEdn);
		BaseWordRecord.WriteUInt32(stream, m_lKeyProtDoc);
		if (GutterAtTop)
		{
			m_wvkSaved |= 32768;
		}
		BaseWordRecord.WriteUInt16(stream, m_wvkSaved);
		Dop95.Write(stream);
		Dop97.Write(stream);
		Dop2000.Write(stream);
		Dop2002.Write(stream);
		Dop2003.Write(stream);
		Dop2007.Write(stream);
		if (m_dopLeftData != null)
		{
			stream.Write(m_dopLeftData, 0, m_dopLeftData.Length);
		}
		return (uint)(stream.Position - position);
	}

	internal DOPDescriptor Clone()
	{
		return (DOPDescriptor)MemberwiseClone();
	}

	internal void SetProtection(ProtectionType type, string password)
	{
		ProtectionType = type;
		if (string.IsNullOrEmpty(password))
		{
			m_lKeyProtDoc = 0u;
		}
		else if (type != ProtectionType.NoProtection)
		{
			m_lKeyProtDoc = WordDecryptor.GetPasswordHash(password);
		}
	}

	private DateTime ParseDateTime(uint dateTime)
	{
		if (dateTime == 0)
		{
			return DateTime.MinValue;
		}
		ushort num = (ushort)(dateTime & 0xFFFFu);
		ushort num2 = (ushort)((dateTime & 0xFFFF0000u) >> 16);
		int num3 = num & 0x3F;
		if (num3 < 0 || num3 > 59)
		{
			return DateTime.Now;
		}
		int num4 = (num & 0x7C0) >> 6;
		if (num4 < 0 || num4 > 23)
		{
			return DateTime.Now;
		}
		int num5 = (num & 0xF800) >> 11;
		if (num5 < 1 || num5 > 31)
		{
			return DateTime.Now;
		}
		int num6 = num2 & 0xF;
		if (num6 < 1 || num6 > 12)
		{
			return DateTime.Now;
		}
		int num7 = ((num2 & 0x1FF0) >> 4) + 1900;
		if (num7 < 1900 || num7 > 2411)
		{
			return DateTime.Now;
		}
		return new DateTime(num7, num6, num5, num4, num3, 0, 0);
	}

	private uint SetDateTime(DateTime dt)
	{
		return (uint)(dt.Minute | (dt.Hour << 6) | (dt.Day << 11) | (dt.Month << 16) | (dt.Year - 1900 << 20)) | (ConvertDayOfWeek(dt.DayOfWeek) << 29);
	}

	private uint ConvertDayOfWeek(DayOfWeek dow)
	{
		return dow switch
		{
			DayOfWeek.Monday => 1u, 
			DayOfWeek.Tuesday => 2u, 
			DayOfWeek.Wednesday => 3u, 
			DayOfWeek.Thursday => 4u, 
			DayOfWeek.Friday => 5u, 
			DayOfWeek.Saturday => 6u, 
			_ => 0u, 
		};
	}

	[CLSCompliant(false)]
	internal static ushort GetPasswordHash(string password)
	{
		if (password == null)
		{
			throw new ArgumentNullException("password");
		}
		if (password.Length > 15)
		{
			throw new ArgumentOutOfRangeException("Length of the password can't be more than " + 15);
		}
		ushort num = 0;
		int i = 0;
		for (int length = password.Length; i < length; i++)
		{
			ushort uInt16FromBits = GetUInt16FromBits(RotateBits(GetCharBits15(password[i]), i + 1));
			num ^= uInt16FromBits;
		}
		return (ushort)((uint)(num ^ password.Length) ^ 0xCE4Bu);
	}

	private static bool[] GetCharBits15(char charToConvert)
	{
		bool[] array = new bool[15];
		ushort num = Convert.ToUInt16(charToConvert);
		ushort num2 = 1;
		for (int i = 0; i < 15; i++)
		{
			array[i] = (num & num2) == num2;
			num2 <<= 1;
		}
		return array;
	}

	private static ushort GetUInt16FromBits(bool[] bits)
	{
		if (bits == null)
		{
			throw new ArgumentNullException("bits");
		}
		if (bits.Length > 16)
		{
			throw new ArgumentOutOfRangeException("There can't be more than 16 bits");
		}
		_ = new bool[15];
		ushort num = 0;
		ushort num2 = 1;
		int i = 0;
		for (int num3 = bits.Length; i < num3; i++)
		{
			if (bits[i])
			{
				num += num2;
			}
			num2 <<= 1;
		}
		return num;
	}

	private static bool[] RotateBits(bool[] bits, int count)
	{
		if (bits == null)
		{
			throw new ArgumentNullException("bits");
		}
		if (bits.Length == 0)
		{
			return bits;
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count can't be less than zero");
		}
		bool[] array = new bool[bits.Length];
		int i = 0;
		for (int num = bits.Length; i < num; i++)
		{
			int num2 = (i + count) % num;
			array[num2] = bits[i];
		}
		return array;
	}

	internal static int Round(int value, int degree)
	{
		if (degree == 0)
		{
			throw new ArgumentOutOfRangeException("degree can't be 0");
		}
		int num = value % degree;
		return value - num + degree;
	}
}
