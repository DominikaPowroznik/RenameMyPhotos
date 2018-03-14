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
        private string currentDirectory = "";
        private string currentFilePath = "";

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
                string[] files;
                if (recursiveCheckBox.Checked)
                {
                    files = Directory.EnumerateFiles(textBox1.Text, "*.*", SearchOption.AllDirectories).Where(s => s.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) || s.EndsWith(".png", StringComparison.OrdinalIgnoreCase)).ToArray();
                }
                else
                {
                    files = Directory.EnumerateFiles(textBox1.Text, "*.*", SearchOption.TopDirectoryOnly).Where(s => s.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) || s.EndsWith(".png", StringComparison.OrdinalIgnoreCase)).ToArray();
                }

                if (files.Any())
                {
                    listBox1.Items.AddRange(files);
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
                currentFilePath = listBox1.SelectedItem.ToString();
                LoadPhotoAndMetadata(currentFilePath);
                renameSelectedButton.Enabled = true;
            }
            else
            {
                renameSelectedButton.Enabled = false;
            }
        }

        private void LoadPhotoAndMetadata(string path)
        {
            //ShowPropertyItems(path);
            //ChangeDateTaken(path);
            //CopyPropertyItems(path);

            string[] pathParts = path.Split('\\');

            string photoName = pathParts[pathParts.Length - 1];
            photoNameLabel.Text = photoName;

            string tempPhotoPath = path.Insert(path.LastIndexOf('.'), "_temp");
            File.Copy(path, tempPhotoPath);

            using (FileStream fs = new FileStream(tempPhotoPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.DeleteOnClose)) //read
            {
                pictureBox1.Image = Image.FromStream(fs);
            }

            string dateTaken, cMan, cModel;

            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                BitmapSource img = BitmapFrame.Create(fs);
                BitmapMetadata md = (BitmapMetadata)img.Metadata;

                if(GetDateTaken(md) != null)
                {
                    dateTimePicker.CustomFormat = "dd.MM.yyyy HH:mm:ss";
                    dateTimePicker.Value = GetDateTaken(md).Value;
                    dateTaken = GetDateTaken(md).Value.ToString("yyyy.MM.dd HH.mm.ss", System.Globalization.CultureInfo.InvariantCulture);
                }
                else
                {
                    dateTimePicker.CustomFormat = " ";
                    dateTaken = "";
                }

                cMan = GetCamManufacturer(md);
                cManText.Text = cMan;

                cModel = GetCamModel(md);
                cModelText.Text = cModel;

                if (dateTaken != "" && cMan != "" && cModel != "")
                {
                    string newFileName = dateTaken + " " + cMan + " " + cModel;
                    newPhotoNameLabel.Text = newFileName;
                }
                else
                {
                    newPhotoNameLabel.Text = "Can't generate foto name!";
                }
            }

            dateTimePicker.Enabled = true;
            cManText.Enabled = true;
            cModelText.Enabled = true;
        }

        private void renameAllButton_Click(object sender, EventArgs e)
        {
            ClearView();
            List<string> photoPaths = new List<string>();
            foreach (var item in listBox1.Items)
            {
                photoPaths.Add(item.ToString());
            }
            foreach (var item in photoPaths)
            {
                RenameFile(item);
            }
            LoadFilenames(currentDirectory);
        }

        private void renameSelectedButton_Click(object sender, EventArgs e)
        {
            ClearView();
            RenameFile(listBox1.SelectedItem.ToString());
            LoadFilenames(currentDirectory);
        }

        private void RenameFile(string path)
        {
            listBox1.ClearSelected();

            string[] pathParts = path.Split('\\');
            string newPath = "";
            string dateTaken, cMan, cModel;

            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                BitmapSource img = BitmapFrame.Create(fs);
                BitmapMetadata md = (BitmapMetadata)img.Metadata;

                if (GetDateTaken(md) != null)
                {
                    dateTaken = GetDateTaken(md).Value.ToString("yyyy.MM.dd HH.mm.ss", System.Globalization.CultureInfo.InvariantCulture);
                }
                else
                {
                    dateTaken = "";
                }
                cMan = GetCamManufacturer(md);
                cModel = GetCamModel(md);

                if (dateTaken != "" && cMan != "" && cModel != "")
                {
                    string newFileName = dateTaken + " " + cMan + " " + cModel;

                    for (int i = 0; i < pathParts.Length - 1; i++)
                    {
                        newPath += pathParts[i] + "\\";
                    }
                    newPath += newFileName + ".jpg";
                }

                fs.Close();
                fs.Dispose();
            }

            if (newPath != "")
            {
                File.Move(path, newPath);
            }
            else
            {
                MessageBox.Show("Not enough metadata to generate new file name!");
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

        private string GetCamManufacturer(BitmapMetadata md)
        {
            string cMan = md.CameraManufacturer;
            if (cMan != null)
            {
                return cMan;
            }
            return "";
        }

        private string GetCamModel(BitmapMetadata md)
        {
            string cModel = md.CameraModel;
            if (cModel != null)
            {
                return cModel;
            }
            return "";
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
            ChangePropertyItems(currentFilePath, dateTimePicker.Value, cManText.Text, cModelText.Text);
        }

        private void metadataBoxes_TextChanged(object sender, EventArgs e)
        {
            changeMetadataButton.Enabled = true;
        }
    }
}
