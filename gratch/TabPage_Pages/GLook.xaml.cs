#define MSBOX
#define EXCEL
#undef MSBOX
#undef EXCEL

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;
using System.Xml;
using System.Xml.Linq;
using System.IO;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Xml.Serialization;
using System.Windows.Input;

namespace gratch
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class GLook : ContentPage
    {
        private string[] holidays = new string[7];
        public List<int> Groups = new List<int>();
        /*
                private readonly string[] Days =
                {
                    "Нд",
                    "Пн",
                    "Вт",
                    "Ср",
                    "Чт",
                    "Пт",
                    "Сб"
                };
        */
        public GLook()
        {
            InitializeComponent();
            this.BindingContext = this;
            grid1_start();
            if (Tools.is_graph) Start((int?)picker1.SelectedItem ?? 1);
        }
        public void FillGroups()
        {
            if (Tools.is_graph)
            {
                Groups.Clear();
                XmlDocument lDoc = new XmlDocument();
                lDoc.Load(Tools.lPath);
                for (int i = 1; i <= lDoc.DocumentElement.ChildNodes.Count; i++) Groups.Add(i);
            }
        }
        void grid1_start()
        {
            if (Tools.is_graph)
            {
                XmlDocument gDoc = new XmlDocument();
                gDoc.Load(Tools.gPath);
                if (grid1.RowDefinitions.Count <= 0)//grid1.RowDefinitions.Count == gDoc.DocumentElement.FirstChild.ChildNodes.Count
                {
                    for (int i = 1; i <= gDoc.DocumentElement.FirstChild.ChildNodes.Count; i++)
                    {
                        grid1.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0.12, GridUnitType.Star) });
                    }
                }
                else if (grid1.RowDefinitions.Count > gDoc.DocumentElement.FirstChild.ChildNodes.Count)
                {
                    for (int k = grid1.RowDefinitions.Count - gDoc.DocumentElement.FirstChild.ChildNodes.Count; k != 0; k--)
                    {
                        grid1.RowDefinitions.RemoveAt(k);
                    }
                }
                else
                {
                    for (int x = grid1.RowDefinitions.Count; x < gDoc.DocumentElement.FirstChild.ChildNodes.Count; x++)
                    {
                        grid1.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0.12, GridUnitType.Star) });
                    }
                }
            }
        }
        void grid1_add(string content, int col, int row)
        {
            Label buff = new Label
            {
                Text = content,
                Style = (Style)Application.Current.Resources["GLook_Label"]
            };
            //buff.GestureRecognizers.Add(new TapGestureRecognizer { NumberOfTapsRequired = 1, Command = TapCommand, CommandParameter = row });
            buff.GestureRecognizers.Add(new DragGestureRecognizer { CanDrag = true });
            buff.GestureRecognizers.Add(new DropGestureRecognizer { AllowDrop = true });
            grid1.Children.Add(buff, col, row);
        }
        /*ICommand TapCommand => new Command((object arg) =>
                                             {
                                                 PunishUnit(arg);
                                             });
        async public void PunishUnit(object ReturnedRow)
        {
            await DisplayAlert("lol", ReturnedRow.ToString(), "OK");
        }*/
        public void Start(int group)
        {
            holidays = Tools.hday_init();
            FillGroups();
            picker1.ItemsSource = Groups;

            XmlDocument gDoc = new XmlDocument();
            gDoc.Load(Tools.gPath);

            int i = 0;
            foreach (XmlNode node in gDoc.DocumentElement.ChildNodes)
            {
                if (node.ChildNodes.Count > 0)
                {
                    if (node.Name == $"group{group}")
                    {
                        foreach (XmlNode unit in node.ChildNodes)
                        {
                            if (Int32.Parse(unit.Attributes.GetNamedItem("day").InnerText) < 10)
                            {
                                grid1_add($"0{unit.Attributes.GetNamedItem("day").InnerText}", 0, i);
                                grid1_add(unit.InnerText, 1, i);
                            }
                            else
                            {
                                grid1_add(unit.Attributes.GetNamedItem("day").InnerText, 0, i);
                                grid1_add(unit.InnerText, 1, i);
                            }
                            i++;
                        }
                    }
                }
            }
        }
        private void GLook_Activated(object sender, EventArgs e)
        {
            if (Tools.is_graph)
            {
                grid1.Children.Clear();
                grid1_start();
                picker1.ItemsSource = null;
                Start((int?)picker1.SelectedItem ?? 1);
            }
        }
        private void picker1_changed(object sender, EventArgs e)
        {
            grid1.Children.Clear();
            if (Tools.is_graph) Start((int?)picker1.SelectedItem ?? 1);
        }

        private void Button_Clicked(object sender, EventArgs e) // стандарт
        {

        }

        private void Button_Clicked_1(object sender, EventArgs e) // Ексель
        {
            Tools.NPOI_Excel.CallExcelFunc();
            //Process.Start();
        }

    }
}