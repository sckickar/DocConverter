using System;

namespace DocGen.OfficeChart.Parser.Biff_Records;

internal class BytesList
{
	private const int DEF_CAPACITY_STEP = 512;

	private const int DEF_DEFAULT_CAPACITY = 512;

	private const int DEF_RECORD_SIZE = 20;

	private byte[] m_arrBuffer;

	private int m_iCurPos;

	private bool m_bExactSize = true;

	internal byte[] InnerBuffer => m_arrBuffer;

	public int Count => m_iCurPos;

	public BytesList()
		: this(512)
	{
	}

	public BytesList(bool bExactSize)
	{
		m_bExactSize = bExactSize;
	}

	public BytesList(int iCapacity)
	{
		EnsureFreeSpace(iCapacity);
	}

	public BytesList(byte[] arrData)
	{
		if (arrData == null)
		{
			throw new ArgumentNullException("arrData");
		}
		EnsureFreeSpace(arrData.Length);
		AddRange(arrData);
	}

	public void Add(byte bToAdd)
	{
		EnsureFreeSpace(1);
		m_arrBuffer[m_iCurPos] = bToAdd;
		m_iCurPos++;
	}

	public void AddRange(byte[] arrToAdd)
	{
		if (arrToAdd == null)
		{
			throw new ArgumentNullException("arrToAdd");
		}
		int num = arrToAdd.Length;
		if (num > 0)
		{
			EnsureFreeSpace(num);
			Buffer.BlockCopy(arrToAdd, 0, m_arrBuffer, m_iCurPos, num);
			m_iCurPos += num;
		}
	}

	public void AddRange(BytesList list)
	{
		if (list == null)
		{
			throw new ArgumentNullException("list");
		}
		int count = list.Count;
		if (count > 0)
		{
			EnsureFreeSpace(count);
			Buffer.BlockCopy(list.m_arrBuffer, 0, m_arrBuffer, m_iCurPos, count);
			m_iCurPos += count;
		}
	}

	public void CopyTo(int iStartIndex, byte[] arrDest, int iDestIndex, int iCount)
	{
		if (arrDest == null)
		{
			throw new ArgumentNullException("arrDest");
		}
		int num = arrDest.Length;
		if (iStartIndex < 0 || iStartIndex + iCount > m_iCurPos)
		{
			throw new ArgumentOutOfRangeException("iStartIndex");
		}
		if (iDestIndex < 0 || iDestIndex + iCount > num)
		{
			throw new ArgumentOutOfRangeException("iDestIndex");
		}
		Buffer.BlockCopy(m_arrBuffer, iStartIndex, arrDest, iDestIndex, iCount);
	}

	public void EnsureFreeSpace(int iSize)
	{
		int num = ((m_arrBuffer != null) ? m_arrBuffer.Length : 0);
		int num2 = m_iCurPos + iSize;
		if (num >= num2)
		{
			return;
		}
		int num3 = num2;
		if (!m_bExactSize)
		{
			num3 = ((num == 0) ? 20 : (num * 2));
			if (num3 < num2)
			{
				num3 = num2;
			}
		}
		byte[] array = new byte[num3];
		if (num > 0 && m_arrBuffer != null)
		{
			Buffer.BlockCopy(m_arrBuffer, 0, array, 0, num);
		}
		m_arrBuffer = array;
	}
}
