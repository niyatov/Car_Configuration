using System.ComponentModel.DataAnnotations.Schema;

namespace Car_Configuration.Entities;

public class UserWheelColor
{
    public int UserId { get; set; }
    [ForeignKey("UserId")]
    public virtual User User { get; set; }
    public int WheelColorModelId { get; set; }
    [ForeignKey("WheelColorModelId")]
    public virtual WheelColorModel WheelColorModel { get; set; }
}
