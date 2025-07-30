using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using System.Timers;
using Autodesk.AutoCAD.Windows;
using System.Collections.Generic;
using System;
using Autodesk.AutoCAD.Internal;
using System.Linq;

namespace ManholePlugin
{
    public class ManholeCommands : IExtensionApplication
    {
        private static System.Timers.Timer recalcTimer = null;
        private static bool isSelfUpdating = false;

        private static PaletteSet _paletteSet;
        private static ManholePalette _manholePalette;

        [CommandMethod("MHPALETTE")]
        public void ShowManholePalette()
        {
            if (_paletteSet == null)
            {
                _manholePalette = new ManholePalette();
                _paletteSet = new PaletteSet("Straight Manhole Calculator")
                {
                    DockEnabled = DockSides.Left | DockSides.Right
                };
                _paletteSet.Add("Calculator", _manholePalette);
            }
            _paletteSet.Visible = true;
        }
        public static BlockReference InsertBlockAndReturn(Transaction tr, string blockName, Editor ed)
        {
            Database db = HostApplicationServices.WorkingDatabase;
            BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
            BlockTableRecord ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

            // Check if already inserted
            foreach (ObjectId id in ms)
            {
                if (tr.GetObject(id, OpenMode.ForRead) is BlockReference existingBr)
                {
                    BlockTableRecord brDef = (BlockTableRecord)tr.GetObject(existingBr.BlockTableRecord, OpenMode.ForRead);
                    if (brDef.Name.Equals(blockName, StringComparison.OrdinalIgnoreCase))
                    {
                        ed.WriteMessage($"\n[Info] Block '{blockName}' already exists.");
                        return existingBr;
                    }
                }
            }

            // Check if the definition exists
            if (!bt.Has(blockName))
            {
                ed.WriteMessage($"\n[Warning] Block definition '{blockName}' not found.");
                return null;
            }

            BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[blockName], OpenMode.ForRead);
            Point3d insertPoint = new Point3d(22.6, 3.6, 0); // Hardcoded for testing

            BlockReference br = new BlockReference(insertPoint, btr.ObjectId);
            ms.AppendEntity(br);
            tr.AddNewlyCreatedDBObject(br, true);

            // Add attribute references
            foreach (ObjectId id in btr)
            {
                if (tr.GetObject(id, OpenMode.ForRead) is AttributeDefinition attDef && !attDef.Constant)
                {
                    AttributeReference attRef = new AttributeReference();
                    attRef.SetAttributeFromBlock(attDef, br.BlockTransform);
                    attRef.TextString = attDef.TextString;

                    br.AttributeCollection.AppendAttribute(attRef);
                    tr.AddNewlyCreatedDBObject(attRef, true);

                    ed.WriteMessage($"\n[DEBUG] Attribute '{attDef.Tag}' added to '{blockName}'.");
                }
            }

            ed.WriteMessage($"\n[Auto] Inserted block '{blockName}' at {insertPoint}.");
            return br;
        }
        public static void UpdateAttributeText(Transaction tr, BlockReference br, string tag, string value)
        {
            if (br == null) return;

            foreach (ObjectId attId in br.AttributeCollection)
            {
                if (tr.GetObject(attId, OpenMode.ForWrite) is AttributeReference attRef &&
                    attRef.Tag.Equals(tag, StringComparison.OrdinalIgnoreCase))
                {
                    attRef.TextString = value;
                    return;
                }
            }
        }
[CommandMethod("TEST_INSERT_ONE")]
public void TestInsertOneBlock()
{
    Document doc = Application.DocumentManager.MdiActiveDocument;
    Database db = doc.Database;
    Editor ed = doc.Editor;

    try
    {
        ed.WriteMessage("\n[TEST] Starting TEST_INSERT_ONE...");

        using (DocumentLock docLock = doc.LockDocument())
        using (Transaction tr = db.TransactionManager.StartTransaction())
        {
            BlockReference br = InsertBlockAndReturn(tr, "STRAIGHT_MANHOLE", ed);
            if (br != null)
            {
                UpdateAttributeText(tr, br, "F", "99");
                ed.WriteMessage("\n[TEST] Updated F = 99");
            }
            else
            {
                ed.WriteMessage("\n[TEST ERROR] Failed to insert or locate STRAIGHT_MANHOLE.");
            }

            tr.Commit();
        }
    }
    catch (System.Exception ex)
    {
        ed.WriteMessage($"\n[TEST EXCEPTION] {ex.Message}");
    }
}




        public void Initialize()
        {
            Application.DocumentManager.MdiActiveDocument.Database.ObjectModified += OnObjectModified;
           

        }

        




        public void Terminate()
        {
            Application.DocumentManager.MdiActiveDocument.Database.ObjectModified -= OnObjectModified;
        }

        private void OnObjectModified(object sender, ObjectEventArgs e)
        {
            if (isSelfUpdating) return;

            if (e.DBObject is AttributeReference attRef)
            {
                string tag = attRef.Tag.ToUpper();

                // Only react if it's a tag we care about
                if (tag != "A" && tag != "V" && tag != "H" && tag != "F" && tag != "ML") return;

                // Check if the modified attribute belongs to L TYPE PROFILE
                if (attRef.OwnerId.IsValid)
                {
                    using (DocumentLock docLock = Application.DocumentManager.MdiActiveDocument.LockDocument())
                    using (Transaction tr = attRef.Database.TransactionManager.StartTransaction())
                    {
                        try
                        {
                            var owner = tr.GetObject(attRef.OwnerId, OpenMode.ForRead) as BlockReference;
                            if (owner != null)
                            {
                                var btr = (BlockTableRecord)tr.GetObject(owner.BlockTableRecord, OpenMode.ForRead);
                                string blockName = btr.Name.ToUpper();

                                if (blockName == "L TYPE PROFILE")
                                {
                                    isSelfUpdating = true;
                                    SyncLTypeManholeFromProfile(tr, owner); // 🆕 Copy A, F, H, ML, V
                                }
                            }
                            tr.Commit();
                        }
                        finally
                        {
                            isSelfUpdating = false;
                        }
                    }
                }

                // Debounced trigger of recalculation
                if (recalcTimer != null)
                {
                    recalcTimer.Stop();
                    recalcTimer.Dispose();
                }

                recalcTimer = new System.Timers.Timer(300);
                recalcTimer.Elapsed += (s, args) =>
                {
                    recalcTimer.Stop();
                    recalcTimer.Dispose();
                    recalcTimer = null;

                    Application.DocumentManager.MdiActiveDocument.SendStringToExecute("RECALC_INTERNAL\n", true, false, false);
                };
                recalcTimer.Start();
            }
        }
private void SyncLTypeManholeFromProfile(Transaction tr, BlockReference profile)
{
    Database db = HostApplicationServices.WorkingDatabase;
    Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

    BlockReference lManhole = FindBlockByName(tr, "L TYPE MANHOLE");
    if (lManhole == null)
    {
        ed.WriteMessage("\n[Sync] 'L TYPE MANHOLE' not found.");
        return;
    }

    // Only sync H
    string hVal = GetAttributeText(tr, profile, "H");
    if (!string.IsNullOrWhiteSpace(hVal))
    {
        UpdateAttributeText(tr, lManhole, "H", hVal);
        ed.WriteMessage($"\n[Sync] H synced from PROFILE → MANHOLE: H = {hVal}");
    }
}
[CommandMethod("RUN_STRAIGHT_CALC")]
public void RunStraightManholeCalculation()
{
    _manholePalette?.CalculateStraightManhole();
}

[CommandMethod("RUN_L_CALC")]
public void RunLTypeManholeCalculation()
{
    _manholePalette?.CalculateLTypeManhole();
}


        [CommandMethod("RECALC_INTERNAL", CommandFlags.NoHistory | CommandFlags.NoUndoMarker)]
        public void InternalRecalc()
        {
            isSelfUpdating = true;

            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            ed.WriteMessage("\n[DEBUG] Starting InternalRecalc");

            try
            {
                string selectedType = _manholePalette?.manholeTypeComboBox?.Text ?? "Straight";

                switch (selectedType)
                {
                    case "Straight":
                        RecalcStraightManhole(doc, db, ed);
                        break;
                    case "L-Type":
                        RecalcLTypeManhole(doc, db, ed);
                        break;
                    case "T-Type":
                        ed.WriteMessage("\n[TODO] T-Type recalculation logic is not implemented.");
                        break;
                }
            }
            catch (System.Exception ex)
            {
                ed.WriteMessage($"\n[ERROR] Recalculation failed: {ex.Message}");
            }
            finally
            {
                ed.WriteMessage("\n[DEBUG] Finished InternalRecalc");
                isSelfUpdating = false;
            }
        }

    
        private void RecalcStraightManhole(Document doc, Database db, Editor ed)
        {
            using (DocumentLock docLock = doc.LockDocument())
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockReference manholeBlock = FindBlockByName(tr, "STRAIGHT_MANHOLE");
                BlockReference widthBlock = FindBlockByName(tr, "WIDTH CALCULATION");
                BlockReference planViewBlock = FindBlockByName(tr, "STRAIGHT PLAN VIEW");
                BlockReference lengthBlock = FindBlockByName(tr, "LENGTH CALCULATION");
                BlockReference wallJointBlock = FindBlockByName(tr, "Wall mounted Joint");
                BlockReference headroomBlock = FindBlockByName(tr, "HEADROOM CALCULATION BLOCK");

                if (wallJointBlock == null)
                    ed.WriteMessage("\n[WARNING] Wall mounted Joint block not found.");

                UpdateFromPalette(tr, widthBlock, "WC", _manholePalette?.widthInput.Text, ed);
                UpdateFromPalette(tr, planViewBlock, "R", _manholePalette?.bendingRadiusTextBox.Text, ed);
                UpdateFromPalette(tr, lengthBlock, "LCURRENT", _manholePalette?.lengthInput.Text, ed);
                UpdateFromPalette(tr, planViewBlock, "D", _manholePalette?.dTextBox.Text, ed);

                if (TryGetAttributeValue(tr, wallJointBlock, "AJOINT") == 0)
                    UpdateFromPalette(tr, wallJointBlock, "AJOINT", _manholePalette?.aTextBox.Text, ed);
                if (TryGetAttributeValue(tr, wallJointBlock, "BJOINT") == 0)
                    UpdateFromPalette(tr, wallJointBlock, "BJOINT", _manholePalette?.bTextBox.Text, ed);
                if (TryGetAttributeValue(tr, wallJointBlock, "F1") == 0)
                    UpdateFromPalette(tr, wallJointBlock, "F1", _manholePalette?.fTextBox.Text, ed);
                if (TryGetAttributeValue(tr, wallJointBlock, "F2") == 0)
                    UpdateFromPalette(tr, wallJointBlock, "F2", _manholePalette?.fTextBox.Text, ed);
                if (TryGetAttributeValue(tr, wallJointBlock, "D") == 0)
                    UpdateFromPalette(tr, wallJointBlock, "D", _manholePalette?.dheightlabel.Text, ed);

                double ajoint = TryGetAttributeValue(tr, wallJointBlock, "AJOINT");
                double bjoint = TryGetAttributeValue(tr, wallJointBlock, "BJOINT");

                double f = 0;
                string ductType = _manholePalette?.ductPositionComboBox?.Text ?? "Outer Duct";

                if (ajoint > 0 || bjoint > 0)
                {
                    f = ductType == "Inner Duct" ? ajoint : ajoint + bjoint;
                    ed.WriteMessage($"\n[Auto] F = {f} from block AJOINT/BJOINT using duct type '{ductType}'");
                }
                else
                {
                    double paletteA = TryParseSafe(_manholePalette?.aTextBox.Text);
                    double paletteB = TryParseSafe(_manholePalette?.bTextBox.Text);
                    f = ductType == "Inner Duct" ? paletteA : paletteA + paletteB;
                    ed.WriteMessage($"\n[Auto] F = {f} from palette A/B using duct type '{ductType}'");
                }

                UpdateAll(tr, ed, f.ToString("0.##"), "F", planViewBlock, manholeBlock, widthBlock);

                double w = TryGetAttributeValue(tr, widthBlock, "W");
                double ml = TryGetAttributeValue(tr, manholeBlock, "ML");
                double v = TryGetAttributeValue(tr, manholeBlock, "V");
                double a = TryGetAttributeValue(tr, manholeBlock, "A");
                double h = TryGetAttributeValue(tr, manholeBlock, "H");

                double hm = w - ml - f + 5;
                UpdateAttributeText(tr, manholeBlock, "HM", hm.ToString("0.##"));
                ed.WriteMessage($"\n[Auto] HM = {hm}");

                double e = Math.Sqrt((v * v) + (hm * hm));
                UpdateAttributeText(tr, manholeBlock, "E", e.ToString("0.##"));
                ed.WriteMessage($"\n[Auto] E = {e}");

                double resultW = a + h + f;
                double fullW = resultW * 2;
                double wc = TryParseSafe(_manholePalette?.widthInput.Text);

                if (widthBlock != null)
                {
                    UpdateAttributeText(tr, widthBlock, "W", resultW.ToString("0.##"));
                    UpdateAttributeText(tr, widthBlock, "A", a.ToString("0.##"));
                    UpdateAttributeText(tr, widthBlock, "H", h.ToString("0.##"));
                    UpdateAttributeText(tr, widthBlock, "F", f.ToString("0.##"));

                    bool passes = resultW > wc;
                    UpdateAttributeText(tr, widthBlock, ">", passes ? ">" : "<");
                    UpdateAttributeText(tr, widthBlock, "PASSES", passes ? "PASSES" : "FAILS");
                    UpdateAttributeText(tr, widthBlock, "W2", fullW.ToString("0.##"));
                    ed.WriteMessage($"\n[Auto] Width result: {resultW} (W2={fullW})");
                }

                UpdateAttributeText(tr, planViewBlock, "A", a.ToString("0.##"));
                UpdateAttributeText(tr, planViewBlock, "H", h.ToString("0.##"));

                string l1s = GetAttributeText(tr, planViewBlock, "L1");
                string l2s = GetAttributeText(tr, planViewBlock, "L2");
                string ds = GetAttributeText(tr, planViewBlock, "D");

                if (double.TryParse(l1s, out double l1) && double.TryParse(l2s, out double l2) && double.TryParse(ds, out double d))
                {
                    if (_manholePalette?.cableBellsCheckboxL1.Checked == true) l1 += 3;
                    if (_manholePalette?.cableBellsCheckboxL2.Checked == true) l2 += 3;

                    double L = l1 + l2 + d;

                    UpdateAttributeText(tr, lengthBlock, "L", L.ToString("0.##"));
                    UpdateAttributeText(tr, lengthBlock, "LTEST", L.ToString("0.##"));
                    UpdateAttributeText(tr, lengthBlock, "L1", l1.ToString("0.##"));
                    UpdateAttributeText(tr, lengthBlock, "L2", l2.ToString("0.##"));
                    UpdateAttributeText(tr, lengthBlock, "D", d.ToString("0.##"));
                    UpdateAttributeText(tr, lengthBlock, "E", e.ToString("0.##"));

                    double ltarget = TryParseSafe(_manholePalette?.lengthInput.Text);
                    bool passes = L > ltarget;
                    UpdateAttributeText(tr, lengthBlock, ">", passes ? ">" : "<");
                    UpdateAttributeText(tr, lengthBlock, "PASSES", passes ? "PASSES" : "FAILS");

                    ed.WriteMessage($"\n[Auto] Length result: L = {L}");
                }

                if (wallJointBlock != null && _manholePalette != null)
                {
                    string voltage = _manholePalette.voltageComboBox.Text;
                    string cableType = _manholePalette.cableTypeComboBox.Text;
                    string description = _manholePalette.descriptionComboBox.Text;

                    var spec = CableBracketData.GetCableSpecs().FirstOrDefault(s =>
                        s.VoltageLevel == voltage && s.CableType == cableType && s.Description == description);

                    if (spec != null)
                    {
                        double specD = spec.D;
                        double specF = spec.F;

                        int rackCount = int.TryParse(_manholePalette.rackCountInput.Text, out int rc) ? rc : 0;
                        double calculated = specD + (rackCount * specF) + 21;
                        double requiredHeadroom = Math.Max(72, calculated);

                        if (headroomBlock != null)
                        {
                            double currentHeadroom = TryParseSafe(_manholePalette.currentHeadRoomTextBox.Text);
                            UpdateAttributeText(tr, headroomBlock, "CURRENTHEADROOM", currentHeadroom.ToString("0.##"));
                            UpdateAttributeText(tr, headroomBlock, "CALCULATEDHEADROOM", requiredHeadroom.ToString("0.##"));
                            UpdateAttributeText(tr, headroomBlock, "RACKS", rackCount.ToString());
                            UpdateAttributeText(tr, headroomBlock, "D", specD.ToString("0.##"));
                            UpdateAttributeText(tr, headroomBlock, "F", specF.ToString("0.##"));
                            UpdateAttributeText(tr, headroomBlock, "HEADROOM", requiredHeadroom.ToString("0.##"));
                            string result = currentHeadroom >= requiredHeadroom ? "PASSES" : "FAILS";
                            string comparison = currentHeadroom >= requiredHeadroom ? "<" : ">";

                            UpdateAttributeText(tr, headroomBlock, "PASSES", result);
                            UpdateAttributeText(tr, headroomBlock, ">", comparison);

                            ed.WriteMessage($"\n[Auto] Headroom check: {currentHeadroom}\" vs required {requiredHeadroom}\" → {result}");
                        }
                    }
                }

                tr.Commit();
            }
        }

        private void RecalcLTypeManhole(Document doc, Database db, Editor ed)
        {
            using (DocumentLock docLock = doc.LockDocument())
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockReference manholeBlock = FindBlockByName(tr, "L TYPE MANHOLE");
                BlockReference lengthBlock = FindBlockByName(tr, "L TYPE LENGTH CALCULATION");
                BlockReference shortWidthBlock = FindBlockByName(tr, "L TYPE SHORT WIDTH CALCULATION");
                BlockReference longWidthBlock = FindBlockByName(tr, "L TYPE LONG WIDTH CALCULATION");

                if (manholeBlock == null || lengthBlock == null)
                {
                    ed.WriteMessage("\n[WARNING] L-Type blocks not found.");
                    return;
                }

                double L1 = TryGetAttributeValue(tr, manholeBlock, "L1");
                double L2 = TryGetAttributeValue(tr, manholeBlock, "L2");
                double L3 = TryGetAttributeValue(tr, manholeBlock, "L3");
                double R = TryGetAttributeValue(tr, manholeBlock, "R");
                double D = TryGetAttributeValue(tr, manholeBlock, "D");
                double B = TryGetAttributeValue(tr, manholeBlock, "B");
                double H = TryGetAttributeValue(tr, manholeBlock, "H");
                double G = TryGetAttributeValue(tr, manholeBlock, "G");
                double F = TryGetAttributeValue(tr, manholeBlock, "F");

                double X = (L3 - 1.6 * R) > 0 ? (R + (L3 - 1.6 * R)) : R;
                double L = L1 + D + X + B + H + G + F;

                UpdateAttributeText(tr, lengthBlock, "L", L.ToString("0.##"));
                ed.WriteMessage($"\n[Auto] L-Type Length = {L}");

                double sw = TryParseSafe(_manholePalette?.shortWidthInput?.Text);
                double lw = TryParseSafe(_manholePalette?.lengthInput?.Text);

                if (shortWidthBlock != null)
                    UpdateAttributeText(tr, shortWidthBlock, "W", sw.ToString("0.##"));

                if (longWidthBlock != null)
                    UpdateAttributeText(tr, longWidthBlock, "W", lw.ToString("0.##"));

                tr.Commit();
            }
        }

        private string GetAttributeText(Transaction tr, BlockReference br, string tag)
        {
            foreach (ObjectId attId in br.AttributeCollection)
            {
                if (tr.GetObject(attId, OpenMode.ForRead) is AttributeReference attRef &&
                    attRef.Tag.Equals(tag, System.StringComparison.OrdinalIgnoreCase))
                {
                    return attRef.TextString;
                }
            }
            return null;
        }

        

        private BlockReference FindBlockByName(Transaction tr, string blockName)
        {
            Database db = HostApplicationServices.WorkingDatabase;
            BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
            BlockTableRecord ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);

            foreach (ObjectId id in ms)
            {
                if (tr.GetObject(id, OpenMode.ForRead) is BlockReference br)
                {
                    BlockTableRecord brDef = (BlockTableRecord)tr.GetObject(br.BlockTableRecord, OpenMode.ForRead);
                    if (brDef.Name.Equals(blockName, StringComparison.OrdinalIgnoreCase))
                        return br;
                }
            }
            return null;
        }

        private void UpdateFromPalette(Transaction tr, BlockReference br, string tag, string input, Editor ed)
        {
            if (br == null || string.IsNullOrWhiteSpace(input)) return;
            if (double.TryParse(input, out double val))
            {
                UpdateAttributeText(tr, br, tag, val.ToString("0.##"));
                ed.WriteMessage($"\n[Auto] {tag} = {val} from palette input");
            }
        }

        private double TryGetAttributeValue(Transaction tr, BlockReference br, string tag)
        {
            string val = GetAttributeText(tr, br, tag);
            return TryParseSafe(val);
        }

        private double TryParseSafe(string val)
        {
            return double.TryParse(val, out double result) ? result : 0;
        }

        private void UpdateAll(Transaction tr, Editor ed, string value, string tag, params BlockReference[] blocks)
        {
            foreach (var br in blocks)
                if (br != null)
                    UpdateAttributeText(tr, br, tag, value);
        }
    }

    public class CableBracketSpec
    {
        public string VoltageLevel { get; set; }
        public string Description { get; set; }
        public double A { get; set; }
        public double B { get; set; }
        public double C { get; set; }
        public double D { get; set; }
        public double E { get; set; }
        public double F { get; set; }
        public double Bracket1Cable { get; set; }
        public double Bracket2Cables { get; set; }
        public double? Bracket3Cables { get; set; }
        public string CableType { get; set; }
        public double BendingRadius { get; set; }
        public double StraightJoint { get; set; }
        public double YJoint { get; set; }
    }

    public static class CableBracketData
    {
        public static List<CableBracketSpec> GetCableSpecs() =>
            new List<CableBracketSpec>
            {
            new CableBracketSpec { VoltageLevel = "600V", CableType = "Plastic-Insulated Secondary", Description = "Standard Manhole", A = 5, B = 9, C = 0, D = 12, E = 21, F = 12, Bracket1Cable = 13.5, Bracket2Cables = 18.5, Bracket3Cables = null, BendingRadius = 15 , StraightJoint = 24, YJoint = 24 },
            new CableBracketSpec { VoltageLevel = "600V", CableType = "Plastic-Insulated Secondary", Description = "Congested Manhole", A = 5, B = 8, C = 0, D = 12, E = 12, F = 9, Bracket1Cable = 13.5, Bracket2Cables = 16.5, Bracket3Cables = null, BendingRadius = 15,  StraightJoint = 24, YJoint = 24},
            new CableBracketSpec { VoltageLevel = "600V", CableType = "Plastic-Insulated Secondary", Description = "Secondary Network", A = 5, B = 6, C = 6, D = 12, E = 21, F = 12, Bracket1Cable = 13.5, Bracket2Cables = 16.5, Bracket3Cables = 23.5, BendingRadius = 15 ,  StraightJoint = 24, YJoint = 24},
            new CableBracketSpec { VoltageLevel = "4kV", CableType = "PL/PLJ", Description = "", A = 7, B = 9, C = 0, D = 12, E = 21, F = 12, Bracket1Cable = 13.5, Bracket2Cables = 18.5, Bracket3Cables = null, BendingRadius = 29 ,  StraightJoint = 30, YJoint = 42},
            new CableBracketSpec { VoltageLevel = "4kV", CableType = "EXL/EXLJ", Description = "", A = 7, B = 9, C = 0, D = 12, E = 21, F = 12, Bracket1Cable = 13.5, Bracket2Cables = 18.5, Bracket3Cables = null, BendingRadius = 21 ,  StraightJoint = 30, YJoint = 42},
            new CableBracketSpec { VoltageLevel = "4kV", CableType = "EXCcJ/EXFSJ", Description = "", A = 7, B = 9, C = 0, D = 12, E = 21, F = 12, Bracket1Cable = 13.5, Bracket2Cables = 18.5, Bracket3Cables = null, BendingRadius = 20 ,  StraightJoint = 30, YJoint = 42},
            new CableBracketSpec { VoltageLevel = "12.5kV", CableType = "PL/PLJ", Description = "Network Feeders", A = 8, B = 10, C = 0, D = 12, E = 21, F = 12, Bracket1Cable = 13.5, Bracket2Cables = 23.5, Bracket3Cables = null, BendingRadius = 29,  StraightJoint = 36, YJoint = 42 },
            new CableBracketSpec { VoltageLevel = "12.5kV", CableType = "EXL/EXLJ", Description = "Network Feeders", A = 8, B = 10, C = 0, D = 12, E = 21, F = 12, Bracket1Cable = 13.5, Bracket2Cables = 23.5, Bracket3Cables = null, BendingRadius = 21 ,  StraightJoint = 36, YJoint = 42 },
            new CableBracketSpec { VoltageLevel = "12.5kV", CableType = "EXCcJ/EXFSJ", Description = "Network Feeders", A = 8, B = 10, C = 0, D = 12, E = 21, F = 12, Bracket1Cable = 13.5, Bracket2Cables = 23.5, Bracket3Cables = null, BendingRadius = 20,  StraightJoint = 36, YJoint = 42  },
            new CableBracketSpec { VoltageLevel = "12.5kV", CableType = "EXL/EXLJ", Description = "All Other Circuits", A = 10.75, B = 15.5, C = 0, D = 12, E = 21, F = 12, Bracket1Cable = 13.5, Bracket2Cables = 30.5, Bracket3Cables = null, BendingRadius = 29,  StraightJoint = 36, YJoint = 42  },
            new CableBracketSpec { VoltageLevel = "12.5kV", CableType = "PL/PLJ", Description = "All Other Circuits", A = 10.75, B = 15.5, C = 0, D = 12, E = 21, F = 12, Bracket1Cable = 13.5, Bracket2Cables = 30.5, Bracket3Cables = null, BendingRadius = 21,  StraightJoint = 36, YJoint = 42  },
            new CableBracketSpec { VoltageLevel = "12.5kV", CableType = "EXCcJ/EXFSJ", Description = "All Other Circuits", A = 10.75, B = 15.5, C = 0, D = 12, E = 21, F = 12, Bracket1Cable = 13.5, Bracket2Cables = 30.5, Bracket3Cables = null, BendingRadius = 20 ,  StraightJoint = 36, YJoint = 42 },
            new CableBracketSpec { VoltageLevel = "34.5kV", CableType = "EXL/EXLJ", Description = "", A = 9, B = 11, C = 0, D = 15, E = 21, F = 15, Bracket1Cable = 13.5, Bracket2Cables = 23.5, Bracket3Cables = null, BendingRadius = 27,  StraightJoint = 42, YJoint = 48  },
            new CableBracketSpec { VoltageLevel = "34.5kV", CableType = "EXCcJ", Description = "", A = 9, B = 11, C = 0, D = 15, E = 21, F = 15, Bracket1Cable = 13.5, Bracket2Cables = 23.5, Bracket3Cables = null, BendingRadius = 22 ,  StraightJoint = 42, YJoint = 48 }
            };
    }
}
