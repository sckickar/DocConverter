using System;

namespace DocGen.Pdf.Interactive;

[Obsolete("Please use PdfLoadedAnnotationType instead")]
public enum PdfLoadedAnnotationTypes
{
	Highlight,
	Underline,
	RedactionAnnotation,
	StrikeOut,
	Squiggly,
	AnnotationStates,
	TextAnnotation,
	LinkAnnotation,
	DocumentLinkAnnotation,
	FileLinkAnnotation,
	FreeTextAnnotation,
	LineAnnotation,
	CircleAnnotation,
	EllipseAnnotation,
	SquareAnnotation,
	RectangleAnnotation,
	PolygonAnnotation,
	PolyLineAnnotation,
	SquareandCircleAnnotation,
	PolygonandPolylineAnnotation,
	TextMarkupAnnotation,
	CaretAnnotation,
	RubberStampAnnotation,
	LnkAnnotation,
	PopupAnnotation,
	FileAttachmentAnnotation,
	SoundAnnotation,
	MovieAnnotation,
	ScreenAnnotation,
	WidgetAnnotation,
	PrinterMarkAnnotation,
	TrapNetworkAnnotation,
	WatermarkAnnotation,
	TextWebLinkAnnotation,
	InkAnnotation,
	Null
}
