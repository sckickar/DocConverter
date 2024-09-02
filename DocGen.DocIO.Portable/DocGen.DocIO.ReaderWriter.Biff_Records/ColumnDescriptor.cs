using System;

namespace DocGen.DocIO.ReaderWriter.Biff_Records;

[CLSCompliant(false)]
internal class ColumnDescriptor
{
	private const ushort DEF_WIDTH = 1000;

	private const ushort DEF_SPACE = 720;

	private SinglePropertyModifierRecord m_widthRecord;

	private SinglePropertyModifierRecord m_spaceRecord;

	internal ushort Width
	{
		get
		{
			return BitConverter.ToUInt16(m_widthRecord.ByteArray, 1);
		}
		set
		{
			byte[] bytes = BitConverter.GetBytes(value);
			m_widthRecord.ByteArray[1] = bytes[0];
			m_widthRecord.ByteArray[2] = bytes[1];
		}
	}

	internal ushort Space
	{
		get
		{
			return BitConverter.ToUInt16(m_spaceRecord.ByteArray, 1);
		}
		set
		{
			byte[] bytes = BitConverter.GetBytes(value);
			m_spaceRecord.ByteArray[1] = bytes[0];
			m_spaceRecord.ByteArray[2] = bytes[1];
		}
	}

	internal ColumnDescriptor(SinglePropertyModifierRecord width, SinglePropertyModifierRecord space)
	{
		m_widthRecord = width;
		m_spaceRecord = space;
		Width = 1000;
		Space = 720;
	}

	internal ColumnDescriptor()
	{
		m_widthRecord = new SinglePropertyModifierRecord(61955);
		m_widthRecord.ByteArray = new byte[3];
		m_spaceRecord = new SinglePropertyModifierRecord(61956);
		m_spaceRecord.ByteArray = new byte[3];
		Width = 1000;
		Space = 720;
	}
}
