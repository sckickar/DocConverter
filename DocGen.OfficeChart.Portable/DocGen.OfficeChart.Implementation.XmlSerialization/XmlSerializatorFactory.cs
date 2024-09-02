using System;
using System.Collections.Generic;
using DocGen.OfficeChart.Interfaces.XmlSerialization;

namespace DocGen.OfficeChart.Implementation.XmlSerialization;

internal sealed class XmlSerializatorFactory
{
	private static Dictionary<int, IXmlSerializator> s_dicSerializators;

	static XmlSerializatorFactory()
	{
		s_dicSerializators = new Dictionary<int, IXmlSerializator>();
		RegisterXmlSerializator(OfficeXmlSaveType.DLS, typeof(DLSXmlSerializator));
		RegisterXmlSerializator(OfficeXmlSaveType.MSExcel, typeof(WorkbookXmlSerializator));
	}

	private XmlSerializatorFactory()
	{
	}

	public static void RegisterXmlSerializator(OfficeXmlSaveType saveType, Type type)
	{
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		IXmlSerializator value = (IXmlSerializator)Activator.CreateInstance(type);
		s_dicSerializators.Add((int)saveType, value);
	}

	public static IXmlSerializator GetSerializator(OfficeXmlSaveType saveType)
	{
		return s_dicSerializators[(int)saveType];
	}
}
