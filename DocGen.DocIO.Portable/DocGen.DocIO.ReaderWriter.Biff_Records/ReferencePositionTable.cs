using System;
using System.IO;

namespace DocGen.DocIO.ReaderWriter.Biff_Records;

[CLSCompliant(false)]
internal class ReferencePositionTable : BaseWordRecord
{
	private int[] m_arrPositions;

	private ushort[] m_arrNumbers;

	internal int[] Positions => m_arrPositions;

	internal ushort[] Numbers => m_arrNumbers;

	internal override int Length => m_arrNumbers.Length * 2 + m_arrPositions.Length * 4;

	internal ReferencePositionTable()
	{
	}

	internal ReferencePositionTable(byte[] data)
		: base(data)
	{
	}

	internal ReferencePositionTable(byte[] arrData, int iOffset)
		: base(arrData, iOffset)
	{
	}

	internal ReferencePositionTable(byte[] arrData, int iOffset, int iCount)
		: base(arrData, iOffset, iCount)
	{
	}

	internal ReferencePositionTable(Stream stream, int iCount)
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
		if (iCount < 0 || iOffset + iCount > arrData.Length)
		{
			throw new ArgumentOutOfRangeException("iOffset");
		}
		if (iCount == 0)
		{
			m_arrNumbers = null;
			m_arrPositions = null;
			return;
		}
		int num = (iCount - 4) / 6;
		m_arrPositions = new int[num + 1];
		m_arrNumbers = new ushort[num];
		int num2 = (num + 1) * 4;
		Buffer.BlockCopy(arrData, iOffset, m_arrPositions, 0, num2);
		iOffset += num2;
		Buffer.BlockCopy(arrData, iOffset, m_arrNumbers, 0, num * 2);
	}
}
