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
        public static ObservableCollection<Page> Pages = new ObservableCollection<Page> { new GLook(), new Start(), new RedactG() };
        public MainPage()
        {
            InitializeComponent();
            MainPageRefresh();
            RedactDays.PageChanging += RedactDays_PageChanging;
            RedactG.PageChanging += RedactG_PageChanging;

        }

        private void RedactG_PageChanging(object page, EventArgs e)
        {
            Pages[2] = new RedactDays();
            MainPageRefresh((int)page);
        }

        private void RedactDays_PageChanging(object page, EventArgs e)
        {
            Pages[2] = new RedactG();
            MainPageRefresh((int)page);
        }
        public void MainPageRefresh(int page = 1)
        {
            Children.Clear();
            for (int i = 0; i < Pages.Count; i++) Children.Add(Pages[i]);
            CurrentPage = Children[page];
        }


    }
}
