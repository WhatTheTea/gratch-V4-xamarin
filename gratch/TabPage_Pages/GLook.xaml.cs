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
using Xamarin.Forms.Markup;
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
            buff.GestureRecognizers.Add(new TapGestureRecognizer { NumberOfTapsRequired = 1, Command = TapCommand, CommandParameter = row });
            grid1.Children.Add(buff, col, row);
        }
        ICommand TapCommand => new Command((object arg) =>
                                             {
                                                 PunishUnit(arg);
                                             });
        async public void PunishUnit(object ReturnedRow)
        {
            await DisplayAlert("lol", ReturnedRow.ToString(), "OK");
        }
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
        private void listBox1_MouseDoubleClick()
        {
            //int naturalIndex = listBox1.SelectedIndex + 1;
            //XmlDocument gDoc = new XmlDocument();
            //gDoc.Load(Tools.gPath);
            //XmlNode BadUnit = gDoc.DocumentElement.SelectSingleNode($"group{numericUpDown1.Value}/unit[{naturalIndex}]");
            //int index = int.Parse(BadUnit.SelectSingleNode("@day").InnerText);
            //new Punish2(BadUnit.ParentNode.LocalName, BadUnit.InnerText, index).ShowDialog();
        }

        private void button1_Click()
        {
            //new RedactG().CreateGraph();
            //listBox1.Items.Clear();
            //Start((int)numericUpDown1.Value);
            //treeView1.Nodes.Clear();
            //ForDay();
        }

        private void Button_Clicked(object sender, EventArgs e) // стандарт
        {

        }

        private void Button_Clicked_1(object sender, EventArgs e) // Ексель
        {
            Tools.NPOI_Excel.CallExcelFunc();
            //Process.Start();
        }

#if EXCEL
public void ExcelCreator(int grp, IWorkbook book)
        {
            //int grp = (int)numericUpDown1.Value;
            int dayinmon = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);
            XmlDocument lDoc = new XmlDocument();
            lDoc.Load(Tools.lPath);
            int unitCount = lDoc.DocumentElement.SelectSingleNode($"group{grp}").ChildNodes.Count;
            //Excel
            //Создание воркбука происходит в вызывающей функции.
            ISheet sht = book.CreateSheet($"ГраЧ | Група {grp}");
            //Styles
            var HolidayStyle = Tools.StyleGenerator(book, IndexedColors.Grey25Percent.Index, FillPattern.SolidForeground);
            var EmptyStyle = Tools.StyleGenerator(book, IndexedColors.White.Index, FillPattern.NoFill);
            var UnitStyle = Tools.StyleGenerator(book, IndexedColors.LightGreen.Index, FillPattern.SolidForeground);
            var TopStyle = Tools.StyleGenerator(book, IndexedColors.Grey25Percent.Index, FillPattern.ThinBackwardDiagonals);
            var DutyStyle = Tools.StyleGenerator(book, IndexedColors.SkyBlue.Index, FillPattern.SolidForeground);
            //CellDraw
            IRow row = sht.CreateRow(0);
            ICell cell;

            for (int i = 0; i <= unitCount; i++)
            {
                row = sht.CreateRow(i);
                for (int c = 0; c <= dayinmon; c++)
                {
                    row.CreateCell(c);
                    row.GetCell(c).CellStyle = EmptyStyle;
                }
            }

            //CellFill
            sht.SetColumnWidth(0, 10000);
            row.HeightInPoints = 25;
            for (int i = 1, cellvalue = 1; i <= dayinmon; i++, cellvalue++)
            {
                DayOfWeek dof = Convert.ToDateTime($"{i}." +
                    $"{DateTime.Now.Month}.{DateTime.Now.Year}").DayOfWeek;
                row = sht.GetRow(0);
                cell = row.GetCell(i);
                sht.SetColumnWidth(i, 1370);
                cell.CellStyle = TopStyle;
                if (Tools.is_holiday(dof))
                {
                    cell.CellStyle = HolidayStyle;
                    for (int k = 1; k <= unitCount; k++)
                    {
                        sht.GetRow(k).GetCell(i).CellStyle = HolidayStyle;
                    }
                }

                cell.SetCellValue(cellvalue + " " + Days[(int)(DateTime.Parse($"{cellvalue},{DateTime.Now.Month},{DateTime.Now.Year}").DayOfWeek)]);              //Привет
            }

            row.GetCell(0).SetCellValue(@"Ім'я\Число");
            row.GetCell(0).CellStyle = TopStyle;

            for (int rm = 1; rm <= unitCount; rm++)
            {
                row = sht.GetRow(rm);
                row.HeightInPoints = 20;
                cell = row.GetCell(0);
                cell.SetCellValue(
                lDoc.DocumentElement.SelectSingleNode($"//group{grp}/unit[{rm}]").InnerText);
                cell.CellStyle = UnitStyle;
            }

            //DutyCheck
            XmlDocument gDoc = new XmlDocument();
            gDoc.Load(Tools.gPath);
            string dof_copy;
            for (int i = 1, j = 1; j <= dayinmon; i++, j++)
            {
                if (i > unitCount) i = 1;
                row = sht.GetRow(i);
                for (int d = 1; d <= dayinmon; d++)
                {
                    dof_copy = $"{d}.{DateTime.Now.Month}.{DateTime.Now.Year}";
                    cell = row.GetCell(d);
                    if (cell == null || cell.CellType == CellType.Blank)
                    {
                        DayOfWeek dof = Convert.ToDateTime(dof_copy).DayOfWeek;
                        if (!Tools.is_holiday(dof))
                        {
                            string dStr = gDoc.DocumentElement
                                .SelectSingleNode($"//group{grp}/unit[@day='{d}']").InnerText;
                            string iStr = lDoc.DocumentElement
                                .SelectSingleNode($"//group{grp}/unit[{i}]").InnerText;
                            if (dStr == iStr)
                            {
                                //cell.SetCellValue($"{i}|{j}|{d}");
                                cell.CellStyle = DutyStyle;
                            }
                        }
                    }
                }
            }
        }
        public void CallExcelFunc()
        {
            XmlDocument lDoc = new XmlDocument();
            lDoc.Load(Tools.lPath);
            IWorkbook book = new XSSFWorkbook();
            for (int grp = 1; grp <= lDoc.DocumentElement.ChildNodes.Count; grp++) ExcelCreator(grp, book);
            book.Write(File.Create("ГраЧ - Версія для друку.xlsx"));
            book.Close();
        }
#endif
        /*private void ForDay()
            {
                DateTime now = DateTime.Now;
                int dinm = DateTime.DaysInMonth(now.Year, now.Month);
                dateTimePicker1.MaxDate = Convert.ToDateTime($"{dinm}.{now.Month}.{now.Year}");
                dateTimePicker1.MinDate = Convert.ToDateTime($"1.{now.Month}.{now.Year}");
                XmlDocument graph = new XmlDocument();
                graph.Load(Tools.gPath);

                for (int notholiday = dateTimePicker1.Value.Day; Tools.is_holiday(dateTimePicker1.Value.DayOfWeek); notholiday++)
                {
                    if (notholiday > dinm)
                    {
                        for (notholiday--; Tools.is_holiday(Convert.ToDateTime($"{notholiday},{now.Month},{now.Year}").DayOfWeek); notholiday--) ;
                    }
                    dateTimePicker1.Value = Convert.ToDateTime($"{notholiday}.{now.Month}.{now.Year}");
                } // Доработать, шоб красиво было, не с единички, а в обратную сторону. 
                  // Сделал :>
                foreach (XmlNode node in graph.DocumentElement)
                {
                    if (node.Name.StartsWith("group"))
                    {
                        TreeNode grp = new TreeNode(node.Name);
                        grp.Nodes.Add("Черговий: " + node.SelectSingleNode($"unit[@day='{dateTimePicker1.Value.Day}']").InnerText);
                        treeView1.Nodes.Add(grp);
                    }
                }
            }*/
    }
}