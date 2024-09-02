using System;

namespace DocGen.OfficeChart.Parser.Biff_Records.Formula;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
internal sealed class ReferenceIndexAttribute : Attribute
{
	private int m_iIndex = 1;

	private int[] m_arrIndex;

	private Type m_TargetType;

	public int Index => m_iIndex;

	public int this[int index]
	{
		get
		{
			if (m_arrIndex != null && index >= 0 && index < m_arrIndex.Length)
			{
				return m_arrIndex[index];
			}
			return Index;
		}
	}

	public Type TargetType => m_TargetType;

	public int Count
	{
		get
		{
			if (m_arrIndex == null)
			{
				return 0;
			}
			return m_arrIndex.Length;
		}
	}

	private ReferenceIndexAttribute()
	{
	}

	public ReferenceIndexAttribute(int index)
	{
		if (index < 1 || index > 3)
		{
			throw new ArgumentOutOfRangeException();
		}
		m_iIndex = index;
		m_TargetType = typeof(RefPtg);
	}

	public ReferenceIndexAttribute(params int[] arrParams)
		: this(typeof(RefPtg), arrParams)
	{
	}

	public ReferenceIndexAttribute(Type targetType, params int[] arrParams)
	{
		m_arrIndex = new int[arrParams.Length];
		arrParams.CopyTo(m_arrIndex, 0);
		m_TargetType = targetType;
	}

	public ReferenceIndexAttribute(Type targetType, int index)
	{
		m_TargetType = targetType;
		m_iIndex = index;
	}
}
