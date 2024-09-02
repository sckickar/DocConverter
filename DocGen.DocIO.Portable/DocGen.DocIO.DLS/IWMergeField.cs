namespace DocGen.DocIO.DLS;

public interface IWMergeField : IWField, IWTextRange, IParagraphItem, IEntity
{
	string FieldName { get; set; }

	string TextBefore { get; set; }

	string TextAfter { get; set; }

	string Prefix { get; }

	string NumberFormat { get; }

	string DateFormat { get; }
}
