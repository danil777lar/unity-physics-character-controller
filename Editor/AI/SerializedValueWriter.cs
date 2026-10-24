using System;
using System.Globalization;
using UnityEditor;
using UnityEngine;

namespace Larje.Character.Editor.AI
{
    internal static class SerializedValueWriter
    {
        public static bool TryWrite(SerializedProperty property, string text, out string error)
        {
            error = null;
            try
            {
                switch (property.propertyType)
                {
                    case SerializedPropertyType.Integer:
                        property.intValue = int.Parse(text, CultureInfo.InvariantCulture);
                        return true;
                    case SerializedPropertyType.Float:
                        property.floatValue = float.Parse(text, CultureInfo.InvariantCulture);
                        return true;
                    case SerializedPropertyType.Boolean:
                        property.boolValue = text == "1" || string.Equals(text, "true", StringComparison.OrdinalIgnoreCase);
                        return true;
                    case SerializedPropertyType.String:
                        property.stringValue = text;
                        return true;
                    case SerializedPropertyType.Enum:
                        int index = Array.FindIndex(property.enumNames, x => string.Equals(x, text, StringComparison.OrdinalIgnoreCase));
                        if (index < 0)
                        {
                            error = $"Enum value '{text}' not in [{string.Join(", ", property.enumNames)}]";
                            return false;
                        }

                        property.enumValueIndex = index;
                        return true;
                    case SerializedPropertyType.Vector2:
                        Vector3 v2 = ParseVector(text);
                        property.vector2Value = new Vector2(v2.x, v2.y);
                        return true;
                    case SerializedPropertyType.Vector3:
                        property.vector3Value = ParseVector(text);
                        return true;
                    case SerializedPropertyType.ObjectReference:
                        UnityEngine.Object asset = string.IsNullOrEmpty(text) ? null : AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(text);
                        if (!string.IsNullOrEmpty(text) && asset == null)
                        {
                            error = $"Asset not found at '{text}'";
                            return false;
                        }

                        property.objectReferenceValue = asset;
                        return true;
                    default:
                        error = $"Unsupported property type {property.propertyType}";
                        return false;
                }
            }
            catch (FormatException exception)
            {
                error = exception.Message;
                return false;
            }
        }

        public static string Read(SerializedProperty property)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                    return property.intValue.ToString(CultureInfo.InvariantCulture);
                case SerializedPropertyType.Float:
                    return property.floatValue.ToString(CultureInfo.InvariantCulture);
                case SerializedPropertyType.Boolean:
                    return property.boolValue ? "true" : "false";
                case SerializedPropertyType.String:
                    return property.stringValue;
                case SerializedPropertyType.Enum:
                    return property.enumValueIndex >= 0 && property.enumValueIndex < property.enumNames.Length ? property.enumNames[property.enumValueIndex] : property.intValue.ToString();
                case SerializedPropertyType.Vector2:
                    return property.vector2Value.ToString("F3");
                case SerializedPropertyType.Vector3:
                    return property.vector3Value.ToString("F3");
                case SerializedPropertyType.ObjectReference:
                    return property.objectReferenceValue != null ? property.objectReferenceValue.name : "null";
                default:
                    return property.propertyType.ToString();
            }
        }

        private static Vector3 ParseVector(string text)
        {
            string[] parts = text.Replace("(", string.Empty).Replace(")", string.Empty).Split(',');
            float x = parts.Length > 0 ? float.Parse(parts[0], CultureInfo.InvariantCulture) : 0f;
            float y = parts.Length > 1 ? float.Parse(parts[1], CultureInfo.InvariantCulture) : 0f;
            float z = parts.Length > 2 ? float.Parse(parts[2], CultureInfo.InvariantCulture) : 0f;
            return new Vector3(x, y, z);
        }
    }
}
