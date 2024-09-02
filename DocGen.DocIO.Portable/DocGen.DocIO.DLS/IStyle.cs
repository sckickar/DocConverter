namespace DocGen.DocIO.DLS;

public interface IStyle
{
	string Name { get; set; }

	StyleType StyleType { get; }

	void Remove();

	IStyle Clone();

	void Close();
}
