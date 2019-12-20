using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NXOpen;

namespace CycBasic
{
    public class CycHoleRegMgr
    {

        private static CycHoleRegMgr m_instance = null;

        private List<CycHoleFeature> m_holeList = new List<CycHoleFeature>();

        public static CycHoleRegMgr Instance()
        {
            if (m_instance == null)
                m_instance = new CycHoleRegMgr();
            return m_instance;
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        private CycHoleRegMgr()
        {

        }
        /// <summary>
        /// 清除
        /// </summary>
        public void Reset()
        {
            m_holeList.Clear();
        }
        /// <summary>
        /// 判断面是否是孔面
        /// </summary>
        /// <param name="face"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        private bool IsHole(Face face, Body body)
        {
            CycFaceData fd = CycFaceUtils.AskFaceData(face);

            Matrix4 mat = new Matrix4();
            mat.Identity();
            mat.TransformToZAxis(fd.Point, fd.Dir);

            Matrix4 invers = mat.GetInversMatrix();
            Point3d center = new Point3d();
            Point3d dis = new Point3d();
            NXObject[] obj = { face };
            CycBoundingBoxUtils.GetBoundingBoxInLocal(obj, CycBoundingBoxUtils.CreateCoordinateSystem(mat, invers), mat, ref center, ref dis);

            Session theSession = Session.GetSession();
            Part workPart = theSession.Parts.Work;
            NXOpen.UF.UFSession ufsession = NXOpen.UF.UFSession.GetUFSession();
            invers.ApplyPos(ref center);
            double[] pot = { center.X, center.Y, center.Z };
            int out_status;
            ufsession.Modl.AskPointContainment(pot, body.Tag, out out_status);

            if (out_status != 2)
            {
                return false;
            }
            return true;
        }
        /// <summary>
        /// 查找整圆孔
        /// </summary>
        /// <param name="body"></param>
        public void Regcognize(Body body)
        {
            List<CycHoleStep> stepList = new List<CycHoleStep>();
            #region 分类
            foreach (Face face in body.GetFaces())
            {
                CycFaceData facedata = CycFaceUtils.AskFaceData(face);
                Matrix4 mat = new Matrix4();
                mat.TransformToZAxis(facedata.Point, facedata.Dir);
                if (face.SolidFaceType == Face.FaceType.Cylindrical) //圆柱
                {
                    if (IsHole(face, body))
                    {
                        if (face.GetEdges().Length == 2)
                        {
                            CycHoleStep hs = new CycHoleStep();
                            hs.Face = face;
                            hs.Type = HoleStep_Type_t.HoleStep_Cylinder_Type_t;
                            hs.ComputeHoleStepAttr();
                            stepList.Add(hs);
                        }
                    }

                }
                else if (face.SolidFaceType == Face.FaceType.Conical)  //圆锥
                {

                    if (face.GetEdges().Length <= 2)
                    {
                        CycHoleStep hs = new CycHoleStep();
                        hs.Face = face;
                        if (face.GetEdges().Length == 1)
                            hs.Type = HoleStep_Type_t.HoleStep_Cone_Type_t;
                        else
                            hs.Type = HoleStep_Type_t.HoleStep_Tur_type_t;
                        hs.ComputeHoleStepAttr();
                        stepList.Add(hs);
                    }
                }
                else if (face.SolidFaceType == Face.FaceType.Planar) //平面
                {
                    if (face.GetEdges().Length <= 2)
                    {
                        bool isCycle = true;

                        foreach (Edge edge in face.GetEdges())
                        {
                            if (edge.SolidEdgeType != Edge.EdgeType.Circular)
                            {
                                isCycle = false;
                                break;
                            }

                        }
                        if (isCycle)
                        {
                            CycHoleStep hs = new CycHoleStep();
                            hs.Face = face;
                            hs.Type = HoleStep_Type_t.HoleStep_Ring_Typt_t;
                            hs.ComputeHoleStepAttr();
                            stepList.Add(hs);
                        }
                    }

                }
            }
            #endregion
            for (int i = 0; i < stepList.Count; i++)
            {
                CycHoleStep hs = stepList[i];

                CycHoleFeature hf = FindHole(hs);
                if (hf == null)
                {


                    hf = new CycHoleFeature();

                    m_holeList.Add(hf);
                }

                hf.StepList.Add(hs);
            }


        }
        /// <summary>
        /// 查找一个孔的面
        /// </summary>
        /// <param name="holeStep"></param>
        /// <returns></returns>
        private CycHoleFeature FindHole(CycHoleStep holeStep)
        {
            if (m_holeList == null || m_holeList.Count == 0)
                return null;

            foreach (CycHoleFeature hf in m_holeList)
            {
                if (hf.IsInThisHole(holeStep))
                    return hf;
            }

            return null;
        }
        /// <summary>
        /// 获取孔列表
        /// </summary>
        /// <returns></returns>
        public List<CycHoleFeature> GetHoleList()
        {
            List<CycHoleFeature> ht = new List<CycHoleFeature>();
            foreach (CycHoleFeature hf in m_holeList)
            {
                hf.SetHoleFeatureAttr();
               if (hf.StepList.Count == 1)
                {
                    if (!(hf.StepList[0].Type == HoleStep_Type_t.HoleStep_Ring_Typt_t || hf.StepList[0].Type == HoleStep_Type_t.HoleStep_Cone_Type_t))
                        ht.Add(hf);
                }
                else
                    ht.Add(hf);
            }
            return ht;
        }

    }
}
