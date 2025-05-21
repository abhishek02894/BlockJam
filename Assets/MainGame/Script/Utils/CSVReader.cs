using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Tag
{
    public class CSVReader
    {
        static string SPLIT_RE = @",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))";
        static string LINE_SPLIT_RE = @"\r\n|\n\r|\n|\r";
        static char[] TRIM_CHARS = { '\"' };

        public static List<Dictionary<string, object>> Read(string filePath)
        {
            var list = new List<Dictionary<string, object>>();
#if UNITY_EDITOR
            TextAsset data = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(filePath);
            if (data == null)
            {
                Debug.LogError($"Failed to load CSV file at path: {filePath}");
                return list;
            }

            var lines = Regex.Split(data.text, LINE_SPLIT_RE);

            if (lines.Length <= 1) return list;

            var header = Regex.Split(lines[0], SPLIT_RE);
            for (var i = 1; i < lines.Length; i++)
            {
                var values = Regex.Split(lines[i], SPLIT_RE);
                if (values.Length == 0 || string.IsNullOrWhiteSpace(values[0])) continue;

                var entry = new Dictionary<string, object>();
                for (var j = 0; j < header.Length && j < values.Length; j++)
                {
                    string value = values[j];
                    value = value.TrimStart(TRIM_CHARS).TrimEnd(TRIM_CHARS).Replace("\\", "");
                    object finalvalue = value;
                    int n;
                    float f;
                    if (int.TryParse(value, out n))
                    {
                        finalvalue = n;
                    }
                    else if (float.TryParse(value, out f))
                    {
                        finalvalue = f;
                    }
                    entry[header[j].Trim()] = finalvalue;
                }
                list.Add(entry);
            }
#endif
            return list;
        }

        public static List<string[]> ReadRawData(string filePath)
        {
            var result = new List<string[]>();
#if UNITY_EDITOR
            TextAsset data = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(filePath);
            if (data == null)
            {
                Debug.LogError($"Failed to load CSV file at path: {filePath}");
                return result;
            }

            var lines = Regex.Split(data.text, LINE_SPLIT_RE);
            
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var values = Regex.Split(line, SPLIT_RE);
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = values[i].TrimStart(TRIM_CHARS).TrimEnd(TRIM_CHARS);
                }
                result.Add(values);
            }
#endif
            return result;
        }

        public List<Dictionary<string, object>> Read(TextAsset file)
        {
            var list = new List<Dictionary<string, object>>();
            TextAsset data = file;
            if (data == null)
            {
                Debug.LogError("Null TextAsset provided to CSVReader");
                return list;
            }

            var lines = Regex.Split(data.text, LINE_SPLIT_RE);

            if (lines.Length <= 1) return list;

            var header = Regex.Split(lines[0], SPLIT_RE);
            for (var i = 1; i < lines.Length; i++)
            {
                var values = Regex.Split(lines[i], SPLIT_RE);
                if (values.Length == 0 || string.IsNullOrWhiteSpace(values[0])) continue;

                var entry = new Dictionary<string, object>();
                for (var j = 0; j < header.Length && j < values.Length; j++)
                {
                    string value = values[j];
                    value = value.TrimStart(TRIM_CHARS).TrimEnd(TRIM_CHARS).Replace("\\", "");
                    object finalvalue = value;
                    int n;
                    float f;
                    if (int.TryParse(value, out n))
                    {
                        finalvalue = n;
                    }
                    else if (float.TryParse(value, out f))
                    {
                        finalvalue = f;
                    }
                    entry[header[j].Trim()] = finalvalue;
                }
                list.Add(entry);
            }
            return list;
        }

        public static List<string[]> ReadRawData(TextAsset file)
        {
            var result = new List<string[]>();
            if (file == null)
            {
                Debug.LogError("Null TextAsset provided to CSVReader");
                return result;
            }

            var lines = Regex.Split(file.text, LINE_SPLIT_RE);
            
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var values = Regex.Split(line, SPLIT_RE);
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = values[i].TrimStart(TRIM_CHARS).TrimEnd(TRIM_CHARS);
                }
                result.Add(values);
            }
            return result;
        }

        public static JObject ConvertCSVToJSON(string csvData, string mainField = "data")
        {
            // Split the CSV into rows
            string[] rows = Regex.Split(csvData, LINE_SPLIT_RE);

            // Create a list to hold the JSON objects
            List<JObject> jsonObjects = new List<JObject>();

            // Get the header from the first row
            string[] headers = Regex.Split(rows[0], SPLIT_RE);
            for (int i = 0; i < headers.Length; i++)
            {
                headers[i] = headers[i].Trim();
            }

            // Iterate over the remaining rows
            for (int i = 1; i < rows.Length; i++)
            {
                // Skip empty rows
                if (string.IsNullOrWhiteSpace(rows[i]))
                    continue;

                // Split the row into columns
                string[] columns = Regex.Split(rows[i], SPLIT_RE);

                // Create a new JSON object
                JObject jsonObject = new JObject();

                // Add each column value to the JSON object with the corresponding header
                for (int j = 0; j < headers.Length; j++)
                {
                    // Trim whitespace and handle cases where there are fewer columns than headers
                    string value = j < columns.Length ? columns[j].Trim() : "";
                    jsonObject[headers[j]] = value;
                }

                // Add the JSON object to the list
                jsonObjects.Add(jsonObject);
            }

            // Create a JSON array from the list of JSON objects
            JArray jsonArray = new JArray(jsonObjects);

            // Wrap the array in a JSON object if needed (optional)
            JObject finalJson = new JObject
            {
                [mainField] = jsonArray
            };
            return finalJson;
        }
    }
}