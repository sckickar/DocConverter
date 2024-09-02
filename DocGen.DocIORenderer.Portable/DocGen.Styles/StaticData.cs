using System;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Reflection;

namespace DocGen.Styles;

[DebuggerStepThrough]
internal class StaticData
{
	internal ArrayList styleInfoProperties;

	internal int objectCount;

	internal int expandableObjectCount;

	internal int dataVectorCount;

	private object dataPreviousSection;

	private short dataPreviousBitCount;

	private int previousIncludeBit;

	private Type styleInfoType;

	private string[] sortOrder;

	private bool sortProperties;

	private const int maxbits = 31;

	private const int maxbits1 = 30;

	private static int currentExpandableObjectKey;

	internal Type StyleInfoType => styleInfoType;

	public ICollection StyleInfoProperties => styleInfoProperties;

	public string[] PropertyGridSortOrder
	{
		get
		{
			if (sortOrder == null)
			{
				if (sortProperties)
				{
					sortOrder = new string[styleInfoProperties.Count];
					for (int i = 0; i < styleInfoProperties.Count; i++)
					{
						StyleInfoProperty styleInfoProperty = (StyleInfoProperty)styleInfoProperties[i];
						sortOrder[i] = styleInfoProperty.PropertyName;
					}
				}
				else
				{
					sortOrder = new string[0];
				}
			}
			return sortOrder;
		}
	}

	public bool IsEmpty => styleInfoProperties.Count == 0;

	public void Dispose()
	{
		styleInfoProperties = null;
		dataPreviousSection = null;
		styleInfoType = null;
		sortOrder = null;
	}

	public StaticData(Type type, Type styleInfoType, bool sortProperties)
	{
		this.styleInfoType = styleInfoType;
		this.sortProperties = sortProperties;
		Type? baseType = type.BaseType;
		object[] customAttributes = baseType.GetCustomAttributes(typeof(StaticDataFieldAttribute), inherit: false);
		FieldInfo field = baseType.GetField((customAttributes.Length == 0) ? StaticDataFieldAttribute.Default.FieldName : ((StaticDataFieldAttribute)customAttributes[0]).FieldName, BindingFlags.Static | BindingFlags.NonPublic);
		if (field != null && field.GetValue(null) is StaticData staticData)
		{
			styleInfoProperties = (ArrayList)staticData.styleInfoProperties.Clone();
			dataPreviousSection = staticData.dataPreviousSection;
			dataPreviousBitCount = staticData.dataPreviousBitCount;
			dataVectorCount = staticData.dataVectorCount;
			previousIncludeBit = staticData.previousIncludeBit;
		}
		else
		{
			styleInfoProperties = new ArrayList();
			dataPreviousSection = null;
			dataPreviousBitCount = 0;
			dataVectorCount = 0;
			previousIncludeBit = 0;
			customAttributes = null;
			field = null;
		}
	}

	public string[] CreatePropertyGridSortOrder(string[] sortOrder)
	{
		this.sortOrder = sortOrder;
		return sortOrder;
	}

	public BitVector32.Section AllocateDataVectorSection(short maxValue, out int bvi)
	{
		if (dataVectorCount == 0)
		{
			dataVectorCount = 1;
		}
		short num = CountBitsSet(maxValue);
		if (dataPreviousSection != null)
		{
			_ = (BitVector32.Section)dataPreviousSection;
			if (num + dataPreviousBitCount > 30)
			{
				dataVectorCount++;
				dataPreviousSection = null;
				dataPreviousBitCount = 0;
			}
		}
		BitVector32.Section result;
		dataPreviousSection = (result = ((dataPreviousSection == null) ? BitVector32.CreateSection(maxValue) : BitVector32.CreateSection(maxValue, (BitVector32.Section)dataPreviousSection)));
		bvi = dataVectorCount - 1;
		dataPreviousBitCount += num;
		return result;
	}

	private static short CountBitsSet(short mask)
	{
		short num = 0;
		while (mask != 0)
		{
			mask >>= 1;
			num++;
		}
		return num;
	}

	public StyleInfoProperty CreateStyleInfoProperty(Type type, string name)
	{
		return CreateStyleInfoProperty(type, name, 0, makeBitValue: false);
	}

	public StyleInfoProperty CreateStyleInfoProperty(Type type, string name, StyleInfoPropertyOptions propertyOptions)
	{
		return CreateStyleInfoProperty(type, name, 0, makeBitValue: false, propertyOptions);
	}

	public StyleInfoProperty CreateStyleInfoProperty(Type type, string name, short maxValue)
	{
		return CreateStyleInfoProperty(type, name, maxValue, makeBitValue: false);
	}

	private static int CreateExpandableObjectKey()
	{
		return currentExpandableObjectKey++;
	}

	public StyleInfoProperty CreateStyleInfoProperty(Type type, string name, short maxValue, bool makeBitValue)
	{
		return CreateStyleInfoProperty(type, name, maxValue, makeBitValue, styleInfoType, StyleInfoPropertyOptions.All);
	}

	public StyleInfoProperty CreateStyleInfoProperty(Type type, string name, short maxValue, bool makeBitValue, StyleInfoPropertyOptions propertyOptions)
	{
		return CreateStyleInfoProperty(type, name, maxValue, makeBitValue, styleInfoType, propertyOptions);
	}

	public StyleInfoProperty CreateStyleInfoProperty(Type type, string name, short maxValue, bool makeBitValue, Type componentType, StyleInfoPropertyOptions propertyOptions)
	{
		StyleInfoProperty styleInfoProperty = _CreateStyleInfoProperty(type, name, maxValue, makeBitValue, componentType);
		styleInfoProperty.IsSerializable = (propertyOptions & StyleInfoPropertyOptions.Serializable) != 0;
		styleInfoProperty.IsCloneable = (propertyOptions & StyleInfoPropertyOptions.Cloneable) != 0;
		styleInfoProperty.IsDisposable = (propertyOptions & StyleInfoPropertyOptions.Disposable) != 0;
		return styleInfoProperty;
	}

	private StyleInfoProperty _CreateStyleInfoProperty(Type type, string name, short maxValue, bool makeBitValue, Type componentType)
	{
		int num = -1;
		StyleInfoProperty styleInfoProperty = null;
		for (int i = 0; i < styleInfoProperties.Count; i++)
		{
			styleInfoProperty = (StyleInfoProperty)styleInfoProperties[i];
			if (styleInfoProperty.PropertyName == name)
			{
				num = i;
				break;
			}
		}
		StyleInfoProperty styleInfoProperty2 = new StyleInfoProperty(type, name, maxValue, componentType);
		if (maxValue == 0)
		{
			if (type == typeof(bool))
			{
				maxValue = 1;
			}
			else if (type == typeof(byte) || type == typeof(sbyte))
			{
				maxValue = 255;
			}
			else if (type == typeof(short))
			{
				maxValue = short.MaxValue;
			}
		}
		else if ((type == typeof(bool) && maxValue > 1) || ((type == typeof(byte) || type == typeof(sbyte)) && maxValue > 255) || (type == typeof(short) && maxValue > short.MaxValue))
		{
			throw new ArgumentOutOfRangeException("maxValue", maxValue, "too large for type " + type.Name);
		}
		if (num != -1 && styleInfoProperty.MaxValue == maxValue && styleInfoProperty.IsExpandable == styleInfoProperty2.IsExpandable && makeBitValue == (styleInfoProperty2.DataVectorIndex != -1))
		{
			styleInfoProperty2.MaxValue = styleInfoProperty.MaxValue;
			styleInfoProperty2.DataVectorIndex = styleInfoProperty.DataVectorIndex;
			styleInfoProperty2.DataVectorSection = styleInfoProperty.DataVectorSection;
			styleInfoProperty2.ExpandableObjectStoreKey = styleInfoProperty.ExpandableObjectStoreKey;
			styleInfoProperty2.ObjectStoreKey = styleInfoProperty.ObjectStoreKey;
			styleInfoProperty2.BitVectorIndex = styleInfoProperty.BitVectorIndex;
			styleInfoProperty2.BitVectorMask = styleInfoProperty.BitVectorMask;
			styleInfoProperty2.Index = styleInfoProperty.Index;
		}
		else
		{
			if (makeBitValue && maxValue != 0 && (type.IsEnum || type == typeof(bool) || type == typeof(byte) || type == typeof(sbyte) || type == typeof(short)))
			{
				styleInfoProperty2.DataVectorSection = AllocateDataVectorSection(maxValue, out var bvi);
				styleInfoProperty2.DataVectorIndex = bvi;
				styleInfoProperty2.MaxValue = maxValue;
			}
			else if (styleInfoProperty2.IsExpandable)
			{
				styleInfoProperty2.ExpandableObjectStoreKey = CreateExpandableObjectKey();
				expandableObjectCount++;
			}
			else
			{
				objectCount++;
				styleInfoProperty2.ObjectStoreKey = StyleInfoObjectStore.CreateKey();
			}
			int count = styleInfoProperties.Count;
			if (count % 31 == 0)
			{
				styleInfoProperty2.BitVectorMask = BitVector32.CreateMask();
			}
			else
			{
				styleInfoProperty2.BitVectorMask = BitVector32.CreateMask(previousIncludeBit);
			}
			previousIncludeBit = styleInfoProperty2.BitVectorMask;
			styleInfoProperty2.BitVectorIndex = count / 31;
			styleInfoProperty2.Index = count;
		}
		if (num == -1)
		{
			styleInfoProperties.Add(styleInfoProperty2);
		}
		else
		{
			styleInfoProperties[num] = styleInfoProperty2;
		}
		return styleInfoProperty2;
	}
}
