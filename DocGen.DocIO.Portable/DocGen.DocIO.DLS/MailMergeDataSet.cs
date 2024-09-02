using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace DocGen.DocIO.DLS;

public class MailMergeDataSet
{
	private string DEF_GROUPNAME_PROPERTY = "GroupName";

	private string DEF_SOURCEDATA_PROPERTY = "SourceData";

	private List<object> m_dataSet;

	public List<object> DataSet => m_dataSet;

	public MailMergeDataSet()
	{
		m_dataSet = new List<object>();
	}

	public void Add(object dataTable)
	{
		m_dataSet.Add(dataTable);
	}

	public void Clear()
	{
		m_dataSet.Clear();
		m_dataSet = null;
	}

	internal MailMergeDataTable GetDataTable(string tableName)
	{
		foreach (object item in m_dataSet)
		{
			Type type = item.GetType();
			string text = type.GetRuntimeProperty(DEF_GROUPNAME_PROPERTY).GetValue(item, null).ToString();
			if (!string.IsNullOrEmpty(text) && text == tableName)
			{
				IEnumerator enumerator2 = type.GetRuntimeProperty(DEF_SOURCEDATA_PROPERTY).GetValue(item, null) as IEnumerator;
				MailMergeDataTable result = null;
				if (enumerator2 != null)
				{
					result = new MailMergeDataTable(text, enumerator2);
				}
				return result;
			}
		}
		return null;
	}

	internal void RemoveDataTable(string tableName)
	{
		foreach (object item in m_dataSet)
		{
			string text = item.GetType().GetRuntimeProperty(DEF_GROUPNAME_PROPERTY).GetValue(item, null)
				.ToString();
			if (!string.IsNullOrEmpty(text) && text == tableName)
			{
				m_dataSet.Remove(item);
				break;
			}
		}
	}
}
