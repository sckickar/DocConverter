namespace DocGen.Compression.Zip;

public class BeginsWithNamePreprocessor : IFileNamePreprocessor
{
	private string m_strStartToRemove;

	public BeginsWithNamePreprocessor(string startToRemove)
	{
		m_strStartToRemove = startToRemove;
	}

	public string PreprocessName(string fullName)
	{
		string result = fullName;
		if (m_strStartToRemove != null && fullName.StartsWith(m_strStartToRemove))
		{
			result = fullName.Remove(0, m_strStartToRemove.Length);
		}
		return result;
	}
}
