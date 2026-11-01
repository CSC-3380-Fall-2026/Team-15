using Godot;

public partial class RoomTemplate : Node2D
{
	[Export] public int Width { get; set; } = 20;
	[Export] public int Height { get; set; } = 16;

		// Obstacle tile positions (in tiles, not pixels). Edit per room in the Inspector.
	[Export] public Godot.Collections.Array<Vector2I> Obstacles { get; set; } = new()
	{
		new Vector2I(4, 4), new Vector2I(5, 4), new Vector2I(14, 11),
		new Vector2I(15, 11), new Vector2I(10, 4)
	};

	// Positions in placeholder_tiles.png (source 0)
	private static readonly Vector2I WallTile = new(0, 0);
	private static readonly Vector2I FloorTile = new(1, 0);
	private static readonly Vector2I ObstacleTile = new(2, 0);

	public override void _Ready()
	{
		var tiles = GetNode<TileMapLayer>("Tiles");
		tiles.Clear();

		for (int x = 0; x < Width; x++)
		for (int y = 0; y < Height; y++)
		{
			bool isEdge = x == 0 || y == 0 || x == Width - 1 || y == Height - 1;
			tiles.SetCell(new Vector2I(x, y), 0, isEdge ? WallTile : FloorTile);
		}

		foreach (var cell in Obstacles)
			tiles.SetCell(cell, 0, ObstacleTile);
	}
}
