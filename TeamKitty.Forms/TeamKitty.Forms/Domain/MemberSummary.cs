using System.ComponentModel;

namespace TeamKitty.Forms.Domain
{
    public class MemberSummary : INotifyPropertyChanged
    {
        public string MemberName { get; internal set; }
        public decimal BeerTotal { get; internal set; }
        public decimal PreMixTotal { get; internal set; }
        public decimal SoftDrinkTotal { get; internal set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

    }
}