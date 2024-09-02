using System.Collections;

namespace DocGen.DocIO.DLS;

public class BeforeClearGroupFieldEventArgs
{
	private WordDocument m_doc;

	private IWMergeField m_currentMergeField;

	private string m_groupName;

	private bool m_clearGroup;

	private bool m_fieldHasMappedInDataSource;

	private string[] m_fieldNames;

	private IEnumerable m_alternateValues;

	internal WordDocument Doc => m_doc;

	public string GroupName => m_groupName;

	public bool HasMappedGroupInDataSource => m_fieldHasMappedInDataSource;

	public bool ClearGroup
	{
		get
		{
			return m_clearGroup;
		}
		set
		{
			m_clearGroup = value;
		}
	}

	public string[] FieldNames => m_fieldNames;

	public IEnumerable AlternateValues
	{
		get
		{
			return m_alternateValues;
		}
		set
		{
			m_alternateValues = value;
			MailMergeDataTable dataSource = new MailMergeDataTable(m_groupName.Split(':')[^1], m_alternateValues);
			Doc.MailMerge.ExecuteGroup(dataSource);
		}
	}

	public BeforeClearGroupFieldEventArgs(WordDocument doc, string groupName, IWMergeField field, bool fieldHasMappedInDataSource, string[] fieldNames)
	{
		m_doc = doc;
		m_groupName = groupName;
		m_fieldHasMappedInDataSource = fieldHasMappedInDataSource;
		m_currentMergeField = field;
		m_fieldNames = fieldNames;
		m_clearGroup = Doc.MailMerge.ClearFields;
	}
}
