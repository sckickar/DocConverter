using System;
using System.Collections.Generic;
using System.IO;
using DocGen.DocIO.ReaderWriter.Biff_Records;
using DocGen.DocIO.ReaderWriter.Biff_Records.Structures;

namespace DocGen.DocIO.ReaderWriter;

[CLSCompliant(false)]
internal class WordFKPData
{
	private const int DEF_COUNTRUN_SIZE = 1;

	private Fib m_fib;

	private WPTablesData m_tables;

	private List<uint> m_papxPositions = new List<uint>();

	private List<ParagraphExceptionInDiskPage> m_papxProps = new List<ParagraphExceptionInDiskPage>();

	private List<uint> m_chpxPositions = new List<uint>();

	private List<CharacterPropertyException> m_chpxProps = new List<CharacterPropertyException>();

	private List<int> m_sepxPositions = new List<int>();

	private List<SectionPropertyException> m_sepxProps = new List<SectionPropertyException>();

	private FKPStructure[] m_chpxFKPs = new FKPStructure[0];

	private FKPStructure[] m_papxFKPs = new FKPStructure[0];

	private ParagraphPropertiesPage[] m_papxPages;

	private CharacterPropertiesPage[] m_chpxPages;

	private SectionPropertyException[] m_secProperties;

	private long m_lastSepxPosition;

	internal Fib Fib => m_fib;

	internal long EndOfSepx => m_lastSepxPosition;

	internal int SepxAddedCount => m_sepxProps.Count;

	internal WPTablesData Tables => m_tables;

	internal WordFKPData(Fib fib, WPTablesData tables)
	{
		m_fib = fib;
		m_tables = tables;
	}

	internal SectionPropertyException GetSepx(int index)
	{
		return m_secProperties[index];
	}

	internal void AddChpxProperties(uint pos, CharacterPropertyException chpx)
	{
		if (chpx == null)
		{
			chpx = new CharacterPropertyException();
		}
		m_chpxPositions.Add(pos);
		m_chpxProps.Add(chpx);
	}

	internal void AddPapxProperties(uint pos, ParagraphExceptionInDiskPage papx, MemoryStream dataStream)
	{
		if (papx == null)
		{
			papx = new ParagraphExceptionInDiskPage();
		}
		m_papxPositions.Add(pos);
		if (papx.Length < 485)
		{
			m_papxProps.Add(papx);
			return;
		}
		int value = (int)dataStream.Position;
		byte[] bytes = BitConverter.GetBytes((short)papx.PropertyModifiers.Length);
		dataStream.Write(bytes, 0, bytes.Length);
		papx.SaveHugePapx(dataStream);
		papx = new ParagraphExceptionInDiskPage();
		papx.PropertyModifiers.SetIntValue(26182, value);
		m_papxProps.Add(papx);
	}

	internal void AddSepxProperties(int pos, SectionPropertyException sepx)
	{
		m_sepxPositions.Add(pos);
		m_sepxProps.Add(sepx);
	}

	internal void Read(MemoryStream stream)
	{
		m_chpxFKPs = ReadFKPs(stream, m_tables.CHPXBinaryTable);
		m_chpxPages = new CharacterPropertiesPage[m_chpxFKPs.Length];
		m_papxFKPs = ReadFKPs(stream, m_tables.PAPXBinaryTable);
		m_papxPages = new ParagraphPropertiesPage[m_papxFKPs.Length];
		SectionDescriptor[] descriptors = m_tables.SectionsTable.Descriptors;
		m_secProperties = new SectionPropertyException[descriptors.Length];
		int i = 0;
		for (int num = descriptors.Length; i < num; i++)
		{
			uint sepxPosition = descriptors[i].SepxPosition;
			if (sepxPosition != uint.MaxValue)
			{
				stream.Position = sepxPosition;
				m_secProperties[i] = new SectionPropertyException(stream);
			}
			else
			{
				m_secProperties[i] = new SectionPropertyException(isDefaultSEP: true);
			}
		}
		m_lastSepxPosition = stream.Position;
	}

	internal void Write(Stream stream)
	{
		WriteChpx(stream);
		WritePapx(stream);
		WriteSepx(stream);
	}

	internal ParagraphPropertiesPage GetPapxPage(int i)
	{
		if (m_papxPages[i] == null)
		{
			m_papxPages[i] = new ParagraphPropertiesPage(m_papxFKPs[i]);
		}
		return m_papxPages[i];
	}

	internal CharacterPropertiesPage GetChpxPage(int i)
	{
		if (m_chpxPages[i] == null)
		{
			m_chpxPages[i] = new CharacterPropertiesPage(m_chpxFKPs[i]);
		}
		return m_chpxPages[i];
	}

	internal void CloneAndAddLastPapx(uint pos)
	{
		int index = m_papxPositions.IndexOf(pos);
		ParagraphExceptionInDiskPage value = m_papxProps[index];
		m_papxProps[m_papxProps.Count - 1] = value;
	}

	internal void CloneAndAddLastChpx(uint pos)
	{
		int index = m_chpxPositions.IndexOf(pos);
		CharacterPropertyException value = m_chpxProps[index];
		m_chpxProps[m_chpxProps.Count - 1] = value;
	}

	internal void Close()
	{
		m_fib = null;
		if (m_tables != null)
		{
			m_tables.Close();
			m_tables = null;
		}
		if (m_papxPositions != null)
		{
			m_papxPositions.Clear();
			m_papxPositions = null;
		}
		if (m_papxProps != null)
		{
			m_papxProps.Clear();
			m_papxProps = null;
		}
		if (m_chpxPositions != null)
		{
			m_chpxPositions.Clear();
			m_chpxPositions = null;
		}
		if (m_chpxProps != null)
		{
			m_chpxProps.Clear();
			m_chpxProps = null;
		}
		if (m_sepxPositions != null)
		{
			m_sepxPositions.Clear();
			m_sepxPositions = null;
		}
		if (m_sepxProps != null)
		{
			m_sepxProps.Clear();
			m_sepxProps = null;
		}
		if (m_chpxFKPs != null)
		{
			m_chpxFKPs = null;
		}
		if (m_papxFKPs != null)
		{
			m_papxFKPs = null;
		}
		if (m_papxPages != null)
		{
			m_papxPages = null;
		}
		if (m_chpxPages != null)
		{
			m_chpxPages = null;
		}
		if (m_secProperties != null)
		{
			m_secProperties = null;
		}
	}

	internal FKPStructure[] ReadFKPs(MemoryStream stream, BinaryTable table)
	{
		int num = table.Entries.Length;
		FKPStructure[] array = new FKPStructure[num];
		for (int i = 0; i < num; i++)
		{
			int value = table.Entries[i].Value;
			stream.Position = value * 512;
			array[i] = new FKPStructure(stream);
		}
		return array;
	}

	private void WritePapx(Stream stream)
	{
		int count = m_papxPositions.Count;
		int num = 0;
		uint num2 = m_fib.BaseReserved5;
		BinaryWriter writer = new BinaryWriter(stream);
		while (num < count)
		{
			ParagraphPropertiesPage paragraphPropertiesPage = new ParagraphPropertiesPage();
			num = FillPapxPage(paragraphPropertiesPage, num2, num);
			int papxPos = AlignByDiskPage(stream);
			m_tables.AddPapxRecord(num2, papxPos);
			paragraphPropertiesPage.SaveToStream(writer, stream);
			num2 = m_papxPositions[num - 1];
		}
	}

	private int FillPapxPage(ParagraphPropertiesPage page, uint pagePos, int papxIndex)
	{
		page.RunsCount = GetPapxCountPerPage(papxIndex);
		if (page.RunsCount == 0)
		{
			throw new Exception(string.Empty);
		}
		page.FileCharPos[0] = pagePos;
		int i = 0;
		for (int runsCount = page.RunsCount; i < runsCount; i++)
		{
			page.ParagraphProperties[i] = m_papxProps[papxIndex];
			page.FileCharPos[i + 1] = m_papxPositions[papxIndex];
			papxIndex++;
		}
		return papxIndex;
	}

	private int GetPapxCountPerPage(int papxIndex)
	{
		int num = 0;
		int count = m_papxPositions.Count;
		int i;
		for (i = papxIndex; i < count; i++)
		{
			ParagraphExceptionInDiskPage paragraphExceptionInDiskPage = m_papxProps[i];
			if (paragraphExceptionInDiskPage.Length + 13 + 8 + num > 511)
			{
				break;
			}
			num += paragraphExceptionInDiskPage.Length + 13 + 4;
		}
		return i - papxIndex;
	}

	private void WriteChpx(Stream stream)
	{
		int count = m_chpxPositions.Count;
		int num = 0;
		uint num2 = m_fib.BaseReserved5;
		BinaryWriter writer = new BinaryWriter(stream);
		while (num < count)
		{
			CharacterPropertiesPage characterPropertiesPage = new CharacterPropertiesPage();
			num = FillChpxPage(characterPropertiesPage, num2, num);
			int chpxPos = AlignByDiskPage(stream);
			m_tables.AddChpxRecord(num2, chpxPos);
			characterPropertiesPage.SaveToStream(writer, stream);
			num2 = m_chpxPositions[num - 1];
		}
	}

	private int FillChpxPage(CharacterPropertiesPage page, uint pagePos, int chpxIndex)
	{
		page.RunsCount = GetChpxCountPerPage(chpxIndex);
		page.FileCharPos[0] = pagePos;
		int i = 0;
		for (int runsCount = page.RunsCount; i < runsCount; i++)
		{
			page.CharacterProperties[i] = m_chpxProps[chpxIndex];
			page.FileCharPos[i + 1] = m_chpxPositions[chpxIndex];
			chpxIndex++;
		}
		return chpxIndex;
	}

	private int GetChpxCountPerPage(int chpxIndex)
	{
		int num = 0;
		int count = m_chpxPositions.Count;
		int i;
		for (i = chpxIndex; i < count; i++)
		{
			CharacterPropertyException ex = m_chpxProps[i];
			int num2 = ((ex.Length % 2 != 0) ? (ex.Length + 1) : ex.Length);
			if (IsChpxRepeats(chpxIndex, i))
			{
				if (num + 8 + 1 >= 511)
				{
					break;
				}
				num += 5;
			}
			else
			{
				if (num + num2 + 8 + 1 >= 511)
				{
					break;
				}
				num += num2 + 4 + 1;
			}
		}
		return i - chpxIndex;
	}

	internal bool IsChpxRepeats(int chpxIndex, int CurrentIndex)
	{
		CharacterPropertyException ex = m_chpxProps[CurrentIndex];
		bool result = false;
		for (int i = chpxIndex; i < CurrentIndex; i++)
		{
			CharacterPropertyException ex2 = m_chpxProps[i];
			if (ex.Length == ex2.Length && ex.ModifiersCount == ex2.ModifiersCount && (result = ex.Equals(ex2)))
			{
				break;
			}
		}
		return result;
	}

	private void WriteSepx(Stream stream)
	{
		m_secProperties = new SectionPropertyException[m_sepxPositions.Count];
		int i = 0;
		for (int count = m_sepxPositions.Count; i < count; i++)
		{
			int charPos = m_sepxPositions[i];
			SectionPropertyException ex = m_sepxProps[i];
			int sepxPos = AlignByDiskPage(stream);
			m_tables.AddSectionRecord(charPos, sepxPos);
			m_secProperties[i] = ex;
			ex.Save(stream);
		}
	}

	private int AlignByDiskPage(Stream stream)
	{
		int num = (int)stream.Position;
		int num2 = num / 512;
		if (num % 512 != 0)
		{
			num2++;
		}
		num = 512 * num2;
		while (stream.Position < num)
		{
			byte[] buffer = new byte[num - stream.Position];
			stream.Write(buffer, 0, (int)(num - stream.Position));
		}
		return num2;
	}
}
