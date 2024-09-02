namespace DocGen.Office;

public interface IVbaModule
{
	string Name { get; set; }

	VbaModuleType Type { get; }

	string Code { get; set; }

	object DesignerStorage { get; set; }
}
