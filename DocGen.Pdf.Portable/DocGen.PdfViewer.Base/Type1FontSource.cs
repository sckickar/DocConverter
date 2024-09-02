using System;

namespace DocGen.PdfViewer.Base;

internal class Type1FontSource : FontSource
{
	public override string FontFamily
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public override bool IsBold
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public override bool IsItalic
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public override short Ascender
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public override short Descender
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public BaseType1Font Font { get; private set; }

	public Type1FontSource(byte[] data)
	{
		Initialize(data);
	}

	public override void GetGlyphName(Glyph glyph)
	{
		ushort cid = (ushort)glyph.CharId.IntValue;
		glyph.Name = Font.GetGlyphName(cid);
	}

	private void Initialize(byte[] data)
	{
		FontInterpreter fontInterpreter = new FontInterpreter();
		fontInterpreter.Execute(Type1FontReader.StripData(data));
		Font = Countable.FirstOrDefault(fontInterpreter.Fonts.Values);
	}

	public override void GetAdvancedWidth(Glyph glyph)
	{
		glyph.AdvancedWidth = (double)(int)Font.GetAdvancedWidth(glyph) / 1000.0;
	}

	public override void GetGlyphOutlines(Glyph glyph, double fontSize)
	{
		Font.GetGlyphOutlines(glyph, fontSize);
	}
}
