using System;
using System.Collections.Generic;
using System.Reflection;

namespace DocGen.OfficeChart.Implementation;

public static class TypeExtension
{
	private static readonly Dictionary<Type, TypeCode> _typeCodeTable = new Dictionary<Type, TypeCode>
	{
		{
			typeof(bool),
			TypeCode.Boolean
		},
		{
			typeof(char),
			TypeCode.Char
		},
		{
			typeof(byte),
			TypeCode.Byte
		},
		{
			typeof(short),
			TypeCode.Int16
		},
		{
			typeof(int),
			TypeCode.Int32
		},
		{
			typeof(long),
			TypeCode.Int64
		},
		{
			typeof(sbyte),
			TypeCode.SByte
		},
		{
			typeof(ushort),
			TypeCode.UInt16
		},
		{
			typeof(uint),
			TypeCode.UInt32
		},
		{
			typeof(ulong),
			TypeCode.UInt64
		},
		{
			typeof(float),
			TypeCode.Single
		},
		{
			typeof(double),
			TypeCode.Double
		},
		{
			typeof(DateTime),
			TypeCode.DateTime
		},
		{
			typeof(decimal),
			TypeCode.Decimal
		},
		{
			typeof(string),
			TypeCode.String
		}
	};

	public static Type UnderlyingSystemType { get; }

	public static ConstructorInfo GetConstructor(this Type type, Type[] types)
	{
		bool flag = false;
		IEnumerator<ConstructorInfo> enumerator = type.GetTypeInfo().DeclaredConstructors.GetEnumerator();
		while (enumerator.MoveNext())
		{
			flag = false;
			ConstructorInfo current = enumerator.Current;
			ParameterInfo[] parameters = current.GetParameters();
			if (current.IsStatic)
			{
				continue;
			}
			if (types.Length == 0 && types.Length == parameters.Length)
			{
				return current;
			}
			if (types.Length != parameters.Length)
			{
				continue;
			}
			for (int i = 0; i < types.Length && i < parameters.Length; i++)
			{
				flag = true;
				if (!parameters[i].ParameterType.Equals(types[i]) && !IsSupportedType(parameters[i].ParameterType, types[i]))
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				return current;
			}
		}
		return null;
	}

	internal static bool IsSupportedType(Type type1, Type type2)
	{
		if (!type1.GetTypeInfo().IsPrimitive || !type2.GetTypeInfo().IsPrimitive)
		{
			return false;
		}
		int sizeOfType = GetSizeOfType(type1.Name);
		int sizeOfType2 = GetSizeOfType(type2.Name);
		if (sizeOfType >= sizeOfType2)
		{
			return true;
		}
		return false;
	}

	internal static int GetSizeOfType(string typeName)
	{
		return typeName switch
		{
			"UInt16" => 2, 
			"UInt32" => 4, 
			"UInt64" => 8, 
			"Int16" => 2, 
			"Int32" => 4, 
			_ => 0, 
		};
	}

	internal static TypeCode GetTypeCode(Type type)
	{
		if (type == null)
		{
			return TypeCode.Empty;
		}
		return GetTypeCodeImpl(type);
	}

	internal static TypeCode GetTypeCodeImpl(Type type)
	{
		if (typeof(Type) != UnderlyingSystemType && UnderlyingSystemType != null)
		{
			return GetTypeCode(UnderlyingSystemType);
		}
		if (!_typeCodeTable.TryGetValue(type, out var value))
		{
			return TypeCode.Object;
		}
		return value;
	}

	public static object[] GetCustomAttributes(this Type type, Type attributeType, bool inherit)
	{
		return type.GetTypeInfo().GetCustomAttributes(attributeType, inherit);
	}

	public static bool IsSubclassOf(this Type type, Type parentType)
	{
		return type.GetTypeInfo().IsSubclassOf(parentType);
	}

	public static Type GetInterface(this Type type, string interfaceName, bool ignoreCase)
	{
		IEnumerator<Type> enumerator = type.GetTypeInfo().ImplementedInterfaces.GetEnumerator();
		while (enumerator.MoveNext())
		{
			Type current = enumerator.Current;
			if (current.Name.Equals(interfaceName, ignoreCase ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture))
			{
				return current;
			}
		}
		return null;
	}

	public static PropertyInfo[] GetProperties(this Type type)
	{
		return type.GetTypeInfo().DeclaredProperties.ToArray();
	}

	internal static PropertyInfo GetProperty(this Type type, string propertyName, BindingFlags flags)
	{
		return GetProperty(type, propertyName);
	}

	public static PropertyInfo GetProperty(this Type type, string propertyName)
	{
		PropertyInfo[] array = type.GetTypeInfo().DeclaredProperties.ToArray();
		foreach (PropertyInfo propertyInfo in array)
		{
			if (propertyInfo.Name == propertyName)
			{
				return propertyInfo;
			}
		}
		return null;
	}

	public static object[] GetCustomAttributes(this PropertyInfo property, bool inherit)
	{
		return property.GetCustomAttributes(inherit).ToArray();
	}
}
