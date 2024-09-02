namespace DocGen.Pdf.Parsing;

internal class SystemFontProperty<T> : ISystemFontProperty
{
	private readonly ISystemFontConverter converter;

	private T value;

	public SystemFontPropertyDescriptor Descriptor { get; set; }

	public SystemFontProperty(SystemFontPropertyDescriptor descriptor)
	{
		Descriptor = descriptor;
	}

	public SystemFontProperty(SystemFontPropertyDescriptor descriptor, ISystemFontConverter converter)
		: this(descriptor)
	{
		this.converter = converter;
	}

	public SystemFontProperty(SystemFontPropertyDescriptor descriptor, T defaultValue)
		: this(descriptor)
	{
		value = defaultValue;
	}

	public SystemFontProperty(SystemFontPropertyDescriptor descriptor, ISystemFontConverter converter, T defaultValue)
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
