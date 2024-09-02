using System;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
[CLSCompliant(false)]
internal sealed class BiffOffsetOrderAttribute : Attribute
{
	private TBIFFRecord[] m_order;

	public TBIFFRecord[] OrderArray => m_order;

	private BiffOffsetOrderAttribute()
	{
	}

	public BiffOffsetOrderAttribute(params TBIFFRecord[] order)
	{
		m_order = order;
	}
}
