using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB.Structure;


namespace Question2_2020
{
    [Journaling(JournalingMode.NoCommandData), Regeneration(RegenerationOption.Manual), Transaction(TransactionMode.Manual)]
    class SplitFloor : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            Floor MasterFloor;

            try
            {
                MasterFloor = doc.GetElement(uidoc.Selection.PickObject(ObjectType.Element, new SelectionFloor(),"Selection Master Floor").ElementId) as Floor;
            }
            catch
            {
                return Result.Cancelled;
            }

            FloorType MasterFloorType = MasterFloor.FloorType;
            Level MasterFloorLevel = doc.GetElement(MasterFloor.LevelId) as Level;
            bool MasterFloorStructural;
            if(MasterFloor.LookupParameter("Structural").AsDouble() == 0)
            {
                MasterFloorStructural = false;
            }
            else
            {
                MasterFloorStructural = true;
            }

            // Get List DB.Line from Floor
            ElementClassFilter filter = new ElementClassFilter(typeof(Sketch));
            ElementId sketchId = MasterFloor.GetDependentElements(filter).First();
            Sketch floorSketch = doc.GetElement(sketchId) as Sketch;
            CurveArray floorProfile = floorSketch.Profile.get_Item(0);
            List<Line> listcurve = new List<Line>();
            for(int i=0;i<floorProfile.Size;i++)
            {
                listcurve.Add(floorProfile.get_Item(i) as Line);
            }

            // Filter the lines parallel to the X axis,Y axis
            List<Line> ListLineX = new List<Line>(); // list line parallel to the X axis
            List<Line> ListLineY = new List<Line>(); // list line parallel to the Y axis
            foreach (Line l in listcurve)
            {
                if(l.Direction.DotProduct(new XYZ(1,0,0)) == -1 || l.Direction.DotProduct(new XYZ(1, 0, 0)) == 1)
                {
                    ListLineX.Add(l);
                }
                else
                {
                    ListLineY.Add(l);
                }
            }

            // Get points on the cut line
            IEnumerable<Line> LineXGroupByY = ListLineX.GroupBy(l =>Math.Round(l.GetEndPoint(0).Y)).Select(grb => grb.ToList().First());
            List<XYZ> ListpointX = new List<XYZ>();
            foreach(Line l in LineXGroupByY)
            {
                ListpointX.Add(l.GetEndPoint(0));
            }
            List<XYZ> ListpointXsortY = ListpointX.OrderBy(P => P.Y).ToList();


            // move list point X sort Y 
            List<XYZ> Listpointcutcheck = new List<XYZ>();
            foreach (XYZ p in ListpointXsortY)
            {
                Listpointcutcheck.Add(new XYZ(p.X, p.Y + 0.000001, p.Z));
            }

            // list boubdary sub floor
            List<CurveArray> ListBoundary = new List<CurveArray>();

            for (int n = 0; n< ListpointXsortY.Count() - 1; n++)
            {
                //find the lines that the clipping points see
                XYZ PointCutCheck = Listpointcutcheck[n];
                List<Line> LineYcut = new List<Line>();
                foreach (Line l in ListLineY)
                {
                    XYZ PminY = new XYZ();
                    XYZ PmaxY = new XYZ();
                    if (l.GetEndPoint(0).Y > l.GetEndPoint(1).Y)
                    {
                        PminY = l.GetEndPoint(1);
                        PmaxY = l.GetEndPoint(0);
                    }
                    else
                    {
                        PminY = l.GetEndPoint(0);
                        PmaxY = l.GetEndPoint(1);
                    }
                    if (PointCutCheck.Y > PminY.Y & PointCutCheck.Y < PmaxY.Y)
                    {
                        LineYcut.Add(l);
                    }
                }

                // get rectangular bottom line
                XYZ PointCut = ListpointXsortY[n];
                List<XYZ> RecBottomP = new List<XYZ>();
                foreach (Line lineY in LineYcut)
                {
                    RecBottomP.Add(lineY.Project(PointCut).XYZPoint);
                }

                List<XYZ> RecBottomPSort = RecBottomP.OrderBy(p => p.X).ToList();

                List<Line> RecButtomLines = new List<Line>();
                for (int i = 0; i < (RecBottomPSort.Count() / 2); i++)
                {
                    RecButtomLines.Add(Line.CreateBound(RecBottomPSort[i * 2], RecBottomPSort[i * 2 + 1]));
                }

                // create boundrary sub floor
                XYZ PointTop = ListpointXsortY[n+1];
                foreach (Line lineButtom in RecButtomLines)
                {
                    XYZ PButoomStart = lineButtom.GetEndPoint(0);
                    XYZ PButoomEnd = lineButtom.GetEndPoint(1);
                    XYZ PTopStrart = new XYZ(PButoomEnd.X, PointTop.Y, PointTop.Z);
                    XYZ PTopEnd = new XYZ(PButoomStart.X, PointTop.Y, PointTop.Z);

                    Line lineside1 = Line.CreateBound(PButoomEnd, PTopStrart);
                    Line linetop = Line.CreateBound(PTopStrart, PTopEnd);
                    Line lineside2 = Line.CreateBound(PTopEnd, PButoomStart);

                    CurveArray Boundary = new CurveArray();
                    Boundary.Append(lineButtom);
                    Boundary.Append(lineside1);
                    Boundary.Append(linetop);
                    Boundary.Append(lineside2);

                    ListBoundary.Add(Boundary);
                }
            }


            Transaction trans = new Transaction(doc);
            trans.Start("Split Floor");

            // create sub floors and delete master floor
            foreach (CurveArray bondary in ListBoundary)
            {
                doc.Create.NewFloor(bondary, MasterFloorType, MasterFloorLevel, MasterFloorStructural);
            }

            doc.Delete(MasterFloor.Id);

            trans.Commit();
            return Result.Succeeded;
        }
    }

    class SelectionFloor : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            if (elem.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Floors)
            {
                return true;
            }
            return false;

        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }
}
