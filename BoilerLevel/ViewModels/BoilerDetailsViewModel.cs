using BoilerLevel.Localization;
using BoilerLevel.Messages;
using BoilerLevel.Models;
using BoilerLevel.Utils;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using OpenExtensions.Android.Services;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BoilerLevel.ViewModels
{
    public class BoilerDetailsViewModel : ViewModelBase
    {
        public Boiler Boiler { get; private set; }
        public PlotModel PlotModel { get; set; }

        public BoilerDetailsViewModel(Boiler boiler)
        {
            Boiler = boiler;

            Messenger.Default.Send(new TitleMessage(boiler.Name));

            AddMeasurmentCommand = new RelayCommand<Measurment>(AddMeasurment, CanAddMeasurment);
            AddTemplateCommand = new RelayCommand<List<string>>(AddTemplate, CanAddTemplate);
        }

        public Task<PlotModel> InitiatePlotModel()
        {
            PlotModel = new PlotModel();

            var color = ThemeService.IsDarkTheme() ? OxyColor.FromRgb(255, 255, 255) : OxyColor.FromRgb(0, 0, 0);
            var minValue = Boiler.Measurments.Count > 0 ? DateTimeAxis.ToDouble(Boiler.Measurments[0].DateTime.AddDays(-1)) : DateTimeAxis.ToDouble(DateTime.Now);
            var markSize = 4;
            var markType = MarkerType.Circle;

            PlotModel.TextColor = color;
            PlotModel.PlotAreaBorderColor = color;
            //Y axis
            PlotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                Minimum = 0,
                MaximumPadding = 1,
                MajorGridlineStyle = LineStyle.Dash,
                MajorGridlineColor = color
            });
            //X axis
            PlotModel.Axes.Add(new DateTimeAxis
            {
                Title ="Dates",
                Position = AxisPosition.Bottom,
                Minimum = DateTimeAxis.ToDouble(DateTime.Now.AddDays(-2)),
                Maximum = DateTimeAxis.ToDouble(DateTime.Now.AddDays(2)),
                MajorGridlineStyle = LineStyle.Dash,
                MajorGridlineColor = color,
                StringFormat = "dd/MM/yyyy"
            });
            //Create Series
            var TempLineSeries = new LineSeries
            {
                Title = CodeResources.Temperature,
                MarkerSize = markSize,
                MarkerType = markType
            };
            var LevelLineSeries = new LineSeries
            {
                Title = CodeResources.Level,
                MarkerSize = markSize,
                MarkerType = markType
            };

            PlotModel.Series.Add(TempLineSeries);
            PlotModel.Series.Add(LevelLineSeries);
            if (Boiler.Template != null)
                foreach (var key in Boiler.Template)
                {
                    PlotModel.Series.Add(new LineSeries
                    {
                        Title = key,
                        MarkerSize = markSize,
                        MarkerType = markType
                    });
                }
            //Ade measurments to the Series
            foreach (var measur in Boiler.Measurments)
            {
                TempLineSeries.Points.Add(new DataPoint(DateTimeAxis.ToDouble(measur.DateTime), measur.Temperature));
                LevelLineSeries.Points.Add(new DataPoint(DateTimeAxis.ToDouble(measur.DateTime), measur.Level));

                if (Boiler.Template != null)
                {
                    foreach (var key in Boiler.Template)
                    {
                        if (measur.Values != null && measur.Values.ContainsKey(key))
                        {
                            var series = (LineSeries)PlotModel.Series.First(x => x.Title == key);
                            series.Points.Add(new DataPoint(DateTimeAxis.ToDouble(measur.DateTime), measur.Values[key]));
                        }
                    }
                }
            }
            //return the new plotmodel
            return Task.Run(() => PlotModel);
        }

        public async Task<string> ShareExcelFile(string filename)
        {
            var excelManager = await CreateExcel();
            return excelManager.Save(filename);
        }

        public async Task SaveExcelFile(FileStream fileStream)
        {
            var excelManager = await CreateExcel();
            excelManager.SaveAs(fileStream);
        }

        private Task<ExcelManager> CreateExcel()
        {
            return Task.Run(() =>
            {
                var excelManager = new ExcelManager();
                excelManager
                    .AddTable(CodeResources.Temperature, Boiler.Measurments.Select(x => x.DateTime.ToString()), Boiler.Measurments.Select(x => x.Temperature))
                    .AddTable(CodeResources.Level, Boiler.Measurments.Select(x => x.DateTime.ToString()), Boiler.Measurments.Select(x => x.Level));

                if (Boiler.Template != null)
                {
                    foreach (var key in Boiler.Template)
                    {
                        var measurments = Boiler.Measurments.Where(x => x.Values != null && x.Values.ContainsKey(key));
                        excelManager.AddTable(key, measurments.Select(x => x.DateTime.ToString()), measurments.Select(x => x.Values[key]));
                    }
                }
                return excelManager;
            });
        }

        #region Commands

        public RelayCommand<Measurment> AddMeasurmentCommand;

        private void AddMeasurment(Measurment measurement)
        {
            MeasurementManager.AddMeasurement(measurement);
            var tempSeries = (LineSeries)PlotModel.Series[0];
            var levelSeries = (LineSeries)PlotModel.Series[1];

            tempSeries.Points.Add(new DataPoint(DateTimeAxis.ToDouble(measurement.DateTime), measurement.Temperature));
            levelSeries.Points.Add(new DataPoint(DateTimeAxis.ToDouble(measurement.DateTime), measurement.Level));

            if (Boiler.Template != null)
            {
                foreach (var key in Boiler.Template)
                {
                    var series = (LineSeries)PlotModel.Series.First(x => x.Title == key);
                    series.Points.Add(new DataPoint(DateTimeAxis.ToDouble(measurement.DateTime), measurement.Values[key]));
                }
            }

            PlotModel.InvalidatePlot(true);
        }

        private bool CanAddMeasurment(Measurment measurment)
        {
            if (measurment != null)
                return true;
            return false;
        }

        public RelayCommand<List<string>> AddTemplateCommand;

        private void AddTemplate(List<string> template)
        {
            Boiler.Template = template;
            BoilerManager.UpdateBoiler(Boiler);

            for (int i = 2; i < PlotModel.Series.Count;)
            {
                var item = PlotModel.Series[i];
                if (!template.Contains(item.Title))
                    PlotModel.Series.Remove(item);
                else
                    i++;
            }

            foreach (var key in template)
            {
                if (!PlotModel.Series.Any(x => x.Title == key))
                {
                    var series = new LineSeries { Title = key, MarkerType = MarkerType.Circle, MarkerSize = 4 };
                    PlotModel.Series.Add(series);
                    foreach (var measur in Boiler.Measurments)
                    {
                        if (measur.Values != null && measur.Values.ContainsKey(key))
                            series.Points.Add(new DataPoint(DateTimeAxis.ToDouble(measur.DateTime), measur.Values[key]));
                    }
                }
            }

            PlotModel.InvalidatePlot(true);
        }

        private bool CanAddTemplate(List<string> template)
        {
            if (template != null || template.Count != 0 || !template.SequenceEqual(Boiler.Template))
                return true;
            return false;
        }
        #endregion
    }
}