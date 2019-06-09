using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.Widget;
using Android.Widget;
using BoilerLevel.Localization;
using BoilerLevel.Messages;
using BoilerLevel.Utils;
using BoilerLevel.Views;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Views;
using OpenExtensions.Android;
using OpenExtensions.Android.App;
using OpenExtensions.Android.FragmentNavigation;
using OpenExtensions.Android.Services;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Messenger = GalaSoft.MvvmLight.Messaging.Messenger;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace BoilerLevel
{
    [Activity(MainLauncher = true, ConfigurationChanges = ConfigChanges.Orientation)]
    public class Shell : FragmentNavigationActivity
    {
        public static INavigationService navigationService;

        public override Task OnLaunchAsync(Bundle savedInstanceState)
        {
            Platform.Init(this, savedInstanceState);
            ExperimentalFeatures.Enable(ExperimentalFeatures.ShareFileRequest);

            BoilerManager.Initialize();
            MeasurementManager.Initialize();

            Messenger.Default.Register<TitleMessage>(this, (message) => SupportActionBar.Title = message.Title);
            Messenger.Default.Register<NotificationMessage>(this, (notification) =>
            {
                Toast
                    .MakeText(this, notification.Notification, ToastLength.Long)
                    .Show();
            });

            SetupDrawerView();

            navigationService = NavigationService;
            NavigationService.NavigateTo("Home");

            return Task.CompletedTask;
        }

        private void SetupDrawerView()
        {
            FindViewById<TextView>(Resource.Id.HeaderText).Text =
                $"-{CodeResources.OverviewUntilNow}- \n" +
                $"{CodeResources.Boilers}: {BoilerManager.Count} \n" +
                $"{CodeResources.Measurments}: {MeasurementManager.Count}";

            var themeSwitch = FindViewById<Switch>(Resource.Id.ThemeSwitch);
            themeSwitch.Text = CodeResources.DarkTheme;
            themeSwitch.Checked = ThemeService.IsDarkTheme();
            themeSwitch.Click += (s, e) =>
            {
                new AlertDialog.Builder(this)
                .SetTitle(CodeResources.Restart)
                .SetMessage(CodeResources.RestartRequired)
                .SetPositiveButton(CodeResources.Yes, (s1, e1) =>
                {
                    ThemeService.SetTheme(themeSwitch.Checked ? ThemeService.Theme.Dark : ThemeService.Theme.Light);
                    RestartAsync();
                })
                .SetNegativeButton(CodeResources.No, (s1, e1) => themeSwitch.Checked = !themeSwitch.Checked)
                .Create()
                .Show();
            };
        }

        #region overrides

        public override int ShellLayout => Resource.Layout.activity_shell;
        public override int NavigationServiceFrame => Resource.Id.contentFrame;
        public override Toolbar Toolbar => FindViewById<Toolbar>(Resource.Id.toolbar);
        public override DrawerLayout Drawer => FindViewById<DrawerLayout>(Resource.Id.DrawerLayout);

        public override Android.Support.V4.App.Fragment OnCreateNavigationService()
        {
            var nav = new FragmentNavigationService()
                .Configure("Home", typeof(Home))
                .Configure("BoilerDetails", typeof(BoilerDetails));

            nav.SetAnimations(
                R.Animation.enter_from_right,
                R.Animation.exit_to_left,
                R.Animation.enter_from_left,
                R.Animation.exit_to_right);

            return nav;
        }

        public override void SetTheme()
        {
            ThemeService.Initialize(this, R.Style.DarkTheme, R.Style.LightTheme);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
        #endregion
    }
}

