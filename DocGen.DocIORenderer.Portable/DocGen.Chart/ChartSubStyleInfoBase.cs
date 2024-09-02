using System;
using System.Collections.Generic;
using System.ComponentModel;
using DocGen.Styles;

namespace DocGen.Chart;

internal abstract class ChartSubStyleInfoBase : StyleInfoSubObjectBase
{
	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public new bool CacheValues
	{
		get
		{
			return base.CacheValues;
		}
		set
		{
			base.CacheValues = value;
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public new List<WeakReference> WeakReferenceChangedListeners => base.WeakReferenceChangedListeners;

	public ChartSubStyleInfoBase(StyleInfoSubObjectIdentity identity, StyleInfoStore store)
		: base(identity, store)
	{
	}

	public ChartSubStyleInfoBase(StyleInfoStore store)
		: base(store)
	{
	}
}
