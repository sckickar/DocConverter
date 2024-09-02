namespace DocGen.Office;

public interface IOfficeRun
{
	IOfficeMathRunElement OwnerMathRunElement { get; set; }

	IOfficeRun CloneRun();

	void Dispose();
}
