namespace Esprima;

public readonly struct OctalValue
{
	public readonly int Code;

	public readonly bool Octal;

	public OctalValue(int code, bool octal)
	{
		Code = code;
		Octal = octal;
	}
}
