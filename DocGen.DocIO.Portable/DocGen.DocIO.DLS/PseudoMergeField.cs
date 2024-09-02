using System.Text.RegularExpressions;

namespace DocGen.DocIO.DLS;

internal class PseudoMergeField
{
	private bool m_fitMailMerge;

	private string m_name;

	private string m_value;

	internal string Name => m_name;

	internal string Value
	{
		get
		{
			return m_value;
		}
		set
		{
			m_value = value;
		}
	}

	internal bool FitMailMerge => m_fitMailMerge;

	internal PseudoMergeField(string fieldText)
	{
		if (fieldText == null)
		{
			return;
		}
		if (fieldText.IndexOf("MERGEFIELD") == -1)
		{
			char[] separator = new char[1] { '"' };
			string[] array = fieldText.Split(separator);
			if (array.Length == 1)
			{
				m_value = fieldText.Trim();
			}
			else if (array.Length == 3)
			{
				m_value = array[1];
			}
			else
			{
				m_value = string.Empty;
			}
		}
		else
		{
			Match match = new Regex("MERGEFIELD\\s+\"?([^\"]+)\"").Match(fieldText);
			if (match.Groups.Count > 1)
			{
				m_name = match.Groups[1].Value;
				m_fitMailMerge = true;
			}
		}
	}
}
