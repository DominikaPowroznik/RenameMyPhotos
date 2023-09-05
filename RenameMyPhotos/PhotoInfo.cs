using System;
using System.IO;
using System.Text.RegularExpressions;
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
                string fileName = "IMG_" + DateTakenString + "_" + CameraManufacturer + "-" + CameraModel;
                if (index > 0) fileName += " #" + index;
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
                    DateTakenString = DateTakenDT.Value.ToString("yyyyMMdd_HHmmss", System.Globalization.CultureInfo.InvariantCulture);
                }
                else
                {
                    DateTakenDT = GuessDateTaken(FileName);
                    if (DateTakenDT != null)
                    {
                        DateTakenString = DateTakenDT.Value.ToString("yyyyMMdd_HHmmss", System.Globalization.CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        DateTakenString = "";
                    }
                }

                CameraManufacturer = GetCameraManufacturer(md).Replace(" ", "");
                CameraModel = GetCameraModel(md).Replace(" ", "");
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

        private DateTime? GuessDateTaken(string fileName)
        {
            if (!String.IsNullOrEmpty(fileName))
            {
                Regex dateTimeRegex = new Regex("[0-9]{4}[0-9]{1,2}[0-9]{1,2}");
                MatchCollection extractedDateTime = dateTimeRegex.Matches(fileName);
                string date = "", time = "", dateTime = "";
                if (extractedDateTime.Count > 0)
                    date = extractedDateTime[0].Value;
                if (extractedDateTime.Count > 1)
                    time = extractedDateTime[1].Value;
                if(date != "")
                {
                    dateTime = date;
                    if(time != "")
                        dateTime += time;  
                    else
                        dateTime += "000000";
                }
                try
                {
                    DateTime dt = DateTime.ParseExact(dateTime, "yyyyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture);
                    return dt;
                }
                catch (Exception e)
                {
                    return null;
                }
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
