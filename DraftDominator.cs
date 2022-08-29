using DraftDominatorSleeperLiveDraftSync.SleeperApi;
using Microsoft.Playwright;

namespace DraftDominatorSleeperLiveDraftSync;

public class DraftDominator
{
    public DraftDominator(Configuration configuration)
    {
        this.configuration = configuration;
    }

    public async Task Initialize(bool headless)
    {
        IPlaywright playwright = await Playwright.CreateAsync();
        browser = await playwright.Chromium.LaunchAsync(
            new BrowserTypeLaunchOptions
            {
                Headless = headless
            });

        page = await browser.NewPageAsync();
        await page.GotoAsync(DraftDominatorUrl);

        await page.FillAsync("input[name='login']", configuration.FootballGuysUsername);
        await page.FillAsync("input[name='password']", configuration.FootballGuysPassword);
        await page.Locator(":nth-match(span.x-button-label:text('Log In'), 2)").ClickAsync();

        await page.Locator("span.x-button-label", new PageLocatorOptions { HasTextString = "Switch Draft" }).ClickAsync();
        await page.Locator("span.title", new PageLocatorOptions { HasTextString = "Add New Draft" }).ClickAsync();
        await page.Locator("div.label", new PageLocatorOptions { HasTextString = configuration.LeagueName.Replace('\'', '’') }).ClickAsync();
        await page.Locator("span.x-label-text-el", new PageLocatorOptions { HasTextString = "Real Draft" }).ClickAsync();
        await Task.Delay(2000);
        await page.Locator("span.x-button-label", new PageLocatorOptions { HasTextString = "Create Draft" }).ClickAsync();
        await page.Locator("span.x-button-label", new PageLocatorOptions { HasTextString = "Got It!" }).ClickAsync();
        await page.Locator("span.x-button-label", new PageLocatorOptions { HasTextString = "Start Draft" }).ClickAsync();
    }
    
    public async Task AddDraftPicks(IEnumerable<DraftPick> draftPicks, CancellationToken cancellationToken)
    {
        foreach (DraftPick draftPick in draftPicks)
        {
            if (cancellationToken.IsCancellationRequested)
                break;
            
            await page.FillAsync(
                "input[type='search']", 
                $"{draftPick.Metadata.FirstName.Replace('’', '\'')} {draftPick.Metadata.LastName.Replace('’', '\'')}");
            // Wait for search to happen
            await Task.Delay(2500, cancellationToken);
            await page.Locator(":nth-match([data-componentid='ext-container-53'] div.player-label, 1)").ClickAsync();
            await page.Locator("span.x-button-label", new PageLocatorOptions { HasTextString = "Pick" }).ClickAsync();
        }
    }
    
    private readonly Configuration configuration;
    private IBrowser? browser;
    private IPage page;

    private const string DraftDominatorUrl = "https://draft.footballguys.com/#fbg/fbgcloud";
}