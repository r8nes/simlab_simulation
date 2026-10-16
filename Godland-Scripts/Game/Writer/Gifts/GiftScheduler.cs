using System;
using Godland.Core.World.Timing;
using Godland.Game.Writer.Run;
using Godland.Game.Presentation.Timing;
using VContainer.Unity;

namespace Godland.Game.Writer.Gifts
{
    public sealed class GiftScheduler : IStartable, IDisposable
    {
        private readonly WorldClock _clock;
        private readonly GiftOffers _offers;
        private readonly TimeFlow _flow;
        private readonly RunConfig _config;

        public GiftScheduler(WorldClock clock, GiftOffers offers, TimeFlow flow, RunConfig config)
        {
            _clock = clock;
            _offers = offers;
            _flow = flow;
            _config = config;
        }

        public void Start()
        {
            _clock.Ticked += OnTicked;
            _offers.Offered += _flow.Block;
            _offers.Chosen += OnChosen;
        }

        public void Dispose()
        {
            _clock.Ticked -= OnTicked;
            _offers.Offered -= _flow.Block;
            _offers.Chosen -= OnChosen;
        }

        private void OnTicked(int year)
        {
            if (year % _config.GiftIntervalYears == 0) _offers.Offer();
        }

        private void OnChosen(GiftDefinition _) => _flow.Unblock();
    }
}
