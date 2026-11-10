/* Copyright (c) 2006-2008, Peter Golde
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without 
 * modification, are permitted provided that the following conditions are 
 * met:
 * 
 * 1. Redistributions of source code must retain the above copyright
 * notice, this list of conditions and the following disclaimer.
 * 
 * 2. Redistributions in binary form must reproduce the above copyright
 * notice, this list of conditions and the following disclaimer in the
 * documentation and/or other materials provided with the distribution.
 * 
 * 3. Neither the name of Peter Golde, nor "Purple Pen", nor the names
 * of its contributors may be used to endorse or promote products
 * derived from this software without specific prior written permission.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND
 * CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES,
 * INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING,
 * BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
 * SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE
 * USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY
 * OF SUCH DAMAGE.
 */

namespace PurplePen
{
    partial class PrintCourses
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
            if (disposing && (components != null)) {
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PrintCourses));
            printerGroup = new System.Windows.Forms.GroupBox();
            tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            printerChange = new System.Windows.Forms.Button();
            printerName = new System.Windows.Forms.Label();
            printerLabel = new System.Windows.Forms.Label();
            previewButton = new System.Windows.Forms.Button();
            coursesGroupBox = new System.Windows.Forms.GroupBox();
            courseSelector = new CourseSelector();
            copiesGroupBox = new System.Windows.Forms.GroupBox();
            tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            copiesUpDown = new System.Windows.Forms.NumericUpDown();
            copiesLabel = new System.Windows.Forms.Label();
            checkBoxPausePrinting = new System.Windows.Forms.CheckBox();
            printDialog = new System.Windows.Forms.PrintDialog();
            groupBoxAppearance = new System.Windows.Forms.GroupBox();
            checkBoxMergeParts = new System.Windows.Forms.CheckBox();
            tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            labelColorModel = new System.Windows.Forms.Label();
            comboBoxColorModel = new System.Windows.Forms.ComboBox();
            comboBoxMultiPage = new System.Windows.Forms.ComboBox();
            labelAppearanceInfo = new System.Windows.Forms.Label();
            printerGroup.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            coursesGroupBox.SuspendLayout();
            copiesGroupBox.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)copiesUpDown).BeginInit();
            groupBoxAppearance.SuspendLayout();
            tableLayoutPanel4.SuspendLayout();
            SuspendLayout();
            // 
            // okButton
            // 
            resources.ApplyResources(okButton, "okButton");
            okButton.Click += okButton_Click;
            // 
            // cancelButton
            // 
            resources.ApplyResources(cancelButton, "cancelButton");
            // 
            // printerGroup
            // 
            printerGroup.Controls.Add(tableLayoutPanel1);
            resources.ApplyResources(printerGroup, "printerGroup");
            printerGroup.Name = "printerGroup";
            printerGroup.TabStop = false;
            // 
            // tableLayoutPanel1
            // 
            resources.ApplyResources(tableLayoutPanel1, "tableLayoutPanel1");
            tableLayoutPanel1.Controls.Add(printerChange, 2, 0);
            tableLayoutPanel1.Controls.Add(printerName, 1, 0);
            tableLayoutPanel1.Controls.Add(printerLabel, 0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            // 
            // printerChange
            // 
            resources.ApplyResources(printerChange, "printerChange");
            printerChange.Name = "printerChange";
            printerChange.UseVisualStyleBackColor = true;
            printerChange.Click += printerChange_Click;
            // 
            // printerName
            // 
            resources.ApplyResources(printerName, "printerName");
            printerName.Name = "printerName";
            // 
            // printerLabel
            // 
            resources.ApplyResources(printerLabel, "printerLabel");
            printerLabel.Name = "printerLabel";
            // 
            // previewButton
            // 
            resources.ApplyResources(previewButton, "previewButton");
            previewButton.Name = "previewButton";
            previewButton.UseVisualStyleBackColor = true;
            previewButton.Click += previewButton_Click;
            // 
            // coursesGroupBox
            // 
            coursesGroupBox.Controls.Add(courseSelector);
            resources.ApplyResources(coursesGroupBox, "coursesGroupBox");
            coursesGroupBox.Name = "coursesGroupBox";
            coursesGroupBox.TabStop = false;
            // 
            // courseSelector
            // 
            resources.ApplyResources(courseSelector, "courseSelector");
            courseSelector.Name = "courseSelector";
            // 
            // copiesGroupBox
            // 
            copiesGroupBox.Controls.Add(tableLayoutPanel2);
            resources.ApplyResources(copiesGroupBox, "copiesGroupBox");
            copiesGroupBox.Name = "copiesGroupBox";
            copiesGroupBox.TabStop = false;
            // 
            // tableLayoutPanel2
            // 
            resources.ApplyResources(tableLayoutPanel2, "tableLayoutPanel2");
            tableLayoutPanel2.Controls.Add(copiesUpDown, 1, 0);
            tableLayoutPanel2.Controls.Add(copiesLabel, 0, 0);
            tableLayoutPanel2.Controls.Add(checkBoxPausePrinting, 0, 1);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            // 
            // copiesUpDown
            // 
            resources.ApplyResources(copiesUpDown, "copiesUpDown");
            copiesUpDown.Maximum = new decimal(new int[] { 9999, 0, 0, 0 });
            copiesUpDown.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            copiesUpDown.Name = "copiesUpDown";
            copiesUpDown.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // copiesLabel
            // 
            resources.ApplyResources(copiesLabel, "copiesLabel");
            copiesLabel.Name = "copiesLabel";
            // 
            // checkBoxPausePrinting
            // 
            tableLayoutPanel2.SetColumnSpan(checkBoxPausePrinting, 3);
            resources.ApplyResources(checkBoxPausePrinting, "checkBoxPausePrinting");
            checkBoxPausePrinting.Name = "checkBoxPausePrinting";
            checkBoxPausePrinting.UseVisualStyleBackColor = true;
            // 
            // printDialog
            // 
            printDialog.AllowPrintToFile = false;
            // 
            // groupBoxAppearance
            // 
            groupBoxAppearance.Controls.Add(checkBoxMergeParts);
            groupBoxAppearance.Controls.Add(tableLayoutPanel4);
            groupBoxAppearance.Controls.Add(comboBoxMultiPage);
            groupBoxAppearance.Controls.Add(labelAppearanceInfo);
            resources.ApplyResources(groupBoxAppearance, "groupBoxAppearance");
            groupBoxAppearance.Name = "groupBoxAppearance";
            groupBoxAppearance.TabStop = false;
            // 
            // checkBoxMergeParts
            // 
            resources.ApplyResources(checkBoxMergeParts, "checkBoxMergeParts");
            checkBoxMergeParts.Name = "checkBoxMergeParts";
            checkBoxMergeParts.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel4
            // 
            resources.ApplyResources(tableLayoutPanel4, "tableLayoutPanel4");
            tableLayoutPanel4.Controls.Add(labelColorModel, 0, 0);
            tableLayoutPanel4.Controls.Add(comboBoxColorModel, 1, 0);
            tableLayoutPanel4.Name = "tableLayoutPanel4";
            // 
            // labelColorModel
            // 
            resources.ApplyResources(labelColorModel, "labelColorModel");
            labelColorModel.Name = "labelColorModel";
            // 
            // comboBoxColorModel
            // 
            resources.ApplyResources(comboBoxColorModel, "comboBoxColorModel");
            comboBoxColorModel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboBoxColorModel.FormattingEnabled = true;
            comboBoxColorModel.Items.AddRange(new object[] { resources.GetString("comboBoxColorModel.Items"), resources.GetString("comboBoxColorModel.Items1") });
            comboBoxColorModel.Name = "comboBoxColorModel";
            // 
            // comboBoxMultiPage
            // 
            comboBoxMultiPage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboBoxMultiPage.FormattingEnabled = true;
            comboBoxMultiPage.Items.AddRange(new object[] { resources.GetString("comboBoxMultiPage.Items"), resources.GetString("comboBoxMultiPage.Items1") });
            resources.ApplyResources(comboBoxMultiPage, "comboBoxMultiPage");
            comboBoxMultiPage.Name = "comboBoxMultiPage";
            // 
            // labelAppearanceInfo
            // 
            resources.ApplyResources(labelAppearanceInfo, "labelAppearanceInfo");
            labelAppearanceInfo.Name = "labelAppearanceInfo";
            // 
            // PrintCourses
            // 
            resources.ApplyResources(this, "$this");
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            Controls.Add(groupBoxAppearance);
            Controls.Add(coursesGroupBox);
            Controls.Add(previewButton);
            Controls.Add(copiesGroupBox);
            Controls.Add(printerGroup);
            HelpTopic = "FilePrintCourses.htm";
            Name = "PrintCourses";
            Controls.SetChildIndex(printerGroup, 0);
            Controls.SetChildIndex(copiesGroupBox, 0);
            Controls.SetChildIndex(previewButton, 0);
            Controls.SetChildIndex(coursesGroupBox, 0);
            Controls.SetChildIndex(groupBoxAppearance, 0);
            Controls.SetChildIndex(okButton, 0);
            Controls.SetChildIndex(cancelButton, 0);
            printerGroup.ResumeLayout(false);
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            coursesGroupBox.ResumeLayout(false);
            copiesGroupBox.ResumeLayout(false);
            copiesGroupBox.PerformLayout();
            tableLayoutPanel2.ResumeLayout(false);
            tableLayoutPanel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)copiesUpDown).EndInit();
            groupBoxAppearance.ResumeLayout(false);
            tableLayoutPanel4.ResumeLayout(false);
            tableLayoutPanel4.PerformLayout();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox printerGroup;
        private System.Windows.Forms.Button previewButton;
        private System.Windows.Forms.GroupBox coursesGroupBox;
        private System.Windows.Forms.GroupBox copiesGroupBox;
        private CourseSelector courseSelector;
        private System.Windows.Forms.PrintDialog printDialog;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Button printerChange;
        private System.Windows.Forms.Label printerName;
        private System.Windows.Forms.Label printerLabel;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.NumericUpDown copiesUpDown;
        private System.Windows.Forms.Label copiesLabel;
        private System.Windows.Forms.GroupBox groupBoxAppearance;
        private System.Windows.Forms.Label labelAppearanceInfo;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private System.Windows.Forms.ComboBox comboBoxColorModel;
        private System.Windows.Forms.Label labelColorModel;
        private System.Windows.Forms.ComboBox comboBoxMultiPage;
        private System.Windows.Forms.CheckBox checkBoxMergeParts;
        private System.Windows.Forms.CheckBox checkBoxPausePrinting;
    }
}
