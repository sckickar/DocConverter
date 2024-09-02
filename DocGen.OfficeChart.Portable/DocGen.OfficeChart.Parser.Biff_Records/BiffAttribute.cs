using System;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
internal sealed class BiffAttribute : Attribute
{
	private TBIFFRecord m_code;

	public TBIFFRecord Code => m_code;

	private BiffAttribute()
	{
	}

	public BiffAttribute(TBIFFRecord code)
	{
		m_code = code;
	}
}
