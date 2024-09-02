using System;
using System.ComponentModel;

namespace DocGen.Styles;

internal abstract class StyleInfoSubObjectBase : StyleInfoBase, IStyleInfoSubObject
{
	private StyleInfoBase owner;

	private StyleInfoProperty sip;

	private StyleInfoSubObjectIdentity sid;

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public StyleInfoProperty Sip => sip;

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public object Data => _store;

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public StyleInfoBase Owner => owner;

	protected StyleInfoSubObjectBase(StyleInfoStore store)
		: base(null, store)
	{
	}

	protected StyleInfoSubObjectBase(StyleInfoSubObjectIdentity identity, StyleInfoStore store)
		: base(identity, store)
	{
		sid = identity;
		if (sid != null)
		{
			owner = sid.Owner;
			sip = sid.Sip;
			base.CacheValues = owner.CacheValues;
		}
	}

	public override void Dispose()
	{
		owner = null;
		sip = null;
		sid = null;
		base.Dispose();
	}

	protected override void OnStyleChanged(StyleInfoProperty sip)
	{
		base.OnStyleChanged(sip);
		if (owner != null)
		{
			owner.OnSubObjectChanged(this);
		}
	}

	protected override StyleInfoBase IntGetDefaultStyleInfo(StyleInfoProperty sip)
	{
		if (identity != null)
		{
			return identity.GetBaseStyle(this, sip);
		}
		return null;
	}

	public virtual IStyleInfoSubObject MakeCopy(StyleInfoBase newOwner, StyleInfoProperty sip)
	{
		return (IStyleInfoSubObject)Activator.CreateInstance(GetType(), newOwner.CreateSubObjectIdentity(sip), _store.Clone());
	}
}
