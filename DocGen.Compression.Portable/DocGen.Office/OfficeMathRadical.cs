using System;
using System.Collections.Generic;

namespace DocGen.Office;

internal class OfficeMathRadical : OfficeMathFunctionBase, IOfficeMathRadical, IOfficeMathFunctionBase, IOfficeMathEntity
{
	internal const short HideDegreeKey = 51;

	private Dictionary<int, object> m_propertiesHash;

	public OfficeMath m_degree;

	public OfficeMath m_equation;

	internal IOfficeRunFormat m_controlProperties;

	public IOfficeMath Degree => m_degree;

	public IOfficeMath Equation => m_equation;

	public bool HideDegree
	{
		get
		{
			return (bool)GetPropertyValue(51);
		}
		set
		{
			SetPropertyValue(51, value);
		}
	}

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

	internal OfficeMathRadical(IOfficeMathEntity owner)
		: base(owner)
	{
		m_type = MathFunctionType.Radical;
		m_equation = new OfficeMath(this);
		m_degree = new OfficeMath(this);
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
		if (key == 51)
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

	internal override OfficeMathFunctionBase Clone(IOfficeMathEntity owner)
	{
		OfficeMathRadical officeMathRadical = (OfficeMathRadical)MemberwiseClone();
		officeMathRadical.SetOwner(owner);
		officeMathRadical.m_equation = m_equation.CloneImpl(officeMathRadical);
		if (officeMathRadical.m_controlProperties != null)
		{
			officeMathRadical.m_controlProperties = m_controlProperties.Clone();
		}
		officeMathRadical.m_degree = m_degree.CloneImpl(officeMathRadical);
		return officeMathRadical;
	}

	internal override void Close()
	{
		if (m_propertiesHash != null)
		{
			m_propertiesHash.Clear();
			m_propertiesHash = null;
		}
		if (m_degree != null)
		{
			m_degree.Close();
		}
		if (m_equation != null)
		{
			m_equation.Close();
		}
		if (m_controlProperties != null)
		{
			m_controlProperties.Dispose();
			m_controlProperties = null;
		}
		base.Close();
	}
}
