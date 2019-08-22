using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media.Imaging;

namespace OctTools
{
    public partial class RecordMarkImage : Image
    {
        public RecordMarkImage()
        {
            Thickness thick = new Thickness(700, 0, 0, 0);
            Margin = thick;
            Uri uri = new Uri(@"Pict/DownArrow02.ico", UriKind.Relative);
            Source = new BitmapImage(uri);
            Stretch = Stretch.Uniform;
            HorizontalAlignment = HorizontalAlignment.Left;
            VerticalAlignment = VerticalAlignment.Top;
        }
    }
}
