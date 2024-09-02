using System;
using System.Diagnostics;
using System.IO;

namespace DocGen.CompoundFile.DocIO.Net;

internal class CompoundFile : ICompoundFile, IDisposable
{
	private const string RootEntryName = "Root Entry";

	private Stream m_stream;

	private FileHeader m_header;

	private FAT m_fat;

	private DIF m_dif;

	private Directory m_directory;

	private CompoundStorage m_root;

	private Stream m_shortStream;

	private Stream m_miniFatStream;

	private FAT m_miniFat;

	private bool m_bDirectMode;

	internal FileHeader Header => m_header;

	public Directory Directory => m_directory;

	public ICompoundStorage Root => m_root;

	internal DIF DIF => m_dif;

	internal FAT Fat => m_fat;

	internal Stream BaseStream => m_stream;

	internal bool DirectMode
	{
		get
		{
			return m_bDirectMode;
		}
		set
		{
			m_bDirectMode = value;
		}
	}

	public ICompoundStorage RootStorage => m_root;

	public CompoundFile()
	{
		m_stream = new MemoryStream();
		InitializeVariables();
	}

	public CompoundFile(Stream stream)
	{
		Open(stream);
	}

	public void Open(Stream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		stream.Position = 0L;
		long position = stream.Position;
		int num = (int)(stream.Length - position);
		MemoryStream memoryStream = new MemoryStream(num);
		memoryStream.SetLength(num);
		byte[] array = new byte[num];
		stream.Read(array, 0, array.Length);
		memoryStream.Write(array, 0, array.Length);
		memoryStream.Position = 0L;
		m_stream = memoryStream;
		m_header = new FileHeader(m_stream);
		m_dif = new DIF(m_stream, m_header);
		m_fat = new FAT(this, m_stream, m_dif, m_header);
		byte[] stream2 = m_fat.GetStream(m_stream, m_header.DirectorySectorStart, this);
		m_directory = new Directory(stream2);
		DirectoryEntry directoryEntry = m_directory.Entries[0];
		m_root = new CompoundStorage(this, directoryEntry);
		int startSector = directoryEntry.StartSector;
		if (startSector >= 0)
		{
			m_shortStream = new MemoryStream(m_fat.GetStream(m_stream, startSector, this));
			m_miniFatStream = new MemoryStream(m_fat.GetStream(m_stream, m_header.MiniFastStart, this));
			m_miniFat = new FAT(m_shortStream, m_header.MiniSectorShift, m_miniFatStream, 0);
			m_fat.CloseChain(startSector);
			m_fat.CloseChain(m_header.MiniFastStart);
		}
	}

	private void InitializeVariables()
	{
		m_directory = new Directory();
		m_root = new CompoundStorage(this, "Root Entry", 0);
		m_stream.SetLength(512L);
		m_header = new FileHeader();
		m_dif = new DIF();
		DirectoryEntry entry = m_root.Entry;
		entry.Type = DirectoryEntry.EntryType.Root;
		m_directory.Add(entry);
		m_fat = new FAT(m_stream, m_header.SectorShift, 512);
	}

	internal void ReadSector(byte[] buffer, int offset, int sectorIndex, FileHeader header)
	{
		ushort sectorShift = header.SectorShift;
		int sectorSize = header.SectorSize;
		long sectorOffset = GetSectorOffset(sectorIndex, sectorShift);
		m_stream.Position = sectorOffset;
		m_stream.Read(buffer, offset, sectorSize);
	}

	internal Stream GetEntryStream(DirectoryEntry entry)
	{
		if (entry == null)
		{
			throw new ArgumentNullException("entry");
		}
		Stream stream = null;
		if (entry.Type == DirectoryEntry.EntryType.Stream)
		{
			FAT fAT;
			Stream stream2;
			if (m_miniFat != null && entry.Size < m_header.MiniSectorCutoff)
			{
				fAT = m_miniFat;
				stream2 = m_shortStream;
			}
			else
			{
				fAT = m_fat;
				stream2 = m_stream;
			}
			byte[] stream3 = fAT.GetStream(stream2, entry.StartSector, this);
			stream = ((stream3 != null) ? new MemoryStream(stream3) : new MemoryStream());
			stream.SetLength(entry.Size);
		}
		return stream;
	}

	internal void SetEntryStream(DirectoryEntry entry, Stream stream)
	{
		if (entry == null)
		{
			throw new ArgumentNullException("entry");
		}
		if (stream.Length >= m_header.MiniSectorCutoff)
		{
			SetEntryLongStream(entry, stream);
		}
		else
		{
			SetEntryShortStream(entry, stream);
		}
		entry.Size = (uint)stream.Length;
	}

	private void SetEntryLongStream(DirectoryEntry entry, Stream stream)
	{
		_ = m_header.SectorShift;
		int sectorSize = m_header.SectorSize;
		long num = entry.Size;
		long length = stream.Length;
		int iAllocatedSectors = (int)Math.Ceiling((double)num / (double)sectorSize);
		int iRequiredSectors = (int)Math.Ceiling((double)length / (double)sectorSize);
		AllocateSectors(entry, iAllocatedSectors, iRequiredSectors, m_fat);
		WriteData(m_stream, entry.StartSector, stream, m_fat);
	}

	private void SetEntryShortStream(DirectoryEntry entry, Stream stream)
	{
		if (m_shortStream == null)
		{
			m_shortStream = new MemoryStream();
		}
		if (m_miniFat == null)
		{
			m_miniFat = new FAT(m_shortStream, m_header.MiniSectorShift, 0);
		}
		_ = m_header.MiniSectorShift;
		int sectorSize = m_miniFat.SectorSize;
		long num = entry.Size;
		long length = stream.Length;
		int iAllocatedSectors = (int)Math.Ceiling((double)num / (double)sectorSize);
		int iRequiredSectors = (int)Math.Ceiling((double)length / (double)sectorSize);
		AllocateSectors(entry, iAllocatedSectors, iRequiredSectors, m_miniFat);
		WriteData(m_shortStream, entry.StartSector, stream, m_miniFat);
	}

	private void WriteData(Stream destination, int startSector, Stream stream, FAT fat)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		long sectorOffset = fat.GetSectorOffset(startSector);
		int sectorSize = fat.SectorSize;
		byte[] buffer = new byte[sectorSize];
		long position = stream.Position;
		stream.Position = 0L;
		int count;
		while ((count = stream.Read(buffer, 0, sectorSize)) > 0)
		{
			destination.Position = sectorOffset;
			destination.Write(buffer, 0, count);
			startSector = fat.NextSector(startSector);
			if (startSector < 0)
			{
				break;
			}
			sectorOffset = fat.GetSectorOffset(startSector);
		}
		stream.Position = position;
	}

	private void AllocateSectors(DirectoryEntry entry, int iAllocatedSectors, int iRequiredSectors, FAT fat)
	{
		if (iAllocatedSectors != iRequiredSectors)
		{
			int num = ((entry.LastSector >= 0) ? entry.LastSector : entry.StartSector);
			int startSector = AllocateSectors(num, iAllocatedSectors, iRequiredSectors, fat);
			if (num < 0)
			{
				entry.StartSector = startSector;
			}
		}
	}

	private int AllocateSectors(int iSector, int iAllocatedSectors, int iRequiredSectors, FAT fat)
	{
		int result = -1;
		if (iAllocatedSectors == iRequiredSectors)
		{
			result = iSector;
		}
		else if (iAllocatedSectors < iRequiredSectors)
		{
			for (int num = ((iSector >= 0) ? fat.NextSector(iSector) : iSector); num >= 0; num = fat.NextSector(iSector))
			{
				iSector = num;
			}
			int num2 = fat.EnlargeChain(iSector, iRequiredSectors - iAllocatedSectors);
			if (iSector < 0)
			{
				result = num2;
			}
		}
		else
		{
			for (int i = 0; i < iRequiredSectors - 1; i++)
			{
				iSector = fat.NextSector(iSector);
			}
			fat.CloseChain(iSector);
		}
		return result;
	}

	[CLSCompliant(false)]
	public static long GetSectorOffset(int sectorIndex, ushort sectorShift)
	{
		return (sectorIndex << (int)sectorShift) + 512;
	}

	[CLSCompliant(false)]
	public static long GetSectorOffset(int sectorIndex, ushort sectorShift, int headerSize)
	{
		return (sectorIndex << (int)sectorShift) + headerSize;
	}

	public static bool CheckHeader(Stream stream)
	{
		return FileHeader.CheckSignature(stream);
	}

	internal DirectoryEntry AllocateDirectoryEntry(string streamName, DirectoryEntry.EntryType entryType)
	{
		DirectoryEntry directoryEntry = new DirectoryEntry(streamName, entryType, m_directory.Entries.Count);
		m_directory.Add(directoryEntry);
		DateTime dateModify = (directoryEntry.DateCreate = DateTime.Now);
		directoryEntry.DateModify = dateModify;
		return directoryEntry;
	}

	internal void RemoveItem(DirectoryEntry directoryEntry)
	{
		if (directoryEntry == null)
		{
			throw new ArgumentNullException("directoryEntry");
		}
		directoryEntry.Type = DirectoryEntry.EntryType.Invalid;
		if (directoryEntry.Type == DirectoryEntry.EntryType.Stream)
		{
			m_fat.CloseChain(directoryEntry.StartSector);
			directoryEntry.StartSector = -1;
		}
	}

	internal int ReadData(DirectoryEntry entry, long position, byte[] buffer, int length)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	internal void WriteData(DirectoryEntry entry, long position, byte[] buffer, int offset, int length)
	{
		_ = m_header.SectorShift;
		int sectorSize = m_header.SectorSize;
		long num = entry.Size;
		long num2 = position + length;
		int num3 = (int)Math.Ceiling((double)num / (double)sectorSize);
		int num4 = (int)Math.Ceiling((double)num2 / (double)sectorSize);
		if (num4 > num3)
		{
			AllocateSectors(entry, num3, num4, m_fat);
		}
		int iCurrentSector = entry.StartSector;
		int sectorSize2 = m_fat.SectorSize;
		int iCurrentOffset = sectorSize2;
		GetOffsets(entry, position, ref iCurrentOffset, ref iCurrentSector);
		entry.LastSector = iCurrentSector;
		entry.LastOffset = iCurrentOffset;
		int num5 = (int)(position % sectorSize2);
		while (length > 0)
		{
			int num6 = Math.Min(length, sectorSize2 - num5);
			long position2 = m_fat.GetSectorOffset(iCurrentSector) + num5;
			m_stream.Position = position2;
			m_stream.Write(buffer, offset, num6);
			num5 = 0;
			offset += num6;
			length -= num6;
			iCurrentSector = m_fat.NextSector(iCurrentSector);
		}
		entry.Size = (uint)Math.Max(entry.Size, num2);
	}

	private void GetOffsets(DirectoryEntry entry, long position, ref int iCurrentOffset, ref int iCurrentSector)
	{
		int sectorSize = m_fat.SectorSize;
		if (entry.LastSector >= 0)
		{
			int num = (int)position % sectorSize;
			long num2 = ((num > 0) ? (position + sectorSize - num) : position);
			if (entry.LastOffset <= num2)
			{
				iCurrentOffset = entry.LastOffset;
				iCurrentSector = entry.LastSector;
			}
			else
			{
				Debugger.Break();
			}
		}
		while (iCurrentOffset <= position)
		{
			iCurrentSector = m_fat.NextSector(iCurrentSector);
			iCurrentOffset += sectorSize;
		}
	}

	public void Flush()
	{
		m_root.Flush();
		SaveMiniStream();
		SerializeDirectory();
		m_fat.Write(m_stream, m_dif, m_header);
		m_dif.Write(m_stream, m_header);
		m_header.Write(m_stream);
		m_stream.Position = 0L;
	}

	public void Save(Stream stream)
	{
		Flush();
		WriteStreamTo(stream);
	}

	private void WriteStreamTo(Stream destination)
	{
		if (m_stream is MemoryStream memoryStream)
		{
			memoryStream.WriteTo(destination);
			return;
		}
		byte[] buffer = new byte[32768];
		int count;
		while ((count = m_stream.Read(buffer, 0, 32768)) > 0)
		{
			destination.Write(buffer, 0, count);
		}
	}

	private void SaveMiniStream()
	{
		if (m_shortStream != null && m_shortStream.Length != 0L)
		{
			int iRequiredSectors = (int)Math.Ceiling((double)m_shortStream.Length / (double)m_header.SectorSize);
			DirectoryEntry directoryEntry = m_directory.Entries[0];
			int startSector = directoryEntry.StartSector;
			int iAllocatedSectors = (int)Math.Ceiling((double)directoryEntry.Size / (double)m_fat.SectorSize);
			startSector = AllocateSectors(startSector, iAllocatedSectors, iRequiredSectors, m_fat);
			WriteData(m_stream, startSector, m_shortStream, m_fat);
			DirectoryEntry directoryEntry2 = m_directory.Entries[0];
			directoryEntry2.StartSector = startSector;
			directoryEntry2.Size = (uint)m_shortStream.Length;
			MemoryStream memoryStream = new MemoryStream();
			m_miniFat.WriteSimple(memoryStream, m_header.SectorSize);
			iRequiredSectors = (int)Math.Ceiling((double)memoryStream.Length / (double)m_header.SectorSize);
			startSector = AllocateSectors(m_header.MiniFastStart, m_header.MiniFatNumber, iRequiredSectors, m_fat);
			WriteData(m_stream, startSector, memoryStream, m_fat);
			m_header.MiniFastStart = startSector;
			_ = m_header.MiniSectorShift;
			m_header.MiniFatNumber = iRequiredSectors;
			memoryStream.Dispose();
		}
	}

	private void SerializeDirectory()
	{
		MemoryStream memoryStream = new MemoryStream();
		m_directory.Write(memoryStream);
		int iRequiredSectors = (int)Math.Ceiling((double)memoryStream.Length / (double)m_header.SectorSize);
		int directorySectorStart = m_header.DirectorySectorStart;
		int iAllocatedSectors = ((directorySectorStart >= 0) ? m_fat.GetChainLength(directorySectorStart) : 0);
		int num2 = (m_header.DirectorySectorStart = AllocateSectors(directorySectorStart, iAllocatedSectors, iRequiredSectors, m_fat));
		directorySectorStart = num2;
		WriteData(m_stream, directorySectorStart, memoryStream, m_fat);
		memoryStream.Dispose();
	}

	public void Dispose()
	{
		if (m_root != null)
		{
			m_root.Dispose();
			m_root = null;
			m_stream.Dispose();
			m_stream = null;
			m_header = null;
			m_fat = null;
			m_directory = null;
		}
		if (m_shortStream != null)
		{
			m_shortStream.Dispose();
			m_shortStream = null;
		}
	}
}
