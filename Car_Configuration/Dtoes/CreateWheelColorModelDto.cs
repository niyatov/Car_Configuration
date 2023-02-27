using Car_Configuration.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace Car_Configuration.Dtoes;

public class CreateWheelColorModelDto
{
    public string? ModelName { get; set; }
    public string? ColorModelName { get; set; }
    public string? WheelName { get; set; }
}
