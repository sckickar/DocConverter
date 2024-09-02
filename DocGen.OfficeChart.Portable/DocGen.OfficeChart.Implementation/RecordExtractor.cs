using System;
using System.Collections.Generic;
using DocGen.OfficeChart.Parser;
using DocGen.OfficeChart.Parser.Biff_Records;

namespace DocGen.OfficeChart.Implementation;

internal class RecordExtractor
{
	private Dictionary<int, BiffRecordRaw> m_dictRecords;

	public RecordExtractor()
	{
		m_dictRecords = new Dictionary<int, BiffRecordRaw>();
	}

	public BiffRecordRaw GetRecord(DataProvider provider, int iOffset, OfficeVersion version)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		int recordType = provider.ReadInt16(iOffset);
		iOffset += 2;
		BiffRecordRaw record = GetRecord(recordType);
		int iLength = (record.Length = provider.ReadInt16(iOffset));
		iOffset += 2;
		record.ParseStructure(provider, iOffset, iLength, version);
		return record;
	}

	public BiffRecordRaw GetRecord(int recordType)
	{
		if (!m_dictRecords.TryGetValue(recordType, out var value))
		{
			value = BiffRecordFactory.GetRecord(recordType);
			m_dictRecords.Add(recordType, value);
		}
		return value;
	}
}
