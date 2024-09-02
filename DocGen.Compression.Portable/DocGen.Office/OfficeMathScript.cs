namespace DocGen.Office;

internal class OfficeMathScript : OfficeMathFunctionBase, IOfficeMathScript, IOfficeMathFunctionBase, IOfficeMathEntity
{
	private MathScriptType m_scriptType;

	private OfficeMath m_equation;

	private OfficeMath m_script;

	internal IOfficeRunFormat m_controlProperties;

	public MathScriptType ScriptType
	{
		get
		{
			return m_scriptType;
		}
		set
		{
			m_scriptType = value;
		}
	}

	public IOfficeMath Equation => m_equation;

	public IOfficeMath Script => m_script;

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

	internal OfficeMathScript(IOfficeMathEntity owner)
		: base(owner)
	{
		m_equation = new OfficeMath(this);
		m_script = new OfficeMath(this);
		m_type = MathFunctionType.SubSuperscript;
	}

	internal override void Close()
	{
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
		OfficeMathScript officeMathScript = (OfficeMathScript)MemberwiseClone();
		officeMathScript.SetOwner(owner);
		officeMathScript.m_equation = m_equation.CloneImpl(officeMathScript);
		if (officeMathScript.m_controlProperties != null)
		{
			officeMathScript.m_controlProperties = m_controlProperties.Clone();
		}
		return officeMathScript;
	}
}
