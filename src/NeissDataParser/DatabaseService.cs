using SQLite;

namespace NeissDataParser;

public class DatabaseService
{
    private readonly SQLiteAsyncConnection _connection;

    public DatabaseService(string dbPath)
    {
        _connection = new SQLiteAsyncConnection(dbPath);
        _connection.CreateTableAsync<IncidentRecord>().Wait();
    }

    public async Task<List<IncidentRecord>> GetIncidentsAsync()
    {
        return await _connection.Table<IncidentRecord>().ToListAsync();
    }

    public async Task<int> SaveIncidentAsync(IncidentRecord incident)
    {
        return await _connection.UpdateAsync(incident, typeof(IncidentRecord));
    }

    public async Task<int> InsertIncidentsAsync(List<IncidentRecord> incidents)
    {
        return await _connection.InsertAllAsync(incidents, typeof(IncidentRecord));
    }

    public async Task<int> DeleteIncidentAsync(IncidentRecord incident)
    {
        return await _connection.Table<IncidentRecord>().DeleteAsync(i => i.CaseNumber == incident.CaseNumber);
    }

    public async Task<int> CountIncidentsAsync()
    {
        return await _connection.Table<IncidentRecord>().CountAsync();
    }

    public async Task<List<IncidentRecord>> GetMaleGenitalInjuriesAsync()
    {
        return await _connection.Table<IncidentRecord>().Where(r => r.Age > 12 && r.Age <= 100 && !r.Blocked && !r.HasSeen && r.Gender == Gender.Male && (r.BodyPart == 38 || r.BodyPart2 == 38)).ToListAsync();
    }

    public async Task<int> ResetSeenAsync()
    {
        return await _connection.ExecuteAsync("UPDATE IncidentRecord SET HasSeen = 0");
    }
}