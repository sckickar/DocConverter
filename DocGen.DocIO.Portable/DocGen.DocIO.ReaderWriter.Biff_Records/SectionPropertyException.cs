using System;
using System.IO;

namespace DocGen.DocIO.ReaderWriter.Biff_Records;

[CLSCompliant(false)]
internal class SectionPropertyException : BaseWordRecord
{
	private SinglePropertyModifierArray m_arrSprms = new SinglePropertyModifierArray();

	internal SinglePropertyModifierArray Properties
	{
		get
		{
			return m_arrSprms;
		}
		set
		{
			m_arrSprms = value;
		}
	}

	internal int Count => m_arrSprms.Count;

	internal override int Length => 2 + m_arrSprms.Length;

	internal ushort HeaderHeight
	{
		get
		{
			return m_arrSprms.GetUShort(45079, 0);
		}
		set
		{
			if (value != 0)
			{
				m_arrSprms.SetUShortValue(45079, value);
			}
		}
	}

	internal ushort FooterHeight
	{
		get
		{
			return m_arrSprms.GetUShort(45080, 0);
		}
		set
		{
			if (value != 0)
			{
				m_arrSprms.SetUShortValue(45080, value);
			}
		}
	}

	internal bool IsTitlePage
	{
		get
		{
			return m_arrSprms.GetBoolean(12298, defValue: false);
		}
		set
		{
			if (value)
			{
				m_arrSprms.SetBoolValue(12298, flag: true);
			}
		}
	}

	internal byte BreakCode
	{
		get
		{
			return m_arrSprms.GetByte(12297, 2);
		}
		set
		{
			if (value != 2)
			{
				m_arrSprms.SetByteValue(12297, value);
			}
		}
	}

	internal int ColumnsCount
	{
		get
		{
			return m_arrSprms.GetUShort(20491, 0) + 1;
		}
		set
		{
			if (value < 1)
			{
				throw new ArgumentOutOfRangeException();
			}
			m_arrSprms.SetUShortValue(20491, (ushort)(value - 1));
		}
	}

	internal SectionPropertyException()
	{
	}

	internal SectionPropertyException(bool isDefaultSEP)
	{
		if (isDefaultSEP)
		{
			m_arrSprms.SetUShortValue(45071, 720);
			m_arrSprms.SetUShortValue(45072, 720);
			m_arrSprms.SetBoolValue(12306, flag: true);
			m_arrSprms.SetBoolValue(12293, flag: true);
			m_arrSprms.SetUShortValue(45087, 12240);
			m_arrSprms.SetUShortValue(45088, 15840);
			m_arrSprms.SetUShortValue(45079, 720);
			m_arrSprms.SetUShortValue(45080, 720);
			m_arrSprms.SetBoolValue(12317, flag: true);
			m_arrSprms.SetUShortValue(36876, 720);
			m_arrSprms.SetUShortValue(36899, 1440);
			m_arrSprms.SetUShortValue(45089, 1440);
			m_arrSprms.SetUShortValue(36900, 1440);
			m_arrSprms.SetUShortValue(45090, 1440);
			m_arrSprms.SetUShortValue(20508, 1);
		}
	}

	internal SectionPropertyException(Stream stream)
	{
		Parse(stream);
	}

	internal override void Close()
	{
		base.Close();
		if (m_arrSprms != null)
		{
			m_arrSprms = null;
		}
	}

	private void Parse(Stream stream)
	{
		byte[] array = new byte[2];
		if (stream.Read(array, 0, 2) != 2)
		{
			throw new Exception("Was unable to read required bytes from the stream");
		}
		int num = (int)(BitConverter.ToUInt16(array, 0) + stream.Position);
		while (stream.Position < num)
		{
			SinglePropertyModifierRecord modifier = new SinglePropertyModifierRecord(stream);
			m_arrSprms.Add(modifier);
		}
	}

	internal override int Save(byte[] arrData, int iOffset)
	{
		ushort num = (ushort)m_arrSprms.Length;
		if (arrData == null)
		{
			throw new ArgumentNullException("arrData");
		}
		if (iOffset < 0 || iOffset + num > arrData.Length)
		{
			throw new ArgumentOutOfRangeException("iOffset");
		}
		int num2 = iOffset;
		BitConverter.GetBytes(num).CopyTo(arrData, iOffset);
		iOffset += 2;
		iOffset += m_arrSprms.Save(arrData, iOffset);
		return iOffset - num2;
	}
}
