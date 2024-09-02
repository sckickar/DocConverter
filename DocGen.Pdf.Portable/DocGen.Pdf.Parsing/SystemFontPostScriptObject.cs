using System.Collections.Generic;

namespace DocGen.Pdf.Parsing;

internal abstract class SystemFontPostScriptObject
{
	private readonly Dictionary<string, ISystemFontProperty> properties;

	public SystemFontPostScriptObject()
	{
		properties = new Dictionary<string, ISystemFontProperty>();
	}

	public void Load(SystemFontPostScriptDictionary fromDict)
	{
		foreach (KeyValuePair<string, object> item in fromDict)
		{
			if (properties.TryGetValue(item.Key, out ISystemFontProperty value))
			{
				value.SetValue(item.Value);
			}
		}
	}

	protected SystemFontProperty<T> CreateProperty<T>(SystemFontPropertyDescriptor descriptor)
	{
		SystemFontProperty<T> systemFontProperty = new SystemFontProperty<T>(descriptor);
		RegisterProperty(systemFontProperty);
		return systemFontProperty;
	}

	protected SystemFontProperty<T> CreateProperty<T>(SystemFontPropertyDescriptor descriptor, ISystemFontConverter converter)
	{
		SystemFontProperty<T> systemFontProperty = new SystemFontProperty<T>(descriptor, converter);
		RegisterProperty(systemFontProperty);
		return systemFontProperty;
	}

	protected SystemFontProperty<T> CreateProperty<T>(SystemFontPropertyDescriptor descriptor, T defaultValue)
	{
		SystemFontProperty<T> systemFontProperty = new SystemFontProperty<T>(descriptor, defaultValue);
		RegisterProperty(systemFontProperty);
		return systemFontProperty;
	}

	protected SystemFontProperty<T> CreateProperty<T>(SystemFontPropertyDescriptor descriptor, ISystemFontConverter converter, T defaultValue)
	{
		SystemFontProperty<T> systemFontProperty = new SystemFontProperty<T>(descriptor, converter, defaultValue);
		RegisterProperty(systemFontProperty);
		return systemFontProperty;
	}

	private void RegisterProperty(SystemFontPropertyDescriptor descriptor, ISystemFontProperty property)
	{
		if (descriptor.Name != null)
		{
			properties[descriptor.Name] = property;
		}
	}

	private void RegisterProperty(ISystemFontProperty property)
	{
		RegisterProperty(property.Descriptor, property);
	}
}
