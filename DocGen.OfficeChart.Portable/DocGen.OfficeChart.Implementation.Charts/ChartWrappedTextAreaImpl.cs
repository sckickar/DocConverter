using System;
using System.Collections.Generic;
using DocGen.OfficeChart.Interfaces;
using DocGen.OfficeChart.Parser;
using DocGen.OfficeChart.Parser.Biff_Records;
using DocGen.OfficeChart.Parser.Biff_Records.Charts;

namespace DocGen.OfficeChart.Implementation.Charts;

internal class ChartWrappedTextAreaImpl : ChartTextAreaImpl, ISerializable
{
	private static readonly byte[][] DEF_UNKNOWN_START = new byte[4][]
	{
		new byte[20]
		{
			80, 8, 0, 0, 10, 10, 3, 0, 80, 8,
			90, 8, 97, 8, 97, 8, 106, 8, 107, 8
		},
		new byte[12]
		{
			82, 8, 0, 0, 13, 0, 0, 0, 0, 0,
			0, 0
		},
		new byte[12]
		{
			106, 8, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0
		},
		new byte[12]
		{
			84, 8, 0, 0, 18, 0, 0, 0, 0, 0,
			0, 0
		}
	};

	private static readonly byte[][] DEF_UNKNOWN_END = new byte[2][]
	{
		new byte[12]
		{
			85, 8, 0, 0, 18, 0, 0, 0, 0, 0,
			0, 0
		},
		new byte[12]
		{
			83, 8, 0, 0, 13, 0, 0, 0, 0, 0,
			0, 0
		}
	};

	protected override bool ShouldSerialize => true;

	public ChartWrappedTextAreaImpl(IApplication application, object parent)
		: base(application, parent)
	{
	}

	public ChartWrappedTextAreaImpl(IApplication application, object parent, IList<BiffRecordRaw> data, ref int iPos)
		: base(application, parent, data, ref iPos)
	{
	}

	[CLSCompliant(false)]
	public ChartWrappedTextAreaImpl(IApplication application, object parent, ExcelObjectTextLink textLink)
		: base(application, parent, textLink)
	{
	}

	protected override ChartFrameFormatImpl CreateFrameFormat()
	{
		return new ChartWrappedFrameFormatImpl(base.Application, this);
	}

	[CLSCompliant(false)]
	protected override void SerializeRecord(IList<IBiffStorage> records, BiffRecordRaw record)
	{
		if (records == null)
		{
			throw new ArgumentNullException("records");
		}
		if (record == null)
		{
			throw new ArgumentNullException("record");
		}
		if (record.TypeCode == TBIFFRecord.ChartDataLabels)
		{
			base.SerializeRecord(records, record);
			return;
		}
		ChartWrapperRecord chartWrapperRecord = (ChartWrapperRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartWrapper);
		chartWrapperRecord.Record = (BiffRecordRaw)record.Clone();
		records.Add(chartWrapperRecord);
	}

	private void SerializeUnknown(OffsetArrayList records, byte[][] arrUnknown)
	{
		if (arrUnknown == null)
		{
			throw new ArgumentNullException("arrUnknown");
		}
		if (records == null)
		{
			throw new ArgumentNullException("records");
		}
		int i = 0;
		for (int num = arrUnknown.Length; i < num; i++)
		{
			byte[] array = arrUnknown[i];
			if (array == null)
			{
				throw new ArgumentNullException("arrData");
			}
			int num2 = array.Length;
			UnknownRecord unknownRecord = (UnknownRecord)BiffRecordFactory.GetRecord(TBIFFRecord.Unknown);
			unknownRecord.RecordCode = BitConverter.ToUInt16(array, 0);
			unknownRecord.m_data = new byte[num2];
			unknownRecord.DataLen = num2;
			array.CopyTo(unknownRecord.m_data, 0);
			records.Add(unknownRecord);
		}
	}
}
