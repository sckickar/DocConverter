using System.Collections.Generic;

namespace DocGen.PdfViewer.Base;

internal abstract class PostScriptOperators
{
	internal const string Array = "array";

	internal const string Begin = "begin";

	internal const string ClearToMark = "cleartomark";

	internal const string CloseFile = "closefile";

	internal const string CurrentDict = "currentdict";

	internal const string CurrentFile = "currentfile";

	internal const string Def = "def";

	internal const string DefineFont = "definefont";

	internal const string Dict = "dict";

	internal const string Dup = "dup";

	internal const string EExec = "eexec";

	internal const string End = "end";

	internal const string Exch = "exch";

	internal const string For = "for";

	internal const string Get = "get";

	internal const string Index = "index";

	internal const string Put = "put";

	internal const string RD = "RD";

	internal const string RDAlternate = "-|";

	internal const string Mark = "mark";

	internal const string ND = "ND";

	internal const string NDAlternate = "|-";

	internal const string NoAccess = "noaccess";

	internal const string NP = "NP";

	internal const string NPAlternate = "|";

	internal const string ReadString = "readstring";

	internal const string String = "string";

	internal const string Pop = "pop";

	internal const string Copy = "copy";

	internal const string SystemDict = "systemdict";

	internal const string Known = "known";

	internal const string If = "if";

	internal const string IfElse = "ifelse";

	internal const string FontDirectory = "FontDirectory";

	private static Dictionary<string, PostScriptOperators> operators;

	public static bool IsOperator(string str)
	{
		return operators.ContainsKey(str);
	}

	public static PostScriptOperators FindOperator(string op)
	{
		if (!operators.TryGetValue(op, out PostScriptOperators value))
		{
			return null;
		}
		return value;
	}

	private static void InitializeOperators()
	{
		operators = new Dictionary<string, PostScriptOperators>();
		operators["array"] = new Array();
		operators["begin"] = new Begin();
		operators["cleartomark"] = new ClearToMark();
		operators["closefile"] = new CloseFile();
		operators["currentdict"] = new CurrentDict();
		operators["currentfile"] = new CurrentFile();
		operators["def"] = new Def();
		operators["definefont"] = new DefineFont();
		operators["dict"] = new Dict();
		operators["dup"] = new Dup();
		operators["eexec"] = new EExec();
		operators["end"] = new End();
		operators["exch"] = new Exch();
		operators["for"] = new For();
		operators["get"] = new Get();
		operators["index"] = new Index();
		operators["mark"] = new Mark();
		operators["ND"] = new ND();
		operators["|-"] = new ND();
		operators["noaccess"] = new NoAccess();
		operators["NP"] = new NP();
		operators["|"] = new NP();
		operators["pop"] = new Pops();
		operators["put"] = new Put();
		operators["RD"] = new RD();
		operators["-|"] = new RD();
		operators["readstring"] = new ReadString();
		operators["string"] = new String();
		operators["copy"] = new Copy();
		operators["systemdict"] = new SystemDict();
		operators["known"] = new Known();
		operators["if"] = new If();
		operators["ifelse"] = new IfElse();
		operators["FontDirectory"] = new FontDirectory();
	}

	static PostScriptOperators()
	{
		InitializeOperators();
	}

	public abstract void Execute(FontInterpreter interpreter);
}
