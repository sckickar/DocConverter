using System;

namespace DocGen.DocIO.DLS;

public class BodyItemCollection : EntityCollection
{
	private static readonly Type[] DEF_ELEMENT_TYPES = new Type[4]
	{
		typeof(WTable),
		typeof(WParagraph),
		typeof(BlockContentControl),
		typeof(AlternateChunk)
	};

	internal new TextBodyItem this[int index] => (TextBodyItem)base[index];

	protected override Type[] TypesOfElement => DEF_ELEMENT_TYPES;

	public BodyItemCollection(WTextBody body)
		: base(body.Document, body)
	{
	}

	internal BodyItemCollection(WordDocument doc)
		: base(doc, null)
	{
	}
}
