using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AdvanceLogReader.Popups
{
    public partial class TimePickerPopup : Window
    {
        /// <summary>
        /// ������������
        /// </summary>
        private MainWindow? MainWindow;
        /// <summary>
        /// �Ƿ��趨��Ϊ��ʼʱ��
        /// </summary>
        private bool IsStartDt;

        public TimePickerPopup()
        {
            InitializeComponent();
#if DEBUG
            //this.AttachDevTools();
#endif
            SetCurrTime();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            //�󶨰�ť�¼�
            this.FindControl<Button>("Btn_Commit").Click += TimePickerPopup_Click;
            Closing += TimePickerPopup_Closing;
        }
        /// <summary>
        /// �ر�ʱ�����¼�
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimePickerPopup_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            SetTime();
        }
        /// <summary>
        /// ��ť�����¼�
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimePickerPopup_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Close();
        }
        /// <summary>
        /// ��ѡ������Ϊ��ǰʱ��
        /// </summary>
        private void SetCurrTime()
        {
            var now = DateTime.Now;

            var datePicker = this.FindControl<DatePicker>("DatePicker");
            datePicker.SelectedDate = now.Date;

            var timePicker = this.FindControl<TimePicker>("TimePicker");
            timePicker.SelectedTime = new TimeSpan(now.Hour, now.Minute, 0);
        }
        /// <summary>
        /// ���մ����������ʱ������
        /// </summary>
        private void SetTime()
        {
            var datePicker = this.FindControl<DatePicker>("DatePicker");
            var timePicker = this.FindControl<TimePicker>("TimePicker");
            var time = datePicker.SelectedDate!.Value.DateTime.Add((TimeSpan)(timePicker?.SelectedTime!));

            if (IsStartDt == true)
                MainWindow!.StartDt = time;
            else
                MainWindow!.EndDt = time;
        }
        /// <summary>
        /// ����
        /// </summary>
        /// <param name="mainWindow"></param>
        /// <param name="isStartDt"></param>
        public void SetParam(MainWindow mainWindow, bool isStartDt)
        {
            MainWindow = mainWindow;
            IsStartDt = isStartDt;
        }
    }
}
