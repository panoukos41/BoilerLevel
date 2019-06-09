using Android.Content;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using BoilerLevel.Models;
using GalaSoft.MvvmLight.Helpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BoilerLevel.Controls
{
    public class CreateMeasurementDialog : DialogFragment
    {
        #region Properties

        private Boiler Boiler { get; set; }
        private List<string> Measurments = new List<string>();
        private bool searchedForTemp = true;

        private RecyclerView recycler;
        private ObservableRecyclerAdapter<string, CachingViewHolder> recyclerAdapter;

        private TaskCompletionSource<Measurment> completionSource = new TaskCompletionSource<Measurment>();
        #endregion

        #region overrides

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var rootView = inflater.Inflate(Resource.Layout.control_CreateMeasurmentDialog, container, false);

            recycler = rootView.FindViewById<RecyclerView>(Resource.Id.recyclerView);
            recycler.SetLayoutManager(new LinearLayoutManager(Context));
            recycler.SetAdapter(recyclerAdapter);

            rootView.FindViewById<Button>(Resource.Id.AddButton).Click += AddButton_Click;
            rootView.FindViewById<Button>(Resource.Id.CancelButton).Click += (s, e) => Dismiss();

            return rootView;
        }

        public override void OnDismiss(IDialogInterface dialog)
        {
            base.OnDismiss(dialog);
            completionSource.TrySetResult(null);
        }
        #endregion

        public Task<Measurment> ShowAsync(FragmentManager manager, Boiler boiler)
        {
            Boiler = boiler;

            Measurments.Add("");
            Measurments.Add("");
            for (int i = 0; i < Boiler.Template?.Count; i++)
            {
                Measurments.Add("");
            }

            recyclerAdapter = Measurments.GetRecyclerAdapter(BindViewHolderDelegate, Resource.Layout.adapter_control_CreateMeasurement);

            ShowNow(manager, "CreateMeasurementDialog");
            return completionSource.Task;
        }

        private void BindViewHolderDelegate(CachingViewHolder holder, string item, int index)
        {
            switch (index)
            {
                case 0:
                    holder.FindCachedViewById<TextInputLayout>(Resource.Id.ValueInputLayout).Hint = GetString(Resource.String.Temperature);
                    break;
                case 1:
                    holder.FindCachedViewById<TextInputLayout>(Resource.Id.ValueInputLayout).Hint = GetString(Resource.String.Level);
                    break;
                default:
                    holder.FindCachedViewById<TextInputLayout>(Resource.Id.ValueInputLayout).Hint = Boiler.Template[index - 2];
                    break;
            }
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            bool success = true;

            for (int i = 0; i < Measurments.Count; i++)
            {
                var view = recycler.FindViewHolderForAdapterPosition(i);
                var inputLayout = ((CachingViewHolder)view).FindCachedViewById<TextInputLayout>(Resource.Id.ValueInputLayout);
                var editText = ((CachingViewHolder)view).FindCachedViewById<TextInputEditText>(Resource.Id.ValueEditText);
                if (editText.Text == "" || editText.Text == null)
                {
                    inputLayout.Error = "Field can't be empty!";
                    success = false;
                }
                else if (!float.TryParse(editText.Text, out _))
                {
                    inputLayout.Error = "Field can only be number!";
                    success = false;
                }
                else
                {
                    inputLayout.Error = null;
                    Measurments[i] = editText.Text;
                }
            }

            if (success)
            {
                var values = new Dictionary<string, float>();

                for (int i = 0; i < Measurments.Count - 2; i++)
                    values[Boiler.Template[i]] = float.Parse(Measurments[i + 2]);

                var measurement = new Measurment(Boiler.Id)
                {
                    Temperature = float.Parse(Measurments[0]),
                    Level = float.Parse(Measurments[1]),
                    Values = values
                };

                Dismiss();
                completionSource.TrySetResult(measurement);
            }
        }
    }
}