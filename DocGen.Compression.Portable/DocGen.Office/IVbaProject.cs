namespace DocGen.Office;

public interface IVbaProject
{
	string Name { get; set; }

	string Description { get; set; }

	string Constants { get; set; }

	string HelpFile { get; set; }

	uint HelpContextId { get; set; }

	IVbaModules Modules { get; }
}
