using System;

namespace DocGen.DocIO.ReaderWriter.Biff_Records.Structures;

[CLSCompliant(false)]
internal class StyleSheetInfoStructure : IDataStructure
{
	private const int DEF_BIT_STYLE_NAMES_WRITTEN = 0;

	private const int DEF_RECORD_SIZE = 20;

	private ushort m_usStylesCount;

	private ushort m_usSTDBaseLength;

	private ushort m_usOptions = 1;

	private ushort m_usStiMaxWhenSaved;

	private ushort m_usISTDMaxFixedWhenSaved;

	private ushort m_usBuiltInNamesVersion = 4;

	private ushort[] m_arrStandardChpStsh = new ushort[3];

	private ushort m_ftcBi;

	internal ushort StylesCount
	{
		get
		{
			return m_usStylesCount;
		}
		set
		{
			m_usStylesCount = value;
		}
	}

	internal ushort STDBaseLength
	{
		get
		{
			return m_usSTDBaseLength;
		}
		set
		{
			m_usSTDBaseLength = value;
		}
	}

	internal bool IsStdStyleNamesWritten
	{
		get
		{
			return BaseWordRecord.GetBit(m_usOptions, 0);
		}
		set
		{
			m_usOptions = (ushort)BaseWordRecord.SetBit(m_usOptions, 0, value);
		}
	}

	internal ushort StiMaxWhenSaved
	{
		get
		{
			return m_usStiMaxWhenSaved;
		}
		set
		{
			m_usStiMaxWhenSaved = value;
		}
	}

	internal ushort ISTDMaxFixedWhenSaved
	{
		get
		{
			return m_usISTDMaxFixedWhenSaved;
		}
		set
		{
			m_usISTDMaxFixedWhenSaved = value;
		}
	}

	internal ushort BuiltInNamesVersion
	{
		get
		{
			return m_usBuiltInNamesVersion;
		}
		set
		{
			m_usBuiltInNamesVersion = value;
		}
	}

	internal ushort[] StandardChpStsh
	{
		get
		{
			return m_arrStandardChpStsh;
		}
		set
		{
			if (value == null || value.Length != 3)
			{
				throw new ArgumentException("Trying to set wrong StandardChpStsh");
			}
			m_arrStandardChpStsh = value;
		}
	}

	internal ushort FtcBi
	{
		get
		{
			return m_ftcBi;
		}
		set
		{
			m_ftcBi = value;
		}
	}

	public int Length => 20;

	public void Parse(byte[] arrData, int iOffset)
	{
		m_usStylesCount = ByteConverter.ReadUInt16(arrData, ref iOffset);
		m_usSTDBaseLength = ByteConverter.ReadUInt16(arrData, ref iOffset);
		m_usOptions = ByteConverter.ReadUInt16(arrData, ref iOffset);
		m_usStiMaxWhenSaved = ByteConverter.ReadUInt16(arrData, ref iOffset);
		m_usISTDMaxFixedWhenSaved = ByteConverter.ReadUInt16(arrData, ref iOffset);
		m_usBuiltInNamesVersion = ByteConverter.ReadUInt16(arrData, ref iOffset);
		for (int i = 0; i < 3; i++)
		{
			m_arrStandardChpStsh[i] = ByteConverter.ReadUInt16(arrData, ref iOffset);
		}
		if (arrData.Length > iOffset + 1)
		{
			m_ftcBi = ByteConverter.ReadUInt16(arrData, ref iOffset);
		}
	}

	public int Save(byte[] arrData, int iOffset)
	{
		ByteConverter.WriteUInt16(arrData, ref iOffset, m_usStylesCount);
		ByteConverter.WriteUInt16(arrData, ref iOffset, m_usSTDBaseLength);
		ByteConverter.WriteUInt16(arrData, ref iOffset, m_usOptions);
		ByteConverter.WriteUInt16(arrData, ref iOffset, m_usStiMaxWhenSaved);
		ByteConverter.WriteUInt16(arrData, ref iOffset, m_usISTDMaxFixedWhenSaved);
		ByteConverter.WriteUInt16(arrData, ref iOffset, m_usBuiltInNamesVersion);
		for (int i = 0; i < 3; i++)
		{
			ByteConverter.WriteUInt16(arrData, ref iOffset, m_arrStandardChpStsh[i]);
		}
		ByteConverter.WriteUInt16(arrData, ref iOffset, m_ftcBi);
		return 20;
	}
}
