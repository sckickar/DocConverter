namespace DocGen.Pdf.Security;

public enum KeyStorageFlags
{
	DefaultKeySet = 0,
	UserKeySet = 1,
	MachineKeySet = 2,
	Exportable = 4,
	UserProtected = 8,
	PersistKeySet = 0x10
}
