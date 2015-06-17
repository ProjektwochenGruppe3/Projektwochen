using Core.Network;
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
        private List<Component> usedComponents;

        public struct LineHelper
        {
            public Line dockLine;
            public bool IsEnd;
        }

        public MainWindow()
        {
            InitializeComponent();
            serverComponents = new List<Component>();
            usedComponents = new List<Component>();

            var testComponent = new Component();
            testComponent.FriendlyName = "Test1";
            testComponent.InputHints = new List<string>() { "string", "int", "double" };
            testComponent.OutputHints = new List<string>() { "string"};
            serverComponents.Add(testComponent);

            testComponent = new Component();
            testComponent.FriendlyName = "Test2";
            testComponent.InputHints = new List<string>() { "string", "int", "string"};
            testComponent.OutputHints = new List<string>() { "string", "double" };
            serverComponents.Add(testComponent);

            testComponent = new Component();
            testComponent.FriendlyName = "Test2";
            testComponent.InputHints = new List<string>() { "string", "int", "string" };
            testComponent.OutputHints = new List<string>() { "string", "double", "bla", "lol" };
            serverComponents.Add(testComponent);

            foreach (var item in serverComponents)
            {
                Label componentLabel = new Label();
                componentLabel.Content = item.FriendlyName;
                componentLabel.Tag = item;
                componentView.Items.Add(componentLabel);
            }
        }

        private void newLabel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var myLabel = (Label)sender;
            MessageBox.Show(myLabel.Content.ToString());
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

            int boxTopPosition = 25;
            int boxLeftPosition = 25;

            Label methodLabel = new Label();
            methodLabel.Content = toAdd.FriendlyName;
            methodLabel.Width = 60;
            newMethod.Children.Add(methodLabel);
            Canvas.SetLeft(methodLabel, boxLeftPosition);
            Canvas.SetTop(methodLabel, 0);

            var outputCount = toAdd.OutputHints.Count();
            var inputCount = toAdd.InputHints.Count();
            int outputHeight = 0;
            int inputHeight = 20;
            int fullHeight = 0;

            if (inputCount > outputCount)
            {
                inputHeight = 20;

                fullHeight = (inputCount + 1) * inputHeight;
                outputHeight = fullHeight / (outputCount + 1);
            }
            else
            {
                outputHeight = 20;
                fullHeight = (outputCount + 1) * outputHeight;
                inputHeight = fullHeight / (inputCount + 1);
            }

            Rectangle methodBox = new Rectangle();
            methodBox.Height = fullHeight;
            methodBox.Width = 50;
            methodBox.Stroke = new SolidColorBrush(Colors.Black);
            methodBox.Fill = new SolidColorBrush(Colors.Gray);
            newMethod.Children.Add(methodBox);
            Canvas.SetLeft(methodBox, boxLeftPosition);
            Canvas.SetTop(methodBox, boxTopPosition);

            int k = boxTopPosition + outputHeight;

            for (int i = 0; i < toAdd.OutputHints.Count(); i++)
            {
                Line outputLine = new Line();
                outputLine.X1 = boxLeftPosition + methodBox.Width;
                outputLine.X2 = boxLeftPosition + methodBox.Width + 20;
                outputLine.Y1 = k;
                outputLine.Y2 = k;
                outputLine.Stroke = new SolidColorBrush(Colors.Black);
                newMethod.Children.Add(outputLine);

                Ellipse dockPointOutput = new Ellipse();
                dockPointOutput.Height = 10;
                dockPointOutput.Width = 10;
                dockPointOutput.Stroke = new SolidColorBrush(Colors.Brown);
                dockPointOutput.Fill = new SolidColorBrush(Colors.LightBlue);
                dockPointOutput.MouseDown += dockPoint_MouseDown;
                dockPointOutput.MouseUp += dockPoint_MouseUp;
                newMethod.Children.Add(dockPointOutput);
                Canvas.SetLeft(dockPointOutput, boxLeftPosition + methodBox.Width + 20 - 5);
                Canvas.SetTop(dockPointOutput, k - 5);
                k = k + outputHeight;
            }

            int j = boxTopPosition + inputHeight;

            for (int i = 0; i < toAdd.InputHints.Count(); i++)
            {
                Line inputLine = new Line();
                inputLine.X1 = boxLeftPosition - 20;
                inputLine.X2 = boxLeftPosition;
                inputLine.Y1 = j;
                inputLine.Y2 = j;
                inputLine.Stroke = new SolidColorBrush(Colors.Black);
                newMethod.Children.Add(inputLine);

                Ellipse dockPointInput = new Ellipse();
                dockPointInput.Height = 10;
                dockPointInput.Width = 10;
                dockPointInput.Stroke = new SolidColorBrush(Colors.Brown);
                dockPointInput.Fill = new SolidColorBrush(Colors.LightBlue);
                dockPointInput.MouseDown += dockPoint_MouseDown;
                dockPointInput.MouseUp += dockPoint_MouseUp;
                newMethod.Children.Add(dockPointInput);
                Canvas.SetLeft(dockPointInput, boxLeftPosition - 20 - 5);
                Canvas.SetTop(dockPointInput, j - 5);
                j = j + inputHeight;
            }
        }

        private void dockPoint_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (SelectedLine != null)
            {
                var dockPoint = sender as Ellipse;

                if (dockPoint.Tag != null)
                {
                    ((dockPoint.Parent as Canvas).Parent as Canvas).Children.Remove(((LineHelper)dockPoint.Tag).dockLine);
                }

                var p = dockPoint.TranslatePoint(new Point(0, 0), canvas);
                SelectedLine.X2 = p.X + 5;
                SelectedLine.Y2 = p.Y + 5;
                LineHelper dockHelper;
                dockHelper.IsEnd = true;
                dockHelper.dockLine = SelectedLine;
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
                if (dockPoint.Tag != null)
                {
                    ((dockPoint.Parent as Canvas).Parent as Canvas).Children.Remove(((LineHelper)dockPoint.Tag).dockLine);
                }

                var p = dockPoint.TranslatePoint(new Point(0, 0), canvas);
                SelectedLine.X1 = p.X + 5;
                SelectedLine.Y1 = p.Y + 5;
                SelectedLine.X2 = p.X + 5;
                SelectedLine.Y2 = p.Y + 5;
                canvas.Children.Add(SelectedLine);
                LineHelper dockHelper;
                dockHelper.IsEnd = false;
                dockHelper.dockLine = SelectedLine;
                dockPoint.Tag = dockHelper;
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

                            if (dockPoint.Tag != null)
                            {
                                var dockHelper = (LineHelper)dockPoint.Tag;
                                var p = dockPoint.TranslatePoint(new Point(0, 0), canvas);

                                if (dockHelper.IsEnd == true)
                                {
                                    dockHelper.dockLine.X2 = p.X + 5;
                                    dockHelper.dockLine.Y2 = p.Y + 5;
                                }
                                else
                                {
                                    dockHelper.dockLine.X1 = p.X + 5;
                                    dockHelper.dockLine.Y1 = p.Y + 5;
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
    }
}