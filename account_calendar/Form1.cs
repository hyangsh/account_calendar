using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;  
using System.Linq;           
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace account_calendar
{
     


    public partial class Form1 : Form
    {
        public static int _month, _year;
        private UcStatistics statsView = null;
        // 전체 데이터 리스트
        private List<AccountItem> accountData = new List<AccountItem>();

        // 현재 선택된 날짜 (기본값: 오늘)
        private DateTime selectedDate = DateTime.Today;

        // 목표 금액
        private int dailyTargetAmount = 30000;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            _month = DateTime.Now.Month;
            _year = DateTime.Now.Year;

           
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // 초기 달력 표시
            showDays(_month, _year);

            // 초기 우측 패널 세팅
            SelectDate(DateTime.Today);
        }

        private void showDays(int month, int year)
        {
            flowLayoutPanel1.Controls.Clear();
            _year = year;
            _month = month;

            lbMonth.Text = new DateTimeFormatInfo().GetMonthName(month).ToUpper() + " " + year;

            DateTime startMonth = new DateTime(year, month, 1);
            int days = DateTime.DaysInMonth(year, month);
            int week = Convert.ToInt32(startMonth.DayOfWeek.ToString("d"));

            // 공백 채우기
            for (int i = 0; i < week; i++)  
            {
                ucDayrs1 uc = new ucDayrs1("");
                flowLayoutPanel1.Controls.Add(uc);
            }

            // 날짜 채우기
            for (int i = 1; i <= days; i++)
            {
                ucDayrs1 uc = new ucDayrs1(i + "");
                int currentDay = i;
                DateTime currentDate = new DateTime(year, month, i);

                // 1. 지출 계산 
                int dailySpent = accountData
                    .Where(x => x.Date.Date == currentDate.Date && !x.IsIncome)
                    .Sum(x => x.Amount);

                // 2. 수입 계산
                int dailyIncome = accountData
                    .Where(x => x.Date.Date == currentDate.Date && x.IsIncome)
                    .Sum(x => x.Amount);

                // 3. 데이터 주입 
               
                uc.SetGraphData(dailySpent, dailyIncome, dailyTargetAmount);

                // 클릭 이벤트 연결
                uc.Click += (sender, e) => SelectDate(new DateTime(year, month, currentDay));

                flowLayoutPanel1.Controls.Add(uc);
            }
        }
        private void UpdatePanelGraph(ucDayrs1 uc, DateTime date)
        {
            int dailySpent = accountData
                .Where(x => x.Date.Date == date.Date && !x.IsIncome)
                .Sum(x => x.Amount);
            int dailyIncome = accountData
            .Where(x => x.Date.Date == date.Date && x.IsIncome)
            .Sum(x => x.Amount);
            uc.SetGraphData(dailySpent, dailyIncome, dailyTargetAmount);
        }

        // 특정 날짜의 지출 합계를 구하는 헬퍼 함수
        private int GetDailySpentAmount(DateTime date)
        {
            // accountData 리스트에서 해당 날짜(date)이면서, 지출(!IsIncome)인 것들의 합계
            return accountData
                .Where(x => x.Date.Year == date.Year && x.Date.Month == date.Month && x.Date.Day == date.Day && !x.IsIncome)
                .Sum(x => x.Amount);
        }
        private void SelectDate(DateTime date)
        {
            selectedDate = date;
            lblSelectedDate.Text = $"{date.ToString("yyyy년 MM월 dd일")} 내역";

            // 해당 날짜의 데이터만 뽑아서 그리드뷰에 표시
            var dailyList = accountData.Where(x => x.Date.Date == date.Date).ToList();

            // 그리드뷰 갱신 (DataSource 재설정)
            dataGridView1.DataSource = null;
            dataGridView1.DataSource = dailyList;

            // 불필요한 컬럼 숨기기
            if (dataGridView1.Columns["Id"] != null) dataGridView1.Columns["Id"].Visible = false;
        }
       
       
       

        // 화면 전체 갱신 
        private void RefreshUI()
        {
            // 1. 우측 리스트 다시 로드
            SelectDate(selectedDate);

            // 2. 좌측 달력 그래프 다시 그리기 (전체 다시 그리기 or 해당 날짜만 찾기)
            
            showDays(_month, _year);
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            _month -= 1;
            if (_month < 1)
            {
                _month = 12;
                _year -= 1;
            }
            showDays(_month, _year);

        }


        private void btnAdd_Click_1(object sender, EventArgs e)
        {
            // 유효성 검사
            if (string.IsNullOrWhiteSpace(txtContent.Text) || string.IsNullOrWhiteSpace(txtAmount.Text))
            {
                MessageBox.Show("내용과 금액을 입력해주세요.");
                return;
            }

            int amount;
            if (!int.TryParse(txtAmount.Text, out amount))
            {
                MessageBox.Show("금액은 숫자만 입력 가능합니다.");
                return;
            }

            // 데이터 생성 및 추가
            AccountItem newItem = new AccountItem
            {
                Date = selectedDate, // 현재 선택된 날짜로 저장
                Description = txtContent.Text,
                Amount = amount,
                IsIncome = rbIncome.Checked // 라디오버튼 확인
            };

            accountData.Add(newItem);

            // 화면 갱신
            RefreshUI();

            // 입력창 초기화
            txtContent.Text = "";
            txtAmount.Text = "";
        }

        private void btnDelete_Click_1(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("삭제할 항목을 선택해주세요.");
                return;
            }

            // 선택된 행의 데이터 가져오기
            var selectedItem = dataGridView1.SelectedRows[0].DataBoundItem as AccountItem;

            if (selectedItem != null)
            {
                // 리스트에서 제거
                accountData.Remove(selectedItem);
                RefreshUI();
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("수정할 항목을 목록에서 선택해주세요.");
                return;
            }

            // 2. 입력값 유효성 검사  
            if (string.IsNullOrWhiteSpace(txtContent.Text) || string.IsNullOrWhiteSpace(txtAmount.Text))
            {
                MessageBox.Show("내용과 금액을 입력해주세요.");
                return;
            }

            int newAmount;
            if (!int.TryParse(txtAmount.Text, out newAmount))
            {
                MessageBox.Show("금액은 숫자만 입력 가능합니다.");
                return;
            }

            // 3. 원본 데이터 가져오기
            var selectedItem = dataGridView1.SelectedRows[0].DataBoundItem as AccountItem;

            if (selectedItem != null)
            {
                //데이터 수정 (새로운 입력값으로 덮어쓰기)
                selectedItem.Description = txtContent.Text;
                selectedItem.Amount = newAmount;
                selectedItem.IsIncome = rbIncome.Checked;

                // 4. 화면 갱신
                
                UpdateSingleDayGraph(selectedDate); // 아까 만든 그래프 갱신 함수
                SelectDate(selectedDate);           // 리스트(표) 새로고침

                MessageBox.Show("수정되었습니다.");

                
                txtContent.Text = "";
                txtAmount.Text = "";
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || dataGridView1.SelectedRows.Count == 0) return;

            // 1. 선택된 줄의 데이터 원본 가져오기
             
            var selectedItem = dataGridView1.SelectedRows[0].DataBoundItem as AccountItem;

            if (selectedItem != null)
            {
                // 2. 입력창(텍스트박스)에 값 채워넣기
                txtContent.Text = selectedItem.Description;
                txtAmount.Text = selectedItem.Amount.ToString();

                // 3. 수입/지출 라디오 버튼 체크
                if (selectedItem.IsIncome)
                    rbIncome.Checked = true;
                else
                    rbExpense.Checked = true;
            }
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {
            _month += 1;
            if (_month > 12)
            {
                _month = 1;
                _year += 1;
            }
            showDays(_month, _year);
        }

        private void btnStats_Click(object sender, EventArgs e)
        {
            splitContainer1.Visible = false;

            // 2. 통계 화면이 아직 안 만들어졌다면 생성
            if (statsView == null)
            {
                statsView = new UcStatistics();
                statsView.Dock = DockStyle.Fill; // 폼 전체 채우기

                
                this.Controls.Add(statsView);
                statsView.BringToFront(); // 맨 앞으로 가져오기
            }

            // 3. 통계 화면 보여주기
            statsView.Visible = true;
            panelMenu.BringToFront();
        }

        private void btnCalendar_Click(object sender, EventArgs e)
        {
            // 1. 통계 화면 숨기기
            if (statsView != null)
            {
                statsView.Visible = false;
            }

            // 2. 달력 화면 다시 보여주기
            splitContainer1.Visible = true;
        }

        // [달력 보기] 버튼 클릭


        private void UpdateSingleDayGraph(DateTime targetDate)
        {
             
            foreach (Control c in flowLayoutPanel1.Controls)
            {
                // 1. 이 컨트롤이 우리가 만든 날짜칸(ucDayrs1)인지 확인
                if (c is ucDayrs1 uc)
                {
                    // 2. 이 칸이 우리가 찾던 그 날짜(targetDate)인지 확인
                   
                    if (uc.DayNo == targetDate.Day.ToString())
                    {
                        // 3. 해당 날짜의 '지출' 합계만 다시 계산
                        int dailySpent = accountData
                            .Where(x => x.Date.Date == targetDate.Date && !x.IsIncome)
                            .Sum(x => x.Amount);

                        int dailyIncome = accountData
                        .Where(x => x.Date.Date == targetDate.Date && x.IsIncome)
                        .Sum(x => x.Amount);
                        // 4. 그래프 데이터 업데이트 (여기서 화면이 다시 그려짐)
                        uc.SetGraphData(dailySpent, dailyIncome, dailyTargetAmount);

                        break;  
                    }
                }
            }
        }


    } 

    public class AccountItem
    {
        public DateTime Date { get; set; }     // 날짜

        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Type => IsIncome ? "수입" : "지출";
        public string Description { get; set; } // 내역
        public int Amount { get; set; }        // 금액
        public bool IsIncome { get; set; }     // 수입(true)/지출(false)
    }

}