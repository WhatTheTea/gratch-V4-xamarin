using System;
using System.Collections.Generic;
using System.Text;

namespace gratch
{
    public class PageChangingEventArgs : EventArgs
    {
        //<summary>
        //Hello World   
        //</summary>
        public PageChangingEventArgs(Xamarin.Forms.Page targetpage)
        {
            TargetPage = targetpage;
        }

        public Xamarin.Forms.Page TargetPage { get; set; }
    }
}
