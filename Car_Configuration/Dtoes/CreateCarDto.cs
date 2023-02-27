namespace CarConfiguration_api.Dtoes;
public class CreateCarDto
{
    public int ModelId { get; set; }
    public string ModelName { get; set; }
    public string ColorName { get; set; }
    public string WheelName { get; set; }
    public Byte[] ByteArray { get; set; }
}
