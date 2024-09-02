using System;
using System.Collections.Generic;
using DocGen.CompoundFile.DocIO;
using DocGen.DocIO.DLS.XML;

namespace DocGen.DocIO.DLS;

public class SummaryDocumentProperties : XDLSSerializableBase
{
	protected Dictionary<int, DocumentProperty> m_summaryHash;

	protected Dictionary<int, string> m_internalSummaryHash;

	internal const int ContentStatusKey = 1;

	public string Author
	{
		get
		{
			if (!m_summaryHash.ContainsKey(4))
			{
				return null;
			}
			return this[PIDSI.Author].Text;
		}
		set
		{
			SetPropertyValue(PIDSI.Author, value);
			this[PIDSI.Author].Text = value;
		}
	}

	public string ApplicationName
	{
		get
		{
			if (!m_summaryHash.ContainsKey(18))
			{
				return null;
			}
			return this[PIDSI.Appname].Text;
		}
		set
		{
			SetPropertyValue(PIDSI.Appname, value);
			this[PIDSI.Appname].Text = value;
		}
	}

	public string Title
	{
		get
		{
			if (!m_summaryHash.ContainsKey(2))
			{
				return null;
			}
			return this[PIDSI.Title].Text;
		}
		set
		{
			SetPropertyValue(PIDSI.Title, value);
			this[PIDSI.Title].Text = value;
		}
	}

	public string Subject
	{
		get
		{
			if (!m_summaryHash.ContainsKey(3))
			{
				return null;
			}
			return this[PIDSI.Subject].Text;
		}
		set
		{
			SetPropertyValue(PIDSI.Subject, value);
			this[PIDSI.Subject].Text = value;
		}
	}

	public string Keywords
	{
		get
		{
			if (!m_summaryHash.ContainsKey(5))
			{
				return null;
			}
			return this[PIDSI.Keywords].Text;
		}
		set
		{
			SetPropertyValue(PIDSI.Keywords, value);
			this[PIDSI.Keywords].Text = value;
		}
	}

	public string Comments
	{
		get
		{
			if (!m_summaryHash.ContainsKey(6))
			{
				return null;
			}
			return this[PIDSI.Comments].Text;
		}
		set
		{
			SetPropertyValue(PIDSI.Comments, value);
			this[PIDSI.Comments].Text = value;
		}
	}

	public string Template
	{
		get
		{
			if (!m_summaryHash.ContainsKey(7))
			{
				return null;
			}
			return this[PIDSI.Template].Text;
		}
		set
		{
			SetPropertyValue(PIDSI.Template, value);
			this[PIDSI.Template].Value = value;
		}
	}

	public string LastAuthor
	{
		get
		{
			if (!m_summaryHash.ContainsKey(8))
			{
				return null;
			}
			return this[PIDSI.LastAuthor].Text;
		}
		set
		{
			SetPropertyValue(PIDSI.LastAuthor, value);
			this[PIDSI.LastAuthor].Text = value;
		}
	}

	public string RevisionNumber
	{
		get
		{
			if (!m_summaryHash.ContainsKey(9))
			{
				return null;
			}
			return this[PIDSI.Revnumber].Text;
		}
		set
		{
			SetPropertyValue(PIDSI.Revnumber, value);
			this[PIDSI.Revnumber].Value = value;
		}
	}

	public TimeSpan TotalEditingTime
	{
		get
		{
			if (m_summaryHash.ContainsKey(10))
			{
				if (!(this[PIDSI.EditTime].TimeSpan < TimeSpan.Zero))
				{
					return this[PIDSI.EditTime].TimeSpan;
				}
				return TimeSpan.Zero;
			}
			return TimeSpan.MinValue;
		}
		set
		{
			SetPropertyValue(PIDSI.EditTime, value);
			this[PIDSI.EditTime].Value = value;
		}
	}

	public DateTime LastPrinted
	{
		get
		{
			if (!m_summaryHash.ContainsKey(11))
			{
				return DateTime.MinValue;
			}
			return this[PIDSI.LastPrinted].DateTime;
		}
		set
		{
			SetPropertyValue(PIDSI.LastPrinted, value);
			this[PIDSI.LastPrinted].DateTime = value;
		}
	}

	public DateTime CreateDate
	{
		get
		{
			if (!m_summaryHash.ContainsKey(12))
			{
				return DateTime.Now;
			}
			return this[PIDSI.Create_dtm].DateTime;
		}
		set
		{
			if (!value.Equals(default(DateTime)))
			{
				if (value.CompareTo(new DateTime(1900, 12, 31)) > 0)
				{
					SetPropertyValue(PIDSI.Create_dtm, value);
					this[PIDSI.Create_dtm].DateTime = value;
				}
				else if (!base.Document.IsOpening)
				{
					throw new Exception("Date time value must be after 12/31/1900(MM/DD/YYYY).");
				}
			}
			else if (m_summaryHash.ContainsKey(12))
			{
				m_summaryHash.Remove(12);
			}
		}
	}

	public DateTime LastSaveDate
	{
		get
		{
			if (!m_summaryHash.ContainsKey(13))
			{
				return CreateDate;
			}
			return this[PIDSI.LastSave_dtm].DateTime;
		}
		set
		{
			if (!value.Equals(default(DateTime)))
			{
				if (value.CompareTo(new DateTime(1900, 12, 31)) > 0)
				{
					SetPropertyValue(PIDSI.LastSave_dtm, value);
					this[PIDSI.LastSave_dtm].DateTime = value;
				}
				else if (!base.Document.IsOpening)
				{
					throw new Exception("Date time value must be after 12/31/1900(MM/DD/YYYY).");
				}
			}
			else if (m_summaryHash.ContainsKey(13))
			{
				m_summaryHash.Remove(13);
			}
		}
	}

	public int PageCount
	{
		get
		{
			if (!m_summaryHash.ContainsKey(14))
			{
				return int.MinValue;
			}
			return this[PIDSI.Pagecount].ToInt();
		}
		internal set
		{
			SetPropertyValue(PIDSI.Pagecount, value);
			this[PIDSI.Pagecount].Int32 = value;
		}
	}

	public int WordCount
	{
		get
		{
			if (!m_summaryHash.ContainsKey(15))
			{
				return int.MinValue;
			}
			return this[PIDSI.Wordcount].ToInt();
		}
		internal set
		{
			SetPropertyValue(PIDSI.Wordcount, value);
			this[PIDSI.Wordcount].Int32 = value;
		}
	}

	public int CharCount
	{
		get
		{
			if (!m_summaryHash.ContainsKey(16))
			{
				return int.MinValue;
			}
			return this[PIDSI.Charcount].ToInt();
		}
		internal set
		{
			SetPropertyValue(PIDSI.Charcount, value);
			this[PIDSI.Charcount].Int32 = value;
		}
	}

	public string Thumbnail
	{
		get
		{
			if (!m_summaryHash.ContainsKey(17))
			{
				return null;
			}
			return this[PIDSI.Thumbnail].Text;
		}
		set
		{
			SetPropertyValue(PIDSI.Thumbnail, value);
			this[PIDSI.Thumbnail].Text = value;
		}
	}

	public int DocSecurity
	{
		get
		{
			if (!m_summaryHash.ContainsKey(19))
			{
				return int.MinValue;
			}
			return this[PIDSI.Doc_security].ToInt();
		}
		set
		{
			SetPropertyValue(PIDSI.Doc_security, value);
			this[PIDSI.Doc_security].Int32 = value;
		}
	}

	internal string ContentStatus
	{
		get
		{
			if (HasKeyValue(1))
			{
				return m_internalSummaryHash[1];
			}
			return null;
		}
		set
		{
			SetKeyValue(1, value);
		}
	}

	internal DocumentProperty this[PIDSI pidsi] => m_summaryHash[(int)pidsi];

	public int Count => m_summaryHash.Count;

	internal Dictionary<int, DocumentProperty> SummaryHash => m_summaryHash;

	internal SummaryDocumentProperties()
		: this(0)
	{
	}

	internal SummaryDocumentProperties(int count)
		: base(null, null)
	{
		m_summaryHash = new Dictionary<int, DocumentProperty>(count);
		m_internalSummaryHash = new Dictionary<int, string>();
	}

	internal SummaryDocumentProperties(WordDocument doc)
		: base(doc, null)
	{
		m_summaryHash = new Dictionary<int, DocumentProperty>();
		m_internalSummaryHash = new Dictionary<int, string>();
	}

	private bool HasKey(int key)
	{
		return m_summaryHash.ContainsKey(key);
	}

	public void Add(int key, DocumentProperty props)
	{
		m_summaryHash.Add(key, props);
	}

	internal void SetPropertyValue(PIDSI pidsi, object value)
	{
		if (m_summaryHash.ContainsKey((int)pidsi))
		{
			this[pidsi].Value = value;
			return;
		}
		DocumentProperty value2 = new DocumentProperty((BuiltInProperty)pidsi, value);
		m_summaryHash[(int)pidsi] = value2;
	}

	internal bool HasKeyValue(int Key)
	{
		if (m_internalSummaryHash != null && m_internalSummaryHash.ContainsKey(Key))
		{
			return true;
		}
		return false;
	}

	internal void SetKeyValue(int propKey, string value)
	{
		m_internalSummaryHash[propKey] = value;
	}

	internal override void Close()
	{
		base.Close();
		if (m_summaryHash != null)
		{
			foreach (DocumentProperty value in m_summaryHash.Values)
			{
				value.Close();
			}
			m_summaryHash.Clear();
			m_summaryHash = null;
		}
		if (m_internalSummaryHash != null)
		{
			m_internalSummaryHash.Clear();
			m_internalSummaryHash = null;
		}
	}
}
