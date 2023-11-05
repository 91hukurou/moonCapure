using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MirroringCaptureApp
{
    public static class SettingsInitializer
    {
        public static void InitializeHotkeySettings(MainWindow mainWindow)
        {
            // ホットキー関連の設定
            //アプリ起動時にホットキーが有効なら有効の設定を適用する
            mainWindow.HotkeyCheckBox.IsChecked = Properties.Settings.Default.CheckBoxState;
            mainWindow.RbMinimizeCheck.IsChecked = Properties.Settings.Default.taskChData;
            mainWindow.StartUpCheckBox.IsChecked = Properties.Settings.Default.StartUpFlag;
            mainWindow.HotkeyInput.Text = Properties.Settings.Default.HotkeySetting;

        }

    }

}
