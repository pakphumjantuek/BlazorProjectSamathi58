namespace ProjectBlazorManage1.Model
{
    public class Initial
    {
        [Key]
        public int Initial_Id { get; set; }
        [DisplayName("คํานําหน้าชื่อภาษาไทย")]
        public string Initial_Name { get; set; }    
    }
}
