using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NXOpen;
using NXOpen.UF;
using NXOpen.Utilities;


namespace CycBasic
{
    public class CycHoleFeature : IDisplayObject
    {
        /// <summary>
        /// 阶梯
        /// </summary>
        public List<CycHoleStep> StepList = new List<CycHoleStep>();
        /// <summary>
        /// 矩阵，相对坐标系
        /// </summary>
        private Matrix4 m_mat = null;
        /// <summary>
        /// 逆矩阵
        /// </summary>
        private Matrix4 m_wcsMat = null;
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 中心点
        /// </summary>
        public Point3d Origin { get; set; }
        /// <summary>
        ///轴向
        /// </summary>
        public Vector3d Direction { get; set; }
        /// <summary>
        /// 顶边
        /// </summary>
        public Edge TopEdge { get; set; } = null;
        /// <summary>
        /// 台阶面
        /// </summary>
        public List<CycHoleStep> StepFace { get; set; } = new List<CycHoleStep>();
        /// <summary>
        /// 孔类型
        /// </summary>
        public HoleFeature_Type_t HoleType { get; set; }
        /// <summary>
        /// 孔高度
        /// </summary>
        public double HoleHigth { get; set; }
        /// <summary>
        /// 高亮显示
        /// </summary>
        /// <param name="highlight"></param>
        public void Highlight(bool highlight)
        {
            foreach (CycHoleStep hs in this.StepList)
            {

                hs.Highlight(highlight);

            }
        }
        /// <summary>
        /// 判断是否是同一个孔
        /// </summary>
        /// <param name="hs"></param>
        /// <returns></returns>
        public bool IsInThisHole(CycHoleStep hs)
        {
            if (this.StepList == null || this.StepList.Count == 0)
                return false;
            CycHoleStep first = this.StepList[0];
            if (hs.IsTheSameHole(first))
            {
                return true;
            }

            return false;
        }
        /// <summary>
        /// 写入属性
        /// </summary>
        public void SetHoleFeatureAttr()
        {
            this.StepList.Sort();
            m_mat = new Matrix4();
            m_mat.Identity();
            if (this.StepList != null || this.StepList.Count > 0)
            {

                this.StepList.Sort();
                if (this.StepList.Count == 1)
                {
                    this.HoleHigth = UMathUtils.GetDis(this.StepList[0].StartPos, this.StepList[0].EndPos);  //HoleStep 轴方向
                    this.HoleType = HoleFeature_Type_t.HoleFeature_Only_Blind_Hole_Type_t;  //单一通孔
                    this.Direction = StepList[0].FaceData.Dir;
                    Point3d pt = new Point3d();
                    this.TopEdge = StepList[0].GetEdgeOfHoleStep(this.Direction, ref pt);
                    this.Origin = StepList[0].StartPos;

                    this.Name = this.StepList[0].ToString();
                }
                else
                {
                    this.Direction = UMathUtils.GetVector(this.StepList[1].StartPos, this.StepList[0].StartPos);
                    Point3d pt = new Point3d();
                    this.TopEdge = StepList[0].GetEdgeOfHoleStep(this.Direction, ref pt);
                    this.Origin = pt;
                    m_mat.TransformToZAxis(pt, this.Direction);
                    foreach (CycHoleStep hs in this.StepList)
                    {
                        hs.Matr = m_mat;
                        if (hs.Type == HoleStep_Type_t.HoleStep_Ring_Typt_t)
                        {
                            this.StepFace.Add(hs);
                        }
                    }
                    this.StepList.Sort();
                    foreach (CycHoleStep hs in this.StepList)   //转换为WCS坐标
                    {
                        this.HoleHigth = this.HoleHigth + hs.HoleStepHigth;

                        this.Name = this.Name + "(" + hs.ToString() + ")";
                    }
                    if (StepList[StepList.Count - 1].Type != HoleStep_Type_t.HoleStep_Ring_Typt_t ||
                        StepList[StepList.Count - 1].Type != HoleStep_Type_t.HoleStep_Cone_Type_t)
                    {
                        if (this.StepFace.Count == 0)
                            this.HoleType = HoleFeature_Type_t.HoleFeature_Only_Through_Hole_Type_t; //单一通孔
                        else
                            this.HoleType = HoleFeature_Type_t.HoleFeature_Step_Through_Hole_Type_t; //台阶通孔
                    }
                    if (StepList[StepList.Count - 1].Type != HoleStep_Type_t.HoleStep_Cone_Type_t)
                    {
                        if (this.StepFace.Count == 0)
                            this.HoleType = HoleFeature_Type_t.HoleFeature_Only_Blind_Hole_Type_t; //单一盲孔
                        else
                            this.HoleType = HoleFeature_Type_t.HoleFeature_Step_Blind_Hole_Type_t;//台阶盲孔
                    }
                    if (StepList[StepList.Count - 1].Type != HoleStep_Type_t.HoleStep_Ring_Typt_t)
                    {
                        this.HoleType = HoleFeature_Type_t.HoleFeature_Step_Hole_Type_t;             //平底孔
                    }


                }
            }
            else
            {
                CycBasic.LogMgr.WriteLog("CycHoleFeature.StepList=null!");
            }
        }

        //public bool IsEquals(CycHoleFeature other)
        //{
        //    if (this.HoleType == other.HoleType)
        //    {
        //        if (this.Name == other.Name)
        //        {

        //        }
        //        else
        //            return false;
        //    }
        //    else
        //        return true;
        //}
    }
    public enum HoleFeature_Type_t
    {
        HoleFeature_Only_Through_Hole_Type_t = 0,             //单一通孔

        HoleFeature_Step_Through_Hole_Type_t = 1,           //台阶通孔

        HoleFeature_Only_Blind_Hole_Type_t = 2,             //单一盲孔

        HoleFeature_Step_Blind_Hole_Type_t = 3,             // 台阶盲孔

        HoleFeature_Step_Hole_Type_t = 4,                   //平底孔
    }
}
