using System;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
internal sealed class BiffOffsetsRecordsAttribute : Attribute
{
	private TBIFFRecord m_type;

	public TBIFFRecord OffsetsRecordsType => m_type;

	private BiffOffsetsRecordsAttribute()
	{
	}

	public BiffOffsetsRecordsAttribute(TBIFFRecord type)
	{
		m_type = type;
	}
}
