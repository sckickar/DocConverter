using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace DocGen.OfficeChart.Implementation;

internal sealed class HashCalculate
{
	private static int m_level;

	private HashCalculate()
	{
	}

	public static int CalculateHash(object value, string[] skipProps)
	{
		m_level = 0;
		StringBuilder stringBuilder = new StringBuilder(8192);
		List<string> list = new List<string>(skipProps);
		list.Sort();
		ObjectToString(stringBuilder, value, list);
		return stringBuilder.ToString().GetHashCode();
	}

	private static void ObjectToString(StringBuilder builder, object value, List<string> toSkip)
	{
		m_level++;
		Type type = value.GetType();
		if (!type.GetTypeInfo().IsPrimitive)
		{
			PropertyInfo[] array = type.GetRuntimeProperties().ToArray();
			if (array.Length != 0)
			{
				for (int i = 0; i < array.Length; i++)
				{
					if (!array[i].CanRead || toSkip.BinarySearch(array[i].Name) >= 0)
					{
						continue;
					}
					try
					{
						object value2 = array[i].GetValue(value, new object[0]);
						if (value2 != null)
						{
							Type type2 = value2.GetType();
							if (value2 is string)
							{
								builder.Append(value2.ToString());
							}
							else if (value2 is ICollection)
							{
								IEnumerator enumerator = ((ICollection)value2).GetEnumerator();
								enumerator.Reset();
								int num = 0;
								while (enumerator.MoveNext())
								{
									ObjectToString(builder, enumerator.Current, toSkip);
									num++;
								}
							}
							else if (!type2.GetTypeInfo().IsPrimitive)
							{
								ObjectToString(builder, value2, toSkip);
							}
							else
							{
								builder.Append(value2.ToString());
							}
						}
						else
						{
							builder.Append("null");
						}
					}
					catch (Exception)
					{
					}
				}
			}
			else
			{
				builder.Append(value.ToString());
			}
		}
		else
		{
			builder.Append(value.ToString());
		}
		m_level--;
	}
}
