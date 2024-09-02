using System;
using System.Collections.Generic;
using DocGen.OfficeChart.Parser;
using DocGen.OfficeChart.Parser.Biff_Records;
using DocGen.OfficeChart.Parser.Biff_Records.Charts;

namespace DocGen.OfficeChart.Implementation.Charts;

internal class ChartWrappedFrameFormatImpl : ChartFrameFormatImpl
{
	public ChartWrappedFrameFormatImpl(IApplication application, object parent)
		: base(application, parent)
	{
	}

	[CLSCompliant(false)]
	protected override bool CheckBegin(BiffRecordRaw record)
	{
		record = UnwrapRecord(record);
		return base.CheckBegin(record);
	}

	[CLSCompliant(false)]
	protected override void ParseRecord(BiffRecordRaw record, ref int iBeginCounter)
	{
		record = UnwrapRecord(record);
		base.ParseRecord(record, ref iBeginCounter);
	}

	[CLSCompliant(false)]
	protected override BiffRecordRaw UnwrapRecord(BiffRecordRaw record)
	{
		if (record == null)
		{
			throw new ArgumentNullException("record");
		}
		if (record.TypeCode == TBIFFRecord.ChartWrapper)
		{
			return ((ChartWrapperRecord)record).Record;
		}
		return record;
	}

	[CLSCompliant(false)]
	protected override void SerializeRecord(IList<IBiffStorage> list, BiffRecordRaw record)
	{
		if (list == null)
		{
			throw new ArgumentNullException("list");
		}
		if (record == null)
		{
			throw new ArgumentNullException("record");
		}
		ChartWrapperRecord chartWrapperRecord = (ChartWrapperRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartWrapper);
		chartWrapperRecord.Record = record;
		list.Add((BiffRecordRaw)chartWrapperRecord.Clone());
	}
}
