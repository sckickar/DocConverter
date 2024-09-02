using System;

namespace DocGen.OfficeChart.Implementation.XmlSerialization;

internal class Relation : ICloneable
{
	private string m_strTarget;

	private string m_strType;

	private bool m_bIsExternal;

	public string Target => m_strTarget.Replace("/xl/", "../");

	public string Type => m_strType;

	public bool IsExternal => m_bIsExternal;

	private Relation()
	{
	}

	public Relation(string target, string type)
	{
		if (target == null)
		{
			throw new ArgumentNullException("target");
		}
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		m_strTarget = target;
		m_strType = type;
	}

	public Relation(string target, string type, bool isExternal)
		: this(target, type)
	{
		m_bIsExternal = isExternal;
	}

	public object Clone()
	{
		return MemberwiseClone();
	}
}
