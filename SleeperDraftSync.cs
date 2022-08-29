using DraftDominatorSleeperLiveDraftSync.SleeperApi;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace DraftDominatorSleeperLiveDraftSync;

public class SleeperDraftSync
{
    public SleeperDraftSync(HttpClient httpClient, Configuration configuration, DraftDominator draftDominator)
    {
        this.httpClient = httpClient;
        this.configuration = configuration;
        this.draftDominator = draftDominator;
    }

    public async Task Run(CancellationToken cancellationToken)
    {
        await SetSleeperUserId(cancellationToken);
        await SetDraftId(cancellationToken);
        await MonitorSleeperDraft(cancellationToken);
    }

    private async Task MonitorSleeperDraft(CancellationToken cancellationToken)
    {
        bool draftStarted = false;
        while (!cancellationToken.IsCancellationRequested)
        {
            if (!draftStarted)
            {
                draftStarted = await CheckIfDraftStarted(cancellationToken);
            }
            else
            {
                await CheckForNewPicks(cancellationToken);
            }
            
            await Task.Delay(
                draftStarted ? StartedDelay : NotStartedDelay,
                cancellationToken);
        }
    }

    private async Task CheckForNewPicks(CancellationToken cancellationToken)
    {
        try
        {
            HttpResponseMessage responseMessage =
                await httpClient.GetAsync(
                    $"{SleeperApiRoot}/{Draft}/{sleeperDraftId}/{Picks}",
                    cancellationToken);

            responseMessage.EnsureSuccessStatusCode();

            DraftPick[] draftPicks = 
                JsonConvert.DeserializeObject<DraftPick[]>(
                    await responseMessage.Content.ReadAsStringAsync(cancellationToken),
                    JsonSerializerSettings)!;

            if (draftPicks.Length > lastKnownPick)
            {
                await draftDominator.AddDraftPicks(
                    draftPicks.OrderBy(d => d.PickNo).Skip(lastKnownPick),
                    cancellationToken);
                lastKnownPick = draftPicks.Length;
            }
        }
        catch (HttpRequestException)
        {
            // Failed to call API, we'll just try again
        }
        catch (Exception)
        {
            // Didn't get back expected data from API, we'll just try again
        }
    }

    private async Task<bool> CheckIfDraftStarted(CancellationToken cancellationToken)
    {
        HttpResponseMessage responseMessage = 
            await httpClient.GetAsync(
                $"{SleeperApiRoot}/{Draft}/{sleeperDraftId}",
                cancellationToken);

        responseMessage.EnsureSuccessStatusCode();

        string text = await responseMessage.Content.ReadAsStringAsync(cancellationToken);

        if (text == "null")
            throw new ApplicationException("Could not find draft!");

        return (text.StartsWith('[') ?
            JArray.Parse(text)[0]["status"]!.Value<string>() :
            JObject.Parse(text)["status"]!.Value<string>())! != "pre_draft";
    }

    private async Task SetDraftId(CancellationToken cancellationToken)
    {
        sleeperDraftId = "869611818828591104";
        HttpResponseMessage responseMessage = 
            await httpClient.GetAsync(
                $"{SleeperApiRoot}/{User}/{sleeperUserId}/{NflDrafts}",
                cancellationToken);
        
        responseMessage.EnsureSuccessStatusCode();
        
        string text = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
        
        if (string.IsNullOrWhiteSpace(text) || text == "[]")
            throw new ApplicationException("Could not find any drafts!");

        JArray draftsArray = JArray.Parse(text);
        foreach (JObject draft in draftsArray.OfType<JObject>())
        {
            string? leagueName = draft["metadata"]!["name"]!.Value<string>();
            if (leagueName!.Replace('’', '\'').Equals(configuration.LeagueName, StringComparison.CurrentCultureIgnoreCase))
            {
                sleeperDraftId = draft["draft_id"]!.Value<string>()!;
                break;
            }
        }
    }

    private async Task SetSleeperUserId(CancellationToken cancellationToken)
    {
        HttpResponseMessage responseMessage = 
            await httpClient.GetAsync(
                $"{SleeperApiRoot}/{User}/{configuration.SleeperUsername}",
                cancellationToken);

        responseMessage.EnsureSuccessStatusCode();

        string text = await responseMessage.Content.ReadAsStringAsync(cancellationToken);

        if (text == "null")
            throw new ApplicationException($"Sleeper Username {configuration.SleeperUsername} does not exist");

        sleeperUserId = JObject.Parse(text)["user_id"]!.Value<string>()!;
    }
    
    private const string SleeperApiRoot = "https://api.sleeper.app/v1";
    private const string User = "user";
    private const string NflDrafts = "drafts/nfl/2022";
    private const string Draft = "draft";
    private const string Picks = "picks";
    private string? sleeperUserId;
    private string? sleeperDraftId;
    private int lastKnownPick;
    private readonly Configuration configuration;
    private readonly DraftDominator draftDominator;
    private readonly HttpClient httpClient;
    private static readonly TimeSpan NotStartedDelay = TimeSpan.FromMinutes(1);
    private static readonly TimeSpan StartedDelay = TimeSpan.FromSeconds(10);

    private static readonly JsonSerializerSettings JsonSerializerSettings =
        new()
        {
            ContractResolver = new SnakeCasePropertyNamesContractResolver()
        };

    private class SnakeCasePropertyNamesContractResolver : DefaultContractResolver
    {
        public SnakeCasePropertyNamesContractResolver() => 
            NamingStrategy = new SnakeCaseNamingStrategy();
    }    
}