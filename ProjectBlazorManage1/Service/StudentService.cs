
using iTextSharp.text.pdf;
using iTextSharp.text;
using Microsoft.AspNetCore.Hosting;

namespace ProjectBlazorManage1.Service
{
    public class StudentService : IStudenService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BranchService> _logger;
        private readonly IWebHostEnvironment webHostEnvironment;

        public StudentService(ApplicationDbContext context, ILogger<BranchService> logger, IWebHostEnvironment webHostEnvironment)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.webHostEnvironment = webHostEnvironment;
        }
        public async Task<List<Students>> GetStudents()
        {
            try
            {
                return await _context.students
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "เกิดข้อผิดพลาดขณะดึงข้อมูลทั้งหมด");
                throw; 
            }
        }
        public async Task<Students> GetStudentId(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("รหัสสาขาต้องเป็นค่าที่มากกว่า 0");
            }

            try
            {
                var student = await _context.students
                    .AsNoTracking()
                    .FirstOrDefaultAsync(b => b.Id == id);

                return student ?? throw new KeyNotFoundException($"ไม่พบข้อมูลด้วยรหัส {id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"เกิดข้อผิดพลาดขณะดึงข้อมูลสาขารหัส {id}");
                throw;
            }
        }
        public async Task AddStudent(Students student)
        {
            if (student == null)
            {
                throw new ArgumentNullException(nameof(student));
            }

            try
            {
                await _context.students.AddAsync(student);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "เกิดข้อผิดพลาดขณะเพิ่มข้อมูลใหม่");
                throw new InvalidOperationException("ไม่สามารถเพิ่มข้อมูลได้ อาจมีข้อมูลซ้ำ", ex);
            }
        }
        public async Task UpdateStudent(Students student)
        {
            var existingBranch = _context.students.FirstOrDefault(b => b.Id == student.Id);
            if (existingBranch != null)
            {
                existingBranch.Id = student.Id;
                existingBranch.FirstName = student.FirstName;
                existingBranch.LastName = student.LastName;
                existingBranch.PersonalId = student.PersonalId;
                existingBranch.Initial_Id = student.Initial_Id;
                existingBranch.Nickname = student.Nickname;
                existingBranch.Phone_Number = student.Phone_Number;
                existingBranch.EnglishName = student.EnglishName;
                existingBranch.BirthDate = student.BirthDate;
                existingBranch.degreeType = student.degreeType;
                existingBranch.relationshipType = student.relationshipType;
                existingBranch.Institution = student.Institution;
                existingBranch.CitizenCardAddress = student.CitizenCardAddress;
                existingBranch.ContactAddress = student.ContactAddress;
                existingBranch.Occupation = student.Occupation;
                existingBranch.Workplace = student.Workplace;
                existingBranch.ClosePersonPhone = student.ClosePersonPhone;
                existingBranch.MedicalCondition = student.MedicalCondition;
                existingBranch.Email = student.Email;
                existingBranch.LineId = student.LineId;
                existingBranch.ApplicationDate = student.ApplicationDate;
                existingBranch.Remarks = student.Remarks;
                existingBranch.adviser = student.adviser;
                existingBranch.Images = student.Images;
                existingBranch.ClassStudie_Id = student.ClassStudie_Id;
                existingBranch.ClosePersonName = student.ClosePersonName;
                existingBranch.credit = student.credit;


                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteStudent(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("รหัสสาขาต้องเป็นค่าที่มากกว่า 0");
            }

            try
            {
                var student = await _context.students.FindAsync(id);
                if (student == null)
                {
                    throw new KeyNotFoundException($"ไม่พบสาขาด้วยรหัส {id}");
                }

                _context.students.Remove(student);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, $"เกิดข้อผิดพลาดขณะลบหัส {id}");
                throw new InvalidOperationException("ไม่สามารถลบได้ ข้อมูลอาจถูกใช้งานอยู่", ex);
            }
        }


        public byte[] GenerateStudentCards(List<int> selectedIds)
        {
            if (selectedIds == null || selectedIds.Count == 0)
                throw new ArgumentException("กรุณาเลือกนักเรียนอย่างน้อย 1 คน");

            var students = _context.students
                .Include(p => p.classStudied)
                .Include(p => p.Initial)
                .Where(s => selectedIds.Contains(s.Id))
                .ToList();

            if (students.Count == 0)
                throw new ArgumentException("ไม่พบข้อมูลนักเรียนที่เลือก");

            using (MemoryStream ms = new MemoryStream())
            {
                Document document = new Document(PageSize.A4, 50, 50, 30, 0);
                PdfWriter writer = PdfWriter.GetInstance(document, ms);
                document.Open();

                document.AddTitle("บัตรนักศึกษา");
                document.AddCreationDate();

                string fontPath = Path.Combine(webHostEnvironment.WebRootPath, "fonts", "THSarabunNew.ttf");
                BaseFont thaiFont = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                Font titleFont = new Font(thaiFont, 20, Font.BOLD);
                Font subTitleFont = new Font(thaiFont, 16, Font.BOLD);
                Font normalFont = new Font(thaiFont, 14, Font.NORMAL);
                Font largeFont = new Font(thaiFont, 18, Font.BOLD);
                Font smallFont = new Font(thaiFont, 12, Font.NORMAL);

                string fontPath1 = Path.Combine(webHostEnvironment.WebRootPath, "fonts", "THSarabunIT∙.ttf");
                BaseFont thaiFont1 = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                Font titleFont1 = new Font(thaiFont, 20, Font.BOLD);
                Font subTitleFont1 = new Font(thaiFont, 16, Font.BOLD);
                Font normalFont1 = new Font(thaiFont, 14, Font.NORMAL);
                Font largeFont1 = new Font(thaiFont, 18, Font.BOLD);
                Font smallFont1 = new Font(thaiFont, 12, Font.NORMAL);

                string bgPath = Path.Combine(webHostEnvironment.WebRootPath, "images", "BG_TEST.png");
                Image bgImage = null;
                if (System.IO.File.Exists(bgPath))
                {
                    bgImage = Image.GetInstance(bgPath);
                }

                PdfPTable table = new PdfPTable(2);
                table.WidthPercentage = 100;
                table.SetWidths(new float[] { 50f, 50f });
                table.SpacingBefore = 0f;
                table.SpacingAfter = 0f;
                table.DefaultCell.Border = Rectangle.NO_BORDER;
                table.DefaultCell.Padding = 0f;
                table.DefaultCell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.DefaultCell.VerticalAlignment = Element.ALIGN_MIDDLE;

                int count = 0;
                foreach (var student in students)
                {
                    float cardWidth = 240;
                    float cardHeight = 144;

                    PdfPCell cardCell = new PdfPCell();
                    cardCell.FixedHeight = cardHeight;
                    cardCell.MinimumHeight = cardHeight;
                    cardCell.Border = Rectangle.NO_BORDER;
                    cardCell.Padding = 0f;
                    cardCell.HorizontalAlignment = Element.ALIGN_CENTER;
                    cardCell.VerticalAlignment = Element.ALIGN_MIDDLE;

                    PdfContentByte cb = writer.DirectContent;
                    PdfTemplate template = cb.CreateTemplate(cardWidth, cardHeight);

                    if (bgImage != null)
                    {
                        bgImage.ScaleAbsolute(cardWidth, cardHeight);
                        bgImage.SetAbsolutePosition(0, 0);
                        template.AddImage(bgImage);
                    }

                    Phrase cardName = new Phrase(student.Initial.Initial_Name + student.FirstName + " " + student.LastName, largeFont1);
                    ColumnText cardNames = new ColumnText(template);
                    cardNames.SetSimpleColumn(cardName, 50, cardHeight - 70, cardWidth, cardHeight - 100, 15, Element.ALIGN_CENTER);
                    cardNames.Go();

                    float photoWidth = 60;  // Reduced size
                    float photoHeight = 60; // Reduced size
                    Rectangle photoRect = new Rectangle(10, cardHeight - 110, 10 + photoWidth, cardHeight - 50);
                    template.Rectangle(photoRect.Left, photoRect.Bottom, photoRect.Width, photoRect.Height);
                    template.Stroke();

                    // Add student image
                    if (!string.IsNullOrEmpty(student.Images))
                    {
                        try
                        {
                            var imagePaths = student.Images.Split(',');
                            string imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", imagePaths[0]);

                            if (System.IO.File.Exists(imagePath))
                            {
                                Image studentImage = Image.GetInstance(imagePath);

                                // Scale image to fit the smaller photo area exactly
                                studentImage.ScaleAbsolute(photoWidth, photoHeight); // Force exact size (60x60)

                                // Position at the left edge of the rectangle
                                float x = photoRect.Left; // Align to left (no centering)
                                float y = photoRect.Bottom + (photoRect.Height - studentImage.ScaledHeight) / 2; // Center vertically in rectangle

                                studentImage.SetAbsolutePosition(x, y);
                                template.AddImage(studentImage);
                            }
                            else
                            {
                                Phrase photoText = new Phrase("ไม่มีรูป", normalFont);
                                ColumnText ctPhoto = new ColumnText(template);
                                ctPhoto.SetSimpleColumn(photoText, photoRect.Left, photoRect.Bottom, photoRect.Right, photoRect.Top, 15, Element.ALIGN_CENTER);
                                ctPhoto.Go();
                            }
                        }
                        catch (Exception)
                        {
                            Phrase photoText = new Phrase("รูป\nนักศึกษา", normalFont);
                            ColumnText ctPhoto = new ColumnText(template);
                            ctPhoto.SetSimpleColumn(photoText, photoRect.Left, photoRect.Bottom, photoRect.Right, photoRect.Top, 15, Element.ALIGN_CENTER);
                            ctPhoto.Go();
                        }
                    }
                    Image cardImage = Image.GetInstance(template);
                    cardCell.Image = cardImage;
                    table.AddCell(cardCell);

                    Phrase cardId = new Phrase(student.PersonalId, normalFont);
                    ColumnText cardIds = new ColumnText(template);
                    cardIds.SetSimpleColumn(cardId, 10, -5, 70, 35, 15, Element.ALIGN_CENTER); // ปรับ y จาก (10,50) → (5,45)
                    cardIds.Go();

                    Phrase cardClassId = new Phrase(ConvertToThaiNumbers(student.ClassStudie_Id.ToString()), normalFont1);
                    ColumnText cardClassIds = new ColumnText(template);
                    cardClassIds.SetSimpleColumn(cardClassId, 183, 12, 243, 57, 15, Element.ALIGN_CENTER);
                    cardClassIds.Go();

                    count++;
                    if (count % 6 == 0) // 6 cards per page
                    {
                        document.Add(table);
                        document.NewPage();
                        table.DeleteBodyRows();
                    }

                }
                if (count > 0)
                {

                    if (count % 2 != 0)
                    {
                        PdfPCell emptyCell = new PdfPCell();
                        emptyCell.FixedHeight = 144;
                        emptyCell.Border = Rectangle.NO_BORDER;
                        table.AddCell(emptyCell);
                    }
                    document.Add(table);
                }

                document.Close();
                writer.Close();

                return ms.ToArray();
            }
        }

        public static string ConvertToThaiNumbers(string input)
        {
            return input.Replace('0', '๐')
                        .Replace('1', '๑')
                        .Replace('2', '๒')
                        .Replace('3', '๓')
                        .Replace('4', '๔')
                        .Replace('5', '๕')
                        .Replace('6', '๖')
                        .Replace('7', '๗')
                        .Replace('8', '๘')
                        .Replace('9', '๙');
        }


    }
}
