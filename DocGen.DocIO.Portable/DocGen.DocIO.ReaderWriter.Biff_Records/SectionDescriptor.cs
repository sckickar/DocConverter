using System;
using DocGen.DocIO.ReaderWriter.Biff_Records.Structures;

namespace DocGen.DocIO.ReaderWriter.Biff_Records;

[CLSCompliant(false)]
internal class SectionDescriptor : BaseWordRecord
{
	private SectionDescriptorStructure m_structure = new SectionDescriptorStructure();

	internal const int DEF_RECORD_SIZE = 12;

	internal short Internal1
	{
		get
		{
			return m_structure.Internal1;
		}
		set
		{
			m_structure.Internal1 = value;
		}
	}

	internal short Internal2
	{
		get
		{
			return m_structure.Internal2;
		}
		set
		{
			m_structure.Internal2 = value;
		}
	}

	internal uint SepxPosition
	{
		get
		{
			return m_structure.SepxPosition;
		}
		set
		{
			m_structure.SepxPosition = value;
		}
	}

	internal int MacPrintOffset
	{
		get
		{
			return m_structure.MacPrintOffset;
		}
		set
		{
			m_structure.MacPrintOffset = value;
		}
	}

	protected override IDataStructure UnderlyingStructure => m_structure;

	internal override int Length => 12;

	internal SectionDescriptor()
	{
	}

	internal SectionDescriptor(byte[] arrData)
		: base(arrData)
	{
	}

	internal SectionDescriptor(byte[] arrData, int iOffset)
		: base(arrData, iOffset)
	{
	}

	internal override void Parse(byte[] arrData, int iOffset, int iCount)
	{
		m_structure.Parse(arrData, iOffset);
	}
}
