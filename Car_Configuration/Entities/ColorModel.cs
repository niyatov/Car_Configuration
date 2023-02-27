using System.ComponentModel.DataAnnotations.Schema;

namespace Car_Configuration.Entities;

public class ColorModel
{
    public int Id { get; set; }
    public int ModelId { get; set; }
    [ForeignKey("ModelId")]
    public virtual Model Model { get; set; }
    public int ColorId { get; set; }
    [ForeignKey("ColorId")]
    public virtual Color Color { get; set; }
    public virtual List<WheelColorModel>? WheelColors { get; set; }
}
