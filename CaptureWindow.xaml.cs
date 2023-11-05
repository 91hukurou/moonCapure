using System;
using System.Windows;
using System.Windows.Input;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;
using System.Threading.Tasks;

namespace MirroringCaptureApp
{
    public partial class CaptureWindow : Window
    {
        System.Windows.Point startPoint;
        System.Windows.Shapes.Rectangle selectionRectangle;

        public CaptureWindow()
        {
            InitializeComponent();

            // マルチモニター全体をカバーするようにウィンドウのサイズと位置を設定
            this.Left = System.Windows.SystemParameters.VirtualScreenLeft;
            this.Top = System.Windows.SystemParameters.VirtualScreenTop;
            this.Width = System.Windows.SystemParameters.VirtualScreenWidth;
            this.Height = System.Windows.SystemParameters.VirtualScreenHeight;

            // カーソルを十字に設定
            this.Cursor = Cursors.Cross;

        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            startPoint = e.GetPosition(Canvas);

            selectionRectangle = new System.Windows.Shapes.Rectangle
            {
                Stroke = System.Windows.Media.Brushes.LightBlue,
                //ドラッグ範囲の線の細さ
                StrokeThickness = 0.5,
                //ドラッグ範囲の色付け
                Fill = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromArgb(50, 0, 0, 255)) // A(透明度), R, G, B
            };
            System.Windows.Controls.Canvas.SetLeft(selectionRectangle, startPoint.X);
            System.Windows.Controls.Canvas.SetTop(selectionRectangle, startPoint.Y);
            Canvas.Children.Add(selectionRectangle);
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                System.Windows.Point endPoint = e.GetPosition(Canvas);

                var width = Math.Abs(endPoint.X - startPoint.X);
                var height = Math.Abs(endPoint.Y - startPoint.Y);
                var left = Math.Min(startPoint.X, endPoint.X);
                var top = Math.Min(startPoint.Y, endPoint.Y);

                selectionRectangle.Width = width;
                selectionRectangle.Height = height;

                System.Windows.Controls.Canvas.SetLeft(selectionRectangle, left);
                System.Windows.Controls.Canvas.SetTop(selectionRectangle, top);
            }
        }

        private async void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (selectionRectangle != null)
            {
                System.Windows.Point endPoint = e.GetPosition(Canvas);

                //左上を0,0基準としてx軸、y軸そしてそのポイントから横幅と高さで領域を定義する。
                int x = (int)Math.Min(startPoint.X, endPoint.X);
                int y = (int)Math.Min(startPoint.Y, endPoint.Y);
                int width = (int)Math.Abs(endPoint.X - startPoint.X);
                int height = (int)Math.Abs(endPoint.Y - startPoint.Y);


                // 透明なブラシに設定し、選択範囲の色をクリアする。
                selectionRectangle.Fill = null;

                // キャプチャを実行する前に、描画の更新を行うために
                // UI スレッドに対して描画処理を強制的に実行させる。 
                this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Render, (Action)(() => { }));
                await Task.Delay(100);  // ここで100ms（または適当な時間）待機します。

                CaptureArea(x, y, width, height);

                // selectionRectangleをCanvasから削除して、nullに設定する。
                Canvas.Children.Remove(selectionRectangle);
                selectionRectangle = null;
                // メソッドの最後でカーソルをデフォルトに戻す
                this.Cursor = Cursors.Arrow;
                this.Close();
            }
        }

        private void CaptureArea(int x, int y, int width, int height)
        {
            if (selectionRectangle != null)
            {

                // selectionRectangleの色をクリアします。
                selectionRectangle.Fill = null;

                // selectionRectangleをCanvasから完全に削除します。
                Canvas.Children.Remove(selectionRectangle);

                // 少し待つことで画面更新を確実にします。適切なウェイトタイムを設定します（ミリ秒単位）。
                System.Threading.Thread.Sleep(100);
            }

            // ウィンドウ座標をスクリーン座標に変換
            System.Windows.Point screenPoint = ConvertToScreenCoordinates(new System.Windows.Point(x, y));
            int screenX = (int)screenPoint.X;
            int screenY = (int)screenPoint.Y;

            // キャプチャ領域がVirtual Screenの範囲外になることを防ぐためのチェック
            screenX = Math.Max((int)System.Windows.SystemParameters.VirtualScreenLeft, screenX);
            screenY = Math.Max((int)System.Windows.SystemParameters.VirtualScreenTop, screenY);
            width = Math.Min((int)System.Windows.SystemParameters.VirtualScreenWidth - screenX, width);
            height = Math.Min((int)System.Windows.SystemParameters.VirtualScreenHeight - screenY, height);


            using (var bitmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            {
                using (var graphics = Graphics.FromImage(bitmap))
                {
                    // スクリーン座標を使用してキャプチャ
                    graphics.CopyFromScreen(screenX, screenY, 0, 0, new System.Drawing.Size(width, height), CopyPixelOperation.SourceCopy);
                }

//               キャプチャーしたときはローカルで保存する
//                bitmap.Save("screenshot.png", ImageFormat.Png);

                // bitmapをメモリストリームに変換し、BitmapImageを生成します。
                using (MemoryStream memory = new MemoryStream())
                {
                    bitmap.Save(memory, ImageFormat.Png);
                    memory.Position = 0;

                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = memory;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();
                    bitmapImage.Freeze();

                    // 画像を表示する新しいウィンドウを開きます。
                    ImageViewer imageViewer = new ImageViewer(bitmapImage, (int)screenX, (int)screenY);
                    imageViewer.Show();
                }

            }

            // selectionRectangleがnullでないことを確認し、再表示します。
            if (selectionRectangle != null)
            {
                selectionRectangle.Visibility = Visibility.Visible;
            }
        }

        //キャプチャ範囲の座標をウィンドウ基準からスクリーン基準に変換するメソッド

        private System.Windows.Point ConvertToScreenCoordinates(System.Windows.Point windowPoint)
        {
            // 現在のウィンドウの位置を取得
            System.Windows.Point windowLocation = new System.Windows.Point(this.Left, this.Top);

            // ウィンドウ座標をスクリーン座標に変換
            return new System.Windows.Point(windowPoint.X + windowLocation.X, windowPoint.Y + windowLocation.Y);
        }


    }
}