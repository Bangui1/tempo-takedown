using UnityEngine;
using System.Collections.Generic;

public class SimpleMazeGenerator : MonoBehaviour
{
	[Header("Spawn Area")]
	public GameObject wallPrefab;
	public int gridWidth = 20;
	public int gridHeight = 20;
	public float cellSize = 1.0f;
	public Vector2 gridOffset = Vector2.zero;

	[Header("Structure Settings")]
	public int structureCount = 15;
	public int minLineLength = 4;
	public int maxLineLength = 10;
	public int minBlockWidth = 2;
	public int maxBlockWidth = 5;
	public int minBlockHeight = 2;
	public int maxBlockHeight = 4;
	[Range(0f, 1f)] public float singleWeight = 0.1f;
	[Range(0f, 1f)] public float lineWeight = 0.6f;
	[Range(0f, 1f)] public float cornerWeight = 0.2f;
	[Range(0f, 1f)] public float blockWeight = 0.1f;
	public int maxPlacementAttemptsPerStructure = 60;
	
	[Header("Spacing & Alignment")]
	public float minStructureDistance = 2.5f;
	public bool snapToGrid = true;
	public float gridSnapSize = 1.0f;

	[Header("Collision")]
	public LayerMask wallLayerMask;
	public LayerMask ampLayerMask;
	public float clearance = 0.1f;

	[Header("Lifecycle")]
	public bool generateOnStart = true;
	public bool clearExisting = true;

	private readonly List<GameObject> spawned = new List<GameObject>();
	private readonly List<Vector2> placedStructureCenters = new List<Vector2>();

	void Start()
	{
		if (generateOnStart)
		{
			Generate();
		}
	}

	public void Generate()
	{
		if (clearExisting)
		{
			Clear();
		}
		SpawnStructures();
	}

	void SpawnStructures()
	{
		for (int i = 0; i < structureCount; i++)
		{
			bool placed = TryPlaceRandomStructure();
			if (!placed) { continue; }
		}
	}

	bool TryPlaceRandomStructure()
	{
		float roll = Random.value;
		float s = singleWeight;
		float l = s + lineWeight;
		float c = l + cornerWeight;
		int attempts = 0;

		while (attempts++ < maxPlacementAttemptsPerStructure)
		{
			Vector2Int origin = new Vector2Int(
				Random.Range(1, gridWidth - 1),
				Random.Range(1, gridHeight - 1)
			);

			// Snap to grid if enabled
			if (snapToGrid)
			{
				origin.x = Mathf.RoundToInt(origin.x / gridSnapSize) * Mathf.RoundToInt(gridSnapSize);
				origin.y = Mathf.RoundToInt(origin.y / gridSnapSize) * Mathf.RoundToInt(gridSnapSize);
				origin.x = Mathf.Clamp(origin.x, 1, gridWidth - 1);
				origin.y = Mathf.Clamp(origin.y, 1, gridHeight - 1);
			}

			List<Vector2Int> cells;

			if (roll < s)
			{
				cells = BuildSingle(origin);
			}
			else if (roll < l)
			{
				cells = BuildLine(origin);
			}
			else if (roll < c)
			{
				cells = BuildCorner(origin);
			}
			else
			{
				cells = BuildBlock(origin);
			}

			if (CellsAreFree(cells) && HasMinimumDistance(cells))
			{
				PlaceCells(cells);
				// Record the center of this structure for distance checking
				Vector2 structureCenter = GetStructureCenter(cells);
				placedStructureCenters.Add(structureCenter);
				return true;
			}
		}
		return false;
	}

	List<Vector2Int> BuildSingle(Vector2Int o)
	{
		return new List<Vector2Int> { o };
	}

	List<Vector2Int> BuildLine(Vector2Int o)
	{
		bool horizontal = Random.value < 0.5f;
		int len = Random.Range(minLineLength, maxLineLength + 1);
		List<Vector2Int> cells = new List<Vector2Int>();
		
		// Add some variation - sometimes create thicker lines
		bool isThick = Random.value < 0.3f && len > 4;
		int thickness = isThick ? 2 : 1;
		
		for (int i = 0; i < len; i++)
		{
			for (int t = 0; t < thickness; t++)
			{
				int x = o.x + (horizontal ? i : t);
				int y = o.y + (horizontal ? t : i);
				cells.Add(new Vector2Int(x, y));
			}
		}
		return cells;
	}

	List<Vector2Int> BuildCorner(Vector2Int o)
	{
		bool horizontalFirst = Random.value < 0.5f;
		int lenA = Random.Range(3, Mathf.Max(3, maxLineLength));
		int lenB = Random.Range(3, Mathf.Max(3, maxLineLength));
		List<Vector2Int> cells = new List<Vector2Int>();
		
		// Build first segment
		for (int i = 0; i < lenA; i++)
		{
			cells.Add(new Vector2Int(o.x + (horizontalFirst ? i : 0), o.y + (horizontalFirst ? 0 : i)));
		}
		
		// Build second segment from the end of first segment
		Vector2Int elbow = cells[cells.Count - 1];
		for (int j = 1; j < lenB; j++)
		{
			cells.Add(new Vector2Int(elbow.x + (horizontalFirst ? 0 : j), elbow.y + (horizontalFirst ? j : 0)));
		}
		
		// Sometimes add a third segment for more complex shapes
		if (Random.value < 0.4f && lenA > 3 && lenB > 3)
		{
			Vector2Int secondElbow = cells[cells.Count - 1];
			int lenC = Random.Range(2, 4);
			for (int k = 1; k < lenC; k++)
			{
				cells.Add(new Vector2Int(secondElbow.x + (horizontalFirst ? k : 0), secondElbow.y + (horizontalFirst ? 0 : k)));
			}
		}
		
		return cells;
	}

	List<Vector2Int> BuildBlock(Vector2Int o)
	{
		int w = Random.Range(minBlockWidth, maxBlockWidth + 1);
		int h = Random.Range(minBlockHeight, maxBlockHeight + 1);
		List<Vector2Int> cells = new List<Vector2Int>();
		
		// Sometimes create hollow blocks (walls only on perimeter)
		bool isHollow = Random.value < 0.3f && w > 3 && h > 3;
		
		for (int x = 0; x < w; x++)
		{
			for (int y = 0; y < h; y++)
			{
				if (!isHollow || x == 0 || x == w - 1 || y == 0 || y == h - 1)
				{
					cells.Add(new Vector2Int(o.x + x, o.y + y));
				}
			}
		}
		return cells;
	}

	bool HasMinimumDistance(List<Vector2Int> cells)
	{
		Vector2 structureCenter = GetStructureCenter(cells);
		
		foreach (Vector2 existingCenter in placedStructureCenters)
		{
			float distance = Vector2.Distance(structureCenter, existingCenter);
			if (distance < minStructureDistance)
			{
				return false;
			}
		}
		return true;
	}

	Vector2 GetStructureCenter(List<Vector2Int> cells)
	{
		if (cells.Count == 0) return Vector2.zero;
		
		Vector2 sum = Vector2.zero;
		foreach (Vector2Int cell in cells)
		{
			sum += new Vector2(cell.x * cellSize + gridOffset.x, cell.y * cellSize + gridOffset.y);
		}
		return sum / cells.Count;
	}

	bool CellsAreFree(List<Vector2Int> cells)
	{
		int mask = wallLayerMask.value | ampLayerMask.value;
		if (mask == 0) { mask = ~0; }
		Vector2 size = new Vector2(cellSize - clearance, cellSize - clearance);
		foreach (Vector2Int c in cells)
		{
			if (c.x < 1 || c.x >= gridWidth - 1 || c.y < 1 || c.y >= gridHeight - 1) { return false; }
			Vector2 world = new Vector2(c.x * cellSize + gridOffset.x, c.y * cellSize + gridOffset.y);
			Collider2D hit = Physics2D.OverlapBox(world, size, 0f, mask);
			if (hit != null) { return false; }
		}
		return true;
	}

	void PlaceCells(List<Vector2Int> cells)
	{
		foreach (Vector2Int c in cells)
		{
			Vector3 pos = new Vector3(c.x * cellSize + gridOffset.x, c.y * cellSize + gridOffset.y, 0f);
			GameObject w = Instantiate(wallPrefab, pos, Quaternion.identity);
			w.name = $"Amp_{c.x}_{c.y}";
			spawned.Add(w);
		}
	}

	public void Clear()
	{
		foreach (GameObject w in spawned)
		{
			if (w != null) { DestroyImmediate(w); }
		}
		spawned.Clear();
		placedStructureCenters.Clear();
	}

	public bool IsWall(Vector2 worldPos)
	{
		int mask = wallLayerMask.value | ampLayerMask.value;
		if (mask == 0) { mask = ~0; }
		Collider2D hit = Physics2D.OverlapBox(worldPos, new Vector2(cellSize - clearance, cellSize - clearance), 0f, mask);
		return hit != null;
	}

	public Vector2 GetRandomEmptyPosition()
	{
		int tries = 0;
		int maxTries = 200;
		while (tries++ < maxTries)
		{
			Vector2 world = new Vector2(
				(Random.Range(1, gridWidth - 1)) * cellSize + gridOffset.x,
				(Random.Range(1, gridHeight - 1)) * cellSize + gridOffset.y
			);
			if (!IsWall(world)) { return world; }
		}
		return Vector2.zero;
	}
}
