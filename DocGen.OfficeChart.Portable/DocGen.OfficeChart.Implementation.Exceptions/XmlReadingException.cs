using System;

namespace DocGen.OfficeChart.Implementation.Exceptions;

internal class XmlReadingException : ApplicationException
{
	private const string DEF_ERROR = "Some problem occured during parse.";

	public XmlReadingException()
		: base("Some problem occured during parse.")
	{
	}

	public XmlReadingException(string message)
		: base("Some problem occured during parse.. Error message: " + message)
	{
	}

	public XmlReadingException(string strBlock, string strDescription)
		: base("Exception occured in " + strBlock + " of xml structure. Error message: " + strDescription)
	{
	}
}
