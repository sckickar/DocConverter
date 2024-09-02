using System;
using System.Runtime.Serialization;
using DocGen.Styles;

namespace DocGen.Chart;

[Serializable]
[StaticDataField("sd")]
internal class ChartBordersInfoStore : StyleInfoStore
{
	private static StaticData sd;

	public static StyleInfoProperty OuterProperty;

	public static StyleInfoProperty InnerProperty;

	protected override StaticData StaticDataStore => sd;

	public ChartBordersInfoStore()
	{
	}

	static ChartBordersInfoStore()
	{
		sd = new StaticData(typeof(ChartBordersInfoStore), typeof(ChartBordersInfo), sortProperties: true);
		OuterProperty = sd.CreateStyleInfoProperty(typeof(ChartBorder), "Inner");
		InnerProperty = sd.CreateStyleInfoProperty(typeof(ChartBorder), "Outer");
	}

	internal static ChartBordersInfoStore InitializeStaticVariables()
	{
		if (sd == null)
		{
			sd = new StaticData(typeof(ChartBordersInfoStore), typeof(ChartBordersInfo), sortProperties: true);
			OuterProperty = sd.CreateStyleInfoProperty(typeof(ChartBorder), "Inner");
			InnerProperty = sd.CreateStyleInfoProperty(typeof(ChartBorder), "Outer");
		}
		return new ChartBordersInfoStore();
	}

	private ChartBordersInfoStore(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	internal new void Dispose()
	{
		if (OuterProperty != null)
		{
			OuterProperty.Dispose();
			OuterProperty = null;
		}
		if (InnerProperty != null)
		{
			InnerProperty.Dispose();
			InnerProperty = null;
		}
		if (sd != null)
		{
			sd.Dispose();
			sd = null;
		}
	}

	public override object Clone()
	{
		StyleInfoStore styleInfoStore = new ChartBordersInfoStore();
		CopyTo(styleInfoStore);
		return styleInfoStore;
	}
}
