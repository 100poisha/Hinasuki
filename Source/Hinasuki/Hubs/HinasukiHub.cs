using Hinasuki.Repositories;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Hinasuki.Hubs
{
    public class HinasukiHub : Hub
    {
        public HinasukiHub(HinasukiHubValue hinasukiHubValue, HinasukiRepository hinasukiRepository)
        {
            HinasukiHubValue = hinasukiHubValue ?? throw new ArgumentNullException(nameof(hinasukiHubValue));
            HinasukiRepository = hinasukiRepository ?? throw new ArgumentNullException(nameof(hinasukiRepository));
        }

        public override async Task OnConnectedAsync()
        {
            HinasukiHubValue.ConnectedUser();

            await Clients.Others.SendAsync("UserCount", HinasukiHubValue.UserCount);

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            HinasukiHubValue.DisconnectedUser();

            await Clients.Others.SendAsync("UserCount", HinasukiHubValue.UserCount);

            await base.OnDisconnectedAsync(exception);

        }

        public async Task GetStatus()
        {
            var count = await HinasukiRepository.GetCount();
            await Clients.Caller.SendAsync("Hinasuki", count);

            await Clients.Caller.SendAsync("UserCount", HinasukiHubValue.UserCount);
        }

        public Task AddCount()
        {
            return HinasukiHubValue.AddCount();
        }

        private HinasukiHubValue HinasukiHubValue { get; }

        private HinasukiRepository HinasukiRepository { get; }
    }

    public class HinasukiHubValue
    {
        public HinasukiHubValue(HinasukiRepository hinasukiRepository, IHubContext<HinasukiHub> hinasukiHubContext)
        {
            HinasukiRepository = hinasukiRepository ?? throw new ArgumentNullException(nameof(hinasukiRepository));
            HinasukiHubContext = hinasukiHubContext ?? throw new ArgumentNullException(nameof(hinasukiHubContext));

            NotifyTimer = new System.Timers.Timer()
            {
                AutoReset = true,
                Interval = 100,
            };
            NotifyTimer.Elapsed += NotifyTimer_Elapsed;
            NotifyTimer.Start();
        }

        public int ConnectedUser()
        {
            return Interlocked.Increment(ref _UserCount);
        }

        public int DisconnectedUser()
        {
            return Interlocked.Decrement(ref _UserCount);
        }

        public async Task AddCount()
        {
            await HinasukiRepository.AddCount();

            Interlocked.Exchange(ref IsNotify, 1);
        }

        private async void NotifyTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (IsNotify == 0) return;

            var count = await HinasukiRepository.GetCount();
            if (OldCount == count)
            {
                Interlocked.Exchange(ref IsNotify, 0);
                return;
            }

            await HinasukiHubContext.Clients.All.SendAsync("Hinasuki", count);
            OldCount = count;
        }

        private System.Timers.Timer NotifyTimer { get; }

        private HinasukiRepository HinasukiRepository { get; }

        private IHubContext<HinasukiHub> HinasukiHubContext { get; }

        private static int _UserCount = 0;

        public static int UserCount => _UserCount;

        private static int IsNotify = 0;

        private static long OldCount = long.MinValue;
    }
}
