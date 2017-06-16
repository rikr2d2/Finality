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
using Gcm.Client;

#if OFFLINE_SYNC_ENABLED
using Microsoft.WindowsAzure.MobileServices.Sync;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;
#endif

namespace RetoFinal
{
    [Activity(Label = "@string/tickets_name")]
    public class TicketsActivity : Activity
    {
        // Client reference.
        private MobileServiceClient client;

#if OFFLINE_SYNC_ENABLED
        private IMobileServiceSyncTable<SCT_Tic_Mto> SCT_Tic_MtoTable;

        const string localDbFilename = "localstore.db";
#else
        private IMobileServiceTable<SCT_Tic_Mto> SCT_Tic_MtoTable;
#endif

        // Adapter to map the items list to the view
        private SCT_Tic_MtoAdapter adapter;

        // EditText containing the "New ToDo" text
        private EditText textNewToDo;

        // URL of the mobile app backend.
        const string applicationURL = @"https://portaltickets.azurewebsites.net";

        // Create a new instance field for this activity.
        static TicketsActivity instance = new TicketsActivity();

        private string usuarioId;

        // Return the current activity instance.
        public static TicketsActivity CurrentActivity
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
            SetContentView(Resource.Layout.Tickets);

            CurrentPlatform.Init();

            // Create the client instance, using the mobile app backend URL.
            client = new MobileServiceClient(applicationURL);

            // Set the current instance of TodoActivity. 
            instance = this;
            //Make sure the GCM client is set up correctly. 
            GcmClient.CheckDevice(this); GcmClient.CheckManifest(this);

            // Register the app for push notifications. 
            GcmClient.Register(this, ToDoBroadcastReceiver.senderIDs);


#if OFFLINE_SYNC_ENABLED
            await InitLocalStoreAsync();

            // Get the sync table instance to use to store TodoItem rows.
            SCT_Tic_MtoTable = client.GetSyncTable<SCT_Tic_Mto>();
#else
            SCT_Tic_MtoTable = client.GetTable<SCT_Tic_Mto>();
#endif

            textNewToDo = FindViewById<EditText>(Resource.Id.textNewToDo);

            // Create an adapter to bind the items with the view
            adapter = new SCT_Tic_MtoAdapter(this, Resource.Layout.Row_List_To_Do);
            var listViewToDo = FindViewById<ListView>(Resource.Id.listViewToDo);
            listViewToDo.Adapter = adapter;

            // Load the items from the mobile app backend.
            OnRefreshItemsSelected();

            usuarioId = Intent.GetStringExtra("Usuario");
        }

#if OFFLINE_SYNC_ENABLED
        private async Task InitLocalStoreAsync()
        {
            string path = Path.Combine(System.Environment.GetFolderPath(
                System.Environment.SpecialFolder.Personal), localDbFilename);
            if (!File.Exists(path))
            {
                File.Create(path).Dispose();
            }
            var store = new MobileServiceSQLiteStore(path);
            store.DefineTable<SCT_Tic_Mto>();

            // Uses the default conflict handler, which fails on conflict
            // To use a different conflict handler, pass a parameter to InitializeAsync.
            // For more details, see http://go.microsoft.com/fwlink/?LinkId=521416
            await client.SyncContext.InitializeAsync(store);
        }

        private async Task SyncAsync(bool pullData = false)
        {
            try {
                await client.SyncContext.PushAsync();

                if (pullData) {
                    await SCT_Tic_MtoTable.PullAsync("allSCT_Tic_Mtos", SCT_Tic_MtoTable.CreateQuery()); // query ID is used for incremental sync
                }
            }
            catch (Java.Net.MalformedURLException) {
                CreateAndShowDialog(new Exception("There was an error creating the Mobile Service. Verify the URL"), "Error");
            }
            catch (Exception e) {
                CreateAndShowDialog(e, "Error");
            }
        }
#endif

        //Initializes the activity menu
        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.activity_main, menu);
            return true;
        }

        //Select an option from the menu
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == Resource.Id.menu_refresh)
            {
                item.SetEnabled(false);

                OnRefreshItemsSelected();

                item.SetEnabled(true);
            }
            return true;
        }

        // Called when the refresh menu option is selected.
        private async void OnRefreshItemsSelected()
        {
#if OFFLINE_SYNC_ENABLED
			// Get changes from the mobile app backend.
            await SyncAsync(pullData: true);
#endif
            // refresh view using local store.
            await RefreshItemsFromTableAsync();
        }

        //Refresh the list with the items in the local store.
        private async Task RefreshItemsFromTableAsync()
        {
            try
            {
                // Get the items that weren't marked as completed and add them in the adapter
                var list = await SCT_Tic_MtoTable.Where(item => item.Liberado == false).ToListAsync();

                adapter.Clear();

                foreach (SCT_Tic_Mto current in list)
                    adapter.Add(current);

            }
            catch (Exception e)
            {
                CreateAndShowDialog(e, "Error");
            }
        }

        public async Task CheckItem(SCT_Tic_Mto item)
        {
            if (client == null)
            {
                return;
            }

            // Set the item as completed and update it in the table
            item.Liberado = true;
            item.QuienLibera = usuarioId;
            item.FechaLiberado = DateTime.Now;
            try
            {
                // Update the new item in the local store.
                await SCT_Tic_MtoTable.UpdateAsync(item);
#if OFFLINE_SYNC_ENABLED
                // Send changes to the mobile app backend.
				await SyncAsync();
#endif

                //if (item.Liberado)
                //    adapter.Remove(item);

            }
            catch (Exception e)
            {
                CreateAndShowDialog(e, "Error");
            }
        }

        [Java.Interop.Export()]
        public async void AddItem(View view)
        {
            if (client == null || string.IsNullOrWhiteSpace(textNewToDo.Text))
            {
                return;
            }

            // Create a new item
            var item = new SCT_Tic_Mto
            {
                Asunto = textNewToDo.Text,
                Liberado = false,
                Quien = usuarioId
            };

            try
            {
                // Insert the new item into the local store.
                await SCT_Tic_MtoTable.InsertAsync(item);
#if OFFLINE_SYNC_ENABLED
                // Send changes to the mobile app backend.
				await SyncAsync();
#endif

                if (!item.Liberado)
                {
                    adapter.Add(item);
                }
            }
            catch (Exception e)
            {
                CreateAndShowDialog(e, "Error");
            }

            textNewToDo.Text = "";
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
    }
}


