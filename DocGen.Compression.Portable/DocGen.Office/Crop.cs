using System;

namespace DocGen.Office;

internal class Crop : ICrop, IDisposable
{
	private IShapeFrame _shapeFrame;

	private int _bottomCrop;

	private int _leftCrop;

	private int _rightCrop;

	private int _topCrop;

	internal double BottomCrop
	{
		get
		{
			return (double)_bottomCrop / 100000.0;
		}
		set
		{
			_bottomCrop = (int)(value * 100000.0);
		}
	}

	internal double LeftCrop
	{
		get
		{
			return (double)_leftCrop / 100000.0;
		}
		set
		{
			_leftCrop = (int)(value * 100000.0);
		}
	}

	internal double RightCrop
	{
		get
		{
			return (double)_rightCrop / 100000.0;
		}
		set
		{
			_rightCrop = (int)(value * 100000.0);
		}
	}

	internal double TopCrop
	{
		get
		{
			return (double)_topCrop / 100000.0;
		}
		set
		{
			_topCrop = (int)(value * 100000.0);
		}
	}

	public float OffsetX
	{
		get
		{
			float num = (float)(_shapeFrame.Width / (1.0 - (RightCrop + LeftCrop)));
			float num2 = (float)((0.0 - LeftCrop) * (double)num + _shapeFrame.Left);
			return (float)((double)((num - (float)_shapeFrame.Width) / 2f) + ((double)num2 - _shapeFrame.Left));
		}
		set
		{
			if (value > 169092f || value < -169092f)
			{
				throw new ArgumentException("OffsetX value must be between -169092 and 169092");
			}
			float num = (float)(_shapeFrame.Width / (1.0 - (RightCrop + LeftCrop)));
			LeftCrop = ((double)num - _shapeFrame.Width) / (double)(2f * num) - (double)(value / num);
			RightCrop = ((double)num - _shapeFrame.Width) / (double)(2f * num) + (double)(value / num);
		}
	}

	public float OffsetY
	{
		get
		{
			float num = (float)(_shapeFrame.Height / (1.0 - (TopCrop + BottomCrop)));
			float num2 = (float)((0.0 - TopCrop) * (double)num + _shapeFrame.Top);
			return (float)((double)((num - (float)_shapeFrame.Height) / 2f) + ((double)num2 - _shapeFrame.Top));
		}
		set
		{
			if (value > 169092f || value < -169092f)
			{
				throw new ArgumentException("OffsetY value must be between -169092 and 169092");
			}
			float num = (float)(_shapeFrame.Height / (1.0 - (TopCrop + BottomCrop)));
			TopCrop = ((double)num - _shapeFrame.Height) / (double)(2f * num) - (double)(value / num);
			BottomCrop = ((double)num - _shapeFrame.Height) / (double)(2f * num) + (double)(value / num);
		}
	}

	public float Width
	{
		get
		{
			return (float)(_shapeFrame.Width / (1.0 - (RightCrop + LeftCrop)));
		}
		set
		{
			if (value > 169092f || value < 0f)
			{
				throw new ArgumentException("Width value must be between 0 and 169092");
			}
			float num = (float)(_shapeFrame.Width / (1.0 - (RightCrop + LeftCrop)));
			float num2 = (float)(LeftCrop * (double)num + _shapeFrame.Left);
			float num3 = (num - (float)_shapeFrame.Width) / 2f + (num2 - (float)_shapeFrame.Left);
			RightCrop = 0.0 - ((_shapeFrame.Width - (double)value) / (double)(2f * value) + (double)(num3 / value));
			LeftCrop = 0.0 - ((_shapeFrame.Width - (double)value) / (double)(2f * value) - (double)(num3 / value));
		}
	}

	public float Height
	{
		get
		{
			return (float)(_shapeFrame.Height / (1.0 - (TopCrop + BottomCrop)));
		}
		set
		{
			if (value > 169092f || value < 0f)
			{
				throw new ArgumentException("Height value must be between 0 and 169092");
			}
			float num = (float)(_shapeFrame.Height / (1.0 - (TopCrop + BottomCrop)));
			float num2 = (float)((TopCrop - BottomCrop) * _shapeFrame.Height + _shapeFrame.Top);
			float num3 = (num - (float)_shapeFrame.Height) / 2f + (num2 - (float)_shapeFrame.Top);
			TopCrop = 0.0 - ((_shapeFrame.Height - (double)value) / (double)(2f * value) + (double)(num3 / value));
			BottomCrop = 0.0 - ((_shapeFrame.Height - (double)value) / (double)(2f * value) - (double)(num3 / value));
		}
	}

	public float ContainerLeft
	{
		get
		{
			return (float)_shapeFrame.Left;
		}
		set
		{
			if (value > 169092f || value < -169092f)
			{
				throw new ArgumentException("ContainerLeft value must be between -169092 and 169092");
			}
			float num = (float)(_shapeFrame.Width / (1.0 - (RightCrop + LeftCrop)));
			float num2 = (float)((0.0 - LeftCrop) * (double)num + _shapeFrame.Left);
			_shapeFrame.Left = value;
			LeftCrop = 0f - (num2 - value) / num;
			RightCrop = (num - (float)_shapeFrame.Width) / num + (num2 - value) / num;
		}
	}

	public float ContainerTop
	{
		get
		{
			return (float)_shapeFrame.Top;
		}
		set
		{
			if (value > 169092f || value < -169092f)
			{
				throw new ArgumentException("ContainerTop value must be between -169092 and 169092");
			}
			float num = (float)(_shapeFrame.Height / (1.0 - (TopCrop + BottomCrop)));
			float num2 = (float)((0.0 - TopCrop) * (double)num + _shapeFrame.Top);
			_shapeFrame.Top = value;
			TopCrop = 0f - (num2 - value) / num;
			BottomCrop = (num - (float)_shapeFrame.Height) / num + (num2 - value) / num;
		}
	}

	public float ContainerWidth
	{
		get
		{
			return (float)_shapeFrame.Width;
		}
		set
		{
			if (value > 169092f || value < 0f)
			{
				throw new ArgumentException("ContainerWidth value must be between 0 and 169092");
			}
			float num = (float)(_shapeFrame.Width / (1.0 - (RightCrop + LeftCrop)));
			float num2 = (float)((0.0 - LeftCrop) * (double)num + (double)(float)_shapeFrame.Left);
			_shapeFrame.Width = value;
			RightCrop = (num - value) / num + (num2 - (float)_shapeFrame.Left) / num;
		}
	}

	public float ContainerHeight
	{
		get
		{
			return (float)_shapeFrame.Height;
		}
		set
		{
			if (value > 169092f || value < 0f)
			{
				throw new ArgumentException("ContainerHeight value must be between 0 and 169092");
			}
			float num = (float)(_shapeFrame.Height / (1.0 - (TopCrop + BottomCrop)));
			float num2 = (float)((0.0 - TopCrop) * (double)num + (double)(float)_shapeFrame.Top);
			_shapeFrame.Height = value;
			BottomCrop = (num - value) / num + (num2 - (float)_shapeFrame.Top) / num;
		}
	}

	public Crop(IShapeFrame shapeFrame)
	{
		_shapeFrame = shapeFrame;
	}

	internal Crop Clone()
	{
		return (Crop)MemberwiseClone();
	}

	public void Dispose()
	{
	}
}
