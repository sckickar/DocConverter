using System.Collections.Generic;
using System.IO;
using DocGen.DocIO.DLS;
using DocGen.Drawing;
using DocGen.Drawing.DocIOHelper;
using DocGen.Office;
using DocGen.OfficeChart;

namespace DocGen.DocIO.Drawing;

internal interface IHelper
{
	void CreateFont(Stream stream, string fontName, float fontSize, FontStyle fontStyle);

	IFontFamily GetFontFamily(string name);

	IFontFamily GetFontFamily(string name, float fontSize);

	Matrix MakeRotationDegrees(float angle, float x, float y);

	Dictionary<string, float> ParseShapeFormula(DocGen.DocIO.DLS.AutoShapeType autoShapeType, Dictionary<string, string> shapeGuide, RectangleF bounds);

	float GetDescent(DocGen.Drawing.Font font, FontScriptType scriptType);

	float GetAscent(DocGen.Drawing.Font font, FontScriptType scriptType);

	float GetLineSpacing(DocGen.Drawing.Font font);

	float GetEmHeight(DocGen.Drawing.Font font);

	float GetFontHeight(DocGen.Drawing.Font font, FontScriptType scriptType);

	Stream GetFontStream(DocGen.Drawing.Font font, FontScriptType scriptType);

	string GetFontName(string fontName, float fontSize, FontStyle fontStyle, FontScriptType scriptType);

	string GetFontName(string fontName, float fontSize, FontStyle fontStyle, FontScriptType scriptType, ref bool hasStylesAndWeights);

	string GetUnicodeFamilyName(string text, string fontName);

	IBitmap GetBitmap(int width, int height);

	IGraphics GetGraphics(IImage image);

	ISolidBrush GetSolidBrush(Color color);

	ITextureBrush GetTextureBrush(IImage image, RectangleF rect, IImageAttributes imageAttributes);

	IHatchBrush GetHatchBrush(HatchStyle hatchstyle, Color foreColor, Color backColor);

	IGraphicsPath GetGraphicsPath();

	IBitmap GetBitmap();

	IImageAttributes GetImageAttributes();

	IImage CreateImageFromStream(MemoryStream stream);

	byte[] ConvertTiffToPng(byte[] imageBytes);

	bool HasBitmap(IImage image);

	IColorMatrix GetColorMatrix();

	IColorMatrix GetColorMatrix(float[][] newColorMatrix);

	IPen GetPen(Color color);

	IPen GetPen(Color color, float width);

	void ApplyScale(Matrix matrix, float x, float y);

	void MultiplyMatrix(Matrix srcMatrix, Matrix matrix, MatrixOrder order);

	void TranslateMatrix(Matrix matrix, float offsetX, float offsetY, MatrixOrder order);

	void RotateMatrix(Matrix matrix, float angle, PointF point, MatrixOrder order);

	void RotateMatrix(Matrix matrix, float angle);

	IFontExtension GetFontExtension(string fontName, float fontSize, FontStyle fontStyle, GraphicsUnit graphicsUnit, FontScriptType scriptType);

	bool IsValidFontStream(Stream fontStream);

	DocGen.Drawing.Font GetFallbackFont(DocGen.Drawing.Font font, string text, FontScriptType scriptType, List<FallbackFont> fallbackFonts, Dictionary<string, Stream> fontStreams);

	DocGen.Drawing.Font GetRegularStyleFontToMeasure(DocGen.Drawing.Font font, string text, FontScriptType scriptType);

	void ConvertChartAsImage(IOfficeChart officeChart, Stream imageAsStream, ChartRenderingOptions imageOptions);
}
