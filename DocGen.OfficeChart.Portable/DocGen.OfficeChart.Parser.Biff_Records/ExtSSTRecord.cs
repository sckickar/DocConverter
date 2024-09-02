using System;
using System.IO;
using DocGen.OfficeChart.Implementation.Exceptions;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.ExtSST)]
[CLSCompliant(false)]
internal class ExtSSTRecord : BiffRecordRawWithArray
{
	private const int DEF_FIXED_SIZE = 2;

	private const int DEF_SUB_ITEM_SIZE = 8;

	[BiffRecordPos(0, 2)]
	private ushort m_usStringPerBucket = 8;

	private ExtSSTInfoSubRecord[] m_arrSSTInfo;

	private bool m_bIsEnd;

	private SSTRecord m_sst;

	public ushort StringPerBucket
	{
		get
		{
			return m_usStringPerBucket;
		}
		set
		{
			m_usStringPerBucket = value;
		}
	}

	public ExtSSTInfoSubRecord[] SSTInfo
	{
		get
		{
			return m_arrSSTInfo;
		}
		set
		{
			m_arrSSTInfo = value;
		}
	}

	public override int MinimumRecordSize => 0;

	public bool IsEnd => m_bIsEnd;

	public SSTRecord SST
	{
		get
		{
			return m_sst;
		}
		set
		{
			m_sst = value;
		}
	}

	public ExtSSTRecord()
	{
	}

	public ExtSSTRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public ExtSSTRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure()
	{
		m_usStringPerBucket = GetUInt16(0);
		int num = (m_iLength - 2) / 8;
		int num2 = m_iLength - 2;
		if (num2 % 8 != 0)
		{
			if (num2 % 4 != 0)
			{
				int @int = GetInt32(m_iLength - 4);
				m_bIsEnd = @int == 10;
				if (m_bIsEnd)
				{
					throw new WrongBiffRecordDataException("ExtSSTRecord's data size minus 2 must be divided by 8.");
				}
			}
			else
			{
				int int2 = GetInt32(m_iLength - 4);
				m_bIsEnd = int2 == 10;
			}
		}
		m_arrSSTInfo = new ExtSSTInfoSubRecord[num];
		int num3 = 2;
		using ByteArrayDataProvider arrData = new ByteArrayDataProvider(m_data);
		int num4 = 0;
		while (num4 < num)
		{
			ExtSSTInfoSubRecord extSSTInfoSubRecord = (ExtSSTInfoSubRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ExtSSTInfoSub);
			extSSTInfoSubRecord.StreamPos = StreamPos + num3;
			extSSTInfoSubRecord.ParseStructure(arrData, num3, 8, OfficeVersion.Excel97to2003);
			m_arrSSTInfo[num4] = extSSTInfoSubRecord;
			num4++;
			num3 += 8;
		}
	}

	public override void InfillInternalData(OfficeVersion version)
	{
		m_data = new byte[GetStoreSize(OfficeVersion.Excel97to2003)];
		SetUInt16(0, m_usStringPerBucket);
		m_iLength = 2;
		if (m_arrSSTInfo != null)
		{
			int num = 0;
			int num2 = m_arrSSTInfo.Length;
			while (num < num2)
			{
				m_arrSSTInfo[num].StreamPos = m_iLength;
				SetBytes(m_iLength, m_arrSSTInfo[num].Data, 0, 8);
				num++;
				m_iLength += 8;
			}
		}
	}

	public void UpdateStringOffsets()
	{
		int num = (int)m_sst.StreamPos;
		int numberOfUniqueStrings = (int)m_sst.NumberOfUniqueStrings;
		if (numberOfUniqueStrings > 0)
		{
			int[] stringsOffsets = m_sst.StringsOffsets;
			int[] stringsStreamPos = m_sst.StringsStreamPos;
			int num2 = 0;
			int num3 = 0;
			while (num2 < numberOfUniqueStrings)
			{
				int num4 = stringsOffsets[num2];
				ExtSSTInfoSubRecord obj = m_arrSSTInfo[num3];
				obj.StreamPosition = num + stringsStreamPos[num2];
				obj.BucketSSTOffset = (ushort)num4;
				num2 += StringPerBucket;
				num3++;
			}
		}
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		int num = ((m_arrSSTInfo != null) ? m_arrSSTInfo.Length : 0);
		return 2 + num * 8;
	}
}
