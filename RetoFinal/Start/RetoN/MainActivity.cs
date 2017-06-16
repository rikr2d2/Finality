/*
 * To add Offline Sync Support:
 *  1) Add the NuGet package Microsoft.Azure.Mobile.Client.SQLiteStore (and dependencies) to all client projects
 *  2) Uncomment the #define OFFLINE_SYNC_ENABLED
 *
 * For more information, see: http://go.microsoft.com/fwlink/?LinkId=717898
 */
//#define OFFLINE_SYNC_ENABLED
using System;
using Android.OS;
using Android.App;
using Android.Views;
using Android.Widget;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using RetoFinal;
using Android.Content;
using Android.Runtime;
using Gcm.Client;

#if OFFLINE_SYNC_ENABLED
using Microsoft.WindowsAzure.MobileServices.Sync;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;
#endif

namespace RetoFinal
{
    [Activity(MainLauncher = true,
               Icon = "@drawable/ic_launcher", Label = "@string/app_name",
               Theme = "@style/AppTheme")]
    public class MainActivity : Activity
    {
        // Client reference.
        private MobileServiceClient client;

        // Create a new instance field for this activity.
        static MainActivity instance = new MainActivity();

        // URL of the mobile app backend.
        const string applicationURL = @"https://portaltickets.azurewebsites.net";

        // Return the current activity instance.
        public static MainActivity CurrentActivity
        {
            get
            {
                return instance;
            }
        }
        // Return the Mobile Services client.
        public MobileServiceClient CurrentClient
        {
            get
            {
                return client;
            }
        }


        protected override async void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            var btn = FindViewById<Button>(Resource.Id.buttonLoginUser);
            btn.Enabled = false;

            // Create the client instance, using the mobile app backend URL.
            client = new MobileServiceClient(applicationURL);

            CurrentPlatform.Init();

            Toast.MakeText(this, "Validando conexión a Internet", ToastLength.Long).Show();

            Plugin.Connectivity.CrossConnectivity.Current.ConnectivityChanged += Current_ConnectivityChanged;

            if (Plugin.Connectivity.CrossConnectivity.Current.IsConnected)
            {
                Toast.MakeText(this, "Conectado a Internet. Favor de iniciar sesión.", ToastLength.Long).Show();
                btn.Enabled = true;
            }
            else
            {
                Toast.MakeText(this, "No hay una conexión disponible. Necesitas una conexión a internet para iniciar sesión.", ToastLength.Long).Show();
                btn.Enabled = false;
            }
        }

        private void Current_ConnectivityChanged(object sender, Plugin.Connectivity.Abstractions.ConnectivityChangedEventArgs e)
        {
            var btn = FindViewById<Button>(Resource.Id.buttonLoginUser);
            if (Plugin.Connectivity.CrossConnectivity.Current.IsConnected)
            {
                Toast.MakeText(this, "Conectado a Internet. Ya puedes iniciar sesión.", ToastLength.Long).Show();
                btn.Enabled = true;
            }
            else
            {
                Toast.MakeText(this, "No hay una conexión disponible. Necesitas una conexión a internet para iniciar sesión.", ToastLength.Long).Show();
                btn.Enabled = false;
            }
        }

        //Initializes the activity menu
        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            //MenuInflater.Inflate(Resource.Menu.activity_main, menu);
            return true;
        }

        private void CreateAndShowDialog(Exception exception, String title)
        {
            CreateAndShowDialog(exception.Message, title);
        }

        private void CreateAndShowDialog(string message, string title)
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(this);

            builder.SetMessage(message);
            builder.SetTitle(title);
            builder.Create().Show();
        }

        // Define a authenticated user.
        private MobileServiceUser user;
        private async Task<bool> Authenticate()
        {
            var success = false;
            try
            {
                // Sign in with Facebook login using a server-managed flow.
                user = await client.LoginAsync(this,
                    MobileServiceAuthenticationProvider.MicrosoftAccount);
                CreateAndShowDialog(string.Format("Iniciado - {0}",
                    user.UserId), "Iniciado correctamente.");

                success = true;
            }
            catch (Exception ex)
            {
                CreateAndShowDialog(ex, "Falló inicio de sesión");
            }
            return success;
        }

        [Java.Interop.Export()]
        public async void LoginUser(View view)
        {
            // Load data only after authentication succeeds.
            if (await Authenticate())
            {
                var intent = new Intent(this, typeof(TicketsActivity));
                intent.PutExtra("Usuario", user.UserId);
                StartActivityForResult(intent, 1);
            }


            //var intent = new Intent(this, typeof(TicketsActivity));
            //intent.PutExtra("Usuario", "asdaces");
            //StartActivityForResult(intent, 1);

        }
    }
}


