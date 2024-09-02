using System;

namespace DocGen.DocIO.ReaderWriter.Biff_Records;

[CLSCompliant(false)]
internal class UniversalPropertyException : BaseWordRecord
{
	private byte[] m_arrData;

	internal byte[] Data => m_arrData;

	internal override int Length => m_arrData.Length;

	internal UniversalPropertyException()
	{
	}

	internal UniversalPropertyException(byte[] arrData, int iOffset, int iCount)
		: base(arrData, iOffset, iCount)
	{
	}

	internal override void Close()
	{
		base.Close();
		m_arrData = null;
	}

	internal override void Parse(byte[] arrData, int iOffset, int iCount)
	{
		if (arrData == null)
		{
			throw new ArgumentNullException("arrData");
		}
		if (iOffset < 0 || iOffset > arrData.Length)
		{
			throw new ArgumentOutOfRangeException("iOffset", "Value can not be less 0 and greater arrData.Length");
		}
		if (iCount < 0 || iCount + iOffset > arrData.Length)
		{
			throw new ArgumentOutOfRangeException("iCount");
		}
		if (m_arrData == null || m_arrData.Length != iCount)
		{
			m_arrData = new byte[iCount];
		}
		Array.Copy(arrData, iOffset, m_arrData, 0, iCount);
	}
}
