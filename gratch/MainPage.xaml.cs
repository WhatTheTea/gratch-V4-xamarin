using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

using Xamarin.Forms;

namespace gratch
{
    public partial class MainPage : TabbedPage
    {
        public static Collection<Page> Pages = new() { new GLook(), new Start(), new RedactG() };
        public MainPage()
        {
            InitializeComponent();
            InitializePages();
        }
        private void InitializePages()
        {
            foreach (var page in Pages) Children.Add(page); // Init Pages
            CurrentPage = Children[1];

            RedactG.PageChanged += PageChanged;
            RedactDays.PageChanged += PageChanged;
        }

        public void ChildrenRefresh()
        {
            Children.Clear();
            for (int i = 0; i < Pages.Count; i++) Children.Add(Pages[i]);
        }

        public void PageChanged(object sender, PageChangingEventArgs args) // sender = from, args = where
        {
            Pages[2] = args.TargetPage;
            ChildrenRefresh();
            CurrentPage = Children[Pages.IndexOf(args.TargetPage)];
            
        }


    }
}
