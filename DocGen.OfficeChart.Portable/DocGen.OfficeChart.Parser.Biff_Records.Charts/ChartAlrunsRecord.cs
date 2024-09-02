using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records.Charts;

[Biff(TBIFFRecord.ChartAlruns)]
[CLSCompliant(false)]
internal class ChartAlrunsRecord : BiffRecordRaw
{
	public class TRuns : ICloneable
	{
		internal const int Size = 4;

		private ushort m_usFirstChar;

		private ushort m_usFontIndex;

		private bool m_newParagraphStart;

		public ushort FirstCharIndex
		{
			get
			{
				return m_usFirstChar;
			}
			set
			{
				m_usFirstChar = value;
			}
		}

		public ushort FontIndex
		{
			get
			{
				return m_usFontIndex;
			}
			set
			{
				m_usFontIndex = value;
			}
		}

		internal bool HasNewParagarphStart
		{
			get
			{
				return m_newParagraphStart;
			}
			set
			{
				m_newParagraphStart = value;
			}
		}

		public TRuns(ushort firstChar, ushort fontIndex)
		{
			m_usFirstChar = firstChar;
			m_usFontIndex = fontIndex;
		}

		public object Clone()
		{
			return MemberwiseClone();
		}
	}

	[BiffRecordPos(0, 2)]
	private ushort m_usQuantity;

	private TRuns[] m_array = new TRuns[0];

	public ushort Quantity
	{
		get
		{
			return m_usQuantity;
		}
		set
		{
			if (value != m_usQuantity)
			{
				m_usQuantity = value;
			}
		}
	}

	public TRuns[] Runs
	{
		get
		{
			return m_array;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			m_array = value;
			m_usQuantity = (ushort)m_array.Length;
		}
	}

	public ChartAlrunsRecord()
	{
	}

	public ChartAlrunsRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public ChartAlrunsRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_usQuantity = provider.ReadUInt16(iOffset);
		m_array = new TRuns[m_usQuantity];
		int num = iOffset + 2;
		int num2 = 0;
		while (num2 < m_usQuantity)
		{
			m_array[num2] = new TRuns(provider.ReadUInt16(num), provider.ReadUInt16(num + 2));
			num2++;
			num += 4;
		}
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		provider.WriteUInt16(iOffset, m_usQuantity);
		int num = iOffset + 2;
		int num2 = 0;
		while (num2 < m_usQuantity)
		{
			provider.WriteUInt16(num, m_array[num2].FirstCharIndex);
			provider.WriteUInt16(num + 2, m_array[num2].FontIndex);
			num2++;
			num += 4;
		}
		m_iLength = num;
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 2 + m_array.Length * 4;
	}

	public override object Clone()
	{
		ChartAlrunsRecord chartAlrunsRecord = (ChartAlrunsRecord)base.Clone();
		if (m_array == null)
		{
			return chartAlrunsRecord;
		}
		int num = m_array.Length;
		chartAlrunsRecord.m_array = new TRuns[num];
		for (int i = 0; i < num; i++)
		{
			chartAlrunsRecord.m_array[i] = (TRuns)CloneUtils.CloneCloneable(m_array[i]);
		}
		return chartAlrunsRecord;
	}
}
