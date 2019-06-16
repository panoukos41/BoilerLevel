using Android;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Views;
using Android.Widget;
using BoilerLevel.Controls;
using BoilerLevel.Models;
using BoilerLevel.ViewModels;
using OpenExtensions.Droid.FragmentNavigation;
using OpenExtensions.Droid.Services;
using OxyPlot.Xamarin.Android;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace BoilerLevel.Views
{
    public class BoilerDetails : NavigationFragment
    {
        private BoilerDetailsViewModel VM;
        private PlotView PlotView;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            HasOptionsMenu = true;
            VM = new BoilerDetailsViewModel((Boiler)Nav_parameter);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var rootView = inflater.Inflate(Resource.Layout.fragment_boiler, container, false);
            PlotView = rootView.FindViewById<PlotView>(Resource.Id.PlotView);

            rootView.FindViewById<FloatingActionButton>(Resource.Id.AddMeasurement).Click += async (s, e) =>
            {
                var dialog = new CreateMeasurementDialog();
                var result = await dialog.ShowAsync(ChildFragmentManager, VM.Boiler);
                VM.AddMeasurmentCommand.Execute(result);
            };

            return rootView;
        }

        public override async void OnStart()
        {
            base.OnStart();
            if (PlotView.Model == null)
                PlotView.Model = await VM.InitiatePlotModel();
        }

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            inflater.Inflate(Resource.Menu.boiler_details_menu, menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            var id = item.ItemId;
            switch (id)
            {
                case Resource.Id.TestMeasurments:
                    AddTestMeasurments();
                    return true;

                case Resource.Id.EditTemplate:
                    _ = AddTemplateAsync();
                    return true;

                case Resource.Id.ShareExcel:
                    ShareExcel();
                    return true;

                case Resource.Id.SaveExcel:
                    SaveExcel();
                    return true;

                default:
                    return base.OnOptionsItemSelected(item);
            }

            void AddTestMeasurments()
            {
                for (int i = 0; i < 40; i++)
                {
                    var measur = new Measurment(VM.Boiler.Id)
                    {
                        DateTime = System.DateTime.Now.AddDays(i),
                        Temperature = 20 + i,
                        Level = 130 - (i * 2)
                    };

                    if (VM.Boiler.Template != null)
                    {
                        int a = 0;
                        var values = new Dictionary<string, float>();
                        foreach (var key in VM.Boiler.Template)
                        {
                            values[key] = 5 + a + (2 * i);
                            a++;
                        }
                        measur.Values = values;
                    }
                    VM.AddMeasurmentCommand.Execute(measur);
                }
            }

            async Task AddTemplateAsync()
            {
                var dialog = new CreateTemplateDialog();
                var result = await dialog.ShowAsync(ChildFragmentManager, VM.Boiler);
                VM.AddTemplateCommand.Execute(result);
            }

            async void ShareExcel()
            {
                ShareFile file = new ShareFile(await VM.ShareExcelFile(VM.Boiler.Name));

                await Share.RequestAsync(new ShareFileRequest
                {
                    Title = "Share",
                    File = file
                });
            }

            async void SaveExcel()
            {
                var permissionService = new PermissionService(Activity);
                if (!permissionService.HasPermission(Manifest.Permission.WriteExternalStorage))
                {
                    permissionService.RequestPermission(Manifest.Permission.WriteExternalStorage);
                }

                if (!permissionService.HasPermission(Manifest.Permission.WriteExternalStorage))
                    Toast.MakeText(Context, "Can't save excel without permission.", ToastLength.Long).Show();
                else
                    await Save();

                async Task Save()
                {
                    var envpath = Environment.GetExternalStoragePublicDirectory(Environment.DirectoryDocuments).AbsolutePath;
                    var fileName = VM.Boiler.Name;
                    var path = Path.Combine(envpath, fileName + ".xlsx");

                    int tries = 1;
                    while (File.Exists(path))
                    {
                        fileName = $"{VM.Boiler.Name}_{tries++}";
                        path = Path.Combine(envpath, fileName + ".xlsx");
                    }
                    var fileStream = File.Create(path);

                    await VM.SaveExcelFile(fileStream);
                    Toast.MakeText(Context, $"Saved at: {Environment.DirectoryDocuments} \nWith name: {fileName}", ToastLength.Long).Show();
                }
            }
        }
    }
}