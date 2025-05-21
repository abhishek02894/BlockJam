using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Tag
{
    public class LevelDataReader : MonoBehaviour
    {
        [SerializeField] private TextAsset levelDataAsset;
        [SerializeField] private string levelDataPath = "Editor/CsvFile/LevelData.csv";

        private List<string[]> rawLevelData;
        public void LoadLevelData()
        {
            if (levelDataAsset != null)
            {
                rawLevelData = CSVReader.ReadRawData(levelDataAsset);
                //Debug.Log($"Loaded level data from TextAsset with {rawLevelData.Count} rows");
            }
            else
            {
                rawLevelData = CSVReader.ReadRawData(levelDataPath);
                //Debug.Log($"Loaded level data from path with {rawLevelData.Count} rows");
            }

            // Log sample of loaded data for verification
            if (rawLevelData != null && rawLevelData.Count > 0)
            {
                //Debug.Log("First row headers: " + string.Join(", ", rawLevelData[0]));
                if (rawLevelData.Count > 1)
                {
                    //Debug.Log("Second row data: " + string.Join(", ", rawLevelData[1]));
                }
            }
        }

        // Get cell at specific row and column
        public string GetCell(int row, int col)
        {
            if (rawLevelData == null || row < 0 || row >= rawLevelData.Count)
                return string.Empty;

            string[] rowData = rawLevelData[row];
            if (col < 0 || col >= rowData.Length)
                return string.Empty;

            return rowData[col];
        }

        // Get all data for a specific level
        private List<string[]> GetLevelData(int levelNumber)
        {
            if (rawLevelData == null || rawLevelData.Count < 2)
                return null;

            List<string[]> levelRows = new List<string[]>();
            bool inLevel = false;

            // Start from row 1 (skip header)
            for (int i = 1; i < rawLevelData.Count; i++)
            {
                string[] row = rawLevelData[i];
                if (row.Length == 0) continue;

                // Check if this is a row that starts a level section
                if (row[0] != string.Empty && int.TryParse(row[0], out int currentLevel))
                {
                    if (currentLevel == levelNumber)
                    {
                        inLevel = true;
                        levelRows.Add(row);
                    }
                    else if (inLevel)
                    {
                        // We've moved past our target level
                        break;
                    }
                }
                else if (inLevel)
                {
                    // Still in the same level section
                    levelRows.Add(row);
                }
            }

            return levelRows;
        }

        // Get a specific level row as a 2D grid 
        private string[,] GetLevelGrid(int levelNumber)
        {
            List<string[]> levelData = GetLevelData(levelNumber);
            if (levelData == null || levelData.Count == 0)
                return null;

            // Find the maximum column count in this level
            int maxCols = 0;
            foreach (string[] row in levelData)
            {
                maxCols = Mathf.Max(maxCols, row.Length);
            }

            // Create the 2D grid
            string[,] grid = new string[levelData.Count, maxCols];

            // Fill the grid with data
            for (int r = 0; r < levelData.Count; r++)
            {
                string[] rowData = levelData[r];
                for (int c = 0; c < rowData.Length; c++)
                {
                    grid[r, c] = rowData[c];
                }

                // Fill remaining cells with empty string
                for (int c = rowData.Length; c < maxCols; c++)
                {
                    grid[r, c] = string.Empty;
                }
            }

            return grid;
        }

        // Get a transposed grid (rows and columns swapped)
        [Button]
        public string[,] GetTransposedLevelGrid(int levelNumber)
        {
            LoadLevelData();
            string[,] originalGrid = GetLevelGrid(levelNumber);
            if (originalGrid == null)
                return null;

            int originalRows = originalGrid.GetLength(0);
            int originalCols = originalGrid.GetLength(1);

            // Create lists to store non-empty cell values
            List<List<string>> compactedColumns = new List<List<string>>();
            
            // Process each column of the original grid, starting from column 1 (skip first column with level numbers)
            for (int c = 1; c < originalCols; c++)
            {
                List<string> columnValues = new List<string>();
                bool hasNonEmptyValues = false;
                
                // Extract non-empty values from this column
                for (int r = 0; r < originalRows; r++)
                {
                    string value = originalGrid[r, c];
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        columnValues.Add(value);
                        hasNonEmptyValues = true;
                    }
                }
                
                // Only add columns that have at least one non-empty value
                if (hasNonEmptyValues)
                {
                    compactedColumns.Add(columnValues);
                }
            }
            
            // If no valid columns found, return an empty 1x1 grid
            if (compactedColumns.Count == 0)
            {
                return new string[1, 1] { { string.Empty } };
            }
            
            // Find the maximum height needed for any column
            int maxHeight = 0;
            foreach (var column in compactedColumns)
            {
                maxHeight = Mathf.Max(maxHeight, column.Count);
            }
            
            // Create the final compacted and transposed grid
            string[,] transposedGrid = new string[compactedColumns.Count, maxHeight];
            
            // Fill the grid with values
            for (int c = 0; c < compactedColumns.Count; c++)
            {
                List<string> columnValues = compactedColumns[c];
                for (int r = 0; r < columnValues.Count; r++)
                {
                    transposedGrid[c, r] = columnValues[r];
                }
                
                // Initialize remaining cells with empty string
                for (int r = columnValues.Count; r < maxHeight; r++)
                {
                    transposedGrid[c, r] = string.Empty;
                }
            }
            
            return transposedGrid;
        }
    }
}