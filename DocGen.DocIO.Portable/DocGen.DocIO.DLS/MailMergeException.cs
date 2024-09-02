using System;

namespace DocGen.DocIO.DLS;

public class MailMergeException : Exception
{
	private const string DEF_MESSAGE = "Incorrect syntax of mail merge fields";

	public MailMergeException()
		: base("Incorrect syntax of mail merge fields")
	{
	}

	public MailMergeException(Exception innerExc)
		: this("Incorrect syntax of mail merge fields", innerExc)
	{
	}

	public MailMergeException(string message)
		: base(message)
	{
	}

	public MailMergeException(string message, Exception innerExc)
		: base(message, innerExc)
	{
	}
}
