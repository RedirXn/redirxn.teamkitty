namespace Redirxn.TeamKitty.ViewModels
{
    public class TickDisplay : BaseViewModel
    {
        private bool _ticked;
        public bool Ticked
        {
            get { return _ticked; }
            set { SetProperty(ref _ticked, value); }
        }
        public string DisplayName { get; set; }
        public string Id { get; set; }
    }
}