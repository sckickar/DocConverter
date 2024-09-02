using System;
using System.IO;
using DocGen.DocIO.ReaderWriter.Biff_Records.Structures;

namespace DocGen.DocIO.ReaderWriter.Biff_Records;

[CLSCompliant(false)]
internal class ParagraphPropertiesPage : BaseWordRecord
{
	private const int DEF_FC_SIZE = 4;

	private uint[] m_arrFC;

	private BXStructure[] m_arrHeight;

	private ParagraphExceptionInDiskPage[] m_arrPAPX;

	internal uint[] FileCharPos => m_arrFC;

	internal BXStructure[] Heights => m_arrHeight;

	internal ParagraphExceptionInDiskPage[] ParagraphProperties => m_arrPAPX;

	internal int RunsCount
	{
		get
		{
			if (m_arrPAPX == null)
			{
				return 0;
			}
			return m_arrPAPX.Length;
		}
		set
		{
			if (value < 0)
			{
				throw new ArgumentOutOfRangeException("RunsCount");
			}
			if (RunsCount != value)
			{
				m_arrPAPX = new ParagraphExceptionInDiskPage[value];
				m_arrFC = new uint[value + 1];
				m_arrHeight = new BXStructure[value];
			}
		}
	}

	internal override int Length => 512;

	internal ParagraphPropertiesPage()
	{
	}

	internal ParagraphPropertiesPage(FKPStructure structure)
	{
		Parse(structure);
	}

	internal override void Close()
	{
		base.Close();
		if (m_arrHeight != null)
		{
			m_arrHeight = null;
		}
		if (m_arrPAPX != null)
		{
			m_arrPAPX = null;
		}
	}

	private void Parse(FKPStructure structure)
	{
		byte[] pageData = structure.PageData;
		m_arrFC = new uint[structure.Count + 1];
		int num = (structure.Count + 1) * 4;
		Buffer.BlockCopy(pageData, 0, m_arrFC, 0, num);
		m_arrPAPX = new ParagraphExceptionInDiskPage[structure.Count];
		m_arrHeight = new BXStructure[structure.Count];
		int num2 = num;
		for (int i = 0; i < structure.Count; i++)
		{
			m_arrHeight[i] = new BXStructure();
			m_arrHeight[i].Parse(pageData, num2);
			num2 += 13;
		}
		for (int j = 0; j < structure.Count; j++)
		{
			num2 = m_arrHeight[j].Offset * 2;
			m_arrPAPX[j] = new ParagraphExceptionInDiskPage();
			m_arrPAPX[j].Parse(pageData, num2);
		}
	}

	internal FKPStructure Save()
	{
		FKPStructure fKPStructure = new FKPStructure();
		int runsCount = RunsCount;
		int num = m_arrFC.Length * 4;
		fKPStructure.Count = (byte)runsCount;
		Buffer.BlockCopy(m_arrFC, 0, fKPStructure.PageData, 0, num);
		int num2 = num;
		byte b = byte.MaxValue;
		for (int i = 0; i < runsCount; i++)
		{
			if (m_arrPAPX[i] != null)
			{
				b -= (byte)(m_arrPAPX[i].Length / 2);
				if (m_arrHeight[i] == null)
				{
					m_arrHeight[i] = new BXStructure();
				}
				BXStructure bXStructure = m_arrHeight[i];
				bXStructure.Offset = b;
				bXStructure.Save(fKPStructure.PageData, num2);
				num2 += 13;
				m_arrPAPX[i].Save(fKPStructure.PageData, bXStructure.Offset * 2);
			}
		}
		return fKPStructure;
	}

	internal override int Save(byte[] arrData, int iOffset)
	{
		if (arrData == null)
		{
			throw new ArgumentNullException("arrData");
		}
		if (iOffset < 0 || iOffset + 512 > arrData.Length)
		{
			throw new ArgumentOutOfRangeException("iOffset");
		}
		Save().Save(arrData, iOffset);
		return 512;
	}

	internal void SaveToStream(BinaryWriter writer, Stream stream)
	{
		long position = stream.Position;
		int runsCount = RunsCount;
		int num = m_arrFC.Length * 4;
		int i = 0;
		for (int num2 = m_arrFC.Length; i < num2; i++)
		{
			writer.Write(m_arrFC[i]);
		}
		int num3 = num;
		byte b = byte.MaxValue;
		for (int j = 0; j < runsCount; j++)
		{
			if (m_arrPAPX[j] != null)
			{
				b -= (byte)(m_arrPAPX[j].Length / 2);
				if (m_arrHeight[j] == null)
				{
					m_arrHeight[j] = new BXStructure();
				}
				BXStructure bXStructure = m_arrHeight[j];
				bXStructure.Offset = b;
				stream.Position = position + num3;
				bXStructure.Save(writer);
				num3 += 13;
				stream.Position = position + bXStructure.Offset * 2;
				m_arrPAPX[j].Save(writer, stream);
			}
		}
		stream.Position = position + 511;
		stream.WriteByte((byte)runsCount);
	}
}
