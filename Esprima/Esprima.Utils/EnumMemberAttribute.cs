using System;

namespace Esprima.Utils;

[AttributeUsage(AttributeTargets.Field)]
internal sealed class EnumMemberAttribute : Attribute
{
	private string _value;

	public string Value
	{
		get
		{
			return _value;
		}
		set
		{
			_value = value;
			IsValueSetExplicitly = true;
		}
	}

	public bool IsValueSetExplicitly { get; set; }
}
