using Android.Content;
using Android.OS;
using Android.Support.V4.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using BoilerLevel.Models;
using GalaSoft.MvvmLight.Helpers;
using OpenExtensions.Droid;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BoilerLevel.Controls
{
    public class CreateTemplateDialog : DialogFragment
    {
        #region Properties

        private RecyclerView recycler;
        private ObservableRecyclerAdapter<string, CachingViewHolder> recyclerAdapter;
        private Boiler Boiler;

        private List<string> template = new List<string>();
        private List<string> newTemplate = new List<string>();
        private readonly TaskCompletionSource<List<string>> completionSource = new TaskCompletionSource<List<string>>();
        #endregion

        #region overrides

        public override int Theme => Shell.ThemeService.IsDarkTheme() ? R.Style.DarkTheme_FullScreen_Dialog : R.Style.LightTheme_FullScreen_Dialog;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            template.Add(GetString(Resource.String.Temperature));
            template.Add(GetString(Resource.String.Level));
            template.AddRange(Boiler.Template ?? new List<string>());

            recyclerAdapter = template.GetRecyclerAdapter(BindViewHolder, Resource.Layout.adapter_control_CreateTemplateDialog);

            var rootView = inflater.Inflate(Resource.Layout.control_CreateTemplateDialog, container, false);

            recycler = rootView.FindViewById<RecyclerView>(Resource.Id.recyclerView);
            recycler.SetLayoutManager(new LinearLayoutManager(Context));
            recycler.SetAdapter(recyclerAdapter);

            rootView.FindViewById<Button>(Resource.Id.SaveButton).Click += SaveButton;
            rootView.FindViewById<Button>(Resource.Id.AddButton).Click += (s, e) =>
            {
                template.Add("");
                recyclerAdapter.NotifyItemInserted(template.Count - 1);
            };
            rootView.FindViewById<Button>(Resource.Id.CancelButton).Click += (s, e) => Dismiss();

            return rootView;
        }

        public override void OnDismiss(IDialogInterface dialog)
        {
            base.OnDismiss(dialog);
            completionSource.TrySetResult(null);
        }
        #endregion

        public Task<List<string>> ShowAsync(FragmentManager manager, Boiler boiler)
        {
            Boiler = boiler;
            Show(manager, "CreateTemplateDialog");
            return completionSource.Task;
        }

        private void BindViewHolder(CachingViewHolder holder, string item, int index)
        {
            var button = holder.FindCachedViewById<ImageView>(Resource.Id.DeleteButton);
            var EditTextView = holder.FindCachedViewById<EditText>(Resource.Id.EditTextView);

            if (index == 0 || index == 1)
            {
                EditTextView.Text = item;
                EditTextView.Enabled = false;
                button.Enabled = false;
            }
            else if (item != null && item != "")
            {
                EditTextView.Text = item;
                EditTextView.Enabled = false;
            }
            else
            {
                EditTextView.Text = "";
                EditTextView.Hint = GetString(Resource.String.your_measurment_name_id);
                EditTextView.Enabled = true;
            }

            if (!button.HasOnClickListeners)
            {
                button.Click += (s, e) =>
                {
                    template.RemoveAt(holder.AdapterPosition);
                    recyclerAdapter.NotifyItemRemoved(holder.AdapterPosition);
                };
            }
        }

        private void SaveButton(object s, object e)
        {
            bool success = true;

            for (int i = 2; i < template.Count; i++)
            {
                var view = recycler.GetChildAt(i);
                template[i] = view.FindViewById<EditText>(Resource.Id.EditTextView).Text;
            }

            for (int i = 2; i < template.Count; i++)
            {
                var view = recycler.GetChildAt(i);
                var editText = view.FindViewById<EditText>(Resource.Id.EditTextView);
                if (editText.Text == "" || editText.Text == null)
                {
                    success = false;
                    editText.Error = GetString(Resource.String.EmptyValue);
                }
                else if (template.Where(x => x == editText.Text).Count() > 1)
                {
                    success = false;
                    editText.Error = GetString(Resource.String.NameExists);
                }
                else
                {
                    editText.Error = null;
                    newTemplate.Add(editText.Text);
                }
            }

            if (success)
            {
                completionSource?.TrySetResult(newTemplate);
                Dismiss();
            }
            else
                newTemplate.Clear();
        }
    }
}