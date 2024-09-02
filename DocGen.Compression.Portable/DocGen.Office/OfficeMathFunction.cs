namespace DocGen.Office;

internal class OfficeMathFunction : OfficeMathFunctionBase, IOfficeMathFunction, IOfficeMathFunctionBase, IOfficeMathEntity
{
	private OfficeMath m_equation;

	private OfficeMath m_fName;

	internal IOfficeRunFormat m_controlProperties;

	public IOfficeMath Equation => m_equation;

	public IOfficeMath FunctionName => m_fName;

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

	internal OfficeMathFunction(IOfficeMathEntity owner)
		: base(owner)
	{
		m_type = MathFunctionType.Function;
		m_equation = new OfficeMath(this);
		m_fName = new OfficeMath(this);
	}

	internal override void Close()
	{
		if (m_equation != null)
		{
			m_equation.Close();
		}
		if (m_fName != null)
		{
			m_fName.Close();
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
		OfficeMathFunction officeMathFunction = (OfficeMathFunction)MemberwiseClone();
		officeMathFunction.SetOwner(owner);
		if (officeMathFunction.m_controlProperties != null)
		{
			officeMathFunction.m_controlProperties = m_controlProperties.Clone();
		}
		officeMathFunction.m_equation = m_equation.CloneImpl(officeMathFunction);
		officeMathFunction.m_fName = m_fName.CloneImpl(officeMathFunction);
		return officeMathFunction;
	}
}
