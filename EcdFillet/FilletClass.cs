using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;



namespace EcdFillet
{
    public class FilletClass 
    {


        public static Document doc = Application.DocumentManager.MdiActiveDocument;
        public static Database db = doc.Database;
        public static Editor ed = doc.Editor;
        public static Point3d ptStart;
        public static Point3d ptEnd;
        public static Point3d IntePoint;
        public static int _index = 1;
        public static ObjectIdCollection ids;

        [CommandMethod("EcdFillet")]
        public static void EcdFillt()
        {
            Fillet();


            //PromptPointResult resultp = ed.GetPoint("\n指定第一个顶点");
            //if (resultp.Status != PromptStatus.OK) return;
            //Point3d basePt = resultp.Value;
            //resultp = ed.GetCorner("\n指定另一个顶点", basePt);
            //if (resultp.Status != PromptStatus.OK) return;
            //Point3d cornerPt = resultp.Value;
            //Point2d point1 = new Point2d(basePt.X, basePt.Y);
            //Point2d point2 = new Point2d(cornerPt.X, cornerPt.Y);
            //Polyline polyline = new Polyline();
            //Transaction(polyline, point1, point2);

            //Point3d point3d2 = new Point3d(basePt.X, cornerPt.Y, 0);//第一个顶点竖下对着的点
            //Point3d point3d3 = new Point3d(cornerPt.X, basePt.Y, 0);//第一个顶点横着对着的点


            //CreatePL();


            //PromptEntityOptions m_peo = new PromptEntityOptions("\n请选择第一条曲线:");
            //PromptEntityResult m_per = ed.GetEntity(m_peo);
            //if (m_per.Status != PromptStatus.OK) { return; }
            //ObjectId m_objid1 = m_per.ObjectId;

            //m_peo = new PromptEntityOptions("\n请选择第二条曲线:");
            //m_per = ed.GetEntity(m_peo);
            //if (m_per.Status != PromptStatus.OK) { return; }
            //ObjectId m_objid2 = m_per.ObjectId;

            //using (Transaction m_tr = db.TransactionManager.StartTransaction())
            //{
            //    Curve m_cur1 = (Curve)m_tr.GetObject(m_objid1, OpenMode.ForRead);
            //    Curve m_cur2 = (Curve)m_tr.GetObject(m_objid2, OpenMode.ForRead);

            //    Point3dCollection m_ints = new Point3dCollection();
            //    m_cur1.IntersectWith(m_cur2, Intersect.OnBothOperands, new Plane(), m_ints, IntPtr.Zero, IntPtr.Zero); //得出的所有交点在c1曲线上
            //    foreach (Point3d m_pt in m_ints)
            //    {
            //        ed.WriteMessage("\n第一条曲线与第二条曲线交点:{0}", m_pt);

            //        if (PtInPolygon1(m_pt, polyline) == 0)
            //        { ed.WriteMessage($"\n该点在矩形上，{PtInPolygon1(m_pt, polyline)}"); }
            //        else if (PtInPolygon1(m_pt, polyline) == 1)
            //        { ed.WriteMessage($"\n该点在矩形内，{PtInPolygon1(m_pt, polyline)}"); }
            //        else if (PtInPolygon1(m_pt, polyline) == -1)
            //        { ed.WriteMessage($"\n该点在矩形外，{PtInPolygon1(m_pt, polyline)}"); }

            //    }

            //    m_tr.Commit();
            //}

            //demo();

            //AddRegion();

            // TraceBoundaryAndHatch();
            //GetIntersection();
            //daojiao();
            //    ed.WriteMessage($"第一个点:{basePt},第二个点:{point3d3},第三个点:{cornerPt},第四个点:{point3d2}," +
            //    $"第一个交点：{ PLL(basePt, point3d2, ptStart, ptEnd)}" +
            //    $"第二个交点：{PLL(basePt,point3d3,ptStart,ptEnd)}");
            //ed.WriteMessage($"端点:{PLL(basePt, point3d2, ptStart, ptEnd)},夹角：" +
            //    $"{Angle(PLL(basePt, point3d2, ptStart, ptEnd),basePt, PLL(basePt, point3d3, ptStart, ptEnd))}");


        }

        /// <summary>
        ///倒角弧度
        /// </summary>
        public static void Fillet()//daojiao
        {


            using (var tr = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord btr = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                double dist;//偏移距离
                double radius;//半径
                              //PromptEntityOptions selectent = new PromptEntityOptions("\n选择实体对象");
                              //PromptEntityResult entityResult = ed.GetEntity(selectent);
                              //if (entityResult.Status != PromptStatus.OK) return;
                              //PromptSelectionOptions promptSelection = new PromptSelectionOptions();
                              //var dbobject = tr.GetObject(entityResult.ObjectId, OpenMode.ForRead);
                              //Polyline ent = dbobject as Polyline;
                              //ent.Highlight();

                //var objectID = entityResult.ObjectId;



                PromptDistanceOptions promptDistance = new PromptDistanceOptions("\n请输入偏移的距离");
                PromptDoubleResult distanceresult = ed.GetDistance(promptDistance);
                
                if (distanceresult.Status != PromptStatus.OK) return;
                dist = distanceresult.Value;

                PromptDoubleOptions promptr = new PromptDoubleOptions("\n请输入半径");
                PromptDoubleResult rresult = ed.GetDouble(promptr);
                if (rresult.Status != PromptStatus.OK) return;
                radius = rresult.Value;


                PromptSelectionOptions promptSelOpt = new PromptSelectionOptions();
                promptSelOpt.MessageForAdding = "\n请选择实体<回车结束>";

                SelectionFilter frameFilter = new SelectionFilter(
                         new TypedValue[]
                         { new TypedValue(0, "LWPOLYLINE"),
                        });
                PromptSelectionResult selectedFrameResult = ed.GetSelection(promptSelOpt, frameFilter);
                if (selectedFrameResult.Status != PromptStatus.OK) return;

                List<ObjectId> resultObjectIds = new List<ObjectId>(selectedFrameResult.Value.GetObjectIds());//获取所有选中实体的ID

                foreach (ObjectId frameId in resultObjectIds)
                {
                    Polyline framePline = tr.GetObject(frameId, OpenMode.ForRead) as Polyline;
                    //framePline.Highlight();
                    var offid = EntityHelper.Offset(framePline, dist);
                    foreach (DBObject id in offid)
                    {

                        Polyline offent = id as Polyline;
                        FilletatVertex(offent, radius);

                        btr.AppendEntity(offent);
                        tr.AddNewlyCreatedDBObject(offent, true);

                    }
                }
                tr.Commit();



            }

        }



        public static void FilletatVertex(Polyline polyline, double radius)
        {
            using (var tr = db.TransactionManager.StartTransaction())//开始事务     
            {
                try
                {
                    int indexs = polyline.NumberOfVertices;//获取顶点个数
                    Vector2d vec1;//向量1
                    Vector2d vec2;//向量2
                    double angle;
                    CircularArc2d cir;
                    //List<Point2d> pts = new List<Point2d>();
                    //for (int i =0; i <indexs; i++)
                    //{
                    //    Point2d pt = polyline.GetPoint2dAt(i);
                    //    pts.Add(new Point2d(pt.X, pt.Y));  //将顶点坐标放入集合中

                    //}

                    int[] vertexIndex = new int[indexs];

                    for (int i = 0; i < indexs; i++)
                    {

                        vertexIndex[i] = i;
                    }
                    Point2d pointmax = polyline.GetPoint2dAt(vertexIndex.Max()); //获取顶点最大的坐标作为最后一个倒角的坐标
                    foreach (var item in vertexIndex.Reverse())//反序倒角
                    {

                        Point2d pnt = polyline.GetPoint2dAt(item);
                        if (item == vertexIndex.Max())//当顶点为最大顶点时
                        {

                            vec1 = polyline.GetPoint2dAt(item - 1) - pnt;
                            vec2 = polyline.GetPoint2dAt(0) - pnt;
                            angle = vec1.GetAngleTo(vec2);//获得角点两侧线段间的角度变化
                        }
                        else if (item == vertexIndex.Min()) //当顶点为最小顶点时
                        {
                            vec1 = pointmax - pnt;
                            Point2d point = polyline.GetPoint2dAt(vertexIndex.Max());
                            vec2 = polyline.GetPoint2dAt(item + 1) - pnt;
                            Point2d point2 = polyline.GetPoint2dAt(item + 1);
                            angle = vec1.GetAngleTo(vec2);//获得角点两侧线段间的角度变化
                        }
                        //item=1 
                        else
                        {

                            vec1 = polyline.GetPoint2dAt(item - 1) - pnt;//由顶点与前一个顶点组成的向量
                            Point2d point = polyline.GetPoint2dAt(item - 1);
                            vec2 = polyline.GetPoint2dAt(item + 1) - pnt;//由顶点与后一个顶点组成的向量
                            Point2d point2 = polyline.GetPoint2dAt(item + 1);
                            angle = vec1.GetAngleTo(vec2);//获得角点两侧线段间的角度变化
                        }


                        if ((vec1.Length < radius / Math.Sin(angle / 2)) || (vec2.Length < radius / Math.Sin(angle / 2)))
                        {
                            ed.WriteMessage("\n半径太大,无法倒角!");
                            continue;
                        }

                        else
                        {


                            double lentemp = radius / Math.Tan(angle / 2);
                            Point2d addedVertex1 = pnt + vec1.GetNormal() * lentemp;
                            Point2d addedVertex2 = pnt + vec2.GetNormal() * lentemp;

                            polyline.RemoveVertexAt(item);
                            polyline.AddVertexAt(item, addedVertex1, 0, 0, 0);
                            polyline.AddVertexAt(item + 1, addedVertex2, 0, 0, 0);
                            //polyline.AddVertexAt(vertexIndex, addedVertex1.Convert2d(new Plane()), 0, 0, 0);
                            //polyline.AddVertexAt(vertexIndex + 1, addedVertex2.Convert2d(new Plane()), 0, 0, 0);

                            //根据前后三顶点判断该顶点处多义线的前进方向是顺时针还是逆时针
                            if (item == vertexIndex.Min())
                            {

                                cir = new CircularArc2d(pointmax, pnt, polyline.GetPoint2dAt(item + 1));

                            }
                            else
                            {

                                cir = new CircularArc2d(polyline.GetPoint2dAt(item - 1), pnt, polyline.GetPoint2dAt(item + 1));

                            }
                            double ang = Math.PI - angle;
                            double bulge = Math.Abs(Math.Tan(ang / 4));
                            if (cir.IsClockWise)
                            {
                                polyline.SetBulgeAt(item, -1 * bulge);//若为顺时针，则凸度为负；否则为正
                            }
                            else
                            {
                                polyline.SetBulgeAt(item, bulge);
                            }
                        }

                    }
                }
                catch (Autodesk.AutoCAD.Runtime.Exception e)
                {

                    ed.WriteMessage(e.ToString());
                    tr.Abort();
                }
                finally
                {
                    tr.Commit();
                }


            }
        }


        //public static void GetIntersection()
        //{

        //    Point3d pointstart;
        //    Point3d pointend;
        //    using (Transaction tr = db.TransactionManager.StartTransaction())
        //    {

        //        PromptEntityOptions optgetent = new PromptEntityOptions("\n请选择第一条多段线:");
        //        optgetent.SetRejectMessage("\n错误的选择");
        //        optgetent.AddAllowedClass(typeof(Polyline), true);
        //        PromptEntityResult resgetent = ed.GetEntity(optgetent);
        //        if (resgetent.Status == PromptStatus.OK)
        //        {
        //            ObjectId id1 = resgetent.ObjectId;
        //            Polyline line1 = (Polyline)tr.GetObject(id1, OpenMode.ForWrite);
        //            pointstart = line1.StartPoint;
        //            pointend = line1.EndPoint;

        //            //line1.Highlight();
        //            Point3d pt1 = resgetent.PickedPoint;
        //            optgetent.Message = "\n请选择第二条多段线:";
        //            resgetent = ed.GetEntity(optgetent);

        //            if (resgetent.Status == PromptStatus.OK)
        //            {
        //                ObjectId id2 = resgetent.ObjectId;
        //                Point3d pt2 = resgetent.PickedPoint;
        //                Polyline line2 = (Polyline)tr.GetObject(id2, OpenMode.ForWrite);
        //                //line2.Highlight();
        //                pt1 = line1.GetClosestPointTo(pt1, false);

        //                pt2 = line2.GetClosestPointTo(pt2, false);
        //                //获取两直线交点
        //                Point3dCollection pts = new Point3dCollection();
        //                pts.Add(pointstart);
        //                line1.IntersectWith(line2, Intersect.ExtendBoth, pts, IntPtr.Zero, IntPtr.Zero);
        //                foreach (var points in pts)
        //                {
        //                    ed.WriteMessage($"交点:{points}");
        //                }
        //                //Polyline newpl = new Polyline();
        //                //newpl.Highlight();
        //                //EntityHelper.CreatePolyline(newpl,pts);
        //                TransactionPL(pts);

        //                // CurveBoundary curveBoundary = new CurveBoundary();

        //                PromptPointResult resultp = ed.GetPoint("\n拾取内部的点：");
        //                if (resultp.Status != PromptStatus.OK) return;
        //                Point3d basePt = resultp.Value;
        //                ed.TraceBoundary(basePt, true);


        //                //SelectKeywordPolyLine();
        //            }

        //        }
        //    }
        //}
        //public static void CreatePL()//创建多段线
        //{


        //    PromptPointResult pPtRes;
        //    PromptPointOptions pPtOpts = new PromptPointOptions("\n选择起点:")
        //    {
        //        // 提示起点
        //        Message = "\n选择起点: "
        //    };
        //    pPtRes = ed.GetPoint(pPtOpts);
        //    ptStart = pPtRes.Value;

        //    // 如果用户按ESC键或取消命令，就退出
        //    if (pPtRes.Status == PromptStatus.Cancel) return;
        //    // 提示终点
        //    pPtOpts.Message = "\n选择终点 ";
        //    pPtOpts.UseBasePoint = true;
        //    pPtOpts.BasePoint = ptStart;
        //    pPtRes = ed.GetPoint(pPtOpts);
        //    ptEnd = pPtRes.Value;
        //    if (pPtRes.Status == PromptStatus.Cancel) return;
        //    Point3dCollection point3DCollection = new Point3dCollection
        //    {
        //        ptStart,
        //        ptEnd
        //    };
        //    TransactionPL(point3DCollection);

        //    GetKeywordPolyLine();



        //}



        //public static void TraceBoundaryAndHatch()
        //{


        //    //以点选的方式来获取我们的边界
        //    PromptPointResult ppr = ed.GetPoint("\n请选择内部点: ");
        //    if (ppr.Status != PromptStatus.OK)
        //        return;

        //    //获取创建边界的对象集合
        //    DBObjectCollection objs = ed.TraceBoundary(ppr.Value, false);



        //    if (objs.Count > 0)
        //    {
        //        Transaction tr = doc.TransactionManager.StartTransaction();
        //        using (tr)
        //        {
        //            //我们将对象添加到模型空间
        //            BlockTable bt = (BlockTable)tr.GetObject(doc.Database.BlockTableId, OpenMode.ForRead);
        //            BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

        //            PromptSelectionResult selectre = ed.SelectLast();
        //            List<ObjectId> objectId = new List<ObjectId>(selectre.Value.GetObjectIds());
        //            foreach (var id in objectId)
        //            {
        //                var dBObject = tr.GetObject(id, OpenMode.ForRead);
        //                if (dBObject is Polyline)
        //                {
        //                    Polyline pl = dBObject as Polyline;
        //                    //pl.Highlight();
        //                    string msg = string.Format($"ID：{id}，面域：{pl.Area}");
        //                    ed.WriteMessage(msg);



        //                }
        //            }
        //            //var value1 = new TypedValue(DxfCode.LayerName,);

        //            //SelectionFilter frameFilter = new SelectionFilter(new TypedValue[]
        //            //  { new TypedValue(0, "LWPOLYLINE"),
        //            //       new TypedValue(90, 4),
        //            //       new TypedValue(70, 1) });


        //            //PromptSelectionResult selectedFrameResult1 = ed.SelectAll(frameFilter);
        //            //List<ObjectId> resultObjectIds = new List<ObjectId>(selectedFrameResult1.Value.GetObjectIds());

        //            //foreach (ObjectId frameId in resultObjectIds)
        //            //{
        //            //    Polyline framePline = tr.GetObject(frameId, OpenMode.ForRead) as Polyline;
        //            //    framePline.Highlight();
        //            //}
        //            //将每个边界对象添加到块记录中,其ObjectId集合到ids集合
        //            ids = new ObjectIdCollection();

        //            foreach (DBObject obj in objs)
        //            {
        //                Entity ent = obj as Entity;
        //                Polyline pl = obj as Polyline;
        //                pl.Highlight();
        //                ent.Highlight();
        //                if (ent != null)
        //                {
        //                    //设置边界对象的颜色为自动增加的色号
        //                    ent.ColorIndex = 140;
        //                    // ent.Highlight();
        //                    //我们的透明度设置为50％（= 127）阿尔法值截断(255 * (100-n)/100)
        //                    ent.Transparency = new Transparency(150);

        //                    //每个边界对象添加到块记录,其ID添加到集合
        //                    ids.Add(btr.AppendEntity(ent));
        //                    tr.AddNewlyCreatedDBObject(ent, true);
        //                }
        //            }
        //            ////创建一个填充对象
        //            //Hatch hat = new Hatch();

        //            ////以Solid方式填充,其填充的颜色为自动增加的色号
        //            //hat.SetHatchPattern(HatchPatternType.PreDefined, "Solid");
        //            //hat.ColorIndex = _index++;

        //            ////我们的透明度设置为50％（= 127）阿尔法值截断(255 * (100-n)/100)
        //            //hat.Transparency = new Transparency(1);

        //            ////添加到块记录和事务
        //            //ObjectId hatId = btr.AppendEntity(hat);
        //            //tr.AddNewlyCreatedDBObject(hat, true);

        //            //hat.Associative = true;
        //            //hat.AppendLoop(HatchLoopTypes.Default, ids);
        //            //hat.EvaluateHatch(true);

        //            //提交事务
        //            tr.Commit();
        //        }
        //    }
        //}



        //public static void GetKeywordPolyLine()//是否重复创建多段线
        //{
        //    PromptKeywordOptions pKeyOpts = new PromptKeywordOptions("")
        //    {
        //        Message = "\n是否继续创建多段线 "
        //    };
        //    pKeyOpts.Keywords.Add("Y");
        //    pKeyOpts.Keywords.Add("N");
        //    pKeyOpts.AllowNone = true;
        //    PromptResult pKeyRes = ed.GetKeywords(pKeyOpts);
        //    if (pKeyRes.StringResult == "Y")
        //    {

        //        CreatePL();
        //    }
        //    else
        //    {

        //        return;
        //    }


        //}
        //public static void SelectKeywordPolyLine()//是否重复选择多段线
        //{
        //    var doc = Application.DocumentManager.MdiActiveDocument;
        //    PromptKeywordOptions pKeyOpts = new PromptKeywordOptions("")
        //    {
        //        Message = "\n是否继续选择多段线 "
        //    };
        //    pKeyOpts.Keywords.Add("Y");
        //    pKeyOpts.Keywords.Add("N");
        //    pKeyOpts.AllowNone = true;
        //    PromptResult pKeyRes = doc.Editor.GetKeywords(pKeyOpts);


        //    if (pKeyRes.StringResult == "Y")
        //    {

        //        GetIntersection();
        //    }
        //    else
        //    {

        //        return;
        //    }


        //}
        ///// <summary>
        ///// 根据余弦定理求两个线段夹角
        ///// </summary>
        ///// <param name="o">端点</param>
        ///// <param name="s">start点</param>
        ///// <param name="e">end点</param>
        ///// <returns></returns>
        //public static double Angle(Point3d o, Point3d s, Point3d e)
        //{
        //    double cosfi = 0, fi = 0, norm = 0;
        //    double dsx = s.X - o.X;
        //    double dsy = s.Y - o.Y;
        //    double dex = e.X - o.X;
        //    double dey = e.Y - o.Y;

        //    cosfi = dsx * dex + dsy * dey;
        //    norm = (dsx * dsx + dsy * dsy) * (dex * dex + dey * dey);
        //    cosfi /= Math.Sqrt(norm);

        //    if (cosfi >= 1.0) return 0;
        //    if (cosfi <= -1.0) return Math.PI;
        //    fi = Math.Acos(cosfi);

        //    if (180 * fi / Math.PI < 180)
        //    {
        //        return 180 * fi / Math.PI;
        //    }
        //    else
        //    {
        //        return 360 - 180 * fi / Math.PI;
        //    }
        //}






        //public static int PtInPolygon1(Point3d pt, Polyline pPolyline)//判断点是否在多边形内
        //{
        //    int count = pPolyline.NumberOfVertices;
        //    // 记录是否在多边形的边上
        //    bool isBeside = false;
        //    //构建多边形外接矩形
        //    double maxx = double.MinValue, maxy = double.MinValue, minx = double.MaxValue, miny = double.MaxValue;
        //    for (int i = 0; i < count; i++)
        //    {
        //        if (pPolyline.GetPoint3dAt(i).X > maxx)
        //        {
        //            maxx = pPolyline.GetPoint3dAt(i).X;
        //        }
        //        if (pPolyline.GetPoint3dAt(i).Y > maxy)
        //        {
        //            maxy = pPolyline.GetPoint3dAt(i).Y;
        //        }
        //        if (pPolyline.GetPoint3dAt(i).X < minx)
        //        {
        //            minx = pPolyline.GetPoint3dAt(i).X;
        //        }
        //        if (pPolyline.GetPoint3dAt(i).Y < miny)
        //        {
        //            miny = pPolyline.GetPoint3dAt(i).Y;
        //        }
        //    }
        //    if (pt.X > maxx || pt.Y > maxy || pt.X < minx || pt.Y < miny)
        //    {
        //        return -1;
        //    }

        //    Line line1 = new Line(new Point3d(maxx, pt.Y, 0), new Point3d(minx, pt.Y, 0));

        //    int crossCount = 0;

        //    using (Transaction tran = db.TransactionManager.StartTransaction())
        //    {
        //        BlockTableRecord pBlockTableRecord = tran.GetObject(db.CurrentSpaceId, OpenMode.ForWrite) as BlockTableRecord;

        //        Point3dCollection crossPoint = new Point3dCollection();

        //        line1.IntersectWith(pPolyline, Intersect.OnBothOperands, crossPoint, IntPtr.Zero, IntPtr.Zero);
        //        if (crossPoint.Count >= 1)
        //        {
        //            for (int n = 0; n < crossPoint.Count; n++)
        //            {
        //                Point3d crossPt = crossPoint[n];
        //                if (crossPt.X > pt.X)
        //                {
        //                    crossCount++;

        //                    //Circle pCircle = new Circle(crossPt, Vector3d.ZAxis, 2);

        //                    //pBlockTableRecord.AppendEntity(pCircle);
        //                    //tran.AddNewlyCreatedDBObject(pCircle, true);
        //                }
        //                else if (crossPt.X == pt.X)
        //                {
        //                    isBeside = true;
        //                }
        //            }

        //        }
        //        //Circle circle = new Circle(pt, Vector3d.ZAxis, 2);
        //        //pBlockTableRecord.AppendEntity(circle);
        //        //tran.AddNewlyCreatedDBObject(circle, true);

        //        //pBlockTableRecord.AppendEntity(line1);
        //        //tran.AddNewlyCreatedDBObject(line1, true);
        //        tran.Commit();
        //    }
        //    if (isBeside)
        //    { return 0; }
        //    else if (crossCount % 2 == 1)

        //        return 1;
        //    return -1;



        //}

        ///// <summary>
        ///// 求两两连线的交点
        ///// </summary>
        ///// <param name="P11">第一组点</param>
        ///// <param name="P12">第一组点</param>
        ///// <param name="P21">第二组点</param>
        ///// <param name="P22">第二组点</param>
        ///// <returns>若有交点就返回交点，否则返回P11</returns>
        //public static Point3d PLL(Point3d P11, Point3d P12, Point3d P21, Point3d P22)
        //{
        //    double A1 = P12.Y - P11.Y;
        //    double B1 = P11.X - P12.X;
        //    double C1 = -A1 * P11.X - B1 * P11.Y;
        //    double A2 = P22.Y - P21.Y;
        //    double B2 = P21.X - P22.X;
        //    double C2 = -A2 * P21.X - B2 * P21.Y;
        //    double dlt = A1 * B2 - A2 * B1;
        //    double dltx = C1 * B2 - C2 * B1;
        //    double dlty = A1 * C2 - A2 * C1;
        //    if (Math.Abs(dlt) < 0.00000001)
        //    {
        //        return P11;
        //    }
        //    else
        //    {
        //        return new Point3d(-1.0 * (dltx / dlt), -1.0 * (dlty / dlt), 0);

        //    }
        //}
        //public static void TransactionPL(Point3dCollection point3DCollection)//生成多段线添加模型空间
        //{


        //    using (var tr = db.TransactionManager.StartTransaction())
        //    {
        //        BlockTable bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
        //        BlockTableRecord btr = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
        //        Polyline polyline1 = new Polyline();

        //        EntityHelper.CreatePolyline(polyline1, point3DCollection);
        //        btr.AppendEntity(polyline1);
        //        polyline1.Highlight();
        //        // tr.AddNewlyCreatedDBObject(polyline1, true);

        //        tr.Commit();
        //        tr.Dispose();


        //    }

        //}
        //public static void Transaction(Polyline polyline, Point2d point1, Point2d point2)//将生成的矩形添加到模型空间
        //{

        //    using (var tr = db.TransactionManager.StartTransaction())
        //    {
        //        BlockTable bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
        //        BlockTableRecord btr = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

        //        EntityHelper.CreateRectangle(polyline, point1, point2);
        //        btr.AppendEntity(polyline);
        //        tr.AddNewlyCreatedDBObject(polyline, true);
        //        tr.Commit();
        //        tr.Dispose();
        //    }

        //}

        //public static void AddRegion()
        //{
        //    // 获取当前文档和数据库
        //    //获取当前文档及数据库
        //    Document acDoc = Application.DocumentManager.MdiActiveDocument;

        //    Database acCurDb = acDoc.Database;
        //    // 启动事务
        //    using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
        //    {
        //        // 以读模式打开Block表
        //        BlockTable acBlkTbl;
        //        acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId,
        //        OpenMode.ForRead) as BlockTable;
        //        // 以写模式打开Block表记录Model空间
        //        BlockTableRecord acBlkTblRec;
        //        acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
        //        OpenMode.ForWrite) as BlockTableRecord;
        //        // 在内存创建一个圆
        //        using (Circle acCirc = new Circle())
        //        {
        //            acCirc.Center = new Point3d(2, 2, 0);
        //            acCirc.Radius = 5;
        //            // 将圆添加到对象数组
        //            DBObjectCollection acDBObjColl = new DBObjectCollection();
        //            acDBObjColl.Add(acCirc);
        //            // 基于每个闭环计算面域
        //            DBObjectCollection myRegionColl = new DBObjectCollection();
        //            myRegionColl = Region.CreateFromCurves(acDBObjColl);
        //            Region acRegion = myRegionColl[0] as Region;
        //            // 将新对象添加到块表记录和事务
        //            acBlkTblRec.AppendEntity(acRegion);
        //            acTrans.AddNewlyCreatedDBObject(acRegion, true);
        //            //处置内存中的圆，不添加到数据库;
        //        }
        //        // 将新对象保存到数据库
        //        acTrans.Commit();
        //    }
        //}

        //public static void demo()
        //{


        //    PromptEntityOptions peo = new PromptEntityOptions("\n选择多段线: ");
        //    peo.SetRejectMessage("\n必须为多段线！");
        //    peo.AddAllowedClass(typeof(Polyline), true);
        //    PromptEntityResult per = ed.GetEntity(peo);
        //    if (per.Status != PromptStatus.OK)
        //        return;
        //    using (var tr = db.TransactionManager.StartTransaction())
        //    {
        //        Polyline pline = (Polyline)tr.GetObject(per.ObjectId, OpenMode.ForRead);


        //        var pdr = ed.GetDistance("\n指定偏移距离: ");
        //        if (pdr.Status != PromptStatus.OK)
        //            return;
        //        var offsetCurves = pline.GetOffsetCurves(pdr.Value);
        //        if (offsetCurves.Count != 1)
        //        {
        //            ed.WriteMessage("\n曲线创建偏移出现错误");
        //            foreach (DBObject obj in offsetCurves) obj.Dispose();
        //            return;
        //        }
        //        using (var polygon = (Polyline)offsetCurves[0])
        //        {
        //            //offsetCurves = pline.GetOffsetCurves(-pdr.Value);
        //            //if (offsetCurves.Count != 1)
        //            //{
        //            //    ed.WriteMessage("\n曲线创建偏移出现错误");
        //            //    foreach (DBObject obj in offsetCurves) obj.Dispose();
        //            //    return;
        //            //}
        //            using (var curve = (Polyline)offsetCurves[0])
        //            using (var line = new Line(polygon.EndPoint, curve.EndPoint))
        //            {
        //                polygon.JoinEntities(new Entity[] { new Line(polygon.EndPoint, curve.EndPoint), curve });
        //                polygon.Closed = true;
        //                var curSpace = (BlockTableRecord)tr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite);
        //                curSpace.AppendEntity(polygon);
        //                tr.AddNewlyCreatedDBObject(polygon, true);
        //            }
        //        }
        //        tr.Commit();
        //    }
        //}

       
    }
}