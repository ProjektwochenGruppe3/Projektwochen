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

        public struct LineHelper
        {
            public Line dockLine;
            public bool IsEnd;
        }

        public MainWindow()
        {
            InitializeComponent();

            drawMethod(9, "Komp. 1");
            drawMethod(12, "Komp. 2");
            drawMethod(1, "Komp. 3");


            //Canvas methodCanvas = new Canvas();
            //canvas.Children.Add(methodCanvas);
            //methodCanvas.MouseDown += new MouseButtonEventHandler(MouseDownObject);

            //Label label = new Label();
            //label.Content = "Komponente 1";
            //methodCanvas.Children.Add(label);
            //Canvas.SetLeft(label, 0);
            //Canvas.SetTop(label, 0);

            //Rectangle test = new Rectangle();
            //test.Height = 50;
            //test.Width = 50;
            //test.Stroke = new SolidColorBrush(Colors.Red);
            //test.Fill = new SolidColorBrush(Colors.Gray);
            //methodCanvas.Children.Add(test);
            //Canvas.SetLeft(test, 20);
            //Canvas.SetTop(test, 25);
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
        }

        private void canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void drawMethod(int inputCount, string name)
        {
            Canvas newMethod = new Canvas();
            newMethod.MouseDown += new MouseButtonEventHandler(MouseDownObject);
            canvas.Children.Add(newMethod);

            int boxTopPosition = 25;
            int boxLeftPosition = 25;

            Label methodLabel = new Label();
            methodLabel.Content = name;
            methodLabel.Width = 60;
            newMethod.Children.Add(methodLabel);
            Canvas.SetLeft(methodLabel, boxLeftPosition);
            Canvas.SetTop(methodLabel, 0);

            Rectangle methodBox = new Rectangle();
            methodBox.Height = (inputCount + 1) * 20;
            methodBox.Width = 50;
            methodBox.Stroke = new SolidColorBrush(Colors.Red);
            methodBox.Fill = new SolidColorBrush(Colors.Gray);
            newMethod.Children.Add(methodBox);
            Canvas.SetLeft(methodBox, boxLeftPosition);
            Canvas.SetTop(methodBox, boxTopPosition);

            Line outputLine = new Line();
            outputLine.X1 = boxLeftPosition + methodBox.Width;
            outputLine.X2 = boxLeftPosition + methodBox.Width + 20;

            outputLine.Y1 = methodBox.Height / 2 + boxTopPosition;
            outputLine.Y2 = methodBox.Height / 2 + boxTopPosition;
            outputLine.Stroke = new SolidColorBrush(Colors.Black);
            newMethod.Children.Add(outputLine);

            Ellipse dockPointOutput = new Ellipse();
            dockPointOutput.Height = 10;
            dockPointOutput.Width = 10;
            dockPointOutput.Stroke = new SolidColorBrush(Colors.Brown);
            dockPointOutput.Fill = new SolidColorBrush(Colors.LightBlue);
            dockPointOutput.MouseDown += dockPointOutput_MouseDown;
            dockPointOutput.MouseUp += dockPointOutput_MouseUp;
            newMethod.Children.Add(dockPointOutput);
            Canvas.SetLeft(dockPointOutput, boxLeftPosition + methodBox.Width + 20 - 5);
            Canvas.SetTop(dockPointOutput, methodBox.Height / 2 + boxTopPosition - 5);

            int j = boxTopPosition + 20;

            for (int i = 0; i < inputCount; i++)
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
                newMethod.Children.Add(dockPointInput);
                Canvas.SetLeft(dockPointInput, boxLeftPosition - 20 - 5);
                Canvas.SetTop(dockPointInput, j - 5);
                j = j + 20;
            }
        }

        private void dockPointOutput_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (SelectedLine != null)
            {
                var dockPoint = sender as Ellipse;
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

        private void dockPointOutput_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && e.RightButton != MouseButtonState.Pressed)
            {
                SelectedLine = new Line();
                SelectedLine.Stroke = new SolidColorBrush(Colors.Black);
                var dockPoint = sender as Ellipse;
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

                    foreach (var item in ((Canvas)sender).Children)
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
                                    dockHelper.dockLine.X2 = p.X;
                                    dockHelper.dockLine.Y2 = p.Y;
                                }
                                else
                                {
                                    dockHelper.dockLine.X1 = p.X;
                                    dockHelper.dockLine.Y1 = p.Y;
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
