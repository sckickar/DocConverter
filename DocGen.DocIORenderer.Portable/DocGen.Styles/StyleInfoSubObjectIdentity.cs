using System;
using System.Diagnostics;

namespace DocGen.Styles;

[DebuggerStepThrough]
internal class StyleInfoSubObjectIdentity : StyleInfoIdentityBase
{
	private StyleInfoBase owner;

	private StyleInfoProperty sip;

	public StyleInfoBase Owner => owner;

	public StyleInfoProperty Sip => sip;

	public override void Dispose()
	{
		owner = null;
		sip = null;
		base.Dispose();
	}

	public StyleInfoSubObjectIdentity(StyleInfoBase owner, StyleInfoProperty sip)
	{
		this.owner = owner;
		this.sip = sip;
	}

	public override IStyleInfo[] GetBaseStyles(IStyleInfo thisStyleInfo)
	{
		IStyleInfo[] array = null;
		if (owner.Identity != null)
		{
			array = owner.Identity.GetBaseStyles(owner);
		}
		if (array != null)
		{
			IStyleInfo[] array2 = new StyleInfoBase[array.Length + 1];
			IStyleInfo[] array3 = array2;
			int num = 0;
			array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				StyleInfoBase styleInfoBase = (StyleInfoBase)array2[i];
				if (styleInfoBase.HasValue(sip))
				{
					array3[num++] = (StyleInfoBase)styleInfoBase.GetValue(sip);
				}
			}
			StyleInfoBase defaultStyle = owner.GetDefaultStyle();
			if (defaultStyle != null && defaultStyle.HasValue(sip))
			{
				array3[num++] = (StyleInfoBase)defaultStyle.GetValue(sip);
			}
			if (num == array3.Length)
			{
				return array3;
			}
			StyleInfoBase[] array4 = new StyleInfoBase[num];
			if (num > 0)
			{
				Array.Copy(array3, 0, array4, 0, num);
			}
			return array4;
		}
		return null;
	}
}
