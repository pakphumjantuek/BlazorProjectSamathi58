namespace ProjectBlazorManage1.Model
{
    public class JobPosition
    {
        [Key]
        public int Id { get; set; }
        [DisplayName("ตำเเหน่งงาน")]
        public string Position_Name { get; set; }
        [DisplayName("ระดับชั้น")]
        public int Position_Level { get; set; }
        [DisplayName("สถานะ")]
        public bool Status { get; set; }
    }
}
