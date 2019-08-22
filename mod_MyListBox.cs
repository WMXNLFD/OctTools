using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Controls;

namespace OctTools
{
    public class MyListBox : ListBox
    {
        //定义Imagesource依赖属性
        public static readonly System.Windows.DependencyProperty ImgSourceProperty = System.Windows.DependencyProperty.Register
            ("ImgSource", typeof(ImageSource), typeof(MyButton), null);

        public ImageSource ImgSource
        {
            get { return (ImageSource)GetValue(ImgSourceProperty); }
            set { SetValue(ImgSourceProperty, value); }
        }
    }
}
