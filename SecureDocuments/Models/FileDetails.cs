#nullable enable

using SecureDocuments.Models.File;

namespace SecureDocuments.Models
{
    public record FileDetails(
        string Name,
        string RealExtension,
        Role Role,
        string Hash,
        long Size,
        CategoryName Category,
        string[] Tags)
    {
        public string NameExt => $"{Name}{RealExtension}";

        public string CategoryColor
        {
            get
            {
                return Category switch
                {
                    CategoryName.All => "Grey",
                    CategoryName.Inquiry => "Aqua",
                    CategoryName.Correspondence => "Bisque",
                    CategoryName.Specification => "Brown",
                    CategoryName.Documentation => "Chocolate",
                    CategoryName.Protocol => "DarkKhaki",
                    CategoryName.Schedule => "ForestGreen",
                    CategoryName.Contract => "Goldenrod",
                    CategoryName.Offer => "IndianRed",
                    CategoryName.Unknown => "LemonChiffon",
                    _ => "Grey"
                };
            }
        }

        public string ReadableSize
        {
            get
            {
                string[] sizes = { "B", "KB", "MB", "GB", "TB" };
                double len = Size;
                var order = 0;
                while (len >= 1024 && order < sizes.Length - 1)
                {
                    order++;
                    len /= 1024;
                }

                return $"{len:0.##} {sizes[order]}";
            }
        }

        public string TagsInLine => Tags != null ? string.Join(", ", Tags) : "";

        public string ExtIcon
        {
            get
            {
                return RealExtension.ToLower() switch
                {
                    ".mp3" => "FileMusic",
                    ".txt" => "FileDocument",
                    ".mkv" => "FileVideo",
                    ".flv" => "FileVideo",
                    ".mpeg" => "FileVideo",
                    ".mpg" => "FileVideo",
                    ".m4v" => "FileVideo",
                    ".mp4" => "FileVideo",
                    ".avi" => "FileVideo",
                    ".zip" => "FolderZip",
                    ".xps" => "FilePdf",
                    ".oxps" => "FilePdf",
                    ".pdf" => "FilePdf",
                    ".dim" => "FileCad",
                    ".las" => "FileCad",
                    ".lin" => "FileCad",
                    ".mln" => "FileCad",
                    ".nfl" => "FileCad",
                    ".pat" => "FileCad",
                    ".dxf" => "FileCad",
                    ".dwg" => "FileCad",
                    ".mpp" => "FileTable", // Microsoft Project 
                    ".sldm" => "FilePowerpoint", //Powerpoint
                    ".sldx" => "FilePowerpoint", //Powerpoint
                    ".ppsm" => "FilePowerpoint", //Powerpoint
                    ".ppsx" => "FilePowerpoint", //Powerpoint
                    ".ppam" => "FilePowerpoint", //Powerpoint
                    ".potm" => "FilePowerpoint", //Powerpoint
                    ".potx" => "FilePowerpoint", //Powerpoint
                    ".pptm" => "FilePowerpoint", //Powerpoint
                    ".pptx" => "FilePowerpoint", //Powerpoint
                    ".pps" => "FilePowerpoint", //Powerpoint
                    ".pot" => "FilePowerpoint", //Powerpoint
                    ".ppt" => "FilePowerpoint", //Powerpoint
                    ".xltm" => "FileExcel", //Excel
                    ".xltx" => "FileExcel", //Excel
                    ".xlsm" => "FileExcel", //Excel
                    ".xlsx" => "FileExcel", //Excel
                    ".xlm" => "FileExcel", //Excel
                    ".xlt" => "FileExcel", //Excel
                    ".xls" => "FileExcel", //Excel
                    ".docb" => "FileWord", //Word
                    ".dotm" => "FileWord", //Word
                    ".dotx" => "FileWord", //Word
                    ".docm" => "FileWord", //Word
                    ".odt" => "FileWord", //Word
                    ".rtf" => "FileWord", //Word
                    ".docx" => "FileWord", //Word
                    ".wbk" => "FileWord", //Word
                    ".dot" => "FileWord", //Word
                    ".doc" => "FileWord", //Word
                    ".pst" => "Email", //email
                    ".eml" => "Email", //email
                    ".bmp" => "FileImage", //images
                    ".tif" => "FileImage", //images
                    ".tiff" => "FileImage", //images
                    ".gif" => "FileImage", //images
                    ".png" => "FileImage", //images
                    ".jpeg" => "FileImage", //images
                    ".jpg" => "FileImage", //images
                    ".rw2" => "FileImage", //images
                    _ => "File"
                };
            }
        }
    }
}