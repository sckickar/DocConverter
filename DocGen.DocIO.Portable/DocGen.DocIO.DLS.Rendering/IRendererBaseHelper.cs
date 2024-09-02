using System.Collections.Generic;
using System.IO;
using DocGen.Office;

namespace DocGen.DocIO.DLS.Rendering;

internal interface IRendererBaseHelper
{
	Font GetFallbackFont(Font font, string text, FontScriptType scriptType, WCharacterFormat charFormat, List<FallbackFont> fallbackFonts, Dictionary<string, Stream> fontStreams);
}
