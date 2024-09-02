namespace DocGen.DocIO.DLS;

public class TextBoxCollection : CollectionImpl
{
	public WTextBox this[int index] => base.InnerList[index] as WTextBox;

	internal TextBoxCollection(WordDocument doc)
		: base(doc, doc)
	{
	}

	public void RemoveAt(int index)
	{
		WTextBox wTextBox = base.InnerList[index] as WTextBox;
		wTextBox.OwnerParagraph.Items.Remove(wTextBox);
	}

	public void Clear()
	{
		while (base.InnerList.Count > 0)
		{
			int index = base.InnerList.Count - 1;
			RemoveAt(index);
		}
	}

	internal void Add(WTextBox textbox)
	{
		base.InnerList.Add(textbox);
	}

	internal void Remove(WTextBox textbox)
	{
		base.InnerList.Remove(textbox);
	}
}
