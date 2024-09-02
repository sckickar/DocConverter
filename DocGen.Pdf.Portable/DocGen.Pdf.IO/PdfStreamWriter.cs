using System;
using System.Collections;
using System.Text;
using DocGen.Drawing;
using DocGen.Pdf.ColorSpace;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.IO;

internal class PdfStreamWriter : IPdfWriter
{
	private PdfStream m_stream;

	public long Position
	{
		get
		{
			return m_stream.InternalStream.Position;
		}
		set
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public long Length => m_stream.InternalStream.Length;

	public PdfDocumentBase Document
	{
		get
		{
			return null;
		}
		set
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public PdfStreamWriter(PdfStream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		m_stream = stream;
	}

	public void ModifyTM(PdfTransformationMatrix matrix)
	{
		m_stream.Write(matrix.ToString());
		m_stream.Write(" ");
		WriteOperator("Tm");
	}

	public void SetFont(PdfFont font, string name, float size)
	{
		SetFont(font, new PdfName(name), size);
	}

	public void SetFont(PdfFont font, PdfName name, float size)
	{
		if (font == null)
		{
			throw new ArgumentNullException("font");
		}
		m_stream.Write(name.ToString());
		m_stream.Write(" ");
		m_stream.Write(PdfNumber.FloatToString(size));
		m_stream.Write(" ");
		WriteOperator("Tf");
	}

	public void SetColorSpace(PdfColorSpaces colorspace, PdfName name)
	{
		if (colorspace == null)
		{
			throw new ArgumentNullException("Color Space");
		}
		m_stream.Write(name.ToString());
		m_stream.Write(" ");
		WriteOperator("CS");
		m_stream.Write(name.ToString());
		m_stream.Write(" ");
		WriteOperator("cs");
	}

	public void SetCharacterSpacing(float charSpacing)
	{
		m_stream.Write(PdfNumber.FloatToString(charSpacing));
		m_stream.Write(" ");
		m_stream.Write("Tc");
		m_stream.Write("\r\n");
	}

	public void SetWordSpacing(float wordSpacing)
	{
		m_stream.Write(PdfNumber.FloatToString(wordSpacing));
		m_stream.Write(" ");
		WriteOperator("Tw");
	}

	public void SetHorizontalScaling(float scalingFactor)
	{
		m_stream.Write(PdfNumber.FloatToString(scalingFactor));
		m_stream.Write(" ");
		WriteOperator("Tz");
	}

	public void SetLeading(float leading)
	{
		m_stream.Write(PdfNumber.FloatToString(leading));
		m_stream.Write(" ");
		WriteOperator("TL");
	}

	public void SetTextRenderingMode(TextRenderingMode renderingMode)
	{
		PdfStream stream = m_stream;
		int num = (int)renderingMode;
		stream.Write(num.ToString());
		m_stream.Write(" ");
		WriteOperator("Tr");
	}

	public void SetTextRise(float rise)
	{
		m_stream.Write(PdfNumber.FloatToString(rise));
		m_stream.Write(" ");
		WriteOperator("Ts");
	}

	internal void SetTextScaling(float textScaling)
	{
		m_stream.Write(PdfNumber.FloatToString(textScaling));
		m_stream.Write(" ");
		WriteOperator("Tz");
	}

	public void StartNextLine()
	{
		WriteOperator("T*");
	}

	public void StartNextLine(PointF point)
	{
		WritePoint(point);
		WriteOperator("Td");
	}

	public void StartNextLine(float x, float y)
	{
		WritePoint(x, y);
		WriteOperator("Td");
	}

	public void StartLineAndSetLeading(PointF point)
	{
		WritePoint(point);
		WriteOperator("TD");
	}

	public void StartLineAndSetLeading(float x, float y)
	{
		WritePoint(x, y);
		WriteOperator("TD");
	}

	public void ShowText(byte[] text, bool hex)
	{
		CheckTextParam(text);
		WriteText(text, hex);
		WriteOperator("Tj");
	}

	public void ShowText(string text, bool hex)
	{
		CheckTextParam(text);
		WriteText(text, hex);
		WriteOperator("Tj");
	}

	public void ShowText(PdfString text)
	{
		CheckTextParam(text);
		WriteText(text);
		WriteOperator("Tj");
	}

	public void ShowText(PdfArray formattedText)
	{
		if (formattedText == null)
		{
			throw new ArgumentNullException("formattedText");
		}
		formattedText.Save(this);
		WriteOperator("TJ");
	}

	internal void ShowFormatedText(string text)
	{
		m_stream.Write(text);
		WriteOperator("TJ");
	}

	public void ShowNextLineText(byte[] text, bool hex)
	{
		CheckTextParam(text);
		WriteText(text, hex);
		WriteOperator("'");
	}

	public void ShowNextLineText(string text, bool hex)
	{
		CheckTextParam(text);
		WriteText(text, hex);
		WriteOperator("'");
	}

	public void ShowNextLineText(PdfString text)
	{
		CheckTextParam(text);
		WriteText(text);
		WriteOperator("'");
	}

	public void ShowNextLineTextWithSpacings(float wordSpacing, float charSpacing, byte[] text, bool hex)
	{
		CheckTextParam(text);
		WritePoint(wordSpacing, charSpacing);
		WriteText(text, hex);
		WriteOperator("\"");
	}

	public void ShowNextLineTextWithSpacings(float wordSpacing, float charSpacing, string text, bool hex)
	{
		CheckTextParam(text);
		WritePoint(wordSpacing, charSpacing);
		WriteText(text, hex);
		WriteOperator("\"");
	}

	public void ShowNextLineTextWithSpacings(float wordSpacing, float charSpacing, PdfString text)
	{
		CheckTextParam(text);
		WritePoint(wordSpacing, charSpacing);
		WriteText(text);
		WriteOperator("\"");
	}

	public void ShowText(IList formatting)
	{
		if (formatting == null)
		{
			throw new ArgumentNullException("formatting");
		}
		m_stream.Write("[");
		foreach (object item in formatting)
		{
			if (item == null)
			{
				throw new ArgumentException("Invalid formatting", "formatting");
			}
			if (item is PdfNumber)
			{
				m_stream.Write((item as PdfNumber).IntValue.ToString());
				m_stream.Write(" ");
			}
			else if (item is int)
			{
				m_stream.Write(item.ToString());
				m_stream.Write(" ");
			}
			else
			{
				WriteText(item);
			}
		}
		m_stream.Write("]");
		WriteOperator("TJ");
	}

	public void BeginText()
	{
		WriteOperator("BT");
	}

	internal void WriteTag(string tag)
	{
		m_stream.Write(tag);
		m_stream.Write("\r\n");
	}

	public void EndText()
	{
		WriteOperator("ET");
	}

	public void BeginMarkupSequence(string name)
	{
		if (name == null || name == string.Empty)
		{
			throw new ArgumentNullException("name");
		}
		m_stream.Write("/");
		m_stream.Write(name);
		m_stream.Write(" ");
		WriteOperator("BMC");
	}

	public void BeginMarkupSequence(PdfName name)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		m_stream.Write(name.ToString());
		m_stream.Write(" ");
		WriteOperator("BMC");
	}

	public void EndMarkupSequence()
	{
		WriteOperator("EMC");
	}

	public void WriteComment(string comment)
	{
		if (comment != null && comment.Length > 0)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("%");
			stringBuilder.Append(" ");
			stringBuilder.Append(comment);
			WriteOperator(stringBuilder.ToString());
		}
	}

	public void BeginPath(PointF startPoint)
	{
		BeginPath(startPoint.X, startPoint.Y);
	}

	public void BeginPath(float x, float y)
	{
		WritePoint(x, y);
		WriteOperator("m");
	}

	public void AppendBezierSegment(PointF p1, PointF p2, PointF p3)
	{
		AppendBezierSegment(p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y);
	}

	public void AppendBezierSegment(float x1, float y1, float x2, float y2, float x3, float y3)
	{
		WritePoint(x1, y1);
		WritePoint(x2, y2);
		WritePoint(x3, y3);
		WriteOperator("c");
	}

	public void AppendBezierSegment(PointF p2, PointF p3, bool useFirstPoint)
	{
		AppendBezierSegment(p2.X, p2.Y, p3.X, p3.Y, useFirstPoint);
	}

	public void AppendBezierSegment(float x2, float y2, float x3, float y3, bool useFirstPoint)
	{
		WritePoint(x2, y2);
		WritePoint(x3, y3);
		if (useFirstPoint)
		{
			WriteOperator("y");
		}
		else
		{
			WriteOperator("v");
		}
	}

	public void AppendLineSegment(PointF point)
	{
		AppendLineSegment(point.X, point.Y);
	}

	public void AppendLineSegment(float x, float y)
	{
		WritePoint(x, y);
		WriteOperator("l");
	}

	public void AppendRectangle(RectangleF rect)
	{
		AppendRectangle(rect.X, rect.Y, rect.Width, rect.Height);
	}

	public void AppendRectangle(float x, float y, float width, float height)
	{
		WritePoint(x, y);
		WritePoint(width, height);
		WriteOperator("re");
	}

	public void ClosePath()
	{
		WriteOperator("h");
	}

	public void CloseSubPath()
	{
		WriteOperator("h");
	}

	public void FillPath(bool useEvenOddRule)
	{
		m_stream.Write("f");
		if (useEvenOddRule)
		{
			m_stream.Write("*");
		}
		m_stream.Write("\r\n");
	}

	public void StrokePath()
	{
		WriteOperator("S");
	}

	public void FillStrokePath(bool useEvenOddRule)
	{
		m_stream.Write("B");
		if (useEvenOddRule)
		{
			m_stream.Write("*");
		}
		m_stream.Write("\r\n");
	}

	public void CloseStrokePath()
	{
		WriteOperator("s");
	}

	public void CloseFillStrokePath(bool useEvenOddRule)
	{
		m_stream.Write("b");
		if (useEvenOddRule)
		{
			m_stream.Write("*");
		}
		m_stream.Write("\r\n");
	}

	public void CloseFillPath(bool useEvenOddRule)
	{
		WriteOperator("h");
		m_stream.Write("f");
		if (useEvenOddRule)
		{
			m_stream.Write("*");
		}
		m_stream.Write("\r\n");
	}

	public void ClipPath(bool useEvenOddRule)
	{
		m_stream.Write("W");
		if (useEvenOddRule)
		{
			m_stream.Write("*");
		}
		m_stream.Write(" ");
		m_stream.Write("n");
		m_stream.Write("\r\n");
	}

	public void EndPath()
	{
		WriteOperator("n");
	}

	public void SaveGraphicsState()
	{
		WriteOperator("q");
	}

	public void RestoreGraphicsState()
	{
		WriteOperator("Q");
	}

	public void ModifyCTM(PdfTransformationMatrix matrix)
	{
		if (matrix == null)
		{
			throw new ArgumentNullException("matrix");
		}
		m_stream.Write(matrix.ToString());
		m_stream.Write(" ");
		WriteOperator("cm");
	}

	public void SetLineWidth(float width)
	{
		m_stream.Write(PdfNumber.FloatToString(width));
		m_stream.Write(" ");
		WriteOperator("w");
	}

	public void SetLineCap(PdfLineCap lineCapStyle)
	{
		PdfStream stream = m_stream;
		int num = (int)lineCapStyle;
		stream.Write(num.ToString());
		m_stream.Write(" ");
		WriteOperator("J");
	}

	public void SetLineJoin(PdfLineJoin lineJoinStyle)
	{
		PdfStream stream = m_stream;
		int num = (int)lineJoinStyle;
		stream.Write(num.ToString());
		m_stream.Write(" ");
		WriteOperator("j");
	}

	public void SetMiterLimit(float miterLimit)
	{
		m_stream.Write(PdfNumber.FloatToString(miterLimit));
		m_stream.Write(" ");
		WriteOperator("M");
	}

	public void SetLineDashPattern(float[] pattern, float patternOffset)
	{
		PdfArray pattern2 = new PdfArray(pattern);
		PdfNumber patternOffset2 = new PdfNumber(patternOffset);
		SetLineDashPattern(pattern2, patternOffset2);
	}

	private void SetLineDashPattern(PdfArray pattern, PdfNumber patternOffset)
	{
		pattern.Save(this);
		m_stream.Write(" ");
		patternOffset.Save(this);
		m_stream.Write(" ");
		WriteOperator("d");
	}

	public void SetColorRenderingIntent(ColorIntent intent)
	{
		m_stream.Write("/");
		m_stream.Write(intent.ToString());
		m_stream.Write(" ");
		WriteOperator("ri");
	}

	public void SetFlatnessTolerance(int tolerance)
	{
		m_stream.Write(PdfNumber.FloatToString(tolerance));
		m_stream.Write(" ");
		WriteOperator("i");
	}

	public void SetGraphicsState(PdfName dictionaryName)
	{
		if (dictionaryName == null || dictionaryName == string.Empty)
		{
			throw new ArgumentNullException("dictionaryName");
		}
		m_stream.Write(dictionaryName.ToString());
		m_stream.Write(" ");
		WriteOperator("gs");
	}

	public void SetGraphicsState(string dictionaryName)
	{
		if (dictionaryName == null || dictionaryName == string.Empty)
		{
			throw new ArgumentNullException("dictionaryName");
		}
		m_stream.Write("/");
		m_stream.Write(dictionaryName);
		m_stream.Write(" ");
		WriteOperator("gs");
	}

	public void SetColorSpace(string name, bool forStroking)
	{
		SetColorSpace(new PdfName(name), forStroking);
	}

	public void SetColorSpace(PdfName name, bool forStroking)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		string text = (forStroking ? "CS" : "cs");
		m_stream.Write(name.ToString());
		m_stream.Write(" ");
		m_stream.Write(text);
		m_stream.Write("\r\n");
	}

	public void SetColorAndSpace(PdfColor color, PdfColorSpace colorSpace, bool forStroking)
	{
		if (!color.IsEmpty)
		{
			m_stream.Write(color.ToString(colorSpace, forStroking));
			m_stream.Write("\r\n");
		}
	}

	public void SetColorAndSpace(PdfColor color, PdfColorSpace colorSpace, bool forStroking, bool check)
	{
		if (!color.IsEmpty)
		{
			m_stream.Write(color.CalToString(colorSpace, forStroking));
			m_stream.Write("\r\n");
		}
	}

	public void SetColorAndSpace(PdfColor color, PdfColorSpace colorSpace, bool forStroking, bool check, bool iccbased)
	{
		if (!color.IsEmpty)
		{
			m_stream.Write(color.IccColorToString(colorSpace, forStroking));
			m_stream.Write("\r\n");
		}
	}

	public void SetColorAndSpace(PdfColor color, PdfColorSpace colorSpace, bool forStroking, bool check, bool iccbased, bool indexed)
	{
		if (!color.IsEmpty)
		{
			m_stream.Write(color.IndexedToString(forStroking));
			m_stream.Write("\r\n");
		}
	}

	public void SetColor(PdfColor color, PdfColorSpace currentSpace, bool forStroking)
	{
		string opcode = (forStroking ? "SC" : "sc");
		switch (currentSpace)
		{
		case PdfColorSpace.CMYK:
			m_stream.Write(PdfNumber.FloatToString(color.C));
			m_stream.Write(" ");
			m_stream.Write(PdfNumber.FloatToString(color.M));
			m_stream.Write(" ");
			m_stream.Write(PdfNumber.FloatToString(color.Y));
			m_stream.Write(" ");
			m_stream.Write(PdfNumber.FloatToString(color.K));
			m_stream.Write(" ");
			break;
		case PdfColorSpace.GrayScale:
			m_stream.Write(PdfNumber.FloatToString(color.Gray));
			break;
		case PdfColorSpace.RGB:
			m_stream.Write(PdfNumber.FloatToString(color.Red));
			m_stream.Write(" ");
			m_stream.Write(PdfNumber.FloatToString(color.Green));
			m_stream.Write(" ");
			m_stream.Write(PdfNumber.FloatToString(color.Blue));
			m_stream.Write(" ");
			break;
		default:
			throw new ArgumentException("Unknown current color space");
		}
		WriteOperator(opcode);
	}

	public void SetColourWithPattern(IList colours, PdfName patternName, bool forStroking)
	{
		if (colours != null)
		{
			int i = 0;
			for (int count = colours.Count; i < count; i++)
			{
				m_stream.Write(colours[i].ToString());
				m_stream.Write(" ");
			}
		}
		if (patternName != null)
		{
			m_stream.Write(patternName.ToString());
			m_stream.Write(" ");
		}
		if (forStroking)
		{
			WriteOperator("SCN");
		}
		else
		{
			WriteOperator("scn");
		}
	}

	public void ExecuteObject(string name)
	{
		ExecuteObject(new PdfName(name));
	}

	public void ExecuteObject(PdfName name)
	{
		m_stream.Write(name.ToString());
		m_stream.Write(" ");
		WriteOperator("Do");
	}

	internal PdfStream GetStream()
	{
		return m_stream;
	}

	internal void Clear()
	{
		m_stream.Clear();
	}

	private void WritePoint(PointF point)
	{
		WritePoint(point.X, point.Y);
	}

	private void WritePoint(float x, float y)
	{
		m_stream.Write(PdfNumber.FloatToString(x));
		m_stream.Write(" ");
		y = PdfGraphics.UpdateY(y);
		m_stream.Write(PdfNumber.FloatToString(y));
		m_stream.Write(" ");
	}

	private void WriteText(object text)
	{
		if (text is PdfString)
		{
			WriteText(text as PdfString);
			return;
		}
		if (text is string)
		{
			WriteText(text as string);
			return;
		}
		if (text is byte[])
		{
			WriteText(text as byte[]);
			return;
		}
		throw new ArgumentException("Unknown text format", "text");
	}

	private void WriteText(byte[] text, bool hex)
	{
		char symbol;
		char symbol2;
		if (hex)
		{
			symbol = "<>"[0];
			symbol2 = "<>"[1];
		}
		else
		{
			symbol = "()"[0];
			symbol2 = "()"[1];
		}
		m_stream.Write(symbol);
		if (hex)
		{
			m_stream.Write(PdfString.BytesToHex(text));
		}
		else
		{
			m_stream.Write(text);
		}
		m_stream.Write(symbol2);
	}

	private void WriteText(string text, bool hex)
	{
		char symbol;
		char symbol2;
		if (hex)
		{
			symbol = "<>"[0];
			symbol2 = "<>"[1];
		}
		else
		{
			symbol = "()"[0];
			symbol2 = "()"[1];
		}
		m_stream.Write(symbol);
		m_stream.Write(text);
		m_stream.Write(symbol2);
	}

	private void WriteText(PdfString text)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		byte[] data = text.PdfEncode(null);
		m_stream.Write(data);
	}

	private void WriteOperator(string opcode)
	{
		m_stream.Write(opcode);
		m_stream.Write("\r\n");
	}

	private void CheckTextParam(byte[] text)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
	}

	private void CheckTextParam(string text)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		if (text == string.Empty)
		{
			throw new ArgumentException("The text can't be an empty string", "text");
		}
	}

	private void CheckTextParam(PdfString text)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
	}

	public void Write(IPdfPrimitive pdfObject)
	{
		pdfObject.Save(this);
	}

	public void Write(long number)
	{
		m_stream.Write(number.ToString());
	}

	public void Write(float number)
	{
		m_stream.Write(PdfNumber.FloatToString(number));
	}

	public void Write(string text)
	{
		m_stream.Write(text);
	}

	public void Write(char[] text)
	{
		m_stream.Write(new string(text));
	}

	public void Write(byte[] data)
	{
		m_stream.Write(data);
	}
}
