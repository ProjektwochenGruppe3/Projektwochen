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

            foreach (var item in myMainWindow.Clients)
            {
                cb_selectCalcClient.Items.Add(item.Item2);
            }
        }
    }
}
