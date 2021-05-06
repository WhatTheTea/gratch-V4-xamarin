using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using System.Xml.Linq;
using System.Xml.XPath;

namespace gratch
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RedactDays : ContentPage
    {
        private Dictionary<DayOfWeek, Switch> SwitchByDay;
        private Collection<string> DaysWindows;
        private XDocument xDays = null;
        //public static Dictionary<Days, Guid> Switch_IDs;
        public RedactDays()
        {
            InitializeComponent();
            Start();
        }
        void Start()
        {
            DaysWindows = new Collection<string> { "Редактор списку чергових", "Редактор вихідних" };
            DaysWindowPicker.ItemsSource = DaysWindows;
            DaysWindowPicker.SelectedIndex = 1;

            SwitchByDay = new Dictionary<DayOfWeek, Switch> {
                {DayOfWeek.Monday, Switch_Monday },
                {DayOfWeek.Tuesday, Switch_Tuesday },
                {DayOfWeek.Wednesday, Switch_Wednesday },
                {DayOfWeek.Thursday, Switch_Thursday },
                {DayOfWeek.Friday, Switch_Friday },
                {DayOfWeek.Saturday, Switch_Saturday },
                {DayOfWeek.Sunday, Switch_Sunday}
            };

            xDays = Tools.is_days ? XDocument.Load(Tools.dPath) : new XDocument(
                    new XElement(DayOfWeek.Monday.ToString(), false),
                    new XElement(DayOfWeek.Tuesday.ToString(), false),
                    new XElement(DayOfWeek.Wednesday.ToString(), false),
                    new XElement(DayOfWeek.Thursday.ToString(), false),
                    new XElement(DayOfWeek.Friday.ToString(), false),
                    new XElement(DayOfWeek.Saturday.ToString(), true),
                    new XElement(DayOfWeek.Sunday.ToString(), true)
                    );

            foreach(var @switch in SwitchByDay.Values.Zip(SwitchByDay.Keys,Tuple.Create))
            {
                @switch.Item1.IsToggled = bool.Parse(xDays.Root.Element(@switch.Item2.ToString()).Value);
            }
        }

        private void DaysWindowPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (DaysWindowPicker.SelectedIndex)
            {
                case 0:
                    MainPage.Pages[this.TabIndex] = new RedactG();
                    break;
                case 1:
                    //do nothing
                    break;
                default:
                    throw new ArgumentException("Invalid DaysWindowPicker index");
            }
        }

        private void Button2_Clicked(object sender, EventArgs e)
        {

        }

        async private void ChangeDays(DayOfWeek day)
        {
            IEnumerable<Switch> holidays = SwitchByDay.Values.Where(value => value.IsToggled == true);
            if (holidays.Count() < 7)
            {
                xDays.Root.Element(day.ToString()).Value = SwitchByDay[day].IsToggled.ToString();
                xDays.Save(Tools.dPath);
                gratch.RedactG.CreateGraph();
            } else
            {
                await DisplayAlert("Помилка", "Занадто багато вихідних", "Ок");
                holidays.Last().IsToggled = false;
            }
        }
        private void Switch_Monday_Toggled(object sender, ToggledEventArgs e)
        {
            ChangeDays(DayOfWeek.Monday);
        }

        private void Switch_Tuesday_Toggled(object sender, ToggledEventArgs e)
        {
            ChangeDays(DayOfWeek.Tuesday);
        }

        private void Switch_Wednesday_Toggled(object sender, ToggledEventArgs e)
        {
            ChangeDays(DayOfWeek.Wednesday);
        }

        private void Switch_Thursday_Toggled(object sender, ToggledEventArgs e)
        {
            ChangeDays(DayOfWeek.Thursday);
        }

        private void Switch_Friday_Toggled(object sender, ToggledEventArgs e)
        {
            ChangeDays(DayOfWeek.Friday);
        }

        private void Switch_Saturday_Toggled(object sender, ToggledEventArgs e)
        {
            ChangeDays(DayOfWeek.Saturday);
        }

        private void Switch_Sunday_Toggled(object sender, ToggledEventArgs e)
        {
            ChangeDays(DayOfWeek.Sunday);
        }
    }
}