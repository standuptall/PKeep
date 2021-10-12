using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Speech;
using Android.Widget;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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
        new AuthenticationHeaderValue("Bearer", Xamarin.Forms.Application.Current.Properties["token"].ToString());
                    var url = Settings.BaseUrl+"/api/password";
                    if (!string.IsNullOrEmpty(searchBar.Text))
                    {
                        url += "?search=" + searchBar.Text;
                    }
                    var items = http.GetAsync(url).Result;
                    if (items.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    {
                        Xamarin.Forms.Application.Current.MainPage = new NavigationPage(new LoginPage());
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
            Xamarin.Forms.Application.Current.Properties["key"] = result;
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
        private async void Fuel_Clicked(object sender, EventArgs e)
        {
            ///qui parte il riconoscimento vocale
            ///tipo "Aggiungi quaranta euro di rifornimento, tachimetro nove quattro due nove, costo uno virgola sei sette cinque, residuo cinquantaquattro
            //var voiceIntent = new Intent(RecognizerIntent.ActionRecognizeSpeech);
            //voiceIntent.PutExtra(RecognizerIntent.ExtraLanguageModel, RecognizerIntent.LanguageModelFreeForm);

            //// message and modal dialog  
            //voiceIntent.PutExtra(RecognizerIntent.ExtraPrompt, "Speak now");

            //// end capturing speech if there is 3 seconds of silence  
            //voiceIntent.PutExtra(RecognizerIntent.ExtraSpeechInputCompleteSilenceLengthMillis, 3000);
            //voiceIntent.PutExtra(RecognizerIntent.ExtraSpeechInputPossiblyCompleteSilenceLengthMillis, 3000);
            //voiceIntent.PutExtra(RecognizerIntent.ExtraSpeechInputMinimumLengthMillis, 30000);
            //voiceIntent.PutExtra(RecognizerIntent.ExtraMaxResults, 1);
            //voiceIntent.AddFlags(ActivityFlags.NewTask);

            //// method to specify other languages to be recognised here if desired  
            ////voiceIntent.PutExtra(RecognizerIntent.ExtraLanguage, Java.Util.Locale.Default);
            ////((Activity)Forms.Context).StartActivityForResult(voiceIntent, 10);
            //Android.App.Application.Context.StartActivity(voiceIntent);


            //voiceIntent.
            var test = await this.SpeechToTextAsync();
            var importo = 40;
            var tachimetro = 9429;
            var costo = 1.675;
            var residuo = 54;
            ///alla fine avrò un oggetto 
            var rif = new Rifornimento()
            {
                Guid = Guid.NewGuid(),
                Data = DateTime.Now,
                Importo = importo,
                Tachimetro = tachimetro,
                Costo = costo,
                Residuo = residuo
            };
            //salva nel repository local
            rif.Save();
            //prova a memorizzarlo sul server
            try
            {
                using (var http = new HttpClient())
                {
                    http.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", Xamarin.Forms.Application.Current.Properties["token"].ToString());
                    var url = Settings.BaseUrl + "/api/rifornimenti";
                    var bd = new StringContent(JsonConvert.SerializeObject(rif));
                    var items = await http.PostAsync(url, bd);

                    if (!items.IsSuccessStatusCode)
                    {
                        throw new Exception("Errore " + items.StatusCode);
                    }
                    //cancella dal repository locale
                    rif.Remove();
                    Toast.MakeText(Android.App.Application.Context, "Rifornimento aggiunto con successo",ToastLength.Long).Show();
                }
            }
            catch (Exception ex)
            {
                Toast.MakeText(Android.App.Application.Context, ex.Message,ToastLength.Long).Show();
            }
        }
        public Task<string> SpeechToTextAsync()
        {
            var tcs = new TaskCompletionSource<string>();

            try
            {
                var voiceIntent = new Intent(RecognizerIntent.ActionRecognizeSpeech);
                voiceIntent.PutExtra(RecognizerIntent.ExtraLanguageModel, RecognizerIntent.LanguageModelFreeForm);
                voiceIntent.PutExtra(RecognizerIntent.ExtraPrompt, "Sprechen Sie jetzt");
                voiceIntent.PutExtra(RecognizerIntent.ExtraSpeechInputCompleteSilenceLengthMillis, 1500);
                voiceIntent.PutExtra(RecognizerIntent.ExtraSpeechInputPossiblyCompleteSilenceLengthMillis, 1500);
                voiceIntent.PutExtra(RecognizerIntent.ExtraSpeechInputMinimumLengthMillis, 15000);
                voiceIntent.PutExtra(RecognizerIntent.ExtraMaxResults, 1);
                voiceIntent.AddFlags(ActivityFlags.SingleTop);
                voiceIntent.PutExtra(RecognizerIntent.ExtraLanguage, Java.Util.Locale.Default);
                var exp = convertImplicitIntentToExplicitIntent(voiceIntent, Android.App.Application.Context);
                try
                {
                    //CrossCurrentActivity.Current.Activity
                    Android.App.Application.Context.StartForegroundService(voiceIntent);
                    //Android.App.Application.Context.StartActivity(voiceIntent);
                    //var c = Xamarin.Forms.Forms.Context;
                    //((Activity)Android.App.Application.Context).StartActivityForResult(voiceIntent, 10);

                }
                catch (ActivityNotFoundException a)
                {
                    tcs.SetResult("Device doesn't support speech to text");
                }
            }
            catch (Exception ex)
            {

                tcs.SetException(ex);
            }

            return tcs.Task;
        }
        public static Intent convertImplicitIntentToExplicitIntent(Intent implicitIntent, Context context)
        {
            PackageManager pm = context.PackageManager;
            IList<ResolveInfo> resolveInfoList = pm.QueryIntentServices(implicitIntent,PackageInfoFlags.Services);

            if (resolveInfoList == null || resolveInfoList.Count != 1)
            {
                return null;
            }
            ResolveInfo serviceInfo = resolveInfoList[0];
            ComponentName component = new ComponentName(serviceInfo.ServiceInfo.PackageName, serviceInfo.ServiceInfo.Name);
            Intent explicitIntent = new Intent(implicitIntent);
            explicitIntent.SetComponent(component);
            return explicitIntent;
        }

    }
}
