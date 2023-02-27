using Car_Configuration.Models;

namespace Car_Configuration.Dtoes;

public class ChooseColorAndWheelDto
{
    public List<GetColorModelVM>? Colors { get; set; }
    public List<GetWheelVM>? Wheels { get; set; }
}
