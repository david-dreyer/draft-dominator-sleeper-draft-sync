using DraftDominatorSleeperLiveDraftSync;

HttpClient httpClient = new();
Configuration configuration = CredentialsHelper.Get();
DraftDominator draftDominator = new(configuration);
SleeperDraftSync sleeperDraftSync = new(httpClient, configuration, draftDominator);

CancellationTokenSource cancellationTokenSource = new();
Console.CancelKeyPress += (_, e) =>
{
    Console.WriteLine("Stopping...");
    cancellationTokenSource.Cancel();
    e.Cancel = true;
};

// Does not work in headless mode, haven't look into why. I know the DD site doesn't like to load for limited browsers
await draftDominator.Initialize(headless: false);
await sleeperDraftSync.Run(cancellationTokenSource.Token);