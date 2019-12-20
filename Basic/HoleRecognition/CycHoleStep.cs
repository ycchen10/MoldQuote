using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NXOpen;

namespace CycBasic
{
    public class CycHoleStep : IDisplayObject, IComparable<CycHoleStep>
    {
        /// <summary>
        /// 起点
        /// </summary>
        public Point3d StartPos { get; set; }
        /// <summary>
        /// 终点
        /// </summary>
        public Point3d EndPos { get; set; }
        /// <summary>
        /// 最大半径
        /// </summary>
        public double MaxDia { get; set; }
        /// <summary>
        /// 最小半径
        /// </summary>
        public double MinDia { get; set; }
        /// <summary>
        /// 孔类型
        /// </summary>
        public HoleStep_Type_t Type { get; set; }

        public Face Face { get; set; }
        /// <summary>
        /// 面数据
        /// </summary>
        public CycFaceData FaceData { get; set; }
        /// <summary>
        /// 高度
        /// </summary>
        public double HoleStepHigth { get; set; }
        /// <summary>
        /// 矩阵
        /// </summary>
        public Matrix4 Matr { get; set; }
        /// <summary>
        /// 高亮显示
        /// </summary>
        /// <param name="highlight"></param>
        public void Highlight(bool highlight)
        {
            if (highlight)
                this.Face.Highlight();
            else
                this.Face.Unhighlight();
        }
        /// <summary>
        /// 计算孔台阶数据（WCS）
        /// </summary>
        /// <param name="vec">孔向量</param>
        public void ComputeHoleStepAttr()
        {
            try
            {


                this.FaceData = CycFaceUtils.AskFaceData(this.Face);
                Matrix4 mat = new Matrix4();
                mat.Identity();
                string err = "";
                if (this.Face.GetEdges().Length > 0 && this.Face.GetEdges().Length <= 2)
                {
                    if (this.Face.GetEdges().Length == 1)
                    {
                        ArcEdgeData da = CycEdgeUtils.GetArcData(this.Face.GetEdges()[0], ref err);
                        if (this.Type == HoleStep_Type_t.HoleStep_Cone_Type_t)
                        {
                            this.StartPos = da.Center;
                            this.EndPos = UMathUtils.GetSymmetry(da.Center, this.FaceData.Point);
                            Vector3d vec = UMathUtils.GetVector(this.StartPos, this.EndPos);
                            mat.TransformToZAxis(da.Center, vec);
                            this.Matr = mat;
                            this.MaxDia = Math.Round(da.Radius, 4);
                            this.MinDia = 0;
                            this.HoleStepHigth = Math.Round(UMathUtils.GetDis(this.EndPos,this.StartPos), 4);
                        }
                        else
                        {
                            mat.TransformToZAxis(da.Center, this.FaceData.Dir);
                            this.MaxDia = Math.Round(da.Radius, 4);
                            this.MinDia = 0;
                            this.HoleStepHigth = 0;
                        }

                    }
                    else
                    {
                        ArcEdgeData da1 = CycEdgeUtils.GetArcData(this.Face.GetEdges()[0], ref err);
                        ArcEdgeData da2 = CycEdgeUtils.GetArcData(this.Face.GetEdges()[1], ref err);
                        this.HoleStepHigth = Math.Round(UMathUtils.GetDis(da1.Center, da2.Center), 4);
                        if (da1.Radius >= da2.Radius)
                        {
                            this.MaxDia = Math.Round(da1.Radius, 4);
                            this.MinDia = Math.Round(da2.Radius, 4);

                            this.StartPos = da1.Center;
                            this.EndPos = da2.Center;
                            if (this.HoleStepHigth == 0)
                            {
                                Vector3d vec = this.FaceData.Dir;
                                mat.TransformToZAxis(da1.Center, vec);
                                this.Matr = mat;
                            }
                            else
                            {
                                Vector3d vec = UMathUtils.GetVector(da1.Center, da2.Center);
                                mat.TransformToZAxis(da1.Center, vec);
                                this.Matr = mat;
                            }
                        }
                        else
                        {
                            this.MaxDia = Math.Round(da2.Radius, 4);
                            this.MinDia = Math.Round(da1.Radius, 4);
                            Vector3d vec = UMathUtils.GetVector(da2.Center, da1.Center);
                            mat.TransformToZAxis(da1.Center, vec);
                            this.Matr = mat;
                            this.StartPos = da2.Center;
                            this.EndPos = da1.Center;
                        }
                    }
                }
                else
                {
                    LogMgr.WriteLog("CycHoleStrp.ComputeHoleStepAttr:");
                }
            }
            catch (Exception ex)
            {
                LogMgr.WriteLog("CycHoleStrp.ComputeHoleStepAttr:" + ex.Message);
            }

        }
        /// <summary>
        /// 判断是否是同一个孔
        /// </summary>
        /// <param name="hs"></param>
        /// <returns></returns>
        public bool IsTheSameHole(CycHoleStep hs)
        {
            double angle = UMathUtils.Angle(this.FaceData.Dir, hs.FaceData.Dir);
            if (UMathUtils.IsEqual(angle, 0) == false && UMathUtils.IsEqual(angle, Math.PI) == false)
            {
                return false;
            }

            Vector3d vec = UMathUtils.GetVector(this.FaceData.Point, hs.FaceData.Point);
            angle = UMathUtils.Angle(this.FaceData.Dir, vec);
            if (UMathUtils.IsEqual(angle, 0) == false && UMathUtils.IsEqual(angle, Math.PI) == false)
            {
                return false;
            }

            return true;
        }

        public int CompareTo(CycHoleStep other)
        {
            try
            {
                if (this.MaxDia == this.MinDia)
                {
                    Vector3d vec = new Vector3d();
                    Matrix4 mat = new Matrix4();
                    mat.Identity();
                    if (this.MaxDia >= other.MaxDia)
                    {
                        if (UMathUtils.IsEqual(this.StartPos, other.StartPos))
                        {
                            vec = UMathUtils.GetVector(other.EndPos, this.EndPos);
                        }
                        else
                            vec = UMathUtils.GetVector(other.StartPos, this.StartPos);
                    }
                    else
                    {


                        if (UMathUtils.IsEqual(this.StartPos, other.StartPos))
                        {
                            vec = UMathUtils.GetVector(this.EndPos, other.EndPos);
                        }
                        else
                            vec = UMathUtils.GetVector(this.StartPos, other.StartPos);
                    }
                    mat.TransformToZAxis(this.StartPos, vec);
                    Point3d center1 = UMathUtils.GetMiddle(this.StartPos, this.EndPos);
                    Point3d center2 = UMathUtils.GetMiddle(other.StartPos, other.EndPos);
                    mat.ApplyPos(ref center1);
                    mat.ApplyPos(ref center2);
                    if (Math.Round(center1.Z, 4) >= Math.Round(center2.Z, 4))
                        return -1;
                    else
                        return 1;
                }
                else
                {
                    Point3d center1 = UMathUtils.GetMiddle(this.StartPos, this.EndPos);
                    Point3d center2 = UMathUtils.GetMiddle(other.StartPos, other.EndPos);
                    this.Matr.ApplyPos(ref center1);
                    this.Matr.ApplyPos(ref center2);
                    if (Math.Round(center1.Z, 4) >= Math.Round(center2.Z, 4))
                        return -1;
                    else
                        return 1;
                }

            }
            catch (Exception ex)
            {
                LogMgr.WriteLog("CycHoleStrp.CompareTo:" + ex.Message);
                return 1;
            }

        }


        /// <summary>
        /// 分析
        /// </summary>
        /// <param name="hs"></param>
        /// <param name="vec"></param>
        /// <param name="center"></param>
        /// <param name="dis"></param>
        public void AskHoleStepBoundingBox(Matrix4 mat, ref Point3d center, ref Point3d dis)
        {

            Matrix4 invers = mat.GetInversMatrix();
            NXObject[] objs = { this.Face };
            CoordinateSystem cs = CycBoundingBoxUtils.CreateCoordinateSystem(mat, invers);
            CycBoundingBoxUtils.GetBoundingBoxInLocal(objs, cs, mat, ref center, ref dis);
        }

        /// <summary>
        /// 分析孔TOp边
        /// </summary>
        /// <param name="holeStep">孔台阶</param>
        /// <param name="vec">孔向量</param>
        /// <returns></returns>
        public Edge GetEdgeOfHoleStep(Vector3d vec, ref Point3d origin)
        {
            string err = null;
            if (this.Type == HoleStep_Type_t.HoleStep_Cone_Type_t)
            {
                foreach (Edge eg in this.Face.GetEdges())
                {
                    if (eg.SolidEdgeType == Edge.EdgeType.Circular)
                    {
                        origin = CycEdgeUtils.GetArcData(eg, ref err).Center;
                        return eg;
                    }

                }
            }
            else
            {
                List<ArcEdgeData> are = new List<ArcEdgeData>();
                foreach (Edge eg in this.Face.GetEdges())
                {
                    if (eg.SolidEdgeType == Edge.EdgeType.Circular)
                    {

                        are.Add(CycEdgeUtils.GetArcData(eg, ref err));
                    }
                }
                if (are.Count <= 2)
                {
                    if (are.Count == 1)
                        return are[0].Edge;
                    else
                    {
                        Vector3d temp = UMathUtils.GetVector(are[0].Center, are[1].Center);
                        double angle = UMathUtils.Angle(temp, vec);
                        if (UMathUtils.IsEqual(angle, 0))
                        {
                            origin = are[1].Center;
                            return are[1].Edge;
                        }
                        else
                        {
                            origin = are[0].Center;
                            return are[0].Edge;
                        }

                    }
                }
                else
                    return null;
            }
            return null;
        }
        public override string ToString()
        {
            return this.MaxDia.ToString() + "," + this.MinDia.ToString() + "," + this.HoleStepHigth.ToString();
        }



    }
    public enum HoleStep_Type_t
    {
        HoleStep_Cylinder_Type_t = 0,              // 圆柱
        HoleStep_Cone_Type_t = 1,                  //一条边圆锥
        HoleStep_Ring_Typt_t = 2,                 //圆环
        HoleStep_Tur_type_t = 3,                  //两条边圆锥
    }


}

