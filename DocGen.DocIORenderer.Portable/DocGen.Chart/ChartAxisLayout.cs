using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using DocGen.Drawing;
using DocGen.Drawing.SkiaSharpHelper;

namespace DocGen.Chart;

internal sealed class ChartAxisLayout
{
	private class AxesCollection : Collection<ChartAxis>
	{
		private ChartAxisLayout m_owner;

		public AxesCollection(ChartAxisLayout owner)
		{
			m_owner = owner;
		}

		protected override void InsertItem(int index, ChartAxis item)
		{
			item.Layout = m_owner;
			base.InsertItem(index, item);
		}

		protected override void ClearItems()
		{
			using (IEnumerator<ChartAxis> enumerator = GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					enumerator.Current.Layout = null;
				}
			}
			base.ClearItems();
		}

		protected override void RemoveItem(int index)
		{
			base.RemoveItem(index);
		}

		protected override void SetItem(int index, ChartAxis item)
		{
			base[index].Layout = null;
			item.Layout = m_owner;
			base.SetItem(index, item);
		}
	}

	private class AxisLayoutException : Exception
	{
		public AxisLayoutException(string message)
			: base(message)
		{
		}
	}

	private AxesCollection m_axes;

	private ChartAxesLayoutMode m_layoutMode = ChartAxesLayoutMode.Stacking;

	private ChartAxisLayoutCollection m_owner;

	private SizeF m_dimention = SizeF.Empty;

	private float m_spacing;

	private Unit m_height = Unit.Empty;

	public IList<ChartAxis> Axes => m_axes;

	public ChartAxesLayoutMode LayoutMode
	{
		get
		{
			return m_layoutMode;
		}
		set
		{
			m_layoutMode = value;
		}
	}

	public Unit Height
	{
		get
		{
			return m_height;
		}
		set
		{
			m_height = value;
		}
	}

	public float Spacing
	{
		get
		{
			return m_spacing;
		}
		set
		{
			if (m_spacing != value)
			{
				m_spacing = value;
			}
		}
	}

	internal ChartAxisLayoutCollection Owner
	{
		get
		{
			return m_owner;
		}
		set
		{
			if (value == null)
			{
				m_owner = value;
				return;
			}
			if (m_owner != null)
			{
				throw new ArgumentException("ChartAxisLayout is already added");
			}
			m_owner = value;
		}
	}

	private ChartOrientation Orientation
	{
		get
		{
			if (m_axes.Count > 0)
			{
				return m_axes[0].Orientation;
			}
			return ChartOrientation.Horizontal;
		}
	}

	public ChartAxisLayout()
	{
		m_axes = new AxesCollection(this);
	}

	internal void Validate(ChartOrientation orientation)
	{
		if (m_axes.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < m_axes.Count; i++)
		{
			if (m_axes[i].Orientation != orientation)
			{
				throw new AxisLayoutException("Axes should have the same orientation");
			}
		}
	}

	internal void Arrange(RectangleF bounds, ChartOrientation orientation)
	{
		if (orientation == ChartOrientation.Horizontal)
		{
			_ = m_owner.ChartArea.AxisSpacing;
			if (m_layoutMode == ChartAxesLayoutMode.SideBySide)
			{
				float num = 0f;
				float num2 = (bounds.Width - (float)(m_axes.Count - 1) * m_spacing) / (float)m_axes.Count;
				{
					foreach (ChartAxis axis in m_axes)
					{
						axis.DockToRectangle(new RectangleF(bounds.X + num, bounds.Y, num2, bounds.Height));
						num += num2 + m_spacing;
					}
					return;
				}
			}
			RectangleF rect = bounds;
			{
				foreach (ChartAxis axis2 in m_axes)
				{
					rect = axis2.DockToRectangle(rect);
					if (axis2.OpposedPosition)
					{
						rect.Y -= m_spacing;
					}
					rect.Height += m_spacing;
				}
				return;
			}
		}
		_ = m_owner.ChartArea.AxisSpacing;
		if (m_layoutMode == ChartAxesLayoutMode.SideBySide)
		{
			float num3 = 0f;
			float num4 = (bounds.Height - (float)(m_axes.Count - 1) * m_spacing) / (float)m_axes.Count;
			{
				foreach (ChartAxis axis3 in m_axes)
				{
					axis3.DockToRectangle(new RectangleF(bounds.X, bounds.Y + num3, bounds.Width, num4));
					num3 += num4 + m_spacing;
				}
				return;
			}
		}
		RectangleF rect2 = bounds;
		foreach (ChartAxis axis4 in m_axes)
		{
			rect2 = axis4.DockToRectangle(rect2);
			if (!axis4.OpposedPosition)
			{
				rect2.X -= m_spacing;
			}
			rect2.Width += m_spacing;
		}
	}

	internal void Measure(Graphics g, RectangleF bounds, ChartOrientation orientation, out float left, out float right, out float scrolls)
	{
		left = 0f;
		right = 0f;
		scrolls = 0f;
		if (orientation == ChartOrientation.Vertical)
		{
			foreach (ChartAxis axis in m_axes)
			{
				float dimension = axis.GetDimension(g, m_owner.ChartArea, bounds);
				if (m_layoutMode == ChartAxesLayoutMode.Stacking)
				{
					if (axis.OpposedPosition)
					{
						right += dimension;
					}
					else
					{
						left += dimension;
					}
				}
				else if (axis.OpposedPosition)
				{
					right = Math.Max(dimension, right);
				}
				else
				{
					left = Math.Max(dimension, left);
				}
			}
			return;
		}
		foreach (ChartAxis axis2 in m_axes)
		{
			float dimension2 = axis2.GetDimension(g, m_owner.ChartArea, bounds);
			if (m_layoutMode == ChartAxesLayoutMode.Stacking)
			{
				if (axis2.OpposedPosition)
				{
					right += dimension2;
				}
				else
				{
					left += dimension2;
				}
			}
			else if (axis2.OpposedPosition)
			{
				right = Math.Max(dimension2, right);
			}
			else
			{
				left = Math.Max(dimension2, left);
			}
		}
	}
}
