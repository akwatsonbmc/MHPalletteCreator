namespace ManholePlugin
{
    partial class ManholePalette
    {
        private System.ComponentModel.IContainer components = null;
        public System.Windows.Forms.ComboBox voltageComboBox;
        public System.Windows.Forms.ComboBox cableTypeComboBox;
        public System.Windows.Forms.ComboBox descriptionComboBox;
        public System.Windows.Forms.ComboBox ductPositionComboBox;
        public System.Windows.Forms.CheckBox cableBellsCheckboxL1;

        public System.Windows.Forms.CheckBox cableBellsCheckboxL2;
        private System.Windows.Forms.Button calculateButton;
        private System.Windows.Forms.Button generateButton;
        private System.Windows.Forms.TextBox resultBox;

        public System.Windows.Forms.TextBox rackCountInput;

        public System.Windows.Forms.TextBox currentHeadRoomTextBox;

        public System.Windows.Forms.TextBox aTextBox;
        public System.Windows.Forms.TextBox bTextBox;

        public System.Windows.Forms.TextBox dTextBox;
        public System.Windows.Forms.TextBox bendingRadiusTextBox;

        public System.Windows.Forms.TextBox widthInput;

        public System.Windows.Forms.TextBox lengthInput;
        private System.Windows.Forms.TextBox hInput;
        private System.Windows.Forms.TextBox fInput;
        private System.Windows.Forms.TextBox wInput;

        private System.Windows.Forms.TextBox v1Input;
        private System.Windows.Forms.TextBox hm1Input;
        private System.Windows.Forms.TextBox e1Input;
        private System.Windows.Forms.TextBox l1Input;

        private System.Windows.Forms.TextBox v2Input;
        private System.Windows.Forms.TextBox hm2Input;
        private System.Windows.Forms.TextBox e2Input;
        private System.Windows.Forms.TextBox l2Input;

        private System.Windows.Forms.TextBox lTotalInput;
        private System.Windows.Forms.TextBox lExistingInput;

        private System.Windows.Forms.ComboBox jointTypeComboBox;

        public System.Windows.Forms.TextBox dheightlabel;
        public System.Windows.Forms.TextBox fTextBox;

        public System.Windows.Forms.TextBox shortWidthInput;
        public System.Windows.Forms.ComboBox manholeTypeComboBox;

        public System.Windows.Forms.CheckBox diagonalWallCheckbox;
        public System.Windows.Forms.ComboBox diagonalAngleComboBox;
        public System.Windows.Forms.CheckBox diagonalBellCheckbox;

        public System.Windows.Forms.TextBox diagonalLengthInput;
        
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            int labelWidth = 500;
            
           // int inputWidth = 120;
            int controlHeight = 40;
            int margin =10;
            int currentY = 10;

            

            this.voltageComboBox = new System.Windows.Forms.ComboBox();
            this.descriptionComboBox = new System.Windows.Forms.ComboBox();
            this.cableTypeComboBox = new System.Windows.Forms.ComboBox();
            this.ductPositionComboBox = new System.Windows.Forms.ComboBox();
            //this.manholetypeComboBox = new System.Windows.Forms.ComboBox();
            this.calculateButton = new System.Windows.Forms.Button();
            this.resultBox = new System.Windows.Forms.TextBox();
            this.cableBellsCheckboxL1 = new System.Windows.Forms.CheckBox();
            this.cableBellsCheckboxL2 = new System.Windows.Forms.CheckBox();
            this.generateButton = new System.Windows.Forms.Button();
            this.aTextBox = new System.Windows.Forms.TextBox();
            this.aTextBox = new System.Windows.Forms.TextBox();
            this.bTextBox = new System.Windows.Forms.TextBox();
            this.bendingRadiusTextBox = new System.Windows.Forms.TextBox();
            this.hInput = new System.Windows.Forms.TextBox();
            this.wInput = new System.Windows.Forms.TextBox();
            this.v1Input = new System.Windows.Forms.TextBox();
            this.hm1Input = new System.Windows.Forms.TextBox();
            this.e1Input = new System.Windows.Forms.TextBox();
            this.l1Input = new System.Windows.Forms.TextBox();
            this.v2Input = new System.Windows.Forms.TextBox();
            this.hm2Input = new System.Windows.Forms.TextBox();
            this.e2Input = new System.Windows.Forms.TextBox();
            this.l2Input = new System.Windows.Forms.TextBox();
            this.lTotalInput = new System.Windows.Forms.TextBox();
            this.lExistingInput = new System.Windows.Forms.TextBox();
            this.currentHeadRoomTextBox = new System.Windows.Forms.TextBox();
            this.shortWidthInput = new System.Windows.Forms.TextBox();
            this.manholeTypeComboBox = new System.Windows.Forms.ComboBox();
            this.jointTypeComboBox = new System.Windows.Forms.ComboBox();



            this.SuspendLayout();
            // 
            // voltageComboBox
            // 
            System.Windows.Forms.Label voltageLabel = new System.Windows.Forms.Label();
            voltageLabel.Text = "Voltage";
            voltageLabel.Location = new System.Drawing.Point(margin, currentY);
            voltageLabel.Size = new System.Drawing.Size(labelWidth, controlHeight);
            this.Controls.Add(voltageLabel);
            currentY += controlHeight;

            this.voltageComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.voltageComboBox.Location = new System.Drawing.Point(margin, currentY);
            this.voltageComboBox.Size = new System.Drawing.Size(labelWidth, controlHeight);
            this.voltageComboBox.SelectedIndexChanged += new System.EventHandler(this.voltageComboBox_SelectedIndexChanged);
            currentY += controlHeight;
            // 
            // cableTypeComboBox
            // 
            System.Windows.Forms.Label cableTypeLabel = new System.Windows.Forms.Label();
            cableTypeLabel.Text = "Cable Type";
            cableTypeLabel.Location = new System.Drawing.Point(margin, currentY);
            cableTypeLabel.Size = new System.Drawing.Size(labelWidth, controlHeight);
            this.Controls.Add(cableTypeLabel);
            currentY += controlHeight;

            this.cableTypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cableTypeComboBox.Location = new System.Drawing.Point(margin, currentY);
            this.cableTypeComboBox.Size = new System.Drawing.Size(labelWidth, controlHeight);
            this.cableTypeComboBox.SelectedIndexChanged += new System.EventHandler(this.cableTypeComboBox_SelectedIndexChanged);
            currentY += controlHeight;
            // 
            // descriptionComboBox
            //  
            System.Windows.Forms.Label descriptionLabel = new System.Windows.Forms.Label();
            descriptionLabel.Text = "Description";
            descriptionLabel.Location = new System.Drawing.Point(margin, currentY);
            descriptionLabel.Size = new System.Drawing.Size(labelWidth, controlHeight);
            this.Controls.Add(descriptionLabel);
            currentY += controlHeight;

            this.descriptionComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.descriptionComboBox.Location = new System.Drawing.Point(margin, currentY);
            this.descriptionComboBox.Size = new System.Drawing.Size(labelWidth, controlHeight);
            currentY += controlHeight;
            // 
            // ductPositionComboBox
            // 
            System.Windows.Forms.Label ductLabel = new System.Windows.Forms.Label();
            ductLabel.Text = "Inner or Outer Cable on Rack";
            ductLabel.Location = new System.Drawing.Point(margin, currentY);
            ductLabel.Size = new System.Drawing.Size(labelWidth, controlHeight);
            this.Controls.Add(ductLabel);
            currentY += controlHeight;

            this.ductPositionComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ductPositionComboBox.Location = new System.Drawing.Point(margin, currentY);
            this.ductPositionComboBox.Size = new System.Drawing.Size(labelWidth, controlHeight);
            currentY += controlHeight;

                    System.Windows.Forms.Label manholeTypeLabel = new System.Windows.Forms.Label();
            manholeTypeLabel.Text = "Manhole Type";
            manholeTypeLabel.Location = new System.Drawing.Point(margin, currentY);
            manholeTypeLabel.Size = new System.Drawing.Size(labelWidth, controlHeight);
            this.Controls.Add(manholeTypeLabel);
            currentY += controlHeight;

            this.manholeTypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.manholeTypeComboBox.Location = new System.Drawing.Point(margin, currentY);
            this.manholeTypeComboBox.Size = new System.Drawing.Size(labelWidth, controlHeight);
            this.manholeTypeComboBox.Items.AddRange(new object[] { "Straight", "L-Type", "T-Type" });
            this.manholeTypeComboBox.SelectedIndex = 0;
            this.Controls.Add(this.manholeTypeComboBox);
            currentY += controlHeight;

            // SHORT WIDTH INPUT
            System.Windows.Forms.Label shortWidthLabel = new System.Windows.Forms.Label();
            shortWidthLabel.Text = "Short Width (L-Type)";
            shortWidthLabel.Location = new System.Drawing.Point(margin, currentY);
            shortWidthLabel.Size = new System.Drawing.Size(labelWidth, controlHeight);
            this.Controls.Add(shortWidthLabel);
            currentY += controlHeight;

            this.shortWidthInput.Location = new System.Drawing.Point(margin, currentY);
            this.shortWidthInput.Size = new System.Drawing.Size(labelWidth, controlHeight);
            this.shortWidthInput.Text = "0.0";
            this.Controls.Add(this.shortWidthInput);
            currentY += controlHeight;

            System.Windows.Forms.Label widthLabel = new System.Windows.Forms.Label();
            widthLabel.Text = "CURRENT WIDTH";
            widthLabel.Location = new System.Drawing.Point(margin, currentY);
            widthLabel.Size = new System.Drawing.Size(labelWidth, controlHeight);
            this.Controls.Add(widthLabel);
            currentY += controlHeight;

            this.widthInput = new System.Windows.Forms.TextBox();
            this.widthInput.Location = new System.Drawing.Point(margin, currentY);
            this.widthInput.Size = new System.Drawing.Size(labelWidth, controlHeight);
            this.widthInput.Text = "0.0";
            currentY += controlHeight;

            System.Windows.Forms.Label lengthLabel = new System.Windows.Forms.Label();
            lengthLabel.Text = "CURRENT LENGTH";
            lengthLabel.Location = new System.Drawing.Point(margin, currentY);
            lengthLabel.Size = new System.Drawing.Size(labelWidth, controlHeight);
            this.Controls.Add(lengthLabel);
            currentY += controlHeight;

            this.lengthInput = new System.Windows.Forms.TextBox();
            this.lengthInput.Location = new System.Drawing.Point(margin, currentY);
            this.lengthInput.Size = new System.Drawing.Size(labelWidth, controlHeight);
            this.lengthInput.Text = "0.0";
            currentY += controlHeight;

            System.Windows.Forms.Label headRoomLabel = new System.Windows.Forms.Label();
            headRoomLabel.Text = "CURRENT HEADROOM";
            headRoomLabel.Location = new System.Drawing.Point(margin, currentY);
            headRoomLabel.Size = new System.Drawing.Size(labelWidth, controlHeight);
            this.Controls.Add(headRoomLabel);
            currentY += controlHeight;  

            this.currentHeadRoomTextBox = new System.Windows.Forms.TextBox();
            this.currentHeadRoomTextBox.Location = new System.Drawing.Point(margin, currentY);
            this.currentHeadRoomTextBox.Size = new System.Drawing.Size(labelWidth, controlHeight);
            this.currentHeadRoomTextBox.Text = "0.0";
            this.Controls.Add(this.currentHeadRoomTextBox);
            currentY += controlHeight;  

            System.Windows.Forms.Label rackCountLabel = new System.Windows.Forms.Label();
rackCountLabel.Text = "Number of Racks on Wall";
rackCountLabel.Location = new System.Drawing.Point(margin, currentY);
rackCountLabel.Size = new System.Drawing.Size(labelWidth, controlHeight);
this.Controls.Add(rackCountLabel);
currentY += controlHeight;

this.rackCountInput = new System.Windows.Forms.TextBox();
this.rackCountInput.Location = new System.Drawing.Point(margin, currentY);
this.rackCountInput.Size = new System.Drawing.Size(labelWidth, controlHeight);
this.rackCountInput.Text = "3";  // Default value
this.Controls.Add(this.rackCountInput);
currentY += controlHeight;


            this.cableBellsCheckboxL1.Location = new System.Drawing.Point(margin, currentY);
            this.cableBellsCheckboxL1.Size = new System.Drawing.Size(labelWidth, controlHeight);
            this.cableBellsCheckboxL1.Text = "Cable Bells Present on L1";
            this.cableBellsCheckboxL1.Checked = true;
            currentY += controlHeight;

            this.cableBellsCheckboxL2.Location = new System.Drawing.Point(margin, currentY);
            this.cableBellsCheckboxL2.Size = new System.Drawing.Size(labelWidth, controlHeight);
            this.cableBellsCheckboxL2.Text = "Cable Bells Present on L2";
            this.cableBellsCheckboxL2.Checked = true;
            currentY += controlHeight;

            this.cableBellsCheckboxL1.CheckedChanged += new System.EventHandler(this.BellCheckbox_Changed);
            this.cableBellsCheckboxL2.CheckedChanged += new System.EventHandler(this.BellCheckbox_Changed);


            System.Windows.Forms.Label jointTypeLabel = new System.Windows.Forms.Label();
            jointTypeLabel.Text = "Joint Type";
            jointTypeLabel.Location = new System.Drawing.Point(margin, currentY);
            jointTypeLabel.Size = new System.Drawing.Size(labelWidth, controlHeight);
            this.Controls.Add(jointTypeLabel);
            currentY += controlHeight;
            this.jointTypeComboBox = new System.Windows.Forms.ComboBox();
            this.jointTypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.jointTypeComboBox.Location = new System.Drawing.Point(margin, currentY);
            this.jointTypeComboBox.Size = new System.Drawing.Size(labelWidth, controlHeight);
currentY += controlHeight;
           
           // Diagonal wall checkbox

this.diagonalWallCheckbox = new System.Windows.Forms.CheckBox();
this.diagonalWallCheckbox.Location = new System.Drawing.Point(margin, currentY);
this.diagonalWallCheckbox.Size = new System.Drawing.Size(labelWidth, controlHeight);
this.diagonalWallCheckbox.Text = "Enable diagonal wall logic";
this.diagonalWallCheckbox.Checked = false;
this.Controls.Add(this.diagonalWallCheckbox);
currentY += controlHeight;

// Diagonal angle dropdown
System.Windows.Forms.Label angleLabel = new System.Windows.Forms.Label();
angleLabel.Text = "Diagonal Wall Angle A (degrees)";
angleLabel.Location = new System.Drawing.Point(margin, currentY);
angleLabel.Size = new System.Drawing.Size(labelWidth, controlHeight);
this.Controls.Add(angleLabel);
currentY += controlHeight;

this.diagonalAngleComboBox = new System.Windows.Forms.ComboBox();
this.diagonalAngleComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
this.diagonalAngleComboBox.Location = new System.Drawing.Point(margin, currentY);
this.diagonalAngleComboBox.Size = new System.Drawing.Size(labelWidth, controlHeight);
this.diagonalAngleComboBox.Items.AddRange(new object[] { "30", "45", "60", "90" });
this.diagonalAngleComboBox.SelectedIndex = 3; // Default to 90
this.Controls.Add(this.diagonalAngleComboBox);
currentY += controlHeight;

// Diagonal bells checkbox
this.diagonalBellCheckbox = new System.Windows.Forms.CheckBox();
this.diagonalBellCheckbox.Location = new System.Drawing.Point(margin, currentY);
this.diagonalBellCheckbox.Size = new System.Drawing.Size(labelWidth, controlHeight);
this.diagonalBellCheckbox.Text = "Conduit Bells Present on Diagonal End";
this.diagonalBellCheckbox.Checked = false;
this.Controls.Add(this.diagonalBellCheckbox);
currentY += controlHeight;

// Diagonal wall length input
System.Windows.Forms.Label diagWallLengthLabel = new System.Windows.Forms.Label();
diagWallLengthLabel.Text = "Current Diagonal Wall Length";
diagWallLengthLabel.Location = new System.Drawing.Point(margin, currentY);
diagWallLengthLabel.Size = new System.Drawing.Size(labelWidth, controlHeight);
this.Controls.Add(diagWallLengthLabel);
currentY += controlHeight;

this.diagonalLengthInput = new System.Windows.Forms.TextBox();
this.diagonalLengthInput.Location = new System.Drawing.Point(margin, currentY);
this.diagonalLengthInput.Size = new System.Drawing.Size(labelWidth, controlHeight);
this.diagonalLengthInput.Text = "0.0";
this.Controls.Add(this.diagonalLengthInput);
currentY += controlHeight;

      

            // 
            // calculateButton
            // 
            this.calculateButton.Location = new System.Drawing.Point(margin, currentY);
            this.calculateButton.Size = new System.Drawing.Size(labelWidth, 30);
            this.calculateButton.Text = "Calculate";
            this.calculateButton.Click += new System.EventHandler(this.calculateButton_Click);
            currentY += controlHeight;

            //
            //cableBellsCheckbox
            // 

           


            //
            // a value text box
            //
            
            System.Windows.Forms.Label pulledLabel = new System.Windows.Forms.Label();
            pulledLabel.Text = "PULLED VALUES FROM INPUTS";
            pulledLabel.Location = new System.Drawing.Point(margin, currentY);
            pulledLabel.Size = new System.Drawing.Size(labelWidth, controlHeight);
            this.Controls.Add(pulledLabel);
            currentY += controlHeight;


            System.Windows.Forms.Label aLabel = new System.Windows.Forms.Label();
            aLabel.Text = "a VALUE USED FOR HORIZONTAL RACKING DISTANCE (F CALC)";
            aLabel.Location = new System.Drawing.Point(margin, currentY);
            aLabel.Size = new System.Drawing.Size(labelWidth, controlHeight);
            this.Controls.Add(aLabel);
            currentY += controlHeight;

            this.aTextBox = new System.Windows.Forms.TextBox();
            this.aTextBox.Location = new System.Drawing.Point(margin, currentY);
            this.aTextBox.Size = new System.Drawing.Size(labelWidth, controlHeight);
            this.aTextBox.ReadOnly = true;
            this.aTextBox.Text = "0.0";
            currentY += controlHeight;

            //
            // b value text box
            //
            System.Windows.Forms.Label bLabel = new System.Windows.Forms.Label();
            bLabel.Text = "b VALUE USED FOR HORIZONTAL RACKING DISTANCE (H CALC)";
            bLabel.Location = new System.Drawing.Point(margin, currentY);
            bLabel.Size = new System.Drawing.Size(labelWidth, controlHeight);
            this.Controls.Add(bLabel);
            currentY += controlHeight;
            this.bTextBox = new System.Windows.Forms.TextBox();
            this.bTextBox.Location = new System.Drawing.Point(margin, currentY);
            this.bTextBox.Size = new System.Drawing.Size(labelWidth, controlHeight);
            this.bTextBox.ReadOnly = true;
            this.bTextBox.Text = "0.0";
            currentY += controlHeight;

            System.Windows.Forms.Label dLabel = new System.Windows.Forms.Label();
            dLabel.Text = "D VALUE USED FOR LENGTH CALCULATION C5050";
            dLabel.Location = new System.Drawing.Point(margin, currentY);
            dLabel.Size = new System.Drawing.Size(labelWidth, controlHeight);
            this.Controls.Add(dLabel);
            currentY += controlHeight;

            this.dTextBox = new System.Windows.Forms.TextBox();
            this.dTextBox.Location = new System.Drawing.Point(margin, currentY);
            this.dTextBox.Size = new System.Drawing.Size(labelWidth, controlHeight);
            this.dTextBox.ReadOnly = true;
            this.dTextBox.Text = "0.0";
            currentY += controlHeight;

            System.Windows.Forms.Label fLabel = new System.Windows.Forms.Label();
            fLabel.Text = "F VALUE USED FOR LENGTH CALCULATION";
            fLabel.Location = new System.Drawing.Point(margin, currentY);
            fLabel.Size = new System.Drawing.Size(labelWidth, controlHeight);
            this.Controls.Add(fLabel);
            currentY += controlHeight;
            
            this.fTextBox = new System.Windows.Forms.TextBox();
            this.fTextBox.Location = new System.Drawing.Point(margin, currentY);
            this.fTextBox.Size = new System.Drawing.Size(labelWidth, controlHeight);
            this.fTextBox.ReadOnly = true;
            this.fTextBox.Text = "0.0";
            this.Controls.Add(this.fTextBox);
            currentY += controlHeight;

            System.Windows.Forms.Label dheightlabel = new System.Windows.Forms.Label();
            dheightlabel.Text = "D HEIGHT USED FOR LENGTH CALCULATION";
            dheightlabel.Location = new System.Drawing.Point(margin, currentY);
            dheightlabel.Size = new System.Drawing.Size(labelWidth, controlHeight);
            this.Controls.Add(dheightlabel);
            currentY += controlHeight;

            this.dheightlabel = new System.Windows.Forms.TextBox();
            this.dheightlabel.Location = new System.Drawing.Point(margin, currentY);
            this.dheightlabel.Size = new System.Drawing.Size(labelWidth, controlHeight);
            this.dheightlabel.ReadOnly = true;
            this.dheightlabel.Text = "0.0";
            this.Controls.Add(this.dheightlabel);
            currentY += controlHeight;


            //
            // bending radius text box
            //
            System.Windows.Forms.Label bendingRadiusLabel = new System.Windows.Forms.Label();
            bendingRadiusLabel.Text = "BENDING RADIUS USED FOR LENGTH CALCULATION";
            bendingRadiusLabel.Location = new System.Drawing.Point(margin, currentY);
            bendingRadiusLabel.Size = new System.Drawing.Size(labelWidth, controlHeight);
            this.Controls.Add(bendingRadiusLabel);
            currentY += controlHeight;
            this.bendingRadiusTextBox = new System.Windows.Forms.TextBox();
            this.bendingRadiusTextBox.Location = new System.Drawing.Point(margin, currentY);
            this.bendingRadiusTextBox.Size = new System.Drawing.Size(labelWidth, controlHeight);
            this.bendingRadiusTextBox.ReadOnly = true;
            this.bendingRadiusTextBox.Text = "0.0";
            currentY += controlHeight;

            //width section
           

            
            
          


            // 
            // ManholePalette
            // 
            this.Controls.Add(this.voltageComboBox);
            this.Controls.Add(this.cableTypeComboBox);
            this.Controls.Add(this.ductPositionComboBox);
            this.Controls.Add(this.descriptionComboBox);
            this.Controls.Add(this.calculateButton);
            this.Controls.Add(this.resultBox);
            this.Controls.Add(this.cableBellsCheckboxL1);
            this.Controls.Add(this.cableBellsCheckboxL2);
            this.Controls.Add(this.generateButton);
            this.Controls.Add(this.aTextBox);
            this.Controls.Add(this.bTextBox);
            this.Controls.Add(this.aTextBox);
            this.Controls.Add(this.bTextBox);
            this.Controls.Add(this.bendingRadiusTextBox);
            this.Controls.Add(this.hInput);
            this.Controls.Add(this.v1Input);
            this.Controls.Add(this.hm1Input);
            this.Controls.Add(this.e1Input);
            this.Controls.Add(this.l1Input);
            this.Controls.Add(this.v2Input);
            this.Controls.Add(this.hm2Input);
            this.Controls.Add(this.e2Input);
            this.Controls.Add(this.l2Input);
            this.Controls.Add(this.lTotalInput);
            this.Controls.Add(this.lExistingInput);
            this.Controls.Add(this.widthInput);
            this.Controls.Add(this.lengthInput);
            this.Controls.Add(this.dTextBox);
            this.Controls.Add(this.jointTypeComboBox);
     
          
            this.Name = "ManholePalette";
            this.Size = new System.Drawing.Size(300, currentY+20);
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
