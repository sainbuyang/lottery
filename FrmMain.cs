using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using DynamicLottery.Properties;
using System.Collections;
using System.Configuration;
using System.IO;

namespace DynamicLottery
{
    public partial class FrmMain : Form
    {
        private static int LOTTE_SIZE = 10;
        private static string LOG_FILE = "log.txt";
        private static string WINNER_LOG_FILE = "winner.txt";
        private string mglCharacter = "АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ";
        List<string> listAllUsers = new List<string>();
        List<char> listReadyChars = new List<char>();
        List<char> listNumbers = new List<char>();
        List<char> listChars = new List<char>();
        Hashtable customerList = new Hashtable();
        List<Lotte> listLotte = new List<Lotte>();
        Lotte currentLotte = null;

        Generator generator = null;
        System.Threading.Thread thread;
        System.Drawing.Size formOldSize = System.Drawing.Size.Empty;
        string currenNumbers = "";
        int currentRoller = 0;
        bool isAlive = true;
        char lotteNum12 = ' ';
        char lotteNum11 = ' ';
        bool isPicClick11 = false;
        bool isPicClick12 = false;

        int suglaaToo = 0;
        private int idxImage = 0;
        string bigFirstName = "";
        string bigLastName = "";
        string bigBranch = "";
        string bigPosition = "";

        public FrmMain()
        {
            InitializeComponent();
            ScreenBuf();

            // Цифрийн жагсаалт үүсгэх
            for (int i = 0; i < 10; i++)
                listNumbers.Add(Convert.ToChar(i.ToString()));
            // Тэмдэгтийн жагсаалт үүсгэх
            char[] charsArray = mglCharacter.ToCharArray();
            for (int i = 0; i < charsArray.Length; i++)
                listChars.Add(charsArray[i]);

            // Label-ийг Fill хийх
            pic1.Dock = DockStyle.Fill;
            pic2.Dock = DockStyle.Fill;
            pic3.Dock = DockStyle.Fill;
            pic4.Dock = DockStyle.Fill;
            pic5.Dock = DockStyle.Fill;
            pic6.Dock = DockStyle.Fill;
            pic7.Dock = DockStyle.Fill;
            pic8.Dock = DockStyle.Fill;
            pic9.Dock = DockStyle.Fill;
            pic10.Dock = DockStyle.Fill;

        }
        
        public void ScreenBuf()
        {
            int style = NativeWinAPI.GetWindowLong(this.Handle, NativeWinAPI.GWL_EXSTYLE);
            style |= NativeWinAPI.WS_EX_COMPOSIED;
            NativeWinAPI.SetWindowLong(Handle, NativeWinAPI.GWL_EXSTYLE, style);
        }

        private void LotteConfigRead()
        {
            Lotte lotte = null;
            Lotte tmpLotte = null;
            string imageUrl = "";
            ConfigHelper section = (ConfigHelper)ConfigurationManager.GetSection("LotteData");
            if (section != null)
            {
                listLotte.Clear();
                int index = 0;
                foreach (LotteElement element in section.HashKeys)
                {
                    imageUrl = Application.StartupPath + "\\" + element.Image;

                    lotte = new Lotte();
                    lotte.Name = element.Name;

                    for (int i = 0; i < element.Count; i++)
                    {
                        index++;
                        tmpLotte = new Lotte();
                        tmpLotte.Index = index;
                        tmpLotte.Name = lotte.Name;
                        tmpLotte.ImageUrl = lotte.ImageUrl;
                        tmpLotte.Image = lotte.Image;
                        listLotte.Add(tmpLotte);
                    }

                    Console.WriteLine(String.Format("Name {0}, Image {1} ", element.Name, element.Image));
                }
            }
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {
            this.lblName.Text = "";
            this.lblPosition.Text = "";
            //lblResult.Text = "";
            //this.lblLotteName.Text = "";
            //this.lblLotteCount.Text = "";

            this.tmrAD.Enabled = true;

            this.lblName.Font = new Font(lblName.Font.FontFamily, Settings.Default.fontSizeName, FontStyle.Bold);
            this.lblPosition.Font = new Font(lblPosition.Font.FontFamily, Settings.Default.fontSizeBranch, FontStyle.Bold);
            TableLayoutRowStyleCollection styles = this.tableMain.RowStyles;


            //this.lblLotteName.Font = new Font(lblLotteName.Font.FontFamily, Settings.Default.fontSizeLotteName);
            //RowStyle style = styles[1];
            //style.SizeType = SizeType.Percent;
            //style.Height = Settings.Default.footerSize;

            this.BackColor = System.Drawing.Color.White;
            if (!string.IsNullOrEmpty(Settings.Default.headerImage))
            {
                try
                {
                    if (System.IO.Path.IsPathRooted(Settings.Default.headerImage))
                    {
                        Settings.Default.headerImage = Application.StartupPath + "\\" + Settings.Default.headerImage;
                    }
                    this.pnlMain.BackgroundImage = Image.FromFile(Settings.Default.headerImage);
                }
                catch { }
            }
        }

        private void pictureLotte_DoubleClick(object sender, EventArgs e)
        {
            FrmMain_DoubleClick(null, null);
        }

        private void tableInfo_DoubleClick(object sender, EventArgs e)
        {
            FrmMain_DoubleClick(null, null);
        }

        private void tableFooter_DoubleClick(object sender, EventArgs e)
        {
            FrmMain_DoubleClick(null, null);
        }

        private void pnlMain_DoubleClick(object sender, EventArgs e)
        {
            FrmMain_DoubleClick(null, null);
        }

        /// <summary>
        /// DoubleClick хийхэд fullscreen болох
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FrmMain_DoubleClick(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Maximized)
            {
                this.WindowState = FormWindowState.Normal;
                Cursor.Show();
            }
            else
            {
                this.WindowState = FormWindowState.Maximized;
                Cursor.Hide();
            }
        }
        
        /// <summary>
        /// Цонхний хэмжээ өөрчлөгдөхөд дуудагдах
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FrmMain_Resize(object sender, EventArgs e)
        {
            if (formOldSize != System.Drawing.Size.Empty && formOldSize != this.Size)
            {
                float percent = ((float)this.Size.Height) / formOldSize.Height;
                tableFooter.Height = (int)(tableFooter.Height * percent);
            }
            formOldSize = this.Size;
        }
        
        /// <summary>
        /// Гараас товс дарахыг шалгах
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CountSuglaa()
        {
            suglaaToo++;
            if (suglaaToo >= 1 && suglaaToo <= 15)
            {
                //lblCnt.Text = suglaaToo.ToString();
            }
            else
            {
                MessageBox.Show("Сугалаа шалгаруулалт дууссан");
                return;
            }
        }

        private void FrmMain_KeyDown(object sender, KeyEventArgs e)
        {
            if (isAlive)
            {
                // Дараагийн сугалааг эхлүүлэх. Заавар сүүлийн 2 оронг оруулсан байх
                if ((e.KeyCode == Keys.N) && (e.Modifiers & Keys.Control) != 0)
                {
                    //if (listAllUsers.Count < 1)
                    //{
                    //    LotteConfigRead();
                    //    FileRead();
                    //}
                    if (listAllUsers.Count > 0 && suglaaToo < 24)
                    {
                        if (listLotte.Count < 1)
                        {
                            MessageBox.Show("Сугалааны өгөгдөл хоосон байна. Тохиргооны файлд зааж өгнө үү");
                            return;
                        }

                        if (currentLotte != null)
                            listLotte.Remove(currentLotte);

                        if (listLotte.Count < 1)
                        {
                            MessageBox.Show("Шалгаруулалт дууслаа.");
                            return;
                        }

                        if (!(Char.IsDigit(lotteNum11) && Char.IsDigit(lotteNum12)))
                        {
                            //MessageBox.Show("Сүүлийн 2 тоогоо оруулна уу");
                            return;
                        }

                        isPicClick11 = false;
                        isPicClick12 = false;

                        currenNumbers = lotteNum11.ToString() + lotteNum12.ToString();
                        bool isFound = false;
                        foreach (string str in listAllUsers)
                        {
                            if (str.EndsWith(currenNumbers))
                            {
                                isFound = true;
                                break;
                            }
                        }
                        if (!isFound)
                        {
                            MessageBox.Show(currenNumbers + " тоогоор төгссөн мэдээлэл байхгүй байна.");
                            return;
                        }

                        currentLotte = listLotte[0];
                        this.lblName.Text = "";
                        this.lblPosition.Text = "";
                        //lblResult.Text = "";
                        //SetWinnerInvoke(false);
                        SetLabelText(pic1, ' ');
                        SetLabelText(pic2, ' ');
                        SetLabelText(pic3, ' ');
                        SetLabelText(pic4, ' ');
                        SetLabelText(pic5, ' ');
                        SetLabelText(pic6, ' ');
                        SetLabelText(pic7, ' ');
                        SetLabelText(pic8, ' ');
                        SetLabelText(pic9, ' ');
                        SetLabelText(pic10, ' ');

                        //lblLotteName.Text = currentLotte.Name;
                        //pnlMain.BackgroundImage = currentLotte.Image;

                        SetLabelText(pic9, lotteNum11);
                        SetLabelText(pic10, lotteNum12);
                        currentRoller++;
                        currentRoller++;

                        NextNumber();
                        CountSuglaa();
                    }
                    else
                    {
                        MessageBox.Show("CTRL+Enter дарж мэдээллээ оруулна уу!");
                        return;
                    }
                }

                //Дараагийн сугалааг эхлүүлэх
                if ((e.KeyCode == Keys.E) && (e.Modifiers & Keys.Control) != 0)
                {
                    if (listAllUsers.Count > 0 && suglaaToo < 20)
                    {

                        //generator.SetSpeed(0, 0);

                        if (listLotte.Count < 1)
                        {
                            MessageBox.Show("Сугалааны өгөгдөл хоосон байна. Тохиргооны файлд зааж өгнө үү");
                            return;
                        }

                        if (currentLotte != null)
                            listLotte.Remove(currentLotte);

                        if (listLotte.Count < 1)
                        {
                            MessageBox.Show("Шалгаруулалт дууслаа.");
                            return;
                        }

                        if (suglaaToo >= 6)
                        {
                            MessageBox.Show("Шалгаруулалт дууслаа.");
                            return;
                        }

                        isPicClick11 = false;
                        isPicClick12 = false;

                        currentLotte = listLotte[0];

                        this.lblName.Text = "";
                        this.lblPosition.Text = "";
                        //lblResult.Text = "";
                        //SetWinnerInvoke(false);
                        SetLabelText(pic1, ' ');
                        SetLabelText(pic2, ' ');
                        SetLabelText(pic3, ' ');
                        SetLabelText(pic4, ' ');
                        SetLabelText(pic5, ' ');
                        SetLabelText(pic6, ' ');
                        SetLabelText(pic7, ' ');
                        SetLabelText(pic8, ' ');
                        SetLabelText(pic9, ' ');
                        SetLabelText(pic10, ' ');

                        NextNumber();
                        CountSuglaa();
                    }
                    else
                    {
                        MessageBox.Show("CTRL+O дарж мэдээллээ оруулна уу!");
                        return;
                    }
                }

                if ((e.KeyCode == Keys.T) && (e.Modifiers & Keys.Control) != 0)
                {
                    if (listAllUsers.Count > 0)
                    {
                        lblName.Text = bigLastName + " овогтой " + bigFirstName;
                        lblPosition.Text = bigPosition;
                    }
                    else
                    {
                        MessageBox.Show("CTRL+O дарж мэдээллээ оруулна уу!");
                        return;
                    }
                }

                // Цэвэрлэх
                if ((e.KeyCode == Keys.C) && (e.Modifiers & Keys.Control) != 0)
                {
                    SetLabelText(pic1, ' ');
                    SetLabelText(pic2, ' ');
                    SetLabelText(pic3, ' ');
                    SetLabelText(pic4, ' ');
                    SetLabelText(pic5, ' ');
                    SetLabelText(pic6, ' ');
                    SetLabelText(pic7, ' ');
                    SetLabelText(pic8, ' ');
                    SetLabelText(pic9, ' ');
                    SetLabelText(pic10, ' ');
                    lblName.Text = "";
                    lblPosition.Text = "";
                    //lblResult.Text = "";
                }
                
                // Сугалааны файл шинээр унших
                else if ((e.KeyCode == Keys.O) && (e.Modifiers & Keys.Control) != 0)
                {
                    LotteConfigRead();
                    FileRead();
                }
                else if ((e.KeyCode == Keys.F5) && (e.Modifiers & Keys.Control) != 0)
                {
                    FrmMain_DoubleClick(null, null);
                }
                else if ((e.KeyCode == Keys.F11) && (e.Modifiers & Keys.Control) != 0)
                {
                    pic9_DoubleClick(null, null);
                }
                else if ((e.KeyCode == Keys.F12) && (e.Modifiers & Keys.Control) != 0)
                {
                    pic10_DoubleClick(null, null);
                }

                //// Програмыг хаах
                else if ((e.KeyCode == Keys.X) && (e.Modifiers & Keys.Control) != 0)
                {
                    Application.Exit();
                }
            }
        }
        /// <summary>
        /// Сугалааны дараагийн тоо үүсгэх хүсэлт
        /// </summary>
        private void NextNumber()
        {
            int repeatCount = 0;
            int index = 0;
            char charFound;
            char[] chars;
            Hashtable tempData = new Hashtable();
            int totalChar = 0;
            listReadyChars.Clear();
            index = currenNumbers.Length;

            foreach (string str in listAllUsers)
            {
                if (index == 0 || str.EndsWith(currenNumbers))
                {
                    repeatCount = 1; 
                    chars = str.ToCharArray();
                    charFound = chars[LOTTE_SIZE - 1 - index];
                    if (tempData.ContainsKey(charFound))
                    {
                        repeatCount = Convert.ToInt32(tempData[charFound]);
                        tempData.Remove(charFound);
                        repeatCount++;
                    }

                    tempData.Add(charFound, repeatCount);

                    totalChar++;
                }
            }
            int percent = 0;
            int i;

            SetCounterLabelInvoke(totalChar);

            foreach (DictionaryEntry entry in tempData)
            {
                repeatCount = Convert.ToInt32(entry.Value);
                //percent = (repeatCount * 100) / totalChar;
                percent = repeatCount;
                for (i = 0; i < percent; i++)
                {
                    listReadyChars.Add(Convert.ToChar(entry.Key));
                }
            }

            generator.SetReadyChars(listReadyChars);
            isAlive = false;
            if (currentRoller >= 8)
                thread = new System.Threading.Thread(new System.Threading.ThreadStart(generator.RollerChar));
            else
                thread = new System.Threading.Thread(new System.Threading.ThreadStart(generator.Roller));
            thread.Start();
        }
        
        /// <summary>
        /// Тоо эсэхийг шалгах
        /// </summary>
        /// <param name="Value"></param>
        /// <returns></returns>
        public bool IsNumeric(string Value)
        {
            bool RetVal = false;
            if ((Value != null) & (Value.Trim().Length != 0))
            {
                System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"^\d+$");
                RetVal = regex.IsMatch(Value);
            }
            return RetVal;
        }

        public bool IsRegister(string data)
        {
            bool RetVal = false;
            string mglCharacter = "фцужэнгшүзкъещйыбөахролдпячёсмитьвю".ToUpper();
            data = data.ToUpper();
            if ((data != null) && (data.Trim().Length != 0))
            {
                if (data.Trim().Length == 10)
                {
                    System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"^[\D-]{2}[\d-]{8}$");
                    if (regex.IsMatch(data) && mglCharacter.Contains(data.Substring(0, 1)) && mglCharacter.Contains(data.Substring(1, 1)))
                        RetVal = true;
                }
            }
            return RetVal;
        }
        
        /// <summary>
        /// Сугалааны санамсаргүй тоо үүсгэх
        /// </summary>
        private void InitGenerator()
        {
            FrmMain.WriteLog("Хурд " + Properties.Settings.Default.speed + " өөрчлөгдөх тоо " + Properties.Settings.Default.changeCount);
            generator = new Generator(listChars, listNumbers);
            generator.SetSpeed(Properties.Settings.Default.speed, Properties.Settings.Default.changeCount);
            generator.NextEvent += new Generator.NextEventHandler(generator_NextEvent);
            generator.FinishEvent += new Generator.FinishEventHandler(generator_FinishEvent);
        }
        
        /// <summary>
        /// Сугалааны дараагийн цифр сонгогдоход дуудагдана
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="value"></param>
        private void generator_NextEvent(object sender, char value)
        {
            UpdateNumber(value);
        }
        
        /// <summary>
        /// Сугалаа шалгаруулахад дуудагдах
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="value"></param>
        private void generator_FinishEvent(object sender, char value)
        {
            UpdateNumber(value);
            currentRoller++;
            // reset variable
            currenNumbers = value.ToString() + currenNumbers;
            string regNoDel = "";
            int count = 1;
            if (currentRoller >= LOTTE_SIZE)
            {
                lotteNum11 = ' ';
                lotteNum12 = ' ';

                isPicClick11 = false;
                isPicClick12 = false;

                Customer customer = (Customer)customerList[currenNumbers];
                regNoDel = customer.Register;
                FrmMain.WriteWinnerLog("'" + currentLotte.Index + "','" 
                                           + currentLotte.Name + "','" 
                                           + customer.Lotte + "','" 
                                           + customer.LastName + "','" 
                                           + customer.FirstName + "','" 
                                           + customer.Register + "','" 
                                           + customer.BranchName + "','"
                                           //+ customer.BranchName + "','" 
                                           + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff") + "','" 
                                           + currenNumbers + " азтан шалгарав. Нийт " 
                                           + listAllUsers.Count + " азтанаас'");
                
                //MessageBox.Show(regNoDel);
                foreach (DictionaryEntry entity in customerList)
                {
                    count++;
                    Customer remove = (Customer)customerList[entity.Key];
                    if (remove.Register == regNoDel)
                        listAllUsers.RemoveAt(currentLotte.Index);
                        //listAllUsers.RemoveAll(r => r == (remove.Lotte));                   
                }

                //listAllUsers.Remove(currenNumbers);
                //try
                //{
                //    int ret = listAllUsers.RemoveAll(item => item == currenNumbers);
                //    WriteLog(currenNumbers + " remove " + ret);
                //}
                //catch { listAllUsers.Remove(currenNumbers); }

                currenNumbers = "";
                currentRoller = 0;
                SetWinnerInvoke(customer);
                isAlive = true;
            }
            else
                NextNumber();
        }
        
        /// <summary>
        /// Дэлгэцэнд цифр шинэчлэх
        /// </summary>
        /// <param name="value"></param>
        private void UpdateNumber(char value)
        {
            switch (currentRoller)
            {
                case 0:
                    SetLabelTextInvoke(pic10, value); break;
                case 1:
                    SetLabelTextInvoke(pic9, value); break;
                case 2:
                    SetLabelTextInvoke(pic8, value); break;
                case 3:
                    SetLabelTextInvoke(pic7, value); break;
                case 4:
                    SetLabelTextInvoke(pic6, value); break;
                case 5:
                    SetLabelTextInvoke(pic5, value); break;
                case 6:
                    SetLabelTextInvoke(pic4, value); break;
                case 7:
                    SetLabelTextInvoke(pic3, value); break;
                case 8:
                    SetLabelTextInvoke(pic2, value); break;
                case 9:
                    SetLabelTextInvoke(pic1, value); break;
            }
        }
        /// <summary>
        /// Дэлгэцийн цифр шинэчлэх
        /// </summary>
        /// <param name="pic"></param>
        /// <param name="value"></param>
        private void SetLabelTextInvoke(PictureBox lbl, char value)
        {
            if (InvokeRequired)
            {
                this.BeginInvoke(new MethodInvoker(delegate()
                {
                    SetLabelText(lbl, value);
                }));
            }
            else
            {
                SetLabelText(lbl, value);
            }
        }
        /// <summary>
        /// Сугалааны боломжит тоог харуулах
        /// </summary>
        /// <param name="value"></param>
        private void SetCounterLabelInvoke(int value)
        {
            if (InvokeRequired)
            {
                this.BeginInvoke(new MethodInvoker(delegate()
                {
                    //lblLotteCount.Text = value + " " + Settings.Default.LotteAddName;
                }));
            }
            else
            {
                //lblLotteCount.Text = value + " " + Settings.Default.LotteAddName;
            }
        }
        /// <summary>
        /// Азтаны мэдээлэл харуулах
        /// </summary>
        /// <param name="customer"></param>
        
        private void SetWinnerInvoke(Customer customer)
        {
            if (InvokeRequired)
            {
                this.lblName.BeginInvoke(new MethodInvoker(delegate()
                {
                    this.lblName.Text = customer.LastName + " овогтой " + customer.FirstName;
                }));
            }
            else
            {
                this.lblName.Text = customer.LastName + " овогтой " + customer.FirstName;
            }
            if (InvokeRequired)
            {
                this.lblPosition.BeginInvoke(new MethodInvoker(delegate ()
                {
                    this.lblPosition.Text = customer.BranchName;
                }));
            }
            else
            {
                this.lblPosition.Text = customer.BranchName;
            }
        }
        /// <summary>
        /// Дэлгэцийн зургийг оноож өгөх
        /// </summary>
        /// <param name="pic"></param>
        /// <param name="value"></param>
        private void SetLabelText(PictureBox pic, char value)
        {
            switch (value)
            {
                case '0': pic.Image = Properties.Resources._0; break;
                case '1': pic.Image = Properties.Resources._1; break;
                case '2': pic.Image = Properties.Resources._2; break;
                case '3': pic.Image = Properties.Resources._3; break;
                case '4': pic.Image = Properties.Resources._4; break;
                case '5': pic.Image = Properties.Resources._5; break;
                case '6': pic.Image = Properties.Resources._6; break;
                case '7': pic.Image = Properties.Resources._7; break;
                case '8': pic.Image = Properties.Resources._8; break;
                case '9': pic.Image = Properties.Resources._9; break;
                case 'А': pic.Image = Properties.Resources.а; break;
                case 'Б': pic.Image = Properties.Resources.б; break;
                case 'В': pic.Image = Properties.Resources.в; break;
                case 'Г': pic.Image = Properties.Resources.г; break;
                case 'Д': pic.Image = Properties.Resources.д; break;
                case 'Е': pic.Image = Properties.Resources.е; break;
                case 'Ё': pic.Image = Properties.Resources.ё; break;
                case 'Ж': pic.Image = Properties.Resources.ж; break;
                case 'З': pic.Image = Properties.Resources.з; break;
                case 'И': pic.Image = Properties.Resources.и; break;
                case 'Й': pic.Image = Properties.Resources.й; break;
                case 'К': pic.Image = Properties.Resources.к; break;
                case 'Л': pic.Image = Properties.Resources.л; break;
                case 'М': pic.Image = Properties.Resources.м; break;
                case 'Н': pic.Image = Properties.Resources.н; break;
                case 'О': pic.Image = Properties.Resources.о; break;
                case 'П': pic.Image = Properties.Resources.п; break;
                case 'Р': pic.Image = Properties.Resources.р; break;
                case 'С': pic.Image = Properties.Resources.с; break;
                case 'Т': pic.Image = Properties.Resources.т; break;
                case 'У': pic.Image = Properties.Resources.у; break;
                case 'Ф': pic.Image = Properties.Resources.ф; break;
                case 'Х': pic.Image = Properties.Resources.х; break;
                case 'Ц': pic.Image = Properties.Resources.ц; break;
                case 'Ч': pic.Image = Properties.Resources.ч; break;
                case 'Ш': pic.Image = Properties.Resources.ш; break;
                case 'Щ': pic.Image = Properties.Resources.щ; break;
                case 'Ъ': pic.Image = Properties.Resources.ъ; break;
                case 'Ы': pic.Image = Properties.Resources.ы; break;
                case 'Ь': pic.Image = Properties.Resources.ь; break;
                case 'Э': pic.Image = Properties.Resources.э; break;
                case 'Ю': pic.Image = Properties.Resources.ю; break;
                case 'Я': pic.Image = Properties.Resources.я; break;
                default: pic.Image = null; break;
            }
        }
        /// <summary>
        /// Сугалааны мэдээллийг унших
        /// </summary>
        private void FileRead()
        {
            string fileName;
            string[] lines;
            string data;

            Customer customer = null;
            //string lotte;
            string firstName;
            string lastName;
            string register;
            string branch;
            int count = 0;

            //int bigName = 0;
            //string bigNameC = "";

            char[] delimeter = { ',' };

            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            dialog.InitialDirectory = System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
            dialog.Title = "Select a text file";
            DialogResult result = dialog.ShowDialog(); // Show the dialog.
            if (result == DialogResult.OK) // Test result.
            {
                fileName = dialog.FileName;
                FrmMain.WriteLog(fileName + " файлыг уншуулав");
                try
                {
                    customerList.Clear();
                    listAllUsers.Clear();
                    lines = System.IO.File.ReadAllLines(fileName);
                    int j = 0;
                    foreach (string line in lines)
                    {
                        j++;
                        try
                        {
                            data = line.Trim();
                            string[] fieldList = data.Split(delimeter);
                            if (fieldList.Length == 5)
                            {   
                                register = fieldList[0];
                                lastName = fieldList[1];
                                firstName = fieldList[2];
                                branch = fieldList[3];
                                try 
                                { 
                                    count = Convert.ToInt32(fieldList[4]);
                                }
                                catch
                                {
                                    count = 1;
                                }

                                register = register.Trim().ToUpper();
                                if (IsRegister(register))
                                {
                                    if (register.Length == LOTTE_SIZE)
                                    {   
                                        if (!customerList.ContainsKey(register))
                                        {
                                            customer = new Customer
                                            {
                                                Lotte = register.ToUpper(),
                                                Register = register.Trim().ToUpper(),
                                                FirstName = firstName.Trim().ToUpper(),
                                                LastName = lastName.Trim().ToUpper(),
                                                BranchName = branch.Trim().ToUpper(),
                                                RepeatCount = Convert.ToInt32(count)
                                            };

                                            customerList.Add(customer.Lotte, customer);

                                            for (int i = 0; i < customer.RepeatCount; i++)
                                            {
                                                listAllUsers.Add(customer.Lotte);
                                            }
                                        }
                                        else
                                        {
                                            FrmMain.WriteLog(register + " регистрийн дугаар давхардсан байна.");
                                        }
                                    }
                                    else
                                    {
                                        FrmMain.WriteLog(register + " сугалааны дугаарын хэмжээ " + LOTTE_SIZE + " ялгаатай байна. Алгасав.");
                                    }
                                }
                                else
                                {
                                    FrmMain.WriteLog(register + " регистрийн дугаар буруу байна. Алгасав.");
                                }
                                
                            }
                            else
                            {
                                FrmMain.WriteLog(fileName + " файлын алдаатай мөр [" + data + "]");
                            }
                        }
                        catch (Exception ex)
                        {
                            FrmMain.WriteLog(fileName + " файлын уншихад алдаа гарлаа. Message " + ex.Message);
                            FrmMain.WriteLog(fileName + " файлын уншихад алдаа гарлаа. StackTrace " + ex.StackTrace);
                        }
                    }
                    FrmMain.WriteLog(fileName + " файлын " + listAllUsers.Count + " сугалааг амжилттай уншлаа.");
                    if (listAllUsers.Count > 0)
                    {
                        InitGenerator();
                    }
                    else
                        MessageBox.Show(this, "Оролцогчийн мэдээлэл хоосон байна");
                }
                catch (Exception ex)
                {
                    FrmMain.WriteLog(fileName + " файлын уншихад алдаа гарлаа. Message " + ex.Message);
                    FrmMain.WriteLog(fileName + " файлын уншихад алдаа гарлаа. StackTrace " + ex.StackTrace);
                    MessageBox.Show(this, "Файл уншихад алдаа гарлаа. Алдааны мэдээлэл " + ex.Message);
                    listAllUsers.Clear();
                    customerList.Clear();
                }
            }
        }

        /// <summary>
        /// Програмын лог бичих
        /// </summary>
        /// <param name="pstrText"></param>
        public static void WriteLog(String pstrText)
        {
            try
            {
                string filePath = Application.StartupPath + "\\" + LOG_FILE;
                System.IO.FileStream fs = new System.IO.FileStream(filePath, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write);
                System.IO.StreamWriter sw = new System.IO.StreamWriter(fs);

                sw.BaseStream.Seek(0, System.IO.SeekOrigin.End);
                string toSet = DateTime.Now.ToString("o") + ": " + pstrText;
                sw.WriteLine(toSet);

                sw.Close();
            }
            catch { }
        }
       
        /// <summary>
        /// Азтаны мэдээлэл бичих
        /// </summary>
        /// <param name="pstrText"></param>
        public static void WriteWinnerLog(String pstrText)
        {
            try
            {
                string filePath = Application.StartupPath + "\\" + WINNER_LOG_FILE;
                System.IO.FileStream fs = new System.IO.FileStream(filePath, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write);
                System.IO.StreamWriter sw = new System.IO.StreamWriter(fs);

                sw.BaseStream.Seek(0, System.IO.SeekOrigin.End);
                string toSet = pstrText;
                sw.WriteLine(toSet);

                sw.Close();
            }
            catch { }
        }

        /// <summary>
        /// Гараас товч дарахыг сонсох
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FrmMain_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (isAlive)
            {
                // Тоо дарж байгаа эсэх
                if (e.KeyChar >= 48 && e.KeyChar <= 57)
                {
                    if (isPicClick12)
                    {
                        lotteNum12 = e.KeyChar;
                        SetLabelText(pic10, lotteNum12);
                    }
                    else if (isPicClick11)
                    {
                        lotteNum11 = e.KeyChar;
                        SetLabelText(pic9, lotteNum11);
                    }
                    isPicClick11 = false;
                    isPicClick12 = false;
                }
            }
        }

        private void pic9_DoubleClick(object sender, EventArgs e)
        {
            isPicClick11 = true;
            isPicClick12 = false;
        }

        private void pic10_DoubleClick(object sender, EventArgs e)
        {
            isPicClick11 = false;
            isPicClick12 = true;
        }

        private void pic1_Paint(object sender, PaintEventArgs e)
        {
            //int pw = 2;
            //PictureBox pb = (PictureBox)sender;
            //Pen p = new Pen(Brushes.DarkBlue, pw);
            //e.Graphics.DrawRectangle(p, new Rectangle(pw / 2, pw / 2, pb.Width - pw, pb.Height - pw));
        }

        private void tmrAD_Tick(object sender, EventArgs e)
        {
            if (idxImage > 1)
                idxImage = 0;
            switch (idxImage)
            {
                //case 0:
                //    picAD.BackgroundImage = Resources.Ad_01;
                //    break;
                //case 1:
                //    picAD.BackgroundImage = Resources.Ad_02;
                //    break;
                ////case 2:
                ////    picAD.BackgroundImage = Resources.Ad_03;
                ////    break;
            }
            idxImage++;
        }
    }
}
