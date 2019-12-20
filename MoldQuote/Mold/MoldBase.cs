using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NXOpen;

namespace MoldQuote.Mold
{
    public class MoldBase : Base
    {
        /// <summary>
        /// 长
        /// </summary>
        double Length { get; set; }
        /// <summary>
        /// 宽
        /// </summary>
        double Width { get; set; }
        /// <summary>
        /// 高
        /// </summary>
        double Heigth { get; set; }

    }
}
