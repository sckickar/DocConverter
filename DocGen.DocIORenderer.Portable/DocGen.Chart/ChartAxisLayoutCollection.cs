using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using DocGen.Drawing;
using DocGen.Drawing.SkiaSharpHelper;

namespace DocGen.Chart;

internal sealed class ChartAxisLayoutCollection : Collection<ChartAxisLayout>
{
	private ChartArea m_owner;

	private float m_spacing;

	internal ChartArea ChartArea => m_owner;

	public float Spacing
	{
		get
		{
			return m_spacing;
		}
		set
		{
			m_spacing = value;
		}
	}

	internal ChartAxisLayoutCollection(ChartArea owner)
	{
		m_owner = owner;
	}

	protected override void ClearItems()
	{
		using (IEnumerator<ChartAxisLayout> enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				enumerator.Current.Owner = null;
			}
		}
		base.ClearItems();
	}

	protected override void InsertItem(int index, ChartAxisLayout item)
	{
		if (item == null)
		{
			throw new ArgumentNullException("item");
		}
		item.Owner = this;
		base.InsertItem(index, item);
	}

	protected override void RemoveItem(int index)
	{
		base[index].Owner = null;
		base.RemoveItem(index);
	}

	protected override void SetItem(int index, ChartAxisLayout item)
	{
		if (item == null)
		{
			throw new ArgumentNullException("item");
		}
		base[index].Owner = null;
		item.Owner = this;
		base.SetItem(index, item);
	}

	internal void Validate(ChartOrientation orientation)
	{
		if (base.Count <= 0)
		{
			return;
		}
		foreach (ChartAxis axis in m_owner.Axes)
		{
			if (axis.Orientation == orientation && (axis.Layout == null || axis.Layout.Owner != this))
			{
				throw new Exception($"Layouts collection should contains all {orientation} axes.");
			}
		}
		using IEnumerator<ChartAxisLayout> enumerator2 = GetEnumerator();
		while (enumerator2.MoveNext())
		{
			enumerator2.Current.Validate(orientation);
		}
	}

	internal void Arrange(RectangleF bounds, ChartOrientation orientation)
	{
		float num = 0f;
		if (orientation == ChartOrientation.Horizontal)
		{
			float num2 = (bounds.Width - (float)(base.Count - 1) * m_spacing) / (float)base.Count;
		}
		float num3 = (bounds.Height - (float)(base.Count - 1) * m_spacing) / (float)base.Count;
		float num4 = 0f;
		float num5 = 0f;
		float num6 = 0f;
		int num7 = base.Count;
		if (bounds.Height > num4)
		{
			num6 = bounds.Height - num4;
		}
		using IEnumerator<ChartAxisLayout> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			ChartAxisLayout current2 = enumerator.Current;
			switch (current2.Height.Type)
			{
			case UnitType.Percentage:
				if (current2.Height.Value > 0.0)
				{
					num3 = (float)current2.Height.Value * 100f / num5 * num6 / 100f;
				}
				break;
			case UnitType.Pixel:
				if (current2.Height.Value > 0.0)
				{
					num3 = (float)current2.Height.Value;
				}
				break;
			}
			current2.Arrange(new RectangleF(bounds.X, bounds.Y + num, bounds.Width, num3), orientation);
			num += num3 + m_spacing;
		}
	}

	internal void Measure(Graphics g, RectangleF bounds, ChartOrientation orientation, out float left, out float right, out float scrolls)
	{
		left = 0f;
		right = 0f;
		scrolls = 0f;
		using IEnumerator<ChartAxisLayout> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			enumerator.Current.Measure(g, bounds, orientation, out var left2, out var right2, out var scrolls2);
			left = Math.Max(left, left2);
			right = Math.Max(right, right2);
			scrolls = Math.Max(scrolls, scrolls2);
		}
	}
}
