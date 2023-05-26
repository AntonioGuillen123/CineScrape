using CineScrapeProject.Models;
using CineScrapeProject.wwwroot.Models;
using System.Net.Http.Json;

namespace CineScrapeProject.Pages
{
	public partial class Index
	{
		private enum Filters { RateCritics, RateAudience, Platforms, Runtime }

		private readonly List<TimeSpan> RUNTIMEFILTERS = new()
		{
			new(1, 30, 0),
			new(2, 0, 0),
			new(2, 30, 0),
			new(3, 0, 0)
		};

		private List<Slot> _stadistics = new();
		private Filters _filterOption = Filters.RateCritics;

		public List<Movie> MovieList { get; set; }
		public List<Slot> Stadistics { get => _stadistics; set => _stadistics = value; }
		private Filters FilterOption { get => _filterOption; set { _filterOption = value; MakeStadistics(); } }

		protected override async Task OnInitializedAsync() => MovieList = await Http.GetFromJsonAsync<List<Movie>>(Utilities.PATH);
		protected override void OnParametersSet() => MakeStadistics();

		private void MakeStadistics()
		{
			List<Slot> results = new();

			switch (FilterOption)
			{
				case Filters.RateCritics:
					results = Rate(FilterOption);
					break;
				case Filters.RateAudience:
					results = Rate(FilterOption);
					break;
				case Filters.Platforms:
					results = Platforms();
					break;
				case Filters.Runtime:
					results = Runtime();
					break;
			}

			Stadistics = results;
		}

		private List<Slot> Rate(Filters filter)
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
		private List<Slot> Platforms()
		{
			List<Platform> platforms = new();

			MovieList.ForEach(movie => movie.Platforms.ForEach(platform => platforms.Add(platform)));

			return platforms.PlatformFilter();
		}
		private List<Slot> Runtime()
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
	}

	public class Slot
	{
		private string _name;
		private int _count;

		public string Name { get => _name; set => _name = value; }
		public int Count { get => _count; set => _count = value; }
	}
}