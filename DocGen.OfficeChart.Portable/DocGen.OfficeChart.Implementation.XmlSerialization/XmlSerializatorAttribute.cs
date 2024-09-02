using System;

namespace DocGen.OfficeChart.Implementation.XmlSerialization;

[AttributeUsage(AttributeTargets.Class)]
internal sealed class XmlSerializatorAttribute : Attribute
{
	private OfficeXmlSaveType m_saveType;

	public OfficeXmlSaveType SaveType => m_saveType;

	private XmlSerializatorAttribute()
	{
	}

	public XmlSerializatorAttribute(OfficeXmlSaveType saveType)
	{
		m_saveType = saveType;
	}
}
