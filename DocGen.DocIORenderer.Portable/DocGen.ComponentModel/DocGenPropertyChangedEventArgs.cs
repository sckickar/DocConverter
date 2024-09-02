using System.ComponentModel;

namespace DocGen.ComponentModel;

internal class SyncfusionPropertyChangedEventArgs : PropertyChangedEventArgs
{
	private PropertyChangeEffect propertyChangeType;

	private object oldValue;

	private object newValue;

	public PropertyChangeEffect PropertyChangeEffect
	{
		get
		{
			return propertyChangeType;
		}
		set
		{
			propertyChangeType = value;
		}
	}

	public SyncfusionPropertyChangedEventArgs(PropertyChangeEffect propertyChangeType, string propertyName, object oldValue, object newValue)
		: base(propertyName)
	{
		this.propertyChangeType = propertyChangeType;
		this.oldValue = oldValue;
		this.newValue = newValue;
	}
}
