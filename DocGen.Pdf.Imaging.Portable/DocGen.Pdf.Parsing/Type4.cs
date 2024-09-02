using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Parsing;

internal class Type4 : Function
{
	private PdfStream m_stream;

	private string[] m_filter;

	private int m_stackPointer;

	private List<PostScriptOperators> operators;

	private double[] m_stackValue;

	private int[] m_stackType;

	private int m_currentType;

	internal int ResultantValue => base.Range.Count / 2;

	internal string[] Filter
	{
		get
		{
			return m_filter;
		}
		set
		{
			m_filter = value;
		}
	}

	public Type4(PdfDictionary dictionary)
		: base(dictionary)
	{
		m_stream = dictionary as PdfStream;
		SetFilterName(dictionary);
		if (m_stream != null)
		{
			Load(m_stream);
		}
	}

	private void Load(PdfStream stream)
	{
		operators = new List<PostScriptOperators>();
		byte[] decodedStream = GetDecodedStream(stream);
		string @string = Encoding.UTF8.GetString(decodedStream, 0, decodedStream.Length);
		ReadPostScriptOperator(@string, out operators);
	}

	private byte[] GetDecodedStream(PdfStream stream)
	{
		MemoryStream memoryStream = new MemoryStream();
		if (Filter != null)
		{
			for (int i = 0; i < Filter.Length; i++)
			{
				string text = Filter[i];
				if (text == "A85" || text == "ASCII85Decode")
				{
					memoryStream = DecodeASCII85Stream(stream.InternalStream);
					memoryStream.Position = 0L;
					continue;
				}
				try
				{
					memoryStream = DecodeFlateStream(stream.InternalStream);
				}
				catch
				{
				}
			}
			return memoryStream.ToArray();
		}
		return stream.Data;
	}

	protected override double[] PerformFunction(double[] clippedInputValues)
	{
		double[] array = new double[ResultantValue];
		resetStacks(clippedInputValues);
		PerformPostScriptFunction(operators);
		if (base.Domain.Count / 2 == 1)
		{
			int i = 0;
			for (int num = base.Range.Count / 2; i < num; i++)
			{
				array[i] = (float)m_stackValue[i];
			}
		}
		else
		{
			int j = 0;
			for (int num2 = base.Range.Count / 2; j < num2; j++)
			{
				array[j] = (float)m_stackValue[j];
			}
		}
		return array;
	}

	private void SetFilterName(PdfDictionary dictionary)
	{
		if (!dictionary.Items.ContainsKey(new PdfName("Filter")))
		{
			return;
		}
		PdfName pdfName = dictionary.Items[new PdfName("Filter")] as PdfName;
		if (pdfName != null)
		{
			m_filter = new string[1];
			m_filter[0] = pdfName.Value;
		}
		else if (dictionary.Items[new PdfName("Filter")] is PdfArray pdfArray)
		{
			m_filter = new string[pdfArray.Count];
			for (int i = 0; i < pdfArray.Count; i++)
			{
				m_filter[i] = (pdfArray[i] as PdfName).Value;
			}
		}
		else if (dictionary.Items[new PdfName("Filter")] as PdfReferenceHolder != null)
		{
			PdfArray pdfArray2 = (dictionary.Items[new PdfName("Filter")] as PdfReferenceHolder).Object as PdfArray;
			m_filter = new string[pdfArray2.Count];
			for (int j = 0; j < pdfArray2.Count; j++)
			{
				m_filter[j] = (pdfArray2[j] as PdfName).Value;
			}
		}
	}

	private MemoryStream DecodeFlateStream(MemoryStream encodedStream)
	{
		encodedStream.Position = 0L;
		encodedStream.ReadByte();
		encodedStream.ReadByte();
		DeflateStream deflateStream = new DeflateStream(encodedStream, CompressionMode.Decompress, leaveOpen: true);
		byte[] buffer = new byte[4096];
		MemoryStream memoryStream = new MemoryStream();
		while (true)
		{
			int num = deflateStream.Read(buffer, 0, 4096);
			if (num <= 0)
			{
				break;
			}
			memoryStream.Write(buffer, 0, num);
		}
		return memoryStream;
	}

	private MemoryStream DecodeASCII85Stream(MemoryStream encodedStream)
	{
		byte[] array = new ASCII85().decode(encodedStream.ToArray());
		return new MemoryStream(array, 0, array.Length, writable: true, publiclyVisible: true)
		{
			Position = 0L
		};
	}

	private void ReadPostScriptOperator(string code, out List<PostScriptOperators> op)
	{
		List<PostScriptOperators> list = new List<PostScriptOperators>();
		int num = 0;
		string text = "";
		if (code.Contains("\n"))
		{
			code = code.Replace("\n", " ");
		}
		if (code.Contains("  "))
		{
			code = code.Replace("  ", " ");
		}
		if (code.Length > 0 && code[0] == '{' && code[code.Length - 1] == '}')
		{
			code = code.Substring(1, code.Length - 2);
		}
		if (code.Length > 0 && code[0] == ' ')
		{
			code = code.Substring(1, code.Length - 1);
		}
		if (code.Length > 0 && code[code.Length - 1] == ' ')
		{
			code = code.Substring(0, code.Length - 1);
		}
		string[] array = code.Split(' ');
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].Contains("{"))
			{
				num++;
			}
			if (array[i].Contains("}"))
			{
				num--;
				if (num == 0)
				{
					text += array[i];
					list.Add(new PostScriptOperators(PostScriptOperatorTypes.OPERATOR, text));
					text = "";
					continue;
				}
			}
			if (num > 0)
			{
				text += array[i];
				text += " ";
				continue;
			}
			char[] array2 = array[i].ToCharArray();
			try
			{
				switch (array2[0])
				{
				case '+':
				case '-':
				case '0':
				case '1':
				case '2':
				case '3':
				case '4':
				case '5':
				case '6':
				case '7':
				case '8':
				case '9':
					list.Add(new PostScriptOperators(PostScriptOperatorTypes.NUMBER, array[i]));
					break;
				default:
					list.Add(new PostScriptOperators(PostScriptOperatorTypes.OPERATOR, array[i]));
					break;
				}
			}
			catch
			{
			}
		}
		op = list;
	}

	private double Pop()
	{
		m_stackPointer--;
		double result = 0.0;
		if (m_stackPointer >= 0)
		{
			result = m_stackValue[m_stackPointer];
			m_currentType = m_stackType[m_stackPointer];
		}
		return result;
	}

	private void Push(double a, int type)
	{
		if (m_stackPointer <= 99 && m_stackPointer >= 0)
		{
			m_stackValue[m_stackPointer] = a;
			m_stackType[m_stackPointer] = type;
		}
		m_stackPointer++;
	}

	private void Copy(double a)
	{
		int num = (int)a;
		if (m_currentType == 2 && num > 0)
		{
			double[] array = new double[num];
			int[] array2 = new int[num];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = Pop();
				array2[i] = m_currentType;
			}
			for (int num2 = array.Length; num2 > 0; num2--)
			{
				Push(array[num2 - 1], array2[num2 - 1]);
			}
			for (int num3 = array.Length; num3 > 0; num3--)
			{
				Push(array[num3 - 1], array2[num3 - 1]);
			}
		}
	}

	private void Duplicate()
	{
		double a = Pop();
		int currentType = m_currentType;
		Push(a, currentType);
		Push(a, currentType);
	}

	private void Index()
	{
		int num = (int)Pop();
		if (num == 0)
		{
			Duplicate();
		}
		else if (num > 0)
		{
			double[] array = new double[num];
			int[] array2 = new int[num];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = Pop();
				array2[i] = m_currentType;
			}
			double a = Pop();
			int currentType = m_currentType;
			Push(a, currentType);
			for (int num2 = array.Length; num2 > 0; num2--)
			{
				Push(array[num2 - 1], array2[num2 - 1]);
			}
			Push(a, currentType);
		}
	}

	private void Rotate()
	{
		int num = (int)Pop();
		int num2 = (int)Pop();
		_ = 0;
		if (num2 > m_stackPointer)
		{
			num2 = m_stackPointer;
		}
		if (num > 0)
		{
			double[] array = new double[num];
			int[] array2 = new int[num];
			if (num2 - num > 0)
			{
				double[] array3 = new double[num2 - num];
				int[] array4 = new int[num2 - num];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = Pop();
					array2[i] = m_currentType;
				}
				for (int j = 0; j < array3.Length; j++)
				{
					array3[j] = Pop();
					array4[j] = m_currentType;
				}
				for (int num3 = array.Length; num3 > 0; num3--)
				{
					Push(array[num3 - 1], array2[num3 - 1]);
				}
				for (int num4 = array3.Length; num4 > 0; num4--)
				{
					Push(array3[num4 - 1], array4[num4 - 1]);
				}
			}
		}
		else if (num < 0)
		{
			num = -num;
			double[] array5 = new double[num2 - num];
			int[] array6 = new int[num2 - num];
			double[] array7 = new double[num];
			int[] array8 = new int[num];
			for (int k = 0; k < array5.Length; k++)
			{
				array5[k] = Pop();
				array6[k] = m_currentType;
			}
			for (int l = 0; l < array7.Length; l++)
			{
				array7[l] = Pop();
				array8[l] = m_currentType;
			}
			for (int num5 = array5.Length; num5 > 0; num5--)
			{
				Push(array5[num5 - 1], array6[num5 - 1]);
			}
			for (int num6 = array7.Length; num6 > 0; num6--)
			{
				Push(array7[num6 - 1], array8[num6 - 1]);
			}
		}
	}

	private void PerformPostScriptFunction(List<PostScriptOperators> op)
	{
		List<PostScriptOperators> op2 = new List<PostScriptOperators>();
		List<PostScriptOperators> op3 = new List<PostScriptOperators>();
		foreach (PostScriptOperators item in op)
		{
			if (item.Operatortype == PostScriptOperatorTypes.NUMBER)
			{
				float.TryParse(item.Operand, out var result);
				Push(result, 2);
				continue;
			}
			if (item.Operand.Contains("{") && item.Operand.Contains("}"))
			{
				if (op2.Count == 0)
				{
					ReadPostScriptOperator(item.Operand, out op2);
				}
				else if (op3.Count == 0)
				{
					ReadPostScriptOperator(item.Operand, out op3);
				}
			}
			switch (item.Operand)
			{
			case "if":
				if (Pop() > 0.0)
				{
					PerformPostScriptFunction(op2);
				}
				op2.Clear();
				break;
			case "ifelse":
				if (Pop() > 0.0)
				{
					PerformPostScriptFunction(op2);
				}
				else
				{
					PerformPostScriptFunction(op3);
				}
				op2.Clear();
				op3.Clear();
				break;
			case "jz":
			{
				double num = Pop();
				double num2 = Pop();
				break;
			}
			case "j":
			{
				double num2 = Pop();
				break;
			}
			case "abs":
			{
				double num2 = Pop();
				if (num2 < 0.0)
				{
					Push(0.0 - num2, 2);
				}
				else
				{
					Push(num2, 2);
				}
				break;
			}
			case "add":
			{
				double num2 = Pop();
				double num = Pop();
				Push(num2 + num, 2);
				break;
			}
			case "and":
			{
				double num2 = Pop();
				int currentType2 = m_currentType;
				double num = Pop();
				int currentType = m_currentType;
				if (currentType2 == 2 && currentType == 2)
				{
					Push((int)num2 & (int)num, 2);
				}
				else if (currentType2 == 4 && currentType == 4)
				{
					Push((int)num2 & (int)num, 4);
				}
				break;
			}
			case "atan":
			{
				double num2 = Pop();
				Push(Math.Atan(num2), 2);
				break;
			}
			case "bitshift":
			{
				int num3 = (int)Pop();
				int currentType2 = m_currentType;
				int num4 = (int)Pop();
				int currentType = m_currentType;
				if (num3 > 0)
				{
					num3 <<= num4;
				}
				if (num3 < 0)
				{
					num3 >>= -num4;
				}
				Push(num3, 2);
				break;
			}
			case "ceiling":
			{
				double num2 = Pop();
				Push(Math.Ceiling(num2), m_currentType);
				break;
			}
			case "copy":
			{
				double num2 = Pop();
				Copy(num2);
				break;
			}
			case "cos":
			{
				double num2 = Pop();
				Push(Math.Cos(num2), m_currentType);
				break;
			}
			case "cvi":
			{
				double num2 = (int)Pop() | 0;
				Push(num2, m_currentType);
				break;
			}
			case "cvr":
			{
				double num2 = Pop();
				Push(num2, 2);
				break;
			}
			case "div":
			{
				double num = Pop();
				double num2 = Pop();
				Push(num2 / num, 2);
				break;
			}
			case "dup":
				Duplicate();
				break;
			case "eq":
			{
				double num = Pop();
				double num2 = Pop();
				if (num2 == num)
				{
					Push(1.0, 4);
				}
				else
				{
					Push(0.0, 4);
				}
				break;
			}
			case "exch":
			{
				double num2 = Pop();
				int currentType2 = m_currentType;
				double num = Pop();
				int currentType = m_currentType;
				Push(num2, currentType2);
				Push(num, currentType);
				break;
			}
			case "exp":
			{
				double num = Pop();
				double num2 = Pop();
				Push(Math.Pow(num2, num), 2);
				break;
			}
			case "false":
				Push(0.0, 4);
				break;
			case "floor":
			{
				double num2 = Pop();
				Push(Math.Floor(num2), 2);
				break;
			}
			case "ge":
			{
				double num = Pop();
				double num2 = Pop();
				Push((num2 >= num) ? 1 : 0, 4);
				break;
			}
			case "gt":
			{
				double num = Pop();
				double num2 = Pop();
				Push((num2 > num) ? 1 : 0, 4);
				break;
			}
			case "idiv":
			{
				double num = Pop();
				double num2 = Pop();
				Push(num2 / num, 2);
				break;
			}
			case "index":
				Index();
				break;
			case "le":
			{
				double num = Pop();
				double num2 = Pop();
				Push((num2 <= num) ? 1 : 0, 4);
				break;
			}
			case "ln":
			{
				double num2 = Pop();
				Push(Math.Log(num2), 2);
				break;
			}
			case "log":
			{
				double num2 = Pop();
				Push(Math.Log(num2), 2);
				break;
			}
			case "lt":
			{
				double num = Pop();
				double num2 = Pop();
				Push((num2 < num) ? 1 : 0, 4);
				break;
			}
			case "mod":
			{
				double num = Pop();
				double num2 = Pop();
				Push(num2 % num, 2);
				break;
			}
			case "mul":
			{
				double num = Pop();
				double num2 = Pop();
				Push(num2 * num, 2);
				break;
			}
			case "ne":
			{
				double num = Pop();
				double num2 = Pop();
				Push((num2 != num) ? 1 : 0, 4);
				break;
			}
			case "neg":
			{
				double num2 = Pop();
				Push((num2 != 0.0) ? (0.0 - num2) : num2, 2);
				break;
			}
			case "not":
			{
				double num2 = Pop();
				int currentType2 = m_currentType;
				if (num2 == 0.0 && currentType2 == 1)
				{
					Push(1.0, 4);
				}
				else if (num2 == 1.0 && currentType2 == 1)
				{
					Push(0.0, 4);
				}
				else
				{
					Push(~(int)num2, 2);
				}
				break;
			}
			case "or":
			{
				double num = Pop();
				int currentType = m_currentType;
				double num2 = Pop();
				int currentType2 = m_currentType;
				if (currentType2 == 4 && currentType == 4)
				{
					Push((int)num2 | (int)num, 4);
				}
				else if (currentType2 == 2 && currentType == 2)
				{
					Push((int)num2 | (int)num, 2);
				}
				break;
			}
			case "pop":
				Pop();
				break;
			case "roll":
				Rotate();
				break;
			case "round":
			{
				double num2 = Pop();
				Push(Math.Round(num2), 2);
				break;
			}
			case "sin":
			{
				double num2 = Pop();
				Push(Math.Sin(num2), 2);
				break;
			}
			case "sqrt":
			{
				double num2 = Pop();
				Push(Math.Sqrt(num2), 2);
				break;
			}
			case "sub":
			{
				double num = Pop();
				double num2 = Pop();
				Push(num2 - num, 2);
				break;
			}
			case "true":
				Push(1.0, 4);
				break;
			case "truncate":
			{
				double num2 = Pop();
				num2 = ((num2 < 0.0) ? Math.Ceiling(num2) : Math.Floor(num2));
				Push(num2, 2);
				break;
			}
			case "xor":
			{
				double num = Pop();
				int currentType = m_currentType;
				double num2 = Pop();
				int currentType2 = m_currentType;
				if (currentType2 == 4 && currentType == 4)
				{
					Push((int)num2 ^ (int)num, 4);
				}
				else if (currentType2 == 2 && currentType == 2)
				{
					Push((int)num2 ^ (int)num, 2);
				}
				break;
			}
			default:
				if (item.Operatortype == PostScriptOperatorTypes.NUMBER)
				{
					float.TryParse(item.Operand, out var result2);
					Push(result2, 2);
				}
				break;
			}
		}
	}

	public void resetStacks(double[] values)
	{
		m_stackValue = new double[100];
		m_stackType = new int[100];
		m_stackPointer = 0;
		for (int i = 0; i < 100; i++)
		{
			m_stackValue[i] = 0.0;
		}
		for (int j = 0; j < 100; j++)
		{
			m_stackType[j] = 0;
		}
		int num = values.Length;
		for (int k = 0; k < num; k++)
		{
			Push(values[k], 2);
		}
	}
}
