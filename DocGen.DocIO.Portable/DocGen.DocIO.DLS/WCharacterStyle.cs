namespace DocGen.DocIO.DLS;

public class WCharacterStyle : Style, IWCharacterStyle, IStyle
{
	public new WCharacterStyle BaseStyle => base.BaseStyle as WCharacterStyle;

	public override StyleType StyleType => StyleType.CharacterStyle;

	public WCharacterStyle(WordDocument doc)
		: base(doc)
	{
		m_chFormat = new WCharacterFormat(base.Document);
		m_chFormat.SetOwner(this);
		if (doc.CreateBaseStyle)
		{
			doc.CreateBaseStyle = false;
			ApplyBaseStyle(BuiltinStyle.DefaultParagraphFont);
			doc.CreateBaseStyle = true;
		}
	}

	public override IStyle Clone()
	{
		return (IStyle)CloneImpl();
	}
}
