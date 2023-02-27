using System.ComponentModel.DataAnnotations.Schema;

namespace Car_Configuration.Entities;

public class WheelColorModel
{
    public int Id { get; set; }
    public int ColorModelId { get; set; }
    [ForeignKey("ColorModelId")]
    public virtual ColorModel ColorModel { get; set; }
    public int WheelId { get; set; }
    [ForeignKey("WheelId")]
    public virtual Wheel Wheel { get; set; }
    public string? ColorWheelPath { get; set; }
    public virtual List<UserWheelColor>? UserWheelColors { get; set; }
}
