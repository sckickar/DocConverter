using System;

namespace DocGen.DocIO;

public class FileCorruptException : Exception
{
	private const string DEF_MESSAGE = "Document is corrupted and impossible to load";
}
