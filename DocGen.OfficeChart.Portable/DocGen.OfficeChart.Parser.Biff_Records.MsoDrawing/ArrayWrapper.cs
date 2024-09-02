using System;

namespace DocGen.OfficeChart.Parser.Biff_Records.MsoDrawing;

internal class ArrayWrapper
{
	private byte[] m_arrBuffer;

	private int m_iHash;

	private ArrayWrapper()
	{
	}

	public ArrayWrapper(byte[] arrBuffer)
	{
		if (arrBuffer == null)
		{
			throw new ArgumentNullException("arrBuffer");
		}
		m_arrBuffer = arrBuffer;
		EvaluateHash();
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		byte[] array = null;
		if (obj is ArrayWrapper)
		{
			array = ((ArrayWrapper)obj).m_arrBuffer;
		}
		else if (obj is byte[])
		{
			array = (byte[])obj;
		}
		if (array == null)
		{
			return false;
		}
		return BiffRecordRaw.CompareArrays(m_arrBuffer, array);
	}

	public override int GetHashCode()
	{
		return m_iHash;
	}

	private void EvaluateHash()
	{
		int num = m_arrBuffer.Length / 4;
		m_iHash = 0;
		int num2 = 0;
		int num3 = 0;
		while (num2 < num)
		{
			m_iHash |= BitConverter.ToInt32(m_arrBuffer, num3);
			num2++;
			num3 += 4;
		}
	}
}
