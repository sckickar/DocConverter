using System;

namespace DocGen.DocIO.DLS;

public class Hyperlink
{
	private WField m_hyperlink;

	private HyperlinkType m_type;

	private string m_filePath;

	private string m_uriPath;

	private string m_bookmark;

	private string m_textToDisplay;

	private WPicture m_picToDisplay;

	private string m_localReference;

	public string FilePath
	{
		get
		{
			if (m_filePath == null)
			{
				return null;
			}
			return m_filePath.Replace("\"", string.Empty);
		}
		set
		{
			SetFilePathValue(value);
		}
	}

	public string Uri
	{
		get
		{
			if (m_uriPath == null)
			{
				return null;
			}
			return m_uriPath.Replace("\"", string.Empty);
		}
		set
		{
			SetUriValue(value);
			m_uriPath = value;
		}
	}

	public string BookmarkName
	{
		get
		{
			if (m_bookmark != null)
			{
				return m_bookmark.Replace("\"", string.Empty);
			}
			return null;
		}
		set
		{
			SetBookmarkNameValue(value);
			m_bookmark = value;
		}
	}

	public HyperlinkType Type
	{
		get
		{
			return m_type;
		}
		set
		{
			m_type = value;
			UpdateType();
		}
	}

	public string TextToDisplay
	{
		get
		{
			return m_textToDisplay;
		}
		set
		{
			m_textToDisplay = value;
			m_hyperlink.UpdateFieldResult(m_textToDisplay, isFromHyperLink: true);
		}
	}

	public WPicture PictureToDisplay
	{
		get
		{
			return m_picToDisplay;
		}
		set
		{
			m_picToDisplay = value;
			SetImageToDisplay();
		}
	}

	internal WField Field => m_hyperlink;

	public string LocalReference
	{
		get
		{
			return m_localReference;
		}
		set
		{
			SetLocalReferenceValue(value);
			m_localReference = value;
		}
	}

	public Hyperlink(WField hyperlink)
	{
		CheckHyperlink(hyperlink);
		m_hyperlink = hyperlink;
		Parse();
	}

	private void CheckHyperlink(WField field)
	{
		if (field == null)
		{
			throw new ArgumentException("Argument is not a field", "hyperlink");
		}
		if (field.FieldType != FieldType.FieldHyperlink)
		{
			throw new ArgumentException("Argument is not a hyperlink", "hyperlink");
		}
	}

	private void Parse()
	{
		string fieldValue = m_hyperlink.FieldValue;
		if (fieldValue == null || fieldValue == string.Empty)
		{
			return;
		}
		if (StartsWithExt(fieldValue, "\"http"))
		{
			m_type = HyperlinkType.WebLink;
			m_uriPath = fieldValue;
			if (!string.IsNullOrEmpty(m_hyperlink.LocalReference))
			{
				m_localReference = m_hyperlink.LocalReference;
			}
		}
		else if (StartsWithExt(fieldValue, "\"mailto"))
		{
			m_type = HyperlinkType.EMailLink;
			m_uriPath = fieldValue;
		}
		else if (m_hyperlink.IsLocal || m_hyperlink.FormattingString.IndexOf("l") != -1)
		{
			m_type = HyperlinkType.Bookmark;
			if (m_hyperlink.LocalReference != null && m_hyperlink.LocalReference != string.Empty)
			{
				m_bookmark = m_hyperlink.LocalReference;
				m_filePath = fieldValue;
			}
			else
			{
				m_bookmark = fieldValue;
			}
		}
		else
		{
			m_type = HyperlinkType.FileLink;
			m_filePath = fieldValue;
			if (!string.IsNullOrEmpty(m_hyperlink.LocalReference))
			{
				m_localReference = m_hyperlink.LocalReference;
			}
		}
		UpdateTextToDisplay();
		SetHyperlinkFieldCode();
	}

	private void UpdateTextToDisplay()
	{
		if (!(m_hyperlink.Owner is WParagraph))
		{
			return;
		}
		WParagraph wParagraph = m_hyperlink.OwnerParagraph;
		int num = ((m_hyperlink.FieldSeparator != null) ? (m_hyperlink.FieldSeparator.Index + 1) : (-1));
		if (num == -1 || wParagraph.Items.Count <= num)
		{
			return;
		}
		ParagraphItem paragraphItem = wParagraph.Items[num];
		while (!(paragraphItem is WFieldMark))
		{
			paragraphItem = wParagraph.Items[num];
			if (paragraphItem is WTextRange && !(paragraphItem is WField))
			{
				m_textToDisplay += (paragraphItem as WTextRange).Text;
			}
			else if (paragraphItem is WPicture)
			{
				m_picToDisplay = paragraphItem as WPicture;
			}
			num++;
			if (wParagraph.Items.Count <= num)
			{
				if (wParagraph.NextSibling == null || !(wParagraph.NextSibling is WParagraph))
				{
					break;
				}
				wParagraph = wParagraph.NextSibling as WParagraph;
				while (wParagraph.NextSibling != null && wParagraph.NextSibling is WParagraph && wParagraph.Items.Count == 0)
				{
					wParagraph = wParagraph.NextSibling as WParagraph;
				}
				num = ((wParagraph.LastItem is WField) ? 1 : 0);
			}
			paragraphItem = wParagraph.Items[num];
		}
	}

	private void SetImageToDisplay()
	{
		if (m_hyperlink.Owner is WParagraph && Field.FieldSeparator != null && Field.FieldSeparator.OwnerParagraph != null)
		{
			Field.RemoveFieldResult();
			Field.FieldSeparator.OwnerParagraph.Items.Insert(Field.FieldSeparator.Index + 1, m_picToDisplay);
		}
	}

	private int FindHyperlinkText(ref WParagraph ownerPara)
	{
		Entity entity = m_hyperlink;
		int num = 0;
		while (entity.NextSibling != null)
		{
			if (entity is WFieldMark && (entity as WFieldMark).Type == FieldMarkType.FieldSeparator && entity.NextSibling is WFieldMark && (entity.NextSibling as WFieldMark).Type == FieldMarkType.FieldEnd)
			{
				return -1;
			}
			if (entity.NextSibling is InlineShapeObject || entity.NextSibling is WFieldMark)
			{
				num++;
			}
			else if (entity.NextSibling is WTextRange || entity.NextSibling is WPicture)
			{
				num++;
				break;
			}
			if (num > 3)
			{
				num = -1;
				break;
			}
			entity = entity.NextSibling as Entity;
		}
		int num2 = m_hyperlink.GetIndexInOwnerCollection() + num;
		if (ownerPara.Items.Count <= num2)
		{
			if (ownerPara.NextSibling == null)
			{
				return -1;
			}
			num2 = ((ownerPara.LastItem is WField) ? 1 : 0);
			ownerPara = ownerPara.NextSibling as WParagraph;
		}
		ParagraphItem paragraphItem = ownerPara.Items[num2];
		if (paragraphItem is WTextRange || paragraphItem is WPicture)
		{
			return num2;
		}
		return -1;
	}

	private void UpdateType()
	{
		if (m_type == HyperlinkType.Bookmark)
		{
			m_hyperlink.IsLocal = true;
			return;
		}
		m_hyperlink.LocalReference = null;
		m_hyperlink.m_formattingString = string.Empty;
	}

	private void SetUriValue(string uri)
	{
		if (m_type != HyperlinkType.WebLink && m_type != HyperlinkType.EMailLink)
		{
			throw new ArgumentException("Uri can be set only for \"WebLink\" or \"EMailLink\" types of hyperlink");
		}
		uri = CheckUri(uri);
		uri = CheckValue(uri);
		m_hyperlink.m_fieldValue = uri;
		SetHyperlinkFieldCode();
	}

	private void SetBookmarkNameValue(string name)
	{
		if (m_type != HyperlinkType.Bookmark)
		{
			throw new ArgumentException("Bookmark name can be set only for \"Bookmark\" type of hyperlink");
		}
		name = CheckValue(name);
		m_hyperlink.m_fieldValue = name;
		SetHyperlinkFieldCode();
	}

	private void SetLocalReferenceValue(string localReference)
	{
		if (m_type != HyperlinkType.WebLink && m_type != HyperlinkType.FileLink)
		{
			throw new ArgumentException("Local reference can be set only for \"FileLink\" or \"WebLink\" type of hyperlink");
		}
		m_hyperlink.LocalReference = localReference;
		SetHyperlinkFieldCode();
	}

	private void SetFilePathValue(string filePath)
	{
		if (m_type != HyperlinkType.FileLink && m_type != HyperlinkType.Bookmark)
		{
			throw new ArgumentException("File path can be set only for \"FileLink\" or \"Bookmark\" type of hyperlink");
		}
		filePath = CheckPath(filePath);
		filePath = CheckValue(filePath);
		m_hyperlink.m_fieldValue = filePath;
		if (m_type == HyperlinkType.Bookmark)
		{
			if (m_bookmark == null || m_bookmark == string.Empty)
			{
				throw new ArgumentException("Bookmark name can't be null or empty. Bookmark name must be set before file path.");
			}
			m_hyperlink.LocalReference = CheckValue(m_bookmark);
		}
		m_filePath = filePath;
		SetHyperlinkFieldCode();
	}

	private string CheckValue(string value)
	{
		if (!StartsWithExt(value, "\""))
		{
			value = "\"" + value;
		}
		if (!value.EndsWith("\""))
		{
			value += "\"";
		}
		return value;
	}

	private string CheckPath(string path)
	{
		char[] separator = new char[1] { '\\' };
		string[] array = path.Split(separator);
		path = string.Empty;
		int i = 0;
		for (int num = array.Length; i < num; i++)
		{
			if (array[i] != string.Empty)
			{
				path += array[i];
				if (i < num - 1)
				{
					path += "\\\\";
				}
			}
		}
		return path;
	}

	private string CheckUri(string uri)
	{
		uri = uri.Replace("\"", string.Empty);
		if (m_type == HyperlinkType.WebLink)
		{
			if (!uri.Contains("http://") && !uri.Contains("https://"))
			{
				if (StartsWithExt(uri.ToLower(), "www."))
				{
					uri = "http://" + uri;
				}
				else if (uri.Contains("@") && !StartsWithExt(uri.ToLower(), "mailto:"))
				{
					uri = "mailto:" + uri;
				}
			}
		}
		else if (!StartsWithExt(uri.ToLower(), "mailto:"))
		{
			uri = "mailto:" + uri;
		}
		return uri;
	}

	internal void SetHyperlinkFieldCode()
	{
		string empty = string.Empty;
		if (Type == HyperlinkType.Bookmark || m_hyperlink.IsLocal)
		{
			if (string.IsNullOrEmpty(m_hyperlink.LocalReference))
			{
				empty = "HYPERLINK \\l " + m_hyperlink.FieldValue;
			}
			else
			{
				char[] trimChars = new char[1] { '"' };
				m_hyperlink.LocalReference = m_hyperlink.LocalReference.Trim(trimChars);
				empty = "HYPERLINK " + m_hyperlink.FieldValue + " \\l \"" + m_hyperlink.LocalReference + "\"";
			}
		}
		else
		{
			empty = "HYPERLINK " + m_hyperlink.FieldValue + " " + m_hyperlink.LocalReference;
		}
		if (!string.IsNullOrEmpty(m_hyperlink.ScreenTip))
		{
			string text = "\\o \"" + m_hyperlink.ScreenTip + "\"";
			empty += text;
			m_hyperlink.m_formattingString += text;
		}
		if (m_hyperlink.Owner == null && !m_hyperlink.Document.IsInternalManipulation())
		{
			m_hyperlink.m_detachedFieldCode = empty;
		}
		else if (m_hyperlink.NextSibling is WTextRange)
		{
			(m_hyperlink.NextSibling as WTextRange).Text = empty;
		}
	}

	internal bool StartsWithExt(string text, string value)
	{
		return text.StartsWith(value);
	}
}
