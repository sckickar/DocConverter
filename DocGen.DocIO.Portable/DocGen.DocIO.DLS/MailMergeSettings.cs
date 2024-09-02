using System.Collections.Generic;
using System.IO;

namespace DocGen.DocIO.DLS;

public class MailMergeSettings
{
	internal const byte ActiveRecordKey = 0;

	internal const byte AddressFieldNameKey = 1;

	internal const byte CheckErrorsKey = 2;

	internal const byte ConnectStringKey = 3;

	internal const byte DataSourceKey = 4;

	internal const byte DataTypeKey = 5;

	internal const byte DestinationKey = 6;

	internal const byte DoNotSupressBlankLinesKey = 7;

	internal const byte HeaderSourceKey = 8;

	internal const byte LinkToQueryKey = 9;

	internal const byte MailAsAttachmentKey = 10;

	internal const byte MailSubjectKey = 11;

	internal const byte MainDocumentTypeKey = 12;

	internal const byte QueryKey = 13;

	internal const byte ViewMergedDataKey = 14;

	internal const byte ODSOSettingsKey = 15;

	private Dictionary<int, object> m_propertiesHash;

	internal int ActiveRecord
	{
		get
		{
			if (HasKey(0))
			{
				return (int)PropertiesHash[0];
			}
			return 1;
		}
		set
		{
			SetKeyValue(0, value);
		}
	}

	internal string AddressFieldName
	{
		get
		{
			if (HasKey(1))
			{
				return (string)PropertiesHash[1];
			}
			return string.Empty;
		}
		set
		{
			SetKeyValue(1, value);
		}
	}

	internal MailMergeCheckErrors CheckErrors
	{
		get
		{
			if (HasKey(2))
			{
				return (MailMergeCheckErrors)PropertiesHash[2];
			}
			return MailMergeCheckErrors.PauseOnError;
		}
		set
		{
			SetKeyValue(2, value);
		}
	}

	internal string ConnectString
	{
		get
		{
			if (HasKey(3))
			{
				return (string)PropertiesHash[3];
			}
			return string.Empty;
		}
		set
		{
			SetKeyValue(3, value);
		}
	}

	public string DataSource
	{
		get
		{
			if (HasKey(4))
			{
				return ((string)PropertiesHash[4]).Replace("file:///", "");
			}
			return string.Empty;
		}
		set
		{
			SetKeyValue(4, value);
		}
	}

	internal MailMergeDataType DataType
	{
		get
		{
			if (HasKey(5))
			{
				return (MailMergeDataType)PropertiesHash[5];
			}
			return MailMergeDataType.Native;
		}
		set
		{
			SetKeyValue(5, value);
		}
	}

	internal MailMergeDestination Destination
	{
		get
		{
			if (HasKey(6))
			{
				return (MailMergeDestination)PropertiesHash[6];
			}
			return MailMergeDestination.NewDocument;
		}
		set
		{
			SetKeyValue(6, value);
		}
	}

	internal bool DoNotSupressBlankLines
	{
		get
		{
			if (HasKey(7))
			{
				return (bool)PropertiesHash[7];
			}
			return false;
		}
		set
		{
			SetKeyValue(7, value);
		}
	}

	internal string HeaderSource
	{
		get
		{
			if (HasKey(8))
			{
				return (string)PropertiesHash[8];
			}
			return string.Empty;
		}
		set
		{
			SetKeyValue(8, value);
		}
	}

	internal bool LinkToQuery
	{
		get
		{
			if (HasKey(9))
			{
				return (bool)PropertiesHash[9];
			}
			return false;
		}
		set
		{
			SetKeyValue(9, value);
		}
	}

	internal bool MailAsAttachment
	{
		get
		{
			if (HasKey(10))
			{
				return (bool)PropertiesHash[10];
			}
			return false;
		}
		set
		{
			SetKeyValue(10, value);
		}
	}

	internal string MailSubject
	{
		get
		{
			if (HasKey(11))
			{
				return (string)PropertiesHash[11];
			}
			return string.Empty;
		}
		set
		{
			SetKeyValue(11, value);
		}
	}

	internal MailMergeMainDocumentType MainDocumentType
	{
		get
		{
			if (HasKey(12))
			{
				return (MailMergeMainDocumentType)PropertiesHash[12];
			}
			return MailMergeMainDocumentType.FormLetters;
		}
		set
		{
			SetKeyValue(12, value);
		}
	}

	internal string Query
	{
		get
		{
			if (HasKey(13))
			{
				return (string)PropertiesHash[13];
			}
			return string.Empty;
		}
		set
		{
			SetKeyValue(13, value);
		}
	}

	internal bool ViewMergedData
	{
		get
		{
			if (HasKey(14))
			{
				return (bool)PropertiesHash[14];
			}
			return false;
		}
		set
		{
			SetKeyValue(14, value);
		}
	}

	internal Stream ODSOSettings
	{
		get
		{
			if (HasKey(15))
			{
				return (Stream)PropertiesHash[15];
			}
			return null;
		}
		set
		{
			SetKeyValue(15, value);
		}
	}

	private Dictionary<int, object> PropertiesHash
	{
		get
		{
			if (m_propertiesHash == null)
			{
				m_propertiesHash = new Dictionary<int, object>();
			}
			return m_propertiesHash;
		}
	}

	public bool HasData
	{
		get
		{
			if (m_propertiesHash != null && m_propertiesHash.Count > 0)
			{
				return true;
			}
			return false;
		}
	}

	internal MailMergeSettings()
	{
	}

	internal bool HasKey(int Key)
	{
		if (m_propertiesHash != null && m_propertiesHash.ContainsKey(Key))
		{
			return true;
		}
		return false;
	}

	private void SetKeyValue(int propKey, object value)
	{
		PropertiesHash[propKey] = value;
	}

	internal void Close()
	{
		if (m_propertiesHash != null)
		{
			if (m_propertiesHash.ContainsKey(15))
			{
				((Stream)m_propertiesHash[15]).Dispose();
			}
			m_propertiesHash.Clear();
			m_propertiesHash = null;
		}
	}

	public void RemoveData()
	{
		Close();
	}
}
