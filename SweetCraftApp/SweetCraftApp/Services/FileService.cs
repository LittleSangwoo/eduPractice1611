using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetCraftApp.Services
{
    public class FileService
    {
        public string SelectAndCopyImage()
        {
            var dialog = new OpenFileDialog { Filter = "Images (*.png;*.jpg)|*.png;*.jpg" };
            if (dialog.ShowDialog() == true)
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string imagesDir = Path.Combine(baseDir, "ProductImages");
                if (!Directory.Exists(imagesDir)) Directory.CreateDirectory(imagesDir);

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(dialog.FileName);
                string destPath = Path.Combine(imagesDir, fileName);

                File.Copy(dialog.FileName, destPath, true);
                return $"ProductImages/{fileName}"; // Относительный путь для БД[cite: 1]
            }
            return null;
        }
    }
}
