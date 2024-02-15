using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Tgs.VipLevels.Level.Core.ApplicationServices;

namespace Tgs.VipLevels.Level.Core;

internal class LevelUpgradeDispatcher(IServiceProvider serviceProvider) : ILevelUpgradeDispatcher, IDisposable, IAsyncDisposable
{
	private readonly ConcurrentDictionary<string, LevelUpgradeChannel> _counters = new();
	private readonly CancellationTokenSource _cancellationTokenSource = new();

	private bool _disposedValue;

	~LevelUpgradeDispatcher()
	{
		Dispose(false);
	}

	public async ValueTask PushCommandAsync(string playerCode, LevelUpgradeCommand command, CancellationToken cancellationToken = default)
	{
		using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _cancellationTokenSource.Token);

		var counter = _counters.GetOrAdd(
			playerCode,
			key =>
			{
				var scope = serviceProvider.CreateScope();
				var newCounter = ActivatorUtilities.CreateInstance<LevelUpgradeChannel>(
					scope.ServiceProvider,
					key);

				newCounter.Disposed += (_, _) =>
				{
					_ = _counters.TryRemove(newCounter.PlayerCode, out _);
					scope.Dispose();
				};

				return newCounter;
			});

		await counter.PrepareAsync(cts.Token).ConfigureAwait(false);
		await counter.PushCommandAsync(command, cts.Token).ConfigureAwait(false);
	}

	public void ExtendExpiry(string playerCode)
	{
		if (_counters.TryGetValue(playerCode, out var counter))
			counter.ExtendExpiry();
	}

	public ValueTask DisposeAsync()
	{
		Dispose(true);
		GC.SuppressFinalize(this);

		return ValueTask.CompletedTask;
	}

	public void Dispose()
	{
		// 請勿變更此程式碼。請將清除程式碼放入 'Dispose(bool disposing)' 方法
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			if (disposing)
			{
				_cancellationTokenSource.Cancel();
			}

			_cancellationTokenSource.Dispose();
			_disposedValue = true;
		}
	}
}