using System.Collections.Generic;
using System.Globalization;

namespace DocGen.PdfViewer.Base;

internal class FontInterpreter
{
	private readonly Dictionary<string, BaseType1Font> fonts;

	private readonly PostScriptDict systemDict;

	internal OperandCollector Operands { get; private set; }

	internal Stack<PostScriptDict> DictionaryStack { get; private set; }

	internal Stack<PostScriptArray> ArrayStack { get; private set; }

	internal PostScriptDict CurrentDictionary => DictionaryStack.Peek();

	internal PostScriptArray CurrentArray => ArrayStack.Peek();

	internal PostScriptParser Reader { get; private set; }

	internal PostScriptDict SystemDict => systemDict;

	internal PostScriptArray RD { get; set; }

	internal PostScriptArray ND { get; set; }

	internal PostScriptArray NP { get; set; }

	public Dictionary<string, BaseType1Font> Fonts => fonts;

	private static object ParseOperand(Token token, PostScriptParser reader)
	{
		return token switch
		{
			Token.Operator => PostScriptOperators.FindOperator(reader.Result), 
			Token.Integer => ParseInt(reader.Result), 
			Token.Real => ParseReal(reader.Result), 
			Token.Name => reader.Result, 
			Token.ArrayStart => ParseArray(reader), 
			Token.Keyword => PdfKeywords.GetValue(reader.Result), 
			Token.String => new PostScriptStrHelper(reader.Result), 
			Token.Boolean => ParseBool(reader.Result), 
			Token.DictionaryStart => ParseDictionary(reader), 
			_ => null, 
		};
	}

	private static object ParseBool(string b)
	{
		return b == "true";
	}

	private static int ParseInt(string str)
	{
		return int.Parse(str, CultureInfo.InvariantCulture.NumberFormat);
	}

	private static double ParseReal(string str)
	{
		return double.Parse(str, CultureInfo.InvariantCulture.NumberFormat);
	}

	private static PostScriptArray ParseArray(PostScriptParser reader)
	{
		PostScriptArray postScriptArray = new PostScriptArray();
		Token token;
		while (!reader.EndOfFile && (token = reader.ReadToken()) != Token.ArrayEnd)
		{
			object obj = ParseOperand(token, reader);
			if (token != 0 || obj != null)
			{
				postScriptArray.Add(obj);
			}
		}
		return postScriptArray;
	}

	private static PostScriptDict ParseDictionary(PostScriptParser reader)
	{
		PostScriptDict postScriptDict = new PostScriptDict();
		Token token;
		while (!reader.EndOfFile && (token = reader.ReadToken()) != Token.ArrayEnd)
		{
			Token token2 = token;
			if (token2 == Token.Name || token2 == Token.String)
			{
				string result = reader.Result;
				postScriptDict[result] = ParseOperand(reader.ReadToken(), reader);
			}
			else
			{
				reader.ReadToken();
			}
		}
		return postScriptDict;
	}

	public FontInterpreter()
	{
		fonts = new Dictionary<string, BaseType1Font>();
		systemDict = new PostScriptDict();
		InitializeSystemDict();
	}

	public void Execute(byte[] data)
	{
		Operands = new OperandCollector();
		DictionaryStack = new Stack<PostScriptDict>();
		ArrayStack = new Stack<PostScriptArray>();
		Reader = new PostScriptParser(data);
		while (!Reader.EndOfFile)
		{
			Token token = Reader.ReadToken();
			switch (token)
			{
			case Token.Operator:
				PostScriptOperators.FindOperator(Reader.Result).Execute(this);
				break;
			default:
				Operands.AddLast(ParseOperand(token, Reader));
				break;
			case Token.Unknown:
				break;
			}
		}
	}

	internal void ExecuteProcedure(PostScriptArray proc)
	{
		if (proc == null)
		{
			return;
		}
		foreach (object item in proc)
		{
			if (item is PostScriptOperators postScriptOperators)
			{
				postScriptOperators.Execute(this);
			}
			else
			{
				Operands.AddLast(item);
			}
		}
	}

	private void InitializeSystemDict()
	{
		systemDict["FontDirectory"] = new PostScriptDict();
	}
}
