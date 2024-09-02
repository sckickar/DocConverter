using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[CLSCompliant(false)]
internal abstract class BiffContinueRecordRaw : BiffRecordRawWithArray
{
	protected ContinueRecordExtractor m_extractor;

	private ContinueRecordBuilder m_builder;

	protected internal List<int> m_arrContinuePos = new List<int>();

	private int m_iIntLen = -1;

	protected ContinueRecordBuilder Builder
	{
		get
		{
			if (m_builder == null)
			{
				throw new ArgumentNullException("Builder", "Class does not call parent method InfillInternalData.");
			}
			return m_builder;
		}
	}

	protected BiffContinueRecordRaw()
	{
	}

	protected BiffContinueRecordRaw(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	protected BiffContinueRecordRaw(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure()
	{
		ExtractContinueRecords();
	}

	public override void InfillInternalData(OfficeVersion version)
	{
		AutoGrowData = true;
		m_builder = CreateBuilder();
	}

	protected virtual ContinueRecordBuilder CreateBuilder()
	{
		ContinueRecordBuilder continueRecordBuilder = new ContinueRecordBuilder(this);
		continueRecordBuilder.OnFirstContinue += builder_OnFirstContinue;
		return continueRecordBuilder;
	}

	protected virtual bool ExtractContinueRecords()
	{
		if (m_extractor == null)
		{
			throw new ArgumentNullException("m_extractor");
		}
		m_extractor.StoreStreamPosition();
		int iLastPos = m_data.Length;
		m_arrContinuePos.Clear();
		m_arrContinuePos.Add(iLastPos);
		((IEnumerator)m_extractor).Reset();
		int iFullLength;
		List<byte[]> list = CollectRecordsData(out iFullLength, ref iLastPos);
		int count = list.Count;
		if (count > 0)
		{
			byte[] array = new byte[iFullLength + m_iLength];
			Buffer.BlockCopy(m_data, 0, array, 0, m_iLength);
			int num = m_iLength;
			for (int i = 0; i < count; i++)
			{
				byte[] array2 = list[i];
				int num2 = array2.Length;
				Buffer.BlockCopy(array2, 0, array, num, num2);
				num += num2;
			}
			m_data = array;
		}
		return count > 0;
	}

	protected List<byte[]> CollectRecordsData(out int iFullLength, ref int iLastPos)
	{
		((IEnumerator)m_extractor).Reset();
		List<byte[]> list = new List<byte[]>();
		iFullLength = 0;
		while (((IEnumerator)m_extractor).MoveNext())
		{
			int num = AddRecordData(list, m_extractor.Current);
			iLastPos += num;
			iFullLength += num;
			m_arrContinuePos.Add(iLastPos);
		}
		return list;
	}

	protected virtual int AddRecordData(List<byte[]> arrRecords, BiffRecordRaw record)
	{
		if (arrRecords == null)
		{
			throw new ArgumentNullException("arrRecords");
		}
		if (record == null)
		{
			throw new ArgumentNullException("record");
		}
		byte[] data = record.Data;
		arrRecords.Add(data);
		return data.Length;
	}

	protected virtual void builder_OnFirstContinue(object sender, EventArgs e)
	{
		ContinueRecordBuilder continueRecordBuilder = (ContinueRecordBuilder)sender;
		continueRecordBuilder.OnFirstContinue -= builder_OnFirstContinue;
		m_iIntLen = continueRecordBuilder.Position;
	}

	protected void AddContinueRecordType(TBIFFRecord recordType)
	{
		m_extractor.AddRecordType(recordType);
	}

	public override object Clone()
	{
		BiffContinueRecordRaw obj = (BiffContinueRecordRaw)base.Clone();
		obj.m_arrContinuePos = CloneUtils.CloneCloneable(m_arrContinuePos);
		return obj;
	}
}
