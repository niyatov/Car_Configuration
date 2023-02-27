namespace Car_Configuration.Models;

public class GetModelsVM
{
    public int Id { get; set; }
    public string Name { get; set; }
    public bool IsReady { get; set; }
    public int ColorsAmount { get; set; }
    public int WheelsAmount { get; set; }
    public int ColorWheelsAmount { get; set; }
}
