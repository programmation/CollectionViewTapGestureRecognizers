using Windows.Foundation;
using Windows.UI.ViewManagement;

namespace CollectionViewTapGestureRecognizers.UWP
{
    public sealed partial class MainPage
    {
        public MainPage()
        {
            InitializeComponent();
            LoadApplication(new CollectionViewTapGestureRecognizers.App());
        }
    }
}
