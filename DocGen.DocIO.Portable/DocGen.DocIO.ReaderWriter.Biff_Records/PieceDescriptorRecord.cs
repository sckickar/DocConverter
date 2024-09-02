using System;
using DocGen.DocIO.ReaderWriter.Biff_Records.Structures;

namespace DocGen.DocIO.ReaderWriter.Biff_Records;

[CLSCompliant(false)]
internal class PieceDescriptorRecord : BaseWordRecord
{
	internal const int RECORD_SIZE = 8;

	private PieceDescriptorStructure m_field = new PieceDescriptorStructure();

	internal bool fNoParaLast
	{
		get
		{
			return BaseWordRecord.GetBit(m_field.Options, 1);
		}
		set
		{
			m_field.Options = (ushort)BaseWordRecord.SetBit(m_field.Options, 1, value);
		}
	}

	internal bool fPaphNil
	{
		get
		{
			return BaseWordRecord.GetBit(m_field.Options, 2);
		}
		set
		{
			m_field.Options = (ushort)BaseWordRecord.SetBit(m_field.Options, 2, value);
		}
	}

	internal bool fCopied
	{
		get
		{
			return BaseWordRecord.GetBit(m_field.Options, 4);
		}
		set
		{
			m_field.Options = (ushort)BaseWordRecord.SetBit(m_field.Options, 4, value);
		}
	}

	internal uint FileOffset
	{
		get
		{
			return m_field.FileOffset;
		}
		set
		{
			m_field.FileOffset = value;
		}
	}

	internal PropertyModifierStructure PropertyModifier
	{
		get
		{
			return m_field.PropertyModifier;
		}
		set
		{
			m_field.PropertyModifier = value;
		}
	}

	protected override IDataStructure UnderlyingStructure => m_field;

	internal override int Length => m_field.Length;

	internal PieceDescriptorRecord()
	{
	}

	internal PieceDescriptorRecord(byte[] arrData)
		: base(arrData)
	{
	}

	internal PieceDescriptorRecord(byte[] arrData, int iOffset)
		: base(arrData, iOffset)
	{
	}
}
