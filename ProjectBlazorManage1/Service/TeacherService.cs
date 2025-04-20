
using iText.IO.Font;
using iText.IO.Image;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.EntityFrameworkCore;

namespace ProjectBlazorManage1.Service
{
    public class TeacherService : ITeacherService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TeacherService> _logger;
        private readonly IWebHostEnvironment _environment;

        public TeacherService(
            ApplicationDbContext context,
            ILogger<TeacherService> logger,
            IWebHostEnvironment environment)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        }

        public async Task<List<CourseName>> GetAllCourseNames()
        {
            return await _context.CourseNames.ToListAsync();
        }

        public async Task<List<Teacher>> GetTeachersByCourseId(int courseId)
        {
            return await _context.DhammaCourses
                .Include(dc => dc.Teacher)
                .Where(dc => dc.CourseName_Id == courseId)
                .Select(dc => dc.Teacher)
                .Distinct()
                .ToListAsync();
        }

    
        public async Task<List<Teacher>> GetAllTeachers()
        {
            return await _context.Teachers
                 .Include(t => t.Initial)
                    .Include(t => t.Branch)
                    .Include(t => t.jobPosition)
                    .Include(t => t.EducationHistories)
                    .Include(t => t.DhamaHistories)
                    .ThenInclude(h => h.CourseName)
                .AsNoTracking().ToListAsync();
        }

        public async Task<List<CourseName>> GetAllCourses()
        {
            return await _context.CourseNames
                .Where(c => c.Status == true) // แสดงเฉพาะหลักสูตรที่เปิดใช้งาน
                .ToListAsync();
        }
 
        public async Task<Teacher> GetTeacherById(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("รหัสอาจารย์ต้องเป็นค่าที่มากกว่า 0");
            }

            try
            {
                var branch = await _context.Teachers
                    .AsNoTracking()
                    .Include(t => t.Initial)
                    .Include(t => t.Branch)
                    .Include(t => t.jobPosition)
                    .Include(t => t.EducationHistories)
                    .Include(t => t.DhamaHistories)
                    .ThenInclude(t => t.CourseName)
                    .FirstOrDefaultAsync(b => b.Teacher_Id == id);

                return branch ?? throw new KeyNotFoundException($"ไม่พบอาจารย์ด้วยรหัส {id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"เกิดข้อผิดพลาดขณะดึงข้อมูลอาจารย์รหัส {id}");
                throw;
            }
        }
        public async Task AddTeacher(Teacher teacher)
        {
            if (teacher == null)
            {
                throw new ArgumentNullException(nameof(teacher));
            }

            try
            {
                await _context.Teachers.AddAsync(teacher);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "เกิดข้อผิดพลาดขณะเพิ่มอาจารย์ใหม่");
                throw new InvalidOperationException("ไม่สามารถเพิ่มอาจารย์ได้ อาจมีข้อมูลซ้ำ", ex);
            }
        }
        public async Task UpdateTeacher(Teacher teacher)
        {
            var existingTeacher = _context.Teachers.FirstOrDefault(b => b.Teacher_Id == teacher.Teacher_Id);
            if (existingTeacher != null)
            {
                existingTeacher.Teacher_Name = teacher.Teacher_Name;
                existingTeacher.Teacher_LastName = teacher.Teacher_LastName;
                existingTeacher.Phone_Number = teacher.Phone_Number;
                existingTeacher.Address = teacher.Address;
                existingTeacher.Images = teacher.Images;
                existingTeacher.Occupation = teacher.Occupation;
                existingTeacher.Initial_Id = teacher.Initial_Id;
                existingTeacher.Branch_Id = teacher.Branch_Id;
                existingTeacher.JobPosition_Id = teacher.JobPosition_Id;
                existingTeacher.Position_detail = teacher.Position_detail;
                existingTeacher.Special_Skill = teacher.Special_Skill;

                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteTeacher(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("รหัสอาจารย์ต้องเป็นค่าที่มากกว่า 0");
            }

            try
            {
                var teacher = await _context.Teachers.FindAsync(id);
                if (teacher == null)
                {
                    throw new KeyNotFoundException($"ไม่พบอาจารย์ด้วยรหัส {id}");
                }

                _context.Teachers.Remove(teacher);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, $"เกิดข้อผิดพลาดขณะลบอาจารย์รหัส {id}");
                throw new InvalidOperationException("ไม่สามารถลบอาจารย์ได้ ข้อมูลอาจถูกใช้งานอยู่", ex);
            }
        }
      
        public async Task<byte[]> ExportToPdfSecurity(int id)
        {
            var teacher = await GetTeacherById(id);

            if (teacher == null)
            {
                return null;
            }

            try
            {
                using (var ms = new MemoryStream())
                {
                    var writer = new PdfWriter(ms);
                    var pdf = new PdfDocument(writer);
                    var document = new Document(pdf, iText.Kernel.Geom.PageSize.A4);
                    document.SetMargins(25, 25, 25, 25);

                    // Load fonts
                    string fontPath = Path.Combine(_environment.WebRootPath, "fonts", "cordia_new.ttf");
                    string fontBoldPath = Path.Combine(_environment.WebRootPath, "fonts", "cordia_newBold.ttf");

                    PdfFont font = PdfFontFactory.CreateFont(fontPath, PdfEncodings.IDENTITY_H);
                    PdfFont fontBold = PdfFontFactory.CreateFont(fontBoldPath, PdfEncodings.IDENTITY_H);

                    Paragraph title = new Paragraph("ประวัติอาจารย์ผู้บรรยาย")
                        .SetFont(fontBold)
                        .SetFontSize(26)
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetMarginBottom(10);
                    document.Add(title);

                    Table table = new Table(2).UseAllAvailableWidth();
                    Cell textCell = new Cell().SetBorder(Border.NO_BORDER);

                    textCell.Add(new Paragraph($"ชื่อ-นามสกุล: {teacher.Teacher_Name} {teacher.Teacher_LastName}")
                        .SetFont(font)
                        .SetFontSize(24)
                        .SetMarginBottom(2));

                    textCell.Add(new Paragraph($"สังกัดสาขา: {teacher.Branch_Id + " " + teacher.Branch?.Branch_Name ?? "-"}")
                        .SetFont(font)
                        .SetFontSize(24)
                        .SetMarginBottom(0));

                    table.AddCell(textCell);

                    Cell imageCell = new Cell().SetBorder(Border.NO_BORDER)
                        .SetHorizontalAlignment(HorizontalAlignment.RIGHT);

                    if (!string.IsNullOrEmpty(teacher.Images))
                    {
                        var imagePaths = teacher.Images.Split(',');
                        string fullImagePath = Path.Combine(_environment.WebRootPath, imagePaths[0].TrimStart('/'));
                        if (File.Exists(fullImagePath))
                        {
                            ImageData imageData = ImageDataFactory.Create(fullImagePath);
                            Image image = new Image(imageData)
                                .SetWidth(120)
                                .SetHeight(120)
                                .SetHorizontalAlignment(HorizontalAlignment.RIGHT)
                                .SetBorderRadius(new BorderRadius(10));
                            imageCell.Add(image);
                        }
                    }

                    table.AddCell(imageCell);
                    document.Add(table);

                    // Dhama Histories
                    if (teacher.DhamaHistories != null && teacher.DhamaHistories.Any())
                    {
                        Paragraph courseParagraph = new Paragraph()
                            .SetFont(font)
                            .SetFontSize(24)
                            .SetTextAlignment(TextAlignment.LEFT)
                        .SetMarginTop(0)
                            .SetMarginBottom(0);

                        courseParagraph.AddTabStops(new TabStop(97f, TabAlignment.LEFT));
                        courseParagraph.Add(new Text("จบหลักสูตร  ").SetFont(fontBold));

                        bool first = true;
                        foreach (var course in teacher.DhamaHistories.OrderBy(p => p.CourseName.Level))
                        {
                            if (!first)
                            {
                                courseParagraph.Add("\n");
                                courseParagraph.Add(new Tab());
                                courseParagraph.SetMultipliedLeading(0.9f);
                            }
                            courseParagraph.Add(new Text($": {course.CourseName?.Name ?? ""} รุ่นที่: {course.generation.ToString() ?? ""}"));
                            first = false;
                        }
                        document.Add(courseParagraph);
                    }
                    else
                    {
                        document.Add(new Paragraph("ไม่มีข้อมูลประวัติทางธรรม")
                            .SetFont(font)
                            .SetFontSize(24)
                            .SetTextAlignment(TextAlignment.LEFT)
                            .SetMarginTop(0)
                            .SetMarginBottom(0));
                    }

                    // Education Histories
                    if (teacher.EducationHistories != null && teacher.EducationHistories.Any())
                    {
                        Paragraph eduHeader = new Paragraph("จบการศึกษา : ")
                            .SetFont(fontBold)
                            .SetFontSize(24)
                            .SetTextAlignment(TextAlignment.LEFT)
                        .SetMarginBottom(2);

                        foreach (var edu in teacher.EducationHistories.OrderByDescending(p => p.Degree).Take(1))
                        {
                            eduHeader.Add(new Text($"{edu.Degree.ToString() ?? ""}").SetFont(font).SetFontSize(24));
                        }
                        document.Add(eduHeader);
                    }
                    else
                    {
                        document.Add(new Paragraph("ไม่มีข้อมูลประวัติการศึกษา")
                            .SetFont(font)
                            .SetFontSize(24)
                            .SetTextAlignment(TextAlignment.LEFT)
                            .SetMarginBottom(2));
                    }

                    document.Add(new Paragraph($"อาชีพปัจจุบัน: {teacher.Occupation ?? "-"}")
                        .SetFont(font)
                        .SetFontSize(24)
                        .SetMarginBottom(2));

                    document.Add(new Paragraph($"หน้าที่ในสถาบัน: {teacher.jobPosition?.Position_Name ?? "-"}")
                        .SetFont(font)
                        .SetFontSize(24)
                        .SetMarginBottom(2));

                    document.Add(new Paragraph($"ความสารถพิเศษ: {teacher.Special_Skill ?? "-"}")
                        .SetFont(font)
                        .SetFontSize(24)
                        .SetMarginBottom(2));

                    document.Close();
                    return ms.ToArray();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error generating PDF for teacher {id}");
                throw;
            }
        }

    }
}



