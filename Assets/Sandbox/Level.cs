using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Sandbox
{
    public class Level
    {
        public readonly Vector2Int dimensions;
        private Tile[,] tileGrid;

        public Level(MapData mapData, Dictionary<int, TerrainData> terrainData)
        {
            this.dimensions = new Vector2Int(mapData.width, mapData.height);

            tileGrid = new Tile[dimensions.x, dimensions.y];

            // Get map layers
            MapData.Layer terrainLayer = mapData.GetLayer("Terrain");

            int i = 0;
            for (int x = 0; x < dimensions.x; x++)
            {
                for (int y = 0; y < dimensions.y; y++)
                {


                    int terrainIndex = terrainLayer.data[i];
                    TerrainData terrain = terrainData[0];
                    if (terrainData.ContainsKey(terrainIndex))
                    {
                        terrain = terrainData[terrainIndex];
                    }

                    Tile newTile = new Tile(terrain, 0, 0);

                    tileGrid[x, y] = newTile;

                    i++;
                }
            }
        }

        /// <summary>
        /// Tests if given position vector is within the level dimensions.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public bool PositionInside(Vector2Int position)
        {
            return PositionInside(position.x, position.y);
        }

        /// <summary>
        /// Tests if given x and y coordinates is within the level dimensions.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public bool PositionInside(int x, int y)
        {
            bool insideX = x >= 0 && x < dimensions.x;
            bool insideY = y >= 0 && y < dimensions.y;
            return insideX && insideY;
        }

        /// <summary>
        /// Return tile at given coordinates.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public Tile TileAt(int x, int y)
        {
            if (PositionInside(x, y))
            {
                // Return tile if given position in withing bounds.
                return tileGrid[x, y];
            }
            // Tile is out of bounds.
            return null;
        }
    }
}
