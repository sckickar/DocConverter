using System;
using System.IO;

namespace DocGen.DocIO.ReaderWriter.Biff_Records;

[CLSCompliant(false)]
internal class CharPosTableRecord : BaseWordRecord
{
	private int[] m_arrPositions;

	internal int[] Positions
	{
		get
		{
			return m_arrPositions;
		}
		set
		{
			m_arrPositions = value;
		}
	}

	internal override int Length => m_arrPositions.Length * 4;

	internal CharPosTableRecord()
	{
	}

	internal CharPosTableRecord(byte[] data)
		: base(data)
	{
	}

	internal CharPosTableRecord(byte[] arrData, int iOffset)
		: base(arrData, iOffset)
	{
	}

	internal CharPosTableRecord(byte[] arrData, int iOffset, int iCount)
		: base(arrData, iOffset, iCount)
	{
	}

	internal CharPosTableRecord(Stream stream, int iCount)
		: base(stream, iCount)
	{
	}

	internal string GetTextChunk(string text, int position)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		if (text.Length == 0)
		{
			throw new ArgumentException("text - string can not be empty");
		}
		int num = Positions.Length - 1;
		if (position < 0 || position > num)
		{
			throw new ArgumentOutOfRangeException("position", "Value can not be less 0 and greater " + num);
		}
		int num2 = Positions[position];
		int num3 = 0;
		num3 = ((position + 1 >= Positions.Length) ? (text.Length - num2) : (Positions[position + 1] - num2));
		return text.Substring(num2, num3);
	}

	internal override void Close()
	{
		base.Close();
		m_arrPositions = null;
	}

	internal override void Parse(byte[] arrData, int iOffset, int iCount)
	{
		if (arrData == null)
		{
			throw new ArgumentNullException("arrData");
		}
		if (iOffset < 0 || iOffset > arrData.Length - 1)
		{
			throw new ArgumentOutOfRangeException("iOffset", "Value can not be less 0 and greater arrData.Length - 1");
		}
		if (iCount < 0 || iOffset + iCount > arrData.Length)
		{
			throw new ArgumentOutOfRangeException("iCount");
		}
		int num = iCount / 4;
		m_arrPositions = new int[num];
		Buffer.BlockCopy(arrData, 0, m_arrPositions, 0, num * 4);
	}

	internal override int Save(byte[] arrData, int iOffset)
	{
		if (arrData == null)
		{
			throw new ArgumentNullException("arrData");
		}
		if (iOffset < 0 || iOffset > arrData.Length - 1)
		{
			throw new ArgumentOutOfRangeException("iOffset", "Value can not be less 0 and greater arrData.Length - 1");
		}
		Buffer.BlockCopy(m_arrPositions, 0, arrData, 0, Length);
		return arrData.Length;
	}
}
