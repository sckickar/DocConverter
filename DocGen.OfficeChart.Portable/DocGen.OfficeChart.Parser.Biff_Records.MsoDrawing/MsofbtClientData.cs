using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DocGen.OfficeChart.Implementation;

namespace DocGen.OfficeChart.Parser.Biff_Records.MsoDrawing;

[MsoDrawing(MsoRecords.msofbtClientData)]
[CLSCompliant(false)]
internal class MsofbtClientData : MsoBase
{
	private List<BiffRecordRaw> m_arrAdditionalData = new List<BiffRecordRaw>();

	public OBJRecord ObjectRecord
	{
		get
		{
			if (m_arrAdditionalData.Count > 0 && m_arrAdditionalData[0] is OBJRecord)
			{
				return (OBJRecord)m_arrAdditionalData[0];
			}
			return null;
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public BiffRecordRaw[] AdditionalData
	{
		get
		{
			if (m_arrAdditionalData == null)
			{
				return null;
			}
			return m_arrAdditionalData.ToArray();
		}
	}

	public MsofbtClientData(MsoBase parent)
		: base(parent)
	{
	}

	public MsofbtClientData(MsoBase parent, byte[] data, int iOffset)
		: base(parent, data, iOffset)
	{
	}

	public MsofbtClientData(MsoBase parent, byte[] data, int iOffset, GetNextMsoDrawingData dataGetter)
		: base(parent, data, iOffset, dataGetter)
	{
		BiffRecordRaw[] array = dataGetter();
		if (array == null)
		{
			throw new ArgumentException("Additional data can't be null");
		}
		m_arrAdditionalData.Clear();
		m_arrAdditionalData.AddRange(array);
		int num = m_arrAdditionalData.Count - 1;
		while (num >= 0 && m_arrAdditionalData[num] is NoteRecord)
		{
			m_arrAdditionalData.RemoveAt(num);
			num--;
		}
	}

	public override void InfillInternalData(Stream stream, int iOffset, List<int> arrBreaks, List<List<BiffRecordRaw>> arrRecords)
	{
		m_iLength = 0;
		if (arrBreaks != null && arrRecords != null)
		{
			arrBreaks.Add(m_iLength + iOffset);
			arrRecords.Add(m_arrAdditionalData);
		}
	}

	public override void ParseStructure(Stream stream)
	{
	}

	protected override object InternalClone()
	{
		MsofbtClientData obj = (MsofbtClientData)base.InternalClone();
		obj.m_arrAdditionalData = CloneUtils.CloneCloneable(m_arrAdditionalData);
		return obj;
	}

	public override void UpdateNextMsoDrawingData()
	{
		BiffRecordRaw[] array = base.DataGetter();
		if (array == null)
		{
			throw new ArgumentException("Additional data can't be null");
		}
		m_arrAdditionalData.Clear();
		m_arrAdditionalData.AddRange(array);
		int num = m_arrAdditionalData.Count - 1;
		while (num >= 0 && m_arrAdditionalData[num] is NoteRecord)
		{
			m_arrAdditionalData.RemoveAt(num);
			num--;
		}
	}

	public void AddRecord(BiffRecordRaw record)
	{
		m_arrAdditionalData.Add(record);
	}

	public void AddRecordRange(ICollection<BiffRecordRaw> records)
	{
		m_arrAdditionalData.AddRange(records);
	}

	public void AddRecordRange(IList records)
	{
		int i = 0;
		for (int count = records.Count; i < count; i++)
		{
			m_arrAdditionalData.Add(records[i] as BiffRecordRaw);
		}
	}
}
