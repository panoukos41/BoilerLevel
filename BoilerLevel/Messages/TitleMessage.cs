using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace BoilerLevel.Messages
{
    public class TitleMessage
    {
        public TitleMessage() { }

        public TitleMessage(string title)
        {
            Title = title;
        }

        public string Title { get; set; }
    }
}