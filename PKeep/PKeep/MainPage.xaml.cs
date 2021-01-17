using Android.Widget;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace PKeep
{
    public partial class MainPage : ContentPage
    {
        static IEnumerable<Password> list;
        public MainPage()
        {
            InitializeComponent();
            this.Appearing += MainPage_Appearing;

        }

        private async void RevealCode_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new Aggiungi(new Password())
            {
                
            });
        }

        private async void mainList_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            var ag = new Aggiungi((Password)e.Item);
            await Navigation.PushAsync(ag) ;
        }

        private async void MainPage_Appearing(object sender, EventArgs e)
        {
            try
            {
                using (var http = new HttpClient())
                {
                    http.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", Application.Current.Properties["token"].ToString());
                    var url = Settings.BaseUrl+"/api/password";
                    if (!string.IsNullOrEmpty(searchBar.Text))
                    {
                        url += "?search=" + searchBar.Text;
                    }
                    var items = http.GetAsync(url).Result;
                    if (items.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    {
                        Application.Current.MainPage = new NavigationPage(new LoginPage());
                        await Navigation.PopToRootAsync();
                        //await Navigation.PushAsync(new LoginPage());
                        return;
                    }
                    var strin = items.Content.ReadAsStringAsync().Result;
                    list = JsonConvert.DeserializeObject<IEnumerable<Password>>(strin);
                    this.mainList.ItemsSource = list;


                }
            }
            catch
            {
                Toast.MakeText(Android.App.Application.Context, "Si è verificato un errore durante il caricamento delle password", ToastLength.Long).Show();
            }
        }

        private async void SetKey_Clicked(object sender, EventArgs e)
        {
            string result = await DisplayPromptAsync("Imposta chiave", "Inserisci la chiave per decriptare");
            Application.Current.Properties["key"] = result;
        }

        private void searchBar_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(searchBar.Text))
            {
                var listfilter = list.Where(c => c.descrizione.ToLower().Contains(searchBar.Text.ToLower())
                                                || c.nome.ToLower().Contains(searchBar.Text.ToLower())).ToList();
                mainList.ItemsSource = listfilter;
            }
            else
                mainList.ItemsSource = list;

        }
    }
}
