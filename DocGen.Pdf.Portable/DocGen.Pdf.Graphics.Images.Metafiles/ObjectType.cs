namespace DocGen.Pdf.Graphics.Images.Metafiles;

internal enum ObjectType
{
	Invalid = 0,
	Brush = 256,
	Pen = 512,
	Path = 768,
	Region = 1024,
	Image = 1280,
	Font = 1536,
	StringFormat = 1792,
	ImageAttributes = 2048,
	CustomLineCap = 2304
}
