using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AdvanceLogReader.Popups
{
    public partial class TimePickerPopup : Window
    {
        /// <summary>
        /// 父级的主界面
        /// </summary>
        private MainWindow? MainWindow;
        /// <summary>
        /// 是否设定的为起始时间
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
            //绑定按钮事件
            this.FindControl<Button>("Btn_Commit").Click += TimePickerPopup_Click;
            Closing += TimePickerPopup_Closing;
        }
        /// <summary>
        /// 关闭时触发事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimePickerPopup_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            SetTime();
        }
        /// <summary>
        /// 按钮触发事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimePickerPopup_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Close();
        }
        /// <summary>
        /// 将选择器置为当前时间
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
        /// 按照传入参数进行时间设置
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
        /// 传参
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
