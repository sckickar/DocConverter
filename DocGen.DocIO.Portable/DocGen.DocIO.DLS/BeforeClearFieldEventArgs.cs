namespace DocGen.DocIO.DLS;

public class BeforeClearFieldEventArgs
{
	private WordDocument m_doc;

	private IWMergeField m_currentMergeField;

	private string m_groupName;

	private object m_fieldValue;

	private int m_rowIndex;

	private bool m_clearField;

	private bool m_fieldHasMappedInDataSource;

	internal WordDocument Doc => m_doc;

	public string FieldName
	{
		get
		{
			if (m_currentMergeField.Prefix != string.Empty)
			{
				return m_currentMergeField.Prefix + ":" + m_currentMergeField.FieldName;
			}
			return m_currentMergeField.FieldName;
		}
	}

	public object FieldValue
	{
		get
		{
			return m_fieldValue;
		}
		set
		{
			m_fieldValue = value;
		}
	}

	public string GroupName => m_groupName;

	public bool HasMappedFieldInDataSource => m_fieldHasMappedInDataSource;

	public int RowIndex => m_rowIndex;

	public bool ClearField
	{
		get
		{
			return m_clearField;
		}
		set
		{
			m_clearField = value;
		}
	}

	public IWMergeField CurrentMergeField => m_currentMergeField;

	public BeforeClearFieldEventArgs(WordDocument doc, string groupName, int rowIndex, IWMergeField field, bool fieldHasMappedInDataSource, object value)
	{
		m_doc = doc;
		m_currentMergeField = field;
		m_fieldValue = value;
		m_groupName = groupName;
		m_rowIndex = rowIndex;
		m_fieldHasMappedInDataSource = fieldHasMappedInDataSource;
		m_clearField = Doc.MailMerge.ClearFields;
	}
}
