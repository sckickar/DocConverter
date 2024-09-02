using System;
using System.Collections;
using System.Collections.Generic;
using DocGen.OfficeChart.Implementation.Shapes;

namespace DocGen.OfficeChart.Implementation.XmlSerialization;

internal class UniqueInstanceTypeList
{
	private Dictionary<int, Dictionary<Type, object>> m_dictItems = new Dictionary<int, Dictionary<Type, object>>();

	public void AddShape(ShapeImpl shape)
	{
		if (shape == null)
		{
			throw new ArgumentNullException("shape");
		}
		int instance = shape.Instance;
		if (!m_dictItems.TryGetValue(instance, out var value))
		{
			value = new Dictionary<Type, object>();
			m_dictItems[instance] = value;
		}
		value[shape.GetType()] = null;
	}

	public IEnumerable UniquePairs()
	{
		foreach (int instance in m_dictItems.Keys)
		{
			Dictionary<Type, object> dictionary = m_dictItems[instance];
			foreach (Type key in dictionary.Keys)
			{
				yield return new KeyValuePair<int, Type>(instance, key);
			}
		}
	}
}
