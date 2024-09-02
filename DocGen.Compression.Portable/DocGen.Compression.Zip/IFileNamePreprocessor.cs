namespace DocGen.Compression.Zip;

public interface IFileNamePreprocessor
{
	string PreprocessName(string fullName);
}
