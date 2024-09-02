namespace DocGen.Pdf.Security;

internal class ObjectIdentityToken
{
	private string m_id;

	private int m_index;

	internal bool HasMoreTokens => m_index != -1;

	internal ObjectIdentityToken(string id)
	{
		m_id = id;
	}

	internal string NextToken()
	{
		if (m_index == -1)
		{
			return null;
		}
		int num = m_id.IndexOf('.', m_index);
		if (num == -1)
		{
			string result = m_id.Substring(m_index);
			m_index = -1;
			return result;
		}
		string result2 = m_id.Substring(m_index, num - m_index);
		m_index = num + 1;
		return result2;
	}
}
