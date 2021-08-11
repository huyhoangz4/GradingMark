using PRF192_PE_GradeClient.controller;
using PRF192_PE_GradeClient.model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PRF192_PE_GradeClient.view
{
    public partial class Grade : Form
    {
        Controller controller = new Controller();
        private string currentStudentCode = "";
        private string currentQuestion = "";
        private Student student;
        private Mark mark;
        private List<TestCase> listTestCase;
        public Grade()
        {
            InitializeComponent();
            listTestCase = new List<TestCase>();
        }
        // bước 4
        private void btnSearch_Click(object sender, EventArgs e)
        {
            string studentID = txtMssv.Text.Trim();
            if (studentID.Length == 0)
            {
                MessageBox.Show("Enter Student ID, please!!!");
                return;
            }
            else if (!controller.checkStudentID(studentID))
            {
                MessageBox.Show("Student ID is not exist! Enter again!");
                return;
            }
            else
            {
                controller.GetDetailMark(studentID);
                ShowDetail frmDetail = new ShowDetail(studentID);
                frmDetail.Show();
            }
        }

        private void btnDetail_Click(object sender, EventArgs e)
        {
            new ShowDetail().ShowDialog();
        }

        private void dgvGrade_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (dgvGrade.Rows[e.RowIndex].Cells[e.ColumnIndex].Value != null)
                {
                    txtMssv.Text = dgvGrade.Rows[e.RowIndex].Cells[0].Value.ToString();
                }
            }
            catch
            {
                return;
            }
           
        }
        public void RefreshData()
        {
            dgvGrade.DataSource = null;
            dgvGrade.DataSource = controller.GetTableMark();
        }
        private void Grade_Load(object sender, EventArgs e)
        {
            dgvGrade.AllowUserToAddRows = false;
            dgvGrade.ReadOnly = true;
            RefreshData();
        }
        // Bước 1
        private void btnInput_Click(object sender, EventArgs e)
        {
            this.folderBrowserDialog1 = new FolderBrowserDialog(); // tạo ra 1 folder browser
            DialogResult result = this.folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                txtInput.Text = this.folderBrowserDialog1.SelectedPath; // chọn đường dẫn và add vào input
            }
        }
        // Bước 1 
        private void btnTestcase_Click(object sender, EventArgs e)
        {
            this.folderBrowserDialog1 = new FolderBrowserDialog();
            DialogResult result = this.folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                txtTestcase.Text = this.folderBrowserDialog1.SelectedPath;
            }
        }
        // Bước 2 
        private void btnGrade_Click(object sender, EventArgs e)
        {
            lblPro.Text = "Đang chấm ..."; // label
            listTestCase = new List<TestCase>(); // 
            //đọc file txt từ test case
            //chạy file exe
            //lấy giá trị trả về của file exe
            string testCasePath = txtTestcase.Text;
            string exePath = txtInput.Text;
            if (!Directory.Exists(testCasePath) && !Directory.Exists(exePath))
            {
                MessageBox.Show("Not Exsist Directory!");
                return;
            }
            GetListTestCase(testCasePath);
            ExecuteExeFile(exePath);
            lblPro.Text = "Chấm thành công!";
            RefreshData();
        }


        // xuat ra file excel
        private void btnExcelExport_Click(object sender, EventArgs e)
        {
            // ghi dữ liệu ra file excel
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                controller.ToExcel(dgvGrade, saveFileDialog1.FileName);
            }

        }

        private void folderBrowserDialog2_HelpRequest(object sender, EventArgs e)
        {

        }

        // lấy ra list chứa tất cả các test case, mỗi test case(vd: tc1.txt) là 1 phần tử
        public void GetListTestCase(string dirPath) // đường dẫn mình trỏ đến chứa test case
        {
            // không có folder thì thì show message
            if (!Directory.Exists(dirPath))
            {
                MessageBox.Show("Not Exsist Directory!");
            }
            else
            {
                // gọi ra 1 file con cấp 1 (gồm 10 phần tử (từ Q1>>Q10))
                string[] dirEntries = Directory.GetDirectories(dirPath); // chỉ tới cái đường dẫn
                if (dirEntries.Length > 0)
                {
                    // vòng for đầu là Q1
                    foreach (string dirEntryPath in dirEntries)
                    {
                        // đệ quy và quay về chạy hàm get list test case
                        GetListTestCase(dirEntryPath);
                    }
                }
                // 
                string[] fileEntries = Directory.GetFiles(dirPath); // dirPath : là q1
                if (fileEntries.Length > 0)
                {
                    // tạo 1 đối tượng test case
                    TestCase testquestion;
                    // chạy các phần tử trong Q1
                    foreach (string fileName in fileEntries)
                    {

                        // nếu file có đuôi là txt
                        if (fileName.EndsWith(".txt"))
                        {
                            // khởi tạo 1 đối tượng mới
                            testquestion = new TestCase();
                            // file name là 1.txt
                            testquestion.QuestionName = Path.GetFileName(Path.GetDirectoryName(fileName));
                            // tạo 1 cái list test case là dictionary mới (tên, list trong đó)
                            testquestion.ListTestCase = new Dictionary<string, List<string>>();

                            // đọc cái dòng trong file txt (list rỗng)
                            List<string> lineContent = new List<string>();
                            string[] lines = File.ReadAllLines(fileName); // đọc tất các dòng trong file txt
                            foreach (string line in lines)
                            {
                                if (line.Equals("INPUT:") || line.Equals("OUTPUT:") || line.Equals("MARK:"))
                                {
                                    switch (line)
                                    {
                                        case "INPUT:":
                                            lineContent = new List<string>();
                                            break;
                                        case "OUTPUT:":
                                            testquestion.ListTestCase.Add("input", lineContent);
                                            lineContent = new List<string>();
                                            break;
                                        case "MARK:":
                                            testquestion.ListTestCase.Add("output", lineContent);
                                            lineContent = new List<string>();
                                            break;
                                    }
                                }
                                else
                                {
                                    if (line.Trim() != "")
                                    {
                                        lineContent.Add(line);
                                    }
                                }
                            }
                            testquestion.ListTestCase.Add("mark", lineContent);
                            listTestCase.Add(testquestion); // tạo ra 1 test case vào 1 list
                            // list test case tương đương với các Q1,Q2...
                        }
                    }
                }

            }
        }

        // hàm chạy từng file .exe với input lấy từ list test case
        // chạy hết test case
        public void RunExe(string path)
        {
            try
            {
                // chạy lấy list test case 
                // labda expression : tc (từng đối tượng test case )
                // qsName = curr (= Q1)
                // vòng 1 : chạy file 1.txt, 2 là 2.txt
                // chạy 2 lần
                foreach (TestCase testCase in listTestCase.Where(tc => tc.QuestionName == currentQuestion))
                {
                    // xét thông tin cho file
                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    startInfo.FileName = path; // trỏ đến đường dẫn (File Name (Q1...))
                    startInfo.UseShellExecute = false; // luôn phải để là false
                    startInfo.CreateNoWindow = true; // k bị hiện lên 1 cửa sổ khác (command)
                    startInfo.RedirectStandardInput = true; // cho mình input đầu vào
                    startInfo.RedirectStandardOutput = true; // cho mình đọc đầu ra

                    Process process = Process.Start(startInfo); // bắt đầu chạy 
                    StreamWriter sw = process.StandardInput; // khai báo 1 streamwriter để nhập input đầu vào
                    // duyệt trong đối tượng test case (1.txt) lấy đối tượng list test case ( key : input)
                    foreach (string param in testCase.ListTestCase["input"])
                    // input là 5 4 ...

                    {
                        sw.WriteLine(param); // đọc 5 và 4
                    }
                    sw.Close(); // đóng streamwriter

                    bool startWrite = false; // check xem đọc tới đâu
                    List<string> outputLines = new List<string>(); // lưu dòng output
                    // đọc từ trên xuống dưới của cái command line
                    while (!process.StandardOutput.EndOfStream)
                    {
                        // đọc từng dòng từ trên xuống
                        string line = process.StandardOutput.ReadLine();

                        if (startWrite)
                        {
                            outputLines.Add(line); // output
                            continue;
                        }

                        if (line.Trim().Equals("OUTPUT:"))
                        {
                            startWrite = true;
                        }

                    }
                    startWrite = false;
                    process.WaitForExit(); // chờ để thoát
                    process.Close(); // đóng command

                    List<string> result = testCase.ListTestCase["output"]; // lấy list output từ testcase
                    List<string> markInTxt = testCase.ListTestCase["mark"]; // lấy mark từ testcase
                    // result là output trong file txt, output line là trong chương trình chạy
                    if (result.Count == outputLines.Count)
                    {
                        // so sánh các dòng với nhau
                        for (int i = 0; i < result.Count; i++)
                        {
                            // nếu k = nhau thì break;
                            if (!result[i].Equals(outputLines[i]))
                            {
                                break;
                            }
                            else// nếu output giống với kết quả chương trình thì add vào database
                            {
                                foreach (string m in markInTxt)
                                {
                                    // add mark vào database
                                    // mark là đối tượng mark
                                    controller.AddMarkDetail(student, mark, currentQuestion,  
                                        // điểm là m (0,5)
                                        Convert.ToDouble(m.Trim().ToString()));
                                }
                                
                            }
                        }
                    }                   
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception: " + ex);
            }
        }
        // Bước 3
        // Step 1 : 
        // hàm chạy lần lượt tất cả các file .exe
        // mình sẽ truyền vào đường dẫn folder chứa tất cả các file bài làm
        public void ExecuteExeFile(string dirPath) // truyền vào đường dẫn chưa file bài làm (HE_...)
        {
            if (!Directory.Exists(dirPath))//Check xem đường dẫn truyền vào có tồn tại ko
            {
                MessageBox.Show("Not Exsist Directory!");
            }
            else
            {
                // gọi ra 1 cái pathName (tên đường dẫn) chỉ tới cái FileName mình select
                string pathName = Path.GetFileName(dirPath);

                if (pathName.StartsWith("HE"))
                {
                    currentStudentCode = pathName;
                    // thêm student vào database
                    student = controller.addNewStudent(currentStudentCode);
                    // thêm mark vào grid view, điểm tổng lúc này là 0
                    controller.addNewMark(student);
                    //lấy đối tượng mark vừa thêm vào từ database -> lấy record id từ hàm getMark
                    mark = controller.GetMark(student);
                }
                // check đường dẫn bắt đầu bằng Q(Q1,Q2...)
                if (pathName.StartsWith("Q"))
                {
                    currentQuestion = pathName;
                }
                // sử dụng đệ quy để lấy ra list thư mục
                string[] dirEntries = Directory.GetDirectories(dirPath);
                // mỗi phần tử của dirEntries (trong mảng) là 1 folder
                if (dirEntries.Length > 0)
                {

                    foreach (string dirEntryPath in dirEntries)
                    {
                        // chạy đệ quy để trả về 1 danh sách đã được lấy
                        ExecuteExeFile(dirEntryPath);
                    }
                }
                // khi vào tới Q1 (tiếp đến step 2)
                string[] fileEntries = Directory.GetFiles(dirPath);
                if (fileEntries.Length > 0)
                {
                    foreach (string fileName in fileEntries)
                    {
                        if (fileName.EndsWith(".exe"))
                        {
                            // thực hiện việc chấm
                            RunExe(fileName);
                            // chấm xong thì update lại điểm vào grid view
                            controller.UpdateMark(student,mark);
                        }
                    }
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            GradeDetails frm = new GradeDetails();
            frm.Show();
        }

        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {

        }
    }
}
