using Android.Widget;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PKeep
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage : ContentPage
    {
        public LoginPage()
        {
            InitializeComponent();
            this.BindingContext = new Login();
        }

        private async void Button_Clicked(object sender, EventArgs e)
        {
            using (var http = new HttpClient()){
                var obj = (Login)this.BindingContext;
                var con = new StringContent(JsonConvert.SerializeObject(obj));
                var res = http.PostAsync(Settings.BaseUrl + "/api/token", con).Result;
                var strg = res.Content.ReadAsStringAsync().Result;
                var response = JsonConvert.DeserializeObject<LoginResponse>(strg);
                if (string.IsNullOrEmpty(response.token)){
                    Toast.MakeText(Android.App.Application.Context, response.message, ToastLength.Long).Show();
                    return;
                }
                Application.Current.Properties["token"] = response.token;
                await Navigation.PushAsync(new MainPage());
                Navigation.RemovePage(this);
            }
        }
    }
    public class Login
    {
        public string nomeutente { get; set; }
        public string password { get; set; }
    }
    public class LoginResponse
    {
        public string message { get; set; }
        public string token { get; set; }
    }
}