public class ServiceRecord
{
    public int Id { get; set; }
    public int VehicleId { get; set; }
    public Guid ReferenceId { get; set; }
    public DateTime Date { get; set; }
    public string Description { get; set; }
    public string WorkshopResponsible { get; set; }
}
