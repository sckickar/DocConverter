using System;
using System.IO;

namespace DocGen.DocIO.ReaderWriter.Biff_Records;

[CLSCompliant(false)]
internal class ParagraphPropertyException : BaseWordRecord
{
	protected ushort m_usStyleId;

	protected SinglePropertyModifierArray m_arrSprms = new SinglePropertyModifierArray();

	private byte m_bFlags;

	private bool IsHugePapx
	{
		get
		{
			return (m_bFlags & 1) != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xFEu) | (value ? 1u : 0u));
		}
	}

	internal ushort StyleIndex
	{
		get
		{
			return m_usStyleId;
		}
		set
		{
			m_usStyleId = value;
		}
	}

	internal SinglePropertyModifierArray PropertyModifiers
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

	internal int ModifiersCount => m_arrSprms.Count;

	internal override int Length => 2 + m_arrSprms.Length;

	internal ushort ParagraphStyleId
	{
		get
		{
			return m_arrSprms.GetUShort(17920, 0);
		}
		set
		{
			if (ParagraphStyleId != value)
			{
				m_arrSprms.SetUShortValue(17920, value);
			}
		}
	}

	internal ParagraphPropertyException()
	{
	}

	internal ParagraphPropertyException(Stream stream, int iCount, bool isHugePapx)
	{
		IsHugePapx = isHugePapx;
		Parse(stream, iCount);
	}

	internal ParagraphPropertyException(UniversalPropertyException property)
	{
		byte[] data = property.Data;
		Parse(data, 0, data.Length);
	}

	internal override void Close()
	{
		base.Close();
		if (m_arrSprms != null)
		{
			m_arrSprms = null;
		}
	}

	internal override void Parse(byte[] arrData, int iOffset, int iCount)
	{
		if (arrData == null)
		{
			throw new ArgumentNullException("arrData");
		}
		if (iOffset < 0 || iOffset > arrData.Length)
		{
			throw new ArgumentOutOfRangeException("iOffset");
		}
		if (iCount < 2)
		{
			throw new ArgumentOutOfRangeException("iCount");
		}
		if (iCount + iOffset > arrData.Length)
		{
			throw new ArgumentOutOfRangeException("iCount + iOffset");
		}
		if (!IsHugePapx)
		{
			m_usStyleId = BitConverter.ToUInt16(arrData, 0);
			iOffset += 2;
			iCount -= 2;
		}
		m_arrSprms.Parse(arrData, iOffset, iCount);
	}

	internal override int Save(Stream stream)
	{
		int num = 2;
		stream.Write(BitConverter.GetBytes(m_usStyleId), 0, 2);
		if (m_arrSprms != null)
		{
			num += m_arrSprms.Save(stream);
		}
		return num;
	}

	internal int SaveHugePapx(Stream stream)
	{
		int num = 0;
		if (m_arrSprms != null)
		{
			num += m_arrSprms.Save(stream);
		}
		return num;
	}

	internal ParagraphPropertyException ClonePapx(bool stickProperties, ParagraphPropertyException papx)
	{
		ParagraphPropertyException ex = new ParagraphPropertyException();
		ex.PropertyModifiers = papx.PropertyModifiers.Clone();
		ex.m_usStyleId = papx.StyleIndex;
		papx = new ParagraphPropertyException();
		if (stickProperties)
		{
			int i = 0;
			for (int modifiersCount = ex.ModifiersCount; i < modifiersCount; i++)
			{
				SinglePropertyModifierRecord sprmByIndex = ex.PropertyModifiers.GetSprmByIndex(i);
				if (sprmByIndex.TypedOptions != 54792 && sprmByIndex.TypedOptions != 54789 && sprmByIndex.TypedOptions != 38402 && sprmByIndex.TypedOptions != 38401 && sprmByIndex.TypedOptions != 37895 && sprmByIndex.TypedOptions != 9239 && sprmByIndex.TypedOptions != 9291 && sprmByIndex.TypedOptions != 9292)
				{
					SinglePropertyModifierRecord singlePropertyModifierRecord = ex.PropertyModifiers.GetSprmByIndex(i).Clone();
					if (singlePropertyModifierRecord != null)
					{
						papx.PropertyModifiers.Add(singlePropertyModifierRecord);
					}
				}
			}
		}
		return ex;
	}
}
