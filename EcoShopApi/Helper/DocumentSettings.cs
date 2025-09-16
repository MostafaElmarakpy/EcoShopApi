namespace EcoShopApi.Helper
{
    public class DocumentSettings
    {
        public static string UploadFile(IFormFile file, string folderName)
        {
            string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images", folderName);
            string fileName = $"{Guid.NewGuid()} {file.FileName}";
            string filePath = Path.Combine(folderPath, fileName);

            using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);

            //using var sr = new StreamReader(fs);
            //string content = sr.ReadToEnd();
            file.CopyTo(fs);
            return fileName;
        }
        public static void DeleteFile(string fileName, string folderName)

        {
            string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\files", folderName, fileName);

            if (File.Exists(folderPath))
            {
                File.Delete(folderPath);
            }

        }

    }
}