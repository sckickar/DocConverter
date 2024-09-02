using System.Collections.Generic;
using System.IO;

namespace DocGen.Pdf.Graphics;

internal class TiffMetadataParser : IImageMetadataParser
{
	private CatalogedReaderBase m_reader;

	internal TiffMetadataParser(Stream stream)
	{
		m_reader = new CatalogedReader(stream);
	}

	public MemoryStream GetMetadata()
	{
		TiffTag tiffTag = new TiffTag();
		switch (m_reader.ReadInt16(0))
		{
		case 19789:
			m_reader = m_reader.WithByteOrder(isBigEndian: true);
			break;
		case 18761:
			m_reader = m_reader.WithByteOrder(isBigEndian: false);
			break;
		}
		ushort tiffMarker = m_reader.ReadUInt16(2);
		tiffTag.SetTiffMarker(tiffMarker);
		int num = m_reader.ReadInt32(4);
		if (num >= m_reader.Length - 1)
		{
			num = 8;
		}
		List<int> processedOffsets = new List<int>();
		ProcessImageFileDirectory(tiffTag, m_reader, processedOffsets, num);
		return tiffTag.Data;
	}

	internal static void ProcessImageFileDirectory(TiffTag tag, CatalogedReaderBase reader, ICollection<int> processedOffsets, int offset)
	{
		int item = reader.ToUnshiftedOffset(offset);
		if (processedOffsets.Contains(item))
		{
			return;
		}
		processedOffsets.Add(item);
		if (offset < 0 || offset >= reader.Length)
		{
			return;
		}
		int num = reader.ReadUInt16(offset);
		if (num > 255 && (num & 0xFF) == 0)
		{
			num >>= 8;
			reader = reader.WithByteOrder(!reader.IsBigEndian);
		}
		if (2 + 12 * num + 4 + offset > reader.Length)
		{
			return;
		}
		int num2 = 0;
		for (int i = 0; i < num; i++)
		{
			int num3 = CalculateTagOffset(offset, i);
			int tagId = reader.ReadUInt16(num3);
			DataTypeID dataTypeID = (DataTypeID)reader.ReadUInt16(num3 + 2);
			uint num4 = reader.ReadUInt32(num3 + 4);
			DataType dataType = DataType.FromTiffFormatCode(dataTypeID);
			long byteCount;
			if (dataType == null)
			{
				if (!tag.TryCustomProcessFormat(tagId, dataTypeID, num4, out byteCount))
				{
					if (++num2 > 5)
					{
						return;
					}
					continue;
				}
			}
			else
			{
				byteCount = num4 * dataType.Size;
			}
			long num5;
			if (byteCount > 4)
			{
				num5 = reader.ReadUInt32(num3 + 8);
				if (num5 + byteCount > reader.Length)
				{
					continue;
				}
			}
			else
			{
				num5 = num3 + 8;
			}
			if (num5 < 0 || num5 > reader.Length || byteCount < 0 || num5 + byteCount > reader.Length)
			{
				continue;
			}
			bool flag = false;
			if (byteCount == checked(4L * unchecked((long)num4)))
			{
				for (int j = 0; j < num4; j++)
				{
					if (tag.TryEnterSubDirectory(tagId))
					{
						flag = true;
						uint offset2 = reader.ReadUInt32((int)(num5 + j * 4));
						ProcessImageFileDirectory(tag, reader, processedOffsets, (int)offset2);
					}
				}
			}
			if (!flag)
			{
				tag.CustomProcessTag((int)num5, processedOffsets, reader, tagId, (int)byteCount);
			}
		}
		int index = CalculateTagOffset(offset, num);
		int num6 = reader.ReadInt32(index);
		if (num6 != 0 && num6 < reader.Length && num6 >= offset)
		{
			ProcessImageFileDirectory(tag, reader, processedOffsets, num6);
		}
	}

	private static int CalculateTagOffset(int offset, int entry)
	{
		return offset + 2 + 12 * entry;
	}
}
