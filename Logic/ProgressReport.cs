using System;
using System.Threading;

namespace NMS_EnglishAlienWordsMod_Avalonia.Logic
{
	public class ProgressReport
	{
		public int? Percent { get; set; }
		public int? Increment { get; set; }
		public string? Message { get; set; }
	}

	public static partial class App
	{
		public static class ProgressContext
		{
			private static readonly AsyncLocal<IProgress<ProgressReport>?> _current = new();

			public static IProgress<ProgressReport>? Current => _current.Value;

			// Возвращает IDisposable, который при Dispose вернёт предыдущий прогресс
			public static IDisposable Set(IProgress<ProgressReport>? progress)
			{
				return new Reverter(progress);
			}

			private sealed class Reverter : IDisposable
			{
				private readonly IProgress<ProgressReport>? _previous;

				public Reverter(IProgress<ProgressReport>? newProgress)
				{
					_previous = _current.Value;
					_current.Value = newProgress;
				}

				public void Dispose()
				{
					_current.Value = _previous;
				}
			}
		}
	}
}
