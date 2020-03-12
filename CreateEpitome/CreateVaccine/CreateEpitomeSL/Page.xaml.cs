using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.IO;
using System.Text;
using System.Threading;
using Msr.Mlas.SpecialFunctions;
using System.Text.RegularExpressions;
using System.Windows.Browser;

namespace CreateEpitomeSL
{
    //!!!Everything releated to the outputbox could be a control and thus more self-contained.
    //!!!Would be nice to have a class or control that could work with any "slow" enumerators
    public partial class Page : UserControl
    {
        public Page()
        {
            InitializeComponent();
            Loaded += new RoutedEventHandler(Page_Loaded);
        }

        void Page_Loaded(object sender, RoutedEventArgs e)
        {
            goButton.Click += goButton_Click;
            loadSampleButton.Click += loadSampleButton_Click;
            loadTextButton.Click += loadTextButton_Click;
            stopButton.Click += stopButton_Click;
            uploadButton.Click += uploadButton_Click;
            showLastCheckBox.IsChecked = true;
            showLastCheckBox.Click += showLastCheckBox_Click;
            LayoutRoot.SizeChanged += layoutRoot_SizeChanged;

            stopButton.Content = "Pause";
            stopButton.IsEnabled = false;
            layoutRoot_SizeChanged(null, null);
        }

        void layoutRoot_SizeChanged(object sender, RoutedEventArgs e)
        {
            try //If the HTML page doesn't know how to resize, just ignore it
            {
                HtmlPage.Window.Invoke("ResizeObject", new object[] { LayoutRoot.Height });
            }
            catch (System.InvalidOperationException)
            {
            }

        }

        private void UpdateLayoutHeight()
        {
            this.LayoutRoot.UpdateLayout();
            double height = 10;
            foreach (var child in LayoutRoot.Children)
            {
                height += child.RenderSize.Height;
            }

            this.LayoutRoot.Height = height;
        }

        void showLastCheckBox_Click(object sender, RoutedEventArgs e)
        {
            SpecialFunctions.CheckCondition(showLastCheckBox.IsEnabled, "assert");
            StringBuilder sb = new StringBuilder(outputTextBox.Text);
            outputTextBox.Text = showStepsBuffer.ToString();
            showStepsBuffer = sb;
            UpdateLayoutHeight();
        }
        StringBuilder showStepsBuffer = new StringBuilder();


        void stopButton_Click(object sender, RoutedEventArgs e)
        {
            SpecialFunctions.CheckCondition(stopButton.IsEnabled, "expect stopButton to be enabled");
            switch ((string)stopButton.Content)
            {
                case "Pause": //!!!Const
                    {
                        CompositionTarget.Rendering -= CurrentRenderingDelegate;
                        stopButton.Content = "Continue";
                        showLastCheckBox.IsEnabled = true;
                        break;
                    }
                case "Continue": //!!!Const
                    {
                        stopButton.Content = "Pause";
                        CompositionTarget.Rendering += CurrentRenderingDelegate;
                        showLastCheckBox.IsEnabled = false;
                        break;
                    }
                default:
                    {
                        SpecialFunctions.CheckCondition(false, "Don't know state " + stopButton.Content);
                        break;
                    }
            }
            
        }

        void goButton_Click(object sender, RoutedEventArgs e)
        {
            CompositionTarget.Rendering -= CurrentRenderingDelegate;
            CurrentRenderingDelegate = null;
            stopButton.Content = "Pause";
            stopButton.IsEnabled = true;
            showLastCheckBox.IsEnabled = false;

            string patchTableAsString = CreatePatchTable(inputTextBox.Text);
            //inputTextBox.Text = patchTableAsString;

            var enumerator = CreateVaccine.CreateVaccine.GreedyEpitomeEnumerable(patchTableAsString).GetEnumerator();
            CurrentRenderingDelegate = (object senderx, EventArgs ex) => ComputeAndDisplayTheNextStep(enumerator);
            CompositionTarget.Rendering += CurrentRenderingDelegate;
            ClearPlot();
            showStepsBuffer = new StringBuilder(CreateVaccine.CreateVaccine.DisplayHeaderString);
        }


        System.EventHandler CurrentRenderingDelegate = null;


        private void ClearPlot()
        {
            plotCanvas.Children.Clear();
            PlotScale = 1;
            plotLength.Text = plotCanvas.Width.ToString();
        }

        void uploadButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (!openFileDialog.ShowDialog().Equals(true))
            {
                return;
            }
            using (TextReader textReader = openFileDialog.File.OpenText())
            {
                inputTextBox.Text = textReader.ReadToEnd();
            }
        }

        static private Regex regexWhitespaceAndPunctuation = new Regex(@"([,;.:!|]|\s)+");
        static private Regex regexPatchWeight = new Regex(@"^Patch\sWeight",RegexOptions.IgnoreCase);
        static private Regex regexWhitespace = new Regex(@"( |\t|,)+");


        private string CreatePatchTable(string inputString)
        {
            if (regexPatchWeight.IsMatch(inputString))
            {
                string s = regexWhitespace.Replace(inputString, "\t");
                return s;
            }

            StringBuilder sb = new StringBuilder("Patch\tWeight");
            foreach (string word in regexWhitespaceAndPunctuation.Split(inputString.Trim()))
            {
                if (word == "" || regexWhitespaceAndPunctuation.IsMatch(word))
                {
                    continue; //not break;
                }
                sb.AppendFormat("\n{0}\t{1}", word.ToUpper(), 1);
            }
            return sb.ToString();
        }

        private void ComputeAndDisplayTheNextStep(IEnumerator<string> enumerator)
        {
            try
            {
                if (!enumerator.MoveNext())
                {
                    CompositionTarget.Rendering -= CurrentRenderingDelegate;
                    CurrentRenderingDelegate = null;
                    stopButton.Content = "Pause";
                    stopButton.IsEnabled = false;
                    showLastCheckBox.IsEnabled = true;

                    return;
                }

                if (!showLastCheckBox.IsChecked.Value) //!!!const
                {
                    showLastCheckBox.IsChecked = !showLastCheckBox.IsChecked.Value;
                }
                SpecialFunctions.CheckCondition(showLastCheckBox.IsChecked.Value, "assert");


                outputTextBox.Text = string.Format("{0}\n{1}", CreateVaccine.CreateVaccine.DisplayHeaderString, enumerator.Current);
                showStepsBuffer.Append("\n" + enumerator.Current);
                string[] lines = outputTextBox.Text.Split('\n');
                string[] fields = lines[1].Split('\t');
                int aaLength = int.Parse(fields[1]);
                double coverage = double.Parse(fields[3]);

                AddPointAndAdjustPlotAsNecessary(aaLength, coverage);

                UpdateLayoutHeight();
            }
            catch (Exception exception)
            {
                outputTextBox.Text = "\nERROR:\n" + exception.Message;
                if (exception.InnerException != null)
                {
                    outputTextBox.Text += "\n" + exception.InnerException.Message;
                }
                showStepsBuffer.Append(outputTextBox.Text);
                CompositionTarget.Rendering -= CurrentRenderingDelegate;
                CurrentRenderingDelegate = null;
                stopButton.Content = "Pause";
                stopButton.IsEnabled = false;
                showLastCheckBox.IsEnabled = true;
                UpdateLayoutHeight();
            }
        }

        private void AddPointAndAdjustPlotAsNecessary(int aaLength, double coverage)
        {
            Ellipse point = new Ellipse();
            point.Height = 3;
            point.Width = 3;
            point.Fill = new SolidColorBrush(Colors.Black);
            double x = aaLength * PlotScale;
            if (x > plotCanvas.Width)
            {
                PlotScale = PlotScale * .5;
                x = aaLength * PlotScale;
                plotLength.Text = (plotCanvas.Width / PlotScale).ToString();
                foreach (var child in plotCanvas.Children)
                {
                    child.SetValue(Canvas.LeftProperty, (double)child.GetValue(Canvas.LeftProperty) * .5);
                }
            }
            point.SetValue(Canvas.LeftProperty, x);
            point.SetValue(Canvas.TopProperty, coverage * plotCanvas.Height * .98);
            plotCanvas.Children.Add(point);
        }

        double PlotScale = 1; //put in an class
        //StringBuilder FullResults = null;

        void loadSampleButton_Click(object sender, RoutedEventArgs e)
        {
            inputTextBox.Text = @"Patch	Weight
NKIVRMYSP	167
LNKIVRMYS	167
PQDLNTMLN	166
QDLNTMLNT	166
GATPQDLNT	166
EGATPQDLN	166
ATPQDLNTM	165
TPQDLNTML	165";
        }
        void loadTextButton_Click(object sender, RoutedEventArgs e)
        {
            inputTextBox.Text = @"Twinkle, twinkle, little star; How I wonder what you are.";
        }


    }
}
// Microsoft Research, eScience Research Group, Microsoft Reciprocal License (Ms-RL)
// Copyright (c) Microsoft Corporation. All rights reserved.
