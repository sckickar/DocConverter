using System;
using DocGen.OfficeChart.Parser;
using DocGen.OfficeChart.Parser.Biff_Records;

namespace DocGen.OfficeChart.Implementation;

internal class WorksheetCustomProperty : ICustomProperty, ICloneable
{
	private CustomPropertyRecord m_record;

	public string Name => m_record.Name;

	public string Value
	{
		get
		{
			return m_record.Value;
		}
		set
		{
			m_record.Value = value;
		}
	}

	private WorksheetCustomProperty()
	{
	}

	public WorksheetCustomProperty(string strName)
	{
		m_record = (CustomPropertyRecord)BiffRecordFactory.GetRecord(TBIFFRecord.CustomProperty);
		m_record.Name = strName;
	}

	[CLSCompliant(false)]
	public WorksheetCustomProperty(CustomPropertyRecord property)
	{
		if (property == null)
		{
			throw new ArgumentNullException("property");
		}
		m_record = property;
	}

	[CLSCompliant(false)]
	public void Serialize(OffsetArrayList records)
	{
		if (records == null)
		{
			throw new ArgumentNullException("records");
		}
		records.Add(m_record);
	}

	public object Clone()
	{
		WorksheetCustomProperty obj = (WorksheetCustomProperty)MemberwiseClone();
		obj.m_record = (CustomPropertyRecord)CloneUtils.CloneCloneable(m_record);
		return obj;
	}
}
