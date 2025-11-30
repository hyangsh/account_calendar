using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace account_calendar
{
    public partial class UcStatistics : UserControl
    {
        private Chart chart1;
        private Label lblTotal;

        public UcStatistics()
        {
            InitializeComponent();
            InitializeCustomControls();
        }

        private void InitializeCustomControls()
        {
            // 여백 설정
            this.Padding = new Padding(20);
            this.BackColor = Color.White;

            // 1. 상단 라벨
            lblTotal = new Label();
            lblTotal.Dock = DockStyle.Top;
            lblTotal.Height = 60;
            lblTotal.TextAlign = ContentAlignment.MiddleCenter;
            lblTotal.Font = new Font("맑은 고딕", 14, FontStyle.Bold);
            lblTotal.Text = "데이터 집계 중...";

            // 2. 차트 생성
            chart1 = new Chart();
            chart1.Dock = DockStyle.Fill;

            ChartArea chartArea = new ChartArea("MainArea");
            chartArea.AxisX.Title = "날짜 (일)";
            chartArea.AxisX.Interval = 1;
            chartArea.AxisX.MajorGrid.LineColor = Color.LightGray;
            chartArea.AxisY.Title = "금액 (원)";
            chartArea.AxisY.MajorGrid.LineColor = Color.LightGray;

            // Y축 라벨이 잘리지 않게 마진을 자동으로 넉넉히 잡도록 설정
            chartArea.AxisY.IsMarginVisible = true;

            chart1.ChartAreas.Add(chartArea);

            // 범례 위치를 왼쪽(Near) 아래(Bottom)로 변경
            Legend legend = new Legend("MainLegend");
            legend.Docking = Docking.Bottom;        // 아래쪽 배치
            legend.Alignment = StringAlignment.Near; // 왼쪽 정렬 (Center -> Near)
            chart1.Legends.Add(legend);

            // 시리즈 설정
            Series seriesIncome = new Series("수입");
            seriesIncome.ChartType = SeriesChartType.Column;
            seriesIncome.Color = Color.MediumSeaGreen;
            seriesIncome.IsValueShownAsLabel = true;
            chart1.Series.Add(seriesIncome);

            Series seriesExpense = new Series("지출");
            seriesExpense.ChartType = SeriesChartType.Column;
            seriesExpense.Color = Color.Red;
            seriesExpense.IsValueShownAsLabel = true;
            chart1.Series.Add(seriesExpense);

            // 컨트롤 추가 및 순서 정리
            this.Controls.Add(chart1);
            this.Controls.Add(lblTotal);
            lblTotal.BringToFront();
            chart1.SendToBack();
        }

        public void LoadData(List<AccountItem> allData, int year, int month)
        {
            // 1. 초기화
            chart1.Series["수입"].Points.Clear();
            chart1.Series["지출"].Points.Clear();
            chart1.ChartAreas["MainArea"].AxisY.StripLines.Clear();

            // Y축 최대값 설정을 초기화 (이전 달 데이터 영향 제거)
            chart1.ChartAreas["MainArea"].AxisY.Maximum = Double.NaN;
            chart1.ChartAreas["MainArea"].RecalculateAxesScale();

            // 2. 데이터 가져오기
            var monthData = allData
                .Where(x => x.Date.Year == year && x.Date.Month == month)
                .ToList();

            // 3. 합계 및 잔액 계산
            int totalIncome = monthData.Where(x => x.IsIncome).Sum(x => x.Amount);
            int totalExpense = monthData.Where(x => !x.IsIncome).Sum(x => x.Amount);
            int balance = totalIncome - totalExpense;

            lblTotal.Text = $"{year}년 {month}월 합계  |  수입: {totalIncome:N0}  -  지출: {totalExpense:N0}  =  잔액: {balance:N0}원";

            if (balance < 0) lblTotal.ForeColor = Color.Red;
            else lblTotal.ForeColor = Color.Black;

            // 4. 데이터 루프 및 최대값 찾기 (그래프 높이 조절용)
            int daysInMonth = DateTime.DaysInMonth(year, month);
            double dailyExpenseAvg = daysInMonth > 0 ? (double)totalExpense / daysInMonth : 0;
            double maxVal = 0; // 이번 달 가장 큰 금액 찾기

            for (int day = 1; day <= daysInMonth; day++)
            {
                var dailyData = monthData.Where(x => x.Date.Day == day).ToList();

                int dailyIncome = dailyData.Where(x => x.IsIncome).Sum(x => x.Amount);
                int dailyExpense = dailyData.Where(x => !x.IsIncome).Sum(x => x.Amount);

                // 최대값 갱신 (수입이든 지출이든 더 큰 쪽)
                if (dailyIncome > maxVal) maxVal = dailyIncome;
                if (dailyExpense > maxVal) maxVal = dailyExpense;

                chart1.Series["수입"].Points.AddXY(day, dailyIncome);
                int pointIndex = chart1.Series["지출"].Points.AddXY(day, dailyExpense);

                if (dailyExpenseAvg > 0 && dailyExpense > dailyExpenseAvg * 1.5)
                {
                    chart1.Series["지출"].Points[pointIndex].Color = Color.OrangeRed;
                }
            }

            // Y축 최대 높이 강제 설정 
            // 가장 높은 막대보다 20% 더 높게 천장을 잡아서 레이블이 잘리지 않게 함
            if (maxVal > 0)
            {

                chart1.ChartAreas["MainArea"].AxisY.Maximum = maxVal * 1.2;
            }

            // 5. 평균선 그리기
            if (dailyExpenseAvg > 0)
            {
                StripLine averageLine = new StripLine();
                averageLine.StripWidth = 0;
                averageLine.BorderColor = Color.Red;
                averageLine.BorderWidth = 2;
                averageLine.BorderDashStyle = ChartDashStyle.Dash;
                averageLine.IntervalOffset = dailyExpenseAvg;
                averageLine.Text = $"지출 평균: {dailyExpenseAvg:N0}";

                // 평균선 글자도 잘리지 않게 오른쪽 정렬 유지
                averageLine.TextAlignment = StringAlignment.Far;
                chart1.ChartAreas["MainArea"].AxisY.StripLines.Add(averageLine);
            }
        }
    }
}