using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Plugin.FacebookClient;
using Java.Security;
using Android.Content;
using Android.Gms.Common;
using Firebase.Messaging;
using Auth0.OidcClient;
using Xamarin.Forms;
using Redirxn.TeamKitty.Models;

namespace Redirxn.TeamKitty.Droid
{
    [Activity(Label = "Redirxn.TeamKitty", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTask, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        static readonly string TAG = "MainActivity";
        internal static readonly string CHANNEL_ID = "my_notification_channel";
        internal static readonly int NOTIFICATION_ID = 100;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);
            
            IsPlayServicesAvailable(); //You can use this method to check if play services are available.
            CreateNotificationChannel();

            // FacebookClientManager.Initialize(this);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            
            GetAppSignature(); // Remove once registered on Play store

            
            var login = new AndroidLogin();
            DependencyService.RegisterSingleton<ILoginProvider>(login);

            LoadApplication(new App());
        }

        private static void GetAppSignature()
        {
            try
            {
                //PackageInfo info = Application.Context.PackageManager.GetPackageInfo(Android.App.Application.Context.PackageName, PackageInfoFlags.Signatures);
                //foreach (var signature in info.Signatures)
                //{
                //    MessageDigest md = MessageDigest.GetInstance("SHA");
                //    md.Update(signature.ToByteArray());

                //    System.Diagnostics.Debug.WriteLine("************ Signature ******************");
                //    System.Diagnostics.Debug.WriteLine(Convert.ToBase64String(md.Digest()));
                //}
            }
            catch (NoSuchAlgorithmException e)
            {
                System.Diagnostics.Debug.WriteLine(e);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent intent)
        {
            base.OnActivityResult(requestCode, resultCode, intent);
            FacebookClientManager.OnActivityResult(requestCode, resultCode, intent);
        }
        public bool IsPlayServicesAvailable()
        {
            int resultCode = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(this); if (resultCode != ConnectionResult.Success)
            {
                if (GoogleApiAvailability.Instance.IsUserResolvableError(resultCode))
                    System.Diagnostics.Debug.WriteLine(GoogleApiAvailability.Instance.GetErrorString(resultCode));
                else
                {
                    //This device is not supported           
                    Finish(); // Kill the activity if you want.         
                }
                return false;
            }
            else
            {
                //Google Play Services is available.         
                return true;
            }
        }
        void CreateNotificationChannel()
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            {
                // Notification channels are new in API 26 (and not a part of the
                // support library). There is no need to create a notification 
                // channel on older versions of Android.
                return;
            }

            var channel = new NotificationChannel(CHANNEL_ID, "FCM Notifications", NotificationImportance.Default)
            {
                Description = "Firebase Cloud Messages appear in this channel"
            };

            var notificationManager = (NotificationManager)GetSystemService(NotificationService);
            notificationManager.CreateNotificationChannel(channel);
        }
    }


}