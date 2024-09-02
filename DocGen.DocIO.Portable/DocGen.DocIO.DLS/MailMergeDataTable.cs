using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace DocGen.DocIO.DLS;

public class MailMergeDataTable
{
	private string m_groupName;

	private IEnumerator m_sourceData;

	private int m_matchingRecordsCount;

	private string m_command;

	public string GroupName => m_groupName;

	public IEnumerator SourceData => m_sourceData;

	internal int MatchingRecordsCount
	{
		get
		{
			return m_matchingRecordsCount;
		}
		set
		{
			m_matchingRecordsCount = value;
		}
	}

	internal string Command
	{
		get
		{
			return m_command;
		}
		set
		{
			m_command = value;
		}
	}

	public MailMergeDataTable(string groupName, IEnumerable enumerable)
	{
		m_groupName = groupName;
		m_sourceData = enumerable.GetEnumerator();
	}

	internal MailMergeDataTable(string groupName, IEnumerator enumerator)
	{
		m_groupName = groupName;
		m_sourceData = enumerator;
	}

	internal MailMergeDataTable Select(string command)
	{
		string[] array = command.Split(new char[1] { ' ' });
		string text = array[0];
		string text2 = array[2];
		List<object> list = new List<object>();
		m_sourceData.Reset();
		while (m_sourceData.MoveNext())
		{
			if (m_sourceData.Current is IDictionary<string, object>)
			{
				m_sourceData.Reset();
				while (m_sourceData.MoveNext())
				{
					if ((m_sourceData.Current as IDictionary<string, object>).ContainsKey(text) && (text2 == (m_sourceData.Current as IDictionary<string, object>)[text].ToString() || (array[1].ToLower() == "contains" && (m_sourceData.Current as IDictionary<string, object>)[text].ToString().Contains(text2))))
					{
						MatchingRecordsCount++;
					}
				}
				return new MailMergeDataTable(GroupName, list.GetEnumerator());
			}
			object value = m_sourceData.Current.GetType().GetRuntimeProperty(text).GetValue(m_sourceData.Current, null);
			if (text2 == value.ToString())
			{
				MatchingRecordsCount++;
			}
		}
		return new MailMergeDataTable(GroupName, list.GetEnumerator());
	}
}
