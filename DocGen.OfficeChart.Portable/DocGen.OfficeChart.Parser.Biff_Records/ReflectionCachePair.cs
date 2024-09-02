using System;
using System.Reflection;

namespace DocGen.OfficeChart.Parser.Biff_Records;

internal class ReflectionCachePair
{
	private BiffRecordPosAttribute[] m_key;

	private FieldInfo[] m_tag;

	public BiffRecordPosAttribute[] Key
	{
		get
		{
			return m_key;
		}
		set
		{
			if (value != m_key)
			{
				m_key = value;
				OnKeyChanged();
			}
		}
	}

	public FieldInfo[] Tag
	{
		get
		{
			return m_tag;
		}
		set
		{
			if (value != m_tag)
			{
				m_tag = value;
				OnTagChanged();
			}
		}
	}

	public event EventHandler KeyChanged;

	public event EventHandler TagChanged;

	private void OnKeyChanged()
	{
		if (this.KeyChanged != null)
		{
			this.KeyChanged(this, EventArgs.Empty);
		}
	}

	private void OnTagChanged()
	{
		if (this.TagChanged != null)
		{
			this.TagChanged(this, EventArgs.Empty);
		}
	}

	private ReflectionCachePair()
	{
	}

	public ReflectionCachePair(BiffRecordPosAttribute[] key, FieldInfo[] tag)
	{
		m_key = key;
		m_tag = tag;
	}
}
