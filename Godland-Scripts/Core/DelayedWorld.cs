using System;
using System.Collections.Generic;
using System.Linq;

namespace Godland.Core
{
    public sealed class DelayedWorld
    {
        private readonly List<(int Year, Action Action)> _pending = new();
        private int _year;

        public void Schedule(int yearsFromNow, Action action) => _pending.Add((_year + yearsFromNow, action));

        public void Update(int year)
        {
            _year = year;
            foreach (var due in _pending.Where(entry => entry.Year <= year).ToList())
            {
                _pending.Remove(due);
                due.Action();
            }
        }
    }
}
