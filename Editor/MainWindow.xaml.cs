using Core.Network;
using EditorNetwork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using dcs.core;
using System.Threading;

namespace Editor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Canvas SelectedMethod;

        private Point MousePosition;

        private Line SelectedLine;

        private EditorClient MyEditorClient;

        private Guid EditorGuid;

        public List<Tuple<Guid, string>> Clients;

        private List<Component> serverComponents;

        private List<Canvas> usedComponents;

        public SelectClients ClientsWindow;

        private EditorJob CurrentJob;

        private int radius = 10;

        public MainWindow()
        {
            InitializeComponent();

            EditorGuid = Guid.NewGuid();

            txt_ip.Text = "10.13.52.57";
            txt_port.Text = "30000";

            serverComponents = new List<Component>();
            usedComponents = new List<Canvas>();

            //FillWithTestComponents();
            //FillWithTestClients();
        }


        private void FillWithTestComponents()
        {
            var testComponent = new Component();
            testComponent.FriendlyName = "Start";
            testComponent.InputHints = new List<string>() { };
            testComponent.OutputHints = new List<string>() { "string" };
            testComponent.OutputDescriptions = new List<string>() { "Gibt einen sau geilen string zurück yo" };
            testComponent.ComponentGuid = Guid.NewGuid();

            serverComponents.Add(testComponent);

            var ohneInput = new Component();
            ohneInput.FriendlyName = "End";
            ohneInput.InputHints = new List<string>() { "string" };
            ohneInput.OutputHints = new List<string>() { };
            ohneInput.ComponentGuid = Guid.NewGuid();
            serverComponents.Add(ohneInput);

            testComponent = new Component();
            testComponent.FriendlyName = "Simple String";
            testComponent.InputHints = new List<string>() { "string" };
            testComponent.OutputHints = new List<string>() { "string" };
            testComponent.ComponentGuid = Guid.NewGuid();
            serverComponents.Add(testComponent);

            testComponent = new Component();
            testComponent.FriendlyName = "Other";
            testComponent.InputHints = new List<string>() { "string", "string", "string" };
            testComponent.OutputHints = new List<string>() { "string" };
            testComponent.ComponentGuid = Guid.NewGuid();
            serverComponents.Add(testComponent);

            testComponent = new Component();
            testComponent.FriendlyName = "Test2";
            testComponent.InputHints = new List<string>() { "string" };
            testComponent.OutputHints = new List<string>() { "string" };
            testComponent.ComponentGuid = Guid.NewGuid();
            serverComponents.Add(testComponent);

            foreach (var item in serverComponents)
            {
                Label componentLabel = new Label();
                componentLabel.Content = item.FriendlyName;
                componentLabel.Tag = item;
                componentView.Items.Add(componentLabel);
            }
        }

        private void FillWithTestClients()
        {
            Clients = new List<Tuple<Guid, string>>();
            var tupel0 = new Tuple<Guid, string>(Guid.NewGuid(), "Client 1");
            var tupel1 = new Tuple<Guid, string>(Guid.NewGuid(), "Client 2");
            var tupel2 = new Tuple<Guid, string>(Guid.NewGuid(), "Client 3");
            var tupel3 = new Tuple<Guid, string>(Guid.NewGuid(), "Client 4");
            Clients.Add(tupel0);
            Clients.Add(tupel1);
            Clients.Add(tupel2);
            Clients.Add(tupel3);
        }

        private void ListViewItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var item = sender as ListViewItem;
            if (item != null)
            {
                var Label = item.Content as Label;
                var componentToAdd = Label.Tag as Component;
                drawMethod(componentToAdd);
            }
        }

        private void MouseDownObject(object sender, MouseButtonEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed && e.LeftButton != MouseButtonState.Pressed && e.ClickCount == 1)
            {
                SelectedMethod = sender as Canvas;
                var x1 = e.GetPosition(canvas).X;
                var y1 = e.GetPosition(canvas).Y;

                var p = SelectedMethod.TranslatePoint(new Point(0, 0), canvas);
                var x2 = p.X;
                var y2 = p.Y;
                MousePosition.X = x1 - x2;
                MousePosition.Y = y1 - y2;
            }
            else if (e.LeftButton == MouseButtonState.Pressed && e.RightButton != MouseButtonState.Pressed && e.ClickCount == 2)
            {
                deleteCanvas(sender as Canvas);
            }
        }

        private void deleteCanvas(Canvas method)
        {
            foreach (var item in method.Children)
            {
                if (item is Ellipse)
                {
                    var dockPoint = item as Ellipse;
                    var dockTag = dockPoint.Tag as DockTag;

                    if (dockTag.DockLine != null)
                    {
                        if (dockTag.OtherDockPoint != null)
                        {
                            if (dockTag.OtherDockPoint.Tag != null)
                            {
                                (dockTag.DockLine.Parent as Canvas).Children.Remove(dockTag.DockLine);
                                (dockTag.OtherDockPoint.Tag as DockTag).OtherDockPoint = null;
                                (dockTag.OtherDockPoint.Tag as DockTag).DockLine = null;
                            }
                        }
                    }
                }
            }

            canvas.Children.Remove(method);
        }

        private void canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (SelectedMethod != null)
            {
                var index = usedComponents.IndexOf(SelectedMethod);
                SelectedMethod = null;
                FixPosition(usedComponents[index]);
            }

            if (SelectedLine != null && SelectedLine.Parent != null)
            {
                (SelectedLine.Parent as Canvas).Children.Remove(SelectedLine);
            }
        }

        private void canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void drawMethod(Component toAdd)
        {
            Canvas newMethod = new Canvas();
            newMethod.MouseDown += new MouseButtonEventHandler(MouseDownObject);
            canvas.Children.Add(newMethod);
            Canvas.SetTop(newMethod, 20);
            Canvas.SetLeft(newMethod, 50);

            int boxTopPosition = 25;
            int boxLeftPosition = 25;
            int length = 60;
            int width = 100;

            Label methodLabel = new Label();
            methodLabel.Content = toAdd.FriendlyName;
            methodLabel.Width = width;
            methodLabel.FontWeight = FontWeights.Bold;
            methodLabel.ToolTip = toAdd.FriendlyName;
            newMethod.Children.Add(methodLabel);
            Canvas.SetLeft(methodLabel, boxLeftPosition);
            Canvas.SetTop(methodLabel, 0);

            var outputCount = toAdd.OutputHints.Count();
            var inputCount = toAdd.InputHints.Count();
            int outputHeight = 0;
            int inputHeight = 0;
            int fullHeight = 0;
            int distance = 50;

            if (inputCount > outputCount)
            {
                inputHeight = distance;

                fullHeight = (inputCount + 1) * inputHeight;
                outputHeight = fullHeight / (outputCount + 1);
            }
            else
            {
                outputHeight = distance;
                fullHeight = (outputCount + 1) * outputHeight;
                inputHeight = fullHeight / (inputCount + 1);
            }

            Rectangle methodBox = new Rectangle();
            methodBox.Height = fullHeight;
            methodBox.Width = width;
            methodBox.Stroke = new SolidColorBrush(Colors.Black);
            methodBox.Fill = new SolidColorBrush(Colors.Gray);
            newMethod.Children.Add(methodBox);
            Canvas.SetLeft(methodBox, boxLeftPosition);
            Canvas.SetTop(methodBox, boxTopPosition);

            Guid internalGuid = Guid.NewGuid();

            int k = boxTopPosition + outputHeight;

            for (int i = 0; i < toAdd.OutputHints.Count(); i++)
            {
                methodLabel = new Label();
                methodLabel.Content = toAdd.OutputHints.ToList()[i].ToString();
                methodLabel.Width = length;
                methodLabel.BorderBrush = new SolidColorBrush(Colors.Black);
                methodLabel.FontWeight = FontWeights.Bold;
                newMethod.Children.Add(methodLabel);
                if (toAdd.OutputDescriptions == null || toAdd.OutputDescriptions.Count() - 1 < i)
                {
                    methodLabel.ToolTip = toAdd.OutputHints.ToList()[i].ToString();
                }
                else
                {
                    methodLabel.ToolTip = toAdd.OutputHints.ToList()[i].ToString() + "\r\n" + toAdd.OutputDescriptions.ToList()[i].ToString();

                }
                Canvas.SetLeft(methodLabel, boxLeftPosition + methodBox.Width);
                Canvas.SetTop(methodLabel, k);

                Line outputLine = new Line();
                outputLine.X1 = boxLeftPosition + methodBox.Width;
                outputLine.X2 = boxLeftPosition + methodBox.Width + length;
                outputLine.Y1 = k;
                outputLine.Y2 = k;
                outputLine.Stroke = new SolidColorBrush(Colors.Black);
                newMethod.Children.Add(outputLine);

                Ellipse dockPointOutput = new Ellipse();
                dockPointOutput.Height = radius * 2;
                dockPointOutput.Width = radius * 2;
                dockPointOutput.Stroke = new SolidColorBrush(Colors.Brown);
                dockPointOutput.Fill = new SolidColorBrush(Colors.Red);
                dockPointOutput.MouseDown += dockPoint_MouseDown;
                dockPointOutput.MouseUp += dockPoint_MouseUp;
                newMethod.Children.Add(dockPointOutput);
                Canvas.SetLeft(dockPointOutput, boxLeftPosition + methodBox.Width + length - radius);
                Canvas.SetTop(dockPointOutput, k - radius);

                DockTag dockHelper = new DockTag(); ;
                dockHelper.IsEnd = true;
                dockHelper.DockLine = null;
                dockHelper.IsInput = false;
                dockHelper.OtherDockPoint = null;
                dockHelper.Guid = internalGuid;
                dockHelper.ParamPosition = (uint)i + 1;
                dockHelper.DataType = toAdd.OutputHints.ToList()[i].ToString();
                dockPointOutput.Tag = dockHelper;

                k = k + outputHeight;
            }

            int j = boxTopPosition + inputHeight;

            for (int i = 0; i < toAdd.InputHints.Count(); i++)
            {
                methodLabel = new Label();
                methodLabel.Content = toAdd.InputHints.ToList()[i].ToString();
                methodLabel.Width = length;
                methodLabel.FontWeight = FontWeights.Bold;
                methodLabel.ToolTip = toAdd.InputHints.ToList()[i].ToString();
                newMethod.Children.Add(methodLabel);
                Canvas.SetLeft(methodLabel, boxLeftPosition - length);
                Canvas.SetTop(methodLabel, j);

                Line inputLine = new Line();
                inputLine.X1 = boxLeftPosition - length;
                inputLine.X2 = boxLeftPosition;
                inputLine.Y1 = j;
                inputLine.Y2 = j;
                inputLine.Stroke = new SolidColorBrush(Colors.Black);
                newMethod.Children.Add(inputLine);

                Ellipse dockPointInput = new Ellipse();
                dockPointInput.Height = radius * 2;
                dockPointInput.Width = radius * 2;
                dockPointInput.Stroke = new SolidColorBrush(Colors.Brown);
                dockPointInput.Fill = new SolidColorBrush(Colors.Blue);
                dockPointInput.MouseDown += dockPoint_MouseDown;
                dockPointInput.MouseUp += dockPoint_MouseUp;
                newMethod.Children.Add(dockPointInput);
                Canvas.SetLeft(dockPointInput, boxLeftPosition - length - radius);
                Canvas.SetTop(dockPointInput, j - radius);

                DockTag dockHelper = new DockTag();
                dockHelper.IsEnd = true;
                dockHelper.DockLine = null;
                dockHelper.IsInput = true;
                dockHelper.OtherDockPoint = null;
                dockHelper.Guid = internalGuid;
                dockHelper.ParamPosition = (uint)i + 1;
                dockHelper.DataType = toAdd.InputHints.ToList()[i].ToString();
                dockPointInput.Tag = dockHelper;

                j = j + inputHeight;
            }

            newMethod.Tag = toAdd;
            usedComponents.Add(newMethod);
        }

        private void dockPoint_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (SelectedLine != null)
            {
                var dockPoint = sender as Ellipse;

                DockTag dockHelper = (DockTag)dockPoint.Tag;

                if (dockHelper.OtherDockPoint != null)
                {
                    (dockHelper.OtherDockPoint.Tag as DockTag).OtherDockPoint = null;
                }

                var lineHelper = (LineTag)SelectedLine.Tag;

                var p = dockPoint.TranslatePoint(new Point(0, 0), canvas);
                SelectedLine.X2 = p.X + radius;
                SelectedLine.Y2 = p.Y + radius;

                if (lineHelper.InputDock == null)
                {
                    if (dockHelper.IsInput && lineHelper.OutputDock.Parent != dockPoint.Parent && dockHelper.DataType == ((DockTag)lineHelper.OutputDock.Tag).DataType)
                    {
                        if (dockHelper.OtherDockPoint != null)
                        {
                            ((dockPoint.Parent as Canvas).Parent as Canvas).Children.Remove(((DockTag)dockPoint.Tag).DockLine);
                        }

                        dockHelper.DockLine = SelectedLine;
                        dockHelper.IsEnd = true;

                        lineHelper.InputDock = dockPoint;
                        ((DockTag)lineHelper.OutputDock.Tag).OtherDockPoint = dockPoint;
                        dockHelper.OtherDockPoint = lineHelper.OutputDock;
                    }
                    else
                    {
                        if (SelectedLine.Parent != null)
                        {
                            (SelectedLine.Parent as Canvas).Children.Remove(SelectedLine);
                            SelectedLine = null;
                        }
                    }
                }
                else
                {
                    if (dockHelper.IsInput || lineHelper.InputDock.Parent == dockPoint.Parent || dockHelper.DataType != ((DockTag)lineHelper.InputDock.Tag).DataType)
                    {
                        if (SelectedLine.Parent != null)
                        {
                            (SelectedLine.Parent as Canvas).Children.Remove(SelectedLine);
                            SelectedLine = null;
                        }
                    }
                    else
                    {
                        if (dockHelper.OtherDockPoint != null)
                        {
                            ((dockPoint.Parent as Canvas).Parent as Canvas).Children.Remove(((DockTag)dockPoint.Tag).DockLine);
                        }

                        dockHelper.DockLine = SelectedLine;
                        dockHelper.IsEnd = true;
                        lineHelper.OutputDock = dockPoint;
                        ((DockTag)lineHelper.InputDock.Tag).OtherDockPoint = dockPoint;
                        dockHelper.OtherDockPoint = lineHelper.InputDock;
                    }
                }

                dockPoint.Tag = dockHelper;

                SelectedLine = null;
            }
        }

        private void dockPoint_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && e.RightButton != MouseButtonState.Pressed)
            {
                SelectedLine = new Line();
                SelectedLine.Stroke = new SolidColorBrush(Colors.Black);

                var dockPoint = sender as Ellipse;

                DockTag dockHelper = (DockTag)dockPoint.Tag;

                if (dockHelper.OtherDockPoint != null)
                {
                    (dockHelper.OtherDockPoint.Tag as DockTag).OtherDockPoint = null;
                    ((dockPoint.Parent as Canvas).Parent as Canvas).Children.Remove(((DockTag)dockPoint.Tag).DockLine);
                }

                var p = dockPoint.TranslatePoint(new Point(0, 0), canvas);
                SelectedLine.X1 = p.X + radius;
                SelectedLine.Y1 = p.Y + radius;
                SelectedLine.X2 = p.X + radius;
                SelectedLine.Y2 = p.Y + radius;
                canvas.Children.Add(SelectedLine);

                bool dockpointIsInput = false;

                if (dockPoint.Tag != null)
                {
                    var dockpointHelper = (DockTag)dockPoint.Tag;
                    dockpointIsInput = dockpointHelper.IsInput;
                }

                dockHelper.IsEnd = false;
                dockHelper.DockLine = SelectedLine;
                dockHelper.IsInput = dockpointIsInput;
                dockHelper.OtherDockPoint = null;
                dockPoint.Tag = dockHelper;

                LineTag lineHelper = new LineTag();
                lineHelper.InputDock = null;
                lineHelper.OutputDock = null;

                if (dockHelper.IsInput)
                {
                    lineHelper.InputDock = dockPoint;
                }
                else
                {
                    lineHelper.OutputDock = dockPoint;
                }

                SelectedLine.Tag = lineHelper;

                Canvas.SetZIndex(SelectedLine, -10);
            }
        }

        private void FixPosition(Canvas method)
        {
            if (method != null)
            {
                foreach (var item in method.Children)
                {
                    if (item is Ellipse)
                    {
                        var dockPoint = item as Ellipse;

                        if (dockPoint.Tag != null && ((DockTag)dockPoint.Tag).DockLine != null)
                        {
                            var dockHelper = (DockTag)dockPoint.Tag;
                            var p = dockPoint.TranslatePoint(new Point(0, 0), canvas);

                            if (dockHelper.IsEnd == true)
                            {
                                dockHelper.DockLine.X2 = p.X + radius;
                                dockHelper.DockLine.Y2 = p.Y + radius;
                            }
                            else
                            {
                                dockHelper.DockLine.X1 = p.X + radius;
                                dockHelper.DockLine.Y1 = p.Y + radius;
                            }
                        }
                    }
                }
            }
        }

        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed && e.LeftButton != MouseButtonState.Pressed)
            {
                if (SelectedMethod != null)
                {
                    Canvas.SetTop(SelectedMethod, e.GetPosition(canvas).Y - MousePosition.Y);
                    Canvas.SetLeft(SelectedMethod, e.GetPosition(canvas).X - MousePosition.X);

                    foreach (var item in SelectedMethod.Children)
                    {
                        if (item is Ellipse)
                        {
                            var dockPoint = item as Ellipse;

                            if (dockPoint.Tag != null && ((DockTag)dockPoint.Tag).DockLine != null)
                            {
                                var dockHelper = (DockTag)dockPoint.Tag;
                                var p = dockPoint.TranslatePoint(new Point(0, 0), canvas);

                                if (dockHelper.IsEnd == true)
                                {
                                    dockHelper.DockLine.X2 = p.X + radius;
                                    dockHelper.DockLine.Y2 = p.Y + radius;
                                }
                                else
                                {
                                    dockHelper.DockLine.X1 = p.X + radius;
                                    dockHelper.DockLine.Y1 = p.Y + radius;
                                }
                            }
                        }
                    }
                }
            }
            else if (e.LeftButton == MouseButtonState.Pressed && e.RightButton != MouseButtonState.Pressed)
            {
                if (SelectedLine != null)
                {
                    SelectedLine.X2 = e.GetPosition(canvas).X;
                    SelectedLine.Y2 = e.GetPosition(canvas).Y;
                }
            }
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            IPAddress ip;
            int port;

            try
            {
                ip = IPAddress.Parse(txt_ip.Text.ToString());
            }
            catch
            {
                MessageBox.Show("Bitte geben Sie eine gültige Ip-Adresse ein!");
                txt_ip.Text = string.Empty;
                return;
            }
            try
            {
                port = Convert.ToInt32(txt_port.Text.ToString());
            }
            catch
            {
                MessageBox.Show("Bitte geben Sie einen gültigen Port ein!");
                txt_port.Text = string.Empty;
                return;
            }

            if (MyEditorClient != null)
            {
                MyEditorClient.CloseDown();
            }

            try
            {
                MyEditorClient = new EditorClient(ip, port);
                MyEditorClient.ConnecttoServer();
                var serverComponentList = MyEditorClient.GetComponent();
                serverComponents = serverComponentList.ComponentList;
                Clients = serverComponentList.AvailableClients;

            }
            catch (Exception ex)
            {
                MyEditorClient = null;
                MessageBox.Show("Verbindung konnte nicht aufgebaut werden" + ex.Message);
                return;
            }

            foreach (var item in serverComponents)
            {
                Label componentLabel = new Label();
                componentLabel.Content = item.FriendlyName;
                componentLabel.Tag = item;
                componentView.Items.Add(componentLabel);
            }

            btn_disconnect.IsEnabled = true;
            btn_connect.IsEnabled = false;

            btn_execute.IsEnabled = true;
            btn_executesave.IsEnabled = true;
            btn_save.IsEnabled = true;

            lbl_name.IsEnabled = true;
            txt_name.IsEnabled = true;
        }

        private validTypes GraphisValid()
        {
            validTypes result = validTypes.componentjob;

            if (usedComponents.Count <= 1)
            {
                return validTypes.none;
            }

            // Check if there is an open end - if there is one - it is a a component in the best case
            bool foundEmpty = false;

            foreach (var item in canvas.Children)
            {
                if (foundEmpty == true)
                {
                    break;
                }

                if (item is Canvas)
                {
                    Canvas canvasCast = item as Canvas;

                    foreach (var item2 in canvasCast.Children)
                    {
                        if (item2 is Ellipse)
                        {
                            Ellipse ellipseCast = item2 as Ellipse;

                            DockTag ellipseTag = ellipseCast.Tag as DockTag;

                            if (ellipseTag.OtherDockPoint == null)
                            {
                                result = validTypes.component;
                                foundEmpty = true;
                                break;
                            }
                        }
                    }
                }
            }

            List<Canvas> goodMethods = new List<Canvas>();
            List<Canvas> previousGoodMethods = null;
            bool anyNewFlagSet = false;

            while (previousGoodMethods == null || previousGoodMethods.Count != goodMethods.Count || anyNewFlagSet)
            {
                anyNewFlagSet = false;
                previousGoodMethods = goodMethods;

                foreach (var item in usedComponents)
                {
                    bool allParamsAreReady = true;

                    foreach (var item2 in item.Children)
                    {
                        if (item2 is Ellipse)
                        {
                            Ellipse ellipseCast = item2 as Ellipse;

                            DockTag ellipseTag = ellipseCast.Tag as DockTag;

                            if (ellipseTag.IsInput == true)
                            {
                                if (ellipseTag.OtherDockPoint != null && !((DockTag)ellipseTag.OtherDockPoint.Tag).IsReady)
                                {
                                    allParamsAreReady = false;
                                    break;
                                }
                            }
                        }
                    }

                    if (allParamsAreReady)
                    {
                        //Set all Outputs to ready
                        foreach (var item2 in item.Children)
                        {
                            if (item2 is Ellipse)
                            {
                                Ellipse ellipseCast = item2 as Ellipse;

                                DockTag ellipseTag = ellipseCast.Tag as DockTag;

                                if (ellipseTag.IsInput == false)
                                {
                                    if (ellipseTag.IsReady != true)
                                    {
                                        ellipseTag.IsReady = true;
                                        anyNewFlagSet = true;
                                    }
                                }
                            }
                        }

                        if (!goodMethods.Contains(item))
                        {
                            goodMethods.Add(item);
                        }
                    }
                }
            }

            foreach (var item in usedComponents)
            {
                foreach (var item2 in item.Children)
                {
                    if (item2 is Ellipse)
                    {
                        Ellipse ellipseCast = item2 as Ellipse;

                        DockTag ellipseTag = ellipseCast.Tag as DockTag;

                        if (ellipseTag.IsInput == false)
                        {
                            ellipseTag.IsReady = false;
                        }
                    }
                }
            }

            if (goodMethods.Count != usedComponents.Count)
            {
                result = validTypes.none;
            }

            return result;

        }

        private Component createComponent()
        {
            Component creationComponent = new Component();
            return creationComponent;
        }

        private enum validTypes
        {
            component,
            componentjob,
            none
        }

        private void Button_Check(object sender, RoutedEventArgs e)
        {
            var result = GraphisValid();

            switch (result)
            {
                case validTypes.component:
                    MessageBox.Show("Valid component");
                    break;
                case validTypes.componentjob:
                    MessageBox.Show("Valid job");
                    break;
                case validTypes.none:
                    MessageBox.Show("Invalid");
                    break;
                default:
                    break;
            }
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            List<Object> elementsToDelete = new List<Object>();

            foreach (var item in canvas.Children)
            {
                elementsToDelete.Add(item);
            }

            foreach (var item in elementsToDelete)
            {
                canvas.Children.Remove(item as UIElement);
            }

            usedComponents = new List<Canvas>();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (MyEditorClient != null)
            {
                MyEditorClient.CloseDown();
            }
        }

        private void Disconnect_Click(object sender, RoutedEventArgs e)
        {
            if (MyEditorClient != null)
            {
                MyEditorClient.CloseDown();
            }

            btn_disconnect.IsEnabled = false;
            btn_connect.IsEnabled = true;

            btn_execute.IsEnabled = false;
            btn_save.IsEnabled = false;
            btn_executesave.IsEnabled = false;

            lbl_name.IsEnabled = false;
            txt_name.IsEnabled = false;

            serverComponents = new List<Component>();
            componentView.Items.Clear();
            Clear_Click(null, null);

        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (txt_name.Text == string.Empty)
            {
                MessageBox.Show("Bitte geben Sie einen Namen für Ihre Komponente ein");
                txt_name.Focus();
                return;
            }

            switch (GraphisValid())
            {
                case validTypes.none:
                    MessageBox.Show("Die Komponente ist nicht gültig.");
                    return;
                default:
                    break;
            }

            var job = Create_Job();



            if (job == null)
            {
                return;
            }
            else
            {
                job.JobAction = JobAction.Save;
                job.FriendlyName = txt_name.Text;
                Send_Job(job);
            }
        }

        private EditorJob Create_Job()
        {
            var component = Create_Component();

            EditorJob job = new EditorJob();
            job.HopCount = 0;
            job.JobComponent = component;
            job.JobRequestGuid = Guid.NewGuid();
            job.InputData = new List<object>();

            job.JobSourceClientGuid = EditorGuid;
            job.JobGuid = Guid.NewGuid();
            job.TargetCalcClientGuid = null;
            job.TargetDisplayClient = null;

            return job;
        }

        private void Send_Job(EditorJob job)
        {
            if (MyEditorClient != null)
            {
                try
                {
                    MyEditorClient.SendJobRequest(job);
                }
                catch
                {
                    MessageBox.Show("Der Job konnte nicht übertragen werden");
                }
            }
            else
            {
                MessageBox.Show("Der Job konnte nicht übertragen werden");
            }
        }

        private Component Create_Component()
        {
            Component result = new Component();
            List<ComponentEdge> edges = new List<ComponentEdge>();

            List<string> inputHints = new List<string>();
            List<string> outputHints = new List<string>();

            foreach (var item in canvas.Children)
            {
                if (item is Canvas)
                {
                    var method = item as Canvas;

                    foreach (var item2 in method.Children)
                    {
                        if (item2 is Ellipse)
                        {
                            var dockPoint = item2 as Ellipse;
                            var myDockTag = (DockTag)dockPoint.Tag;
                            ComponentEdge myEdge = new ComponentEdge();

                            if (myDockTag.IsInput)
                            {
                                if (myDockTag.OtherDockPoint == null)
                                {
                                    myEdge.InputComponentGuid = ((Component)method.Tag).ComponentGuid;
                                    myEdge.OutputComponentGuid = Guid.Empty;
                                    myEdge.InternalInputComponentGuid = myDockTag.Guid;
                                    myEdge.InternalOutputComponentGuid = Guid.Empty;
                                    myEdge.InputValueID = myDockTag.ParamPosition;
                                    inputHints.Add(myDockTag.DataType);
                                    myEdge.OutputValueID = (uint)inputHints.Count;
                                }
                                else
                                {
                                    myEdge.InputComponentGuid = ((Component)method.Tag).ComponentGuid;
                                    myEdge.OutputComponentGuid = ((Component)((Canvas)myDockTag.OtherDockPoint.Parent).Tag).ComponentGuid;
                                    myEdge.InternalInputComponentGuid = myDockTag.Guid;
                                    myEdge.InternalOutputComponentGuid = ((DockTag)myDockTag.OtherDockPoint.Tag).Guid;
                                    myEdge.InputValueID = myDockTag.ParamPosition;
                                    myEdge.OutputValueID = ((DockTag)myDockTag.OtherDockPoint.Tag).ParamPosition;
                                }
                            }
                            else
                            {
                                if (myDockTag.OtherDockPoint == null)
                                {
                                    myEdge.OutputComponentGuid = ((Component)method.Tag).ComponentGuid;
                                    myEdge.InputComponentGuid = Guid.Empty;
                                    myEdge.InternalOutputComponentGuid = myDockTag.Guid;
                                    myEdge.InternalInputComponentGuid = Guid.Empty;
                                    myEdge.OutputValueID = myDockTag.ParamPosition;
                                    outputHints.Add(myDockTag.DataType);
                                    myEdge.InputValueID = (uint)outputHints.Count;
                                }
                                else
                                {
                                    myEdge.OutputComponentGuid = ((Component)method.Tag).ComponentGuid;
                                    myEdge.InputComponentGuid = ((Component)((Canvas)myDockTag.OtherDockPoint.Parent).Tag).ComponentGuid;
                                    myEdge.InternalOutputComponentGuid = myDockTag.Guid;
                                    myEdge.InternalInputComponentGuid = ((DockTag)myDockTag.OtherDockPoint.Tag).Guid;
                                    myEdge.OutputValueID = myDockTag.ParamPosition;
                                    myEdge.InputValueID = ((DockTag)myDockTag.OtherDockPoint.Tag).ParamPosition;
                                }
                            }

                            if (!edges.Any(e => e.InternalInputComponentGuid == myEdge.InternalInputComponentGuid && e.InternalOutputComponentGuid == myEdge.InternalOutputComponentGuid))
                            {
                                edges.Add(myEdge);
                            }
                        }

                    }
                }
            }

            result.ComponentGuid = Guid.NewGuid();
            result.Edges = edges;
            result.InputHints = inputHints;
            result.OutputHints = outputHints;
            result.IsAtomic = false;
            result.FriendlyName = "NewComponent";
            result.InputDescriptions = new List<string>();
            result.OutputDescriptions = new List<string>();
            return result;
        }

        private void ExecuteSave_Click(object sender, RoutedEventArgs e)
        {
            if (txt_name.Text == string.Empty)
            {
                MessageBox.Show("Bitte geben Sie einen Namen für Ihre Komponente ein");
                txt_name.Focus();
                return;
            }

            switch (GraphisValid())
            {
                case validTypes.none:
                    MessageBox.Show("Die Komponente ist nicht gültig.");
                    return;
                case validTypes.component:
                    MessageBox.Show("Die Komponente ist kein gültiger Job.");
                    return;
                default:
                    break;
            }

            var job = Create_Job();

            if (job == null)
            {
                return;
            }
            else
            {
                job.JobAction = JobAction.SaveAndExecute;
                CurrentJob = job;

                Send_Job(job);
            }
        }

        public void AfterDialogClosed(Guid displayGuid, Guid calcGuid)
        {
            CurrentJob.TargetDisplayClient = displayGuid;
            CurrentJob.TargetCalcClientGuid = calcGuid;
            try
            {
                Send_Job(CurrentJob);
            }
            catch
            {
                MessageBox.Show("Konnte nicht zum Server gesendet werden.");
            }
            CurrentJob = null;
        }

        private void Execute_Click(object sender, RoutedEventArgs e)
        {
            switch (GraphisValid())
            {
                case validTypes.none:
                    MessageBox.Show("Die Komponente ist nicht gültig.");
                    return;
                case validTypes.component:
                    MessageBox.Show("Die Komponente ist kein gültiger Job.");
                    return;
                default:
                    break;
            }

            var job = Create_Job();

            if (job == null)
            {
                return;
            }
            else
            {
                job.JobAction = JobAction.Execute;
                CurrentJob = job;

                SelectClients window = new SelectClients(this);
                window.Show();
            }
        }
    }
}