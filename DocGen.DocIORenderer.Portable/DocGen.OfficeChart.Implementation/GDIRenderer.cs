using System;
using System.Collections.Generic;
using DocGen.Drawing;
using DocGen.Drawing.SkiaSharpHelper;

namespace DocGen.OfficeChart.Implementation;

internal class GDIRenderer : RendererBase
{
	private Graphics _graphics;

	private WorkbookImpl _workbookImpl;

	private int _scale;

	private StringFormat _stringFormat;

	internal override bool IsHfRtfProcess
	{
		get
		{
			return false;
		}
		set
		{
		}
	}

	internal override RectangleF HfImageBounds
	{
		get
		{
			return default(RectangleF);
		}
		set
		{
		}
	}

	internal GDIRenderer(Graphics graphics, StringFormat stringFormat, List<IOfficeFont> fonts, List<string> drawString, WorkbookImpl workbook, int scale)
		: base(null, stringFormat, fonts, drawString, workbook)
	{
		_graphics = graphics;
		_stringFormat = stringFormat;
		_workbookImpl = workbook;
		_scale = scale;
	}

	internal override string CheckPdfFont(Font sysFont, string testString)
	{
		return sysFont.Name;
	}

	internal override bool CheckUnicode(string unicodeText)
	{
		char[] array = unicodeText.ToCharArray();
		char[] array2 = new char[1] { '€' };
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] > 'ÿ' && Array.IndexOf(array2, array[i]) < 0)
			{
				return true;
			}
		}
		return false;
	}

	internal override void DrawString(TextInfoImpl textInfo, StringFormat stringFormat)
	{
		_graphics.DrawString(textInfo.Text, GetSystemFont(textInfo), new SolidBrush(NormalizeColor(textInfo.Font.RGBColor)), textInfo.Bounds, _stringFormat);
	}

	internal override void DrawTextTemplate(RectangleF bounds, List<LineInfoImpl> lineInfoCollection, float y)
	{
		DrawTextTemplate(bounds, _graphics, lineInfoCollection, y);
	}

	internal override float FindAscent(string text, Font font)
	{
		return (float)font.GetAscent(text);
	}

	private void DrawTextTemplate(RectangleF bounds, Graphics graphics, List<LineInfoImpl> lineInfoCollection, float y)
	{
		foreach (LineInfoImpl item in lineInfoCollection)
		{
			foreach (TextInfoImpl item2 in item.TextInfoCollection)
			{
				item2.Y += y;
				_graphics.SetClip(bounds);
				_graphics.DrawString(item2.Text, GetSystemFont(item2), new SolidBrush(NormalizeColor(item2.Font.RGBColor)), item2.Bounds, _stringFormat);
				_graphics.ResetClip();
			}
		}
	}

	internal override void InitializeStringFormat()
	{
	}

	internal override SizeF MeasureString(string text, Font systemFont)
	{
		return _graphics.MeasureString(text, systemFont, new PointF(0f, 0f), _stringFormat);
	}

	internal override string SwitchFonts(string text, byte charSet, string fontName)
	{
		return fontName;
	}

	internal override bool IsJustify()
	{
		return false;
	}
}
