using DocGen.Layouting;

namespace DocGen.DocIO.DLS;

public abstract class WidgetContainer : WidgetBase, IWidgetContainer, IWidget
{
	public int Count => WidgetCollection.Count;

	IWidget IWidgetContainer.this[int index] => WidgetCollection[index] as IWidget;

	protected abstract IEntityCollectionBase WidgetCollection { get; }

	public EntityCollection WidgetInnerCollection => WidgetCollection as EntityCollection;

	public WidgetContainer(WordDocument doc, Entity owner)
		: base(doc, owner)
	{
	}

	internal override void Close()
	{
		base.Close();
		if (m_layoutInfo != null)
		{
			m_layoutInfo = null;
		}
	}
}
