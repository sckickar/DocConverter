using System;
using System.ComponentModel;

namespace DocGen.Styles;

[Serializable]
internal abstract class StyleInfoIdentityBase : IDisposable
{
	private StyleInfoIdentityBase innerIdentity;

	private bool isDisposable = true;

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public StyleInfoIdentityBase InnerIdentity
	{
		get
		{
			return innerIdentity;
		}
		set
		{
			innerIdentity = value;
		}
	}

	public bool IsDisposable
	{
		get
		{
			return isDisposable;
		}
		set
		{
			isDisposable = value;
		}
	}

	public virtual StyleInfoBase GetBaseStyle(IStyleInfo thisStyleInfo, StyleInfoProperty sipSrc)
	{
		if (innerIdentity != null)
		{
			return innerIdentity.GetBaseStyle(thisStyleInfo, sipSrc);
		}
		IStyleInfo[] baseStyles = GetBaseStyles(thisStyleInfo);
		if (baseStyles != null)
		{
			StyleInfoProperty styleInfoProperty = null;
			IStyleInfo[] array = baseStyles;
			for (int i = 0; i < array.Length; i++)
			{
				StyleInfoBase styleInfoBase = (StyleInfoBase)array[i];
				if (styleInfoBase._store != null)
				{
					styleInfoProperty = styleInfoBase._store.FindStyleInfoProperty(sipSrc.PropertyName);
				}
				if (styleInfoProperty != null && styleInfoBase != null && styleInfoBase.Store != null && styleInfoBase.HasValue(styleInfoProperty))
				{
					return styleInfoBase;
				}
			}
		}
		return null;
	}

	public virtual void Dispose()
	{
		GC.SuppressFinalize(this);
	}

	public virtual StyleInfoBase GetBaseStyleNotEmptyExpandable(IStyleInfo thisStyleInfo, StyleInfoProperty sip)
	{
		if (!sip.IsExpandable)
		{
			return null;
		}
		IStyleInfo[] baseStyles = GetBaseStyles(thisStyleInfo);
		if (baseStyles != null)
		{
			IStyleInfo[] array = baseStyles;
			for (int i = 0; i < array.Length; i++)
			{
				StyleInfoBase styleInfoBase = (StyleInfoBase)array[i];
				if (styleInfoBase.HasValue(sip) && !((IStyleInfo)styleInfoBase.GetValue(sip)).IsEmpty)
				{
					return styleInfoBase;
				}
			}
		}
		return null;
	}

	public abstract IStyleInfo[] GetBaseStyles(IStyleInfo thisStyleInfo);

	public virtual void OnStyleChanged(StyleInfoBase style, StyleInfoProperty sip)
	{
	}

	public virtual void OnStyleChanging(StyleInfoBase style, StyleInfoProperty sip)
	{
	}
}
