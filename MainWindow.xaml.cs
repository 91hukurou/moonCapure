using Microsoft.Win32;
using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace MirroringCaptureApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        //ホットキーの設定
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private const int HOTKEY_ID = 9000;  // 任意の一意なID

        // タスクトレイにアプリが格納されているかを示すプロパティを追加
        private bool IsInTray { get; set; } = false;

        // ユーザーが設定したホットキーを保持するフィールド
        private Key mainKey = Key.None; // ホットキーの通常キー部分
        private ModifierKeys modifierKeys = ModifierKeys.None; // 修飾キー部分

        //タスクトレイ管理マネージャ
        private TrayManager trayManager;


        //初期化処理
        public MainWindow()
        {
            InitializeComponent();

            // 初期設定メソッドの呼び出し
            SettingsInitializer.InitializeHotkeySettings(this);
            // TrayManagerのインスタンス化
            trayManager = new TrayManager(this);

            // Loaded イベントにハンドラを追加
            this.Loaded += MainWindow_Loaded;
            // Closing イベントにハンドラを追加
            this.Closing += MainWindow_Closing;


        }

        //初期ロード時にタスクトレイ格納にチェックであればメインウインドウを表示させずに格納起動する
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            trayManager.initialLoadMinimizeToTray();
        }

        // 閉じるボタンを押したとき、タスク格納にチェックならアプリを停止せずに格納する
        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            trayManager.endProcessingCloseMinimizeToTray(sender,e);
        }


        //グローバルホットキーの登録とか
        private bool RegisterGlobalHotkey()
        {
            int modifiers = ConvertModifierKeys(modifierKeys);  // 修飾キー
            int key = KeyInterop.VirtualKeyFromKey(mainKey);  // 'C'キー
            //確認
            bool flag = RegisterHotKey(new WindowInteropHelper(this).Handle, HOTKEY_ID, modifiers, key);

            //ホットキーの登録が成功したらローカルにも保存する。
            if (flag)
            {
                // アプリ終了前に設定を保存する
                Properties.Settings.Default.ModifiersData = modifiers;
                Properties.Settings.Default.KeyData = key;
                Properties.Settings.Default.Save();

            }
            return flag;
        }

        private void UnregisterGlobalHotkey()
        {
            UnregisterHotKey(new WindowInteropHelper(this).Handle, HOTKEY_ID);
        }

        //グローバルホットキーが押されたときに動作
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            HwndSource source = PresentationSource.FromVisual(this) as HwndSource;
            source.AddHook(WndProc);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            // メッセージIDが0x0312の時、ホットキーが押されたと判断
            if (msg == 0x0312)
            {
                // MessageTextBlock がフォーカスされているかチェック
                if ((FocusManager.GetFocusedElement(this) == HotkeyInput) || !(bool)HotkeyCheckBox.IsChecked)
                {
                    // HotkeyInput がフォーカスされているので何もしない
                }
                else
                {
                    // キャプチャー実行
                    CaptureButton_Click(this, new RoutedEventArgs());
                }
            } else
            {
                //初期化時にホットキーの登録をしたいが初期化メソッドではハンドルが間に合っていないので暫定的にここにいれる
                if (((bool)HotkeyCheckBox.IsChecked) && (HotkeyInput.Text != ""))
                {
                    int modifiers = Properties.Settings.Default.ModifiersData;
                    int key = Properties.Settings.Default.KeyData;
                    //実行と確認
                    bool flag = RegisterHotKey(new WindowInteropHelper(this).Handle, HOTKEY_ID, modifiers, key);
                    Console.Write(flag);

                }
            }
            return IntPtr.Zero;
        }
        private void StartUpCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            SetStartup(true);
        }

        private void StartUpCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            SetStartup(false);
        }

        private void SetStartup(bool add)
        {
            // レジストリのキーを取得
            RegistryKey rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            // アプリケーションの名前
            string appName = "moon";

            if (add)
            {
                // アプリケーションのパス
                string appPath = Environment.GetCommandLineArgs()[0];
                // スタートアップにアプリを追加
                rkApp.SetValue(appName, $"\"{appPath}\""); // 引用符を追加して、パスにスペースが含まれていても正しく動作するようにします。
            }
            else
            {
                // スタートアップからアプリを削除
                rkApp.DeleteValue(appName, false);
            }
        }


        private void CaptureButton_Click(object sender, RoutedEventArgs e)
        {
            CaptureWindow captureWindow = new CaptureWindow();
            this.Visibility = Visibility.Hidden; // メインウィンドウを非表示にします。
            captureWindow.ShowDialog();
            this.Visibility = Visibility.Visible; // キャプチャーが終わったら再度メインウィンドウを表示します。
        }


        //ホットキー設定画面で文字入力を検知
        private void HotkeyInput_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {

            HotkeyManager hotkeyManager = new HotkeyManager();

            //初期化処理　全ホットキーの解除
            UnregisterGlobalHotkey();
            //メッセージボックスのリセット
            MessageTextBlock.Text = "";


            // キー入力をハンドル済みとしてマーク
            e.Handled = true;

            // 修飾キーを判定
            modifierKeys = Keyboard.Modifiers;

            // 修飾キーをテキストボックスに表示
            string hotkeyText = hotkeyManager.getModifierKeysText(modifierKeys);

            // 通常キーを判定
            mainKey = e.SystemKey == Key.None ? e.Key : e.SystemKey;

            // 通常キーが修飾キーでないかチェックし、テキストボックスにキー入力を表示
            if(!hotkeyManager.isModifierKey(mainKey))
{
                // 通常キーが修飾キーでない場合の処理
                HotkeyInput.Text = hotkeyText + hotkeyManager.getKeyString(mainKey);
            }
            else
            {
                // 通常キーが修飾キーの場合の処理
                // 修飾キーだけの入力ならメッセージを表示
                if (modifierKeys != ModifierKeys.None && string.IsNullOrEmpty(hotkeyText.Trim()))
                {
                    MessageTextBlock.Text = mainKey + "(修飾キー)だけではホットキーに登録できません";
                    HotkeyInput.Text = "";
                    mainKey = Key.None;
                    modifierKeys = ModifierKeys.None;
                }
                else
                {
                    HotkeyInput.Text = hotkeyText.TrimEnd(new char[] { ' ', '+' });
                }
                return;
            }

            // ホットキーの再登録
            if (!RegisterGlobalHotkey())
            {
                // ホットキーの登録に失敗した場合の処理を記述
                MessageTextBlock.Text = "既に同じホットキーが使用されて使えません";
                HotkeyInput.Text = "";  // エラーが発生したら入力フィールドをクリアする
                mainKey = Key.None;  // 登録に失敗したらキーをリセット
                modifierKeys = ModifierKeys.None;  // 登録に失敗したら修飾キーもリセット
            }

        }

        //アプリケーションが終了するとき、登録されたホットキーを解除しましょう。これを怠るとシステムにハンドラが残ってしまう可能性があります。
        protected override void OnClosed(EventArgs e)
        {
            UnregisterGlobalHotkey();
            // アプリ終了前に設定を保存する
            Properties.Settings.Default.HotkeySetting = HotkeyInput.Text;
            Properties.Settings.Default.CheckBoxState = HotkeyCheckBox.IsChecked.GetValueOrDefault();
            Properties.Settings.Default.taskChData = RbMinimizeCheck.IsChecked.GetValueOrDefault();
            Properties.Settings.Default.StartUpFlag = StartUpCheckBox.IsChecked.GetValueOrDefault();
            Properties.Settings.Default.Save();

            base.OnClosed(e);
        }

        private void MenuItemOpen_Click(object sender, RoutedEventArgs e)
        {
            // タスクバーにアプリを表示
            ShowInTaskbar = true;
            //タスクトレイにアプリを非表示
            MyNotifyIcon.Visibility = Visibility.Collapsed;
            WindowState = WindowState.Normal;
            Activate(); // bring to foreground
        }

        
        //タスクトレイから終了でアプリを落とす時のメモリ解放処理
        private void MenuItemExit_Click(object sender, RoutedEventArgs e)
        {
            MyNotifyIcon.Dispose(); // Always dispose of the notify icon to free resources
            System.Windows.Application.Current.Shutdown();
        }


        //最小化/最大化などのウィンドウの状態が変化したときに発火する
        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);
            //最小化
            if (this.WindowState == WindowState.Minimized)
            {

                if (!(bool)RbMinimizeCheck.IsChecked)
                {
                    //タスクトレイにアプリを表示
                    MyNotifyIcon.Visibility = Visibility.Collapsed;
                    //タスクバーにアプリを非表示
                    this.ShowInTaskbar = true;
                    IsInTray = false;

                } else
                {
                    //タスクトレイにアプリを表示
                    MyNotifyIcon.Visibility = Visibility.Visible;
                    //タスクバーにアプリを非表示
                    this.ShowInTaskbar = false;
                    IsInTray = true;
                }
            }
            else if (this.WindowState == WindowState.Normal)
            {
                //タスクトレイにアプリを非表示
                MyNotifyIcon.Visibility = Visibility.Collapsed;
                //タスクバーにアプリを表示
                this.ShowInTaskbar = true;
                IsInTray = false;
            }
        }

        private int ConvertModifierKeys(ModifierKeys modifier)
        {
            int mod = 0;
            if ((modifier & ModifierKeys.Alt) > 0) mod |= 0x1; // ALT
            if ((modifier & ModifierKeys.Control) > 0) mod |= 0x2; // CTRL
            if ((modifier & ModifierKeys.Shift) > 0) mod |= 0x4; // SHIFT
                                                                 // 必要に応じて他の修飾キーも追加
            return mod;
        }

        //Hotkeyを活性・非活性にするチェックボックスの設定
        private void HotkeyCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            HotkeyInput.IsEnabled = false;
        }

        //Hotkeyを活性・非活性にするチェックボックスの設定
        private void HotkeyCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            HotkeyInput.IsEnabled = true;
        }

    }
}
