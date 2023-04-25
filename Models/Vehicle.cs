using MongoDB.Bson.Serialization.Attributes;

public class Vehicle
{
      [BsonId]
    public Guid Id { get; set; }

    public string Brand { get; set; }
    public string Model { get; set; }
    public string RegistrationNumber { get; set; }
    public int Mileage { get; set; }
    public List<ServiceRecord> ServiceHistory { get; set; }
    public List<ImageRecord> ImageHistory { get; set; }
}