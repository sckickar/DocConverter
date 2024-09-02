using System;
using System.Collections.Generic;

namespace DocGen.Office;

internal class OfficeMathParagraph : OwnerHolder, IOfficeMathParagraph, IOfficeMathEntity
{
	internal const short MathJustificationKey = 77;

	private OfficeMaths m_maths;

	private Dictionary<int, object> m_propertiesHash;

	private byte m_bFlags;

	private object m_ownerEntity;

	internal IOfficeRunFormat m_defaultMathCharacterFormat;

	internal DocumentLaTeXConverter m_documentLaTeXConverter;

	private LaTeXConverter m_laTeXConveter;

	public MathJustification Justification
	{
		get
		{
			return (MathJustification)GetPropertyValue(77);
		}
		set
		{
			SetPropertyValue(77, value);
		}
	}

	public IOfficeMaths Maths => m_maths;

	public object Owner => m_ownerEntity;

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

	internal IOfficeRunFormat DefaultMathCharacterFormat
	{
		get
		{
			return m_defaultMathCharacterFormat;
		}
		set
		{
			m_defaultMathCharacterFormat = value;
		}
	}

	public string LaTeX
	{
		get
		{
			return LaTeXConverter.GetLaTeX(this);
		}
		set
		{
			Maths.Clear();
			ConvertLaTeXToMath(value, m_documentLaTeXConverter);
		}
	}

	internal LaTeXConverter LaTeXConverter
	{
		get
		{
			if (m_laTeXConveter == null)
			{
				m_laTeXConveter = new LaTeXConverter();
			}
			return m_laTeXConveter;
		}
	}

	internal OfficeMathParagraph(object owner)
		: base(null)
	{
		m_maths = new OfficeMaths(this);
		m_ownerEntity = owner;
		IsDefault = true;
	}

	internal void ConvertLaTeXToMath(string laTeX, DocumentLaTeXConverter documentLaTeXConverter)
	{
		LaTeXConverter.ParseMathPara(laTeX, this, documentLaTeXConverter);
	}

	internal OfficeMathParagraph Clone()
	{
		OfficeMathParagraph officeMathParagraph = (OfficeMathParagraph)MemberwiseClone();
		officeMathParagraph.m_maths = new OfficeMaths(officeMathParagraph);
		m_maths.CloneItemsTo(officeMathParagraph.m_maths);
		return officeMathParagraph;
	}

	internal override void Close()
	{
		if (m_maths != null)
		{
			m_maths.Close();
			m_maths = null;
		}
		if (m_laTeXConveter != null)
		{
			m_laTeXConveter = null;
		}
		base.Close();
	}

	internal void SetOwner(object owner)
	{
		m_ownerEntity = owner;
	}

	internal object GetPropertyValue(int propKey)
	{
		return this[propKey];
	}

	internal void SetPropertyValue(int propKey, object value)
	{
		this[propKey] = value;
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

	private object GetDefValue(int key)
	{
		if (key == 77)
		{
			return MathJustification.CenterGroup;
		}
		return new ArgumentException("key has invalid value");
	}
}
