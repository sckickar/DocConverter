using DocGen.Drawing;

namespace DocGen.OfficeChart.Implementation;

internal class TextInfoImpl
{
	private readonly string _text;

	private float _ascent;

	private RectangleF _bounds;

	private IOfficeFont _font;

	private string _unicodeFont;

	private int _length;

	private int _position;

	internal string Text => _text.Substring(_position, _length);

	internal IOfficeFont Font
	{
		get
		{
			return _font;
		}
		set
		{
			_font = value;
		}
	}

	internal string UnicodeFont
	{
		get
		{
			return _unicodeFont;
		}
		set
		{
			_unicodeFont = value;
		}
	}

	internal RectangleF Bounds
	{
		get
		{
			return _bounds;
		}
		set
		{
			_bounds = value;
		}
	}

	internal float Ascent
	{
		get
		{
			return _ascent;
		}
		set
		{
			_ascent = value;
		}
	}

	internal int Position
	{
		get
		{
			return _position;
		}
		set
		{
			_position = value;
		}
	}

	internal int Length
	{
		get
		{
			return _length;
		}
		set
		{
			_length = value;
		}
	}

	internal float X
	{
		get
		{
			return _bounds.X;
		}
		set
		{
			_bounds.X = value;
		}
	}

	internal float Y
	{
		get
		{
			return _bounds.Y;
		}
		set
		{
			_bounds.Y = value;
		}
	}

	internal float Width
	{
		get
		{
			return _bounds.Width;
		}
		set
		{
			_bounds.Width = value;
		}
	}

	internal float Height
	{
		get
		{
			return _bounds.Height;
		}
		set
		{
			_bounds.Height = value;
		}
	}

	internal TextInfoImpl(string text)
	{
		_text = text;
	}

	internal string GetOriginalText()
	{
		return _text;
	}

	internal void CopyTo(TextInfoImpl destination)
	{
		destination.Font = Font;
		destination.Ascent = Ascent;
		destination.UnicodeFont = UnicodeFont;
	}

	internal void Dispose()
	{
		_font = null;
	}
}
