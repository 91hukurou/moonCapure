using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MirroringCaptureApp
{
    public partial class ImageViewer : Window
    {
        //コードビハインド
        public static readonly RoutedCommand CopyCommand = new RoutedCommand(
            "CopyCommand", typeof(ImageViewer),
            new InputGestureCollection() { new KeyGesture(Key.C, ModifierKeys.Control) });

        public static readonly RoutedCommand PasteCommand = new RoutedCommand(
            "PasteCommandCommand", typeof(ImageViewer),
            new InputGestureCollection() { new KeyGesture(Key.V, ModifierKeys.Control) });


        public ImageViewer(BitmapSource bitmapSource, double left, double top)
        {
            InitializeComponent();
            CapturedImage.Source = bitmapSource;
            this.CommandBindings.Add(new CommandBinding(CopyCommand, CopyCommand_Execute));
            this.CommandBindings.Add(new CommandBinding(PasteCommand, PasteCommand_Execute));

            // Start location manually control
            this.WindowStartupLocation = WindowStartupLocation.Manual;

            //座標の設定
            this.Left = left;
            this.Top = top;

            // ウィンドウのサイズを画像のサイズに合わせる
            this.Width = bitmapSource.Width;
            this.Height = bitmapSource.Height;

            // Set this window to be focusable and set focus
            this.Focusable = true;
            this.Focus();

        }

        public ImageViewer(Stream imageStream)
        {
            InitializeComponent();

            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = imageStream;
            bitmapImage.EndInit();

            CapturedImage.Source = bitmapImage;

            // ウィンドウのサイズを画像のサイズに合わせる
            this.Width = bitmapImage.Width;
            this.Height = bitmapImage.Height;
        }

        // マウスボタンがおされたときのイベントハンドラ
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                // ウィンドウの透明度を半透明にします。
                this.Opacity = 0.5;
                // ウィンドウをドラッグします。
                this.DragMove();
                // ウィンドウの透明度を半透明にします。
                this.Opacity = 1;
            }

        }


        private void CloseMenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void SaveImageMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // ユーザーにファイルの名前と場所を選択させるためのSaveFileDialogを作成します。
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "Capture";
            dlg.DefaultExt = ".png";
            dlg.Filter = "PNG Files (*.png)|*.png";

            // ダイアログを表示する。
            bool? result = dlg.ShowDialog();

            // ダイアログが正しく処理され、ユーザーが「保存」を選んだ場合
            if (result == true)
            {
                // 選択されたファイル名を取得
                string filename = dlg.FileName;

                // 画像を保存
                SaveImageToFile(filename);
            }
        }

        private void SaveImageToFile(string filename)
        {
            // CapturedImage.SourceをBitmapSourceにキャストします。
            var bitmapSource = (BitmapSource)CapturedImage.Source;

            // ファイルとして画像を保存します。
            using (var fileStream = new FileStream(filename, FileMode.Create))
            {
                BitmapEncoder encoder = new PngBitmapEncoder(); // または他のエンコーダーを使用します。
                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                encoder.Save(fileStream);
            }
        }

        private void SaveAsJpegMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "Capture";
            dlg.DefaultExt = ".jpeg";
            dlg.Filter = "JPEG Files (*.jpeg)|*.jpeg";

            bool? result = dlg.ShowDialog();
            if (result == true)
            {
                string filename = dlg.FileName;
                SaveImageToFileAsJpeg(filename);
            }
        }

        private void SaveImageToFileAsJpeg(string filename)
        {
            var bitmapSource = (BitmapSource)CapturedImage.Source;

            using (var fileStream = new FileStream(filename, FileMode.Create))
            {
                BitmapEncoder encoder = new JpegBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                encoder.Save(fileStream);
            }
        }

        private void CopyCommand_Click(object sender, RoutedEventArgs e)
        {
            if (CapturedImage.Source != null)
            {
                Clipboard.SetImage((BitmapSource)CapturedImage.Source);
            }

        }

        private void CopyCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            if (CapturedImage.Source != null)
            {
                Clipboard.SetImage((BitmapSource)CapturedImage.Source);
            }
        }


        private void PasteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (Clipboard.ContainsImage())
            {
                BitmapSource pastedImage = Clipboard.GetImage();
                ImageViewer newImageViewer = new ImageViewer(pastedImage, this.Left, this.Top);
                newImageViewer.Show();
            }
        }

        private void PasteCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            if (Clipboard.ContainsImage())
            {
                BitmapSource pastedImage = Clipboard.GetImage();
                ImageViewer newImageViewer = new ImageViewer(pastedImage, this.Left, this.Top);
                newImageViewer.Show();
            }
        }

        private void RotateImageMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (CapturedImage.Source != null)
            {
                // Get the current BitmapSource
                BitmapSource source = (BitmapSource)CapturedImage.Source;

                // Create a TransformedBitmap to hold the rotation
                TransformedBitmap rotatedBitmap = new TransformedBitmap();
                rotatedBitmap.BeginInit();
                rotatedBitmap.Source = source;

                // Create a rotation transformation
                RotateTransform rotate = new RotateTransform(90);
                rotatedBitmap.Transform = rotate;
                rotatedBitmap.EndInit();

                // Set the rotated image as the new source
                CapturedImage.Source = rotatedBitmap;

                // Update the window size to fit the new image dimensions
                this.Width = rotatedBitmap.Width;
                this.Height = rotatedBitmap.Height;
            }

        }

        private void FlipImageMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (CapturedImage.Source != null)
            {
                // Get the current BitmapSource
                BitmapSource source = (BitmapSource)CapturedImage.Source;

                // Create a TransformedBitmap to hold the flipping
                TransformedBitmap flippedBitmap = new TransformedBitmap();
                flippedBitmap.BeginInit();
                flippedBitmap.Source = source;

                // Create a scale transformation to flip horizontally (use -1,1 for vertical flip)
                ScaleTransform flip = new ScaleTransform(-1, 1);
                flippedBitmap.Transform = flip;
                flippedBitmap.EndInit();

                // Set the flipped image as the new source
                CapturedImage.Source = flippedBitmap;

                // Update the window size to fit the new image dimensions
                this.Width = flippedBitmap.Width;
                this.Height = flippedBitmap.Height;
            }
        }

        //以下サムネ表示の設定
        private bool _isThumbnailMode = false;

        private void Image_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ToggleThumbnailMode();
        }

        private void ToggleThumbnailMode()
        {
            if (_isThumbnailMode)
            {
                // Code to return to the original size
                this.Width = ((BitmapSource)CapturedImage.Source).PixelWidth;
                this.Height = ((BitmapSource)CapturedImage.Source).PixelHeight;
                _isThumbnailMode = false;
            }
            else
            {
                // Code to resize to thumbnail
                this.Width = 128;
                this.Height = 128;
                _isThumbnailMode = true;
            }
        }


    }
}