using System;
using System.IO;

namespace DocGen.DocIO.ReaderWriter.Biff_Records;

[CLSCompliant(false)]
internal class SectionExceptionsTable : BaseWordRecord
{
	private int[] m_arrPositions;

	private SectionDescriptor[] m_arrDescriptors;

	internal int[] Positions => m_arrPositions;

	internal SectionDescriptor[] Descriptors => m_arrDescriptors;

	internal override int Length
	{
		get
		{
			int num = m_arrPositions.Length * 4;
			int i = 0;
			for (int num2 = m_arrDescriptors.Length; i < num2; i++)
			{
				num += m_arrDescriptors[i].Length;
			}
			return num;
		}
	}

	internal int EntriesCount
	{
		get
		{
			if (m_arrDescriptors != null)
			{
				return m_arrDescriptors.Length;
			}
			return 0;
		}
		set
		{
			if (value < 0)
			{
				throw new ArgumentOutOfRangeException("EntriesCount");
			}
			if (value != EntriesCount)
			{
				m_arrDescriptors = new SectionDescriptor[value];
				m_arrPositions = new int[value + 1];
				for (int i = 0; i < value; i++)
				{
					m_arrDescriptors[i] = new SectionDescriptor();
				}
			}
		}
	}

	internal SectionExceptionsTable()
	{
	}

	internal SectionExceptionsTable(byte[] arrData)
		: base(arrData)
	{
	}

	internal SectionExceptionsTable(Stream stream, int iCount)
		: base(stream, iCount)
	{
	}

	internal override void Close()
	{
		base.Close();
		m_arrPositions = null;
		m_arrDescriptors = null;
	}

	internal override void Parse(byte[] data, int iOffset, int iCount)
	{
		if (data == null)
		{
			throw new ArgumentNullException("arrData");
		}
		if (iOffset != 0)
		{
			throw new ArgumentOutOfRangeException("iOffset");
		}
		if (iCount < 0)
		{
			throw new ArgumentOutOfRangeException("iCount");
		}
		if (iOffset + iCount > data.Length)
		{
			throw new ArgumentOutOfRangeException("iOffset + iCount");
		}
		int num = data.Length;
		int num2 = 16;
		int num3 = num / num2;
		m_arrPositions = new int[num3 + 1];
		m_arrDescriptors = new SectionDescriptor[num3];
		iOffset = (num3 + 1) * 4;
		Buffer.BlockCopy(data, 0, m_arrPositions, 0, iOffset);
		int num4 = 0;
		while (num4 < num3)
		{
			m_arrDescriptors[num4] = new SectionDescriptor(data, iOffset);
			num4++;
			iOffset += 12;
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
		int num2 = m_arrPositions.Length * 4;
		Buffer.BlockCopy(m_arrPositions, 0, arrData, iOffset, num2);
		iOffset += num2;
		int i = 0;
		for (int num3 = m_arrDescriptors.Length; i < num3; i++)
		{
			m_arrDescriptors[i].Save(arrData, iOffset);
			iOffset += m_arrDescriptors[i].Length;
		}
		return iOffset - num;
	}
}
