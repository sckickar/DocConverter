using DocGen.ComponentModel;

namespace DocGen.Chart;

internal abstract class ChartConfigItem : IChangeNotifyingItem
{
	private bool noEvents;

	protected bool NoEvents
	{
		get
		{
			return noEvents;
		}
		set
		{
			noEvents = value;
		}
	}

	public event SyncfusionPropertyChangedEventHandler PropertyChanged;

	protected void RaisePropertyChanged(string propertyName)
	{
		RaisePropertyChanged(PropertyChangeEffect.NeedRepaint, propertyName, null, null);
	}

	protected void RaisePropertyChanged(PropertyChangeEffect effect, string propertyName, object oldValue, object newValue)
	{
		if (!noEvents)
		{
			SyncfusionPropertyChangedEventArgs e = new SyncfusionPropertyChangedEventArgs(effect, propertyName, oldValue, newValue);
			if (this.PropertyChanged != null)
			{
				this.PropertyChanged(this, e);
			}
		}
	}
}
