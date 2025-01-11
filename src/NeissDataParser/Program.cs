using System.Globalization;
using ConsoleAppFramework;
using FishyFlip;
using FishyFlip.Lexicon;
using FishyFlip.Models;
using FishyFlip.Tools;
using NeissDataParser;

var app = ConsoleApp.Create();
app.Add<AppCommands>();
app.Run(args);

/// <summary>
/// App Commands.
/// </summary>
#pragma warning disable SA1649 // File name should match first type name
public class AppCommands
#pragma warning restore SA1649 // File name should match first type name
{
    private const string db = "neiss.db";

    /// <summary>
    /// Insert data into SQLite database.
    /// </summary>
    /// <param name="tsvFile">TSV File.</param>
    /// <param name="formatFile">-f, Format File.</param>
    /// <param name="verbose">-v, Verbose logging.</param>
    /// <param name="cancellationToken">Cancellation Token.</param>
    /// <returns>Task.</returns>
    [Command("insert")]
    public async Task InsertDataAsync([Argument] string tsvFile, string formatFile, bool verbose = false, CancellationToken cancellationToken = default)
    {
        var log = new ConsoleLog(verbose);
        log.Log("Inserting data...");

        ProductCodeLookup.Initialize(formatFile);
        DiagnosisCodeLookup.Initialize(formatFile);
        var data = NeissParser.ParseTsvFile(tsvFile);
        log.Log($"Parsed {data.Count} records");
        var db = new DatabaseService(AppCommands.db);
        for (int i = 0; i < data.Count; i++)
        {
            IncidentRecord? record = data[i];
            record.Product1Name = ProductCodeLookup.GetProduct(record.Product1).Name;
            record.Product2Name = ProductCodeLookup.GetProduct(record.Product2).Name;
            record.Product3Name = ProductCodeLookup.GetProduct(record.Product3).Name;
            record.DiagnosisName = DiagnosisCodeLookup.GetDiagnosis(record.Diagnosis).Name;
        }

        await db.InsertIncidentsAsync(data);
        log.Log("Data inserted.");
    }

    /// <summary>
    /// Post to Bsky Social
    /// </summary>
    /// <param name="username">-u, Username.</param>
    /// <param name="password">-p, Password.</param>
    /// <param name="verbose">-v, Verbose logging.</param>
    /// <param name="cancellationToken">Cancellation Token.</param>
    /// <returns>Task.</returns>
    [Command("post")]
    public async Task PostAsync(string username, string password, bool verbose = false, CancellationToken cancellationToken = default)
    {
        var db = new DatabaseService(AppCommands.db);
        var log = new ConsoleLog(verbose);
        var incidents = await db.GetMaleGenitalInjuriesAsync();
        log.Log($"Found {incidents.Count} incidents");
        if (incidents.Count == 0)
        {
            return;
        }

        var randomIncident = incidents[new Random().Next(incidents.Count)];
        var diag = randomIncident.DiagnosisName.Split('-').Last().Trim();
        var text = $"DIAG: {diag}{Environment.NewLine}{randomIncident.TreatmentDate.ToString("MMMM d, yyyy", CultureInfo.InvariantCulture).ToUpperInvariant()}{Environment.NewLine}{randomIncident.Narrative}";
        log.Log(text);
        var atProtocolBuilder = new ATProtocolBuilder();
        var atProtocol = atProtocolBuilder.Build();
        var (result, error) = await atProtocol.AuthenticateWithPasswordResultAsync(username, password, null, cancellationToken);
        if (error != null)
        {
            log.LogError($"Error: {error}");
            return;
        }

        var markdownPost = MarkdownPost.Parse(text);
        if (markdownPost is null)
        {
            log.LogError("Invalid post.");
            return;
        }

        var (postResult, postError) = await atProtocol.Feed.CreatePostAsync(markdownPost.Post, langs: new List<string>() { "en"},  cancellationToken: cancellationToken);
        if (postError != null)
        {
            log.LogError($"Error: {postError}");
            return;
        }

        log.Log($"Post created: {postResult?.Uri}");

        randomIncident.HasSeen = true;
        await db.SaveIncidentAsync(randomIncident);
    }
}