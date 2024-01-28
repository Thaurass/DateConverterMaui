using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore;
using System.Collections.ObjectModel;

namespace DateConverter 
{
    public partial class ThirdChannel : ContentPage
    {
        public ThirdChannel()
        {
            InitializeComponent();
        }

    }

    public partial class ViewModelT3 : ObservableObject
    {
        private ObservableCollection<ObservablePoint> _thirdChannelStrong;
        private ObservableCollection<ObservablePoint> _thirdChannelWeak;
        private List<Tpoint> _pointsT;

        public ViewModelT3()
        {
            // Use ObservableCollections to let the chart listen for changes (or any INotifyCollectionChanged). 
            _thirdChannelStrong = new ObservableCollection<ObservablePoint>
            {
                // Use the ObservableValue or ObservablePoint types to let the chart listen for property changes 
                // or use any INotifyPropertyChanged implementation 
                new ObservablePoint(0, 0),
            };

            SeriesT = new ObservableCollection<ISeries>
            {
                new LineSeries<ObservablePoint>
                {
                    Name = "Третий усиленный канал",
                    Values = _thirdChannelStrong
                },
                new LineSeries<ObservablePoint>
                {
                    Name = "Третий неусиленный канал",
                    Values = _thirdChannelStrong
                }

            };
        }

        public ObservableCollection<ISeries> SeriesT { get; set; }

        [RelayCommand]
        public async void AddItemT()
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
                        ReadTfile(stream);
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

        public void ReadTfile(Stream stream)
        {
            _pointsT = new();
            _thirdChannelStrong = new();
            _thirdChannelWeak = new();
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
                        _pointsT.Add(new Tpoint(Int32.Parse(tmp[0]), Int32.Parse(tmp[1]), Int32.Parse(tmp[2]), Int32.Parse(tmp[5]), Int32.Parse(tmp[6])));
                    }

                }
            }

            SeriesT.Clear();
            _thirdChannelStrong.Clear();

            for (int i = 0; i < _pointsT.Count; i++)
            {
                _thirdChannelStrong.Add(new ObservablePoint(_pointsT[i].X, _pointsT[i].ThirdChannelStrong));
                _thirdChannelWeak.Add(new ObservablePoint(_pointsT[i].X, _pointsT[i].ThirdChannelWeak));
            }
            SeriesT.Add(new LineSeries<ObservablePoint>
            {
                Name = "Третий усиленный канал",
                Values = _thirdChannelStrong
            }
            );
            SeriesT.Add(new LineSeries<ObservablePoint>
            {
                Name = "Третий неусиленный канал",
                Values = _thirdChannelWeak
            }
            );
        }
    }
};


