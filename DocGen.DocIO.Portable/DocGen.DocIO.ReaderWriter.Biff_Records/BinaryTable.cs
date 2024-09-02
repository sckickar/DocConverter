using System;
using System.IO;
using DocGen.DocIO.ReaderWriter.Biff_Records.Structures;

namespace DocGen.DocIO.ReaderWriter.Biff_Records;

[CLSCompliant(false)]
internal class BinaryTable : BaseWordRecord
{
	private uint[] m_arrFC;

	private BinTableEntry[] m_arrEntry;

	internal uint[] FileCharacterPos => m_arrFC;

	internal BinTableEntry[] Entries => m_arrEntry;

	internal int EntriesCount
	{
		get
		{
			return m_arrEntry.Length;
		}
		set
		{
			if (value < 0)
			{
				throw new ArgumentOutOfRangeException("Length");
			}
			m_arrFC = new uint[value + 1];
			m_arrEntry = new BinTableEntry[value];
		}
	}

	internal override int Length
	{
		get
		{
			int num = 4 * m_arrFC.Length;
			int num2 = m_arrEntry.Length * 4;
			return num + num2;
		}
	}

	internal BinaryTable()
	{
	}

	internal BinaryTable(byte[] arrData)
		: base(arrData)
	{
	}

	internal BinaryTable(Stream stream, int iCount)
		: base(stream, iCount)
	{
	}

	internal override void Close()
	{
		base.Close();
		if (m_arrFC != null)
		{
			m_arrFC = null;
		}
		if (m_arrEntry != null)
		{
			m_arrEntry = null;
		}
	}

	internal override void Parse(byte[] arrData, int iOffset, int iLength)
	{
		if (arrData == null)
		{
			throw new ArgumentNullException("arrData");
		}
		if (iOffset != 0)
		{
			throw new ArgumentOutOfRangeException("iOffset", "Value cannot be less  and greater ");
		}
		if (iLength != arrData.Length)
		{
			throw new ArgumentOutOfRangeException("iLength", "Value cannot be less  and greater ");
		}
		int num = (iLength - 4) / 8;
		m_arrFC = new uint[num + 1];
		m_arrEntry = new BinTableEntry[num];
		int num2 = (num + 1) * 4;
		Buffer.BlockCopy(arrData, 0, m_arrFC, 0, num2);
		iOffset = num2;
		for (int i = 0; i < num; i++)
		{
			m_arrEntry[i] = new BinTableEntry();
			iOffset = m_arrEntry[i].Parse(arrData, iOffset);
		}
	}

	internal override int Save(byte[] arrData, int iOffset)
	{
		if (arrData == null)
		{
			throw new ArgumentNullException("arrData");
		}
		if (iOffset < 0 || iOffset > arrData.Length)
		{
			throw new ArgumentOutOfRangeException("iOffset");
		}
		int num = 4 * m_arrFC.Length;
		int num2 = m_arrEntry.Length * 4;
		int num3 = num + num2;
		if (iOffset + num3 > arrData.Length)
		{
			throw new ArgumentOutOfRangeException("arrData.Length");
		}
		Buffer.BlockCopy(m_arrFC, 0, arrData, iOffset, num);
		iOffset += num;
		int i = 0;
		for (int num4 = m_arrEntry.Length; i < num4; i++)
		{
			m_arrEntry[i].Save(arrData, iOffset);
			iOffset += 4;
		}
		return num3;
	}
}
