using System;
using System.IO;

namespace DocGen.DocIO.ReaderWriter.Biff_Records.Structures;

[CLSCompliant(false)]
internal class Copts
{
	private uint m_flagsA = 4126933000u;

	private byte m_flagsg;

	private DOPDescriptor m_dopBase;

	internal Copts80 Copts80 => m_dopBase.Dop95.Copts80;

	internal bool SpLayoutLikeWW8
	{
		get
		{
			return (m_flagsA & 1) != 0;
		}
		set
		{
			m_flagsA = (uint)((m_flagsA & 0xFFFFFFFEu) | (value ? 1 : 0));
		}
	}

	internal bool FtnLayoutLikeWW8
	{
		get
		{
			return (m_flagsA & 2) >> 1 != 0;
		}
		set
		{
			m_flagsA = (uint)((m_flagsA & 0xFFFFFFFDu) | (int)((value ? 1u : 0u) << 1));
		}
	}

	internal bool DontUseHTMLParagraphAutoSpacing
	{
		get
		{
			return (m_flagsA & 4) >> 2 != 0;
		}
		set
		{
			m_flagsA = (uint)((m_flagsA & 0xFFFFFFFBu) | (int)((value ? 1u : 0u) << 2));
		}
	}

	internal bool DontAdjustLineHeightInTable
	{
		get
		{
			return (m_flagsA & 8) >> 3 != 0;
		}
		set
		{
			m_flagsA = (uint)((m_flagsA & 0xFFFFFFF7u) | (int)((value ? 1u : 0u) << 3));
		}
	}

	internal bool ForgetLastTabAlign
	{
		get
		{
			return (m_flagsA & 0x10) >> 4 != 0;
		}
		set
		{
			m_flagsA = (uint)((m_flagsA & 0xFFFFFFEFu) | (int)((value ? 1u : 0u) << 4));
		}
	}

	internal bool UseAutospaceForFullWidthAlpha
	{
		get
		{
			return (m_flagsA & 0x20) >> 5 != 0;
		}
		set
		{
			m_flagsA = (uint)((m_flagsA & 0xFFFFFFDFu) | (int)((value ? 1u : 0u) << 5));
		}
	}

	internal bool AlignTablesRowByRow
	{
		get
		{
			return (m_flagsA & 0x40) >> 6 != 0;
		}
		set
		{
			m_flagsA = (uint)((m_flagsA & 0xFFFFFFBFu) | (int)((value ? 1u : 0u) << 6));
		}
	}

	internal bool LayoutRawTableWidth
	{
		get
		{
			return (m_flagsA & 0x80) >> 7 != 0;
		}
		set
		{
			m_flagsA = (uint)((m_flagsA & 0xFFFFFF7Fu) | (int)((value ? 1u : 0u) << 7));
		}
	}

	internal bool LayoutTableRowsApart
	{
		get
		{
			return (m_flagsA & 0x100) >> 8 != 0;
		}
		set
		{
			m_flagsA = (uint)((m_flagsA & 0xFFFFFEFFu) | (int)((value ? 1u : 0u) << 8));
		}
	}

	internal bool UseWord97LineBreakingRules
	{
		get
		{
			return (m_flagsA & 0x200) >> 9 != 0;
		}
		set
		{
			m_flagsA = (uint)((m_flagsA & 0xFFFFFDFFu) | (int)((value ? 1u : 0u) << 9));
		}
	}

	internal bool DontBreakWrappedTables
	{
		get
		{
			return (m_flagsA & 0x400) >> 10 != 0;
		}
		set
		{
			m_flagsA = (uint)((m_flagsA & 0xFFFFFBFFu) | (int)((value ? 1u : 0u) << 10));
		}
	}

	internal bool DontSnapToGridInCell
	{
		get
		{
			return (m_flagsA & 0x800) >> 11 != 0;
		}
		set
		{
			m_flagsA = (uint)((m_flagsA & 0xFFFFF7FFu) | (int)((value ? 1u : 0u) << 11));
		}
	}

	internal bool DontAllowFieldEndSelect
	{
		get
		{
			return (m_flagsA & 0x1000) >> 12 != 0;
		}
		set
		{
			m_flagsA = (uint)((m_flagsA & 0xFFFFEFFFu) | (int)((value ? 1u : 0u) << 12));
		}
	}

	internal bool ApplyBreakingRules
	{
		get
		{
			return (m_flagsA & 0x2000) >> 13 != 0;
		}
		set
		{
			m_flagsA = (uint)((m_flagsA & 0xFFFFDFFFu) | (int)((value ? 1u : 0u) << 13));
		}
	}

	internal bool DontWrapTextWithPunct
	{
		get
		{
			return (m_flagsA & 0x4000) >> 14 != 0;
		}
		set
		{
			m_flagsA = (uint)((m_flagsA & 0xFFFFBFFFu) | (int)((value ? 1u : 0u) << 14));
		}
	}

	internal bool DontUseAsianBreakRules
	{
		get
		{
			return (m_flagsA & 0x8000) >> 15 != 0;
		}
		set
		{
			m_flagsA = (uint)((m_flagsA & 0xFFFF7FFFu) | (int)((value ? 1u : 0u) << 15));
		}
	}

	internal bool UseWord2002TableStyleRules
	{
		get
		{
			return (m_flagsA & 0x10000) >> 16 != 0;
		}
		set
		{
			m_flagsA = (uint)((m_flagsA & 0xFFFEFFFFu) | (int)((value ? 1u : 0u) << 16));
		}
	}

	internal bool GrowAutoFit
	{
		get
		{
			return (m_flagsA & 0x20000) >> 17 != 0;
		}
		set
		{
			m_flagsA = (uint)((m_flagsA & 0xFFFDFFFFu) | (int)((value ? 1u : 0u) << 17));
		}
	}

	internal bool UseNormalStyleForList
	{
		get
		{
			return (m_flagsA & 0x40000) >> 18 != 0;
		}
		set
		{
			m_flagsA = (uint)((m_flagsA & 0xFFFBFFFFu) | (int)((value ? 1u : 0u) << 18));
		}
	}

	internal bool DontUseIndentAsNumberingTabStop
	{
		get
		{
			return (m_flagsA & 0x80000) >> 19 != 0;
		}
		set
		{
			m_flagsA = (uint)((m_flagsA & 0xFFF7FFFFu) | (int)((value ? 1u : 0u) << 19));
		}
	}

	internal bool FELineBreak11
	{
		get
		{
			return (m_flagsA & 0x100000) >> 20 != 0;
		}
		set
		{
			m_flagsA = (uint)((m_flagsA & 0xFFEFFFFFu) | (int)((value ? 1u : 0u) << 20));
		}
	}

	internal bool AllowSpaceOfSameStyleInTable
	{
		get
		{
			return (m_flagsA & 0x200000) >> 21 != 0;
		}
		set
		{
			m_flagsA = (uint)((m_flagsA & 0xFFDFFFFFu) | (int)((value ? 1u : 0u) << 21));
		}
	}

	internal bool WW11IndentRules
	{
		get
		{
			return (m_flagsA & 0x400000) >> 22 != 0;
		}
		set
		{
			m_flagsA = (uint)((m_flagsA & 0xFFBFFFFFu) | (int)((value ? 1u : 0u) << 22));
		}
	}

	internal bool DontAutofitConstrainedTables
	{
		get
		{
			return (m_flagsA & 0x800000) >> 23 != 0;
		}
		set
		{
			m_flagsA = (uint)((m_flagsA & 0xFF7FFFFFu) | (int)((value ? 1u : 0u) << 23));
		}
	}

	internal bool AutofitLikeWW11
	{
		get
		{
			return (m_flagsA & 0x1000000) >> 24 != 0;
		}
		set
		{
			m_flagsA = (uint)((m_flagsA & 0xFEFFFFFFu) | (int)((value ? 1u : 0u) << 24));
		}
	}

	internal bool UnderlineTabInNumList
	{
		get
		{
			return (m_flagsA & 0x2000000) >> 25 != 0;
		}
		set
		{
			m_flagsA = (uint)((m_flagsA & 0xFDFFFFFFu) | (int)((value ? 1u : 0u) << 25));
		}
	}

	internal bool HangulWidthLikeWW11
	{
		get
		{
			return (m_flagsA & 0x4000000) >> 26 != 0;
		}
		set
		{
			m_flagsA = (uint)((m_flagsA & 0xFBFFFFFFu) | (int)((value ? 1u : 0u) << 26));
		}
	}

	internal bool SplitPgBreakAndParaMark
	{
		get
		{
			return (m_flagsA & 0x8000000) >> 27 != 0;
		}
		set
		{
			m_flagsA = (uint)((m_flagsA & 0xF7FFFFFFu) | (int)((value ? 1u : 0u) << 27));
		}
	}

	internal bool DontVertAlignCellWithSp
	{
		get
		{
			return (m_flagsA & 0x10000000) >> 28 != 0;
		}
		set
		{
			m_flagsA = (uint)((m_flagsA & 0xEFFFFFFFu) | (int)((value ? 1u : 0u) << 28));
		}
	}

	internal bool DontBreakConstrainedForcedTables
	{
		get
		{
			return (m_flagsA & 0x20000000) >> 29 != 0;
		}
		set
		{
			m_flagsA = (uint)((m_flagsA & 0xDFFFFFFFu) | (int)((value ? 1u : 0u) << 29));
		}
	}

	internal bool DontVertAlignInTxbx
	{
		get
		{
			return (m_flagsA & 0x40000000) >> 30 != 0;
		}
		set
		{
			m_flagsA = (uint)((m_flagsA & 0xBFFFFFFFu) | (int)((value ? 1u : 0u) << 30));
		}
	}

	internal bool Word11KerningPairs
	{
		get
		{
			return (m_flagsA & 0x80000000u) >> 31 != 0;
		}
		set
		{
			m_flagsA = (uint)((m_flagsA & 0x7FFFFFFFu) | (int)((value ? 1u : 0u) << 31));
		}
	}

	internal bool CachedColBalance
	{
		get
		{
			return (m_flagsg & 1) != 0;
		}
		set
		{
			m_flagsg = (byte)((m_flagsg & 0xFEu) | (value ? 1u : 0u));
		}
	}

	internal Copts(DOPDescriptor dopBase)
	{
		m_dopBase = dopBase;
	}

	internal void Parse(Stream stream)
	{
		Copts80.Parse(stream);
		m_flagsA = BaseWordRecord.ReadUInt32(stream);
		m_flagsg = (byte)stream.ReadByte();
		stream.ReadByte();
		BaseWordRecord.ReadUInt16(stream);
		BaseWordRecord.ReadUInt32(stream);
		BaseWordRecord.ReadUInt32(stream);
		BaseWordRecord.ReadUInt32(stream);
		BaseWordRecord.ReadUInt32(stream);
		BaseWordRecord.ReadUInt32(stream);
	}

	internal void Write(Stream stream)
	{
		Copts80.Write(stream);
		UseNormalStyleForList = true;
		DontUseIndentAsNumberingTabStop = true;
		FELineBreak11 = true;
		AllowSpaceOfSameStyleInTable = true;
		WW11IndentRules = true;
		DontAutofitConstrainedTables = true;
		AutofitLikeWW11 = true;
		HangulWidthLikeWW11 = true;
		SplitPgBreakAndParaMark = true;
		DontVertAlignCellWithSp = true;
		DontBreakConstrainedForcedTables = true;
		DontVertAlignInTxbx = true;
		Word11KerningPairs = true;
		BaseWordRecord.WriteUInt32(stream, m_flagsA);
		CachedColBalance = true;
		stream.WriteByte(m_flagsg);
		stream.WriteByte(0);
		BaseWordRecord.WriteUInt16(stream, 0);
		BaseWordRecord.WriteUInt32(stream, 0u);
		BaseWordRecord.WriteUInt32(stream, 0u);
		BaseWordRecord.WriteUInt32(stream, 0u);
		BaseWordRecord.WriteUInt32(stream, 0u);
		BaseWordRecord.WriteUInt32(stream, 0u);
	}
}
