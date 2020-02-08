using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Sandbox
{
    public class Level
    {
        public readonly Vector2Int dimensions;
        private Tile[,] tileGrid;
        public List<Actor> actors;

        public Level(MapData mapData, Dictionary<int, TerrainData> terrainData, float elevationStep, int elevationStart)
        {
            this.dimensions = new Vector2Int(mapData.width, mapData.height);
            actors = new List<Actor>();
            tileGrid = new Tile[dimensions.x, dimensions.y];

            // Get map layers
            MapData.Layer terrainLayer = mapData.GetLayer("Terrain");
            MapData.Layer elevationLayer = mapData.GetLayer("Elevation");
            MapData.Layer temperatureLayer = mapData.GetLayer("Temperature");
            int i = 0;
            for (int y = 0; y < dimensions.y; y++)
            {
                for (int x = 0; x < dimensions.x; x++)
                {


                    int terrainIndex = terrainLayer.data[i];
                    float elevation = 0;
                    int temperature = 0;

                    // Try to get elevation
                    if (elevationLayer != null && elevationLayer.data[i] != 0)
                    {
                        elevation = (elevationLayer.data[i] - elevationStart) * elevationStep;
                    }
                
                    // Try to get temperature
                    if (temperatureLayer != null)
                    {
                        temperature = temperatureLayer.data[i];
                    }

                    TerrainData terrain = terrainData[0];
                    if (terrainData.ContainsKey(terrainIndex))
                    {
                        terrain = terrainData[terrainIndex];
                    }

                    Tile newTile = new Tile(this, new Vector2Int(x,y), terrain, elevation, temperature);

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
