using System;
using System.IO;

namespace DocGen.DocIO.ReaderWriter.Biff_Records;

[CLSCompliant(false)]
internal class BreakDescriptorTable : BaseWordRecord
{
	private uint[] m_arrFC;

	private BreakDescriptorRecord[] m_arrEntry;

	internal uint[] FileCharacterPos => m_arrFC;

	internal BreakDescriptorRecord[] Entries => m_arrEntry;

	internal override int Length
	{
		get
		{
			int num = 0;
			if (m_arrFC != null)
			{
				num += m_arrFC.Length + 4;
			}
			if (m_arrEntry != null)
			{
				num += 6 * m_arrEntry.Length;
			}
			return num;
		}
	}

	internal BreakDescriptorTable()
	{
	}

	internal BreakDescriptorTable(byte[] data)
		: base(data)
	{
	}

	internal BreakDescriptorTable(byte[] arrData, int iOffset)
		: base(arrData, iOffset)
	{
	}

	internal BreakDescriptorTable(byte[] arrData, int iOffset, int iCount)
		: base(arrData, iOffset, iCount)
	{
	}

	internal BreakDescriptorTable(Stream stream, int iCount)
		: base(stream, iCount)
	{
	}

	internal override void Parse(byte[] arrData, int iOffset, int iCount)
	{
		if (arrData == null)
		{
			throw new ArgumentNullException("arrData");
		}
		if (iOffset < 0 || iOffset > arrData.Length - 1)
		{
			throw new ArgumentOutOfRangeException("iOffset", "Value can not be less than 0 and greater than arrData.Length - 1");
		}
		if (iCount < 0 || iCount + iOffset > arrData.Length)
		{
			throw new ArgumentOutOfRangeException("iCount");
		}
		m_arrEntry = null;
		m_arrFC = null;
		if (iCount != 0)
		{
			int num = (iCount - 4) / 10;
			m_arrFC = new uint[num + 1];
			m_arrEntry = new BreakDescriptorRecord[num];
			int num2 = (num + 1) * 4;
			Buffer.BlockCopy(arrData, 0, m_arrFC, 0, num2);
			iOffset += num2;
			int num3 = 0;
			while (num3 < num)
			{
				m_arrEntry[num3] = new BreakDescriptorRecord(arrData, iOffset);
				num3++;
				iOffset += 6;
			}
		}
	}

	internal override void Parse(Stream stream, int iCount)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		if (iCount < 0)
		{
			throw new ArgumentOutOfRangeException("iCount");
		}
		m_arrEntry = null;
		m_arrFC = null;
		if (iCount != 0)
		{
			int num = (iCount - 4) / 10;
			m_arrFC = new uint[num + 1];
			m_arrEntry = new BreakDescriptorRecord[num];
			int num2 = (num + 1) * 4;
			byte[] array = new byte[num2];
			stream.Read(array, 0, num2);
			Buffer.BlockCopy(array, 0, m_arrFC, 0, num2);
			if (num2 < 6)
			{
				array = new byte[6];
			}
			for (int i = 0; i < num; i++)
			{
				stream.Read(array, 0, 6);
				m_arrEntry[i] = new BreakDescriptorRecord(array, 0, 6);
			}
		}
	}
}
