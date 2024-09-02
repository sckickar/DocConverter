using System;
using System.Collections.Generic;

namespace DocGen.Office;

internal class OfficeMathGroupCharacter : OfficeMathFunctionBase, IOfficeMathGroupCharacter, IOfficeMathFunctionBase, IOfficeMathEntity
{
	internal const short HasAlignTopKey = 30;

	internal const short CharKey = 31;

	internal const short HasCharTopKey = 32;

	private Dictionary<int, object> m_propertiesHash;

	private OfficeMath m_equation;

	internal IOfficeRunFormat m_controlProperties;

	public bool HasAlignTop
	{
		get
		{
			return (bool)GetPropertyValue(30);
		}
		set
		{
			SetPropertyValue(30, value);
		}
	}

	public string GroupCharacter
	{
		get
		{
			return (string)GetPropertyValue(31);
		}
		set
		{
			SetPropertyValue(31, value);
		}
	}

	public bool HasCharacterTop
	{
		get
		{
			return (bool)GetPropertyValue(32);
		}
		set
		{
			SetPropertyValue(32, value);
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

	internal OfficeMathGroupCharacter(IOfficeMathEntity owner)
		: base(owner)
	{
		m_type = MathFunctionType.GroupCharacter;
		m_equation = new OfficeMath(this);
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
		return key switch
		{
			30 => true, 
			32 => false, 
			31 => Convert.ToString(Convert.ToChar(9183)), 
			_ => new ArgumentException("key has invalid value"), 
		};
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
		OfficeMathGroupCharacter officeMathGroupCharacter = (OfficeMathGroupCharacter)MemberwiseClone();
		officeMathGroupCharacter.SetOwner(owner);
		if (officeMathGroupCharacter.m_controlProperties != null)
		{
			officeMathGroupCharacter.m_controlProperties = m_controlProperties.Clone();
		}
		officeMathGroupCharacter.m_equation = m_equation.CloneImpl(officeMathGroupCharacter);
		return officeMathGroupCharacter;
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
		if (m_controlProperties != null)
		{
			m_controlProperties.Dispose();
			m_controlProperties = null;
		}
		base.Close();
	}
}
