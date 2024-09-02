using System;
using DocGen.Drawing;

namespace DocGen.Chart;

internal class LocationEventArgs : EventArgs
{
	private Point m_location;

	private bool m_allowed;

	public Point Location => m_location;

	public bool Allowed
	{
		get
		{
			return m_allowed;
		}
		set
		{
			m_allowed = value;
		}
	}

	public LocationEventArgs(Point location)
	{
		m_location = location;
		m_allowed = true;
	}
}
