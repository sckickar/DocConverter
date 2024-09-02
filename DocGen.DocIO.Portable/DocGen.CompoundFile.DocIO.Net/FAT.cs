using System;
using System.Collections.Generic;
using System.IO;

namespace DocGen.CompoundFile.DocIO.Net;

internal class FAT
{
	private List<int> m_lstFatChains = new List<int>();

	private List<int> m_freeSectors = new List<int>();

	private ushort m_usSectorShift;

	private Stream m_stream;

	private int m_iHeaderSize;

	public int SectorSize => 1 << (int)m_usSectorShift;

	public FAT(Stream parentStream, ushort sectorShift, int headerSize)
	{
		m_stream = parentStream;
		m_usSectorShift = sectorShift;
		m_iHeaderSize = headerSize;
	}

	public FAT(Stream parentStream, ushort sectorShift, Stream fatStreamToParse, int headerSize)
	{
		m_usSectorShift = sectorShift;
		m_iHeaderSize = headerSize;
		m_stream = parentStream;
		fatStreamToParse.Position = 0L;
		byte[] array = new byte[4];
		while (fatStreamToParse.Read(array, 0, 4) > 0)
		{
			m_lstFatChains.Add(BitConverter.ToInt32(array, 0));
		}
	}

	public FAT(CompoundFile file, Stream stream, DIF dif, FileHeader header)
	{
		if (file == null)
		{
			throw new ArgumentNullException("file");
		}
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		m_stream = file.BaseStream;
		List<int> sectorIds = dif.SectorIds;
		int sectorSize = header.SectorSize;
		m_usSectorShift = header.SectorShift;
		byte[] array = new byte[sectorSize];
		int[] array2 = new int[sectorSize >> 2];
		m_iHeaderSize = 512;
		int i = 0;
		for (int count = sectorIds.Count; i < count; i++)
		{
			int num = sectorIds[i];
			if (num >= 0)
			{
				file.ReadSector(array, 0, num, header);
				Buffer.BlockCopy(array, 0, array2, 0, sectorSize);
				m_lstFatChains.AddRange(array2);
			}
		}
	}

	public byte[] GetStream(Stream stream, int firstSector, CompoundFile file)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		if (file == null)
		{
			throw new ArgumentNullException("file");
		}
		if (firstSector < 0)
		{
			return null;
		}
		List<int> list = new List<int>();
		_ = file.Header;
		for (int num = firstSector; num != -2; num = m_lstFatChains[num])
		{
			if (num < 0 || num >= m_lstFatChains.Count)
			{
				throw new ApplicationException();
			}
			list.Add(num);
		}
		int count = list.Count;
		byte[] array = new byte[count << (int)m_usSectorShift];
		int num2 = 1 << (int)m_usSectorShift;
		int num3 = 0;
		int num4 = 0;
		while (num3 < count)
		{
			long sectorOffset = GetSectorOffset(list[num3]);
			stream.Position = sectorOffset;
			stream.Read(array, num4, num2);
			num3++;
			num4 += num2;
		}
		return array;
	}

	internal int NextSector(int sectorIndex)
	{
		return m_lstFatChains[sectorIndex];
	}

	internal void CloseChain(int iSector)
	{
		int num = m_lstFatChains[iSector];
		m_lstFatChains[iSector] = -2;
		while (num != -2)
		{
			iSector = num;
			num = m_lstFatChains[iSector];
			m_lstFatChains[iSector] = -1;
			m_freeSectors.Add(iSector);
		}
	}

	internal int EnlargeChain(int sector, int sectorCount)
	{
		if (sectorCount <= 0)
		{
			return sector;
		}
		int count = m_freeSectors.Count;
		int num = Math.Min(sectorCount, count);
		int count2 = sectorCount - num;
		AllocateFreeSectors(ref sector, num);
		int result = AllocateNewSectors(ref sector, count2);
		m_lstFatChains[sector] = -2;
		return result;
	}

	internal void FreeSector(int sector)
	{
		int count = m_lstFatChains.Count;
		if (sector < 0 || sector >= count)
		{
			throw new ArgumentOutOfRangeException("sector");
		}
		if (sector != count - 1)
		{
			m_lstFatChains[sector] = -1;
			m_freeSectors.Add(sector);
		}
		else
		{
			m_lstFatChains.RemoveAt(sector);
			FreeLastSector();
		}
	}

	private void FreeLastSector()
	{
		long length = Math.Max(0L, m_stream.Length - SectorSize);
		m_stream.SetLength(length);
	}

	private int AllocateNewSectors(ref int sector, int count)
	{
		int num = sector;
		int num2 = sector;
		sector = AddSectors(count);
		if (num < 0)
		{
			num = sector;
		}
		int num3 = 0;
		while (num3 < count)
		{
			m_lstFatChains.Add(sector + 1);
			if (num2 >= 0)
			{
				m_lstFatChains[num2] = sector;
			}
			num2 = sector;
			num3++;
			sector++;
		}
		sector--;
		m_lstFatChains[sector] = -2;
		return num;
	}

	private int AddSectors(int count)
	{
		long length = m_stream.Length;
		m_stream.SetLength(length + (count << (int)m_usSectorShift));
		return (int)(length - m_iHeaderSize >> (int)m_usSectorShift);
	}

	private int AllocateFreeSectors(ref int sector, int count)
	{
		int result = sector;
		for (int i = 0; i < count; i++)
		{
			int num = m_freeSectors[i];
			if (sector >= 0)
			{
				m_lstFatChains[sector] = num;
			}
			else
			{
				result = num;
			}
			sector = num;
		}
		m_freeSectors.RemoveRange(0, count);
		return result;
	}

	public void Write(Stream stream, DIF dif, FileHeader header)
	{
		int count = m_lstFatChains.Count;
		int sectorSize = header.SectorSize;
		ushort sectorShift = header.SectorShift;
		int num = SectorSize / 4 - 1;
		double num2 = (double)num * (double)count - 109.0;
		double num3 = (double)num * (double)num - 1.0;
		int num5 = (header.FatSectorsNumber = (int)Math.Ceiling(num2 / num3));
		byte[] array = new byte[sectorSize];
		dif.AllocateSectors(num5, this);
		AllocateFatSectors(num5, dif);
		List<int> sectorIds = dif.SectorIds;
		int i = 0;
		int fatItemToStart = 0;
		for (; i < num5; i++)
		{
			fatItemToStart = FillNextSector(fatItemToStart, array);
			long sectorOffset = CompoundFile.GetSectorOffset(sectorIds[i], sectorShift);
			stream.Seek(sectorOffset, SeekOrigin.Begin);
			stream.Write(array, 0, sectorSize);
		}
	}

	private void AllocateFatSectors(int fatSectorsCount, DIF dif)
	{
		if (dif == null)
		{
			throw new ArgumentNullException("dif");
		}
		List<int> sectorIds = dif.SectorIds;
		int count = sectorIds.Count;
		if (count < fatSectorsCount)
		{
			for (int i = count; i < fatSectorsCount; i++)
			{
				int item = AllocateSector(-3);
				sectorIds.Add(item);
			}
		}
	}

	private int FillNextSector(int fatItemToStart, byte[] arrSector)
	{
		int count = m_lstFatChains.Count;
		int num = arrSector.Length;
		int i = 0;
		while (i < num && fatItemToStart < count)
		{
			Buffer.BlockCopy(BitConverter.GetBytes(m_lstFatChains[fatItemToStart]), 0, arrSector, i, 4);
			i += 4;
			fatItemToStart++;
		}
		if (i < num)
		{
			byte[] bytes = BitConverter.GetBytes(-1);
			for (; i < num; i += 4)
			{
				Buffer.BlockCopy(bytes, 0, arrSector, i, 4);
			}
		}
		return fatItemToStart;
	}

	internal int AllocateSector(int sectorType)
	{
		int count = m_freeSectors.Count;
		int num;
		if (count > 0)
		{
			int index = count - 1;
			num = m_freeSectors[index];
			m_freeSectors.RemoveAt(index);
			m_lstFatChains[num] = sectorType;
		}
		else
		{
			num = AddSector();
			m_lstFatChains.Add(sectorType);
		}
		return num;
	}

	internal int AddSector()
	{
		long length = m_stream.Length;
		int sectorSize = SectorSize;
		m_stream.SetLength(length + sectorSize);
		return (int)(length - m_iHeaderSize >> (int)m_usSectorShift);
	}

	internal void WriteSimple(MemoryStream stream, int sectorSize)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		if (sectorSize <= 0)
		{
			throw new ArgumentOutOfRangeException("sectorSize");
		}
		int num = (int)Math.Ceiling((double)(m_lstFatChains.Count * 4) / (double)sectorSize);
		byte[] array = new byte[sectorSize];
		int num2 = sectorSize / 4;
		for (int i = 0; i < num; i++)
		{
			int fatItemToStart = i * num2;
			FillNextSector(fatItemToStart, array);
			stream.Write(array, 0, sectorSize);
		}
	}

	internal long GetSectorOffset(int sectorIndex)
	{
		return CompoundFile.GetSectorOffset(sectorIndex, m_usSectorShift, m_iHeaderSize);
	}

	internal int GetChainLength(int firstSector)
	{
		int num = 1;
		while ((firstSector = NextSector(firstSector)) >= 0)
		{
			num++;
		}
		return num;
	}
}
