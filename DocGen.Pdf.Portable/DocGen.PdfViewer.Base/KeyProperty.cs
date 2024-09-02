namespace DocGen.PdfViewer.Base;

internal class KeyProperty<T> : IProperty
{
	private readonly IConverter converter;

	private T value;

	public KeyPropertyDescriptor Descriptor { get; set; }

	public KeyProperty(KeyPropertyDescriptor descriptor)
	{
		Descriptor = descriptor;
	}

	public KeyProperty(KeyPropertyDescriptor descriptor, IConverter converter)
		: this(descriptor)
	{
		this.converter = converter;
	}

	public KeyProperty(KeyPropertyDescriptor descriptor, T defaultValue)
		: this(descriptor)
	{
		value = defaultValue;
	}

	public KeyProperty(KeyPropertyDescriptor descriptor, IConverter converter, T defaultValue)
		: this(descriptor)
	{
		value = defaultValue;
		this.converter = converter;
	}

	public T GetValue()
	{
		return value;
	}

	public bool SetValue(object value)
	{
		if (value is T)
		{
			this.value = (T)value;
			return true;
		}
		if (converter != null)
		{
			this.value = (T)converter.Convert(typeof(T), value);
			return true;
		}
		return false;
	}
}
