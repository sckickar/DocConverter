namespace DocGen.DocIO.DLS;

public interface IWTextBoxCollection
{
	IWTextBox this[int index] { get; }

	int Add(IWTextBox textBox);
}
