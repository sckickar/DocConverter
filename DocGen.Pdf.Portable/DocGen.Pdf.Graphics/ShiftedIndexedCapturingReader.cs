using System;

namespace DocGen.Pdf.Graphics;

internal class ShiftedIndexedCapturingReader : CatalogedReaderBase
{
	private readonly CatalogedReader m_baseReader;

	private readonly int m_baseOffset;

	internal override long Length => m_baseReader.Length - m_baseOffset;

	internal ShiftedIndexedCapturingReader(CatalogedReader baseReader, int baseOffset, bool isBigEndian)
		: base(isBigEndian)
	{
		if (baseOffset < 0)
		{
			throw new Exception("Invalid offset");
		}
		m_baseReader = baseReader;
		m_baseOffset = baseOffset;
	}

	internal override CatalogedReaderBase WithByteOrder(bool isBigEndian)
	{
		if (isBigEndian != base.IsBigEndian)
		{
			return new ShiftedIndexedCapturingReader(m_baseReader, m_baseOffset, isBigEndian);
		}
		return this;
	}

	internal override CatalogedReaderBase WithShiftedBaseOffset(int shift)
	{
		if (shift != 0)
		{
			return new ShiftedIndexedCapturingReader(m_baseReader, m_baseOffset + shift, base.IsBigEndian);
		}
		return this;
	}

	internal override int ToUnshiftedOffset(int localOffset)
	{
		return localOffset + m_baseOffset;
	}

	internal override byte ReadByte(int index)
	{
		return m_baseReader.ReadByte(m_baseOffset + index);
	}

	internal override byte[] GetBytes(int index, int count)
	{
		return m_baseReader.GetBytes(m_baseOffset + index, count);
	}

	internal override bool IsValidIndex(int index, int length)
	{
		return m_baseReader.IsValidIndex(index + m_baseOffset, length);
	}
}
