using System;
using System.Collections.Generic;

namespace DocGen.Office;

internal class OfficeMathMatrix : OfficeMathFunctionBase, IOfficeMathMatrix, IOfficeMathFunctionBase, IOfficeMathEntity
{
	internal const short RowSpacingKey = 27;

	internal const short RowSpacingRuleKey = 28;

	internal const short MatrixAlignKey = 33;

	internal const short ColumnWidthKey = 34;

	internal const short ColumnSpacingRuleKey = 35;

	internal const short ColumnSpacingKey = 36;

	internal const short PlaceHoldHiddenKey = 37;

	internal OfficeMathMatrixRows m_mathMatrixRows;

	internal OfficeMathMatrixColumns m_mathMatrixColumns;

	private Dictionary<int, object> m_propertiesHash;

	private List<MatrixColumnProperties> m_columnProperties;

	internal IOfficeRunFormat m_controlProperties;

	public MathVerticalAlignment VerticalAlignment
	{
		get
		{
			return (MathVerticalAlignment)GetPropertyValue(33);
		}
		set
		{
			SetPropertyValue(33, value);
		}
	}

	public float ColumnWidth
	{
		get
		{
			return (float)GetPropertyValue(34);
		}
		set
		{
			if (value < 0f || value > 1584f)
			{
				throw new ArgumentException("ColumnWidth must be between 0 pt and 1584 pt.");
			}
			SetPropertyValue(34, value);
		}
	}

	public SpacingRule ColumnSpacingRule
	{
		get
		{
			return (SpacingRule)GetPropertyValue(35);
		}
		set
		{
			SetPropertyValue(35, value);
		}
	}

	public IOfficeMathMatrixColumns Columns => m_mathMatrixColumns;

	public float ColumnSpacing
	{
		get
		{
			return (float)GetPropertyValue(36);
		}
		set
		{
			if ((ColumnSpacingRule == SpacingRule.Exactly || ColumnSpacingRule == SpacingRule.Multiple) && (value < 0f || value > 1584f))
			{
				throw new ArgumentException("ColumnSpacing must be between 0 pt and 1584 pt");
			}
			if (ColumnSpacingRule == SpacingRule.Multiple)
			{
				int num = 6;
				value = (int)Math.Floor(value - value % (float)num);
			}
			SetPropertyValue(36, value);
		}
	}

	public bool HidePlaceHolders
	{
		get
		{
			return (bool)GetPropertyValue(37);
		}
		set
		{
			SetPropertyValue(37, value);
		}
	}

	internal List<MatrixColumnProperties> ColumnProperties
	{
		get
		{
			return m_columnProperties;
		}
		set
		{
			m_columnProperties = value;
		}
	}

	public IOfficeMathMatrixRows Rows => m_mathMatrixRows;

	public float RowSpacing
	{
		get
		{
			return (float)GetPropertyValue(27);
		}
		set
		{
			if (RowSpacingRule == SpacingRule.Exactly && (value < 0f || value > 1584f))
			{
				throw new ArgumentException("RowSpacing must be between 0 pt and 1584 pt for Exactly spacing rule.");
			}
			if (RowSpacingRule == SpacingRule.Multiple && (value < 0f || value > 132f))
			{
				throw new ArgumentException("RowSpacing must be between 0 li and 132 li for Multiple spacing rule.");
			}
			SetPropertyValue(27, value);
		}
	}

	public SpacingRule RowSpacingRule
	{
		get
		{
			return (SpacingRule)GetPropertyValue(28);
		}
		set
		{
			SetPropertyValue(28, value);
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
		}
	}

	internal OfficeMathMatrix(IOfficeMathEntity owner)
		: base(owner)
	{
		m_type = MathFunctionType.Matrix;
		m_propertiesHash = new Dictionary<int, object>();
		m_mathMatrixRows = new OfficeMathMatrixRows(this);
		m_mathMatrixColumns = new OfficeMathMatrixColumns(this);
		m_columnProperties = new List<MatrixColumnProperties>();
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
			27 => 0f, 
			28 => SpacingRule.Single, 
			33 => MathVerticalAlignment.Center, 
			34 => 0f, 
			35 => SpacingRule.Single, 
			36 => 0f, 
			37 => false, 
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
		OfficeMathMatrix officeMathMatrix = (OfficeMathMatrix)MemberwiseClone();
		officeMathMatrix.SetOwner(owner);
		if (officeMathMatrix.m_controlProperties != null)
		{
			officeMathMatrix.m_controlProperties = m_controlProperties.Clone();
		}
		officeMathMatrix.m_mathMatrixColumns = new OfficeMathMatrixColumns(officeMathMatrix);
		m_mathMatrixColumns.CloneItemsTo(officeMathMatrix.m_mathMatrixColumns);
		officeMathMatrix.m_mathMatrixRows = new OfficeMathMatrixRows(officeMathMatrix);
		m_mathMatrixRows.CloneItemsTo(officeMathMatrix.m_mathMatrixRows);
		return officeMathMatrix;
	}

	internal void RemoveMatrixItems(int startColIndex, int startRowIndex, int endColIndex, int endRowIndex)
	{
		while (startRowIndex <= endRowIndex)
		{
			OfficeMathMatrixRow officeMathMatrixRow = m_mathMatrixRows[startRowIndex] as OfficeMathMatrixRow;
			for (int i = startColIndex; i <= endColIndex; i++)
			{
				officeMathMatrixRow?.m_args.InnerList.RemoveAt(startColIndex);
			}
			startRowIndex++;
		}
	}

	internal void ApplyColumnProperties()
	{
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < ColumnProperties.Count; i++)
		{
			MatrixColumnProperties matrixColumnProperties = ColumnProperties[i];
			for (int j = num; j < num + matrixColumnProperties.Count && j < Columns.Count; j++)
			{
				if (Columns[j] is OfficeMathMatrixColumn officeMathMatrixColumn)
				{
					officeMathMatrixColumn.m_alignment = matrixColumnProperties.Alignment;
				}
				num2++;
			}
			num = num2;
		}
	}

	internal void UpdateColumnProperties(OfficeMathMatrix mathMatrix)
	{
		mathMatrix.ColumnProperties.Clear();
		MatrixColumnProperties matrixColumnProperties = new MatrixColumnProperties(this);
		matrixColumnProperties.Alignment = Columns[0].HorizontalAlignment;
		matrixColumnProperties.Count = 1;
		IOfficeMathMatrixColumns columns = mathMatrix.Columns;
		int num = 1;
		for (int i = 1; i < columns.Count; i++)
		{
			MathHorizontalAlignment horizontalAlignment = mathMatrix.Columns[i].HorizontalAlignment;
			if (columns[i - 1].HorizontalAlignment == columns[i].HorizontalAlignment)
			{
				num++;
			}
			else
			{
				m_columnProperties.Add(matrixColumnProperties);
				matrixColumnProperties = new MatrixColumnProperties(this);
				matrixColumnProperties.Alignment = horizontalAlignment;
				num = 1;
			}
			matrixColumnProperties.Count = num;
		}
		m_columnProperties.Add(matrixColumnProperties);
	}

	internal void GetRangeOfArguments(int startColIndex, int startRowIndex, int endColIndex, int endRowIndex, OfficeMaths maths)
	{
		while (startRowIndex <= endRowIndex)
		{
			OfficeMathMatrixRow officeMathMatrixRow = m_mathMatrixRows[startRowIndex] as OfficeMathMatrixRow;
			for (int i = startColIndex; i <= endColIndex; i++)
			{
				if (startColIndex < officeMathMatrixRow.Arguments.Count)
				{
					maths.InnerList.Add(officeMathMatrixRow.Arguments[startColIndex]);
				}
			}
			startRowIndex++;
		}
	}

	internal void CreateArguments(int startColIndex, int startRowIndex, int endColIndex, int endRowIndex)
	{
		while (startRowIndex <= endRowIndex)
		{
			OfficeMathMatrixRow officeMathMatrixRow = m_mathMatrixRows[startRowIndex] as OfficeMathMatrixRow;
			for (int i = startColIndex; i <= endColIndex; i++)
			{
				if (officeMathMatrixRow.Arguments.Count <= endColIndex)
				{
					OfficeMath officeMath = new OfficeMath(officeMathMatrixRow);
					officeMath.m_parentCol = Columns[startColIndex] as OfficeMathMatrixColumn;
					officeMathMatrixRow.m_args.InnerList.Add(officeMath);
				}
			}
			startRowIndex++;
		}
	}

	internal void UpdateColumns()
	{
		int maximumCellCount = GetMaximumCellCount();
		for (int i = 0; i < maximumCellCount; i++)
		{
			Columns.Add();
		}
	}

	private int GetMaximumCellCount()
	{
		int num = 0;
		for (int i = 0; i < Rows.Count; i++)
		{
			if (num < Rows[i].Arguments.Count)
			{
				num = Rows[i].Arguments.Count;
			}
		}
		return num;
	}

	internal override void Close()
	{
		if (m_propertiesHash != null)
		{
			m_propertiesHash.Clear();
			m_propertiesHash = null;
		}
		if (m_mathMatrixRows != null)
		{
			m_mathMatrixRows.Close();
		}
		if (m_mathMatrixColumns != null)
		{
			m_mathMatrixColumns.Close();
		}
		if (m_columnProperties != null)
		{
			m_columnProperties.Clear();
			m_columnProperties = null;
		}
		if (m_controlProperties != null)
		{
			m_controlProperties.Dispose();
			m_controlProperties = null;
		}
		base.Close();
	}
}
