using Android.Content;
using Android.Widget;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PKeep
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Aggiungi : ContentPage
    {
        Password item;
        bool errorcrypt = false;
        public Aggiungi(Password p)
        {
            InitializeComponent();
            this.item = p;
            if (!string.IsNullOrEmpty(p.password))
            {
                try
                {
                    this.item.password = Decrypt(item.password, item.IV);
                }
                catch
                {
                    Toast.MakeText(Android.App.Application.Context, "Non è stato possibile decriptare la password", ToastLength.Long).Show();
                    errorcrypt = true;
                }
            }
            this.Title = string.IsNullOrEmpty(item.ID) ? "Nuova password" : "Modifica password";
            this.BindingContext = p;
            
        }

        

        private void RevealCode_Clicked(object sender, EventArgs e)
        {

        }

        private async void Salva_Clicked(object sender, EventArgs e)
        {
            if (errorcrypt)
            {
                bool answer = await DisplayAlert("Conferma", "Attenzione: si sono verificati errori durante la decriptazione, salvando verrà ricalcolato l'hash con la chiave attuale. Continuare?", "Si", "No");
                if (!answer)
                    return;
            }
            try
            {
                if (!CheckValidations())
                    return;
                this.item = (Password)this.BindingContext;

                string IV;
                var enc = Encrypt(item.password, out IV);
                item.IV = IV;
                item.password = enc;
                if (string.IsNullOrEmpty(item.ID))
                {
                    //aggiungi
                    using (var http = new HttpClient())
                    {
                        http.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", Application.Current.Properties["token"].ToString());
                        var content = new StringContent(JsonConvert.SerializeObject(item));

                        var res = http.PostAsync(Settings.BaseUrl + "/api/password", content).Result;
                        var str = res.Content.ReadAsStringAsync().Result;
                        if (res.IsSuccessStatusCode)
                        {
                            Toast.MakeText(Android.App.Application.Context, "Password salvata correttamente", ToastLength.Long).Show();
                        }
                        else
                        {
                            Toast.MakeText(Android.App.Application.Context, "Si è verificato un errore", ToastLength.Long).Show();
                        }

                    }
                }
                else
                {
                    //modifica
                    using (var http = new HttpClient())
                    {
                        http.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", Application.Current.Properties["token"].ToString());
                        var content = new StringContent(JsonConvert.SerializeObject(item));

                        var res = http.PutAsync(Settings.BaseUrl + "/api/password/" + item.ID, content).Result;
                        if (res.IsSuccessStatusCode)
                        {
                            Toast.MakeText(Android.App.Application.Context, "Password salvata correttamente", ToastLength.Long).Show();
                        }
                        else
                        {
                            Toast.MakeText(Android.App.Application.Context, "Si è verificato un errore", ToastLength.Long).Show();
                        }

                    }
                }
            }
            catch(Exception ex)
            {
                Toast.MakeText(Android.App.Application.Context, "Si è verificato un errore durante il salvataggio", ToastLength.Long).Show();
            }
            await Navigation.PopAsync();
        }

        private bool CheckValidations()
        {
            var message = "";
            if (string.IsNullOrEmpty(item.nome))
                message = "Scegliere un nome";
            if (string.IsNullOrEmpty(item.nomeutente))
                message = "Inserire un nome utente";
            if (string.IsNullOrEmpty(item.password))
                message = "Inserire una password";
            if (!string.IsNullOrEmpty(message))
                Toast.MakeText(Android.App.Application.Context, message, ToastLength.Long).Show();
            return string.IsNullOrEmpty(message);
        }

        public string Encrypt(string original, out string IV)
        {
            using (Aes myAes = Aes.Create())
            {
                myAes.Key = Encoding.Unicode.GetBytes(Application.Current.Properties["key"].ToString());
                // Encrypt the string to an array of bytes.
                byte[] encrypted = EncryptStringToBytes_Aes(original, myAes.Key, myAes.IV);
                IV = Convert.ToBase64String(myAes.IV);
                return Convert.ToBase64String(encrypted);

                //// Decrypt the bytes to a string.
                //string roundtrip = DecryptStringFromBytes_Aes(encrypted, myAes.Key, myAes.IV);

                ////Display the original data and the decrypted data.
                //Console.WriteLine("Original:   {0}", original);
                //Console.WriteLine("Round Trip: {0}", roundtrip);
            }
        }
        public string Decrypt(string original, string IV)
        {
            using (Aes myAes = Aes.Create())
            {
                myAes.Key = Encoding.Unicode.GetBytes(Application.Current.Properties["key"].ToString());
                myAes.IV = Convert.FromBase64String(IV);
                var dec = Convert.FromBase64String(original);
                // Encrypt the string to an array of bytes.
                string decrypted = DecryptStringFromBytes_Aes(dec, myAes.Key, myAes.IV);
                return decrypted;

                //// Decrypt the bytes to a string.
                //string roundtrip = DecryptStringFromBytes_Aes(encrypted, myAes.Key, myAes.IV);

                ////Display the original data and the decrypted data.
                //Console.WriteLine("Original:   {0}", original);
                //Console.WriteLine("Round Trip: {0}", roundtrip);
            }
        }
        static byte[] EncryptStringToBytes_Aes(string plainText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");
            byte[] encrypted;

            // Create an Aes object
            // with the specified key and IV.
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                // Create an encryptor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }

            // Return the encrypted bytes from the memory stream.
            return encrypted;
        }

        static string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");

            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            // Create an Aes object
            // with the specified key and IV.
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                // Create a decryptor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {

                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }

            return plaintext;
        }

        private async void Copia_Clicked(object sender, EventArgs e)
        {
            await Clipboard.SetTextAsync(this.item.password);
            Toast.MakeText(Android.App.Application.Context, "Copiato!", ToastLength.Long).Show();
        }

        private void Cast_Clicked(object sender, EventArgs e)
        {
            try
            {
                var obj = new { name = item.nome, pass = item.password };
                using (var http = new HttpClient())
                {
                    http.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Bearer", Application.Current.Properties["token"].ToString());
                    var content = new StringContent(JsonConvert.SerializeObject(obj));

                    var res = http.PostAsync(Settings.BaseUrl + "/api/cast/", content).Result;
                    var str = res.Content.ReadAsStringAsync().Result;
                    if (res.IsSuccessStatusCode)
                    {
                        Toast.MakeText(Android.App.Application.Context, "Password castata", ToastLength.Long).Show();
                    }
                    else
                    {
                        Toast.MakeText(Android.App.Application.Context, "Si è verificato un errore", ToastLength.Long).Show();
                    }

                }
            }
            catch(Exception ex)
            {
                Toast.MakeText(Android.App.Application.Context, "Si è verificato un errore generico", ToastLength.Long).Show();
            }
        }
    }
}