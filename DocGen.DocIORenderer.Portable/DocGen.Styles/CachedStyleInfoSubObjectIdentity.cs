namespace DocGen.Styles;

internal class CachedStyleInfoSubObjectIdentity : StyleInfoSubObjectIdentity
{
	private IStyleInfo[] cachedBaseStyles;

	public CachedStyleInfoSubObjectIdentity(StyleInfoBase owner, StyleInfoProperty sip)
		: base(owner, sip)
	{
	}

	public override IStyleInfo[] GetBaseStyles(IStyleInfo thisStyleInfo)
	{
		if (cachedBaseStyles == null)
		{
			cachedBaseStyles = base.GetBaseStyles(thisStyleInfo);
		}
		return cachedBaseStyles;
	}

	public override void OnStyleChanged(StyleInfoBase style, StyleInfoProperty sip)
	{
		cachedBaseStyles = null;
	}
}
