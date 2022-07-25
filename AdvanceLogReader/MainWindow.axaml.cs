using System;
using System.IO;
using Avalonia;
using Avalonia.Media;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using Avalonia.Collections;
using System.Collections.Generic;

using AdvanceLogLib;
using AdvanceLogReader.Popups;

namespace AdvanceLogReader
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            //this.AttachDevTools();
            //CreateTestData();
#endif
            //更新UI
            UpdateUI();
            //绑定事件
            BindingEvents();
        }

#if DEBUG
        /// <summary>
        /// 生成测试数据
        /// </summary>
        //private void CreateTestData()
        //{
        //    var log = AdvanceLog.GetInstance();
        //    var names = new string[] { "Module1", "Module2", "Module3", "Module4", "Module5" };
        //    foreach (var name in names)
        //    {
        //        var random = new Random().Next(0, 12);
        //        var level = random < 9
        //            ? random < 6
        //                ? random < 3
        //                    ? LogType.DEBUG
        //                    : LogType.ERROR
        //                : LogType.WARN
        //            : LogType.INFO;

        //        for (int i = 0; i < 30; i++)
        //            log.WriteLog(level, name, "TestFunc", $"Test-{DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        //    }
        //}
#endif

        #region 变量定义
        /// <summary>
        /// 日期限制条件，起始时间
        /// </summary>
        public DateTime? StartDt = null;
        /// <summary>
        /// 日期限制条件，结束时间
        /// </summary>
        public DateTime? EndDt = null;
        /// <summary>
        /// 需要查询的目标模块
        /// </summary>
        private string[]? TargetModules = null;
        /// <summary>
        /// 需要查询的目标类型
        /// </summary>
        private LogType[]? TargetLogTypes = null;
        /// <summary>
        /// 是否为文本检索状态
        /// </summary>
        private bool FindTextMode = false;
        #endregion

        #region 方法
        /// <summary>
        /// 更新UI界面
        /// </summary>
        private void UpdateUI()
        {
            var log = AdvanceLog.GetInstance();
            var list = new List<object>() { new ComboBoxItem() { Content = "全部模块" } };

            var combxModule = this.FindControl<ComboBox>("Combx_Module");
            var modules = log.GetAllModules();

            if (modules == null)
                return;

            foreach (var module in modules)
                list.Add(new CheckBox() { Content = module, IsChecked = true });
            combxModule.Items = list;

            if (list.Count >= 1)
                combxModule.SelectedIndex = 0;

            var dataGrid = this.FindControl<DataGrid>("DataGrid");
            var logs = log.LoadLog(modules);

            if (logs == null)
                return;

            dataGrid.Items = logs;
            if (logs.Count >= 1)
                dataGrid.ScrollIntoView(logs[0], null);

            UpdateStatusLabel();
        }
        /// <summary>
        /// UI组件绑定事件
        /// </summary>
        private void BindingEvents()
        {
            var dataGrid = this.FindControl<DataGrid>("DataGrid");
            dataGrid.SelectionChanged += DataGrid_SelectionChanged;

            var label_clear = this.FindControl<Label>("Label_Clear");
            label_clear.PointerPressed += Label_Clear_PointerPressed;

            var label_export = this.FindControl<Label>("Label_Export");
            label_export.PointerPressed += Label_Export_PointerPressed;

            var label_riselimit = this.FindControl<Label>("Label_RiseLimit");
            label_riselimit.PointerPressed += Label_riselimit_PointerPressed;

            var label_findtext = this.FindControl<Label>("Label_FindText");
            label_findtext.PointerPressed += Label_findtext_PointerPressed;

            var combx_datetime = this.FindControl<ComboBox>("Combx_Datetime");
            combx_datetime.SelectionChanged += Combx_datetime_SelectionChanged; ;

            var btn_submit = this.FindControl<Button>("Btn_Submit");
            btn_submit.Click += Btn_submit_Click;

            var btn_startdt = this.FindControl<Button>("Btn_StartDt");
            btn_startdt.Click += Btn_startdt_Click;

            var btn_enddt = this.FindControl<Button>("Btn_EndDt");
            btn_enddt.Click += Btn_enddt_Click;
        }
        /// <summary>
        /// 更新条目Label
        /// </summary>
        private void UpdateStatusLabel()
        {
            var dataGrid = this.FindControl<DataGrid>("DataGrid");
            var logs = dataGrid.Items as List<LogRecord>;
            this.FindControl<Label>("Label_Status").Content = $"条目数：{logs?.Count} 条";
        }
        /// <summary>
        /// 根据选择条件更新数据内容
        /// </summary>
        private void UpdateDataWithCondition()
        {
            var dataGrid = this.FindControl<DataGrid>("DataGrid");

            if (!FindTextMode)
            {
                dataGrid.Items = null;
                dataGrid.ScrollIntoView(null, null);

                var log = AdvanceLog.GetInstance();
                var list = new List<LogRecord>();

                if (TargetModules == null)
                    TargetModules = log.GetAllModules();

                var condStartDt = StartDt == null;
                var condEndDt = EndDt == null;
                var condTargetLogTypes = TargetLogTypes == null;

                if (condStartDt && condEndDt)
                {
                    if (condTargetLogTypes)
                        list = log.LoadLog(log.GetAllModules()!);
                    else
                        foreach (var logType in TargetLogTypes!)
                            list.AddRange(log.LoadLog(DateTime.MinValue, DateTime.MaxValue, TargetModules!, logType)!);
                }
                else if (!condStartDt && !condEndDt)
                {
                    if (condTargetLogTypes)
                    {
                        list = log.LoadLog((DateTime)StartDt!, (DateTime)EndDt!, TargetModules!);
                    }
                    else
                    {
                        foreach (var logType in TargetLogTypes!)
                            list.AddRange(log.LoadLog((DateTime)StartDt!, (DateTime)EndDt!, TargetModules!, logType)!);
                    }
                }

                if (list?.Count == 0)
                    return;

                this.FindControl<TextBox>("Tbx_Desc").IsReadOnly = true;

                dataGrid.Items = list;
                dataGrid.ScrollIntoView(list?[0], null);

                UpdateStatusLabel();
            }
            else
            {
                if (dataGrid.Items == null || (dataGrid.Items as List<LogRecord>)!.Count == 0)
                    return;

                var tbx = this.FindControl<TextBox>("Tbx_Desc");
                if (string.IsNullOrEmpty(tbx.Text))
                    return;

                var _list = new List<LogRecord>();
                foreach (LogRecord item in dataGrid.Items)
                    if (item.Desc?.IndexOf(tbx.Text) != -1)
                        _list.Add(item);

                ///复位UI组件
                var label_findtext = this.FindControl<Label>("Label_FindText");
                label_findtext.Foreground = new SolidColorBrush(Colors.Blue);
                tbx.IsReadOnly = true;
                FindTextMode = false;
                tbx.Text = "";

                if (_list.Count == 0)
                {
                    dataGrid.Items = null;
                    dataGrid.ScrollIntoView(null, null);
                    return;
                }

                dataGrid.Items = _list;
                dataGrid.ScrollIntoView(_list?[0], null);
            }
        }
        /// <summary>
        /// 更新日期选择按钮的可用性
        /// </summary>
        /// <param name="status"></param>
        private void UpdateTimePickerBtns(bool status)
        {
            var btnStartDt = this.FindControl<Button>("Btn_StartDt");
            var btnEndDt = this.FindControl<Button>("Btn_EndDt");
            btnStartDt.IsEnabled = btnEndDt.IsEnabled = status;
        }
        #endregion

        #region UI绑定事件
        /// <summary>
        /// 导出Label点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private async void Label_Export_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            var text = this.FindControl<TextBox>("Tbx_Desc").Text;

            var sfd = new SaveFileDialog
            {
                DefaultExtension = ".txt",
                Title = "导出选中日志条目",
                Directory = Path.Combine("."),
                InitialFileName = $"{DateTime.Now:yyyyMMddHHmmssfff}.txt"
            };
            var path = await sfd.ShowAsync(this);

            if (!string.IsNullOrEmpty(path))
                File.WriteAllText(path, text);
        }
        /// <summary>
        /// 清除Label点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void Label_Clear_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            this.FindControl<TextBox>("Tbx_Desc").Clear();
            this.FindControl<DataGrid>("DataGrid").Items = null;
        }
        /// <summary>
        /// DataGrid 选定项改变时，刷新文本框中的内容
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void DataGrid_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count < 1)
                return;
            var log = e.AddedItems[0] as LogRecord;
            var tbx = this.FindControl<TextBox>("Tbx_Desc");
            tbx.Clear();

            tbx.Text += $"日期时间：{log?.DateTime:yyyy-MM-dd HH:mm:ss}{Environment.NewLine}";
            tbx.Text += $"事件级别：{log?.LogType}{Environment.NewLine}";
            tbx.Text += $"模块名称：{log?.ModuleName}{Environment.NewLine}";
            tbx.Text += $"函数名称：{log?.FuncName}{Environment.NewLine}";
            tbx.Text += $"日志内容：{Environment.NewLine}{log?.Desc}";
        }
        /// <summary>
        /// 日期下拉框选项变更触发事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void Combx_datetime_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            var combx_datetime = this.FindControl<ComboBox>("Combx_Datetime");
            if (combx_datetime.SelectedIndex == 5)
            {
                UpdateTimePickerBtns(true);
            }
            else
            {
                UpdateTimePickerBtns(false);
                StartDt = EndDt = null;
            }
        }
        /// <summary>
        /// 获取日期限定条件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Submit_Datetime()
        {
            var comboBox = this.FindControl<ComboBox>("Combx_Datetime");
            var now = DateTime.Now;

            switch (comboBox.SelectedIndex)
            {
                case 0: //所有日期
                    StartDt = EndDt = null;
                    break;
                case 1: //当天
                    StartDt = now.Date;
                    EndDt = now.Date.AddDays(1);
                    break;
                case 2: //昨天
                    StartDt = now.Date.AddDays(-1);
                    EndDt = now.Date;
                    break;
                case 3: //近三天
                    StartDt = now.Date.AddDays(-2);
                    EndDt = now.Date.AddDays(1);
                    break;
                case 4: //一周
                    StartDt = now.Date.AddDays(-6);
                    EndDt = now.Date.AddDays(1);
                    break;
                case 5: //自定义日期
                    UpdateTimePickerBtns(true);
                    return;
            }
        }
        /// <summary>
        /// 获取模块限定条件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Submit_Module()
        {
            TargetModules = null;

            var combxModule = this.FindControl<ComboBox>("Combx_Module");

            var items = combxModule.Items as List<object>;
            var targets = new List<string>();
            for (int i = 1; i < items?.Count; i++)
            {
                var chkBx = (CheckBox)items[i];
                if (chkBx?.IsChecked == true)
                    targets.Add((string)chkBx.Content);
            }

            TargetModules = targets.ToArray();

            if (TargetModules.Length == 0)
                TargetModules = AdvanceLog.GetInstance().GetAllModules();
        }
        /// <summary>
        /// 获取级别限定条件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void Submit_Level()
        {
            TargetLogTypes = null;

            var combx_lv = this.FindControl<ComboBox>("Combx_Level");
            var items = combx_lv.Items as AvaloniaList<object>;
            var list = new List<LogType>();
            for (int i = 1; i < items?.Count; i++)
            {
                var chkBx = (CheckBox)items[i];
                if (chkBx.IsChecked == true)
                    list.Add((LogType)(i - 1));
            }

            TargetLogTypes = list.ToArray();

            if (TargetLogTypes.Length == 0)
                TargetLogTypes = new LogType[] { LogType.INFO, LogType.WARN, LogType.ERROR, LogType.DEBUG };
        }
        /// <summary>
        /// 查询按钮点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void Btn_submit_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Submit_Datetime();
            Submit_Module();
            Submit_Level();

            UpdateDataWithCondition();
        }
        /// <summary>
        /// 结束时间按钮点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private async void Btn_enddt_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var timepicker = new TimePickerPopup();
            timepicker.SetParam(this, false);
            await timepicker.ShowDialog(this);

            if (EndDt != null)
            {
                var button = (Button)sender!;
                button.FontSize = 11;
                button.Content = EndDt?.ToString("yyyy/MM/dd HH:mm:ss");
            }
        }
        /// <summary>
        /// 开始按钮点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private async void Btn_startdt_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var timepicker = new TimePickerPopup();
            timepicker.SetParam(this, true);
            await timepicker.ShowDialog(this);

            if (StartDt != null)
            {
                var button = (Button)sender!;
                button.FontSize = 11;
                button.Content = StartDt?.ToString("yyyy/MM/dd HH:mm:ss");
            }
        }
        /// <summary>
        /// 扩限Label点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void Label_riselimit_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            var log = AdvanceLog.GetInstance();
            log.QueryCountLimit += 1000;

            var label_limit = this.FindControl<Label>("Label_Limit");
            label_limit.Content = $"单模块条目限制：{log.QueryCountLimit}";
        }
        /// <summary>
        /// 查询文本Label点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void Label_findtext_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            var label_findtext = this.FindControl<Label>("Label_FindText");
            label_findtext.Foreground = new SolidColorBrush(Colors.Red);

            var tbx = this.FindControl<TextBox>("Tbx_Desc");
            tbx.IsReadOnly = false;
            tbx.Text = "在此输入要检索的文本";
            tbx.SelectAll();
            tbx.Focus();

            FindTextMode = true;
        }
        #endregion
    }
}
