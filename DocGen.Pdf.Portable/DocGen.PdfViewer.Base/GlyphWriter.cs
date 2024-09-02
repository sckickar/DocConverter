using System.Collections.Generic;
using System.IO;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;

namespace DocGen.PdfViewer.Base;

internal class GlyphWriter
{
	private MemoryStream glyphStream;

	private static string charSet = "0123456789.ee -";

	public bool is1C;

	private String[] charForGlyphIndex;

	private int max = 100;

	private double[] operands = new double[100];

	private int operandReached;

	private float[] pt;

	private double xs = -1.0;

	private double ys = -1.0;

	private double x;

	private double y;

	private int ptCount;

	private int currentOp;

	private int hintCount;

	private bool allowAll;

	private double h;

	private PointF CurrentLocation;

	private PdfUnitConvertor m_convertor = new PdfUnitConvertor();

	internal Dictionary<string, byte[]> glyphs = new Dictionary<string, byte[]>();

	internal Dictionary<int, string> UnicodeCharMapTable;

	internal double[] FontMatrix = new double[6] { 0.001, 0.0, 0.0, 0.001, 0.0, 0.0 };

	internal bool HasBaseEncoding;

	public int GlobalBias;

	public GlyphWriter(Dictionary<string, byte[]> glyphCharSet, bool isC1)
	{
		glyphs = glyphCharSet;
		is1C = isC1;
	}

	public GlyphWriter(Dictionary<string, byte[]> glyphCharSet, int bias, bool isC1)
	{
		glyphs = glyphCharSet;
		is1C = isC1;
		GlobalBias = bias;
	}
}
