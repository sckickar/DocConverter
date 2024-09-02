namespace DocGen.Office;

internal class OfficeMathLimit : OfficeMathFunctionBase, IOfficeMathLimit, IOfficeMathFunctionBase, IOfficeMathEntity
{
	private MathLimitType m_limitType;

	private OfficeMath m_equation;

	private OfficeMath m_limit;

	internal IOfficeRunFormat m_controlProperties;

	public MathLimitType LimitType
	{
		get
		{
			return m_limitType;
		}
		set
		{
			m_limitType = value;
		}
	}

	public IOfficeMath Equation => m_equation;

	public IOfficeMath Limit => m_limit;

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

	internal OfficeMathLimit(IOfficeMathEntity owner)
		: base(owner)
	{
		m_type = MathFunctionType.Limit;
		m_equation = new OfficeMath(this);
		m_limit = new OfficeMath(this);
	}

	internal override void Close()
	{
		if (m_limit != null)
		{
			m_limit.Close();
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

	internal override OfficeMathFunctionBase Clone(IOfficeMathEntity owner)
	{
		OfficeMathLimit officeMathLimit = (OfficeMathLimit)MemberwiseClone();
		officeMathLimit.SetOwner(owner);
		if (officeMathLimit.m_controlProperties != null)
		{
			officeMathLimit.m_controlProperties = m_controlProperties.Clone();
		}
		officeMathLimit.m_equation = m_equation.CloneImpl(officeMathLimit);
		officeMathLimit.m_limit = m_limit.CloneImpl(officeMathLimit);
		return officeMathLimit;
	}
}
