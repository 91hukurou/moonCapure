using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MirroringCaptureApp
{
    public class TrayManager
    {
        private Window mainWindow;

        public TrayManager(Window mainWindow)
        {
            this.mainWindow = mainWindow;
        }

        public void MinimizeToTray()
        {
            // ウィンドウの最小化ロジック
            mainWindow.WindowState = WindowState.Minimized;
            // タスクトレイアイコンの表示など、必要に応じて追加
        }

        public void RestoreFromTray()
        {
            // ウィンドウの復元ロジック
            mainWindow.WindowState = WindowState.Normal;
            // タスクトレイアイコンの非表示など、必要に応じて追加
        }

        public void initialLoadMinimizeToTray()
        {
            if (shouldMinimizeToTray())
            {
                windowMinimizationProcessing();

            }
        }

        // 必要に応じて他のメソッドを追加
        
              public void endProcessingCloseMinimizeToTray(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (shouldMinimizeToTray())
            {
                e.Cancel = true; // 終了をキャンセル
                windowMinimizationProcessing();
            }

        }

        private bool shouldMinimizeToTray()
        {
            //タスクトレイ格納するかどうかの状態管理変数
             return Properties.Settings.Default.taskChData;
        }

        private void windowMinimizationProcessing()
        {
            mainWindow.WindowState = WindowState.Minimized; // ウィンドウを最小化
        }

    }

}
