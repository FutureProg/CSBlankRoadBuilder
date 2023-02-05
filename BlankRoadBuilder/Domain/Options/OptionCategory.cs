namespace BlankRoadBuilder.Domain.Options;

public class OptionCategory
{
	public const string MARKINGS = "Markings";


	public static string GetIcon(string category)
	{
		return category switch
		{
			MARKINGS => "I_Markings.png",
			_ => ""
		};
	}
}