using MongoDB.Bson;

public class Model
{
    public ObjectId Id { get; set; } // Identificador único
    public Dictionary<string, object?> propierties { get; set; }
}