using System;
using System.Collections.Generic;

namespace DocGen.Office;

internal class OfficeMathPhantom : OfficeMathFunctionBase, IOfficeMathPhantom, IOfficeMathFunctionBase, IOfficeMathEntity
{
	internal const short ShowKey = 46;

	internal const short SmashKey = 47;

	internal const short TransparentKey = 48;

	internal const short ZeroAscentKey = 49;

	internal const short ZeroDescentKey = 50;

	internal const short ZeroWidthKey = 51;

	private OfficeMath m_equation;

	private Dictionary<int, object> m_propertiesHash;

	private byte m_bFlags;

	internal IOfficeRunFormat m_controlProperties;

	public bool Show
	{
		get
		{
			return (bool)GetPropertyValue(46);
		}
		set
		{
			SetPropertyValue(46, value);
		}
	}

	internal bool Smash
	{
		get
		{
			return (bool)GetPropertyValue(47);
		}
		set
		{
			SetPropertyValue(47, value);
		}
	}

	public bool Transparent
	{
		get
		{
			return (bool)GetPropertyValue(48);
		}
		set
		{
			SetPropertyValue(48, value);
		}
	}

	public bool ZeroAscent
	{
		get
		{
			return (bool)GetPropertyValue(49);
		}
		set
		{
			SetPropertyValue(49, value);
		}
	}

	public bool ZeroDescent
	{
		get
		{
			return (bool)GetPropertyValue(50);
		}
		set
		{
			SetPropertyValue(50, value);
		}
	}

	public bool ZeroWidth
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

	public IOfficeMath Equation => m_equation;

	internal bool IsDefault
	{
		get
		{
			return (m_bFlags & 1) != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xFEu) | (value ? 1u : 0u));
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
			IsDefault = false;
		}
	}

	internal OfficeMathPhantom(IOfficeMathEntity owner)
		: base(owner)
	{
		m_type = MathFunctionType.Phantom;
		m_equation = new OfficeMath(this);
		m_propertiesHash = new Dictionary<int, object>();
		IsDefault = true;
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
			46 => true, 
			47 => false, 
			48 => false, 
			49 => false, 
			50 => false, 
			51 => false, 
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
		OfficeMathPhantom officeMathPhantom = (OfficeMathPhantom)MemberwiseClone();
		officeMathPhantom.SetOwner(owner);
		if (officeMathPhantom.m_controlProperties != null)
		{
			officeMathPhantom.m_controlProperties = m_controlProperties.Clone();
		}
		officeMathPhantom.m_equation = m_equation.CloneImpl(officeMathPhantom);
		return officeMathPhantom;
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
