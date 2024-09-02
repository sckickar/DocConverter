using System.Collections.Generic;
using DocGen.CompoundFile.DocIO;

namespace DocGen.DocIO.DLS;

public class BuiltinDocumentProperties : SummaryDocumentProperties
{
	private Dictionary<int, DocumentProperty> m_documentHash;

	public string Category
	{
		get
		{
			if (!m_documentHash.ContainsKey(1000))
			{
				return null;
			}
			return this[BuiltInProperty.Category].Text;
		}
		set
		{
			SetPropertyValue(BuiltInProperty.Category, value);
			this[BuiltInProperty.Category].Text = value;
		}
	}

	public int BytesCount
	{
		get
		{
			if (!m_documentHash.ContainsKey(1002))
			{
				return int.MinValue;
			}
			return this[BuiltInProperty.ByteCount].Int32;
		}
		internal set
		{
			SetPropertyValue(BuiltInProperty.ByteCount, value);
			this[BuiltInProperty.ByteCount].Int32 = value;
		}
	}

	public int LinesCount
	{
		get
		{
			if (!m_documentHash.ContainsKey(1003))
			{
				return int.MinValue;
			}
			return this[BuiltInProperty.LineCount].ToInt();
		}
		internal set
		{
			SetPropertyValue(BuiltInProperty.LineCount, value);
			this[BuiltInProperty.LineCount].Int32 = value;
		}
	}

	public int ParagraphCount
	{
		get
		{
			if (!m_documentHash.ContainsKey(1004))
			{
				return int.MinValue;
			}
			return this[BuiltInProperty.ParagraphCount].ToInt();
		}
		internal set
		{
			SetPropertyValue(BuiltInProperty.ParagraphCount, value);
			this[BuiltInProperty.ParagraphCount].Int32 = value;
		}
	}

	public int SlideCount
	{
		get
		{
			if (!m_documentHash.ContainsKey(1005))
			{
				return int.MinValue;
			}
			return this[BuiltInProperty.SlideCount].ToInt();
		}
		internal set
		{
			SetPropertyValue(BuiltInProperty.SlideCount, value);
			this[BuiltInProperty.SlideCount].Int32 = value;
		}
	}

	public int NoteCount
	{
		get
		{
			if (!m_documentHash.ContainsKey(1006))
			{
				return int.MinValue;
			}
			return this[BuiltInProperty.NoteCount].ToInt();
		}
		internal set
		{
			SetPropertyValue(BuiltInProperty.NoteCount, value);
			this[BuiltInProperty.NoteCount].Int32 = value;
		}
	}

	public int HiddenCount
	{
		get
		{
			if (!m_documentHash.ContainsKey(1007))
			{
				return int.MinValue;
			}
			return this[BuiltInProperty.HiddenCount].ToInt();
		}
		internal set
		{
			SetPropertyValue(BuiltInProperty.HiddenCount, value);
			this[BuiltInProperty.HiddenCount].Int32 = value;
		}
	}

	public string Company
	{
		get
		{
			if (!m_documentHash.ContainsKey(1013))
			{
				return null;
			}
			return this[BuiltInProperty.Company].Text;
		}
		set
		{
			SetPropertyValue(BuiltInProperty.Company, value);
			this[BuiltInProperty.Company].Text = value;
		}
	}

	public string Manager
	{
		get
		{
			if (!m_documentHash.ContainsKey(1012))
			{
				return null;
			}
			return this[BuiltInProperty.Manager].Text;
		}
		set
		{
			SetPropertyValue(BuiltInProperty.Manager, value);
			this[BuiltInProperty.Manager].Text = value;
		}
	}

	internal Dictionary<int, DocumentProperty> DocumentHash => m_documentHash;

	internal DocumentProperty this[BuiltInProperty property] => m_documentHash[(int)property];

	internal BuiltinDocumentProperties()
		: this(0, 0)
	{
	}

	internal BuiltinDocumentProperties(int docCount, int summCount)
		: base(summCount)
	{
		m_documentHash = new Dictionary<int, DocumentProperty>(docCount);
	}

	internal BuiltinDocumentProperties(WordDocument doc)
		: base(doc)
	{
		m_documentHash = new Dictionary<int, DocumentProperty>();
	}

	private bool HasKey(int key)
	{
		return m_documentHash.ContainsKey(key);
	}

	public BuiltinDocumentProperties Clone()
	{
		BuiltinDocumentProperties builtinDocumentProperties = new BuiltinDocumentProperties(m_documentHash.Count, m_summaryHash.Count);
		foreach (int key in m_documentHash.Keys)
		{
			DocumentProperty documentProperty = m_documentHash[key];
			builtinDocumentProperties.m_documentHash.Add(key, documentProperty.Clone());
		}
		foreach (int key2 in m_summaryHash.Keys)
		{
			DocumentProperty documentProperty2 = m_summaryHash[key2];
			builtinDocumentProperties.m_summaryHash.Add(key2, documentProperty2.Clone());
		}
		foreach (int key3 in m_internalSummaryHash.Keys)
		{
			builtinDocumentProperties.m_internalSummaryHash.Add(key3, m_internalSummaryHash[key3]);
		}
		return builtinDocumentProperties;
	}

	internal void SetPropertyValue(BuiltInProperty builtInProperty, object value)
	{
		if (m_documentHash.ContainsKey((int)builtInProperty))
		{
			this[builtInProperty].Value = value;
			return;
		}
		DocumentProperty value2 = new DocumentProperty(builtInProperty, value);
		m_documentHash[(int)builtInProperty] = value2;
	}

	internal override void Close()
	{
		base.Close();
		if (m_documentHash == null)
		{
			return;
		}
		foreach (DocumentProperty value in m_documentHash.Values)
		{
			value.Close();
		}
		m_documentHash.Clear();
		m_documentHash = null;
	}
}
