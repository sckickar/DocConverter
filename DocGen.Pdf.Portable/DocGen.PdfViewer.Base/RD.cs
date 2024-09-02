namespace DocGen.PdfViewer.Base;

internal class RD : PostScriptOperators
{
	public static bool IsRDOperator(string name)
	{
		if (!(name == "RD"))
		{
			return name == "-|";
		}
		return true;
	}

	public override void Execute(FontInterpreter interpreter)
	{
		interpreter.Reader.Read();
		int lastAsInt = interpreter.Operands.GetLastAsInt();
		CharStringEncryption charStringEncryption = new CharStringEncryption(interpreter.Reader);
		interpreter.Reader.PushEncryption(charStringEncryption);
		charStringEncryption.Initialize();
		interpreter.Operands.AddLast(lastAsInt - charStringEncryption.RandomBytesCount);
		interpreter.ExecuteProcedure(interpreter.RD);
		interpreter.Reader.PopEncryption();
	}
}
