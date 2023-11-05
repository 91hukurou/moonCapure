using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MirroringCaptureApp
{

    public class HotkeyManager
    {

        public HotkeyManager()
        {
        }

        public string getModifierKeysText(ModifierKeys modifierKeys)
        {
            string hotkeyText = "";
            if ((modifierKeys & ModifierKeys.Control) > 0) hotkeyText += "Ctrl + ";
            if ((modifierKeys & ModifierKeys.Shift) > 0) hotkeyText += "Shift + ";
            if ((modifierKeys & ModifierKeys.Alt) > 0) hotkeyText += "Alt + ";
            return hotkeyText;
        }

        public bool isModifierKey(Key key)
        {
            return key == Key.LeftCtrl || key == Key.RightCtrl ||
                   key == Key.LeftShift || key == Key.RightShift ||
                   key == Key.LeftAlt || key == Key.RightAlt ||
                   key == Key.System || key == Key.LWin || key == Key.RWin;
        }

        public string getKeyString(Key key)
        {
            if (key >= Key.D0 && key <= Key.D9)
            {
                // 数字キーの場合、先頭の "D" を取り除く
                return key.ToString().Substring(1);
            }

            // その他のキーの場合は通常通り文字列に変換
            return key.ToString();
        }


    }
}
