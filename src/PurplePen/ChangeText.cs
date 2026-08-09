using System;
using System.Collections.Generic;
using System.ComponentModel;

using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows.Forms;
using PurplePen.Graphics2D;
using PurplePen.MapModel;

namespace PurplePen
{
    public partial class ChangeText: OkCancelDialog
    {
        SpecialColorChooser colorChooser;
        CmykColor purpleColor;
        Func<string, string> textExpander;

        public ChangeText()
        {
            InitializeComponent();
        }

        private void InitializeFontList()
        {
            List<string> familyNames = new List<string>();
            foreach (FontFamily family in FontFamily.Families) {
                familyNames.Add(family.Name);
            }

            listBoxFonts.Items.AddRange(familyNames.ToArray());
        }

        public ChangeText(string title, string explanation, bool allowSpecialTextInsert, CmykColor purpleColor, Func<string, string> textExpander)
            : this()
        {
            InitializeFontList();

            this.textExpander = textExpander;
            this.purpleColor = purpleColor;
            colorChooser = new SpecialColorChooser(comboBoxColor, buttonChangeColor, purpleColor);
            colorChooser.ColorChanged += colorChanged;

            this.Text = title;
            this.usageLabel.Text = explanation;
            if (!allowSpecialTextInsert)
                insertSpecialButton.Visible = false;

            textBoxMain_TextChanged(this, EventArgs.Empty);
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string UserText
        {
            set
            {
                textBoxMain.Text = value;
            }
            get
            {
                return textBoxMain.Text;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string FontName
        {
            set
            {
                if (listBoxFonts.Items.Contains(value))
                    listBoxFonts.SelectedItem = (string)value;
                else
                    listBoxFonts.SelectedItem = "Arial";
            }
            get
            {
                string s = (string) listBoxFonts.SelectedItem;

                if (string.IsNullOrEmpty(s))
                    return "Arial";
                else
                    return s;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool FontBold
        {
            set
            {
                checkBoxBold.Checked = value;
            }

            get
            {
                return checkBoxBold.Checked;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool FontItalic
        {
            set
            {
                checkBoxItalic.Checked = value;
            }

            get
            {
                return checkBoxItalic.Checked;
            }
        }

        public TextEffects TextEffects
        {
            get
            {
                return Util.GetTextEffects(FontBold, FontItalic);
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public SpecialColor FontColor
        {
            get { return colorChooser.Color;  }
            set { colorChooser.Color = value; }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool FontSizeAutomatic {
            get { return checkBoxAutoFontSize.Checked; }
            set { checkBoxAutoFontSize.Checked = value; }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public float FontSize {
            get { return (float) upDownFontSize.Value; }
            set { upDownFontSize.Value = (decimal) value; }
        }

        //JU: Rotated text
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public float TextRotation
        {
            get { return (float) upDownTextRotation.Value; }
            set { upDownTextRotation.Value = (decimal) value; }
        }

        //JU: Multiline texts
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool TextMultiline
        {
            get { return checkBoxMultiline.Checked; }
            set { checkBoxMultiline.Checked = value; }
        }

        void InsertSpecialText(string specialText)
        {
            textBoxMain.Paste(specialText);
            textBoxMain.Focus();
        }

        private void insertSpecialButton_Click(object sender, EventArgs e)
        {
            specialTextMenu.Show(insertSpecialButton, new Point(0, insertSpecialButton.Height), ToolStripDropDownDirection.BelowRight);
        }

        private void eventTitleMenuItem_Click(object sender, EventArgs e)
        {
            InsertSpecialText(TextMacros.EventTitle);
        }

        private void courseNameMenuItem_Click(object sender, EventArgs e)
        {
            InsertSpecialText(TextMacros.CourseName);
        }

        private void coursePartMenuItem_Click(object sender, EventArgs e)
        {
            InsertSpecialText(TextMacros.CoursePart);
        }

        private void courseLengthMenuItem_Click(object sender, EventArgs e)
        {
            InsertSpecialText(TextMacros.CourseLength);
        }

        private void courseClimbMenuItem_Click(object sender, EventArgs e)
        {
            InsertSpecialText(TextMacros.CourseClimb);
        }

        private void classListMenuItem_Click(object sender, EventArgs e)
        {
            InsertSpecialText(TextMacros.ClassList);
        }

        private void printScaleMenuItem_Click(object sender, EventArgs e)
        {
            InsertSpecialText(TextMacros.PrintScale);
        }

        private void variationMenuItem_Click(object sender, EventArgs e)
        {
            InsertSpecialText(TextMacros.Variation);
        }

        private void relayTeamMenuItem_Click(object sender, EventArgs e)
        {
            InsertSpecialText(TextMacros.RelayTeam);
        }

        private void relayLegMenuItem_Click(object sender, EventArgs e)
        {
            InsertSpecialText(TextMacros.RelayLeg);
        }

        private void fileNameMenuItem_Click(object sender, EventArgs e)
        {
            InsertSpecialText(TextMacros.FileName);
        }

        private void mapFileNameMenuItem_Click(object sender, EventArgs e)
        {
            InsertSpecialText(TextMacros.MapFileName);
        }

        private void textBoxMain_TextChanged(object sender, EventArgs e)
        {
            okButton.Enabled = textBoxMain.Text != "";
            UpdatePreview();
        }

        void UpdatePreview()
        {
            pictureBoxPreview.Invalidate();
        }

        private void pictureBoxPreview_Paint(object sender, PaintEventArgs e)
        {
            string expandedText = textExpander(this.UserText);
            
            //JU: Multiline texts
            int lineCount = 1;
            if (checkBoxMultiline.Checked) {
                expandedText = expandedText.Replace("|", "\n");
                lineCount = expandedText.Split('\n').Length;
            }

            //JU: Preview line count
            float emHeight;
            if (checkBoxAutoFontSize.Checked)
            {
                emHeight = CalculateEmHeight(expandedText, this.FontName, this.TextEffects, 0, pictureBoxPreview.Size); 
            }
            else {
                emHeight = GetEmHeight((float)pictureBoxPreview.Height, this.FontName, this.TextEffects, (float)upDownFontSize.Value);
            }

            System.Drawing.Color textColor = SwopColorConverter.Instance.ToColor(colorChooser.CmykColor);
            StringFormat stringFormat = new StringFormat(StringFormat.GenericDefault);
            stringFormat.LineAlignment = StringAlignment.Near; //JU: Center -> Near
            stringFormat.FormatFlags |= StringFormatFlags.NoWrap;
            using (System.Drawing.Font font = ((GdiplusFontLoader)Services.FontLoader).CreateFont(this.FontName, emHeight, this.TextEffects))
            {
                SizeF textSize = e.Graphics.MeasureString(expandedText, font, new SizeF(999999F, 999999F), stringFormat);

                // Center text horizontally, and if larger than preview box use top left corner
                float yOffset = (pictureBoxPreview.Height - textSize.Height) / 2;
                if (yOffset < 0.0F) yOffset = 0.0F;

                using (System.Drawing.Brush brush = new SolidBrush(textColor))
                {
                    //e.Graphics.DrawString(expandedText, font, brush, pictureBoxPreview.ClientRectangle, stringFormat);
                    e.Graphics.DrawString(expandedText, font, brush, 0F, yOffset, stringFormat);
                }
            }
        }

        private float GetEmHeight(float regionHeight, string fontName, TextEffects textEffects, float desiredDigitHeight)
        {
            return (regionHeight / 10F) * desiredDigitHeight * BasicTextCourseObj.EmHeightToDigitHeightRatio(fontName, textEffects);
        }

        //JU: Calculate emHeight for auto font size
        private float CalculateEmHeight(string text, string fontName, TextEffects textEffects, float fontDigitHeight, SizeF desiredSize)
        {
            return BasicTextCourseObj.CalculateEmHeight(text, fontName, textEffects, fontDigitHeight, desiredSize);
        }

        private void listBoxFonts_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdatePreview();
        }

        private void checkBoxBold_CheckedChanged(object sender, EventArgs e)
        {
            UpdatePreview();
        }

        private void checkBoxItalic_CheckedChanged(object sender, EventArgs e)
        {
            UpdatePreview();
        }

        private void colorChanged(object sender, EventArgs e)
        {
            UpdatePreview();
        }

        private void checkBoxAutoFontSize_CheckedChanged(object sender, EventArgs e)
        {
            upDownFontSize.Enabled = labelFontSizeMm.Enabled = !checkBoxAutoFontSize.Checked;
            UpdatePreview();
        }

        private void upDownFontSize_ValueChanged(object sender, EventArgs e)
        {
            UpdatePreview();
        }

        //JU: Rotated text
        private void upDownTextRotation_ValueChanged(object sender, EventArgs e)
        {
            UpdatePreview();
        }

        //JU: Multiline texts
        private void checkBoxMultiline_CheckedChanged(object sender, EventArgs e)
        {
            UpdatePreview();
        }
    }
}
