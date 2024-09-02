namespace DocGen.Styles;

internal interface IStyleInfoSubObject
{
	StyleInfoProperty Sip { get; }

	StyleInfoBase Owner { get; }

	object Data { get; }

	IStyleInfoSubObject MakeCopy(StyleInfoBase newOwner, StyleInfoProperty sip);
}
