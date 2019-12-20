using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NXOpen;

namespace MoldQuote.Mold
{
    public class Base
    {
        /// <summary>
        ///标识
        /// </summary>
        NXObject Obj { get; set; }
        /// <summary>
        /// 名字
        /// </summary>
        string Name { get; set; }
        /// <summary>
        /// 中心点
        /// </summary>
        Point3d CenterPt { get; set; } 
        /// <summary>
        /// 最大外形
        /// </summary>
        Point3d DisPt { get; set; }

    }
}
