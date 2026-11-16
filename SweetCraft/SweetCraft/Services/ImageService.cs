using Microsoft.Win32;
using System;
using System.IO;

namespace SweetCraft.Services
{
    public sealed class ImageService
    {
        public string SelectAndCopyProductImage()
        {
            var dialog = new OpenFileDialog
            {
                Title = "Выберите изображение товара",
                Filter = "Изображения (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp",
                CheckFileExists = true,
                Multiselect = false
            };
            if (dialog.ShowDialog() != true) return null;

            string directory = GetProductImagesDirectory();
            Directory.CreateDirectory(directory);
            string extension = Path.GetExtension(dialog.FileName);
            string fileName = Guid.NewGuid().ToString("N") + extension;
            string destination = Path.Combine(directory, fileName);
            File.Copy(dialog.FileName, destination, false);
            return Path.Combine("ProductImages", fileName);
        }

        public static string GetAbsoluteProductImagePath(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath)) return null;
            string normalizedPath = relativePath.Replace('/', '\\');
            const string relativeDirectory = "ProductImages\\";
            if (!normalizedPath.StartsWith(relativeDirectory, StringComparison.OrdinalIgnoreCase)) return null;
            string fileName = normalizedPath.Substring(relativeDirectory.Length);
            if (string.IsNullOrWhiteSpace(fileName) || Path.IsPathRooted(fileName) || Path.GetFileName(fileName) != fileName) return null;
            return Path.Combine(GetProductImagesDirectory(), fileName);
        }

        private static string GetProductImagesDirectory()
        {
            var directory = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            while (directory != null)
            {
                if (File.Exists(Path.Combine(directory.FullName, "SweetCraft.csproj")))
                    return Path.Combine(directory.FullName, "ProductImages");
                directory = directory.Parent;
            }
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ProductImages");
        }
    }
}
