namespace DocGen.DocIO.DLS;

internal class WCommentExtended
{
	private string m_paraId;

	private string m_parentParaId;

	private bool m_isResolved;

	internal string ParaId
	{
		get
		{
			return m_paraId;
		}
		set
		{
			m_paraId = value;
		}
	}

	internal string ParentParaId
	{
		get
		{
			return m_parentParaId;
		}
		set
		{
			m_parentParaId = value;
		}
	}

	internal bool IsResolved
	{
		get
		{
			return m_isResolved;
		}
		set
		{
			m_isResolved = value;
		}
	}
}
