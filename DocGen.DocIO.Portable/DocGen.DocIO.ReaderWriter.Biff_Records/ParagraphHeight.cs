using System;
using System.IO;
using DocGen.DocIO.ReaderWriter.Biff_Records.Structures;

namespace DocGen.DocIO.ReaderWriter.Biff_Records;

[CLSCompliant(false)]
internal class ParagraphHeight : BaseWordRecord
{
	private const int DEF_BIT_SPARE = 0;

	private const int DEF_BIT_VALID = 1;

	private const int DEF_BIT_DIFF_LINES = 2;

	private const int DEF_BYTE_LINES_COUNT = 1;

	private const int DEF_MASK_NEXT_ROW_HINT = 65532;

	private const int DEF_START_NEXT_ROW_HINT = 2;

	private const int DEF_RECORD_SIZE = 13;

	private ParagraphHeightStructure m_structure = new ParagraphHeightStructure();

	internal ParagraphHeightStructure Structure => m_structure;

	internal bool IsSpare
	{
		get
		{
			return BaseWordRecord.GetBit(m_structure.Options, 0);
		}
		set
		{
			m_structure.Options = BaseWordRecord.SetBit(m_structure.Options, 0, value);
		}
	}

	internal bool IsValid
	{
		get
		{
			return !BaseWordRecord.GetBit(m_structure.Options, 1);
		}
		set
		{
			m_structure.Options = BaseWordRecord.SetBit(m_structure.Options, 1, !value);
		}
	}

	internal bool IsDifferentLines
	{
		get
		{
			return BaseWordRecord.GetBit(m_structure.Options, 2);
		}
		set
		{
			m_structure.Options = BaseWordRecord.SetBit(m_structure.Options, 2, value);
		}
	}

	internal byte LinesCount
	{
		get
		{
			return BitConverter.GetBytes(m_structure.Options)[1];
		}
		set
		{
			byte[] bytes = BitConverter.GetBytes(m_structure.Options);
			bytes[1] = value;
			m_structure.Options = BitConverter.ToUInt32(bytes, 0);
		}
	}

	internal int Width
	{
		get
		{
			return m_structure.Width;
		}
		set
		{
			m_structure.Width = value;
		}
	}

	internal int Height
	{
		get
		{
			return m_structure.Height;
		}
		set
		{
			m_structure.Height = value;
		}
	}

	internal int NextRowHint
	{
		get
		{
			return (int)BaseWordRecord.GetBitsByMask(m_structure.Options, 65532, 2);
		}
		set
		{
			m_structure.Options = BaseWordRecord.SetBitsByMask(m_structure.Options, 65532, value << 2);
		}
	}

	protected override IDataStructure UnderlyingStructure => m_structure;

	internal override int Length => m_structure.Length;

	internal ParagraphHeight()
	{
	}

	internal ParagraphHeight(byte[] arrData)
		: base(arrData)
	{
	}

	internal ParagraphHeight(Stream stream)
	{
		byte[] array = new byte[13];
		stream.Read(array, 0, 13);
		Parse(array);
	}

	internal void Parse(byte[] arrData, int iOffset)
	{
		m_structure.Parse(arrData, iOffset);
	}

	internal new int Save(byte[] arrData, int iOffset)
	{
		return m_structure.Save(arrData, iOffset);
	}
}
