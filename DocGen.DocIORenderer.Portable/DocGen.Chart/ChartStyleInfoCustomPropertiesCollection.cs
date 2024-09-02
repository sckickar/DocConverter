using System;
using System.Collections;
using DocGen.Styles;

namespace DocGen.Chart;

internal class ChartStyleInfoCustomPropertiesCollection : ICollection, IEnumerable
{
	private StyleInfoBase styleInfo;

	private Hashtable types = new Hashtable();

	bool ICollection.IsSynchronized => types.Values.IsSynchronized;

	public int Count => types.Count;

	object ICollection.SyncRoot => types.Values.SyncRoot;

	internal ChartStyleInfoCustomPropertiesCollection(StyleInfoBase styleInfo)
	{
		this.styleInfo = styleInfo;
		ICollection styleInfoProperties = styleInfo.Store.StyleInfoProperties;
		Type type = styleInfo.GetType();
		foreach (StyleInfoProperty item in styleInfoProperties)
		{
			if (!item.ComponentType.IsAssignableFrom(type) && styleInfo.HasValue(item) && !types.ContainsKey(item.ComponentType))
			{
				types.Add(item.ComponentType, Activator.CreateInstance(item.ComponentType, styleInfo));
			}
		}
	}

	public void Add(ChartStyleInfoCustomProperties value)
	{
		value.StyleInfo = (ChartStyleInfo)styleInfo;
	}

	public void CopyTo(ChartStyleInfoCustomProperties[] array, int index)
	{
		types.Values.CopyTo(array, index);
	}

	void ICollection.CopyTo(Array array, int index)
	{
		CopyTo((ChartStyleInfoCustomProperties[])array, index);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return types.Values.GetEnumerator();
	}
}
