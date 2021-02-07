#define DBG
#undef DBG
using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Collections.ObjectModel;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace gratch
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RedactG : ContentPage
    {
        public ObservableCollection<string> Units { get; set; }
        public Collection<string> GraphWindows { get; set; }
        private static DateTime Now => DateTime.Now; //:DDDD
        private string[] holidays = new string[7];
        private int _UnitId = 1, maxUnit = 0;
        public static event EventHandler PageChanging;
        public RedactG()
        {
            InitializeComponent();

            GraphWindows = new Collection<string> { "Редактор списку чергових", "Редактор вихідних" };

            Start();
        }
        private void Start()
        {
            Units = new ObservableCollection<string>();
            WindowPicker.ItemsSource = GraphWindows;
            WindowPicker.SelectedIndex = 0;

            this.BindingContext = this;

            holidays = Tools.hday_init();
            Step.Maximum = 2;

            int group = (int)Step.Value;
            Units.Clear();
            if (Tools.is_list)
            {
                XDocument xlDoc = XDocument.Load(Tools.lPath);
                XmlDocument lDoc = new XmlDocument();
                lDoc.Load(Tools.lPath);
                foreach (XmlNode node in lDoc.DocumentElement.ChildNodes)
                {
                    if (node.ChildNodes.Count > 0)
                    {
                        if (Tools.is_graph)
                        {
                            XmlDocument gDoc = new XmlDocument();
                            gDoc.Load(Tools.gPath);
                            //numericUpDown1.Maximum = gDoc.DocumentElement.ChildNodes.Count;
                            Step.Maximum = gDoc.DocumentElement.ChildNodes.Count;
                        }

                        if (node.LocalName == $"group{group}")
                        {
                            foreach (XElement unit in xlDoc.Root.XPathSelectElements($"//group{group}/unit"))
                            {
                                Units.Add(unit.Value);
                                maxUnit++;
                            }
                        }
                    }
                }
                _UnitId += maxUnit;
            }
        }
        public void OnKeyboardComplete1(object sender, EventArgs e)
        {
            AddUnit();
        }
        public void Btn2Clicked(object sender, EventArgs e)
        {
            ClearFiles();
        }

        static public void CreateGraph(int[] global_UnitId = null, int step = 1) // Чёрный ящик
        {
            XDocument xgDoc = new XDocument(new XElement($"groups"));
            XmlDocument lDoc = new XmlDocument();
            int group = 1;
            int dayinmon_now = DateTime.DaysInMonth(Now.Year, Now.Month);
            lDoc.Load(Tools.lPath);
            for (int k = 0; k < lDoc.DocumentElement.ChildNodes.Count; k++, group++)
            {
                xgDoc.Root.Add(new XElement($"group{group}"));
                for (int i = 1, UnitId = global_UnitId == null ? 1 : global_UnitId[k]; i <= dayinmon_now; i++, UnitId++)
                {
                    foreach (XmlNode node in lDoc.DocumentElement.SelectNodes($"group{group}"))
                    {
                        if (node.ChildNodes.Count > 0)
                        {
                            if (node.SelectSingleNode($"unit[@id='{UnitId}']") != null)
                            {
                                if (!Tools.is_holiday(Tools.dayweek(i)))
                                {
                                    XmlNode unit = node.SelectSingleNode($"unit[{UnitId}]");
                                    XElement xUnit = new XElement("unit",
                                        new XAttribute("day", i),
                                        unit.InnerText
                                        );
                                    xgDoc.Root.Element($"group{group}").Add(xUnit);
                                }
                                else
                                {
                                    UnitId--;
                                }
                            }
                            else
                            {
                                UnitId = 0; i--; //Если _UnitId не 1, бесконечный цикл :V // Пофиксил
                            }
                        }
                    }
                }
            }
            xgDoc.Root.Add(new XElement("date", Now.Month));
            group = (int)step;
            Tools.SaveHidden(Tools.gPath, xgDoc);
        }

        public void ImportGraph(StreamReader list)
        {
            if (list == null) throw new ArgumentNullException(nameof(list));
            XDocument xlDoc;
            XElement UnitElem, GroupElem;
            XmlDocument lDoc = new XmlDocument();
            int group = (int)Step.Value;
            for (; !list.EndOfStream;)
            {
                string unit = list.ReadLine();
                if (Tools.is_list)
                {
                    var xlDoc_copy = new XmlDocument();
                    xlDoc_copy.Load(Tools.lPath);
                    xlDoc = XDocument.Load(Tools.lPath);
                    UnitElem = new XElement(
                        new XElement("unit",
                        new XAttribute("id", _UnitId++),
                        unit
                        ));
                    GroupElem = new XElement($"group{group}");
                    bool is_new_grp = true;
                    foreach (XmlNode node in xlDoc_copy.DocumentElement.ChildNodes)
                    {
                        if (node.ChildNodes.Count > 0 && node.LocalName == $"group{group}")
                        {
                            is_new_grp = false;
                        }
                    }
                    if (is_new_grp)
                    {
                        xlDoc.Root.Add(GroupElem);
                    }
                    xlDoc.Root.Element($"group{group}").Add(UnitElem);
                    Units.Add(unit);
                    Tools.SaveHidden(Tools.lPath, xlDoc);
                    CreateGraph(null,(int)Step.Value);
                }
                else
                {
                    xlDoc = new XDocument(new XElement($"units",
                        new XElement($"group{group}",
                        new XElement("unit",
                        new XAttribute("id", _UnitId++),
                        unit))));
                    xlDoc.Save(Tools.lPath);
                    Units.Add(unit);
                }
                lDoc.Load(Tools.lPath);
                foreach (XmlNode node in lDoc.DocumentElement.ChildNodes)
                {
                    maxUnit = node.ChildNodes.Count;
                    for (int i = 1; i <= maxUnit; i++)
                    {
                        if (node.ChildNodes.Count > 0)
                        {
                            if (node.LocalName == $"group{group}")
                            {
                                node.SelectSingleNode($"unit[{i}]/@id").InnerText = i.ToString();
                            }
                        }
                    }
                }
                _UnitId = maxUnit + 1;
                Tools.SaveHidden(Tools.lPath, lDoc);
            }
        }
        void AddUnit()
        {
            if (Entry1.Text.Length > 1)
            {
                int group = (int)Step.Value;
                XDocument xlDoc;
                XElement UnitElem, GroupElem;
                XmlDocument lDoc_copy = new XmlDocument();
                if (Tools.is_list)
                {
                    xlDoc = XDocument.Load(Tools.lPath);
                    UnitElem = new XElement(
                        new XElement("unit",
                        new XAttribute("id", _UnitId++),
                        Entry1.Text
                        ));
                    GroupElem = new XElement($"group{group}");
                    var lDoc = new XmlDocument();
                    bool is_new_grp = true;
                    lDoc.Load(Tools.lPath);
                    foreach (XmlNode node in lDoc.DocumentElement.ChildNodes)
                    {
                        if (node.ChildNodes.Count > 0)
                        {
                            if (node.LocalName == $"group{group}")
                            {
                                is_new_grp = false;
                            }
                        }
                    }
                    if (is_new_grp) xlDoc.Root.Add(GroupElem);

                    xlDoc.Root.Element($"group{group}").Add(UnitElem);
                    Units.Add(Entry1.Text);
                    Tools.SaveHidden(Tools.lPath, xlDoc);
                    CreateGraph(null, (int)Step.Value);
                }
                else
                {
                    xlDoc = new XDocument(new XElement($"units",
                        new XElement($"group{group}",
                        new XElement("unit",
                        new XAttribute("id", _UnitId++),
                        Entry1.Text))));
                    Tools.SaveHidden(Tools.lPath, xlDoc);
                    Units.Add(Entry1.Text);
                    CreateGraph(null, (int)Step.Value);
                }

                lDoc_copy.Load(Tools.lPath);
                foreach (XmlNode node in lDoc_copy.DocumentElement.ChildNodes)
                {
                    maxUnit = node.ChildNodes.Count;
                    for (int i = 1; i <= maxUnit; i++)
                    {
                        if (node.ChildNodes.Count > 0)
                        {
                            if (node.LocalName == $"group{group}")
                            {
                                node.SelectSingleNode($"unit[{i}]/@id").InnerText = i.ToString();
                            }
                        }
                    }
                }
                _UnitId = maxUnit + 1;
                Tools.SaveHidden(Tools.lPath, lDoc_copy);
            }
            else
            {
                DisplayAlert("Помилка", "Ім'я не може бути менше 2 символів", "Ок");
            }
            Entry1.Text = null;

            if (Tools.is_graph)
            {
                XmlDocument gDoc = new XmlDocument();
                gDoc.Load(Tools.gPath);
                Step.Maximum = gDoc.DocumentElement.ChildNodes.Count;
            }
            else
            {
                Step.Maximum = 2;
            }
            new gratch.Start().FillGroups();
        }
        async void ClearFiles()
        {
            bool Alert_DisplayAlert = await DisplayAlert("Видалення", 
                "Бажаєте видалити графік?\nПримітка: Список чергових та вихідних також буде видалено", 
                "Так", 
                "Ні");
            if (Alert_DisplayAlert)
            {
                if (Tools.is_list) File.Delete(Tools.lPath);
                if (Tools.is_graph) File.Delete(Tools.gPath);
                if (Tools.is_days) File.Delete(Tools.dPath);
                Start();
                _UnitId = 1;
                maxUnit = 0;

                list1.ItemsSource = null;
                list1.ItemsSource = Units;
                new gratch.Start().FillGroups();
#if DBG
                await DisplayAlert("List", Tools.is_list.ToString(), "Ок");
                await DisplayAlert("Graph", Tools.is_graph.ToString(), "Ок");
                await DisplayAlert("Days", Tools.is_days.ToString(), "Ок");
#endif
                await DisplayAlert("Інфо", "Графік було успішно видалено", "Ок");
            }
        }
        void Step_ValueChanged(object sender, EventArgs e)
        {
            UpdPicker1();

            Entry2.Text = $"Група чергових №{Step.Value}";
        }

        private void Button_Clicked(object sender, EventArgs e)
        {

        }

        private void WindowPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
#if DBG
            DisplayAlert(WindowPicker.SelectedItem.ToString(), WindowPicker.SelectedIndex.ToString(), "OK");
#endif
            switch (WindowPicker.SelectedIndex)
            {
                case 0:
                    //do nothing
                    break;
                case 1:
                    PageChanging.Invoke(2, new EventArgs());
                    break;
                default:
                    throw new ArgumentException("Invalid WindowPicker index");
            }
        }
        
        async void UpdPicker1()
        {
            if (Units != null)
            {
                XmlDocument gDoc = new XmlDocument();
                // SAME IN START
                if (Tools.is_graph)
                {
                    gDoc.Load(Tools.gPath);
                    //numericUpDown1.Maximum = gDoc.DocumentElement.ChildNodes.Count;
                    Step.Maximum = gDoc.DocumentElement.ChildNodes.Count;
                }
                else
                {//numericUpDown1.Maximum = 2;
                    Step.Maximum = 2;
                }
                //SAME IN START
                int group = (int)Step.Value;
                //listBox1.Items.Clear();
                Units.Clear();

                if (!Tools.is_list)
                {
                    if (Step.Value == 1)
                    {
                        //MessageBox.Show("Треба створити список учнів", "Інфо",
                        //    MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await DisplayAlert("Інфо", "Треба створити список учнів", "Ок"); // ??????????????
                    }
                    Step.Value = 1;
                }
                else
                {
                    XDocument xlDoc = XDocument.Load(Tools.lPath);
                    var lDoc = new XmlDocument();
                    lDoc.Load(Tools.lPath);
                    _UnitId = 1;
                    maxUnit = 0;
                    foreach (XmlNode node in lDoc.DocumentElement.ChildNodes)
                    {
                        if (node.ChildNodes.Count > 0)
                        {
                            if (node.LocalName == $"group{group}")
                            {
                                foreach (XElement it in xlDoc.Root.XPathSelectElements($"group{group}/unit"))
                                {
                                    Units.Add(it.Value);
                                    maxUnit++;
                                }
                            }
                        }
                    }
                    _UnitId += maxUnit;
                }
            }
        }
    }
}