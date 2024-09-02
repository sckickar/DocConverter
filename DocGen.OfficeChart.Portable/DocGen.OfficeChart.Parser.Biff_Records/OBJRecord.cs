using System;
using System.Collections.Generic;
using System.IO;
using DocGen.OfficeChart.Parser.Biff_Records.ObjRecords;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.OBJ)]
[CLSCompliant(false)]
internal class OBJRecord : BiffRecordRaw, ICloneable
{
	private List<ObjSubRecord> m_records = new List<ObjSubRecord>();

	public ObjSubRecord[] Records => m_records.ToArray();

	public List<ObjSubRecord> RecordsList => m_records;

	public override bool NeedDataArray => true;

	public OBJRecord()
	{
	}

	public OBJRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public OBJRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		int iStartOffset = iOffset;
		int num = iOffset + m_iLength;
		TObjType objectType = TObjType.otGroup;
		do
		{
			ObjSubRecord subRecord = GetSubRecord(provider, iOffset, iStartOffset, objectType);
			m_records.Add(subRecord);
			if (subRecord.Type == TObjSubRecordType.ftCmo)
			{
				objectType = ((ftCmo)subRecord).ObjectType;
			}
			iOffset += subRecord.Length + 4;
		}
		while (iOffset < num);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_iLength = 0;
		int i = 0;
		for (int count = m_records.Count; i < count; i++)
		{
			ObjSubRecord objSubRecord = m_records[i];
			objSubRecord.FillArray(provider, iOffset);
			int storeSize = objSubRecord.GetStoreSize(version);
			m_iLength += storeSize;
			iOffset += storeSize;
		}
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		int num = 0;
		int i = 0;
		for (int count = m_records.Count; i < count; i++)
		{
			ObjSubRecord objSubRecord = m_records[i];
			num += objSubRecord.GetStoreSize(version);
		}
		return num;
	}

	protected ObjSubRecord GetSubRecord(DataProvider provider, int offset, int iStartOffset, TObjType objectType)
	{
		TObjSubRecordType tObjSubRecordType = (TObjSubRecordType)provider.ReadInt16(offset);
		ushort num = provider.ReadUInt16(offset + 2);
		if (tObjSubRecordType == TObjSubRecordType.ftLbsData || num + offset + 4 > base.Length)
		{
			int num2 = 4;
			int iOffset = base.Length - 4;
			if (provider.ReadInt16(iOffset) == 0)
			{
				num2 += 4;
			}
			num = (ushort)(base.Length - offset - num2 + iStartOffset);
		}
		if (tObjSubRecordType == TObjSubRecordType.ftEnd)
		{
			num = 0;
		}
		byte[] array = new byte[num];
		provider.ReadArray(offset + 4, array);
		return tObjSubRecordType switch
		{
			TObjSubRecordType.ftCmo => new ftCmo(tObjSubRecordType, num, array), 
			TObjSubRecordType.ftEnd => new ftEnd(tObjSubRecordType, num, array), 
			TObjSubRecordType.ftNts => new ftNts(tObjSubRecordType, num, array), 
			TObjSubRecordType.ftSbs => new ftSbs(tObjSubRecordType, num, array), 
			TObjSubRecordType.ftSbsFormula => new ftSbsFormula(tObjSubRecordType, num, array), 
			TObjSubRecordType.ftLbsData => new ftLbsData(tObjSubRecordType, num, array, objectType), 
			TObjSubRecordType.ftCbls => new ftCbls(tObjSubRecordType, num, array), 
			TObjSubRecordType.ftCblsData => new ftCblsData(tObjSubRecordType, num, array), 
			TObjSubRecordType.ftCblsFmla => new ftCblsFmla(tObjSubRecordType, num, array), 
			TObjSubRecordType.ftMacro => new ftMacro(num, array), 
			TObjSubRecordType.ftRbo => new ftRbo(num, array), 
			TObjSubRecordType.ftRboData => new ftRboData(num, array), 
			TObjSubRecordType.ftCf => new ftCf(TObjSubRecordType.ftCf, num, array), 
			TObjSubRecordType.ftPioGrbit => new ftPioGrbit(TObjSubRecordType.ftPioGrbit, num, array), 
			_ => new ftUnknown(tObjSubRecordType, num, array), 
		};
	}

	public void AddSubRecord(ObjSubRecord record)
	{
		m_records.Add(record);
	}

	public ObjSubRecord FindSubRecord(TObjSubRecordType recordType)
	{
		int num = FindSubRecordIndex(recordType);
		if (num < 0)
		{
			return null;
		}
		return m_records[num];
	}

	public int FindSubRecordIndex(TObjSubRecordType recordType)
	{
		int result = -1;
		int i = 0;
		for (int count = m_records.Count; i < count; i++)
		{
			if (m_records[i].Type == recordType)
			{
				result = i;
				break;
			}
		}
		return result;
	}

	public new object Clone()
	{
		OBJRecord obj = (OBJRecord)base.Clone();
		obj.m_records = CloneUtils.CloneCloneable(m_records);
		return obj;
	}
}
