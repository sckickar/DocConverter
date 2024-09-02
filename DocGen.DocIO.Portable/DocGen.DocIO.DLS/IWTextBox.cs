namespace DocGen.DocIO.DLS;

public interface IWTextBox : IParagraphItem, IEntity, ICompositeEntity
{
	string Name { get; set; }

	bool Visible { get; set; }

	new EntityCollection ChildEntities { get; }

	WTextBody TextBoxBody { get; }

	WTextBoxFormat TextBoxFormat { get; set; }
}
