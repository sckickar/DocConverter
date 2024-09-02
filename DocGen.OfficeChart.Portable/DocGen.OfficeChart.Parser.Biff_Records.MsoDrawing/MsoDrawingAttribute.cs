using System;

namespace DocGen.OfficeChart.Parser.Biff_Records.MsoDrawing;

internal class MsoDrawingAttribute : Attribute
{
	private MsoRecords m_recordType;

	public MsoRecords RecordType => m_recordType;

	private MsoDrawingAttribute()
	{
	}

	public MsoDrawingAttribute(MsoRecords recordType)
	{
		m_recordType = recordType;
	}
}
