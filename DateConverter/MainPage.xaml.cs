using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;

namespace DateConverter
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

        }

        public async void DisplayError(string _errMsg)
        {
            await DisplayAlert("Ошибка!", _errMsg, "OK");
        }
    }

    public partial class ViewModelAA : ObservableObject
    {
        private readonly ObservableCollection<ObservablePoint> _firstChannel;
        private ObservableCollection<ObservablePoint> _thirdChannel;
        private List<AApoint> _pointsAA;

        public ViewModelAA()
        {
            // Use ObservableCollections to let the chart listen for changes (or any INotifyCollectionChanged). 
            _firstChannel = new ObservableCollection<ObservablePoint>
            {
                // Use the ObservableValue or ObservablePoint types to let the chart listen for property changes 
                // or use any INotifyPropertyChanged implementation 
                new ObservablePoint(0, 0),
            };

            SeriesAA = new ObservableCollection<ISeries>
            {
                new LineSeries<ObservablePoint>
                {
                    Name = "Первый канал",
                    Values = _firstChannel,
                    Fill = null,
                    GeometrySize = 0,
                    LineSmoothness = 0
                },
                new LineSeries<ObservablePoint>
                {
                    Name = "Третий канал",
                    Values = _firstChannel,
                    Fill = null,
                    GeometrySize = 0,
                    LineSmoothness = 0
                }
            };
        }

        public ObservableCollection<ISeries> SeriesAA { get; set; }

        [RelayCommand]
        public async void AddItemAA()
        {
            var customFileType = new FilePickerFileType(
                new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.iOS, new[] { ".log" } },
                    { DevicePlatform.Android, new[] { ".log" } },
                    { DevicePlatform.WinUI, new[] { ".log" } },
                    { DevicePlatform.Tizen, new[] { ".log" } },
                    { DevicePlatform.macOS, new[] { ".log" } },
                });

            PickOptions options = new()
            {
                PickerTitle = "Выберите файл с расширением log",
                FileTypes = customFileType,
            };

            await PickAndShow(options);

            
        }

        public async Task<FileResult> PickAndShow(PickOptions options)
        {
            try
            {
                var result = await FilePicker.Default.PickAsync(options);
                if (result != null)
                {
                    if (result.FileName.EndsWith("log", StringComparison.OrdinalIgnoreCase))
                    {
                        using var stream = await result.OpenReadAsync();
                        ReadAAfile(stream);
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                //await MainPage.DisplayError(ex.Message);
            }

            return null;
        }

        public void ReadAAfile(Stream stream)
        {
            _pointsAA = new();
            _thirdChannel = new();
            StreamReader sr = new StreamReader(stream);
            string line;
            // Read and display lines from the file until the end of
            // the file is reached.
            bool b = true;
            while ((line = sr.ReadLine()) != null)
            {
                if (b || line == null)
                {
                    b = false;
                }
                else
                {
                    line = System.Text.RegularExpressions.Regex.Replace(line, @"\s+", " ").Trim();
                    string[] tmp = line.Split(' ');
                    if (line != "\n") 
                    {
                        _pointsAA.Add(new AApoint(Double.Parse(tmp[0], CultureInfo.InvariantCulture), Int32.Parse(tmp[1]), Int32.Parse(tmp[3])));
                    }
                    
                }
            }

            SeriesAA.Clear();
            _firstChannel.Clear();

            for (int i = 0; i < _pointsAA.Count; i++)
            {
                _firstChannel.Add(new ObservablePoint(_pointsAA[i].X, _pointsAA[i].FirstChannel));
                _thirdChannel.Add(new ObservablePoint(_pointsAA[i].X, _pointsAA[i].ThirdChannel));
            }

            SeriesAA.Add(new LineSeries<ObservablePoint>
            {
                Name = "Первый канал",
                Values = _firstChannel,
                Fill = null,
                GeometrySize = 0,
                LineSmoothness = 0
            }
            );
            SeriesAA.Add(new LineSeries<ObservablePoint>
            {
                Name = "Третий канал",
                Values = _thirdChannel,
                Fill = null,
                GeometrySize = 0,
                LineSmoothness = 0
            }
            );
        }
    }

   

    public class AApoint
    {
        public AApoint(double _x, int _firstChannel, int _thirdChannel)
        {
            X = _x;
            FirstChannel = _firstChannel;
            ThirdChannel = _thirdChannel;
        }

        public double X { get; }
        public double FirstChannel { get; }
        public double ThirdChannel { get; }
    }

}
