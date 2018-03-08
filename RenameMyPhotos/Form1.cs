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

namespace RenameMyPhotos
{
    public partial class Form1 : Form
    {
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
                LoadPhotoAndMetadata(listBox1.SelectedItem.ToString());
                renameSelectedButton.Enabled = true;
            }
            else
            {
                renameSelectedButton.Enabled = false;
            }
        }

        private void LoadPhotoAndMetadata(string path)
        {
            string[] pathParts = path.Split('\\');

            string photoName = pathParts[pathParts.Length - 1];
            photoNameLabel.Text = photoName;

            string tempPhotoPath = path.Insert(path.LastIndexOf('.'), "_temp");
            File.Copy(path, tempPhotoPath);

            using (FileStream fs = new FileStream(tempPhotoPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.DeleteOnClose)) //read
            {
                pictureBox1.Image = Image.FromStream(fs);
            }

            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                BitmapSource img = BitmapFrame.Create(fs);
                BitmapMetadata md = (BitmapMetadata)img.Metadata;

                string dateTaken = md.DateTaken;
                if (dateTaken != null)
                {
                    DateTime dt = DateTime.ParseExact(dateTaken, "dd.MM.yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                    dateTaken = dt.ToString("yyyy.MM.dd-HH.mm.ss", System.Globalization.CultureInfo.InvariantCulture);
                }
                else
                {
                    dateTaken = "NoDate";
                }
                dateLabel.Text = dateTaken;

                string cMan = md.CameraManufacturer;
                if (cMan != null)
                {
                    cMan = cMan.Replace(' ', '-');
                }
                else
                {
                    cMan = "NoCamManufacturer";
                }
                cManLabel.Text = cMan;

                string cModel = md.CameraModel;
                if (cModel != null)
                {
                    cModel = cModel.Replace(' ', '-');
                }
                else
                {
                    cModel = "NoCamModel";
                }
                cModelLabel.Text = cModel;

                string newFileName = dateTaken + " " + cMan + " " + cModel;
                newPhotoNameLabel.Text = newFileName;
            }
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

            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                BitmapSource img = BitmapFrame.Create(fs);
                BitmapMetadata md = (BitmapMetadata)img.Metadata;

                string dateTaken = md.DateTaken;
                if (dateTaken != null)
                {
                    DateTime dt = DateTime.ParseExact(dateTaken, "dd.MM.yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                    dateTaken = dt.ToString("yyyy.MM.dd-HH.mm.ss", System.Globalization.CultureInfo.InvariantCulture);
                }
                else
                {
                    dateTaken = "NoDate";
                }

                string cMan = md.CameraManufacturer;
                if (cMan != null)
                {
                    cMan = cMan.Replace(' ', '-');
                }
                else
                {
                    cMan = "NoCamManufacturer";
                }

                string cModel = md.CameraModel;
                if (cModel != null)
                {
                    cModel = cModel.Replace(' ', '-');
                }
                else
                {
                    cModel = "NoCamModel";
                }
                string newFileName = dateTaken + " " + cMan + " " + cModel;

                for (int i = 0; i < pathParts.Length - 1; i++)
                {
                    newPath += pathParts[i] + "\\";
                }
                newPath += newFileName + ".jpg";

                fs.Close();
                fs.Dispose();
            }

            if (newPath != "")
            {
                File.Move(path, newPath);
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
            dateLabel.Text = "";
            cManLabel.Text = "";
            cModelLabel.Text = "";
            newPhotoNameLabel.Text = "";
        }
    }
}
