namespace DocGen.PdfViewer.Base;

internal class DefineFont : PostScriptOperators
{
	public override void Execute(FontInterpreter interpreter)
	{
		PostScriptDict lastAs = interpreter.Operands.GetLastAs<PostScriptDict>();
		string lastAs2 = interpreter.Operands.GetLastAs<string>();
		BaseType1Font baseType1Font = new BaseType1Font();
		baseType1Font.Load(lastAs);
		interpreter.Fonts[lastAs2] = baseType1Font;
		interpreter.Operands.AddLast(baseType1Font);
	}
}
