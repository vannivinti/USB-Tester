using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using PresentationControls;


namespace USB_Tester
{
    public partial class Form1 : Form
    {
        Label driveLabel = new Label();
        Label fileSizeLabel = new Label();
        Label loopsLabel = new Label();
        Button benchmarkButton = new Button();
        CheckBoxComboBox driveLetter = new CheckBoxComboBox();
        ComboBox fileSize = new ComboBox();
        ComboBox loops = new ComboBox();
        //ListBox driveLetter = new ListBox();
        TextBox resultArea = new TextBox();
        string testDriveLetter;
        int testFileSize;
        int testIterations;

        #region DELEGATES
        private delegate void UpdateTextBox(string message);
        #endregion

        public Form1()
        {
            InitializeComponent();

            this.Text = "USB Perfomance Analyzer v1.0";
            this.Size = new Size(600, 350);
            driveLabel.Location = new Point(5, 8);
            driveLabel.Text = "Device drive letter";
            driveLabel.Size = new Size(95, 20);
            driveLetter.Location = new Point(103, 5);
            driveLetter.Size = new Size(43, 15);
            driveLetter.Items.AddRange(GetRemovableDriveLetters());
            if (GetRemovableDriveLetters().Length > 0)
            {
                driveLetter.SelectedIndex = 0;
            }
            fileSizeLabel.Location = new Point(161, 8);
            fileSizeLabel.Text = "File size";
            fileSizeLabel.Size = new Size(50, 20);
            fileSize.Location = new Point(213, 5);
            fileSize.Size = new Size(49, 15);
            fileSize.Items.AddRange(new object[]
              {"100",
               "200",
               "400",
               "800",
               "1000",
               "2000"});
            fileSize.SelectedIndex = 1;
            loopsLabel.Location = new Point(288, 8);
            loopsLabel.Text = "Loops";
            loopsLabel.Size = new Size(46, 20);
            loops.Location = new Point(334, 5);
            loops.Size = new Size(57, 15);
            loops.Items.AddRange(new object[]
              {"1",
               "2",
               "3",
               "4",
               "5",
               "10",
               "20",
               "40",
               "INFINITE"});

            loops.SelectedIndex = 4;
            benchmarkButton.Location = new Point(449, 5);
            benchmarkButton.Size = new Size(50, 20);
            benchmarkButton.Text = "Start";
            resultArea.Location = new Point(5, 30);
            resultArea.Size = new Size(575, 278);
            resultArea.ReadOnly = true;
            resultArea.Multiline = true;
            resultArea.ScrollBars = ScrollBars.Vertical;
            resultArea.WordWrap = false;
            resultArea.Font = new Font("Courier New", 8);
            this.Controls.Add(driveLabel);
            this.Controls.Add(driveLetter);
            this.Controls.Add(fileSizeLabel);
            this.Controls.Add(fileSize);
            this.Controls.Add(loopsLabel);
            this.Controls.Add(loops);
            this.Controls.Add(benchmarkButton);
            this.Controls.Add(resultArea);
            benchmarkButton.Click += new EventHandler(benchmarkButton_Click);
        }

        private void OnUpdateTextBox(string message)
        {
            resultArea.AppendText(message);
        }

        private string[] GetRemovableDriveLetters()
        {
            System.Collections.ArrayList RemovableDriveLetters = new System.Collections.ArrayList();
            DriveInfo[] allDrives = DriveInfo.GetDrives();

            foreach (DriveInfo d in allDrives)
            {
                if (d.DriveType == DriveType.Removable)
                {
                    RemovableDriveLetters.Add(d.Name.Substring(0, 1));
                }
            }

            return RemovableDriveLetters.ToArray(typeof(string)) as string[];
        }

        private void benchmarkButton_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < driveLetter.Items.Count; i++)
            {
                if(driveLetter.CheckBoxItems[i].Checked == true)
                {
                    testDriveLetter = driveLetter.CheckBoxItems[i].Text + ":";
                    testFileSize = Convert.ToInt32(fileSize.Text) * 1000000;
                    try
                    {
                        testIterations = Convert.ToInt32(loops.Text);
                    }
                    catch
                    {
                        testIterations = -1;
                    }

                    Thread testThread = new Thread(new ParameterizedThreadStart(TestPerf));
                    testThread.Start(testDriveLetter);
                }
               
            }
            
        }

        private void TestPerf(object obj)
        {
            string letter = (string)obj;
            string filenameappend = letter.Replace(":", string.Empty);
            string message;
            int j = 0;
           
            try
            {
                int appendIterations = testFileSize / 100000;

                message = "Running a " + testFileSize / 1000000 + "MB file write on drive " + letter + " " + testIterations + " times...\r\n";
                BeginInvoke(new UpdateTextBox(OnUpdateTextBox), new object[] { message });

                double totalPerf = 0;
                DateTime startTime;
                DateTime stopTime;
                string randomText = RandomString(100000);

                while(true)
                //for (int j = 1; j <= testIterations; j++)
                {
                    j++;
                    if ( (j>testIterations) && (testIterations!= -1) )
                    {
                        break;
                    }

                    Application.DoEvents();
                    if (File.Exists(System.Environment.CurrentDirectory + "\\" + j + filenameappend + "test.tmp"))
                    {
                        File.Delete(System.Environment.CurrentDirectory + "\\" + j + filenameappend + "test.tmp");
                    }

                    StreamWriter sWriter = new StreamWriter(System.Environment.CurrentDirectory + "\\" + j + filenameappend + "test.tmp", true);
                    for (int i = 1; i <= appendIterations; i++)
                    {
                        sWriter.Write(randomText);
                    }

                    sWriter.Close();
                    startTime = DateTime.Now;
                    File.Copy(System.Environment.CurrentDirectory + "\\" + j + filenameappend + "test.tmp", letter + "\\" + j + filenameappend + "test.tmp");
                    stopTime = DateTime.Now;
                    File.Delete(System.Environment.CurrentDirectory + "\\" + j + filenameappend + "test.tmp");
                    File.Delete(filenameappend +":" + "\\" + j + filenameappend + "test.tmp");
                    TimeSpan interval = stopTime - startTime;

                    message = letter+"Iteration "   + j + ":   " + Math.Round((testFileSize / 1000) / interval.TotalMilliseconds, 2) +" MB/sec\r\n";
                    BeginInvoke(new UpdateTextBox(OnUpdateTextBox), new object[] { message });
                    totalPerf += (testFileSize / 1000) / interval.TotalMilliseconds;
                }

                if (testIterations != -1)
                {
                    message = "------------------------------\r\n";
                    message += letter + "Average:      " + Math.Round(totalPerf / testIterations, 2) + " MB/sec\r\n";
                    message += "------------------------------\r\n";
                    BeginInvoke(new UpdateTextBox(OnUpdateTextBox), new object[] { message });
                }
                
                //resultArea.Invalidate();
            }
            catch (Exception e)
            {
                message = letter+"An error occured: " + e.Message + "\r\n";
                BeginInvoke(new UpdateTextBox(OnUpdateTextBox), new object[] { message });
                //resultArea.AppendText("An error occured: " + e.Message + "\r\n");
            }
        }

        private string RandomString(int size)
        {
            StringBuilder builder = new StringBuilder();
            Random random = new Random();
            char ch;
            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }
            return builder.ToString();
        }
    }
}
