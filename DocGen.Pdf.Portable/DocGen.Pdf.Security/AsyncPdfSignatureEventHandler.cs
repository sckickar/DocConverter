using System.Threading.Tasks;

namespace DocGen.Pdf.Security;

internal delegate Task AsyncPdfSignatureEventHandler(object sender, PdfSignatureEventArgs ars);
