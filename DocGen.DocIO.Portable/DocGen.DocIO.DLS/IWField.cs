namespace DocGen.DocIO.DLS;

public interface IWField : IWTextRange, IParagraphItem, IEntity
{
	TextFormat TextFormat { get; set; }

	FieldType FieldType { get; set; }

	string FieldCode { get; set; }

	void Update();

	void Unlink();
}
