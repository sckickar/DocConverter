using System;

namespace DocGen.DocIO.ReaderWriter.Biff_Records.Structures;

internal class BinTableEntry
{
	public const int RECORD_SIZE = 4;

	private int m_iValue;

	internal int Value
	{
		get
		{
			return m_iValue;
		}
		set
		{
			m_iValue = value;
		}
	}

	internal int Parse(byte[] arrData, int iOffset)
	{
		if (arrData == null)
		{
			throw new ArgumentNullException("arrData");
		}
		if (iOffset < 0 || iOffset > arrData.Length)
		{
			throw new ArgumentOutOfRangeException("iOffset", "Value can not be less 0 and greater arrData.Length");
		}
		m_iValue = BitConverter.ToInt32(arrData, iOffset);
		return iOffset + 4;
	}

	internal void Save(byte[] arrData, int iOffset)
	{
		if (arrData == null)
		{
			throw new ArgumentNullException("arrData");
		}
		if (iOffset < 0 || iOffset > arrData.Length)
		{
			throw new ArgumentOutOfRangeException("iOffset");
		}
		if (iOffset + 4 > arrData.Length)
		{
			throw new ArgumentOutOfRangeException("arrData.Length");
		}
		BitConverter.GetBytes(m_iValue).CopyTo(arrData, iOffset);
	}
}
