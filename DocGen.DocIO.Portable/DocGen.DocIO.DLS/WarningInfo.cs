namespace DocGen.DocIO.DLS;

public class WarningInfo
{
	private string m_description;

	private WarningType m_warningType;

	public string Description => m_description;

	public WarningType WarningType => m_warningType;

	internal WarningInfo(string description, WarningType warningType)
	{
		m_description = description;
		m_warningType = warningType;
	}
}
