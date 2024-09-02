using System.Text.RegularExpressions;

namespace DocGen.DocIO.DLS;

public abstract class TextBodyItem : WidgetBase, ITextBodyItem, IEntity
{
	public WTextBody OwnerTextBody => base.Owner as WTextBody;

	public bool IsInsertRevision => CheckInsertRev();

	public bool IsDeleteRevision => CheckDeleteRev();

	internal bool IsChangedCFormat => CheckChangedCFormat();

	internal bool IsChangedPFormat => CheckChangedPFormat();

	internal TextBodyItem NextTextBodyItem => GetNextTextBodyItemValue();

	public TextBodyItem(WordDocument doc)
		: base(doc, null)
	{
	}

	public abstract TextSelection Find(Regex pattern);

	public abstract int Replace(Regex pattern, string replace);

	public abstract int Replace(string given, string replace, bool caseSensitive, bool wholeWord);

	public abstract int Replace(Regex pattern, TextSelection textSelection);

	public abstract int Replace(Regex pattern, TextSelection textSelection, bool saveFormatting);

	internal abstract TextSelectionList FindAll(Regex pattern, bool isDocumentComparison);

	internal abstract TextBodyItem GetNextTextBodyItemValue();

	internal abstract void MakeChanges(bool acceptChanges);

	internal abstract bool CheckInsertRev();

	internal abstract bool CheckDeleteRev();

	internal abstract bool CheckChangedCFormat();

	internal abstract bool CheckChangedPFormat();

	internal abstract void AcceptCChanges();

	internal abstract void AcceptPChanges();

	internal abstract void RemoveCFormatChanges();

	internal abstract void RemovePFormatChanges();

	internal abstract bool HasTrackedChanges();

	internal abstract void SetChangedCFormat(bool check);

	internal abstract void SetChangedPFormat(bool check);

	internal abstract void SetDeleteRev(bool check);

	internal abstract void SetInsertRev(bool check);

	internal abstract void AddInsMark();

	internal abstract void AddDelMark();

	protected TextBodyItem GetNextInSection(WSection section)
	{
		if (section == null)
		{
			return null;
		}
		if (section.NextSibling is WSection wSection && wSection.Body.Items.Count > 0)
		{
			return wSection.Body.Items[0];
		}
		return null;
	}
}
