namespace DocGen.Office;

internal class OfficeMathMatrixRow : OwnerHolder, IOfficeMathMatrixRow, IOfficeMathEntity
{
	internal OfficeMaths m_args;

	public int RowIndex
	{
		get
		{
			if (base.OwnerMathEntity is OfficeMathMatrix officeMathMatrix)
			{
				return (officeMathMatrix.Rows as OfficeMathMatrixRows).InnerList.IndexOf(this);
			}
			return -1;
		}
	}

	public IOfficeMaths Arguments => m_args;

	internal OfficeMathMatrixRow(IOfficeMathEntity owner)
		: base(owner)
	{
		m_args = new OfficeMaths(this);
	}

	internal OfficeMathMatrixRow Clone(IOfficeMathEntity owner)
	{
		OfficeMathMatrixRow officeMathMatrixRow = (OfficeMathMatrixRow)MemberwiseClone();
		officeMathMatrixRow.SetOwner(owner);
		officeMathMatrixRow.m_args = new OfficeMaths(officeMathMatrixRow);
		m_args.CloneItemsTo(officeMathMatrixRow.m_args);
		return officeMathMatrixRow;
	}

	internal override void Close()
	{
		if (m_args != null)
		{
			m_args.Close();
			m_args.Clear();
			m_args = null;
		}
		base.Close();
	}

	internal void OnRowAdded()
	{
		OfficeMathMatrix officeMathMatrix = base.OwnerMathEntity as OfficeMathMatrix;
		if (officeMathMatrix.Columns.Count > 0)
		{
			officeMathMatrix.CreateArguments(0, RowIndex, officeMathMatrix.Columns.Count - 1, RowIndex);
		}
	}
}
