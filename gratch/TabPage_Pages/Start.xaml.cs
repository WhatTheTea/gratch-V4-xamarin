using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace gratch
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Start : ContentPage
    {
        ObservableCollection<int> Groups { get; set; }
        public Start()
        {
            InitializeComponent();
            Groups = new ObservableCollection<int>();
            this.BindingContext = this;
            FillGroups();
            init(1);
        }
        private void init(int group)
        {
            DateTime now = DateTime.Now;
            clear_empty();
            Entry1.Text = "[Список відсутній]";
            if (Tools.is_graph)
            {

                if (Tools.is_holiday(now.DayOfWeek))
                {
                    Label1.Text = "Сьогодні:";
                    Entry1.Text = "Вихідний день";
                }
                else
                {
                    XDocument xDoc = XDocument.Load(Tools.gPath);
                    try
                    {
                        foreach (XNode node in xDoc.Root.Elements($"group{group}"))
                        {
                            Entry1.Text = node.XPathSelectElement($"unit[@day='{now.Day}']").Value;
                        }
                    }
                    catch (NullReferenceException)
                    {
                        File.Delete(Tools.gPath);
                        File.Delete(Tools.dPath);
                        File.Delete(Tools.lPath);
                        //MessageBox.Show("Файл пошкоджено, графік було видалено", "Помилка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        DisplayAlert("Помилка!", "Файл пошкоджено, графік було видалено", "Ок");
                    }
                }
            }
        }
        private void clear_empty() //BugFix
        {
            if (Tools.is_graph)
            {
                XmlDocument gDoc = new XmlDocument();
                gDoc.Load(Tools.gPath);
                foreach (XmlNode node in gDoc.DocumentElement)
                    if (node.ChildNodes.Count == 0)
                    {
                        gDoc.DocumentElement.RemoveChild(node);
                        gDoc.Save(Tools.gPath);
                    }
            }
            if (Tools.is_list)
            {
                XmlDocument lDoc = new XmlDocument();
                lDoc.Load(Tools.lPath); //
                foreach (XmlNode node in lDoc.DocumentElement)
                    if (node.ChildNodes.Count == 0)
                    {
                        lDoc.DocumentElement.RemoveChild(node);
                        lDoc.Save(Tools.lPath);
                    }
            }
        }
        public void UpdateGraf()
        {
            XmlDocument gDoc = new XmlDocument();
            XmlDocument lDoc = new XmlDocument();
            gDoc.Load(Tools.gPath);
            lDoc.Load(Tools.lPath);
            int gDocInt = gDoc.DocumentElement.ChildNodes.Count;
            int[] updst = new int[gDocInt];
            for (int k = 1; k < gDocInt + 1; k++)
            {
                foreach (XmlNode node in lDoc.DocumentElement.SelectNodes($"group{k}/*"))
                {
                    if (gDoc.DocumentElement.SelectSingleNode($"group{k}").LastChild.InnerText == node.InnerText)
                    {
                        updst[k - 1] = int.Parse(node.SelectSingleNode("@id").Value) + 1;
                        break;
                    }
                }
            }
            RedactG.CreateGraph(updst);
        }
        public void UpdGraphInit()
        {
            XmlDocument gDoc = new XmlDocument();
            gDoc.Load(Tools.gPath);
            XmlNode dateNode = gDoc.DocumentElement.SelectSingleNode("date");
            string month = DateTime.Now.Month.ToString();
            if (dateNode.InnerText != month)
            {
                UpdateGraf();
                dateNode.InnerText = month;
                gDoc.Save(Tools.gPath); //
                //new GLook().CallExcelFunc();
                //MessageBox.Show("Графік оновлено!", "Інфо", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DisplayAlert("Інфо", "Графік оновлено, гарного дня", "Ок");
            }
        }

        public void FillGroups()
        {
            if (Tools.is_graph)
            {
                XmlDocument lDoc = new XmlDocument();
                lDoc.Load(Tools.lPath);
                for (int i = 1; i <= lDoc.DocumentElement.ChildNodes.Count; i++) Groups.Add(i);
            }
            Picker1.ItemsSource = null;
            Picker1.ItemsSource = Groups;
            Picker1.SelectedIndex = 0;
        }
        void Picker1_ValChanged(object sender, EventArgs e)
        {
            init((int?)Picker1.SelectedItem ?? 1);
        }

        void Start_Appear(object sender, EventArgs e)
        {
            Groups = new ObservableCollection<int>();
            this.BindingContext = this;
            FillGroups();
            init(1);
        }
    }
}