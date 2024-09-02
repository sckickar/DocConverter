using System.Collections.Generic;

namespace DocGen.DocIO.DLS;

public class FormFieldCollection : CollectionImpl
{
	private Dictionary<string, WFormField> m_dictionary = new Dictionary<string, WFormField>();

	public WFormField this[int index] => (WFormField)base.InnerList[index];

	public WFormField this[string formFieldName] => GetByName(formFieldName);

	internal Dictionary<string, WFormField> FormFieldDictonary => m_dictionary;

	internal FormFieldCollection(WTextBody textBody)
		: base(textBody.Document, textBody)
	{
		Populate(textBody);
	}

	public bool ContainsName(string itemName)
	{
		return m_dictionary.ContainsKey(itemName);
	}

	internal void CorrectName(string oldName, string newName)
	{
		WFormField value = m_dictionary[oldName];
		m_dictionary.Remove(oldName);
		m_dictionary[newName] = value;
		if (base.OwnerBase is WTableCell { OwnerRow: not null } wTableCell && wTableCell.OwnerRow.OwnerTable != null)
		{
			WTextBody ownerTextBody = wTableCell.OwnerRow.OwnerTable.OwnerTextBody;
			if (ownerTextBody != null && ownerTextBody.IsFormFieldsCreated)
			{
				ownerTextBody.FormFields.CorrectName(oldName, newName);
			}
		}
	}

	internal void Add(WFormField ff)
	{
		if (!base.InnerList.Contains(ff))
		{
			base.InnerList.Add(ff);
		}
		if (ff.Name != null && ff.Name != string.Empty && !m_dictionary.ContainsKey(ff.Name))
		{
			m_dictionary.Add(ff.Name, ff);
		}
	}

	internal void Remove(WFormField ff)
	{
		base.InnerList.Remove(ff);
		if (ff.Name == null || !(ff.Name != string.Empty) || !m_dictionary.ContainsKey(ff.Name))
		{
			return;
		}
		m_dictionary.Remove(ff.Name);
		if (!m_doc.IsDeletingBookmarkContent)
		{
			Bookmark bookmark = m_doc.Bookmarks.FindByName(ff.Name);
			if (bookmark != null)
			{
				m_doc.Bookmarks.Remove(bookmark);
			}
		}
	}

	private void Populate(WTextBody textBody)
	{
		foreach (TextBodyItem childEntity in textBody.ChildEntities)
		{
			switch (childEntity.EntityType)
			{
			case EntityType.Paragraph:
				PopulateFromParagraph((WParagraph)childEntity);
				break;
			case EntityType.Table:
				PopulateFromTable((WTable)childEntity);
				break;
			}
		}
	}

	private void PopulateFromParagraph(WParagraph para)
	{
		foreach (ParagraphItem item in para.Items)
		{
			if (item.EntityType == EntityType.TextFormField || item.EntityType == EntityType.CheckBox || item.EntityType == EntityType.DropDownFormField)
			{
				Add((WFormField)item);
			}
			switch (item.EntityType)
			{
			case EntityType.TextBox:
				Populate((item as WTextBox).TextBoxBody);
				break;
			case EntityType.Footnote:
				Populate((item as WFootnote).TextBody);
				break;
			case EntityType.Comment:
				Populate((item as WComment).TextBody);
				break;
			}
		}
	}

	private void PopulateFromTable(WTable table)
	{
		foreach (WTableRow row in table.Rows)
		{
			foreach (WTableCell cell in row.Cells)
			{
				Populate(cell);
			}
		}
	}

	private WFormField GetByName(string formFieldName)
	{
		if (m_dictionary.ContainsKey(formFieldName))
		{
			return m_dictionary[formFieldName];
		}
		return null;
	}

	internal override void Close()
	{
		if (m_dictionary != null)
		{
			m_dictionary.Clear();
			m_dictionary = null;
		}
		base.Close();
	}
}
