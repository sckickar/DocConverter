namespace DocGen.Office;

internal abstract class DocumentSerializer
{
	internal abstract void SerializeRunCharacterFormat(IOfficeMathRunElement paraItem);

	internal abstract void SerializeControlProperties(IOfficeRunFormat mathControlFormat);

	internal abstract void SerializeMathRun(IOfficeMathRunElement officeMathParaItem);
}
