using System;
using System.Collections.Generic;
using System.Linq;
using Godland.Game.Writer.Progression;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Godland.Game.Presentation.Menu
{
    public sealed class ChronicleBookPresenter : IStartable, IDisposable
    {
        private const string Unknown = "???";
        private const string NoHint = "Мир ещё не показал этого.";

        private static readonly (string Kind, string Title)[] Sections =
        {
            ("epoch", "Эпохи"),
            ("catastrophe", "Катастрофы"),
            ("saga-act", "Саги"),
            ("pantheon", "Пантеоны"),
            ("sovereign", "Правители мира"),
            ("race", "Народы"),
            ("titan", "Титаны"),
            ("anomaly", "Явления"),
            ("emergence", "Возникновения"),
            ("signal", "Знаки мира"),
        };

        private readonly ChronicleBookView _view;
        private readonly TooltipView _tooltip;
        private readonly ProgressionConfig _config;
        private readonly ProfileStore _store;
        private readonly IObjectResolver _resolver;
        private readonly List<DiscoveryRowView> _rows = new();
        private readonly Dictionary<DiscoveryRowView, DiscoveryDefinition> _discoveries = new();
        private readonly List<DiscoveryRowView> _tabs = new();
        private int _section;

        public ChronicleBookPresenter(ChronicleBookView view, TooltipView tooltip, ProgressionConfig config, ProfileStore store, IObjectResolver resolver)
        {
            _view = view;
            _tooltip = tooltip;
            _config = config;
            _store = store;
            _resolver = resolver;
        }

        public void Start()
        {
            _view.Bind();
            _tooltip.Hide();
            _view.Opened += Open;
            _view.Closed += Close;
        }

        public void Dispose()
        {
            _view.Opened -= Open;
            _view.Closed -= Close;
            _view.Unbind();
        }

        private void Open()
        {
            _section = 0;
            Rebuild();
            _view.SetVisible(true);
        }

        private void Rebuild()
        {
            Clear();
            var sections = SectionsOf(_config.Discoveries.Where(discovery => discovery != null).ToList());
            _section = Math.Clamp(_section, 0, Math.Max(0, sections.Count - 1));
            var profile = _store.Profile;
            for (var i = 0; i < sections.Count; i++)
            {
                var (title, entries) = sections[i];
                var opened = entries.Count(discovery => profile.HasDiscovered(discovery.name));
                var waiting = entries.Any(discovery => profile.CanClaim(discovery.name)) ? "  <color=#ffd27a>●</color>" : string.Empty;
                var tab = _resolver.Instantiate(_view.RowPrefab, _view.Tabs);
                tab.ShowTab($"{title}\n<size=75%>{opened} из {entries.Count}</size>{waiting}", i == _section);
                var index = i;
                tab.Clicked += _ => SelectSection(index);
                _tabs.Add(tab);
            }

            if (sections.Count > 0) AddRows(sections[_section].Entries);
            ShowSummary();
        }

        private void SelectSection(int index)
        {
            if (index == _section) return;

            _section = index;
            _tooltip.Hide();
            Rebuild();
        }

        private static List<(string Title, List<DiscoveryDefinition> Entries)> SectionsOf(IReadOnlyList<DiscoveryDefinition> discoveries)
        {
            var sections = Sections
                .Select(section => (section.Title, Entries: discoveries.Where(discovery => KindOf(discovery) == section.Kind).ToList()))
                .Where(section => section.Entries.Count > 0)
                .ToList();
            var known = Sections.Select(section => section.Kind).ToHashSet();
            var rest = discoveries.Where(discovery => !known.Contains(KindOf(discovery))).ToList();
            if (rest.Count > 0) sections.Add(("Прочее", rest));
            return sections;
        }

        private static string KindOf(DiscoveryDefinition discovery)
        {
            var key = discovery.Key ?? string.Empty;
            var colon = key.IndexOf(':');
            return colon < 0 ? key : key.Substring(0, colon);
        }

        private void AddRows(IReadOnlyList<DiscoveryDefinition> section)
        {
            var profile = _store.Profile;
            foreach (var discovery in section
                         .OrderByDescending(discovery => profile.HasDiscovered(discovery.name))
                         .ThenBy(discovery => discovery.Hidden))
            {
                var found = profile.HasDiscovered(discovery.name);
                var row = _resolver.Instantiate(_view.RowPrefab, _view.Rows);
                row.Show(found || !discovery.Hidden ? discovery.Title : Unknown, TooltipOf(discovery, found), found);
                ShowReward(row, discovery);
                row.Hovered += OnHovered;
                row.Left += OnLeft;
                row.Claimed += OnClaimed;
                _rows.Add(row);
                _discoveries[row] = discovery;
            }
        }

        private void ShowSummary()
        {
            var profile = _store.Profile;
            var discoveries = _config.Discoveries.Where(discovery => discovery != null).ToList();
            var count = discoveries.Count(discovery => profile.HasDiscovered(discovery.name));
            var waiting = discoveries.Count(discovery => profile.CanClaim(discovery.name));
            var unclaimed = waiting > 0 ? $"   ·   Ждут награды: {waiting}" : string.Empty;
            _view.ShowSummary($"Открыто: {count} из {discoveries.Count}   ·   Память миров: {profile.Memory}{unclaimed}");
        }

        private void ShowReward(DiscoveryRowView row, DiscoveryDefinition discovery)
        {
            var profile = _store.Profile;
            var reward = _config.DiscoveryReward;
            if (profile.CanClaim(discovery.name)) row.ShowReward($"Забрать +{reward} Памяти миров", true);
            else if (profile.HasClaimed(discovery.name)) row.ShowReward("Награда получена", false);
            else row.ShowReward($"Награда: +{reward} Памяти миров", false);
        }

        private void OnClaimed(DiscoveryRowView row)
        {
            if (!_discoveries.TryGetValue(row, out var discovery) || !_store.Profile.Claim(discovery.name, _config.DiscoveryReward)) return;

            _store.Save();
            Rebuild();
        }

        private void Close()
        {
            _tooltip.Hide();
            _view.SetVisible(false);
            Clear();
        }

        private static string TooltipOf(DiscoveryDefinition discovery, bool found)
        {
            if (found) return string.IsNullOrEmpty(discovery.Text) ? discovery.Title : $"<b>{discovery.Title}</b>\n{discovery.Text}";
            if (!discovery.Hidden) return string.IsNullOrEmpty(discovery.Text) ? $"<b>{discovery.Title}</b>\nЕщё не случалось." : $"<b>{discovery.Title}</b>\n{discovery.Text}\n<i>Ещё не случалось.</i>";
            return string.IsNullOrEmpty(discovery.Hint) ? NoHint : discovery.Hint;
        }

        private void OnHovered(DiscoveryRowView row)
        {
            if (!string.IsNullOrEmpty(row.Tooltip)) _tooltip.Show(row.Tooltip, row.transform.position);
        }

        private void OnLeft(DiscoveryRowView row) => _tooltip.Hide();

        private void Clear()
        {
            foreach (var row in _rows)
            {
                row.Hovered -= OnHovered;
                row.Left -= OnLeft;
                row.Claimed -= OnClaimed;
                UnityEngine.Object.Destroy(row.gameObject);
            }

            _rows.Clear();
            _discoveries.Clear();
            foreach (var tab in _tabs) UnityEngine.Object.Destroy(tab.gameObject);
            _tabs.Clear();
        }
    }
}
