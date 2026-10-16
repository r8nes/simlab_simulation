using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Godland.Game.Presentation.Settings;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Godland.Game.SimLab
{
    public sealed class EditableField
    {
        private readonly FieldInfo _field;
        private readonly object _owner;

        public EditableField(FieldInfo field, object owner, string label)
        {
            _field = field;
            _owner = owner;
            Label = label;
        }

        public string Label { get; }

        public IReadOnlyList<string> Options =>
            _field.FieldType == typeof(bool) ? new[] { "да", "нет" }
            : _field.FieldType.IsEnum ? Enum.GetNames(_field.FieldType)
            : null;

        public string Value => _field.FieldType == typeof(bool)
            ? (bool)_field.GetValue(_owner) ? "да" : "нет"
            : Convert.ToString(_field.GetValue(_owner), CultureInfo.InvariantCulture);

        public bool TrySet(string text)
        {
            var type = _field.FieldType;
            object value = null;
            if (type == typeof(int) && int.TryParse(text, out var i)) value = i;
            else if (type == typeof(float) && float.TryParse(text.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out var f)) value = f;
            else if (type == typeof(bool) && (text == "да" || text == "нет")) value = text == "да";
            else if (type == typeof(string)) value = text;
            else if (type.IsEnum && Enum.TryParse(type, text, true, out var e)) value = e;
            if (value == null) return false;

            _field.SetValue(_owner, value);
            return true;
        }
    }

    public sealed class ConfigSection
    {
        public ConfigSection(string title, IReadOnlyList<EditableField> fields)
        {
            Title = title;
            Fields = fields;
        }

        public string Title { get; }
        public IReadOnlyList<EditableField> Fields { get; }
        public bool Expanded { get; set; }
    }

    public static class ConfigEditor
    {
        private const BindingFlags Instance = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        public static GameSettings Clone(GameSettings source)
        {
            var clone = Object.Instantiate(source);
            foreach (var field in ConfigFields())
                if (field.GetValue(clone) is ScriptableObject config)
                    field.SetValue(clone, Object.Instantiate(config));
            return clone;
        }

        public static IReadOnlyList<ConfigSection> Sections(GameSettings settings) =>
            ConfigFields()
                .Select(field => (Name: Label(field.Name), Config: field.GetValue(settings) as ScriptableObject))
                .Where(pair => pair.Config != null && !SimLabLabels.HiddenSections.Contains(pair.Name))
                .Select(pair => new ConfigSection(SimLabLabels.Sections.GetValueOrDefault(pair.Name, pair.Name), Editable(pair.Name, pair.Config)))
                .Where(section => section.Fields.Count > 0)
                .ToList();

        private static IEnumerable<FieldInfo> ConfigFields() =>
            typeof(GameSettings).GetFields(Instance).Where(field => typeof(ScriptableObject).IsAssignableFrom(field.FieldType));

        private static IReadOnlyList<EditableField> Editable(string section, object owner) =>
            owner.GetType().GetFields(Instance)
                .Where(field => field.IsPublic || field.GetCustomAttribute<SerializeField>() != null)
                .Where(field => field.FieldType == typeof(int) || field.FieldType == typeof(float) || field.FieldType == typeof(bool) || field.FieldType.IsEnum)
                .Select(field => (Field: field, Key: $"{section}.{Label(field.Name)}"))
                .Where(pair => !SimLabLabels.HiddenFields.Contains(pair.Key))
                .Select(pair => new EditableField(pair.Field, owner, SimLabLabels.Fields.GetValueOrDefault(pair.Key, Label(pair.Field.Name))))
                .ToList();

        private static string Label(string name)
        {
            if (name.StartsWith("<")) name = name.Substring(1, name.IndexOf('>') - 1);
            return name.TrimStart('_');
        }
    }
}
