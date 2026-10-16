namespace Godland.Core
{
    public sealed class WorldModifiers
    {
        public int IreThresholdBonus { get; private set; }
        public float PlagueSpreadMultiplier { get; private set; } = 1f;
        public float SuperBiomePeriodMultiplier { get; private set; } = 1f;
        public float CorruptionChanceMultiplier { get; private set; } = 1f;
        public int NightUntilYear { get; private set; }
        public int MiracleCostPercent { get; private set; } = 100;

        public void SetMiracleCost(int percent) => MiracleCostPercent = percent;

        public int EmergenceChancePercent { get; private set; } = 100;

        public void SetEmergenceChance(int percent) => EmergenceChancePercent = percent;

        public int MiraclesBlockedUntil { get; private set; }

        public int MiracleTaxPercent { get; private set; } = 100;

        public int MiracleRefundPercent { get; private set; }

        public void TaxMiracles(int percent) => MiracleTaxPercent = MiracleTaxPercent * percent / 100;

        public void RefundMiracles(int percent) => MiracleRefundPercent += percent;

        public void BlockMiracles(int untilYear) => MiraclesBlockedUntil = untilYear;

        public int MadUntil { get; private set; }

        public void Madden(int untilYear) => MadUntil = untilYear;

        public bool IsNight(int year) => year < NightUntilYear;

        public void BeginNight(int untilYear) => NightUntilYear = untilYear;

        public void Apply(WorldModifier modifier, float value)
        {
            switch (modifier)
            {
                case WorldModifier.IreThreshold:
                    IreThresholdBonus += (int)value;
                    break;
                case WorldModifier.PlagueSpread:
                    PlagueSpreadMultiplier *= value;
                    break;
                case WorldModifier.SuperBiomePeriod:
                    SuperBiomePeriodMultiplier *= value;
                    break;
                case WorldModifier.CorruptionChance:
                    CorruptionChanceMultiplier *= value;
                    break;
            }
        }
    }
}
