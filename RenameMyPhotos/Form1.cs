using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;
using System.Threading;
using System.Windows;

namespace RenameMyPhotos
{
    public partial class Form1 : Form
    {
        private PhotoInfo selectedPhoto;

        private string currentDirectory = "";

        public Form1()
        {
            InitializeComponent();
        }

        private void loadDirectory_Click(object sender, EventArgs e)
        {
            ChooseDirectory();
        }

        private void ChooseDirectory()
        {
            string path = "";
            folderBrowserDialog1.Description = "Choose folder with your photos.";
            folderBrowserDialog1.SelectedPath = @"F:\Photos & RecordedMovies";
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                path = folderBrowserDialog1.SelectedPath;
            }
            textBox1.Text = path;
        }

        private void loadFilenames_Click(object sender, EventArgs e)
        {
            LoadFilenames(textBox1.Text);
        }

        private void LoadFilenames(string path)
        {
            listBox1.Items.Clear();
            ClearView();
            if (Directory.Exists(path))
            {
                currentDirectory = path;
                
                DirectoryInfo directoryInfo = new DirectoryInfo(textBox1.Text);
                List<FileInfo> fileInfos;
                if (recursiveCheckBox.Checked)
                {
                    fileInfos = directoryInfo.GetFiles("*.*", SearchOption.AllDirectories)
                        .OrderBy(f => f.LastWriteTime.Year <= 1601 ? f.CreationTime : f.LastWriteTime)
                        .Where(s => s.Name.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) || s.Name.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }
                else
                {
                    fileInfos = directoryInfo.GetFiles("*.*", SearchOption.TopDirectoryOnly)
                        .OrderBy(f => f.LastWriteTime.Year <= 1601 ? f.CreationTime : f.LastWriteTime)
                        .Where(s => s.Name.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) || s.Name.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }

                if (fileInfos.Any())
                {
                    foreach(FileInfo f in fileInfos)
                    {
                        listBox1.Items.Add(f.FullName);
                    }
                    renameAllButton.Enabled = true;
                }
                else
                {
                    listBox1.Items.Add("No photos in " + textBox1.Text);
                    renameAllButton.Enabled = false;
                }
            }
            else
            {
                listBox1.Items.Add("No such directory!");
                renameAllButton.Enabled = false;
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ClearView();
            if (listBox1.SelectedItem != null)
            {
                selectedPhoto = new PhotoInfo(listBox1.SelectedItem.ToString());
                SetView();
            }
        }

        private void renameAllButton_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder("Output log:\n");
            int success = 0, fail = 0;

            ClearView();
            List<string> photoPaths = new List<string>();
            foreach (var item in listBox1.Items)
            {
                photoPaths.Add(item.ToString());
            }
            foreach (var item in photoPaths)
            {
                if (RenameFile(item, sb) == true) success++;
                else fail++;
            }

            if (success > 0) sb.AppendLine("SUCCESSFULLY RENAMED " + success + " FILES.");
            if (fail > 0) sb.AppendLine("COULD NOT RENAME " + fail + " FILES.");

            Console.Write(sb);

            LoadFilenames(currentDirectory);
        }

        private void renameSelectedButton_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder("Output log:\n");

            ClearView();
            if (RenameFile(listBox1.SelectedItem.ToString(), sb) == true)
                sb.AppendLine("SUCCESSFULLY RENAMED FILE.");
            else
                sb.AppendLine("COULD NOT RENAME FILE.");

            Console.Write(sb);

            LoadFilenames(currentDirectory);
        }

        private bool RenameFile(string path, StringBuilder outputString)
        {
            listBox1.ClearSelected();

            PhotoInfo photo = new PhotoInfo(path);

            string newFilePath = photo.GenerateFilePath();
            if (newFilePath != "")
            {
                int index = 0;
                while (File.Exists(newFilePath))
                {
                    newFilePath = photo.GenerateFilePath(++index);
                }
                File.Move(path, newFilePath);
                outputString.AppendLine("Generated new name and renamed file:\n" + path);
                return true;
            }
            else
            {
                outputString.AppendLine("Not enough metadata to generate new name for file:\n" + path);
                return false;
            }
        }

        private void SetView()
        {
            LoadTempPhotoImage(selectedPhoto.FilePath);

            photoNameLabel.Text = selectedPhoto.FileName;
            if(selectedPhoto.DateTakenDT != null)
            {
                dateTimePicker.CustomFormat = "dd.MM.yyyy HH:mm:ss";
                dateTimePicker.Value = selectedPhoto.DateTakenDT.Value;
            }
            else
            {
                dateTimePicker.CustomFormat = " ";
            }

            cManText.Text = selectedPhoto.CameraManufacturer;
            cModelText.Text = selectedPhoto.CameraModel;

            string photoName = selectedPhoto.GenerateFileName();
            if (photoName != "")
            {
                newPhotoNameLabel.Text = photoName;
                if(selectedPhoto.FileName == photoName)
                {
                    newPhotoNameLabel.ForeColor = Color.LimeGreen;
                }
                else
                {
                    newPhotoNameLabel.ForeColor = Color.Silver;
                }
            }
            else
            {
                newPhotoNameLabel.ForeColor = Color.Red;
                newPhotoNameLabel.Text = "Can't generate foto name!";
            }

            renameSelectedButton.Enabled = true;
            dateTimePicker.Enabled = true;
            cManText.Enabled = true;
            cModelText.Enabled = true;
        }

        private void LoadTempPhotoImage(string path)
        {
            string tempPhotoPath = path.Insert(path.LastIndexOf('.'), "_temp");
            File.Copy(path, tempPhotoPath);

            using (FileStream fs = new FileStream(tempPhotoPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.DeleteOnClose)) //read
            {
                pictureBox1.Image = Image.FromStream(fs);
            }
        }

        private void ClearView()
        {
            photoNameLabel.Text = "";
            if (pictureBox1.Image != null)
            {
                pictureBox1.Image.Dispose();
                pictureBox1.Image = null;
            }
            dateTimePicker.Value = DateTime.Now;
            cManText.Text = "";
            cModelText.Text = "";
            dateTimePicker.Enabled = false;
            cManText.Enabled = false;
            cModelText.Enabled = false;
            newPhotoNameLabel.Text = "";
            changeMetadataButton.Enabled = false;
            renameSelectedButton.Enabled = false;
        }

        public void ChangeDateTaken(string path)
        {
            Image image = new Bitmap(path);
            PropertyItem[] propItems = image.PropertyItems;
            ASCIIEncoding encoding = new ASCIIEncoding();

            List<PropertyItem> dateProps = new List<PropertyItem>();

            PropertyItem dateTaken1 = propItems.Where(a => a.Id.ToString("x") == "132").FirstOrDefault();
            if (dateTaken1 != null) dateProps.Add(dateTaken1);
            PropertyItem dateTaken2 = propItems.Where(a => a.Id.ToString("x") == "9004").FirstOrDefault();
            if (dateTaken2 != null) dateProps.Add(dateTaken2);
            PropertyItem dateTaken3 = propItems.Where(a => a.Id.ToString("x") == "9003").FirstOrDefault();
            if (dateTaken3 != null) dateProps.Add(dateTaken3);

            DateTime newDate = DateTime.ParseExact("1994:05:10 10:10:20", "yyyy:MM:dd HH:mm:ss", null);

            foreach(PropertyItem prop in dateProps)
            {
                prop.Value = encoding.GetBytes(newDate.ToString("yyyy:MM:dd HH:mm:ss") + '\0');
                image.SetPropertyItem(prop);
            }

            string newPath = Path.GetDirectoryName(path) + "\\_" + Path.GetFileName(path);
            image.Save(newPath);
            image.Dispose();
        }

        private void ShowPropertyItems(string path)
        {
            Image image = new Bitmap(path);
            PropertyItem[] propItems = image.PropertyItems;
            Encoding encoding = DetectEncoding(path);
            //Encoding encoding = Encoding.ASCII;
            DateTime date = new DateTime();

            int count = 0;
            foreach (PropertyItem propItem in propItems)
            {
                string p = encoding.GetString(propItem.Value);

                //Console.WriteLine("Property Item " + count.ToString());
                //Console.WriteLine("   iD: 0x" + propItem.Id.ToString("x"));
                //Console.WriteLine("   type: " + propItem.Type.ToString());
                //Console.WriteLine("   length: " + propItem.Len.ToString() + " bytes");
                //Console.WriteLine(p);

                if (DateTime.TryParseExact(p.TrimEnd('\0'), "yyyy:MM:dd HH:mm:ss", null, System.Globalization.DateTimeStyles.AssumeLocal, out date) != false)
                {
                    Console.WriteLine("DateTaken");
                    Console.WriteLine(p);
                    Console.WriteLine("   iD: 0x" + propItem.Id.ToString("x"));
                }

                //for most
                if (propItem.Id == 0x010F)
                {
                    Console.WriteLine("CamMan");
                    Console.WriteLine(p);
                }
                if (propItem.Id == 0x0110)
                {
                    Console.WriteLine("CamModel");
                    Console.WriteLine(p);
                }

                //for huawei
                if (propItem.Id == 0x11a)
                {
                    Console.WriteLine("CamMan");
                    Console.WriteLine(p);
                }
                if (propItem.Id == 0x11b)
                {
                    Console.WriteLine("CamModel");
                    Console.WriteLine(p);
                }

                count++;
            }
        }

        public static Encoding DetectEncoding(string path)
        {
            using (StreamReader reader = new StreamReader(path, true))
            {
                return reader.CurrentEncoding;
            }
        }

        private void ChangePropertyItems(string path, DateTime newDate, string newCamMan, string newCamModel)
        {
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.Read))
            {
                BitmapSource img = BitmapFrame.Create(fs);

                Freezable newFreezable = img.Clone();
                BitmapSource newImg = (BitmapSource)newFreezable;
                BitmapMetadata newMd = (BitmapMetadata)newImg.Metadata;

                newMd.DateTaken = newDate.ToString();
                newMd.CameraManufacturer = newCamMan;
                newMd.CameraModel = newCamModel;

                fs.Close();
                fs.Dispose();
            }
        }

        private void changeMetadataButton_Click(object sender, EventArgs e)
        {
            ChangePropertyItems(selectedPhoto.FilePath, dateTimePicker.Value, cManText.Text, cModelText.Text);
        }

        private void metadataBoxes_TextChanged(object sender, EventArgs e)
        {
            changeMetadataButton.Enabled = true;
        }
    }
}
