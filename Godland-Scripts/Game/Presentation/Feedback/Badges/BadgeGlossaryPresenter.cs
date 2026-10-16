using VContainer.Unity;

namespace Godland.Game.Presentation.Feedback.Badges
{
    public sealed class BadgeGlossaryPresenter : IStartable
    {
        private readonly BadgeGlossaryView _view;
        private readonly FeedbackConfig _config;
        private bool _expanded = true;

        public BadgeGlossaryPresenter(BadgeGlossaryView view, FeedbackConfig config)
        {
            _view = view;
            _config = config;
        }

        public void Start()
        {
            _view.Fill(_config.Badges);
            _view.SetExpanded(_expanded);
            _view.Bind(Toggle);
        }

        private void Toggle()
        {
            _expanded = !_expanded;
            _view.SetExpanded(_expanded);
        }
    }
}
