namespace DocGen.Drawing.DocIOHelper;

internal interface IGraphicsPath
{
	int PointCount { get; }

	PointF[] PathPoints { get; }

	RectangleF GetBounds();

	void AddRectangle(Rectangle rectangle);

	void AddCurve(PointF[] curvePoints);

	void AddBeziers(PointF[] points);

	void AddBezier(PointF point1, PointF point2, PointF point3, PointF point4);

	void AddLine(PointF pointF1, PointF pointF2);

	void AddLine(float pointX1, float pointY1, float pointX2, float pointY2);

	void AddLines(PointF[] linePoints);

	void AddArc(float x, float y, float width, float height, float startAngle, float sweepAngle);

	void AddArc(RectangleF rect, float startAngle, float sweepAngle);

	void AddPie(float x, float y, float width, float height, float startAngle, float sweepAngle);

	void AddEllipse(float x, float y, float width, float height);

	void AddEllipse(Rectangle rect);

	void AddPolygon(PointF[] points);

	void AddString(string s, string familyName, int style, float emSize, PointF origin, StringFormat format);

	void StartFigure();

	void CloseFigure();

	void CloseAllFigures();

	void Reset();

	void Transform(Matrix matrix);

	void Dispose();
}
