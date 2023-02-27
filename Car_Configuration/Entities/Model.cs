namespace Car_Configuration.Entities;
public class Model : EntityBase
{
    public string? FolderPath { get; set; }
    public virtual List<ColorModel>? ColorModels { get; set; }
    public virtual List<Wheel>? Wheels { get; set; }
    public bool IsReady { get; set; }
}
