using System;
using System.Linq;
using System.Windows.Forms;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;

namespace ManholePlugin
{
    public partial class ManholePalette : UserControl
    {
        private bool isUpdating = false;

        public ManholePalette()
        {
            InitializeComponent();

            ductPositionComboBox.Items.AddRange(new[] { "Inner Duct", "Outer Duct" });
            ductPositionComboBox.SelectedIndex = 0;

            jointTypeComboBox.Items.AddRange(new[] { "Straight Joint", "Y Joint" });
            jointTypeComboBox.SelectedIndex = 0;

            manholeTypeComboBox.Items.AddRange(new[] { "Straight", "L-Type", "T-Type" });
            manholeTypeComboBox.SelectedIndex = 0;

            voltageComboBox.Items.AddRange(CableBracketData.GetCableSpecs().Select(s => s.VoltageLevel).Distinct().ToArray());

            cableBellsCheckboxL1.CheckedChanged += BellCheckbox_Changed;
            cableBellsCheckboxL2.CheckedChanged += BellCheckbox_Changed;
        }

        private void voltageComboBox_SelectedIndexChanged(object sender, EventArgs e)
{
    if (isUpdating || voltageComboBox.SelectedItem == null) return;

    isUpdating = true;
    string voltage = voltageComboBox.SelectedItem.ToString();

    var cableTypes = CableBracketData.GetCableSpecs()
        .Where(s => s.VoltageLevel == voltage)
        .Select(s => s.CableType)
        .Distinct()
        .ToArray();

    cableTypeComboBox.BeginUpdate();
    cableTypeComboBox.Items.Clear();
    cableTypeComboBox.Items.AddRange(cableTypes);
    cableTypeComboBox.EndUpdate();

    cableTypeComboBox.SelectedIndex = -1;
    descriptionComboBox.Items.Clear();
    descriptionComboBox.SelectedIndex = -1;
    isUpdating = false;
}

private void cableTypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
{
    if (isUpdating || cableTypeComboBox.SelectedItem == null || voltageComboBox.SelectedItem == null) return;

    isUpdating = true;
    string voltage = voltageComboBox.SelectedItem.ToString();
    string cableType = cableTypeComboBox.SelectedItem.ToString();

    var descriptions = CableBracketData.GetCableSpecs()
        .Where(s => s.VoltageLevel == voltage && s.CableType == cableType)
        .Select(s => s.Description)
        .Distinct()
        .ToArray();

    descriptionComboBox.BeginUpdate();
    descriptionComboBox.Items.Clear();
    descriptionComboBox.Items.AddRange(descriptions);
    descriptionComboBox.EndUpdate();
    descriptionComboBox.SelectedIndex = -1;

    isUpdating = false;
}


        private void calculateButton_Click(object sender, EventArgs e)
{
    string voltage = voltageComboBox.Text;
    string cableType = cableTypeComboBox.Text;
    string description = descriptionComboBox.Text;

    var result = CableBracketData.GetCableSpecs().FirstOrDefault(s =>
        s.VoltageLevel == voltage && s.CableType == cableType && s.Description == description);

    if (result == null)
    {
        Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor
            .WriteMessage("\n[ERROR] No matching CableBracketSpec found.");
        return;
    }

    aTextBox.Text = result.A.ToString("0.##");
    bTextBox.Text = result.B.ToString("0.##");
    fTextBox.Text = result.F.ToString("0.##");
    dheightlabel.Text = result.D.ToString("0.##");
    dTextBox.Text = jointTypeComboBox.Text == "Straight Joint"
        ? result.StraightJoint.ToString("0.##")
        : result.YJoint.ToString("0.##");
    bendingRadiusTextBox.Text = result.BendingRadius.ToString("0.##");

    // ✅ Now call calculation AFTER spec values are filled
    CalculateStraightManhole();
}

private BlockReference InsertBlockAndReturn(Transaction tr, string blockName, Editor ed)
{
    // Use the active document’s database (so the transaction context matches)
    var doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
    Database db = doc.Database;

    // Open the block table & model space record in the same database
    BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
    BlockTableRecord ms = (BlockTableRecord)tr.GetObject(
        bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

    // If the block is already in model space, return it
    foreach (ObjectId id in ms)
    {
        if (tr.GetObject(id, OpenMode.ForRead) is BlockReference existingBr)
        {
            var brDef = (BlockTableRecord)tr.GetObject(
                existingBr.BlockTableRecord, OpenMode.ForRead);
            if (brDef.Name.Equals(blockName, StringComparison.OrdinalIgnoreCase))
            {
                ed.WriteMessage($"\n[Info] Block '{blockName}' already exists.");
                return existingBr;
            }
        }
    }

    // If the definition isn’t in the drawing, warn and bail
    if (!bt.Has(blockName))
    {
        ed.WriteMessage($"\n[Warning] Block definition '{blockName}' not found.");
        return null;
    }

    // Insert a new reference
    BlockTableRecord btr = (BlockTableRecord)tr.GetObject(
        bt[blockName], OpenMode.ForRead);
    Point3d insertPoint = GetInsertionPointForBlock(blockName);

    var br = new BlockReference(insertPoint, btr.ObjectId);
    ms.AppendEntity(br);
    tr.AddNewlyCreatedDBObject(br, true);

    // Copy across any non‑constant attributes
    foreach (ObjectId id in btr)
    {
        if (tr.GetObject(id, OpenMode.ForRead) is AttributeDefinition attDef 
            && !attDef.Constant)
        {
            var attRef = new AttributeReference();
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

     public void CalculateStraightManhole()
{
    var doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
    var db = doc.Database;
    var ed = doc.Editor;

    DocumentLock docLock = null;
    Autodesk.AutoCAD.DatabaseServices.Transaction tr = null;
    try
    {
        docLock = doc.LockDocument();
        tr = db.TransactionManager.StartTransaction();

        // 1. Open BlockTable and ModelSpace ONCE
        var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
        var ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

        // 2. Define your blocks and their insertion points
        var blocksToInsert = new Dictionary<string, Point3d>
        {
            { "STRAIGHT_MANHOLE", new Point3d(22.6, 3.6, 0) },
            { "STRAIGHT PLAN VIEW", new Point3d(41.67, 3.2, 0) },
            { "WIDTH CALCULATION", new Point3d(2.693, -26.657, 0) },
            { "LENGTH CALCULATION", new Point3d(28.5212, -16.9409, 0) },
            { "Wall mounted Joint", new Point3d(76, 3.68, 0) },
            { "HEADROOM CALCULATION BLOCK", new Point3d(35.377, -24.8142, 0) }
        };

        var blockRefs = new Dictionary<string, BlockReference>();

        // 3. Insert or get each block reference
        foreach (var kvp in blocksToInsert)
        {
            string blockName = kvp.Key;
            Point3d insPt = kvp.Value;

            // Try to find an existing one first
            BlockReference found = null;
            foreach (ObjectId id in ms)
            {
                if (tr.GetObject(id, OpenMode.ForRead) is BlockReference br)
                {
                    BlockTableRecord def = (BlockTableRecord)tr.GetObject(br.BlockTableRecord, OpenMode.ForRead);
                    if (def.Name.Equals(blockName, StringComparison.OrdinalIgnoreCase))
                    {
                        found = br;
                        break;
                    }
                }
            }

            if (found != null)
            {
                blockRefs[blockName] = found;
                continue;
            }

            // Insert a new one if block definition exists
            if (!bt.Has(blockName))
            {
                ed.WriteMessage($"\n[Warning] Block '{blockName}' definition not found.");
                continue;
            }

            var btr = (BlockTableRecord)tr.GetObject(bt[blockName], OpenMode.ForRead);
            var newRef = new BlockReference(insPt, btr.ObjectId);
            ms.AppendEntity(newRef);
            tr.AddNewlyCreatedDBObject(newRef, true);

            // Copy attributes from definition
            foreach (ObjectId attId in btr)
            {
                if (tr.GetObject(attId, OpenMode.ForRead) is AttributeDefinition attDef && !attDef.Constant)
                {
                    var attRef = new AttributeReference();
                    attRef.SetAttributeFromBlock(attDef, newRef.BlockTransform);
                    attRef.TextString = attDef.TextString;
                    newRef.AttributeCollection.AppendAttribute(attRef);
                    tr.AddNewlyCreatedDBObject(attRef, true);
                }
            }
            ed.WriteMessage($"\n[Auto] Inserted '{blockName}' at {insPt}.");
            blockRefs[blockName] = newRef;
        }

        // 4. Now update attributes with your palette input values

        // Values from your palette
        double a = TryParseSafe(aTextBox.Text);
        double b = TryParseSafe(bTextBox.Text);
        double d = TryParseSafe(dTextBox.Text);
        double f = (ductPositionComboBox.Text == "Inner Duct") ? a : a + b;
        string fTxt = f.ToString("0.##");

        // Update F attribute everywhere it's needed
        foreach (var key in new[] { "STRAIGHT_MANHOLE", "STRAIGHT PLAN VIEW", "WIDTH CALCULATION" })
            if (blockRefs.TryGetValue(key, out var refBlock))
                UpdateAttributeText(tr, refBlock, "F", fTxt);

        // H on plan view (static)
        if (blockRefs.TryGetValue("STRAIGHT PLAN VIEW", out var planRef))
            UpdateAttributeText(tr, planRef, "H", "18");

        // WIDTH CALCULATION attributes
        if (blockRefs.TryGetValue("WIDTH CALCULATION", out var widthRef))
        {
            UpdateAttributeText(tr, widthRef, "A", a.ToString("0.##"));
            UpdateAttributeText(tr, widthRef, "H", "18");
            UpdateAttributeText(tr, widthRef, "W", (a + f + 18).ToString("0.##"));
        }

        // Wall mounted joint
        if (blockRefs.TryGetValue("Wall mounted Joint", out var jointRef))
        {
            UpdateAttributeText(tr, jointRef, "AJOINT", a.ToString("0.##"));
            UpdateAttributeText(tr, jointRef, "BJOINT", b.ToString("0.##"));
            UpdateAttributeText(tr, jointRef, "F1", fTxt);
            UpdateAttributeText(tr, jointRef, "F2", fTxt);
        }

        // LENGTH CALCULATION
        double baseL1 = TryParseSafe(l1Input.Text);
        double baseL2 = TryParseSafe(l2Input.Text);
        double adjL1 = baseL1 + (cableBellsCheckboxL1.Checked ? 3 : 0);
        double adjL2 = baseL2 + (cableBellsCheckboxL2.Checked ? 3 : 0);
        double totalL = adjL1 + adjL2 + d;

        if (blockRefs.TryGetValue("LENGTH CALCULATION", out var lengthRef))
        {
            UpdateAttributeText(tr, lengthRef, "L1", adjL1.ToString("0.##"));
            UpdateAttributeText(tr, lengthRef, "L2", adjL2.ToString("0.##"));
            UpdateAttributeText(tr, lengthRef, "D", d.ToString("0.##"));
            UpdateAttributeText(tr, lengthRef, "L", totalL.ToString("0.##"));
        }

        // HEADROOM CALCULATION BLOCK
        if (blockRefs.TryGetValue("HEADROOM CALCULATION BLOCK", out var headRef))
        {
            double currHR = TryParseSafe(currentHeadRoomTextBox.Text);
            int racks = int.TryParse(rackCountInput.Text, out var r) ? r : 0;
            double required = Math.Max(72, d + (racks * f) + 21);

            UpdateAttributeText(tr, headRef, "CURRENTHEADROOM", currHR.ToString("0.##"));
            UpdateAttributeText(tr, headRef, "RACKS", racks.ToString());
            UpdateAttributeText(tr, headRef, "D", d.ToString("0.##"));
            UpdateAttributeText(tr, headRef, "F", fTxt);
            UpdateAttributeText(tr, headRef, "CALCULATEDHEADROOM", required.ToString("0.##"));

            string pass = currHR >= required ? "PASSES" : "FAILS";
            string comp = currHR >= required ? "<" : ">";
            UpdateAttributeText(tr, headRef, "PASSES", pass);
            UpdateAttributeText(tr, headRef, ">", comp);
        }

        tr.Commit();
    }
    catch (Autodesk.AutoCAD.Runtime.Exception ex)
    {
        // If error occurs, write to command line
        var ed1 = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;
        ed1.WriteMessage($"\n[ERROR] {ex.ErrorStatus}: {ex.Message}");
    }
    finally
    {
        // Always dispose transaction and document lock, swallowing only InvalidContext
        try { tr?.Dispose(); }
        catch (Autodesk.AutoCAD.Runtime.Exception ex)
        {
            if (ex.ErrorStatus != Autodesk.AutoCAD.Runtime.ErrorStatus.InvalidContext)
                throw;
        }
        docLock?.Dispose();
    }
}






private void TriggerRecalcInternal()
{
    Autodesk.AutoCAD.ApplicationServices.Application.Idle += RunRecalcOnIdle;
}

private void RunRecalcOnIdle(object sender, EventArgs e)
{
    Autodesk.AutoCAD.ApplicationServices.Application.Idle -= RunRecalcOnIdle;

    var doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
    if (doc != null)
    {
        doc.SendStringToExecute("RECALC_INTERNAL\n", true, false, false);
    }
}
public void CalculateLTypeManhole()
{
 //   InsertBlockIfNotExists("L TYPE MANHOLE");
   // InsertBlockIfNotExists("L TYPE PROFILE");
   // InsertBlockIfNotExists("L TYPE LENGTH CALCULATION");
   // InsertBlockIfNotExists("L TYPE LONG WIDTH CALCULATION");
   // InsertBlockIfNotExists("L TYPE SHORT WIDTH CALCULATION");

    Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
    Database db = doc.Database;
    Editor ed = doc.Editor;

    string voltage = voltageComboBox.Text;
    string cableType = cableTypeComboBox.Text;
    string description = descriptionComboBox.Text;

    var spec = CableBracketData.GetCableSpecs().FirstOrDefault(s =>
        s.VoltageLevel == voltage && s.CableType == cableType && s.Description == description);

    if (spec == null)
    {
        MessageBox.Show("No matching CableBracketSpec found.");
        return;
    }

    using (DocumentLock docLock = doc.LockDocument())
    using (Transaction tr = db.TransactionManager.StartTransaction())
    {
        BlockReference lBlock = FindBlockByName(tr, "L TYPE MANHOLE");
        BlockReference profileBlock = FindBlockByName(tr, "L TYPE PROFILE");
        BlockReference calcBlock = FindBlockByName(tr, "L TYPE LENGTH CALCULATION");
        BlockReference longWidthBlock = FindBlockByName(tr, "L TYPE LONG WIDTH CALCULATION");
        BlockReference shortWidthBlock = FindBlockByName(tr, "L TYPE SHORT WIDTH CALCULATION");

        if (lBlock == null || calcBlock == null || profileBlock == null || longWidthBlock == null || shortWidthBlock == null)
        {
            ed.WriteMessage("\n[ERROR] Required blocks not found.");
            return;
        }

        // Push spec values into L TYPE MANHOLE
        double A = spec.A;
        double B = spec.B;
        double D = spec.D;
        double F = spec.F;
        double G = spec.E;
        double R = spec.BendingRadius;
        double H = 0;

        UpdateAttributeText(tr, lBlock, "B", B.ToString("0.##"));
        UpdateAttributeText(tr, lBlock, "D", D.ToString("0.##"));
        UpdateAttributeText(tr, lBlock, "F", F.ToString("0.##"));
        UpdateAttributeText(tr, lBlock, "G", G.ToString("0.##"));
        UpdateAttributeText(tr, lBlock, "R", R.ToString("0.##"));

        // Get H from profile and sync to MANHOLE
        string hFromProfile = GetAttributeText(tr, profileBlock, "H");
        if (!string.IsNullOrWhiteSpace(hFromProfile))
        {
            UpdateAttributeText(tr, lBlock, "H", hFromProfile);
            if (double.TryParse(hFromProfile, out double parsedH))
                H = parsedH;
        }

        // Read L1, L2, L3 and apply +3 if conduit bells are NOT present
        double L1 = TryGetAttributeValue(tr, lBlock, "L1");
        double L2 = TryGetAttributeValue(tr, lBlock, "L2");
        double L3 = TryGetAttributeValue(tr, lBlock, "L3");

        if (!cableBellsCheckboxL1.Checked) L1 += 3;
        if (!cableBellsCheckboxL2.Checked) L2 += 3;

        // Compute X
        double xOffset = L3 - 1.6 * R;
        double X = xOffset > 0 ? R + xOffset : R;

        // Compute total cable length
        double L = L1 + D + X + B + H + G + F;

        // Push to LENGTH CALCULATION block
        UpdateAttributeText(tr, calcBlock, "L1", L1.ToString("0.##"));
        UpdateAttributeText(tr, calcBlock, "L2", L2.ToString("0.##"));
        UpdateAttributeText(tr, calcBlock, "L3", L3.ToString("0.##"));
        UpdateAttributeText(tr, calcBlock, "D", D.ToString("0.##"));
        UpdateAttributeText(tr, calcBlock, "X", X.ToString("0.##"));
        UpdateAttributeText(tr, calcBlock, "B", B.ToString("0.##"));
        UpdateAttributeText(tr, calcBlock, "H", H.ToString("0.##"));
        UpdateAttributeText(tr, calcBlock, "G", G.ToString("0.##"));
        UpdateAttributeText(tr, calcBlock, "F", F.ToString("0.##"));
        UpdateAttributeText(tr, calcBlock, "L", L.ToString("0.##"));

        // Compare against palette input length
        double Lcurrent = TryParseSafe(lengthInput.Text);
        UpdateAttributeText(tr, calcBlock, "LCURRENT", Lcurrent.ToString("0.##"));
        string passFail = (L > Lcurrent) ? "PASSES" : "FAILS";
        string comparison = (L > Lcurrent) ? ">" : "<";
        UpdateAttributeText(tr, calcBlock, "PASSES", passFail);
        UpdateAttributeText(tr, calcBlock, ">", comparison);

        // PROFILE block updates
        UpdateAttributeText(tr, profileBlock, "A", A.ToString("0.##"));
        UpdateAttributeText(tr, profileBlock, "F", F.ToString("0.##"));

        // Compute HM and E
        double ML = TryGetAttributeValue(tr, profileBlock, "ML");
        double V = TryGetAttributeValue(tr, profileBlock, "V");
        double HM = TryParseSafe(widthInput.Text) - ML - F + 5;
        double E = Math.Sqrt((V * V) + (HM * HM));

        UpdateAttributeText(tr, profileBlock, "HM", HM.ToString("0.##"));
        UpdateAttributeText(tr, profileBlock, "E", E.ToString("0.##"));

        // LONG WIDTH CALCULATION
        double W = A + H + F;
        double W2 = 2 * W;
        double Wtarget = TryParseSafe(widthInput.Text);
        bool wPass = W > Wtarget;

        UpdateAttributeText(tr, longWidthBlock, "W", W.ToString("0.##"));
        UpdateAttributeText(tr, longWidthBlock, "W2", W2.ToString("0.##"));
        UpdateAttributeText(tr, longWidthBlock, "A", A.ToString("0.##"));
        UpdateAttributeText(tr, longWidthBlock, "H", H.ToString("0.##"));
        UpdateAttributeText(tr, longWidthBlock, "F", F.ToString("0.##"));
        UpdateAttributeText(tr, longWidthBlock, ">", wPass ? ">" : "<");
        UpdateAttributeText(tr, longWidthBlock, "PASSES", wPass ? "PASSES" : "FAILS");

        // SHORT WIDTH CALCULATION logic
        double origL2 = TryGetAttributeValue(tr, lBlock, "L2");
        double widthLong = TryGetAttributeValue(tr, longWidthBlock, "W");

        double WidthL1 = origL2 + R + G + F;
        double Y = R - F;
        double WidthL2 = widthLong + Y;

        UpdateAttributeText(tr, shortWidthBlock, "L2", origL2.ToString("0.##"));
        UpdateAttributeText(tr, shortWidthBlock, "R1", R.ToString("0.##"));
        UpdateAttributeText(tr, shortWidthBlock, "F", F.ToString("0.##"));
        UpdateAttributeText(tr, shortWidthBlock, "G", G.ToString("0.##"));
        UpdateAttributeText(tr, shortWidthBlock, "Y", Y.ToString("0.##"));
        UpdateAttributeText(tr, shortWidthBlock, "WIDTHL1", WidthL1.ToString("0.##"));
        UpdateAttributeText(tr, shortWidthBlock, "WIDTHL2", WidthL2.ToString("0.##"));

        double shortTarget = TryParseSafe(shortWidthInput.Text);
        string shortPass = (WidthL2 > shortTarget) ? "PASSES" : "FAILS";
        string shortComp = (WidthL2 > shortTarget) ? ">" : "<";

        UpdateAttributeText(tr, shortWidthBlock, "PASSES", shortPass);
        UpdateAttributeText(tr, shortWidthBlock, ">", shortComp);

        if (WidthL2 > WidthL1)
        {
            double delta = (WidthL2 - WidthL1) / 2;

            UpdateAttributeText(tr, shortWidthBlock, "INCREASEL2", delta.ToString("0.##"));
            UpdateAttributeText(tr, shortWidthBlock, "INCREASER1", delta.ToString("0.##"));
            UpdateAttributeText(tr, calcBlock, "INCREASEL2", delta.ToString("0.##"));
            UpdateAttributeText(tr, calcBlock, "INCREASER1", delta.ToString("0.##"));

            ed.WriteMessage($"\n[Short Width] Δ = {delta:0.##} pushed to L2 and R1.");
        }
        else
        {
            UpdateAttributeText(tr, shortWidthBlock, "INCREASEL2", "0");
            UpdateAttributeText(tr, shortWidthBlock, "INCREASER1", "0");
            UpdateAttributeText(tr, calcBlock, "INCREASEL2", "0");
            UpdateAttributeText(tr, calcBlock, "INCREASER1", "0");

            ed.WriteMessage("\n[Short Width] No adjustment needed.");
        }

        tr.Commit();
    }
}




   public void InsertBlockIfNotExists(Transaction tr, string blockName, Editor ed)
{
    Database db = HostApplicationServices.WorkingDatabase;

    BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
    BlockTableRecord ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

    // Check if the block is already inserted in model space
    bool exists = ms.Cast<ObjectId>()
        .Select(id => tr.GetObject(id, OpenMode.ForRead))
        .OfType<BlockReference>()
        .Any(existingBr =>
        {
            BlockTableRecord brDef = (BlockTableRecord)tr.GetObject(existingBr.BlockTableRecord, OpenMode.ForRead);
            return brDef.Name.Equals(blockName, StringComparison.OrdinalIgnoreCase);
        });

    if (exists)
    {
        ed.WriteMessage($"\n[Info] Block '{blockName}' already exists. Skipping insert.");
        return;
    }

    // Check if the block definition exists in the drawing
    if (!bt.Has(blockName))
    {
        ed.WriteMessage($"\n[Warning] Block definition '{blockName}' not found in drawing.");
        return;
    }

    BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[blockName], OpenMode.ForRead);
    Point3d insertPoint = GetInsertionPointForBlock(blockName);

    BlockReference br = new BlockReference(insertPoint, btr.ObjectId);
    ms.AppendEntity(br);
    tr.AddNewlyCreatedDBObject(br, true);

    foreach (ObjectId id in btr)
    {
        if (tr.GetObject(id, OpenMode.ForRead) is AttributeDefinition attDef && !attDef.Constant)
        {
            AttributeReference attRef = new AttributeReference();
            attRef.SetAttributeFromBlock(attDef, br.BlockTransform);
            attRef.TextString = attDef.TextString;
            br.AttributeCollection.AppendAttribute(attRef);
            tr.AddNewlyCreatedDBObject(attRef, true);
        }
    }

    ed.WriteMessage($"\n[Auto] Inserted block '{blockName}' at {insertPoint}.");
}




private Point3d GetInsertionPointForBlock(string blockName)
{
    switch (blockName.ToUpper())
    {
        // 🟦 Straight Manhole Blocks
        case "STRAIGHT_MANHOLE": return new Point3d(22.6, 3.6, 0);
        case "STRAIGHT PLAN VIEW": return new Point3d(41.67, 3.2, 0);
        case "WIDTH CALCULATION": return new Point3d(2.693, -26.6570, 0);
        case "LENGTH CALCULATION": return new Point3d(28.5212, -16.9409, 0);
        case "HEADROOM CALCULATION BLOCK": return new Point3d(35.377, -24.8142, 0);
        case "WALL MOUNTED JOINT": return new Point3d(76, 3.68, 0);
        case "POCKET": return new Point3d(200, -400, 0);
        case "STRAIGHT DIAGONAL": return new Point3d(0, -600, 0);
        case "STRAIGHT DIAGONAL CALCULATION": return new Point3d(35.3093, -37.7859, 0);

        // 🟩 L-Type Manhole Blocks
        case "L TYPE MANHOLE": return new Point3d(0, -800, 0);
        case "L TYPE PROFILE": return new Point3d(200, -800, 0);
        case "L TYPE LENGTH CALCULATION": return new Point3d(0, -1000, 0);
        case "L TYPE LONG WIDTH CALCULATION": return new Point3d(200, -1000, 0);
        case "L TYPE SHORT WIDTH CALCULATION": return new Point3d(400, -1000, 0);

       }

    // ✅ Handle dynamic bending radius blocks like BR15, BR20, etc.
    if (blockName.ToUpper().StartsWith("BR"))
    {
        return new Point3d(600, 0, 0); // or any default location you want
    }

    // Default fallback if unrecognized
    return Point3d.Origin;
}


      


       private BlockReference FindBlockByName(Transaction tr, string blockName)
{
    // Match the database used by the active document
    var doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
    Database db = doc.Database;

    BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
    BlockTableRecord ms = (BlockTableRecord)tr.GetObject(
        bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);

    // Search model space for a reference of the given name
    foreach (ObjectId id in ms)
    {
        if (tr.GetObject(id, OpenMode.ForRead) is BlockReference br)
        {
            var brDef = (BlockTableRecord)tr.GetObject(
                br.BlockTableRecord, OpenMode.ForRead);
            if (brDef.Name.Equals(blockName, StringComparison.OrdinalIgnoreCase))
                return br;
        }
    }

    // Not found
    return null;
}












        private double TryGetAttributeValue(Transaction tr, BlockReference br, string tag)
        {
            string val = GetAttributeText(tr, br, tag);
            return double.TryParse(val, out double result) ? result : 0;
        }

        private string GetAttributeText(Transaction tr, BlockReference br, string tag)
        {
            foreach (ObjectId attId in br.AttributeCollection)
            {
                if (tr.GetObject(attId, OpenMode.ForRead) is AttributeReference attRef &&
                    attRef.Tag.Equals(tag, StringComparison.OrdinalIgnoreCase))
                {
                    return attRef.TextString;
                }
            }
            return null;
        }

        private void UpdateAttributeText(Transaction tr, BlockReference br, string tag, string value)
        {
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

        private void BellCheckbox_Changed(object sender, EventArgs e)
        {
            RecalculateAdjustedLengths();
        }

        private void RecalculateAdjustedLengths()
        {
            double l1 = TryParseSafe(l1Input.Text);
            double l2 = TryParseSafe(l2Input.Text);

            if (cableBellsCheckboxL1.Checked) l1 += 3;
            if (cableBellsCheckboxL2.Checked) l2 += 3;

            l1Input.Text = l1.ToString("0.##");
            l2Input.Text = l2.ToString("0.##");

            double d = TryParseSafe(dTextBox.Text);
            double total = l1 + l2 + d;
            lTotalInput.Text = total.ToString("0.##");
        }

        private double TryParseSafe(string s)
        {
            return double.TryParse(s, out double val) ? val : 0;
        }
    }
}
