using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace RenameMyPhotos
{
    class PhotoInfo
    {
        private string filePath;
        public string FilePath
        {
            get
            {
                return filePath;
            }
            set
            {
                filePath = value;
                FileName = Path.GetFileNameWithoutExtension(value);
            }
        }
        public string FileName { get; private set; }

        public DateTime? DateTakenDT { get; private set; }
        public string DateTakenString { get; private set; }
        public string CameraManufacturer { get; private set; }
        public string CameraModel { get; private set; }

        public string GenerateFileName(int index = 0)
        {
            if(DateTakenString != "" && CameraManufacturer != "" && CameraModel != "")
            {
                string fileName = DateTakenString + " " + CameraManufacturer + " " + CameraModel + " #";
                if (index > 0) fileName += index; 
                return fileName; 
            }
            else
            {
                return "";
            }
        }

        public string GenerateFilePath(int index = 0)
        {
            string name = GenerateFileName(index);
            if (name != "")
            {
                return Path.GetDirectoryName(FilePath) + "\\" + name + Path.GetExtension(FilePath);
            }
            else
            {
                return "";
            }
        }

        public PhotoInfo(string path)
        {
            FilePath = path;

            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                BitmapSource img = BitmapFrame.Create(fs);
                BitmapMetadata md = (BitmapMetadata)img.Metadata;

                DateTakenDT = GetDateTaken(md);
                if (DateTakenDT != null)
                {
                    DateTakenString = GetDateTaken(md).Value.ToString("yyyy.MM.dd HH.mm.ss", System.Globalization.CultureInfo.InvariantCulture);
                }
                else
                {
                    DateTakenString = "";
                }

                CameraManufacturer = GetCameraManufacturer(md);
                CameraModel = GetCameraModel(md);
            }
        }

        private DateTime? GetDateTaken(BitmapMetadata md)
        {
            string dateTaken = md.DateTaken;
            if (dateTaken != null)
            {
                DateTime dt = DateTime.ParseExact(dateTaken, "dd.MM.yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                return dt;
            }
            return null;
        }

        private string GetCameraManufacturer(BitmapMetadata md)
        {
            string cMan = md.CameraManufacturer;
            if (cMan != null)
            {
                return cMan;
            }
            return "";
        }

        private string GetCameraModel(BitmapMetadata md)
        {
            string cModel = md.CameraModel;
            if (cModel != null)
            {
                return cModel;
            }
            return "";
        }
    }
}
