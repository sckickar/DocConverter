namespace DocGen.Office;

internal class OfficeMathRunElement : OfficeMathFunctionBase, IOfficeMathRunElement, IOfficeMathFunctionBase, IOfficeMathEntity
{
	internal IOfficeRun m_item;

	private OfficeMathFormat m_mathFormat;

	public IOfficeRun Item
	{
		get
		{
			return m_item;
		}
		set
		{
			m_item = value;
			m_item.OwnerMathRunElement = this;
		}
	}

	public IOfficeMathFormat MathFormat => m_mathFormat;

	internal OfficeMathRunElement(IOfficeMathEntity owner)
		: base(owner)
	{
		m_type = MathFunctionType.RunElement;
		m_mathFormat = new OfficeMathFormat(this);
	}

	internal override void Close()
	{
		if (m_mathFormat != null)
		{
			m_mathFormat.Close();
			m_mathFormat = null;
		}
		if (m_item != null)
		{
			m_item.Dispose();
			m_item = null;
		}
		base.Close();
	}

	internal override OfficeMathFunctionBase Clone(IOfficeMathEntity owner)
	{
		OfficeMathRunElement officeMathRunElement = (OfficeMathRunElement)MemberwiseClone();
		officeMathRunElement.SetOwner(owner);
		officeMathRunElement.m_mathFormat = m_mathFormat.Clone(officeMathRunElement);
		officeMathRunElement.m_item = m_item.CloneRun();
		officeMathRunElement.m_item.OwnerMathRunElement = officeMathRunElement;
		return officeMathRunElement;
	}
}
