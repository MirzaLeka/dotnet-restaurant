namespace DotNet8Starter.DL.DTO
{
	public class CreatePizzaResponseDTO
	{
		public int Id {  get; set; }
		public string Name {  get; set; }
		public int Slices { get; set; }
		public string SecretIngridient { get; set; }
		public DateTime DeliveryDate { get; set; }
	}
}
