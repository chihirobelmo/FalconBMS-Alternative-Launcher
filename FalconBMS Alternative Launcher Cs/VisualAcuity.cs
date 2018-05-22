using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;
// Added System.Web, System.Drawing, WindowsFormsIntegration to project reference.

namespace FalconBMS_Alternative_Launcher_Cs
{
    public class VisualAcuity
    {
        public MainWindow mainwindow;
        public int horizontalFOV_Ideal_deg;
        public int userSettingFOV_deg;

        public VisualAcuity(MainWindow mainwindow)
        {
            this.mainwindow = mainwindow;

            // Calculate Smart Scaling Factor.
            int width_px = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;
            int height_px = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;
            this.horizontalFOV_Ideal_deg = width_px / 30;

            this.SetGraph(width_px, horizontalFOV_Ideal_deg);

            mainwindow.Label_SystemResolution.Content = width_px + " * " + height_px + " pixels";
            mainwindow.Label_DesiredFOV.Content = this.horizontalFOV_Ideal_deg + " hFOV Degrees";
            mainwindow.Label_DesiredFOV_NoScaling.Content = "with Smart Scaling ON, " + this.horizontalFOV_Ideal_deg/2 + " hFOV if OFF.";

            this.mainwindow.TextBox_hFOV.Text = this.horizontalFOV_Ideal_deg.ToString();
            this.mainwindow.TextBox_widthpx.Text = width_px.ToString();

            this.userSettingFOV_deg = this.horizontalFOV_Ideal_deg;
        }

        public void SeeArbitraryFOVResult()
        {
            int hFOV = Int32.Parse(this.mainwindow.TextBox_hFOV.Text);
            int width_px = Int32.Parse(this.mainwindow.TextBox_widthpx.Text);

            if (width_px < 1)
            {
                width_px = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;
                this.mainwindow.TextBox_widthpx.Text = width_px.ToString();
            }
            if (hFOV < 1)
            {
                hFOV = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width / 30;
                this.mainwindow.TextBox_hFOV.Text = hFOV.ToString();
            }


            this.SetGraph(width_px, hFOV);

            this.userSettingFOV_deg = hFOV;
        }

        public void SetGraph(int width_px, int hFOV)
        {
            var windowsFormsHost = (System.Windows.Forms.Integration.WindowsFormsHost)mainwindow.grid1.Children[0];

            var chart = (Chart)windowsFormsHost.Child;
            ChartArea chartArea = new ChartArea();

            // Chart settings.
            chart.ChartAreas.Clear();
            chart.ChartAreas.Add("SerfossGraph");
            chart.BorderlineColor = System.Drawing.Color.Gray;
            chart.BackColor = System.Drawing.ColorTranslator.FromHtml("#D4E1EE");

            chart.ChartAreas["SerfossGraph"].BackColor = System.Drawing.Color.Transparent;
            chart.ChartAreas["SerfossGraph"].AxisX.Title = "Nautical Miles [nm]";
            chart.ChartAreas["SerfossGraph"].AxisX.Minimum = 0.0;
            chart.ChartAreas["SerfossGraph"].AxisX.Interval = 0.5;
            chart.ChartAreas["SerfossGraph"].AxisX.Maximum = 5.0;
            chart.ChartAreas["SerfossGraph"].AxisY.Title = "Real Life F-16 Visible Width [min]\nBMS F-16 Visible Width [px]";
            chart.ChartAreas["SerfossGraph"].AxisY.Minimum = 0;
            chart.ChartAreas["SerfossGraph"].AxisY.Interval = 5;
            chart.ChartAreas["SerfossGraph"].AxisY.Maximum = 100;
            chart.ChartAreas["SerfossGraph"].AxisY2.Title = "Magnification Factor [multiples]";
            chart.ChartAreas["SerfossGraph"].AxisY2.Minimum = 1;
            chart.ChartAreas["SerfossGraph"].AxisY2.Interval = 0;
            chart.ChartAreas["SerfossGraph"].AxisY2.Maximum = 3.0;

            // Adding Series and values.
            Series realLife = new Series();
            realLife.Name = "Real Life Angular Size [min]";
            realLife.ChartType = SeriesChartType.Line;
            realLife.MarkerStyle = MarkerStyle.Circle;
            realLife.Color = System.Drawing.Color.DarkBlue;

            Series noSmartScaling = new Series();
            noSmartScaling.Name = "BMS Smart Scaling OFF [px]";
            noSmartScaling.ChartType = SeriesChartType.Line;
            noSmartScaling.MarkerStyle = MarkerStyle.Circle;
            noSmartScaling.Color = System.Drawing.Color.MediumPurple;

            Series smartScaling = new Series();
            smartScaling.Name = "BMS Smart Scaling ON [px]";
            smartScaling.ChartType = SeriesChartType.Line;
            smartScaling.MarkerStyle = MarkerStyle.Circle;
            smartScaling.Color = System.Drawing.Color.DarkOrange;

            Series magnificationFactor = new Series();
            magnificationFactor.Name = "Smart Scaling Factor [multiples]";
            magnificationFactor.ChartType = SeriesChartType.Line;
            magnificationFactor.MarkerStyle = MarkerStyle.Circle;
            magnificationFactor.Color = System.Drawing.Color.DarkGray;
            magnificationFactor.YAxisType = AxisType.Secondary;

            Legend leg = new Legend();
            leg.DockedToChartArea = "SerfossGraph";
            leg.Alignment = System.Drawing.StringAlignment.Near;
            leg.BackColor = System.Drawing.ColorTranslator.FromHtml("#D4E1EE");

            for (double distance_NM = 0.1; distance_NM < 5.0; distance_NM = distance_NM + 0.1)
            {
                double F16length_meter = 15.06;

                double distance_ft = distance_NM * 6000;
                double distance_km = distance_NM * 1.852;

                double angularSize_mil = F16length_meter / distance_km;
                double angularSize_minutes = angularSize_mil * 3.375;     // Real Life

                double angularSize_noSmartScaling_minutes = width_px * angularSize_minutes / (hFOV * 60);     // BMS Disable Smart Scaling
                double smartScalingFactor = 1 + 0.09226 * (distance_ft / 1000) - 0.00148 * (distance_ft / 1000) * (distance_ft / 1000);
                if (smartScalingFactor < 1)
                    smartScalingFactor = 1;

                double angularSize_SmartScaling_minutes = angularSize_noSmartScaling_minutes * smartScalingFactor;     // BMS Enable Smart Scaling

                realLife.Points.AddXY(distance_NM, angularSize_minutes);
                noSmartScaling.Points.AddXY(distance_NM, angularSize_noSmartScaling_minutes);
                smartScaling.Points.AddXY(distance_NM, angularSize_SmartScaling_minutes);
                magnificationFactor.Points.AddXY(distance_NM, smartScalingFactor);
            }

            chart.Series.Clear();
            chart.Series.Add(magnificationFactor);
            chart.Series.Add(realLife);
            chart.Series.Add(noSmartScaling);
            chart.Series.Add(smartScaling);
            chart.Legends.Add(leg);
        }
    }
}
