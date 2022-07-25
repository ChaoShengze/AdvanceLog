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
            //����UI
            UpdateUI();
            //���¼�
            BindingEvents();
        }

#if DEBUG
        /// <summary>
        /// ���ɲ�������
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

        #region ��������
        /// <summary>
        /// ����������������ʼʱ��
        /// </summary>
        public DateTime? StartDt = null;
        /// <summary>
        /// ������������������ʱ��
        /// </summary>
        public DateTime? EndDt = null;
        /// <summary>
        /// ��Ҫ��ѯ��Ŀ��ģ��
        /// </summary>
        private string[]? TargetModules = null;
        /// <summary>
        /// ��Ҫ��ѯ��Ŀ������
        /// </summary>
        private LogType[]? TargetLogTypes = null;
        /// <summary>
        /// �Ƿ�Ϊ�ı�����״̬
        /// </summary>
        private bool FindTextMode = false;
        #endregion

        #region ����
        /// <summary>
        /// ����UI����
        /// </summary>
        private void UpdateUI()
        {
            var log = AdvanceLog.GetInstance();
            var list = new List<object>() { new ComboBoxItem() { Content = "ȫ��ģ��" } };

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
        /// UI������¼�
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
        /// ������ĿLabel
        /// </summary>
        private void UpdateStatusLabel()
        {
            var dataGrid = this.FindControl<DataGrid>("DataGrid");
            var logs = dataGrid.Items as List<LogRecord>;
            this.FindControl<Label>("Label_Status").Content = $"��Ŀ����{logs?.Count} ��";
        }
        /// <summary>
        /// ����ѡ������������������
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

                ///��λUI���
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
        /// ��������ѡ��ť�Ŀ�����
        /// </summary>
        /// <param name="status"></param>
        private void UpdateTimePickerBtns(bool status)
        {
            var btnStartDt = this.FindControl<Button>("Btn_StartDt");
            var btnEndDt = this.FindControl<Button>("Btn_EndDt");
            btnStartDt.IsEnabled = btnEndDt.IsEnabled = status;
        }
        #endregion

        #region UI���¼�
        /// <summary>
        /// ����Label����¼�
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
                Title = "����ѡ����־��Ŀ",
                Directory = Path.Combine("."),
                InitialFileName = $"{DateTime.Now:yyyyMMddHHmmssfff}.txt"
            };
            var path = await sfd.ShowAsync(this);

            if (!string.IsNullOrEmpty(path))
                File.WriteAllText(path, text);
        }
        /// <summary>
        /// ���Label����¼�
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
        /// DataGrid ѡ����ı�ʱ��ˢ���ı����е�����
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

            tbx.Text += $"����ʱ�䣺{log?.DateTime:yyyy-MM-dd HH:mm:ss}{Environment.NewLine}";
            tbx.Text += $"�¼�����{log?.LogType}{Environment.NewLine}";
            tbx.Text += $"ģ�����ƣ�{log?.ModuleName}{Environment.NewLine}";
            tbx.Text += $"�������ƣ�{log?.FuncName}{Environment.NewLine}";
            tbx.Text += $"��־���ݣ�{Environment.NewLine}{log?.Desc}";
        }
        /// <summary>
        /// ����������ѡ���������¼�
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
        /// ��ȡ�����޶�����
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Submit_Datetime()
        {
            var comboBox = this.FindControl<ComboBox>("Combx_Datetime");
            var now = DateTime.Now;

            switch (comboBox.SelectedIndex)
            {
                case 0: //��������
                    StartDt = EndDt = null;
                    break;
                case 1: //����
                    StartDt = now.Date;
                    EndDt = now.Date.AddDays(1);
                    break;
                case 2: //����
                    StartDt = now.Date.AddDays(-1);
                    EndDt = now.Date;
                    break;
                case 3: //������
                    StartDt = now.Date.AddDays(-2);
                    EndDt = now.Date.AddDays(1);
                    break;
                case 4: //һ��
                    StartDt = now.Date.AddDays(-6);
                    EndDt = now.Date.AddDays(1);
                    break;
                case 5: //�Զ�������
                    UpdateTimePickerBtns(true);
                    return;
            }
        }
        /// <summary>
        /// ��ȡģ���޶�����
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
        /// ��ȡ�����޶�����
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
        /// ��ѯ��ť����¼�
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
        /// ����ʱ�䰴ť����¼�
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
        /// ��ʼ��ť����¼�
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
        /// ����Label����¼�
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void Label_riselimit_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            var log = AdvanceLog.GetInstance();
            log.QueryCountLimit += 1000;

            var label_limit = this.FindControl<Label>("Label_Limit");
            label_limit.Content = $"��ģ����Ŀ���ƣ�{log.QueryCountLimit}";
        }
        /// <summary>
        /// ��ѯ�ı�Label����¼�
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
            tbx.Text = "�ڴ�����Ҫ�������ı�";
            tbx.SelectAll();
            tbx.Focus();

            FindTextMode = true;
        }
        #endregion
    }
}
