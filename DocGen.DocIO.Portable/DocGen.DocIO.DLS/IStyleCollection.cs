using System.Collections;

namespace DocGen.DocIO.DLS;

public interface IStyleCollection : ICollectionBase, IEnumerable
{
	IStyle this[int index] { get; }

	bool FixedIndex13HasStyle { get; set; }

	bool FixedIndex14HasStyle { get; set; }

	string FixedIndex13StyleName { get; set; }

	string FixedIndex14StyleName { get; set; }

	int Add(IStyle style);

	IStyle FindByName(string name);

	IStyle FindByName(string name, StyleType styleType);

	IStyle FindById(int styleId);
}
