using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Linq;

namespace EcdFillet
{
    public static class EntityHelper
    {

        /// <summary>
        /// 创建矩形
        /// </summary>
        /// <param name="pline">多段线对象</param>
        /// <param name="pt1">矩形的角点</param>
        /// <param name="pt2">矩形的角点</param>
        public static void CreateRectangle(this Polyline pline, Point2d pt1, Point2d pt2)
        {
            //设置矩形的4个顶点
            double minX = Math.Min(pt1.X, pt2.X);
            double maxX = Math.Max(pt1.X, pt2.X);
            double minY = Math.Min(pt1.Y, pt2.Y);
            double maxY = Math.Max(pt1.Y, pt2.Y);
            Point2dCollection pts = new Point2dCollection();
            pts.Add(new Point2d(minX, minY));
            pts.Add(new Point2d(minX, maxY));
            pts.Add(new Point2d(maxX, maxY));
            pts.Add(new Point2d(maxX, minY));
            pline.CreatePolyline(pts);
            pline.Closed = true;//闭合多段线以形成矩形
        }
        /// <summary>
        /// 通过三维点集合创建多段线
        /// </summary>
        /// <param name="pline">多段线对象</param>
        /// <param name="pts">多段线的顶点</param>
        public static void CreatePolyline(this Polyline pline, Point3dCollection pts)
        {
            for (int i = 0; i < pts.Count; i++)
            {
                //添加多段线的顶点
                pline.AddVertexAt(i, new Point2d(pts[i].X, pts[i].Y), 0, 0, 0);
            }
        }

        /// <summary>
        /// 通过二维点集合创建多段线
        /// </summary>
        /// <param name="pline">多段线对象</param>
        /// <param name="pts">多段线的顶点</param>
        public static void CreatePolyline(this Polyline pline, Point2dCollection pts)
        {
            for (int i = 0; i < pts.Count; i++)
            {
                //添加多段线的顶点
                pline.AddVertexAt(i, pts[i], 0, 0, 0);
            }
        }

        /// <summary>
        /// 偏移实体
        /// </summary>
        /// <param name="id">实体的ObjectId</param>
        /// <param name="dis">偏移距离</param>
        /// <returns>返回偏移后的实体Id集合</returns>
        public static ObjectIdCollection Offset(this ObjectId id, double dis)
        {
            ObjectIdCollection ids = new ObjectIdCollection();
            Curve cur = id.GetObject(OpenMode.ForWrite) as Curve;
            if (cur != null)
            {
                try
                {
                    //获取偏移的对象集合
                    DBObjectCollection offsetCurves = cur.GetOffsetCurves(dis);
                    //将对象集合类型转换为实体类的数组，以方便加入实体的操作
                    Entity[] offsetEnts = new Entity[offsetCurves.Count];
                    offsetCurves.CopyTo(offsetEnts, 0);
                    //将偏移的对象加入到数据库
                    ids = id.Database.AddToModelSpace(offsetEnts);
                }
                catch
                {
                    Application.ShowAlertDialog("无法偏移！");
                }
            }
            else
                Application.ShowAlertDialog("无法偏移！");
            return ids;//返回偏移后的实体Id集合
        }

        /// <summary>
        /// 将实体添加到模型空间
        /// </summary>
        /// <param name="db">数据库对象</param>
        /// <param name="ents">要添加的多个实体</param>
        /// <returns>返回添加到模型空间中的实体ObjectId集合</returns>
        public static ObjectIdCollection AddToModelSpace(this Database db, params Entity[] ents)
        {
            ObjectIdCollection ids = new ObjectIdCollection();
            var trans = db.TransactionManager;
            BlockTableRecord btr = (BlockTableRecord)trans.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(db), OpenMode.ForWrite);
            foreach (var ent in ents)
            {
                ids.Add(btr.AppendEntity(ent));
                trans.AddNewlyCreatedDBObject(ent, true);
            }
            btr.DowngradeOpen();
            return ids;
        }

        /// <summary>
        /// 偏移实体
        /// </summary>
        /// <param name="ent">实体</param>
        /// <param name="dis">偏移距离</param>
        /// <returns>返回偏移后的实体集合</returns>
        public static DBObjectCollection Offset(this Polyline ent, double dis)
        {
            DBObjectCollection offsetCurves = new DBObjectCollection();
            Curve cur = ent as Curve;
            if (cur != null)
            {
                try
                {
                    offsetCurves = cur.GetOffsetCurves(dis);
                    Entity[] offsetEnts = new Entity[offsetCurves.Count];
                    offsetCurves.CopyTo(offsetEnts, 0);
                }
                catch
                {
                    Application.ShowAlertDialog("无法偏移！");
                }
            }
            else
                Application.ShowAlertDialog("无法偏移！");
            return offsetCurves;
        }


        /// <summary>
        /// 倒圆角。生成两点，按左右上下序。
        /// </summary>
        /// <param name="vertex"></param>
        /// <param name="vec1"></param>
        /// <param name="vec2"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        public static Point2d[] Fillet(Point2d vertex,
            Vector2d vec1, Vector2d vec2, double radius)
        {
            var uvec1 = vec1.GetNormal();
            var uvec2 = vec2.GetNormal();
            var vecToCenterUnit = (uvec1 + uvec2).GetNormal();

            var vecToCenter = vecToCenterUnit * radius /
                Math.Sin(Math.Min(vecToCenterUnit.GetAngleTo(uvec1),
                         vecToCenterUnit.GetAngleTo(uvec2)));

            var projVec1 = uvec1 * uvec1.DotProduct(vecToCenter);
            var projVec2 = uvec2 * uvec2.DotProduct(vecToCenter);

            return new[] { vertex + projVec1, vertex + projVec2 }
                .OrderBy(p => p.X)
                .ThenBy(p => p.Y)
                .ToArray();
        }

        /// <summary>
        /// 创建多边形。
        /// </summary>
        /// <param name="locations">{ x0, y0, x1, y1, ... }</param>
        /// <param name="bulges"></param>
        /// <param name="startWidth"></param>
        /// <param name="endWidth"></param>
        /// <param name="closed"></param>
        /// <returns></returns>
        public static Polyline CreatePolygon(
            double[] locations,
            Tuple<int, double>[] bulges = null,
            Tuple<int, double>[] startWidth = null,
            Tuple<int, double>[] endWidth = null,
            bool closed = true)
        {
            var poly = new Polyline(locations.Length / 2);
            for (var i = 0; i < locations.Length; i += 2)
            {
                poly.AddVertexAt(poly.NumberOfVertices,
                    new Point2d(locations[i], locations[i + 1]),
                    0, 0, 0);
            }

            if (bulges != null)
            {
                foreach (var b in bulges)
                {
                    poly.SetBulgeAt(b.Item1, b.Item2);
                }
            }

            if (startWidth != null)
            {
                foreach (var s in startWidth)
                {
                    poly.SetStartWidthAt(s.Item1, s.Item2);
                }
            }

            if (endWidth != null)
            {
                foreach (var e in endWidth)
                {
                    poly.SetEndWidthAt(e.Item1, e.Item2);
                }
            }

            poly.Closed = closed;

            return poly;
        }

    }
}
