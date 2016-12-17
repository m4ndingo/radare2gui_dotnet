namespace r2pipe_test
{
    partial class askForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(askForm));
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnPaste = new System.Windows.Forms.Button();
            this.txtHeader = new System.Windows.Forms.RichTextBox();
            this.btnOpenfile = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOk = new System.Windows.Forms.Button();
            this.txtAnswer = new System.Windows.Forms.TextBox();
            this.askLabel = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "paste.png");
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.Controls.Add(this.btnPaste);
            this.panel1.Controls.Add(this.txtHeader);
            this.panel1.Controls.Add(this.btnOpenfile);
            this.panel1.Controls.Add(this.btnCancel);
            this.panel1.Controls.Add(this.btnOk);
            this.panel1.Controls.Add(this.txtAnswer);
            this.panel1.Controls.Add(this.askLabel);
            this.panel1.Location = new System.Drawing.Point(-1, -2);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(392, 213);
            this.panel1.TabIndex = 7;
            // 
            // btnPaste
            // 
            this.btnPaste.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnPaste.ImageIndex = 0;
            this.btnPaste.ImageList = this.imageList1;
            this.btnPaste.Location = new System.Drawing.Point(336, 120);
            this.btnPaste.Name = "btnPaste";
            this.btnPaste.Size = new System.Drawing.Size(44, 39);
            this.btnPaste.TabIndex = 13;
            this.btnPaste.Tag = "Paste";
            this.btnPaste.UseVisualStyleBackColor = true;
            this.btnPaste.Click += new System.EventHandler(this.btnPaste_Click_1);
            // 
            // txtHeader
            // 
            this.txtHeader.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtHeader.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtHeader.Font = new System.Drawing.Font("Century Gothic", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtHeader.Location = new System.Drawing.Point(8, 20);
            this.txtHeader.Margin = new System.Windows.Forms.Padding(0);
            this.txtHeader.Name = "txtHeader";
            this.txtHeader.Size = new System.Drawing.Size(376, 29);
            this.txtHeader.TabIndex = 12;
            this.txtHeader.Text = "txtHeader";
            // 
            // btnOpenfile
            // 
            this.btnOpenfile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOpenfile.Location = new System.Drawing.Point(336, 75);
            this.btnOpenfile.Name = "btnOpenfile";
            this.btnOpenfile.Size = new System.Drawing.Size(44, 39);
            this.btnOpenfile.TabIndex = 11;
            this.btnOpenfile.Text = "...";
            this.btnOpenfile.UseVisualStyleBackColor = true;
            this.btnOpenfile.Click += new System.EventHandler(this.btnOpenfile_Click_1);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(263, 177);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(67, 23);
            this.btnCancel.TabIndex = 10;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnOk
            // 
            this.btnOk.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOk.Location = new System.Drawing.Point(106, 177);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(80, 23);
            this.btnOk.TabIndex = 8;
            this.btnOk.Text = "Ok";
            this.btnOk.UseVisualStyleBackColor = true;
            // 
            // txtAnswer
            // 
            this.txtAnswer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtAnswer.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtAnswer.Location = new System.Drawing.Point(106, 75);
            this.txtAnswer.Multiline = true;
            this.txtAnswer.Name = "txtAnswer";
            this.txtAnswer.Size = new System.Drawing.Size(224, 92);
            this.txtAnswer.TabIndex = 7;
            // 
            // askLabel
            // 
            this.askLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.askLabel.Location = new System.Drawing.Point(11, 76);
            this.askLabel.Name = "askLabel";
            this.askLabel.Size = new System.Drawing.Size(89, 83);
            this.askLabel.TabIndex = 9;
            this.askLabel.Text = "askLabel";
            // 
            // askForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(394, 215);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "askForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "askForm";
            this.Load += new System.EventHandler(this.askForm_Load);
            this.VisibleChanged += new System.EventHandler(this.askForm_VisibleChanged);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnPaste;
        private System.Windows.Forms.RichTextBox txtHeader;
        private System.Windows.Forms.Button btnOpenfile;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.TextBox txtAnswer;
        private System.Windows.Forms.Label askLabel;
    }
}