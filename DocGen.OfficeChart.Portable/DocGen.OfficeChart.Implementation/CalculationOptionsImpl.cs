using System;
using System.Collections;
using DocGen.OfficeChart.Interfaces;
using DocGen.OfficeChart.Parser;
using DocGen.OfficeChart.Parser.Biff_Records;

namespace DocGen.OfficeChart.Implementation;

internal class CalculationOptionsImpl : CommonObject, ICalculationOptions, IParentApplication, ICloneParent
{
	public static readonly TBIFFRecord[] DEF_CORRECT_CODES = new TBIFFRecord[6]
	{
		TBIFFRecord.CalcMode,
		TBIFFRecord.CalCount,
		TBIFFRecord.RefMode,
		TBIFFRecord.Iteration,
		TBIFFRecord.Delta,
		TBIFFRecord.SaveRecalc
	};

	private CalcModeRecord m_calcMode = (CalcModeRecord)BiffRecordFactory.GetRecord(TBIFFRecord.CalcMode);

	private CalcCountRecord m_calcCount = (CalcCountRecord)BiffRecordFactory.GetRecord(TBIFFRecord.CalCount);

	private RefModeRecord m_refMode = (RefModeRecord)BiffRecordFactory.GetRecord(TBIFFRecord.RefMode);

	private IterationRecord m_iteration = (IterationRecord)BiffRecordFactory.GetRecord(TBIFFRecord.Iteration);

	private DeltaRecord m_delta = (DeltaRecord)BiffRecordFactory.GetRecord(TBIFFRecord.Delta);

	private SaveRecalcRecord m_saveRecalc = (SaveRecalcRecord)BiffRecordFactory.GetRecord(TBIFFRecord.SaveRecalc);

	public int MaximumIteration
	{
		get
		{
			return m_calcCount.Iterations;
		}
		set
		{
			m_calcCount.Iterations = (ushort)value;
		}
	}

	public OfficeCalculationMode CalculationMode
	{
		get
		{
			return m_calcMode.CalculationMode;
		}
		set
		{
			m_calcMode.CalculationMode = value;
		}
	}

	public bool RecalcOnSave
	{
		get
		{
			return m_saveRecalc.RecalcOnSave == 1;
		}
		set
		{
			m_saveRecalc.RecalcOnSave = (value ? ((ushort)1) : ((ushort)0));
		}
	}

	public double MaximumChange
	{
		get
		{
			return m_delta.MaxChange;
		}
		set
		{
			m_delta.MaxChange = value;
		}
	}

	public bool IsIterationEnabled
	{
		get
		{
			return m_iteration.IsIteration == 1;
		}
		set
		{
			m_iteration.IsIteration = (value ? ((ushort)1) : ((ushort)0));
		}
	}

	public bool R1C1ReferenceMode
	{
		get
		{
			return m_refMode.IsA1ReferenceMode == 0;
		}
		set
		{
			m_refMode.IsA1ReferenceMode = ((!value) ? ((ushort)1) : ((ushort)0));
		}
	}

	public CalculationOptionsImpl(IApplication application, object parent)
		: base(application, parent)
	{
	}

	[CLSCompliant(false)]
	public CalculationOptionsImpl(IApplication application, object parent, BiffRecordRaw[] data, int iPos)
		: this(application, parent)
	{
		Parse(data, iPos);
	}

	public int Parse(IList data, int iPos)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		int count = data.Count;
		if (iPos < 0 || iPos >= count)
		{
			throw new ArgumentOutOfRangeException("iPos", "Value cannot be less than 0 and greater than data.Length");
		}
		while (iPos < count)
		{
			BiffRecordRaw biffRecordRaw = (BiffRecordRaw)data[iPos];
			switch (biffRecordRaw.TypeCode)
			{
			case TBIFFRecord.CalcMode:
				m_calcMode = (CalcModeRecord)biffRecordRaw;
				break;
			case TBIFFRecord.CalCount:
				m_calcCount = (CalcCountRecord)biffRecordRaw;
				break;
			case TBIFFRecord.RefMode:
				m_refMode = (RefModeRecord)biffRecordRaw;
				break;
			case TBIFFRecord.Iteration:
				m_iteration = (IterationRecord)biffRecordRaw;
				break;
			case TBIFFRecord.Delta:
				m_delta = (DeltaRecord)biffRecordRaw;
				break;
			case TBIFFRecord.SaveRecalc:
				m_saveRecalc = (SaveRecalcRecord)biffRecordRaw;
				break;
			default:
				return iPos;
			}
			iPos++;
		}
		return iPos;
	}

	[CLSCompliant(false)]
	public void Serialize(OffsetArrayList records)
	{
		if (records == null)
		{
			throw new ArgumentNullException("records");
		}
		records.Add(m_calcMode.Clone());
		records.Add(m_calcCount.Clone());
		records.Add(m_refMode.Clone());
		records.Add(m_iteration.Clone());
		records.Add(m_delta.Clone());
		records.Add(m_saveRecalc.Clone());
	}

	public object Clone(object parent)
	{
		CalculationOptionsImpl obj = (CalculationOptionsImpl)MemberwiseClone();
		obj.SetParent(parent);
		obj.m_calcMode = (CalcModeRecord)CloneUtils.CloneCloneable(m_calcMode);
		obj.m_calcCount = (CalcCountRecord)CloneUtils.CloneCloneable(m_calcCount);
		obj.m_refMode = (RefModeRecord)CloneUtils.CloneCloneable(m_refMode);
		obj.m_iteration = (IterationRecord)CloneUtils.CloneCloneable(m_iteration);
		obj.m_delta = (DeltaRecord)CloneUtils.CloneCloneable(m_delta);
		obj.m_saveRecalc = (SaveRecalcRecord)CloneUtils.CloneCloneable(m_saveRecalc);
		return obj;
	}
}
