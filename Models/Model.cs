using MongoDB.Bson;

public class Model
{
    public string Id { get; set; } // Identificador único
    public Dictionary<string, object?> properties { get; set; }
}