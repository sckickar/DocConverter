using System.Xml;

namespace DocGen.Office;

internal abstract class DocumentParser
{
	internal abstract IOfficeRunFormat ParseMathControlFormat(XmlReader reader, IOfficeMathFunctionBase mathFunction);

	internal abstract void ParseMathRun(XmlReader reader, IOfficeMathRunElement mathParaItem);
}
