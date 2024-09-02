using System;

namespace DocGen.OfficeChart.Parser.Biff_Records.Formula;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
internal sealed class ErrorCodeAttribute : Attribute
{
	private string m_StringValue = string.Empty;

	private int m_ErrorCode;

	public string StringValue => m_StringValue;

	public int ErrorCode => m_ErrorCode;

	private ErrorCodeAttribute()
	{
	}

	public ErrorCodeAttribute(string stringValue, int errorCode)
	{
		m_StringValue = stringValue;
		m_ErrorCode = errorCode;
	}
}
