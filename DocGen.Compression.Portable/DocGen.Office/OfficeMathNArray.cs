using System;
using System.Collections.Generic;

namespace DocGen.Office;

internal class OfficeMathNArray : OfficeMathFunctionBase, IOfficeMathNArray, IOfficeMathFunctionBase, IOfficeMathEntity
{
	internal const short NAryCharKey = 41;

	internal const short HasGrowKey = 42;

	internal const short HideSubscriptKey = 43;

	internal const short HideSuperscriptKey = 44;

	internal const short SubSupLimitKey = 45;

	private OfficeMath m_equation;

	private OfficeMath m_subscript;

	private OfficeMath m_superscript;

	private Dictionary<int, object> m_propertiesHash;

	internal IOfficeRunFormat m_controlProperties;

	public string NArrayCharacter
	{
		get
		{
			return (string)GetPropertyValue(41);
		}
		set
		{
			SetPropertyValue(41, value);
		}
	}

	public bool HasGrow
	{
		get
		{
			return (bool)GetPropertyValue(42);
		}
		set
		{
			SetPropertyValue(42, value);
		}
	}

	public bool HideLowerLimit
	{
		get
		{
			return (bool)GetPropertyValue(43);
		}
		set
		{
			SetPropertyValue(43, value);
		}
	}

	public bool HideUpperLimit
	{
		get
		{
			return (bool)GetPropertyValue(44);
		}
		set
		{
			SetPropertyValue(44, value);
		}
	}

	public bool SubSuperscriptLimit
	{
		get
		{
			return (bool)GetPropertyValue(45);
		}
		set
		{
			SetPropertyValue(45, value);
		}
	}

	public IOfficeMath Equation => m_equation;

	public IOfficeMath Subscript => m_subscript;

	public IOfficeMath Superscript => m_superscript;

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

	internal OfficeMathNArray(IOfficeMathEntity owner)
		: base(owner)
	{
		m_type = MathFunctionType.NArray;
		m_equation = new OfficeMath(this);
		m_subscript = new OfficeMath(this);
		m_superscript = new OfficeMath(this);
	}

	internal override void Close()
	{
		if (m_propertiesHash != null)
		{
			m_propertiesHash.Clear();
			m_propertiesHash = null;
		}
		if (m_equation != null)
		{
			m_equation.Close();
		}
		if (m_subscript != null)
		{
			m_subscript.Close();
		}
		if (m_superscript != null)
		{
			m_superscript.Close();
		}
		if (m_controlProperties != null)
		{
			m_controlProperties.Dispose();
			m_controlProperties = null;
		}
		base.Close();
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
		switch (key)
		{
		case 41:
			return Convert.ToString(Convert.ToChar(8747));
		case 42:
		case 43:
		case 44:
			return false;
		case 45:
		{
			char c = Convert.ToChar(8747);
			if (NArrayCharacter.Equals(Convert.ToString(c)))
			{
				return true;
			}
			return false;
		}
		default:
			return new ArgumentException("key has invalid value");
		}
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
		OfficeMathNArray officeMathNArray = (OfficeMathNArray)MemberwiseClone();
		officeMathNArray.SetOwner(owner);
		if (officeMathNArray.m_controlProperties != null)
		{
			officeMathNArray.m_controlProperties = m_controlProperties.Clone();
		}
		officeMathNArray.m_equation = m_equation.CloneImpl(officeMathNArray);
		officeMathNArray.m_subscript = m_subscript.CloneImpl(officeMathNArray);
		officeMathNArray.m_superscript = m_superscript.CloneImpl(officeMathNArray);
		return officeMathNArray;
	}
}
