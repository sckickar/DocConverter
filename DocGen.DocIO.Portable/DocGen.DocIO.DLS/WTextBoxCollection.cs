using System;

namespace DocGen.DocIO.DLS;

public class WTextBoxCollection : EntityCollection, IWTextBoxCollection
{
	private static readonly Type[] TYPES = new Type[1] { typeof(WTextBox) };

	public new IWTextBox this[int index] => (IWTextBox)base.InnerList[index];

	protected override Type[] TypesOfElement => TYPES;

	public WTextBoxCollection(IWordDocument doc)
		: base((WordDocument)doc, (WordDocument)doc)
	{
	}

	public int Add(IWTextBox textBox)
	{
		return base.InnerList.Add(textBox);
	}
}
