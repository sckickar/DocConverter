using DocGen.Compression.Zip;

namespace DocGen.OfficeChart.Implementation.XmlSerialization;

internal class AddSlashPreprocessor : IFileNamePreprocessor
{
	public string PreprocessName(string fullName)
	{
		if (fullName != null && fullName.Length > 0 && fullName[0] != '/')
		{
			fullName = "/" + fullName;
		}
		return fullName;
	}
}
