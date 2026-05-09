namespace IMSWEB.Model.TOs
{
    public class TOCustomer
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string ContactNo { get; set; }
        public string Address { get; set; }
        public decimal TotalDue { get; set; }
        public string CompanyName { get; set; }
        public int CustomerType { get; set; }
    }
}
