using System;
using System.Collections.Generic;

namespace DocGen.Office;

internal class OfficeMathRightScript : OfficeMathFunctionBase, IOfficeMathRightScript, IOfficeMathFunctionBase, IOfficeMathEntity
{
	internal const short SkipAlignKey = 52;

	private OfficeMath m_subscript;

	private OfficeMath m_superscript;

	private OfficeMath m_equation;

	private Dictionary<int, object> m_propertiesHash;

	internal IOfficeRunFormat m_controlProperties;

	public bool IsSkipAlign
	{
		get
		{
			return (bool)GetPropertyValue(52);
		}
		set
		{
			SetPropertyValue(52, value);
		}
	}

	public IOfficeMath Subscript => m_subscript;

	public IOfficeMath Superscript => m_superscript;

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

	internal OfficeMathRightScript(IOfficeMathEntity owner)
		: base(owner)
	{
		m_type = MathFunctionType.RightSubSuperscript;
		m_equation = new OfficeMath(this);
		m_subscript = new OfficeMath(this);
		m_superscript = new OfficeMath(this);
	}

	internal void ToScrPre()
	{
	}

	internal void RemoveSub()
	{
	}

	internal void RemoveSup()
	{
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
		if (key == 52)
		{
			return true;
		}
		return new ArgumentException("key has invalid value");
	}

	internal override OfficeMathFunctionBase Clone(IOfficeMathEntity owner)
	{
		OfficeMathRightScript officeMathRightScript = (OfficeMathRightScript)MemberwiseClone();
		officeMathRightScript.SetOwner(owner);
		officeMathRightScript.m_equation = m_equation.CloneImpl(officeMathRightScript);
		if (officeMathRightScript.m_controlProperties != null)
		{
			officeMathRightScript.m_controlProperties = m_controlProperties.Clone();
		}
		officeMathRightScript.m_subscript = m_subscript.CloneImpl(officeMathRightScript);
		officeMathRightScript.m_superscript = m_superscript.CloneImpl(officeMathRightScript);
		return officeMathRightScript;
	}
}
