using System;
using System.Collections.Generic;

namespace DocGen.Office;

internal class OfficeMathBox : OfficeMathFunctionBase, IOfficeMathBox, IOfficeMathFunctionBase, IOfficeMathEntity
{
	internal const short BoxAlignKey = 11;

	internal const short NoBreakKey = 12;

	internal const short EnableDifferentialKey = 14;

	internal const short OperatorEmulatorKey = 15;

	private OfficeMath m_equation;

	private OfficeMathBreak m_break;

	private Dictionary<int, object> m_propertiesHash;

	internal IOfficeRunFormat m_controlProperties;

	public bool Alignment
	{
		get
		{
			return (bool)GetPropertyValue(11);
		}
		set
		{
			SetPropertyValue(11, value);
		}
	}

	public bool EnableDifferential
	{
		get
		{
			return (bool)GetPropertyValue(14);
		}
		set
		{
			SetPropertyValue(14, value);
		}
	}

	public bool NoBreak
	{
		get
		{
			return (bool)GetPropertyValue(12);
		}
		set
		{
			SetPropertyValue(12, value);
		}
	}

	public bool OperatorEmulator
	{
		get
		{
			return (bool)GetPropertyValue(15);
		}
		set
		{
			SetPropertyValue(15, value);
		}
	}

	public IOfficeMathBreak Break
	{
		get
		{
			return m_break;
		}
		set
		{
			m_break = (OfficeMathBreak)value;
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

	public OfficeMathBox(IOfficeMathEntity owner)
		: base(owner)
	{
		m_type = MathFunctionType.Box;
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
		switch (key)
		{
		case 11:
		case 14:
		case 15:
			return false;
		case 12:
			return true;
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
		OfficeMathBox officeMathBox = (OfficeMathBox)MemberwiseClone();
		officeMathBox.SetOwner(owner);
		if (officeMathBox.m_controlProperties != null)
		{
			officeMathBox.m_controlProperties = m_controlProperties.Clone();
		}
		officeMathBox.m_equation = m_equation.CloneImpl(officeMathBox);
		if (m_break != null)
		{
			officeMathBox.m_break = m_break.Clone(officeMathBox);
		}
		return officeMathBox;
	}

	internal override void Close()
	{
		if (m_propertiesHash != null)
		{
			m_propertiesHash.Clear();
			m_propertiesHash = null;
		}
		if (m_break != null)
		{
			m_break.Close();
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
