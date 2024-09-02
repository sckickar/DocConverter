using System;
using System.IO;
using DocGen.Drawing;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.Palette)]
[CLSCompliant(false)]
internal class PaletteRecord : BiffRecordRaw
{
	public struct TColor
	{
		public byte R;

		public byte G;

		public byte B;

		public byte A;

		public override string ToString()
		{
			return Color.FromArgb(A, R, G, B).ToString();
		}
	}

	[BiffRecordPos(0, 2)]
	private ushort m_usColorsCount = 56;

	private TColor[] m_arrColor;

	public ushort ColorsCount => m_usColorsCount;

	public TColor[] Colors
	{
		get
		{
			return m_arrColor;
		}
		set
		{
			m_arrColor = value;
			m_usColorsCount = (ushort)((value != null) ? ((ushort)value.Length) : 0);
		}
	}

	public override int MinimumRecordSize => 2;

	public override int MaximumRecordSize => 226;

	public PaletteRecord()
	{
	}

	public PaletteRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public PaletteRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_usColorsCount = provider.ReadUInt16(iOffset);
		m_arrColor = new TColor[m_usColorsCount];
		iOffset += 2;
		int num = 0;
		while (num < m_usColorsCount)
		{
			m_arrColor[num].R = provider.ReadByte(iOffset);
			m_arrColor[num].G = provider.ReadByte(iOffset + 1);
			m_arrColor[num].B = provider.ReadByte(iOffset + 2);
			m_arrColor[num].A = provider.ReadByte(iOffset + 3);
			num++;
			iOffset += 4;
		}
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		provider.WriteUInt16(iOffset, m_usColorsCount);
		m_iLength = 2;
		for (int i = 0; i < m_usColorsCount; i++)
		{
			provider.WriteByte(iOffset + m_iLength, m_arrColor[i].R);
			m_iLength++;
			provider.WriteByte(iOffset + m_iLength, m_arrColor[i].G);
			m_iLength++;
			provider.WriteByte(iOffset + m_iLength, m_arrColor[i].B);
			m_iLength++;
			provider.WriteByte(iOffset + m_iLength, m_arrColor[i].A);
			m_iLength++;
		}
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 2 + m_usColorsCount * 4;
	}
}
