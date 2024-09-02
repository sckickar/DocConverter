namespace DocGen.Pdf;

internal interface ImageData
{
	int TileWidth { get; }

	int TileHeight { get; }

	int NomTileWidth { get; }

	int NomTileHeight { get; }

	int ImgWidth { get; }

	int ImgHeight { get; }

	int NumComps { get; }

	int TileIdx { get; }

	int TilePartULX { get; }

	int TilePartULY { get; }

	int ImgULX { get; }

	int ImgULY { get; }

	int getCompSubsX(int c);

	int getCompSubsY(int c);

	int getTileComponentWidth(int t, int c);

	int getTileComponentHeight(int t, int c);

	int getCompImgWidth(int c);

	int getCompImgHeight(int c);

	int getNomRangeBits(int c);

	void setTile(int x, int y);

	void nextTile();

	JPXImageCoordinates getTile(JPXImageCoordinates co);

	int getCompUpperLeftCornerX(int c);

	int getCompUpperLeftCornerY(int c);

	JPXImageCoordinates getNumTiles(JPXImageCoordinates co);

	int getNumTiles();
}
