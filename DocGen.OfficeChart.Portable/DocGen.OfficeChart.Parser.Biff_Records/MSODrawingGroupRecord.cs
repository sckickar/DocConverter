using System;
using System.Collections.Generic;
using System.IO;
using DocGen.OfficeChart.Parser.Biff_Records.MsoDrawing;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.MSODrawingGroup)]
[CLSCompliant(false)]
internal class MSODrawingGroupRecord : BiffContinueRecordRaw, ICloneable, IDisposable
{
	private const int DEF_DATA_OFFSET = 0;

	protected byte[] m_tempData;

	protected List<MsoBase> m_arrStructures = new List<MsoBase>();

	public MsoBase[] Structures => m_arrStructures.ToArray();

	public List<MsoBase> StructuresList => m_arrStructures;

	public override bool NeedDataArray => true;

	protected virtual int StructuresOffset => 0;

	public MSODrawingGroupRecord()
	{
	}

	protected override void OnDispose()
	{
		m_tempData = null;
		for (int i = 0; i < m_arrStructures.Count; i++)
		{
			m_arrStructures[i].Dispose();
		}
	}

	public MSODrawingGroupRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public MSODrawingGroupRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure()
	{
		if (m_extractor != null)
		{
			AddContinueRecordType(base.TypeCode);
			base.ParseStructure();
			m_tempData = new byte[m_data.Length];
			m_data.CopyTo(m_tempData, 0);
			ParseData();
		}
	}

	protected virtual void ParseData()
	{
		MemoryStream memoryStream = new MemoryStream(m_data);
		memoryStream.Position = StructuresOffset;
		int num = m_data.Length;
		while (memoryStream.Position < num)
		{
			MsoBase item = MsoFactory.CreateMsoRecord(null, memoryStream);
			m_arrStructures.Add(item);
		}
	}

	public override void InfillInternalData(OfficeVersion version)
	{
		if (m_arrStructures.Count > 0)
		{
			m_arrContinuePos.Clear();
			int iStartIndex;
			Stream stream = CreateDataList(out iStartIndex);
			FillDataList(stream, iStartIndex);
			int num = (int)stream.Length;
			m_tempData = ((MemoryStream)stream).ToArray();
			m_iLength = ((num > MaximumRecordSize) ? MaximumRecordSize : num);
			AutoGrowData = true;
			SetBytes(0, m_tempData, 0, m_iLength);
			base.InfillInternalData(version);
			if (num > MaximumRecordSize)
			{
				int iLength = m_iLength;
				int length = num - iLength;
				base.Builder.AppendBytes(m_tempData, iLength, length);
				m_iLength = base.Builder.Total;
			}
		}
	}

	protected virtual Stream CreateDataList(out int iStartIndex)
	{
		iStartIndex = 0;
		return new MemoryStream();
	}

	protected void FillDataList(Stream stream, int iStartIndex)
	{
		int count = m_arrStructures.Count;
		for (int i = 0; i < count; i++)
		{
			m_arrStructures[i].FillArray(stream);
		}
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		if (base.NeedInfill)
		{
			InfillInternalData(version);
			base.NeedInfill = false;
		}
		return m_iLength;
	}

	public void AddStructure(MsoBase item)
	{
		m_arrStructures.Add(item);
	}

	protected override ContinueRecordBuilder CreateBuilder()
	{
		ContinueRecordBuilder continueRecordBuilder = base.CreateBuilder();
		continueRecordBuilder.FirstContinueType = TBIFFRecord.MSODrawingGroup;
		return continueRecordBuilder;
	}

	public new object Clone()
	{
		MSODrawingGroupRecord mSODrawingGroupRecord = (MSODrawingGroupRecord)base.Clone();
		m_arrStructures = new List<MsoBase>(mSODrawingGroupRecord.m_arrStructures);
		return mSODrawingGroupRecord;
	}
}
