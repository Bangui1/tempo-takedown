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
	public int structureCount = 20;
	public int minLineLength = 3;
	public int maxLineLength = 8;
	public int minBlockWidth = 2;
	public int maxBlockWidth = 4;
	public int minBlockHeight = 2;
	public int maxBlockHeight = 3;
	[Range(0f, 1f)] public float singleWeight = 0.15f;
	[Range(0f, 1f)] public float lineWeight = 0.55f;
	[Range(0f, 1f)] public float cornerWeight = 0.15f;
	[Range(0f, 1f)] public float blockWeight = 0.15f;
	public int maxPlacementAttemptsPerStructure = 40;

	[Header("Collision")]
	public LayerMask wallLayerMask;
	public LayerMask ampLayerMask;
	public float clearance = 0.1f;

	[Header("Lifecycle")]
	public bool generateOnStart = true;
	public bool clearExisting = true;

	private readonly List<GameObject> spawned = new List<GameObject>();

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

			if (CellsAreFree(cells))
			{
				PlaceCells(cells);
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
		for (int i = 0; i < len; i++)
		{
			int x = o.x + (horizontal ? i : 0);
			int y = o.y + (horizontal ? 0 : i);
			cells.Add(new Vector2Int(x, y));
		}
		return cells;
	}

	List<Vector2Int> BuildCorner(Vector2Int o)
	{
		bool horizontalFirst = Random.value < 0.5f;
		int lenA = Random.Range(2, Mathf.Max(2, maxLineLength));
		int lenB = Random.Range(2, Mathf.Max(2, maxLineLength));
		List<Vector2Int> cells = new List<Vector2Int>();
		for (int i = 0; i < lenA; i++)
		{
			cells.Add(new Vector2Int(o.x + (horizontalFirst ? i : 0), o.y + (horizontalFirst ? 0 : i)));
		}
		Vector2Int elbow = cells[cells.Count - 1];
		for (int j = 1; j < lenB; j++)
		{
			cells.Add(new Vector2Int(elbow.x + (horizontalFirst ? 0 : j), elbow.y + (horizontalFirst ? j : 0)));
		}
		return cells;
	}

	List<Vector2Int> BuildBlock(Vector2Int o)
	{
		int w = Random.Range(minBlockWidth, maxBlockWidth + 1);
		int h = Random.Range(minBlockHeight, maxBlockHeight + 1);
		List<Vector2Int> cells = new List<Vector2Int>();
		for (int x = 0; x < w; x++)
		{
			for (int y = 0; y < h; y++)
			{
				cells.Add(new Vector2Int(o.x + x, o.y + y));
			}
		}
		return cells;
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
