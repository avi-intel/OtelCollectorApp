using LiteDB;
using System.Text.Json;

namespace OtelCollectorApp.Services
{
    public class TelemetryDataService
    {
        private readonly string _dbPath = "TelemetryData.db";

        public void StoreTelemetryData(string type, JsonElement data)
        {
            using var db = new LiteDatabase(_dbPath);
            var collection = db.GetCollection<TelemetryRecord>(type);

            var record = new TelemetryRecord
            {
                Timestamp = DateTime.UtcNow,
                Data = data.ToString()
            };

            collection.Insert(record);
        }

        public List<TelemetryRecord> GetTelemetryData(string type, DateTime? from = null, DateTime? to = null)
        {
            using var db = new LiteDatabase(_dbPath);
            var collection = db.GetCollection<TelemetryRecord>(type);

            var query = collection.Query();

            if (from.HasValue)
                query = query.Where(x => x.Timestamp >= from.Value);
            if (to.HasValue)
                query = query.Where(x => x.Timestamp <= to.Value);

            return query.ToList();
        }
    }

    public class TelemetryRecord
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string Data { get; set; }
    }
}