using System;
using System.Collections.Generic;

namespace DocGen.Office;

internal class OfficeMathBorderBox : OfficeMathFunctionBase, IOfficeMathBorderBox, IOfficeMathFunctionBase, IOfficeMathEntity
{
	internal const short HideTopKey = 3;

	internal const short HideBottomKey = 4;

	internal const short HideRightKey = 5;

	internal const short HideLeftKey = 6;

	internal const short StrikeBLTRKey = 7;

	internal const short StrikeTLBRKey = 8;

	internal const short StrikeVerticalKey = 9;

	internal const short StrikeHorizontalKey = 10;

	private OfficeMath m_equation;

	private Dictionary<int, object> m_propertiesHash;

	internal IOfficeRunFormat m_controlProperties;

	public bool HideTop
	{
		get
		{
			return (bool)GetPropertyValue(3);
		}
		set
		{
			SetPropertyValue(3, value);
		}
	}

	public bool HideBottom
	{
		get
		{
			return (bool)GetPropertyValue(4);
		}
		set
		{
			SetPropertyValue(4, value);
		}
	}

	public bool HideRight
	{
		get
		{
			return (bool)GetPropertyValue(5);
		}
		set
		{
			SetPropertyValue(5, value);
		}
	}

	public bool HideLeft
	{
		get
		{
			return (bool)GetPropertyValue(6);
		}
		set
		{
			SetPropertyValue(6, value);
		}
	}

	public bool StrikeDiagonalUp
	{
		get
		{
			return (bool)GetPropertyValue(7);
		}
		set
		{
			SetPropertyValue(7, value);
		}
	}

	public bool StrikeDiagonalDown
	{
		get
		{
			return (bool)GetPropertyValue(8);
		}
		set
		{
			SetPropertyValue(8, value);
		}
	}

	public bool StrikeVertical
	{
		get
		{
			return (bool)GetPropertyValue(9);
		}
		set
		{
			SetPropertyValue(9, value);
		}
	}

	public bool StrikeHorizontal
	{
		get
		{
			return (bool)GetPropertyValue(10);
		}
		set
		{
			SetPropertyValue(10, value);
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

	internal OfficeMathBorderBox(IOfficeMathEntity owner)
		: base(owner)
	{
		m_type = MathFunctionType.BorderBox;
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
		if ((uint)(key - 3) <= 7u)
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
		OfficeMathBorderBox officeMathBorderBox = (OfficeMathBorderBox)MemberwiseClone();
		officeMathBorderBox.SetOwner(owner);
		if (officeMathBorderBox.m_controlProperties != null)
		{
			officeMathBorderBox.m_controlProperties = m_controlProperties.Clone();
		}
		officeMathBorderBox.m_equation = m_equation.CloneImpl(officeMathBorderBox);
		return officeMathBorderBox;
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
