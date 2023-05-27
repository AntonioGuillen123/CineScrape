using BlazorBootstrap;
using ChartJs.Blazor.Common;
using ChartJs.Blazor.PieChart;
using ChartJs.Blazor.PolarAreaChart;
using ChartJs.Blazor.Util;
using CineScrapeProject.Models;
using CineScrapeProject.wwwroot.Models;
using System.Net.Http.Json;

namespace CineScrapeProject.Pages
{
	public partial class Index
	{
		private enum Filters { RateCritics, RateAudience, Platforms, Runtime, Genders }

		private readonly List<TimeSpan> RUNTIMEFILTERS = new()
		{
			new(1, 30, 0),
			new(2, 0, 0),
			new(2, 30, 0),
			new(3, 0, 0)
		};

		List<ToastMessage> messages = new();

		private PieConfig _pieConfig = new();
		private PieConfig _doughConfig = new();
		private PolarAreaConfig _polarConfig = new();
		private List<Slot> _stadistics = new();
		private Filters _filterOption = Filters.RateCritics;
		private List<Slot> _genders = new();
		private bool prueba = false;

		public List<Movie> MovieList { get; set; }
		public List<Slot> Stadistics { get => _stadistics; set => _stadistics = value; }
		private Filters FilterOption { get => _filterOption; set { _filterOption = value; MakeStadistics(); } }

		public List<Slot> Genders { get => _genders; set => _genders = value; }
		public PieConfig PieConfig { get => _pieConfig; set => _pieConfig = value; }
		public PieConfig DoughConfig { get => _doughConfig; set => _doughConfig = value; }
		public PolarAreaConfig PolarConfig { get => _polarConfig; set => _polarConfig = value; }

		protected override async Task OnInitializedAsync()
		{
			MovieList = await Http.GetFromJsonAsync<List<Movie>>(Utilities.PATH);

			Genders = MovieList.GenderFilter();

			ShowMessage(ToastType.Success);

		}


		private async Task CreateCharts()
		{
			PieConfig = new PieConfig
			{
				Options = new PieOptions
				{
					Responsive = true,
					Title = new OptionsTitle
					{
						Display = false,
						Text = "ChartJs.Blazor Pie Chart"
					}
				}
			};

			DoughConfig = new PieConfig
			{
				Options = new PieOptions
				{
					Responsive = true,
					CutoutPercentage = 50,
					Title = new OptionsTitle
					{
						Display = true,
						Text = "ChartJs.Blazor Pie Chart"
					}
				}
			};
		}
		protected override async Task OnParametersSetAsync()
		{
			await MakeStadistics();
		}

		private async Task FillCharts(Filters filterSelected)
		{

			switch (FilterOption)
			{
				case Filters.Platforms:
					prueba = true;
					FillDoughChart();
					break;
				case Filters.Runtime:

					break;
				case Filters.Genders:

					break;
				default:
					FillPieChart();
					break;
			}


		}


		protected override void OnInitialized()
		{
			CreateCharts();
		}
		private async Task MakeStadistics()
		{
			List<Slot> results = new();

			switch (FilterOption)
			{
				case Filters.RateCritics:
					results = RateStats(FilterOption);
					break;
				case Filters.RateAudience:
					results = RateStats(FilterOption);
					break;
				case Filters.Platforms:
					results = PlatformsStats();
					break;
				case Filters.Runtime:
					results = RuntimeStats();
					break;
				case Filters.Genders:
					results = Genders;
					break;
			}

			Stadistics = results;


			await FillCharts(FilterOption);
		}

		private List<Slot> RateStats(Filters filter)
		{
			List<Slot> results = new()
			{
				new() { Name = "No rate or less than or equal 25%"},
				new() { Name = "Rate 26% - 50%"},
				new() { Name = "Rate 51% - 75%"},
				new() { Name = "Rate 76% - 100%"},
			};

			MovieList.ForEach(movie =>
			{
				int? property = filter == Filters.RateCritics ? movie.RateCritic : movie.RateAudience;

				if (!property.HasValue || property.Value <= 25)
				{
					results[0].Count++;
				}
				else
				{
					int value = property.Value;

					if (value <= 50)
					{
						results[1].Count++;
					}
					else if (value <= 75)
					{
						results[2].Count++;
					}
					else
					{
						results[3].Count++;
					}
				}
			});

			return results;
		}
		private List<Slot> PlatformsStats()
		{
			List<Platform> platforms = new();

			MovieList.ForEach(movie => movie.Platforms.ForEach(platform => platforms.Add(platform)));

			return platforms.PlatformFilter();
		}
		private List<Slot> RuntimeStats()
		{
			List<Slot> results = new()
			{
				new() { Name =  "Runtime less than or equal 1h 30m"},
				new() { Name =  "Runtime 1h 30m - 2h" },
				new() { Name =  "Runtime 2h - 2h 30m" },
				new() { Name =  "Runtime 2h 30m - 3h" },
				new() { Name =  "Runtime greater than 3h" }
			};

			List<TimeSpan> times = GetRuntimes();

			times.ForEach(time =>
			{
				if (time <= RUNTIMEFILTERS[0])
				{
					results[0].Count++;
				}
				else if (time <= RUNTIMEFILTERS[1])
				{
					results[1].Count++;
				}
				else if (time <= RUNTIMEFILTERS[2])
				{
					results[2].Count++;
				}
				else if (time <= RUNTIMEFILTERS[3])
				{
					results[3].Count++;
				}
				else
				{
					results[4].Count++;
				}
			});

			return results;
		}

		private List<TimeSpan> GetRuntimes()
		{
			List<TimeSpan> runtimes = new();

			MovieList.ForEach(movie =>
			{
				if (movie.Characteristics.TryGetValue("Runtime", out string value))
				{
					string[] values = value.Split(' ');

					int hours = 0, minutes = 0;

					for (int i = 0; i < values.Length; i++)
					{
						if (values[i].Contains('h')) hours = int.Parse(values[i].TrimEnd('h'));
						if (values[i].Contains('m')) minutes = int.Parse(values[i].TrimEnd('m'));
					}

					TimeSpan timeSpan = new TimeSpan(hours, minutes, 0);

					runtimes.Add(timeSpan);
				}
			});

			return runtimes;
		}
		private void ShowMessage(ToastType toastType) => messages.Add(CreateToastMessage(toastType));

		private ToastMessage CreateToastMessage(ToastType toastType)
		=> new ToastMessage
		{
			Type = toastType,
			Title = "Blazor Bootstrap",
			HelpText = $"{DateTime.Now}",
			Message = $"Hello, world! This is a toast message. DateTime: {DateTime.Now}",
		};

		private void FillPieChart()
		{
			PieConfig.Data.Labels.Clear();
			PieConfig.Data.Datasets.Clear();

			foreach (Slot slot in Stadistics)
			{
				PieConfig.Data.Labels.Add(slot.Name);
			}

			PieDataset<int> dataset = new PieDataset<int>()
			{

				BackgroundColor = new[]
				{
							ColorUtil.ColorHexString(255, 99, 132), // Slice 1 aka "Red"
							ColorUtil.ColorHexString(255, 205, 86), // Slice 2 aka "Yellow"
							ColorUtil.ColorHexString(75, 192, 192), // Slice 3 aka "Green"
							ColorUtil.ColorHexString(54, 162, 235), // Slice 4 aka "Blue"
						}
			};
			foreach (Slot slot in Stadistics)
			{
				dataset.Add(slot.Count);
			}
			PieConfig.Data.Datasets.Add(dataset);
		}

		private void FillDoughChart()
		{
			DoughConfig.Data.Labels.Clear();
			DoughConfig.Data.Datasets.Clear();

			foreach (Slot slot in Stadistics)
			{
				DoughConfig.Data.Labels.Add(slot.Name);
			}

			PieDataset<int> dataset = new PieDataset<int>()
			{

				BackgroundColor = new[]
				{
							ColorUtil.ColorHexString(255, 99, 132), // Slice 1 aka "Red"
							ColorUtil.ColorHexString(255, 205, 86), // Slice 2 aka "Yellow"
							ColorUtil.ColorHexString(75, 192, 192), // Slice 3 aka "Green"
							ColorUtil.ColorHexString(54, 162, 235), // Slice 4 aka "Blue"
							ColorUtil.RandomColorString(),
							ColorUtil.RandomColorString(),
							ColorUtil.RandomColorString(),
							ColorUtil.RandomColorString(),
							ColorUtil.RandomColorString(),
							ColorUtil.RandomColorString(),
							ColorUtil.RandomColorString(),
							ColorUtil.RandomColorString(),
							ColorUtil.RandomColorString()
				}
			};
			foreach (Slot slot in Stadistics)
			{
				dataset.Add(slot.Count);
			}
			DoughConfig.Data.Datasets.Add(dataset);
		}
	}
}