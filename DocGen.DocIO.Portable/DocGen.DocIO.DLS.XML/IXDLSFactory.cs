namespace DocGen.DocIO.DLS.XML;

public interface IXDLSFactory
{
	IXDLSSerializable Create(IXDLSContentReader reader);
}
