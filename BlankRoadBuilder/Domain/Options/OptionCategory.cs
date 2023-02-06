namespace BlankRoadBuilder.Domain.Options;

public class OptionCategory
{
	public const string MARKINGS = "Markings";
	public const string PROPS = "Props & Trees";
	public const string DESIGN = "Road Design";
	public const string OTHER = "Others";

	public static string GetIcon(string? category)
	{
		return category switch
		{
			MARKINGS => "I_Markings.png",
			PROPS => "I_Props.png",
			DESIGN => "I_RoadDesign.png",
			OTHER => "I_Other.png",
			_ => ""
		};
	}
}