using System;
using System.Collections.Generic;

namespace DocGen.Office;

internal class OfficeMathBar : OfficeMathFunctionBase, IOfficeMathBar, IOfficeMathFunctionBase, IOfficeMathEntity
{
	internal const short BarTopKey = 2;

	private OfficeMath m_equation;

	private Dictionary<int, object> m_propertiesHash;

	internal IOfficeRunFormat m_controlProperties;

	public IOfficeRunFormat ControlProperties
	{
		get
		{
			if (m_controlProperties == null)
			{
				m_controlProperties = GetDefaultControlProperties();
			}
			return m_controlProperties;
		}
		set
		{
			m_controlProperties = value;
		}
	}

	public bool BarTop
	{
		get
		{
			return (bool)GetPropertyValue(2);
		}
		set
		{
			SetPropertyValue(2, value);
		}
	}

	public IOfficeMath Equation => m_equation;

	internal Dictionary<int, object> PropertiesHash
	{
		get
		{
			if (m_propertiesHash == null)
			{
				m_propertiesHash = new Dictionary<int, object>();
			}
			return m_propertiesHash;
		}
	}

	internal object this[int key]
	{
		get
		{
			if (PropertiesHash.ContainsKey(key))
			{
				return PropertiesHash[key];
			}
			return GetDefValue(key);
		}
		set
		{
			PropertiesHash[key] = value;
		}
	}

	internal OfficeMathBar(IOfficeMathEntity owner)
		: base(owner)
	{
		m_type = MathFunctionType.Bar;
		m_equation = new OfficeMath(this);
		m_propertiesHash = new Dictionary<int, object>();
	}

	internal override OfficeMathFunctionBase Clone(IOfficeMathEntity owner)
	{
		OfficeMathBar officeMathBar = (OfficeMathBar)MemberwiseClone();
		if (officeMathBar.m_controlProperties != null)
		{
			officeMathBar.m_controlProperties = m_controlProperties.Clone();
		}
		officeMathBar.m_equation = m_equation.CloneImpl(officeMathBar);
		officeMathBar.SetOwner(owner);
		return officeMathBar;
	}

	internal object GetPropertyValue(int propKey)
	{
		return this[propKey];
	}

	internal void SetPropertyValue(int propKey, object value)
	{
		this[propKey] = value;
	}

	private object GetDefValue(int key)
	{
		if (key == 2)
		{
			return false;
		}
		return new ArgumentException("key has invalid value");
	}

	internal bool HasValue(int propertyKey)
	{
		if (HasKey(propertyKey))
		{
			return true;
		}
		return false;
	}

	internal bool HasKey(int key)
	{
		if (PropertiesHash == null)
		{
			return false;
		}
		return PropertiesHash.ContainsKey(key);
	}

	internal override void Close()
	{
		if (m_propertiesHash != null)
		{
			m_propertiesHash.Clear();
			m_propertiesHash = null;
		}
		if (m_controlProperties != null)
		{
			m_controlProperties.Dispose();
			m_controlProperties = null;
		}
		if (m_equation != null)
		{
			m_equation.Close();
		}
		base.Close();
	}
}
