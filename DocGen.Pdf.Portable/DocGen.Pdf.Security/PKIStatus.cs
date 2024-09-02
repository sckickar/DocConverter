using System;

namespace DocGen.Pdf.Security;

[Flags]
internal enum PKIStatus
{
	Granted = 0,
	GrantedWithMods = 1,
	Rejection = 2,
	Waiting = 3,
	RevocationWarning = 4,
	RevocationNotification = 5
}
