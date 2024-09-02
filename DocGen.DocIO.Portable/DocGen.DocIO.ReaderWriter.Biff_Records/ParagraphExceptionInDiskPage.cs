using System;
using System.IO;

namespace DocGen.DocIO.ReaderWriter.Biff_Records;

[CLSCompliant(false)]
internal class ParagraphExceptionInDiskPage : ParagraphPropertyException
{
	internal override int Length => ((!IsPad) ? 1 : 2) + 2 + m_arrSprms.Length;

	protected bool IsPad => m_arrSprms.Length % 2 == 0;

	internal ParagraphExceptionInDiskPage()
	{
	}

	internal ParagraphExceptionInDiskPage(ParagraphPropertyException papx)
	{
		m_usStyleId = papx.StyleIndex;
		m_arrSprms = papx.PropertyModifiers;
	}

	internal int Parse(byte[] arrData, int iOffset)
	{
		if (arrData == null)
		{
			throw new ArgumentNullException("arrData");
		}
		int num = arrData.Length;
		if (iOffset < 0 || iOffset >= num)
		{
			throw new ArgumentOutOfRangeException("iOffset");
		}
		byte b = arrData[iOffset];
		iOffset++;
		if (b == 0)
		{
			if (iOffset >= num)
			{
				throw new ArgumentOutOfRangeException("iOffset");
			}
			b = arrData[iOffset++];
		}
		int num2 = b * 2;
		if (iOffset + num2 > num)
		{
			throw new ArgumentOutOfRangeException("Data array is too short");
		}
		m_usStyleId = BitConverter.ToUInt16(arrData, iOffset);
		iOffset += 2;
		num2 -= 2;
		m_arrSprms.Parse(arrData, iOffset, num2);
		return iOffset;
	}

	internal override int Save(byte[] arrData, int iOffset)
	{
		int num = iOffset;
		int num2 = Length - ((!IsPad) ? 1 : 2);
		if (IsPad)
		{
			arrData[iOffset++] = 0;
		}
		byte b = (byte)(num2 / 2);
		arrData[iOffset++] = b;
		BitConverter.GetBytes(m_usStyleId).CopyTo(arrData, iOffset);
		iOffset += 2;
		iOffset += m_arrSprms.Save(arrData, iOffset);
		return iOffset - num;
	}

	internal void Save(BinaryWriter writer, Stream stream)
	{
		int num = Length - ((!IsPad) ? 1 : 2);
		if (IsPad)
		{
			writer.Write((byte)0);
		}
		byte b = (byte)(num / 2);
		if (!IsPad)
		{
			b++;
		}
		writer.Write(b);
		writer.Write(m_usStyleId);
		if (m_arrSprms != null)
		{
			m_arrSprms.Save(writer, stream);
		}
	}
}
