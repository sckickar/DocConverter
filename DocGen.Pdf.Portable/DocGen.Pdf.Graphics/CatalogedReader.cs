using System;
using System.Collections.Generic;
using System.IO;

namespace DocGen.Pdf.Graphics;

internal class CatalogedReader : CatalogedReaderBase
{
	private const int m_defaultSegmentLength = 2048;

	private readonly Stream m_stream;

	private readonly int m_length;

	private readonly List<byte[]> m_segments = new List<byte[]>();

	private bool m_isStreamFinished;

	private int m_streamLength;

	internal override long Length
	{
		get
		{
			if (m_stream.Length > 0)
			{
				return m_stream.Length;
			}
			IsValidIndex(int.MaxValue, 1);
			return m_streamLength;
		}
	}

	internal CatalogedReader(Stream stream)
		: base(isBigEndian: true)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		m_length = 2048;
		m_stream = stream;
	}

	internal CatalogedReader(Stream stream, bool isBigEndian)
		: base(isBigEndian)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		m_length = 2048;
		m_stream = stream;
	}

	internal override bool IsValidIndex(int index, int bytesRequested)
	{
		if (index < 0 || bytesRequested < 0)
		{
			return false;
		}
		long num = (long)index + (long)bytesRequested - 1;
		if (num > int.MaxValue)
		{
			return false;
		}
		int num2 = (int)num;
		if (m_isStreamFinished)
		{
			return num2 < m_streamLength;
		}
		int num3 = num2 / m_length;
		while (num3 >= m_segments.Count)
		{
			byte[] array = new byte[m_length];
			int num4 = 0;
			while (!m_isStreamFinished && num4 != m_length)
			{
				int num5 = m_stream.Read(array, num4, m_length - num4);
				if (num5 == 0)
				{
					m_isStreamFinished = true;
					m_streamLength = m_segments.Count * m_length + num4;
					if (num2 >= m_streamLength)
					{
						m_segments.Add(array);
						return false;
					}
				}
				else
				{
					num4 += num5;
				}
			}
			m_segments.Add(array);
		}
		return true;
	}

	internal override int ToUnshiftedOffset(int offset)
	{
		return offset;
	}

	internal override byte ReadByte(int index)
	{
		if (IsValidIndex(index, 1))
		{
			int index2 = index / m_length;
			int num = index % m_length;
			return m_segments[index2][num];
		}
		throw new Exception("Invalid index to read byte");
	}

	internal override byte[] GetBytes(int index, int count)
	{
		byte[] array = new byte[count];
		if (IsValidIndex(index, count))
		{
			int num = count;
			int num2 = index;
			int num3 = 0;
			while (num != 0)
			{
				int index2 = num2 / m_length;
				int num4 = num2 % m_length;
				int num5 = Math.Min(num, m_length - num4);
				Array.Copy(m_segments[index2], num4, array, num3, num5);
				num -= num5;
				num2 += num5;
				num3 += num5;
			}
		}
		return array;
	}

	internal override CatalogedReaderBase WithShiftedBaseOffset(int shift)
	{
		if (shift != 0)
		{
			return new ShiftedIndexedCapturingReader(this, shift, base.IsBigEndian);
		}
		return this;
	}

	internal override CatalogedReaderBase WithByteOrder(bool isBigEndian)
	{
		if (isBigEndian != base.IsBigEndian)
		{
			return new ShiftedIndexedCapturingReader(this, 0, isBigEndian);
		}
		return this;
	}
}
