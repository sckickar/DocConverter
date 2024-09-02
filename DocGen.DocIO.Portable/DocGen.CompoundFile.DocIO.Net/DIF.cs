using System;
using System.Collections.Generic;
using System.IO;

namespace DocGen.CompoundFile.DocIO.Net;

internal class DIF
{
	public const int SectorsInHeader = 109;

	private List<int> m_arrSectorID;

	private List<int> m_arrDifSectors = new List<int>();

	public List<int> SectorIds => m_arrSectorID;

	public DIF()
	{
		m_arrSectorID = new List<int>();
	}

	public DIF(Stream stream, FileHeader header)
	{
		int difNumber = header.DifNumber;
		int sectorSize = header.SectorSize;
		ushort sectorShift = header.SectorShift;
		int capacity = 109 + difNumber * (sectorSize - 4) / 4;
		m_arrSectorID = new List<int>(capacity);
		m_arrSectorID.AddRange(header.FatStart);
		if (difNumber > 0)
		{
			int num = header.DifStart;
			_ = header.SectorShift;
			byte[] array = new byte[sectorSize];
			int[] array2 = new int[sectorSize / 4 - 1];
			while (num >= 0)
			{
				long sectorOffset = CompoundFile.GetSectorOffset(num, sectorShift);
				m_arrDifSectors.Add(num);
				stream.Position = sectorOffset;
				stream.Read(array, 0, sectorSize);
				Buffer.BlockCopy(array, 0, array2, 0, sectorSize - 4);
				m_arrSectorID.AddRange(array2);
				num = BitConverter.ToInt32(array, sectorSize - 4);
			}
		}
	}

	internal void Write(Stream stream, FileHeader header)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		int count = m_arrSectorID.Count;
		int[] fatStart = header.FatStart;
		int i;
		for (i = 0; i < count && i < 109; i++)
		{
			fatStart[i] = m_arrSectorID[i];
		}
		for (; i < 109; i++)
		{
			fatStart[i] = -1;
		}
		if (m_arrDifSectors.Count > 0)
		{
			header.DifStart = m_arrDifSectors[0];
			header.DifNumber = m_arrDifSectors.Count;
		}
		byte[] array = new byte[header.SectorSize];
		int sectorSize = header.SectorSize;
		int num = sectorSize / 4 - 1;
		int j = 0;
		for (int count2 = m_arrDifSectors.Count; j < count2; j++)
		{
			int sectorIndex = m_arrDifSectors[j];
			long sectorOffset = header.GetSectorOffset(sectorIndex);
			int num2 = 0;
			int num3 = 0;
			while (num2 < num)
			{
				Buffer.BlockCopy(BitConverter.GetBytes((i < count) ? m_arrSectorID[i] : (-1)), 0, array, num3, 4);
				num2++;
				i++;
				num3 += 4;
			}
			Buffer.BlockCopy(BitConverter.GetBytes((j == count2 - 1) ? (-2) : m_arrDifSectors[j + 1]), 0, array, sectorSize - 4, 4);
			stream.Position = sectorOffset;
			stream.Write(array, 0, sectorSize);
		}
	}

	internal void AllocateSectors(int fatSectorsRequired, FAT fat)
	{
		int num = fatSectorsRequired - 109;
		if (num > 0)
		{
			int additionalSectors = (int)Math.Ceiling((double)(num * 4) / (double)(fat.SectorSize - 4));
			AllocateDifSectors(additionalSectors, fat);
		}
	}

	private void AllocateDifSectors(int additionalSectors, FAT fat)
	{
		int count = m_arrDifSectors.Count;
		if (count != additionalSectors)
		{
			if (count > additionalSectors)
			{
				RemoveLastSectors(count - additionalSectors, fat);
			}
			else
			{
				AddDifSectors(additionalSectors - count, fat);
			}
		}
	}

	private void RemoveLastSectors(int sectorCount, FAT fat)
	{
		if (sectorCount < 0)
		{
			throw new ArgumentOutOfRangeException("sectorCount");
		}
		if (sectorCount == 0)
		{
			return;
		}
		if (fat == null)
		{
			throw new ArgumentNullException("fat");
		}
		int num = 0;
		int num2 = m_arrDifSectors.Count - 1;
		while (num < sectorCount)
		{
			int sector = m_arrDifSectors[num2];
			fat.FreeSector(sector);
			num++;
			num2--;
		}
		m_arrDifSectors.RemoveRange(m_arrDifSectors.Count - sectorCount, sectorCount);
		throw new NotImplementedException();
	}

	private void AddDifSectors(int sectorCount, FAT fat)
	{
		if (sectorCount < 0)
		{
			throw new ArgumentOutOfRangeException("sectorCount");
		}
		if (fat == null)
		{
			throw new ArgumentNullException("fat");
		}
		for (int i = 0; i < sectorCount; i++)
		{
			int item = fat.AllocateSector(-4);
			m_arrDifSectors.Add(item);
		}
	}
}
