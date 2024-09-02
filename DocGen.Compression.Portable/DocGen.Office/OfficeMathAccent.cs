using System;
using System.Collections.Generic;

namespace DocGen.Office;

internal class OfficeMathAccent : OfficeMathFunctionBase, IOfficeMathAccent, IOfficeMathFunctionBase, IOfficeMathEntity
{
	internal const short AccentCharKey = 1;

	private OfficeMath m_equation;

	private Dictionary<int, object> m_propertiesHash;

	internal IOfficeRunFormat m_controlProperties;

	public string AccentCharacter
	{
		get
		{
			return (string)GetPropertyValue(1);
		}
		set
		{
			SetPropertyValue(1, value);
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

	internal OfficeMathAccent(IOfficeMathEntity owner)
		: base(owner)
	{
		m_type = MathFunctionType.Accent;
		m_equation = new OfficeMath(this);
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

	internal override OfficeMathFunctionBase Clone(IOfficeMathEntity owner)
	{
		OfficeMathAccent officeMathAccent = (OfficeMathAccent)MemberwiseClone();
		if (officeMathAccent.m_controlProperties != null)
		{
			officeMathAccent.m_controlProperties = m_controlProperties.Clone();
		}
		officeMathAccent.m_equation = m_equation.CloneImpl(officeMathAccent);
		officeMathAccent.SetOwner(owner);
		return officeMathAccent;
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
		if (key == 1)
		{
			return Convert.ToString(Convert.ToChar(770));
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
}
