using System;
using System.Collections.Generic;
using DocGen.OfficeChart.Parser;
using DocGen.OfficeChart.Parser.Biff_Records;
using DocGen.OfficeChart.Parser.Biff_Records.Charts;

namespace DocGen.OfficeChart.Implementation;

internal class ChartPageSetupImpl : PageSetupBaseImpl, IOfficeChartPageSetup, IPageSetupBase, IParentApplication
{
	private PrintedChartSizeRecord m_chartSize = (PrintedChartSizeRecord)BiffRecordFactory.GetRecord(TBIFFRecord.PrintedChartSize);

	public new bool FitToPagesTall
	{
		get
		{
			return m_setup.FitHeight != 0;
		}
		set
		{
			ushort num = (value ? ((ushort)1) : ((ushort)0));
			if (m_setup.FitHeight != num)
			{
				m_setup.FitHeight = num;
				SetChanged();
			}
		}
	}

	public new bool FitToPagesWide
	{
		get
		{
			return m_setup.FitWidth != 0;
		}
		set
		{
			ushort num = (value ? ((ushort)1) : ((ushort)0));
			if (m_setup.FitWidth != num)
			{
				m_setup.FitWidth = num;
				SetChanged();
			}
		}
	}

	public ChartPageSetupImpl(IApplication application, object parent)
		: base(application, parent)
	{
		FindParents();
	}

	[CLSCompliant(false)]
	public ChartPageSetupImpl(IApplication application, object parent, BiffReader reader)
		: base(application, parent)
	{
		FindParents();
		Parse(reader);
	}

	[CLSCompliant(false)]
	public ChartPageSetupImpl(IApplication application, object parent, IList<BiffRecordRaw> data, ref int position)
		: base(application, parent)
	{
		FindParents();
		position = Parse(data, position);
	}

	public ChartPageSetupImpl(IApplication application, object parent, List<BiffRecordRaw> data, ref int position)
		: base(application, parent)
	{
		FindParents();
		position = Parse(data, position);
	}

	[CLSCompliant(false)]
	protected override bool ParseRecord(BiffRecordRaw record)
	{
		bool flag = base.ParseRecord(record);
		if (!flag)
		{
			flag = true;
			if (record.TypeCode == TBIFFRecord.PrintedChartSize)
			{
				m_chartSize = (PrintedChartSizeRecord)record;
			}
			else
			{
				flag = false;
			}
		}
		return flag;
	}

	[CLSCompliant(false)]
	public void Parse(BiffReader reader)
	{
		throw new NotImplementedException();
	}

	[CLSCompliant(false)]
	protected override void SerializeEndRecords(OffsetArrayList records)
	{
		if (records == null)
		{
			throw new ArgumentNullException("records");
		}
		if (m_chartSize == null)
		{
			throw new ArgumentNullException("m_chartSize");
		}
		base.SerializeEndRecords(records);
		records.Add((BiffRecordRaw)m_chartSize.Clone());
	}

	public ChartPageSetupImpl Clone(object parent)
	{
		ChartPageSetupImpl obj = (ChartPageSetupImpl)MemberwiseClone();
		obj.SetParent(parent);
		obj.FindParents();
		m_arrFooters = CloneUtils.CloneStringArray(m_arrFooters);
		m_arrHeaders = CloneUtils.CloneStringArray(m_arrHeaders);
		m_chartSize = (PrintedChartSizeRecord)CloneUtils.CloneCloneable(m_chartSize);
		m_setup = (PrintSetupRecord)CloneUtils.CloneCloneable(m_setup);
		m_unknown = (PrinterSettingsRecord)CloneUtils.CloneCloneable(m_unknown);
		return obj;
	}
}
