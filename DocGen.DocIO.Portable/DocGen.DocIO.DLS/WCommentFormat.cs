using System;
using System.Globalization;
using DocGen.DocIO.DLS.XML;

namespace DocGen.DocIO.DLS;

public class WCommentFormat : XDLSSerializableBase
{
	private string m_strUser = "";

	private string m_strUserInitials = "";

	private int m_iBookmarkStartOffset = -1;

	private int m_iBookmarkEndOffset = -1;

	private string m_iTagBkmk = "";

	private int m_iPosition;

	private DateTime date;

	public DateTime DateTime
	{
		get
		{
			return date;
		}
		set
		{
			date = value;
		}
	}

	public string UserInitials
	{
		get
		{
			return m_strUserInitials;
		}
		set
		{
			if (value.Length > 9)
			{
				throw new ArgumentOutOfRangeException("UserInitials", "Users initials length must be less than 10 symbols.");
			}
			m_strUserInitials = value;
		}
	}

	public string User
	{
		get
		{
			return m_strUser;
		}
		set
		{
			m_strUser = value;
		}
	}

	internal int BookmarkStartOffset
	{
		get
		{
			return m_iBookmarkStartOffset;
		}
		set
		{
			m_iBookmarkStartOffset = value;
		}
	}

	internal int BookmarkEndOffset
	{
		get
		{
			return m_iBookmarkEndOffset;
		}
		set
		{
			m_iBookmarkEndOffset = value;
		}
	}

	internal string TagBkmk
	{
		get
		{
			return m_iTagBkmk;
		}
		set
		{
			m_iTagBkmk = value;
		}
	}

	internal int Position
	{
		get
		{
			return m_iPosition;
		}
		set
		{
			m_iPosition = value;
		}
	}

	internal int StartTextPos => m_iPosition - m_iBookmarkStartOffset;

	public WCommentFormat()
		: base(null, null)
	{
	}

	protected override void WriteXmlAttributes(IXDLSAttributeWriter writer)
	{
		base.WriteXmlAttributes(writer);
		if (m_strUserInitials != "")
		{
			writer.WriteValue("UserInitials", m_strUserInitials);
		}
		if (m_strUser != "")
		{
			writer.WriteValue("User", m_strUser);
		}
		if (m_iBookmarkStartOffset != -1)
		{
			writer.WriteValue("BookmarkStartPos", m_iBookmarkStartOffset);
		}
		if (m_iBookmarkEndOffset != -1)
		{
			writer.WriteValue("BookmarkEndPos", m_iBookmarkEndOffset);
		}
		if (m_iTagBkmk != "")
		{
			writer.WriteValue("TagBkmk", m_iTagBkmk);
		}
	}

	protected override void ReadXmlAttributes(IXDLSAttributeReader reader)
	{
		base.ReadXmlAttributes(reader);
		if (reader.HasAttribute("User"))
		{
			m_strUser = reader.ReadString("User");
		}
		if (reader.HasAttribute("UserInitials"))
		{
			m_strUserInitials = reader.ReadString("UserInitials");
		}
		if (reader.HasAttribute("BookmarkStartPos"))
		{
			m_iBookmarkStartOffset = reader.ReadInt("BookmarkStartPos");
		}
		if (reader.HasAttribute("BookmarkEndPos"))
		{
			m_iBookmarkEndOffset = reader.ReadInt("BookmarkEndPos");
		}
		if (reader.HasAttribute("TagBkmk"))
		{
			m_iTagBkmk = reader.ReadInt("TagBkmk").ToString();
		}
	}

	public WCommentFormat Clone(IWordDocument doc)
	{
		WCommentFormat wCommentFormat = new WCommentFormat();
		wCommentFormat.m_strUserInitials = m_strUserInitials;
		wCommentFormat.m_strUser = m_strUser;
		wCommentFormat.m_iBookmarkEndOffset = m_iBookmarkEndOffset;
		wCommentFormat.m_iBookmarkStartOffset = m_iBookmarkStartOffset;
		wCommentFormat.date = date;
		if (doc == base.Document || FindTagBkmk(doc, m_iTagBkmk))
		{
			int result = 0;
			int.TryParse(m_iTagBkmk, NumberStyles.Number, CultureInfo.InvariantCulture, out result);
			wCommentFormat.m_iTagBkmk = Convert.ToString(TagIdRandomizer.GetId(result));
		}
		else
		{
			wCommentFormat.m_iTagBkmk = m_iTagBkmk;
		}
		return wCommentFormat;
	}

	private bool FindTagBkmk(IWordDocument doc, string tagBkmk)
	{
		foreach (WSection section in doc.Sections)
		{
			if (IsCommentExists(section.Body, tagBkmk))
			{
				return true;
			}
		}
		int result = 0;
		int.TryParse(m_iTagBkmk, NumberStyles.Number, CultureInfo.InvariantCulture, out result);
		if (TagIdRandomizer.ChangedIds.ContainsKey(result))
		{
			return true;
		}
		return false;
	}

	private bool IsCommentExists(WTextBody body, string tagBkmk)
	{
		foreach (WParagraph paragraph in body.Paragraphs)
		{
			foreach (IParagraphItem item in paragraph.Items)
			{
				if (item is WComment wComment && wComment.Format.TagBkmk == tagBkmk)
				{
					return true;
				}
			}
		}
		foreach (WTable table in body.Tables)
		{
			foreach (WTableRow row in table.Rows)
			{
				foreach (WTableCell cell in row.Cells)
				{
					if (IsCommentExists(cell, tagBkmk))
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	internal void UpdateTagBkmk()
	{
		m_iTagBkmk = Convert.ToString(TagIdRandomizer.Instance.Next());
	}
}
