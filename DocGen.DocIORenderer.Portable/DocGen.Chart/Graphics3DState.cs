using System;
using System.ComponentModel;

namespace DocGen.Chart;

internal class Graphics3DState
{
	private bool m_autoPerspective = true;

	private bool m_perpective = true;

	private bool m_light = true;

	private Vector3D m_lightPosition = new Vector3D(0.0, 0.0, 1.0);

	private float m_lightCoef = 16f;

	private float m_zDist = 200f;

	public Vector3D LightPosition
	{
		get
		{
			return m_lightPosition;
		}
		set
		{
			if (m_lightPosition != value)
			{
				m_lightPosition = value;
				RaiseChanged();
			}
		}
	}

	[DefaultValue(16)]
	public float LightCoeficient
	{
		get
		{
			return m_lightCoef;
		}
		set
		{
			if (m_lightCoef != value)
			{
				m_lightCoef = value;
				RaiseChanged();
			}
		}
	}

	[DefaultValue(true)]
	public bool Perspective
	{
		get
		{
			return m_perpective;
		}
		set
		{
			if (m_perpective != value)
			{
				m_perpective = value;
				RaiseChanged();
			}
		}
	}

	[DefaultValue(true)]
	public bool AutoPerspective
	{
		get
		{
			return m_autoPerspective;
		}
		set
		{
			if (m_autoPerspective != value)
			{
				m_autoPerspective = value;
				RaiseChanged();
			}
		}
	}

	[DefaultValue(true)]
	public bool Light
	{
		get
		{
			return m_light;
		}
		set
		{
			if (m_light != value)
			{
				m_light = value;
				RaiseChanged();
			}
		}
	}

	[DefaultValue(200f)]
	public float ZDistant
	{
		get
		{
			return m_zDist;
		}
		set
		{
			if (m_zDist != value)
			{
				m_zDist = value;
				RaiseChanged();
			}
		}
	}

	public event EventHandler Changed;

	private void RaiseChanged()
	{
		if (this.Changed != null)
		{
			this.Changed(this, EventArgs.Empty);
		}
	}
}
