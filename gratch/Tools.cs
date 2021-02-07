#define NPOI
#define HIDDEN

using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
#if NPOI
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
#endif

namespace gratch
{
    public class Tools
    {
        //Тексты и були
        public static readonly string appPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
        public static readonly string gPath = $@"{appPath}//graph.xml";
        public static readonly string lPath = $@"{appPath}//list.xml";
        public static readonly string dPath = $@"{appPath}//days.xml";
        public static bool is_graph => File.Exists(gPath);
        public static bool is_list => File.Exists(lPath);
        public static bool is_days => File.Exists(dPath);
        static readonly CultureInfo CultInfo = CultureInfo.GetCultureInfo("ru-ru");
#if HIDDEN
        //Сохраняет файл с аттрибутом "Скрытый" \ Потребности нету, позже уберу
        public static void SaveHidden(string path, XDocument doc) //Топор
        {
            //if (File.Exists(path)) File.SetAttributes(path, FileAttributes.Normal);
            doc.Save(path);
            //if (File.Exists(path)) File.SetAttributes(path, FileAttributes.Hidden);
        }
        public static void SaveHidden(string path, XmlDocument doc) // стал супом
        {
            //if (File.Exists(path)) File.SetAttributes(path, FileAttributes.Normal);
            doc.Save(path);
            //if (File.Exists(path)) File.SetAttributes(path, FileAttributes.Hidden);
        }
#endif
        //Обновляет список выходных
        public static string[] hday_init()
        {
            void CreateFile()
            {
                XDocument xDays = Tools.is_days ? XDocument.Load(Tools.dPath) : new XDocument(
                    new XElement(DayOfWeek.Monday.ToString(), false),
                    new XElement(DayOfWeek.Tuesday.ToString(), false),
                    new XElement(DayOfWeek.Wednesday.ToString(), false),
                    new XElement(DayOfWeek.Thursday.ToString(), false),
                    new XElement(DayOfWeek.Friday.ToString(), false),
                    new XElement(DayOfWeek.Saturday.ToString(), true),
                    new XElement(DayOfWeek.Sunday.ToString(), true)
                    );
                Tools.SaveHidden(Tools.dPath, xDays);
                if (Tools.is_graph) gratch.RedactG.CreateGraph();
            }

            string[] holidays = new string[7];

            if (!is_days) CreateFile();
            XDocument xDays = XDocument.Load(dPath);
            int iter = 0;
            foreach (XElement element in xDays.Root.Elements())
            {
                if (bool.Parse(element.Value))
                {
                    iter++;
                    holidays[iter] = element.Name.ToString();
                }
            }
            return holidays;
        }
        //Если day выходной, возвращает true
        public static bool is_holiday(DayOfWeek day)
        {
            foreach (string item in hday_init())
            {
                if (day.ToString() == item) return true;
            }
            return false;
        }
        //Возвращает день недели notnow текущего месяца
        public static DayOfWeek dayweek(int notnow)
        {
            DateTime now = DateTime.Now;
            DateTime date = new DateTime(now.Year, now.Month, notnow);
            return date.DayOfWeek;
        }
#if NPOI
        public class NPOI_Excel
        {
            static private readonly string[] Days =
                {
                    "Нд",
                    "Пн",
                    "Вт",
                    "Ср",
                    "Чт",
                    "Пт",
                    "Сб"
                };
            static public void ExcelCreator(int grp, IWorkbook book)
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
                var HolidayStyle = StyleGenerator(book, IndexedColors.Grey25Percent.Index, FillPattern.SolidForeground);
                var EmptyStyle = StyleGenerator(book, IndexedColors.White.Index, FillPattern.NoFill);
                var UnitStyle = StyleGenerator(book, IndexedColors.LightGreen.Index, FillPattern.SolidForeground);
                var TopStyle = StyleGenerator(book, IndexedColors.Grey25Percent.Index, FillPattern.ThinBackwardDiagonals);
                var DutyStyle = StyleGenerator(book, IndexedColors.SkyBlue.Index, FillPattern.SolidForeground);
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
                    $"{DateTime.Now.Month}.{DateTime.Now.Year}",CultInfo).DayOfWeek;
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

                    cell.SetCellValue(cellvalue + " " + Days[(int)(DateTime.Parse($"{cellvalue},{DateTime.Now.Month},{DateTime.Now.Year}",CultInfo).DayOfWeek)]);              //Привет
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
                            DayOfWeek dof = Convert.ToDateTime(dof_copy,CultInfo).DayOfWeek;
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
            //Генератор тонких клеточек
            public static ICellStyle StyleGenerator(IWorkbook book, short color, FillPattern pattern)
            {
                var style = book.CreateCellStyle();
                style.FillForegroundColor = color;
                style.FillPattern = pattern;

                style.BorderBottom = BorderStyle.Thin;
                style.BorderLeft = BorderStyle.Thin;
                style.BorderRight = BorderStyle.Thin;
                style.BorderTop = BorderStyle.Thin;
                return style;
            }
            static public void CallExcelFunc()
            {
                XmlDocument lDoc = new XmlDocument();
                lDoc.Load(Tools.lPath);
                IWorkbook book = new XSSFWorkbook();
                for (int grp = 1; grp <= lDoc.DocumentElement.ChildNodes.Count; grp++) ExcelCreator(grp, book);
                book.Write(File.Create($"{appPath}//ГраЧ - Версія для друку.xlsx"));
                book.Close();
            }
        }
#endif
    }
}
