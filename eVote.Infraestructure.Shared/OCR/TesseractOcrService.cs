using eVote360.Core.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Tesseract;
using System.Text;


namespace eVote360.Infraestructure.Shared.OCR
{
    using Tesseract;

    public class TesseractOcrService : IOcrService
    {
        private readonly string _tessDataPath;

        public TesseractOcrService()
        {
            _tessDataPath = Path.Combine(AppContext.BaseDirectory, "Tessdata");
        }

        public async Task<string?> ExtractTextAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return null;

            var tempFilePath = Path.GetTempFileName();

            using (var stream = new FileStream(tempFilePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            try
            {
                using var engine = new TesseractEngine(_tessDataPath, "spa", EngineMode.Default);

                using var img = Pix.LoadFromFile(tempFilePath);

                using var page = engine.Process(img);

                string text = page.GetText();

                return text;
            }
            catch
            {
                return null;
            }
            finally
            {
                if (File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);
                }
            }
        }
    }
}
