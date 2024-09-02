using System;

namespace DocGen.DocIO.ReaderWriter.Biff_Records;

[CLSCompliant(false)]
internal class PieceTable : BaseWordRecord
{
	private uint[] m_arrFC;

	private PieceDescriptorRecord[] m_arrEntry;

	internal uint[] FileCharacterPos => m_arrFC;

	internal PieceDescriptorRecord[] Entries => m_arrEntry;

	internal override int Length
	{
		get
		{
			int num = m_arrFC.Length * 4 + 1 + 4;
			int i = 0;
			for (int num2 = m_arrEntry.Length; i < num2; i++)
			{
				num += m_arrEntry[i].Length;
			}
			return num;
		}
	}

	internal int EntriesCount
	{
		get
		{
			if (m_arrEntry == null)
			{
				return 0;
			}
			return m_arrEntry.Length;
		}
		set
		{
			if (value < 0)
			{
				throw new ArgumentOutOfRangeException("EntriesCount");
			}
			if (value != EntriesCount)
			{
				m_arrEntry = new PieceDescriptorRecord[value];
				m_arrFC = new uint[value + 1];
				for (int i = 0; i < value; i++)
				{
					m_arrEntry[i] = new PieceDescriptorRecord();
				}
			}
		}
	}

	internal PieceTable()
	{
	}

	internal PieceTable(byte[] arrData)
		: base(arrData)
	{
	}

	internal override void Close()
	{
		base.Close();
		m_arrFC = null;
		m_arrEntry = null;
	}

	internal override void Parse(byte[] arrData)
	{
		int num = (arrData.Length - 4) / 12;
		m_arrFC = new uint[num + 1];
		m_arrEntry = new PieceDescriptorRecord[num];
		int num2 = (num + 1) * 4;
		Buffer.BlockCopy(arrData, 0, m_arrFC, 0, num2);
		int num3 = num2;
		int num4 = 0;
		while (num4 < num)
		{
			m_arrEntry[num4] = new PieceDescriptorRecord();
			m_arrEntry[num4].ParseBytes(arrData, num3);
			num4++;
			num3 += 8;
		}
	}

	internal override int Save(byte[] arrData, int iOffset)
	{
		if (arrData == null)
		{
			throw new ArgumentNullException("arrData");
		}
		int length = Length;
		if (iOffset < 0 || iOffset + length > arrData.Length)
		{
			throw new ArgumentOutOfRangeException("iOffset");
		}
		int num = iOffset;
		arrData[iOffset++] = 2;
		BitConverter.GetBytes(length - 1 - 4).CopyTo(arrData, iOffset);
		iOffset += 4;
		if (EntriesCount > 0)
		{
			int num2 = m_arrFC.Length * 4;
			Buffer.BlockCopy(m_arrFC, 0, arrData, iOffset, num2);
			iOffset += num2;
			int i = 0;
			for (int entriesCount = EntriesCount; i < entriesCount; i++)
			{
				iOffset += m_arrEntry[i].Save(arrData, iOffset);
			}
		}
		return iOffset - num;
	}
}
