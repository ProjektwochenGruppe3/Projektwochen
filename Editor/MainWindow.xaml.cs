using Core.Network;
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

namespace Editor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Canvas SelectedMethod;

        public Point MousePosition;

        public Line SelectedLine;

        private List<Component> serverComponents;
        private List<Canvas> usedComponents;

        private int radius = 10;

        public MainWindow()
        {
            InitializeComponent();
            serverComponents = new List<Component>();
            usedComponents = new List<Canvas>();

            var testComponent = new Component();
            testComponent.FriendlyName = "Start";
            testComponent.InputHints = new List<string>() { };
            testComponent.OutputHints = new List<string>() { "string" };
            serverComponents.Add(testComponent);

            var ohneInput = new Component();
            ohneInput.FriendlyName = "End";
            ohneInput.InputHints = new List<string>() { "string" };
            ohneInput.OutputHints = new List<string>() { };
            serverComponents.Add(ohneInput);

            testComponent = new Component();
            testComponent.FriendlyName = "Simple String";
            testComponent.InputHints = new List<string>() { "string" };
            testComponent.OutputHints = new List<string>() { "string" };
            serverComponents.Add(testComponent);

            testComponent = new Component();
            testComponent.FriendlyName = "Other";
            testComponent.InputHints = new List<string>() { "string", "string", "string" };
            testComponent.OutputHints = new List<string>() { "string" };
            serverComponents.Add(testComponent);

            testComponent = new Component();
            testComponent.FriendlyName = "Test2";
            testComponent.InputHints = new List<string>() { "string" };
            testComponent.OutputHints = new List<string>() { "string" };
            serverComponents.Add(testComponent);



            foreach (var item in serverComponents)
            {
                Label componentLabel = new Label();
                componentLabel.Content = item.FriendlyName;
                componentLabel.Tag = item;
                componentView.Items.Add(componentLabel);
            }
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
            if (e.RightButton == MouseButtonState.Pressed && e.LeftButton != MouseButtonState.Pressed)
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
        }

        private void canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            SelectedMethod = null;
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

            int k = boxTopPosition + outputHeight;

            for (int i = 0; i < toAdd.OutputHints.Count(); i++)
            {
                methodLabel = new Label();
                methodLabel.Content = toAdd.OutputHints.ToList()[i].ToString();
                methodLabel.Width = length;
                methodLabel.BorderBrush = new SolidColorBrush(Colors.Black);
                methodLabel.FontWeight = FontWeights.Bold;
                newMethod.Children.Add(methodLabel);
                methodLabel.ToolTip = toAdd.OutputHints.ToList()[i].ToString();
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
                        (SelectedLine.Parent as Canvas).Children.Remove(SelectedLine);
                        SelectedLine = null;
                    }
                }
                else
                {
                    if (dockHelper.IsInput || lineHelper.InputDock.Parent == dockPoint.Parent || dockHelper.DataType != ((DockTag)lineHelper.InputDock.Tag).DataType)
                    {
                        (SelectedLine.Parent as Canvas).Children.Remove(SelectedLine);
                        SelectedLine = null;
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
            //IPAddress ip = new IPAddress(txt_ip.Text.ToString());
            //Guid bla = Guid.NewGuid();


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

        private void Button_Click(object sender, RoutedEventArgs e)
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
    }
}