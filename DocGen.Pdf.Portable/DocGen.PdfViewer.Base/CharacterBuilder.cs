using System.Collections.Generic;

namespace DocGen.PdfViewer.Base;

internal class CharacterBuilder
{
	private static readonly Dictionary<OperatorDescriptor, Operator> operators;

	private static OperatorDescriptor endChar;

	private static HintOperator vStem;

	private readonly IBuildCharacterOwner buildCharHolder;

	private OperandCollector postScriptStack;

	private OperandCollector operands;

	internal OperandCollector Operands => operands;

	internal OperandCollector PostScriptStack => postScriptStack;

	internal PathFigure CurrentPathFigure { get; set; }

	internal GlyphOutlinesCollection GlyphOutlines { get; private set; }

	internal Point CurrentPoint { get; set; }

	internal Point BottomLeft { get; set; }

	internal int? Width { get; set; }

	internal int Hints { get; set; }

	internal static Point CalculatePoint(CharacterBuilder interpreter, int dx, int dy)
	{
		interpreter.CurrentPoint = new Point(interpreter.CurrentPoint.X + (double)dx, interpreter.CurrentPoint.Y + (double)dy);
		return new Point(interpreter.CurrentPoint.X, 0.0 - interpreter.CurrentPoint.Y);
	}

	private static void InitializePathConstructionOperators()
	{
		operators[new OperatorDescriptor(1)] = new HintOperator();
		vStem = new HintOperator();
		operators[new OperatorDescriptor(3)] = vStem;
		operators[new OperatorDescriptor(4)] = new VMoveTo();
		operators[new OperatorDescriptor(5)] = new RLineTo();
		operators[new OperatorDescriptor(6)] = new HLineTo();
		operators[new OperatorDescriptor(7)] = new VLineTo();
		operators[new OperatorDescriptor(8)] = new RRCurveTo();
		endChar = new OperatorDescriptor(14);
		operators[endChar] = new EndChar();
		operators[new OperatorDescriptor(18)] = new HintOperator();
		operators[new OperatorDescriptor(19)] = new HintMaskOperator();
		operators[new OperatorDescriptor(20)] = new HintMaskOperator();
		operators[new OperatorDescriptor(21)] = new RMoveTo();
		operators[new OperatorDescriptor(22)] = new HMoveTo();
		operators[new OperatorDescriptor(23)] = new HintOperator();
		operators[new OperatorDescriptor(24)] = new RCurveLine();
		operators[new OperatorDescriptor(25)] = new RLineCurve();
		operators[new OperatorDescriptor(26)] = new VVCurveTo();
		operators[new OperatorDescriptor(27)] = new HHCurveTo();
		operators[new OperatorDescriptor(30)] = new VHCurveTo();
		operators[new OperatorDescriptor(31)] = new HVCurveTo();
		operators[new OperatorDescriptor(11)] = new Return();
		operators[new OperatorDescriptor(10)] = new CallSubr();
		operators[new OperatorDescriptor(9)] = new ClosePath();
		operators[new OperatorDescriptor(13)] = new Hsbw();
		operators[new OperatorDescriptor(29)] = new CallGSubr();
		operators[new OperatorDescriptor(Helper.CreateByteArray(12, 12))] = new Div();
		operators[new OperatorDescriptor(Helper.CreateByteArray(12, 35))] = new Flex();
		operators[new OperatorDescriptor(Helper.CreateByteArray(12, 37))] = new Flex1();
		operators[new OperatorDescriptor(Helper.CreateByteArray(12, 34))] = new HFlex();
		operators[new OperatorDescriptor(Helper.CreateByteArray(12, 36))] = new HFlex1();
		operators[new OperatorDescriptor(Helper.CreateByteArray(12, 16))] = new CallOtherSubr();
		operators[new OperatorDescriptor(Helper.CreateByteArray(12, 17))] = new Pop();
		operators[new OperatorDescriptor(Helper.CreateByteArray(12, 6))] = new Seac();
		operators[new OperatorDescriptor(Helper.CreateByteArray(12, 7))] = new Sbw();
		operators[new OperatorDescriptor(Helper.CreateByteArray(12, 33))] = new SetCurrentPoint();
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

	static CharacterBuilder()
	{
		operators = new Dictionary<OperatorDescriptor, Operator>();
		InitializePathConstructionOperators();
	}

	public CharacterBuilder(IBuildCharacterOwner subrsHodler)
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
		GlyphOutlinesCollection glyphOutlinesCollection = buildCharHolder.GetGlyphData(accentedChar).Oultlines.Clone();
		GlyphOutlinesCollection collection = buildCharHolder.GetGlyphData(baseChar).Oultlines.Clone();
		GlyphOutlines.AddRange(collection);
		glyphOutlinesCollection.Transform(new Matrix(1.0, 0.0, 0.0, 1.0, dx, dy));
		GlyphOutlines.AddRange(glyphOutlinesCollection);
	}

	public void Execute(byte[] data)
	{
		postScriptStack = new OperandCollector();
		operands = new OperandCollector();
		GlyphOutlines = new GlyphOutlinesCollection();
		CurrentPoint = default(Point);
		Width = null;
		Hints = 0;
		ExecuteInternal(data);
	}

	private void ExecuteInternal(byte[] data)
	{
		EncodedDataParser encodedDataParser = new EncodedDataParser(data, ByteEncodingBase.CharStringByteEncodings);
		while (!encodedDataParser.EndOfFile)
		{
			byte b = encodedDataParser.Peek(0);
			if (IsOperator(b))
			{
				OperatorDescriptor operatorDescriptor = ((!IsTwoByteOperator(b)) ? new OperatorDescriptor(encodedDataParser.Read()) : new OperatorDescriptor(Helper.CreateByteArray(encodedDataParser.Read(), encodedDataParser.Read())));
				ExecuteOperator(operatorDescriptor, encodedDataParser);
				if (operatorDescriptor.Equals(endChar))
				{
					break;
				}
			}
			else
			{
				Operands.AddLast(encodedDataParser.ReadOperand());
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

	private void ExecuteOperator(OperatorDescriptor descr, EncodedDataParser reader)
	{
		if (!operators.TryGetValue(descr, out Operator value))
		{
			Operands.Clear();
		}
		else if (value is HintMaskOperator)
		{
			vStem.Execute(this, out var count);
			Hints += count;
			byte[] array = new byte[GetMaskSize()];
			reader.Read(array, array.Length);
			((HintMaskOperator)value).Execute(this, array);
		}
		else if (value is HintOperator)
		{
			((HintOperator)value).Execute(this, out var count2);
			Hints += count2;
		}
		else
		{
			value.Execute(this);
		}
	}
}
