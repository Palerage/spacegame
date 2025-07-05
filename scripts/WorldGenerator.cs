using Godot;
using System;
using System.Collections.Generic;

public partial class WorldGenerator : Node2D
{
    [Export]
    public Texture2D TileTexture { get; set; }

    [Export]
    public int GridWidth { get; set; } = 16; // OBS: måste vara jämnt!

    [Export]
    public int GridHeight { get; set; } = 15;

    [Export]
    public float TileSize { get; set; } = 16f;

    [Export(PropertyHint.Range, "0,1,0.01")]
    public float SpawnThreshold { get; set; } = 0.5f;

    [Export]
    public float ScrollSpeed { get; set; } = 100f;

    [Export]
    public int NoiseSeed { get; set; } = 0;

    [Export(PropertyHint.Range, "1,8")]
    public int FractalOctaves { get; set; } = 2;

    [Export(PropertyHint.Range, "0.001,0.1")]
    public float Frequency { get; set; } = 0.02f;

    [Export(PropertyHint.Range, "0,1")]
    public float FractalGain { get; set; } = 0.5f;

    private FastNoiseLite baseNoise;
    private List<Node2D> chunks = new List<Node2D>();
    private float screenWidth;
    private int lastFreePassageStartY;
    private Random rand = new Random();

    public override void _Ready()
    {
        if (GridWidth % 2 != 0)
        {
            GD.PrintErr($"GridWidth var udda ({GridWidth}), ökar till jämnt värde.");
            GridWidth += 1;
        }

        if (NoiseSeed == 0)
            NoiseSeed = (int)GD.RandRange(1, 1000000);

        baseNoise = new FastNoiseLite
        {
            Seed = NoiseSeed,
            NoiseType = FastNoiseLite.NoiseTypeEnum.Simplex,
            FractalOctaves = FractalOctaves,
            Frequency = Frequency,
            FractalGain = FractalGain
        };

        screenWidth = GetViewportRect().Size.X;

        lastFreePassageStartY = rand.Next(0, GridHeight - 2);

        SpawnInitialChunk();
    }

    private void SpawnInitialChunk()
    {
        var firstChunk = CreateChunk(lastFreePassageStartY, 0);
        firstChunk.Position = new Vector2(screenWidth, 0);
        chunks.Add(firstChunk);
    }

    /// <summary>
    /// Skapar en chunk med säker passage. Varierar seed och tröskel för mindre repetitivitet.
    /// </summary>
    /// <param name="startFreePassageY">Start-y för fri passage i första paret</param>
    /// <param name="worldStartX">Global start x-koordinat i tile-grid</param>
    private Node2D CreateChunk(int startFreePassageY, int worldStartX)
    {
        var chunk = new Node2D();
        AddChild(chunk);

        int pairsCount = GridWidth / 2;
        int[] freePassageStarts = new int[pairsCount];

        // Variera start med liten slump för fri passage
        freePassageStarts[0] = Mathf.Clamp(startFreePassageY + rand.Next(-1, 2), 0, GridHeight - 2);

        // Generera fri passage per par med max +/-1 steg från föregående, för överlappning
        for (int i = 1; i < pairsCount; i++)
        {
            int prev = freePassageStarts[i - 1];
            int minY = Math.Max(0, prev - 1);
            int maxY = Math.Min(GridHeight - 2, prev + 1);
            freePassageStarts[i] = rand.Next(minY, maxY + 1);
        }

        // Variera tröskel lite per chunk för att minska repetitivitet
        float chunkSpawnThreshold = SpawnThreshold + (float)(rand.NextDouble() * 0.1 - 0.05); // ±0.05 variation
        chunkSpawnThreshold = Mathf.Clamp(chunkSpawnThreshold, 0f, 1f);

        // Skapa tiles - hoppa över fria passageområden (2 kolumner breda, 2 rader höga per par)
        for (int y = 0; y < GridHeight; y++)
        {
            for (int x = 0; x < GridWidth; x++)
            {
                int pairIndex = x / 2;
                int freeYStart = freePassageStarts[pairIndex];

                bool inFreePassage = (y == freeYStart || y == freeYStart + 1);

                if (inFreePassage)
                    continue;

                // Använd baseNoise med offset i seed för chunk och global position för variation
                float noiseX = worldStartX + x + NoiseSeed * 0.1f;
                float noiseY = y + NoiseSeed * 0.1f;

                float noiseValue = (baseNoise.GetNoise2D(noiseX, noiseY) + 1f) * 0.5f;

                if (noiseValue > chunkSpawnThreshold)
                {
                    SpawnTileAt(x, y, chunk);
                }
            }
        }

        lastFreePassageStartY = freePassageStarts[pairsCount - 1];

        return chunk;
    }

    private void SpawnTileAt(int gridX, int gridY, Node2D parent)
    {
        var sprite = new Sprite2D
        {
            Texture = TileTexture,
            Centered = false,
            Position = new Vector2(gridX * TileSize, gridY * TileSize)
        };

        parent.AddChild(sprite);
    }

    public override void _Process(double delta)
    {
        if (chunks.Count == 0)
            return;

        for (int i = chunks.Count - 1; i >= 0; i--)
        {
            var chunk = chunks[i];
            chunk.Position += new Vector2(-ScrollSpeed * (float)delta, 0);

            if (chunk.Position.X < -GridWidth * TileSize)
            {
                chunk.QueueFree();
                chunks.RemoveAt(i);
                continue;
            }
        }

        var lastChunk = chunks[chunks.Count - 1];
        if (lastChunk.Position.X < screenWidth - 10 * TileSize)
        {
            int lastChunkWorldX = (int)(lastChunk.Position.X / TileSize);

            var newChunk = CreateChunk(lastFreePassageStartY, lastChunkWorldX + GridWidth);
            newChunk.Position = new Vector2(lastChunk.Position.X + GridWidth * TileSize, 0);
            chunks.Add(newChunk);
        }
    }
}
