public class LendDto
{
    public Guid BookItemId { get; set; }
    public DateOnly ReturnalDate { get; set; }
    public string Name { get; set; }
    public string SSN { get; set; }
}