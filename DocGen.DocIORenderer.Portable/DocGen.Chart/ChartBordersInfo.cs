using System.ComponentModel;
using System.Diagnostics;
using DocGen.Drawing;
using DocGen.Styles;

namespace DocGen.Chart;

internal class ChartBordersInfo : ChartSubStyleInfoBase
{
	private static ChartBordersInfo defaultBorders;

	private static ChartBordersInfoStore m_store = new ChartBordersInfoStore();

	public static ChartBordersInfo Default
	{
		get
		{
			if (defaultBorders == null)
			{
				defaultBorders = new ChartBordersInfo();
				defaultBorders.Inner = new ChartBorder(ChartBorderStyle.Standard, SystemColors.WindowFrame, ChartBorderWeight.Thin);
				defaultBorders.Outer = new ChartBorder(ChartBorderStyle.Standard, SystemColors.WindowFrame, ChartBorderWeight.Thin);
			}
			return defaultBorders;
		}
	}

	[Browsable(false)]
	public ChartBorder All
	{
		[DebuggerStepThrough]
		set
		{
			SetValue(ChartBordersInfoStore.InnerProperty, value);
			SetValue(ChartBordersInfoStore.OuterProperty, value);
		}
	}

	[Browsable(true)]
	[Category("Appearance")]
	[Description("The inner border")]
	public ChartBorder Inner
	{
		[DebuggerStepThrough]
		get
		{
			return (ChartBorder)GetValue(ChartBordersInfoStore.InnerProperty);
		}
		[DebuggerStepThrough]
		set
		{
			SetValue(ChartBordersInfoStore.InnerProperty, value);
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool HasInner
	{
		[DebuggerStepThrough]
		get
		{
			return HasValue(ChartBordersInfoStore.InnerProperty);
		}
	}

	[Browsable(true)]
	[Category("Appearance")]
	[Description("The outer border")]
	public ChartBorder Outer
	{
		[DebuggerStepThrough]
		get
		{
			return (ChartBorder)GetValue(ChartBordersInfoStore.OuterProperty);
		}
		[DebuggerStepThrough]
		set
		{
			SetValue(ChartBordersInfoStore.OuterProperty, value);
		}
	}

	[DebuggerStepThrough]
	public ChartBordersInfo()
		: base(ChartBordersInfoStore.InitializeStaticVariables())
	{
	}

	~ChartBordersInfo()
	{
		if (defaultBorders != null)
		{
			defaultBorders = null;
		}
	}

	[DebuggerStepThrough]
	public ChartBordersInfo(StyleInfoSubObjectIdentity identity)
		: base(identity, ChartBordersInfoStore.InitializeStaticVariables())
	{
	}

	[DebuggerStepThrough]
	public ChartBordersInfo(StyleInfoSubObjectIdentity identity, ChartBordersInfoStore store)
		: base(identity, store)
	{
	}

	public override void Dispose()
	{
		if (defaultBorders != null)
		{
			defaultBorders.Inner.Dispose();
			defaultBorders.Outer.Dispose();
			defaultBorders = null;
		}
		if (m_store != null)
		{
			m_store.Dispose();
			m_store = null;
		}
		base.Dispose();
	}

	[DebuggerStepThrough]
	public override IStyleInfoSubObject MakeCopy(StyleInfoBase newOwner, StyleInfoProperty sip)
	{
		return new ChartBordersInfo(newOwner.CreateSubObjectIdentity(sip), (ChartBordersInfoStore)base.Store.Clone());
	}

	internal static object CreateObject(StyleInfoSubObjectIdentity identity, object store)
	{
		if (store != null)
		{
			return new ChartBordersInfo(identity, store as ChartBordersInfoStore);
		}
		return new ChartBordersInfo(identity);
	}

	protected internal override StyleInfoBase GetDefaultStyle()
	{
		return Default;
	}

	[DebuggerStepThrough]
	public void ResetAll()
	{
		ResetValue(ChartBordersInfoStore.InnerProperty);
		ResetValue(ChartBordersInfoStore.OuterProperty);
	}

	[DebuggerStepThrough]
	public void ResetInner()
	{
		ResetValue(ChartBordersInfoStore.InnerProperty);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	private bool ShouldSerializeInner()
	{
		return HasValue(ChartBordersInfoStore.InnerProperty);
	}

	[DebuggerStepThrough]
	public void ResetOuter()
	{
		ResetValue(ChartBordersInfoStore.OuterProperty);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	private bool ShouldSerializeOuter()
	{
		return HasValue(ChartBordersInfoStore.OuterProperty);
	}
}
