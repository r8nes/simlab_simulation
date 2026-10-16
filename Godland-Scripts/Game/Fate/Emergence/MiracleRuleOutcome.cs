using System;
using Godland.Core;
using Godland.Core.World.Grid;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Fate.Emergence
{
    [Serializable]
    public sealed class MiracleRuleOutcome : IEmergenceOutcome
    {
        [SerializeField, MinValue(1), SuffixLabel("%"), InfoBox("Множитель цены чудес (100 — без изменений).")] private int _taxPercent = 100;
        [SerializeField, MinValue(0), SuffixLabel("%"), InfoBox("Сколько веры возвращается миру после каждого чуда.")] private int _refundPercent;
        [SerializeField, InfoBox("Изменение порога гнева богоборцев (минус — бунтуют быстрее).")] private int _ireThreshold;

        public void Apply(EmergenceContext context, GridCoord site, string title)
        {
            if (_taxPercent != 100) context.Modifiers.TaxMiracles(_taxPercent);
            if (_refundPercent > 0) context.Modifiers.RefundMiracles(_refundPercent);
            if (_ireThreshold != 0) context.Modifiers.Apply(WorldModifier.IreThreshold, _ireThreshold);
        }
    }
}
