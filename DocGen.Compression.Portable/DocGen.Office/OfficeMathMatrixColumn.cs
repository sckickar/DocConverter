using System;
using System.Collections.Generic;

namespace DocGen.Office;

internal class OfficeMathMatrixColumn : OwnerHolder, IOfficeMathMatrixColumn, IOfficeMathEntity
{
	internal const short ColumnAlignKey = 40;

	internal MathHorizontalAlignment m_alignment;

	internal OfficeMaths m_args;

	private Dictionary<int, object> m_propertiesHash;

	public int ColumnIndex
	{
		get
		{
			if (base.OwnerMathEntity is OfficeMathMatrix officeMathMatrix)
			{
				return (officeMathMatrix.Columns as OfficeMathMatrixColumns).InnerList.IndexOf(this);
			}
			return -1;
		}
	}

	public IOfficeMaths Arguments
	{
		get
		{
			OfficeMathMatrix officeMathMatrix = base.OwnerMathEntity as OfficeMathMatrix;
			OfficeMaths officeMaths = new OfficeMaths(this);
			officeMathMatrix.GetRangeOfArguments(ColumnIndex, 0, ColumnIndex, officeMathMatrix.Rows.Count - 1, officeMaths);
			return officeMaths;
		}
	}

	public MathHorizontalAlignment HorizontalAlignment
	{
		get
		{
			return m_alignment;
		}
		set
		{
			m_alignment = value;
			if (base.OwnerMathEntity is OfficeMathMatrix officeMathMatrix)
			{
				officeMathMatrix.UpdateColumnProperties(officeMathMatrix);
			}
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

	internal OfficeMathMatrixColumn(IOfficeMathEntity owner)
		: base(owner)
	{
		m_propertiesHash = new Dictionary<int, object>();
		m_args = new OfficeMaths(this);
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
		if (key == 40)
		{
			return MathHorizontalAlignment.Center;
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

	internal void OnColumnAdded()
	{
		OfficeMathMatrix officeMathMatrix = base.OwnerMathEntity as OfficeMathMatrix;
		if (officeMathMatrix.Rows.Count > 0)
		{
			officeMathMatrix.CreateArguments(ColumnIndex, 0, ColumnIndex, officeMathMatrix.Rows.Count - 1);
		}
	}

	internal OfficeMathMatrixColumn Clone(IOfficeMathEntity owner)
	{
		OfficeMathMatrixColumn officeMathMatrixColumn = (OfficeMathMatrixColumn)MemberwiseClone();
		officeMathMatrixColumn.SetOwner(owner);
		officeMathMatrixColumn.m_args = new OfficeMaths(officeMathMatrixColumn);
		m_args.CloneItemsTo(officeMathMatrixColumn.m_args);
		return officeMathMatrixColumn;
	}

	internal override void Close()
	{
		if (m_propertiesHash != null)
		{
			m_propertiesHash.Clear();
			m_propertiesHash = null;
		}
		if (m_args != null)
		{
			m_args.Close();
			m_args.Clear();
			m_args = null;
		}
		base.Close();
	}
}
