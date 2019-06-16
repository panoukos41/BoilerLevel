using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using BoilerLevel.Controls;
using BoilerLevel.Localization;
using BoilerLevel.Messages;
using BoilerLevel.Models;
using BoilerLevel.ViewModels;
using GalaSoft.MvvmLight.Helpers;
using OpenExtensions.Droid.FragmentNavigation;
using Messenger = GalaSoft.MvvmLight.Messaging.Messenger;

namespace BoilerLevel.Views
{
    public class Home : NavigationFragment
    {
        private readonly HomeViewModel VM = new HomeViewModel(Shell.navigationService);
        private RecyclerView HomeRecycler;
        private ObservableRecyclerAdapter<Boiler, CachingViewHolder> HomeAdapter;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            HomeAdapter = VM.Boilers.GetRecyclerAdapter(
                HolderDelegate,
                Resource.Layout.adapter_home,
                (oldIndex, oldView, newIndex, newView) => VM.GoToBoilersDetail(VM.Boilers[newIndex]));
        }

        private void HolderDelegate(CachingViewHolder holder, Boiler item, int index)
        {
            holder.FindCachedViewById<TextView>(Resource.Id.NameTextView).Text = item.Name;
            holder.FindCachedViewById<TextView>(Resource.Id.MetaDataTextView).Text =
                $"{GetString(Resource.String.NumberOfMeasurments)} {item.Count}\n" +
                $"{GetString(Resource.String.CreatedAt)} {item.DateCreated.ToShortDateString()}";

            var button = holder.FindCachedViewById<ImageView>(Resource.Id.DeleteButton);
            if (!button.HasOnClickListeners)
                button.Click += (s, e) =>
                {
                    var position = holder.AdapterPosition;
                    new AlertDialog.Builder(Context)
                    .SetTitle(GetString(Resource.String.Delete))
                    .SetMessage($"{GetString(Resource.String.DeleteBoilerQ)} {VM.Boilers[position].Name}?")
                    .SetPositiveButton(GetString(Resource.String.Yes), (s1, e1) => VM.DeleteBoiler(VM.Boilers[position]))
                    .SetNegativeButton(GetString(Resource.String.No), (s1, e1) => { })
                    .Create()
                    .Show();
                };
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            Messenger.Default.Send(new TitleMessage(CodeResources.Home));
            var rootView = inflater.Inflate(Resource.Layout.fragment_home, container, false);

            HomeRecycler = rootView.FindViewById<RecyclerView>(Resource.Id.HomeRecycler);
            HomeRecycler.SetLayoutManager(new LinearLayoutManager(container.Context));
            HomeRecycler.SetAdapter(HomeAdapter);

            rootView.FindViewById<FloatingActionButton>(Resource.Id.AddBoilerFab).Click += async (s, e) =>
            {
                var createDialog = new CreateBoilerDialog();
                var result = await createDialog.ShowAsync(ChildFragmentManager);
                VM.AddBoiler.Execute(result);
            };

            return rootView;
        }
    }
}