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
        public static ObservableCollection<Page> Pages = new ObservableCollection<Page> { new RedactG(), new Start(), new GLook() };
        public MainPage()
        {
            InitializeComponent();
            MainPageRefresh(new object(), new EventArgs());
            CurrentPage = Children[1];
            Pages.CollectionChanged += MainPageRefresh;
        }

        public void MainPageRefresh(object sender, EventArgs e) // sender = TabIndex, e = new EventArgs();
        {
            Children.Clear();
            for (int i = 0; i < Pages.Count; i++) Children.Add(Pages[i]);
            this.CurrentPage = Children[CurrentPage.TabIndex];
        }


    }
}
