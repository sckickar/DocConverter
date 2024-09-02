namespace DocGen.DocIO.DLS;

public interface IStyleHolder
{
	string StyleName { get; }

	void ApplyStyle(string styleName);

	void ApplyStyle(BuiltinStyle builtinStyle);
}
