using System;
using DocGen.Styles;

namespace DocGen.Chart;

internal class ChartStyleInfoCustomProperties
{
	protected internal ChartStyleInfo style;

	public ChartStyleInfo StyleInfo
	{
		get
		{
			return style;
		}
		set
		{
			if (value != style && style != null)
			{
				value.ModifyStyle(style, StyleModifyType.Override);
			}
			style = value;
		}
	}

	protected ChartStyleInfoCustomProperties(ChartStyleInfo style)
	{
		this.style = style;
	}

	protected ChartStyleInfoCustomProperties()
	{
		style = new ChartStyleInfo();
	}

	private static StyleInfoProperty _CreateStyleInfoProperty(Type componentType, StaticData sd, Type type, string propertyName)
	{
		return sd.CreateStyleInfoProperty(type, propertyName, 0, makeBitValue: false, componentType, StyleInfoPropertyOptions.All);
	}

	protected static StyleInfoProperty CreateStyleInfoProperty(Type componentType, Type type, string propertyName)
	{
		return _CreateStyleInfoProperty(componentType, ChartStyleInfoStore.StaticData, type, propertyName);
	}

	protected static StyleInfoProperty CreateStyleInfoProperty(Type componentType, string propertyName)
	{
		Type propertyType = componentType.GetProperty(propertyName).PropertyType;
		return _CreateStyleInfoProperty(componentType, ChartStyleInfoStore.StaticData, propertyType, propertyName);
	}
}
