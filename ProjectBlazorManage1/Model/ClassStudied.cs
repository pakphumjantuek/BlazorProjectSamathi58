namespace ProjectBlazorManage1.Model
{
    public class ClassStudied
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [DisplayName("รุ่นที่เรียน")]
        [Range(1, int.MaxValue, ErrorMessage = "รหัสรุ่นต้องมากกว่า 0")]
        public int Id { get; set; }
        [DisplayName("วันที่เริ่มเรียน")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; } = DateTime.Now;
        [DisplayName("วันที่สิ้นสุดการเรียน")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; } = DateTime.Now;
        [DisplayName("สถานะ")]
        public bool Status { get; set; }
        [DisplayName("ชื่อรุ่น")]
        [Required(ErrorMessage = "กรุณาใส่ชื่อรุ่น")]
        public string ClassName { get; set; } = string.Empty; // ชื่อรุ่น
        [DisplayName("ภาคเรียน")]
        [Required(ErrorMessage = "กรุณาเลือกภาคเรียน")]
        public string termClass { get; set; } = string.Empty; // ภาคเรียน
        [Required(ErrorMessage = "กรุณาเลือกสาขา")]
        public int? Branch_Id { get; set; }
        [ValidateNever]
        [ForeignKey("Branch_Id")]
        public Branch Branch { get; set; }

        [NotMapped]
        public LearningPlan? learningPlan { get; set; }
        public List<LearningPlan> learningPlans { get; set; }

        public List<string> GetStudyMonths()
        {
            List<string> months = new List<string>();
            DateTime current = StartDate;
            while (current <= EndDate && months.Count < 6)
            {
                // แสดงชื่อเดือนและปีในรูปแบบ "เดือน YYYY" (ภาษาไทย)
                months.Add(current.ToString("MMMM", new System.Globalization.CultureInfo("th-TH")));
                current = current.AddMonths(1);
            }
            return months;
        }
    }
}
