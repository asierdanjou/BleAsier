using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using System.Threading.Tasks;
using Android;

namespace BleAsier.Droid
{
    [Activity(Label = "BleAsier", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {

        const int RequestLocationId = 0;

        readonly string[] PermissionsLocation =
            {
                Manifest.Permission.AccessCoarseLocation,
                Manifest.Permission.AccessFineLocation
            };

        async Task GetLocationPermissionAsync()
        {
            const string permission = Manifest.Permission.AccessFineLocation;

            if (CheckSelfPermission(permission) == (int)Permission.Granted)
            {
                //await GetLocationAsync();
                return;
            }

            if (ShouldShowRequestPermissionRationale(permission))
            {
                //Explain to the user why we need to read the contacts
                /*
                Snackbar.Make(layout, "Location access is required to show coffee shops nearby.",
                    Snackbar.LengthIndefinite)
                    .SetAction("OK", v => RequestPermissions(PermissionsLocation, RequestLocationId))
                    .Show();

                return;
                */
            }

            RequestPermissions(PermissionsLocation, RequestLocationId);

        }
        /*
        public override async void OnRequestPermissionsResult(int requestCode, string[] permissions, int[] grantResults)
        {
            switch (requestCode)
            {
                case RequestLocationId:
                    {
                        if (grantResults[0] == (int)Permission.Granted)
                        {
                            //Permission granted
                        }
                        else
                        {
                            //Permission Denied :(
                            //Disabling location functionality
                        }
                    }
                    break;
            }
        }
        */
        protected override async void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            if ((int)Build.VERSION.SdkInt >= 23)
            {
                await GetLocationPermissionAsync();
            }

            base.OnCreate(savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            LoadApplication(new App());
        }
                
    }
}