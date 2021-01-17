using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PKeep
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            Page page;
            if (!Application.Current.Properties.ContainsKey("token"))
                page = new LoginPage();

            else
                page = new MainPage();
                MainPage = new NavigationPage(page);

        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
