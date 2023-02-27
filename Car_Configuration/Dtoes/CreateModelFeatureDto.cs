namespace CarConfiguration_api.Dtoes;

public class CreateModelFeatureDto
{
    public List<string>? ColorNames { get; set; }
    public List<string>? WheelNames { get; set; }
    public int ModelId { get; set; }
}
