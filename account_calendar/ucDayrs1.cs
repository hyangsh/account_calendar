using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace account_calendar
{
    public partial class ucDayrs1 : UserControl
    {
        string _day, date, weekday;

        private int _totalSpent = 0;   // 총 지출
        private int _targetAmount = 0; // 목표 금액
        private int _totalIncome = 0;
        public string DayNo
        {
            get { return _day; }
        }
        public ucDayrs1(string day)
        {
            InitializeComponent();
            _day = day;
            label1.Text = day;
            checkBox1.Hide();
            label1.Click += (s, e) => this.OnClick(e);
            panel1.Click += (s, e) => this.OnClick(e);
            // 날짜 문자열 생성 
            if (string.IsNullOrEmpty(day))
            {
               
                date = "";
                label1.Text = "";
                this.Enabled = false; // 클릭 안되게 비활성화
            }
            else
            {
                date = Form1._month + "/" + _day + "/" + Form1._year;
            }
 
            this.ResizeRedraw = true;

            
            
            panel1.Paint += Panel1_Paint;
            this.label1.Click += (s, e) => this.OnClick(e);     // 날짜 숫자
            this.panel1.Click += (s, e) => this.OnClick(e);     // 배경

            
            this.lblIncome.Click += (s, e) => this.OnClick(e);
            this.lblExpense.Click += (s, e) => this.OnClick(e);
            this.lblTotal.Click += (s, e) => this.OnClick(e);
        }

        private void ucDayrs1_Load(object sender, EventArgs e)
        {
            sundays();
        }

        //  메인 폼에서 데이터를 받아오는 함수
        public void SetGraphData(int spent, int income, int target)
        {
            _totalSpent = spent;
            _totalIncome = income;
            _targetAmount = target;

            // (1) 수입 라벨 설정
            if (income > 0)
                lblIncome.Text = "+" + income.ToString("N0");
            else
                lblIncome.Text = ""; // 0원이면 안 보이게

            // (2) 지출 라벨 설정
            if (spent > 0)
                lblExpense.Text = "-" + spent.ToString("N0");
            else
                lblExpense.Text = "";

            // (3) 합계 라벨 설정
            int netTotal = income - spent;
            if (income > 0 || spent > 0)
            {
                string sign = netTotal >= 0 ? "+" : "";
                lblTotal.Text = "합 " + sign + netTotal.ToString("N0");

                // 이익이면 검정, 손해면 진한 빨강
                lblTotal.ForeColor = (netTotal >= 0) ? Color.Black : Color.DarkRed;
            }
            else
            {
                lblTotal.Text = "";
            }

            // 그래프 다시 그리기 요청
            panel1.Invalidate();
        }

        // 막대그래프 그리기 (시스템이 화면을 그릴 때 자동 호출)
        private void Panel1_Paint(object sender, PaintEventArgs e)
        {
            // 날짜가 없거나 목표금액이 설정 안 됐으면 그리지 않음
            if (string.IsNullOrEmpty(_day) || _targetAmount == 0) return;

            Graphics g = e.Graphics;

             
            // (1) 배경 막대 그래프 그리기
           

            // 지출이 있을 때만 그래프 그림
            if (_totalSpent > 0)
            {
                // 비율 계산
                float ratio = (float)_totalSpent / _targetAmount;

                // 높이 계산 (패널 높이의 80%까지만)
                int maxHeight = (int)(panel1.Height * 0.8);
                int barHeight = (int)(maxHeight * ratio);

                // 그래프가 패널 밖으로 튀어나가지 않게 제한
                if (barHeight > panel1.Height) barHeight = panel1.Height;

                // 색상 결정 (위험도에 따라)
                Color barColor = Color.LightSkyBlue; // 안전
                if (ratio >= 1.0f) barColor = Color.Salmon; // 초과
                else if (ratio >= 0.8f) barColor = Color.Orange; // 위험

                // 그리기 (투명도 50으로 연하게)
                using (SolidBrush brush = new SolidBrush(Color.FromArgb(50, barColor)))
                {
                    g.FillRectangle(brush, 0, panel1.Height - barHeight, panel1.Width, barHeight);
                }
            }


        }
        

        // 기존 로직 유지
        private void sundays()
        {
            try
            {
                if (string.IsNullOrEmpty(date)) return; // 날짜 없으면 패스

                DateTime day = DateTime.Parse(date);
                weekday = day.ToString("dddd");
                if (weekday == "일요일") // 한글 윈도우면 "일요일"일 수도 있으니 주의
                {
                    label1.ForeColor = Color.FromArgb(255, 128, 128);
                }
                else
                {
                    label1.ForeColor = Color.FromArgb(64, 64, 64);
                }
            }
            catch (Exception)
            {
                 
            }
        }

        // 기존 로직 유지
        private void panel1_Click(object sender, EventArgs e)
        {
            if (checkBox1.Checked == false)
            {
                checkBox1.Checked = true;
                this.BackColor = Color.FromArgb(255, 150, 79);
            }
            else
            {
                checkBox1.Checked = false;
                this.BackColor = Color.White;
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {
             
        }
    }
}