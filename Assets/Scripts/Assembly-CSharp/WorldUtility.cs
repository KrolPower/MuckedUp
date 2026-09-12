using System;

public class WorldUtility
{
	public static TextureData.TerrainType WorldHeightToBiome(float height)
	{
		if (MapGenerator.Instance == null || MapGenerator.Instance.terrainData == null || MapGenerator.Instance.textureData == null)
		{
			return TextureData.TerrainType.Grass;
		}
		float heightMultiplier = MapGenerator.Instance.terrainData.heightMultiplier;
		if (heightMultiplier <= 0f)
		{
			return TextureData.TerrainType.Grass;
		}
		height /= heightMultiplier;
		TextureData.Layer[] layers = MapGenerator.Instance.textureData.layers;
		if (layers == null)
		{
			return TextureData.TerrainType.Grass;
		}
		for (int i = layers.Length - 1; i > 0; i--)
		{
			if (height >= layers[i].startHeight)
			{
				return layers[i].type;
			}
		}
		return TextureData.TerrainType.Water;
	}
}
