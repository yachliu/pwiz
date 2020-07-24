﻿namespace pwiz.Skyline.SettingsUI
{
    partial class DefineExtractedMetadataDlg
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.panelButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.lblName = new System.Windows.Forms.Label();
            this.tbxRuleName = new System.Windows.Forms.TextBox();
            this.tbxRegularExpression = new System.Windows.Forms.TextBox();
            this.lblTarget = new System.Windows.Forms.Label();
            this.comboMetadataTarget = new System.Windows.Forms.ComboBox();
            this.lblSourceText = new System.Windows.Forms.Label();
            this.comboSourceText = new System.Windows.Forms.ComboBox();
            this.lblPreview = new System.Windows.Forms.Label();
            this.linkLabelRegularExpression = new System.Windows.Forms.LinkLabel();
            this.bindingListSource1 = new pwiz.Common.DataBinding.Controls.BindingListSource(this.components);
            this.boundDataGridView1 = new pwiz.Common.DataBinding.Controls.BoundDataGridView();
            this.panelButtons.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bindingListSource1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.boundDataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // panelButtons
            // 
            this.panelButtons.Controls.Add(this.btnCancel);
            this.panelButtons.Controls.Add(this.btnOK);
            this.panelButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelButtons.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.panelButtons.Location = new System.Drawing.Point(0, 421);
            this.panelButtons.Name = "panelButtons";
            this.panelButtons.Size = new System.Drawing.Size(583, 29);
            this.panelButtons.TabIndex = 9;
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.btnCancel.Location = new System.Drawing.Point(505, 3);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.btnOK.Location = new System.Drawing.Point(424, 3);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 0;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            // 
            // lblName
            // 
            this.lblName.AutoSize = true;
            this.lblName.Location = new System.Drawing.Point(12, 9);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(70, 13);
            this.lblName.TabIndex = 0;
            this.lblName.Text = "Name of rule:";
            // 
            // tbxRuleName
            // 
            this.tbxRuleName.Location = new System.Drawing.Point(12, 25);
            this.tbxRuleName.Name = "tbxRuleName";
            this.tbxRuleName.Size = new System.Drawing.Size(166, 20);
            this.tbxRuleName.TabIndex = 1;
            // 
            // tbxRegularExpression
            // 
            this.tbxRegularExpression.Location = new System.Drawing.Point(12, 115);
            this.tbxRegularExpression.Name = "tbxRegularExpression";
            this.tbxRegularExpression.Size = new System.Drawing.Size(268, 20);
            this.tbxRegularExpression.TabIndex = 5;
            this.tbxRegularExpression.Leave += new System.EventHandler(this.tbxRegularExpression_Leave);
            // 
            // lblTarget
            // 
            this.lblTarget.AutoSize = true;
            this.lblTarget.Location = new System.Drawing.Point(12, 139);
            this.lblTarget.Name = "lblTarget";
            this.lblTarget.Size = new System.Drawing.Size(41, 13);
            this.lblTarget.TabIndex = 6;
            this.lblTarget.Text = "Target:";
            // 
            // comboMetadataTarget
            // 
            this.comboMetadataTarget.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboMetadataTarget.FormattingEnabled = true;
            this.comboMetadataTarget.Location = new System.Drawing.Point(12, 155);
            this.comboMetadataTarget.Name = "comboMetadataTarget";
            this.comboMetadataTarget.Size = new System.Drawing.Size(268, 21);
            this.comboMetadataTarget.TabIndex = 7;
            this.comboMetadataTarget.SelectedIndexChanged += new System.EventHandler(this.comboMetadataTarget_SelectedIndexChanged);
            // 
            // lblSourceText
            // 
            this.lblSourceText.AutoSize = true;
            this.lblSourceText.Location = new System.Drawing.Point(12, 48);
            this.lblSourceText.Name = "lblSourceText";
            this.lblSourceText.Size = new System.Drawing.Size(73, 13);
            this.lblSourceText.TabIndex = 2;
            this.lblSourceText.Text = "Source value:";
            // 
            // comboSourceText
            // 
            this.comboSourceText.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboSourceText.FormattingEnabled = true;
            this.comboSourceText.Location = new System.Drawing.Point(12, 64);
            this.comboSourceText.Name = "comboSourceText";
            this.comboSourceText.Size = new System.Drawing.Size(268, 21);
            this.comboSourceText.TabIndex = 3;
            this.comboSourceText.SelectedIndexChanged += new System.EventHandler(this.comboSourceText_SelectedIndexChanged);
            // 
            // lblPreview
            // 
            this.lblPreview.AutoSize = true;
            this.lblPreview.Location = new System.Drawing.Point(14, 181);
            this.lblPreview.Name = "lblPreview";
            this.lblPreview.Size = new System.Drawing.Size(48, 13);
            this.lblPreview.TabIndex = 10;
            this.lblPreview.Text = "Preview:";
            // 
            // linkLabelRegularExpression
            // 
            this.linkLabelRegularExpression.AutoSize = true;
            this.linkLabelRegularExpression.Location = new System.Drawing.Point(14, 99);
            this.linkLabelRegularExpression.Name = "linkLabelRegularExpression";
            this.linkLabelRegularExpression.Size = new System.Drawing.Size(100, 13);
            this.linkLabelRegularExpression.TabIndex = 11;
            this.linkLabelRegularExpression.TabStop = true;
            this.linkLabelRegularExpression.Text = "Regular expression:";
            this.linkLabelRegularExpression.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelRegularExpression_LinkClicked);
            // 
            // bindingListSource1
            // 
            this.bindingListSource1.NewRowHandler = null;
            // 
            // boundDataGridView1
            // 
            this.boundDataGridView1.AutoGenerateColumns = false;
            this.boundDataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.boundDataGridView1.DataSource = this.bindingListSource1;
            this.boundDataGridView1.Location = new System.Drawing.Point(12, 197);
            this.boundDataGridView1.MaximumColumnCount = 2000;
            this.boundDataGridView1.Name = "boundDataGridView1";
            this.boundDataGridView1.Size = new System.Drawing.Size(551, 208);
            this.boundDataGridView1.TabIndex = 12;
            this.boundDataGridView1.CellErrorTextNeeded += new System.Windows.Forms.DataGridViewCellErrorTextNeededEventHandler(this.dataGridView1_CellErrorTextNeeded);
            // 
            // DefineExtractedMetadataDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(583, 450);
            this.Controls.Add(this.boundDataGridView1);
            this.Controls.Add(this.linkLabelRegularExpression);
            this.Controls.Add(this.lblPreview);
            this.Controls.Add(this.comboSourceText);
            this.Controls.Add(this.lblSourceText);
            this.Controls.Add(this.comboMetadataTarget);
            this.Controls.Add(this.lblTarget);
            this.Controls.Add(this.tbxRegularExpression);
            this.Controls.Add(this.tbxRuleName);
            this.Controls.Add(this.lblName);
            this.Controls.Add(this.panelButtons);
            this.Name = "DefineExtractedMetadataDlg";
            this.Text = "Result File Metadata Rule";
            this.panelButtons.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.bindingListSource1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.boundDataGridView1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.FlowLayoutPanel panelButtons;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.TextBox tbxRuleName;
        private System.Windows.Forms.TextBox tbxRegularExpression;
        private System.Windows.Forms.Label lblTarget;
        private System.Windows.Forms.ComboBox comboMetadataTarget;
        private System.Windows.Forms.Label lblSourceText;
        private System.Windows.Forms.ComboBox comboSourceText;
        private System.Windows.Forms.Label lblPreview;
        private System.Windows.Forms.LinkLabel linkLabelRegularExpression;
        private Common.DataBinding.Controls.BindingListSource bindingListSource1;
        private Common.DataBinding.Controls.BoundDataGridView boundDataGridView1;
    }
}