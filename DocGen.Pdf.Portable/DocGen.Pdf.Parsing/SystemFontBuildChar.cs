using System.Collections.Generic;
using DocGen.PdfViewer.Base;

namespace DocGen.Pdf.Parsing;

internal class SystemFontBuildChar
{
	private static readonly Dictionary<SystemFontOperatorDescriptor, SystemFontOperator> operators;

	private static SystemFontOperatorDescriptor endChar;

	private static SystemFontHintOperator vStem;

	private readonly ISystemFontBuildCharHolder buildCharHolder;

	private SystemFontOperandsCollection postScriptStack;

	private SystemFontOperandsCollection operands;

	internal SystemFontOperandsCollection Operands => operands;

	internal SystemFontOperandsCollection PostScriptStack => postScriptStack;

	internal SystemFontPathFigure CurrentPathFigure { get; set; }

	internal SystemFontGlyphOutlinesCollection GlyphOutlines { get; private set; }

	internal Point CurrentPoint { get; set; }

	internal Point BottomLeft { get; set; }

	internal int? Width { get; set; }

	internal int Hints { get; set; }

	internal static Point CalculatePoint(SystemFontBuildChar interpreter, int dx, int dy)
	{
		interpreter.CurrentPoint = new Point(interpreter.CurrentPoint.X + (double)dx, interpreter.CurrentPoint.Y + (double)dy);
		return new Point(interpreter.CurrentPoint.X, 0.0 - interpreter.CurrentPoint.Y);
	}

	private static void InitializePathConstructionOperators()
	{
		operators[new SystemFontOperatorDescriptor(1)] = new SystemFontHintOperator();
		vStem = new SystemFontHintOperator();
		operators[new SystemFontOperatorDescriptor(3)] = vStem;
		operators[new SystemFontOperatorDescriptor(4)] = new SystemFontVMoveTo();
		operators[new SystemFontOperatorDescriptor(5)] = new SystemFontRLineTo();
		operators[new SystemFontOperatorDescriptor(6)] = new SystemFontHLineTo();
		operators[new SystemFontOperatorDescriptor(7)] = new SystemFontVLineTo();
		operators[new SystemFontOperatorDescriptor(8)] = new SystemFontRRCurveTo();
		endChar = new SystemFontOperatorDescriptor(14);
		operators[endChar] = new SystemFontEndChar();
		operators[new SystemFontOperatorDescriptor(18)] = new SystemFontHintOperator();
		operators[new SystemFontOperatorDescriptor(19)] = new SystemFontHintMaskOperator();
		operators[new SystemFontOperatorDescriptor(20)] = new SystemFontHintMaskOperator();
		operators[new SystemFontOperatorDescriptor(21)] = new SystemFontRMoveTo();
		operators[new SystemFontOperatorDescriptor(22)] = new SystemFontHMoveTo();
		operators[new SystemFontOperatorDescriptor(23)] = new SystemFontHintOperator();
		operators[new SystemFontOperatorDescriptor(24)] = new SystemFontRCurveLine();
		operators[new SystemFontOperatorDescriptor(25)] = new SystemFontRLineCurve();
		operators[new SystemFontOperatorDescriptor(26)] = new SystemFontVVCurveTo();
		operators[new SystemFontOperatorDescriptor(27)] = new SystemFontHHCurveTo();
		operators[new SystemFontOperatorDescriptor(30)] = new SystemFontVHCurveTo();
		operators[new SystemFontOperatorDescriptor(31)] = new SystemFontHVCurveTo();
		operators[new SystemFontOperatorDescriptor(11)] = new SystemFontReturn();
		operators[new SystemFontOperatorDescriptor(10)] = new SystemFontCallSubr();
		operators[new SystemFontOperatorDescriptor(9)] = new SystemFontClosePath();
		operators[new SystemFontOperatorDescriptor(13)] = new SystemFontHsbw();
		operators[new SystemFontOperatorDescriptor(29)] = new SystemFontCallGSubr();
		operators[new SystemFontOperatorDescriptor(SystemFontHelper.CreateByteArray(12, 12))] = new SystemFontDiv();
		operators[new SystemFontOperatorDescriptor(SystemFontHelper.CreateByteArray(12, 35))] = new SystemFontFlex();
		operators[new SystemFontOperatorDescriptor(SystemFontHelper.CreateByteArray(12, 37))] = new SystemFontFlex1();
		operators[new SystemFontOperatorDescriptor(SystemFontHelper.CreateByteArray(12, 34))] = new SystemFontHFlex();
		operators[new SystemFontOperatorDescriptor(SystemFontHelper.CreateByteArray(12, 36))] = new SystemFontHFlex1();
		operators[new SystemFontOperatorDescriptor(SystemFontHelper.CreateByteArray(12, 16))] = new SystemFontCallOtherSubr();
		operators[new SystemFontOperatorDescriptor(SystemFontHelper.CreateByteArray(12, 17))] = new SystemFontPop();
		operators[new SystemFontOperatorDescriptor(SystemFontHelper.CreateByteArray(12, 6))] = new SystemFontSeac();
		operators[new SystemFontOperatorDescriptor(SystemFontHelper.CreateByteArray(12, 7))] = new SystemFontSbw();
		operators[new SystemFontOperatorDescriptor(SystemFontHelper.CreateByteArray(12, 33))] = new SystemFontSetCurrentPoint();
	}

	private static bool IsOperator(byte b)
	{
		if (b != 28 && 0 <= b)
		{
			return b <= 31;
		}
		return false;
	}

	private static bool IsTwoByteOperator(byte b)
	{
		return b == 12;
	}

	static SystemFontBuildChar()
	{
		operators = new Dictionary<SystemFontOperatorDescriptor, SystemFontOperator>();
		InitializePathConstructionOperators();
	}

	public SystemFontBuildChar(ISystemFontBuildCharHolder subrsHodler)
	{
		buildCharHolder = subrsHodler;
	}

	public void ExecuteSubr(int index)
	{
		byte[] subr = buildCharHolder.GetSubr(index);
		ExecuteInternal(subr);
	}

	public void ExecuteGlobalSubr(int index)
	{
		byte[] globalSubr = buildCharHolder.GetGlobalSubr(index);
		ExecuteInternal(globalSubr);
	}

	public void CombineChars(string accentedChar, string baseChar, int dx, int dy)
	{
		SystemFontGlyphOutlinesCollection systemFontGlyphOutlinesCollection = buildCharHolder.GetGlyphData(accentedChar).Oultlines.Clone();
		SystemFontGlyphOutlinesCollection collection = buildCharHolder.GetGlyphData(baseChar).Oultlines.Clone();
		GlyphOutlines.AddRange(collection);
		systemFontGlyphOutlinesCollection.Transform(new SystemFontMatrix(1.0, 0.0, 0.0, 1.0, dx, dy));
		GlyphOutlines.AddRange(systemFontGlyphOutlinesCollection);
	}

	public void Execute(byte[] data)
	{
		postScriptStack = new SystemFontOperandsCollection();
		operands = new SystemFontOperandsCollection();
		GlyphOutlines = new SystemFontGlyphOutlinesCollection();
		CurrentPoint = default(Point);
		Width = null;
		Hints = 0;
		ExecuteInternal(data);
	}

	private void ExecuteInternal(byte[] data)
	{
		SystemFontEncodedDataReader systemFontEncodedDataReader = new SystemFontEncodedDataReader(data, SystemFontByteEncoding.CharStringByteEncodings);
		while (!systemFontEncodedDataReader.EndOfFile)
		{
			byte b = systemFontEncodedDataReader.Peek(0);
			if (IsOperator(b))
			{
				SystemFontOperatorDescriptor systemFontOperatorDescriptor = ((!IsTwoByteOperator(b)) ? new SystemFontOperatorDescriptor(systemFontEncodedDataReader.Read()) : new SystemFontOperatorDescriptor(SystemFontHelper.CreateByteArray(systemFontEncodedDataReader.Read(), systemFontEncodedDataReader.Read())));
				ExecuteOperator(systemFontOperatorDescriptor, systemFontEncodedDataReader);
				if (systemFontOperatorDescriptor.Equals(endChar))
				{
					break;
				}
			}
			else
			{
				Operands.AddLast(systemFontEncodedDataReader.ReadOperand());
			}
		}
	}

	private int GetMaskSize()
	{
		int num = Hints / 8;
		if (Hints % 8 != 0)
		{
			num++;
		}
		return num;
	}

	private void ExecuteOperator(SystemFontOperatorDescriptor descr, SystemFontEncodedDataReader reader)
	{
		if (!operators.TryGetValue(descr, out SystemFontOperator value))
		{
			Operands.Clear();
		}
		else if (value is SystemFontHintMaskOperator)
		{
			vStem.Execute(this, out var count);
			Hints += count;
			byte[] array = new byte[GetMaskSize()];
			reader.Read(array, array.Length);
			((SystemFontHintMaskOperator)value).Execute(this, array);
		}
		else if (value is SystemFontHintOperator)
		{
			((SystemFontHintOperator)value).Execute(this, out var count2);
			Hints += count2;
		}
		else
		{
			value.Execute(this);
		}
	}
}
