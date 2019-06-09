using Android.Content;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;
using BoilerLevel.Models;
using System;
using System.Threading.Tasks;

namespace BoilerLevel.Controls
{
    public class CreateBoilerDialog : DialogFragment
    {
        private TextInputEditText NameEditText;
        private TextInputLayout NameTextInputLayout;

        private TaskCompletionSource<Boiler> completionSource = new TaskCompletionSource<Boiler>();

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var rootView = inflater.Inflate(Resource.Layout.control_CreateBoilerDialog, container, false);

            NameEditText = rootView.FindViewById<TextInputEditText>(Resource.Id.EditText);
            NameTextInputLayout = rootView.FindViewById<TextInputLayout>(Resource.Id.TextInputLayout);

            rootView.FindViewById<Button>(Resource.Id.CreateButton).Click += CreateButton_Click;
            rootView.FindViewById<Button>(Resource.Id.CancelButton).Click += (s, e) => Dismiss();

            return rootView;
        }

        public Task<Boiler> ShowAsync(FragmentManager manager)
        {
            ShowNow(manager, "CreateBoilerDialog");
            return completionSource.Task;
        }

        public override void OnDismiss(IDialogInterface dialog)
        {
            base.OnDismiss(dialog);
            completionSource.TrySetResult(null);
        }

        private void CreateButton_Click(object sender, EventArgs e)
        {
            if (NameEditText.Text == "" || NameEditText.Text == null)
                NameTextInputLayout.Error = GetString(Resource.String.EmptyName);
            else
            {
                Dismiss();
                completionSource.TrySetResult(new Boiler(NameEditText.Text));
            }
        }
    }
}