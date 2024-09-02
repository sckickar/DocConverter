namespace Esprima;

public class ParserOptions
{
	public bool Range { get; set; }

	public bool Loc { get; set; }

	public bool Tokens { get; set; }

	public bool Comment { get; set; }

	public bool Tolerant { get; set; } = true;


	public SourceType SourceType { get; set; } = SourceType.Script;


	public IErrorHandler ErrorHandler { get; set; }

	public bool AdaptRegexp { get; set; }

	public ParserOptions()
		: this(new ErrorHandler())
	{
	}

	public ParserOptions(string source)
		: this(new ErrorHandler
		{
			Source = source
		})
	{
	}

	public ParserOptions(IErrorHandler errorHandler)
	{
		ErrorHandler = errorHandler;
	}
}
