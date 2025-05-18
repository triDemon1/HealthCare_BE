namespace HaNoiTravel.DTOS
{
    public class ServiceUpdateDto
    {
        public int Serviceid { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int? Duration { get; set; }
        public decimal? Price { get; set; }
        public bool Isactive { get; set; }
        public int Servicegroupid { get; set; }
        public int Subjecttypeid { get; set; }
    }
}
