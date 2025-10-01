namespace DotNet8Starter.DL.Models
{
	public class MakePizza
	{
		public string Name { get; set; }
		public List<string> Ingredients { get; set; }

		public static MakePizza ToMakePizza(string name, string ingrediant)
		{
			return new MakePizza
			{
				Name = name,
				Ingredients = [ingrediant]
			};
		}
	}
}
