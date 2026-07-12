using DocumentFormat.OpenXml.Packaging;
using System.Text;
using UglyToad.PdfPig;

namespace OnlineJobRecruitmentSystem.Infrastructure.Extensions
{
    public static class CvTextExtractor
    {
        public static string ExtractText(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLower();

            return extension switch
            {
                ".pdf" => ExtractFromPdf(filePath),
                ".docx" => ExtractFromDocx(filePath),
                _ => throw new NotSupportedException("Only PDF and DOCX files can be analyzed.")
            };
        }

        private static string ExtractFromPdf(string filePath)
        {
            using var document = PdfDocument.Open(filePath);
            var sb = new StringBuilder();
            foreach (var page in document.GetPages())
                sb.AppendLine(page.Text);
            return sb.ToString();
        }

        private static string ExtractFromDocx(string filePath)
        {
            using var doc = WordprocessingDocument.Open(filePath, false);
            var body = doc.MainDocumentPart?.Document?.Body;
            if (body == null)
                throw new InvalidOperationException("The DOCX file appears to be corrupted or empty.");
            return body.InnerText;
        }
    }
}
