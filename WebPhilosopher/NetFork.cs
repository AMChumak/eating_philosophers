namespace WebPhilosopher;
using System.Diagnostics.CodeAnalysis;
using ForkLib;
using System.Net.Http;
using System.Net.Http.Json;
using NetworkContracts;

public class NetFork: IFork
{
    private HttpClient _httpClient;
    private string _tableServiceUrl;
    private IForkOwner? _owner;

    private EventService _eventService;

    public string Owner => _owner?.GetName() ?? "";

    public required int OrderNumber { get; init; }

    public event ForkChangedOwner? OwnerChanged;

    [SetsRequiredMembers]
    public NetFork(int orderNumber, HttpClient httpClient, string tableServiceUrl, EventService eventService)
    {
        OrderNumber = orderNumber;
        _httpClient = httpClient;
        _tableServiceUrl = tableServiceUrl;
        _eventService = eventService;
    }

    public bool Take(IForkOwner candidat)
    {
        candidat.SetTakingStatus(this, TakingStatus.InProgress);

        var took = false;
        while (!took)
        {
            var request = new TakeForkRequest
            {
                PhilosopherName = candidat.GetName(),
                ForkId = OrderNumber,
            };

            var response = _httpClient.PostAsJsonAsync($"{_tableServiceUrl}/forks/take", request).Result;

            if (response.IsSuccessStatusCode)
            {
                var result = response.Content.ReadFromJsonAsync<TakeForkResponse>();

                result.Wait();

                took = result?.Result?.IsSuccess ?? false;

                if (!took)
                {
                    var r = new Random();
                    Task.Delay(r.Next(200,400));
                }
            }
        }

        return true;
    }

    public void Release(IForkOwner candidat)
    {
        candidat.SetTakingStatus(this, TakingStatus.Inaction);

        var done = false;
        while (!done)
        {
            var request = new ReleaseForksRequest
            {
                PhilosopherName = candidat.GetName(),
                ForkId = OrderNumber,
            };

            var response = _httpClient.PostAsJsonAsync($"{_tableServiceUrl}/forks/release", request).Result;

            done = response.IsSuccessStatusCode;
            if (!done)
            {
                var r = new Random();
                Task.Delay(r.Next(200,400));
            }
        }
    }
}

