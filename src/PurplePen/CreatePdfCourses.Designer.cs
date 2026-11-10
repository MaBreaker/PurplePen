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
    partial class CreatePdfCourses
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CreatePdfCourses));
            coursesGroupBox = new System.Windows.Forms.GroupBox();
            courseSelector = new CourseSelector();
            groupBoxAppearance = new System.Windows.Forms.GroupBox();
            checkBoxMergeParts = new System.Windows.Forms.CheckBox();
            tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            labelColorModel = new System.Windows.Forms.Label();
            comboBoxColorModel = new System.Windows.Forms.ComboBox();
            labelPrintBaseMap = new System.Windows.Forms.Label();
            comboBoxPrintBaseMap = new System.Windows.Forms.ComboBox();
            comboBoxMultiPage = new System.Windows.Forms.ComboBox();
            labelAppearanceInfo = new System.Windows.Forms.Label();
            folderGroupBox = new System.Windows.Forms.GroupBox();
            tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            selectOtherDirectoryButton = new System.Windows.Forms.Button();
            coursesDirectory = new System.Windows.Forms.RadioButton();
            otherDirectoryTextBox = new System.Windows.Forms.TextBox();
            mapDirectory = new System.Windows.Forms.RadioButton();
            otherDirectory = new System.Windows.Forms.RadioButton();
            outputGroupBox = new System.Windows.Forms.GroupBox();
            tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            fileNamePrefixLabel = new System.Windows.Forms.Label();
            filenamePrefixTextBox = new System.Windows.Forms.TextBox();
            filesLabel = new System.Windows.Forms.Label();
            comboBoxFileFormat = new System.Windows.Forms.ComboBox();
            folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            coursesGroupBox.SuspendLayout();
            groupBoxAppearance.SuspendLayout();
            tableLayoutPanel4.SuspendLayout();
            folderGroupBox.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            outputGroupBox.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
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
            // coursesGroupBox
            // 
            resources.ApplyResources(coursesGroupBox, "coursesGroupBox");
            coursesGroupBox.Controls.Add(courseSelector);
            coursesGroupBox.Name = "coursesGroupBox";
            coursesGroupBox.TabStop = false;
            // 
            // courseSelector
            // 
            resources.ApplyResources(courseSelector, "courseSelector");
            courseSelector.Name = "courseSelector";
            // 
            // groupBoxAppearance
            // 
            resources.ApplyResources(groupBoxAppearance, "groupBoxAppearance");
            groupBoxAppearance.Controls.Add(checkBoxMergeParts);
            groupBoxAppearance.Controls.Add(tableLayoutPanel4);
            groupBoxAppearance.Controls.Add(comboBoxMultiPage);
            groupBoxAppearance.Controls.Add(labelAppearanceInfo);
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
            tableLayoutPanel4.Controls.Add(labelColorModel, 0, 1);
            tableLayoutPanel4.Controls.Add(comboBoxColorModel, 1, 1);
            tableLayoutPanel4.Controls.Add(labelPrintBaseMap, 0, 0);
            tableLayoutPanel4.Controls.Add(comboBoxPrintBaseMap, 1, 0);
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
            // labelPrintBaseMap
            // 
            resources.ApplyResources(labelPrintBaseMap, "labelPrintBaseMap");
            labelPrintBaseMap.Name = "labelPrintBaseMap";
            // 
            // comboBoxPrintBaseMap
            // 
            resources.ApplyResources(comboBoxPrintBaseMap, "comboBoxPrintBaseMap");
            comboBoxPrintBaseMap.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboBoxPrintBaseMap.FormattingEnabled = true;
            comboBoxPrintBaseMap.Items.AddRange(new object[] { resources.GetString("comboBoxPrintBaseMap.Items"), resources.GetString("comboBoxPrintBaseMap.Items1") });
            comboBoxPrintBaseMap.Name = "comboBoxPrintBaseMap";
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
            // folderGroupBox
            // 
            resources.ApplyResources(folderGroupBox, "folderGroupBox");
            folderGroupBox.Controls.Add(tableLayoutPanel1);
            folderGroupBox.Name = "folderGroupBox";
            folderGroupBox.TabStop = false;
            // 
            // tableLayoutPanel1
            // 
            resources.ApplyResources(tableLayoutPanel1, "tableLayoutPanel1");
            tableLayoutPanel1.Controls.Add(selectOtherDirectoryButton, 0, 4);
            tableLayoutPanel1.Controls.Add(coursesDirectory, 0, 0);
            tableLayoutPanel1.Controls.Add(otherDirectoryTextBox, 0, 3);
            tableLayoutPanel1.Controls.Add(mapDirectory, 0, 1);
            tableLayoutPanel1.Controls.Add(otherDirectory, 0, 2);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            // 
            // selectOtherDirectoryButton
            // 
            resources.ApplyResources(selectOtherDirectoryButton, "selectOtherDirectoryButton");
            selectOtherDirectoryButton.Name = "selectOtherDirectoryButton";
            selectOtherDirectoryButton.UseVisualStyleBackColor = true;
            selectOtherDirectoryButton.Click += selectOtherDirectoryButton_Click;
            // 
            // coursesDirectory
            // 
            resources.ApplyResources(coursesDirectory, "coursesDirectory");
            coursesDirectory.Name = "coursesDirectory";
            coursesDirectory.TabStop = true;
            coursesDirectory.UseVisualStyleBackColor = true;
            // 
            // otherDirectoryTextBox
            // 
            resources.ApplyResources(otherDirectoryTextBox, "otherDirectoryTextBox");
            otherDirectoryTextBox.Name = "otherDirectoryTextBox";
            // 
            // mapDirectory
            // 
            resources.ApplyResources(mapDirectory, "mapDirectory");
            mapDirectory.Name = "mapDirectory";
            mapDirectory.TabStop = true;
            mapDirectory.UseVisualStyleBackColor = true;
            // 
            // otherDirectory
            // 
            resources.ApplyResources(otherDirectory, "otherDirectory");
            otherDirectory.Name = "otherDirectory";
            otherDirectory.TabStop = true;
            otherDirectory.UseVisualStyleBackColor = true;
            otherDirectory.CheckedChanged += otherDirectory_CheckedChanged;
            // 
            // outputGroupBox
            // 
            resources.ApplyResources(outputGroupBox, "outputGroupBox");
            outputGroupBox.Controls.Add(tableLayoutPanel2);
            outputGroupBox.Name = "outputGroupBox";
            outputGroupBox.TabStop = false;
            // 
            // tableLayoutPanel2
            // 
            resources.ApplyResources(tableLayoutPanel2, "tableLayoutPanel2");
            tableLayoutPanel2.Controls.Add(fileNamePrefixLabel, 0, 0);
            tableLayoutPanel2.Controls.Add(filenamePrefixTextBox, 1, 0);
            tableLayoutPanel2.Controls.Add(filesLabel, 0, 1);
            tableLayoutPanel2.Controls.Add(comboBoxFileFormat, 1, 1);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            // 
            // fileNamePrefixLabel
            // 
            resources.ApplyResources(fileNamePrefixLabel, "fileNamePrefixLabel");
            fileNamePrefixLabel.Name = "fileNamePrefixLabel";
            // 
            // filenamePrefixTextBox
            // 
            resources.ApplyResources(filenamePrefixTextBox, "filenamePrefixTextBox");
            filenamePrefixTextBox.Name = "filenamePrefixTextBox";
            // 
            // filesLabel
            // 
            resources.ApplyResources(filesLabel, "filesLabel");
            filesLabel.Name = "filesLabel";
            // 
            // comboBoxFileFormat
            // 
            resources.ApplyResources(comboBoxFileFormat, "comboBoxFileFormat");
            comboBoxFileFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboBoxFileFormat.DropDownWidth = 280;
            comboBoxFileFormat.FormattingEnabled = true;
            comboBoxFileFormat.Items.AddRange(new object[] { resources.GetString("comboBoxFileFormat.Items"), resources.GetString("comboBoxFileFormat.Items1"), resources.GetString("comboBoxFileFormat.Items2") });
            comboBoxFileFormat.Name = "comboBoxFileFormat";
            // 
            // folderBrowserDialog
            // 
            resources.ApplyResources(folderBrowserDialog, "folderBrowserDialog");
            // 
            // CreatePdfCourses
            // 
            resources.ApplyResources(this, "$this");
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            Controls.Add(outputGroupBox);
            Controls.Add(folderGroupBox);
            Controls.Add(groupBoxAppearance);
            Controls.Add(coursesGroupBox);
            HelpTopic = "FileCreatePdfFiles.htm";
            Name = "CreatePdfCourses";
            Controls.SetChildIndex(coursesGroupBox, 0);
            Controls.SetChildIndex(groupBoxAppearance, 0);
            Controls.SetChildIndex(folderGroupBox, 0);
            Controls.SetChildIndex(outputGroupBox, 0);
            Controls.SetChildIndex(okButton, 0);
            Controls.SetChildIndex(cancelButton, 0);
            coursesGroupBox.ResumeLayout(false);
            groupBoxAppearance.ResumeLayout(false);
            tableLayoutPanel4.ResumeLayout(false);
            tableLayoutPanel4.PerformLayout();
            folderGroupBox.ResumeLayout(false);
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            outputGroupBox.ResumeLayout(false);
            tableLayoutPanel2.ResumeLayout(false);
            tableLayoutPanel2.PerformLayout();
            ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.GroupBox coursesGroupBox;
        private CourseSelector courseSelector;
        private System.Windows.Forms.GroupBox groupBoxAppearance;
        private System.Windows.Forms.Label labelAppearanceInfo;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private System.Windows.Forms.ComboBox comboBoxColorModel;
        private System.Windows.Forms.Label labelColorModel;
        private System.Windows.Forms.ComboBox comboBoxMultiPage;
        private System.Windows.Forms.CheckBox checkBoxMergeParts;
        private System.Windows.Forms.GroupBox folderGroupBox;
        private System.Windows.Forms.TextBox otherDirectoryTextBox;
        private System.Windows.Forms.Button selectOtherDirectoryButton;
        private System.Windows.Forms.RadioButton otherDirectory;
        private System.Windows.Forms.RadioButton mapDirectory;
        private System.Windows.Forms.RadioButton coursesDirectory;
        private System.Windows.Forms.GroupBox outputGroupBox;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Label fileNamePrefixLabel;
        private System.Windows.Forms.TextBox filenamePrefixTextBox;
        private System.Windows.Forms.Label filesLabel;
        private System.Windows.Forms.ComboBox comboBoxFileFormat;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label labelPrintBaseMap;
        private System.Windows.Forms.ComboBox comboBoxPrintBaseMap;
    }
}
