using System.Collections.Generic;

namespace DocGen.PdfViewer.Base;

internal abstract class PostScriptObj
{
	private readonly Dictionary<string, IProperty> properties;

	public PostScriptObj()
	{
		properties = new Dictionary<string, IProperty>();
	}

	public void Load(PostScriptDict fromDict)
	{
		foreach (KeyValuePair<string, object> item in fromDict)
		{
			if (properties.TryGetValue(item.Key, out IProperty value))
			{
				value.SetValue(item.Value);
			}
		}
	}

	protected KeyProperty<T> CreateProperty<T>(KeyPropertyDescriptor descriptor)
	{
		KeyProperty<T> keyProperty = new KeyProperty<T>(descriptor);
		RegisterProperty(keyProperty);
		return keyProperty;
	}

	protected KeyProperty<T> CreateProperty<T>(KeyPropertyDescriptor descriptor, IConverter converter)
	{
		KeyProperty<T> keyProperty = new KeyProperty<T>(descriptor, converter);
		RegisterProperty(keyProperty);
		return keyProperty;
	}

	protected KeyProperty<T> CreateProperty<T>(KeyPropertyDescriptor descriptor, T defaultValue)
	{
		KeyProperty<T> keyProperty = new KeyProperty<T>(descriptor, defaultValue);
		RegisterProperty(keyProperty);
		return keyProperty;
	}

	protected KeyProperty<T> CreateProperty<T>(KeyPropertyDescriptor descriptor, IConverter converter, T defaultValue)
	{
		KeyProperty<T> keyProperty = new KeyProperty<T>(descriptor, converter, defaultValue);
		RegisterProperty(keyProperty);
		return keyProperty;
	}

	private void RegisterProperty(KeyPropertyDescriptor descriptor, IProperty property)
	{
		if (descriptor.Name != null)
		{
			properties[descriptor.Name] = property;
		}
	}

	private void RegisterProperty(IProperty property)
	{
		RegisterProperty(property.Descriptor, property);
	}
}
