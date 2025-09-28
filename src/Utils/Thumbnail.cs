using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Wpf_test.src.Utils
{
    public readonly struct Thumbnail(Border image, string path)
    {
        public readonly Border image = image;
        public readonly string path = path;
    }
}
