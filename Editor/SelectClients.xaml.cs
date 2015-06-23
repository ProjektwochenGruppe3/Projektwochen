using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Editor
{
    /// <summary>
    /// Interaction logic for SelectClients.xaml
    /// </summary>
    public partial class SelectClients : Window
    {
        public MainWindow myMainWindow { get; set; }

        public SelectClients(MainWindow mainWindow)
        {
            this.myMainWindow = mainWindow;

            InitializeComponent();

            cb_selectCalcClient.Items.Add("Dem Server überlassen (Standard)");
            cb_selectDisplayClient.Items.Add("Dem Server überlassen (Standard)");

            foreach (var item in myMainWindow.Clients)
            {
                cb_selectCalcClient.Items.Add(item.Item1 + " " + item.Item2);
                cb_selectDisplayClient.Items.Add(item.Item1 + " " + item.Item2);
            }

            cb_selectCalcClient.FontFamily = new FontFamily("Consolas");
            cb_selectCalcClient.SelectedIndex = 0;

            cb_selectDisplayClient.FontFamily = new FontFamily("Consolas");
            cb_selectDisplayClient.SelectedIndex = 0;
        }
    }
}
