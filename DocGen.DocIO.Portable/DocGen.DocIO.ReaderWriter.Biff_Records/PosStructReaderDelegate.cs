using System.IO;

namespace DocGen.DocIO.ReaderWriter.Biff_Records;

internal delegate void PosStructReaderDelegate(BinaryReader reader, int pos, int nextPos);
