using System;
using System.IO;
using DocGen.DocIO.ReaderWriter.Biff_Records.Structures;

namespace DocGen.DocIO.ReaderWriter.Biff_Records;

[CLSCompliant(false)]
internal class StyleSheetInfoRecord : BaseWordRecord
{
	private const int DEF_BIT_STYLE_NAMES_WRITTEN = 0;

	private StyleSheetInfoStructure m_structure = new StyleSheetInfoStructure();

	internal ushort StylesCount
	{
		get
		{
			return m_structure.StylesCount;
		}
		set
		{
			m_structure.StylesCount = value;
		}
	}

	internal ushort STDBaseLength
	{
		get
		{
			return m_structure.STDBaseLength;
		}
		set
		{
			m_structure.STDBaseLength = value;
		}
	}

	internal bool IsStdStyleNamesWritten
	{
		get
		{
			return m_structure.IsStdStyleNamesWritten;
		}
		set
		{
			m_structure.IsStdStyleNamesWritten = value;
		}
	}

	internal ushort StiMaxWhenSaved
	{
		get
		{
			return m_structure.StiMaxWhenSaved;
		}
		set
		{
			m_structure.StiMaxWhenSaved = value;
		}
	}

	internal ushort ISTDMaxFixedWhenSaved
	{
		get
		{
			return m_structure.ISTDMaxFixedWhenSaved;
		}
		set
		{
			m_structure.ISTDMaxFixedWhenSaved = value;
		}
	}

	internal ushort BuiltInNamesVersion
	{
		get
		{
			return m_structure.BuiltInNamesVersion;
		}
		set
		{
			m_structure.BuiltInNamesVersion = value;
		}
	}

	internal ushort[] StandardChpStsh
	{
		get
		{
			return m_structure.StandardChpStsh;
		}
		set
		{
			m_structure.StandardChpStsh = value;
		}
	}

	protected override IDataStructure UnderlyingStructure => m_structure;

	internal override int Length => m_structure.Length;

	internal ushort FtcBi
	{
		get
		{
			return m_structure.FtcBi;
		}
		set
		{
			m_structure.FtcBi = value;
		}
	}

	internal StyleSheetInfoRecord(ushort iSTDBaseLength)
	{
		STDBaseLength = iSTDBaseLength;
		StiMaxWhenSaved = 91;
		ISTDMaxFixedWhenSaved = 15;
		BuiltInNamesVersion = 0;
		IsStdStyleNamesWritten = true;
	}

	internal StyleSheetInfoRecord(byte[] arrData)
		: base(arrData)
	{
	}

	internal StyleSheetInfoRecord(byte[] arrData, int iOffset)
		: base(arrData, iOffset)
	{
	}

	internal StyleSheetInfoRecord(byte[] arrData, int iOffset, int iCount)
		: base(arrData, iOffset, iCount)
	{
	}

	internal StyleSheetInfoRecord(Stream stream, int iCount)
		: base(stream, iCount)
	{
	}

	internal override void Parse(byte[] arrData, int iOffset, int iCount)
	{
		base.Parse(arrData, iOffset, iCount);
	}

	internal override void Close()
	{
		base.Close();
		m_structure = null;
	}
}
