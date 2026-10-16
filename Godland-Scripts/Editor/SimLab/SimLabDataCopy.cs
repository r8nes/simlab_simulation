using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Godland.Editor.SimLab
{
    public static class SimLabDataCopy
    {
        public const string Folder = "Assets/SimLabBuildTemp";

        private static readonly Dictionary<ScriptableObject, ScriptableObject> Copies = new();

        public static string CreateScene(string scenePath, string settingsPath)
        {
            Delete();
            AssetDatabase.CreateFolder("Assets", "SimLabBuildTemp");
            Copies.Clear();
            var settings = Copy(AssetDatabase.LoadAssetAtPath<ScriptableObject>(settingsPath));
            AssetDatabase.SaveAssets();

            var oldGuid = AssetDatabase.AssetPathToGUID(settingsPath);
            var newGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(settings));
            var copyPath = $"{Folder}/SimLab.unity";
            File.WriteAllText(copyPath, File.ReadAllText(scenePath).Replace(oldGuid, newGuid));
            AssetDatabase.ImportAsset(copyPath);
            return copyPath;
        }

        public static void Delete()
        {
            Copies.Clear();
            if (AssetDatabase.IsValidFolder(Folder)) AssetDatabase.DeleteAsset(Folder);
        }

        private static ScriptableObject Copy(ScriptableObject source)
        {
            if (Copies.TryGetValue(source, out var existing)) return existing;

            var copy = Object.Instantiate(source);
            copy.name = source.name;
            AssetDatabase.CreateAsset(copy, $"{Folder}/{Copies.Count}.asset");
            Copies[source] = copy;

            var serialized = new SerializedObject(copy);
            var property = serialized.GetIterator();
            while (property.Next(true))
            {
                if (property.propertyType != SerializedPropertyType.ObjectReference || property.name == "m_Script") continue;
                property.objectReferenceValue = property.objectReferenceValue is ScriptableObject config ? Copy(config) : null;
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            return copy;
        }
    }
}
