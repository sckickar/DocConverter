namespace DocGen.Office;

internal class OfficeMathLeftScript : OfficeMathFunctionBase, IOfficeMathLeftScript, IOfficeMathFunctionBase, IOfficeMathEntity
{
	private OfficeMath m_equation;

	private OfficeMath m_subScript;

	private OfficeMath m_superScript;

	internal IOfficeRunFormat m_controlProperties;

	public IOfficeMath Equation => m_equation;

	public IOfficeMath Subscript => m_subScript;

	public IOfficeMath Superscript => m_superScript;

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

	internal OfficeMathLeftScript(IOfficeMathEntity owner)
		: base(owner)
	{
		m_type = MathFunctionType.LeftSubSuperscript;
		m_equation = new OfficeMath(this);
		m_subScript = new OfficeMath(this);
		m_superScript = new OfficeMath(this);
	}

	internal override void Close()
	{
		if (m_superScript != null)
		{
			m_superScript.Close();
		}
		if (m_subScript != null)
		{
			m_subScript.Close();
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
		OfficeMathLeftScript officeMathLeftScript = (OfficeMathLeftScript)MemberwiseClone();
		officeMathLeftScript.SetOwner(owner);
		if (officeMathLeftScript.m_controlProperties != null)
		{
			officeMathLeftScript.m_controlProperties = m_controlProperties.Clone();
		}
		officeMathLeftScript.m_equation = m_equation.CloneImpl(officeMathLeftScript);
		officeMathLeftScript.m_subScript = m_subScript.CloneImpl(officeMathLeftScript);
		officeMathLeftScript.m_superScript = m_superScript.CloneImpl(officeMathLeftScript);
		return officeMathLeftScript;
	}
}
