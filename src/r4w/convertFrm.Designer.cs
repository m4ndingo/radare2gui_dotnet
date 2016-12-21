﻿namespace r2pipe_test
{
    partial class convertFrm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(convertFrm));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.btnClear = new System.Windows.Forms.Button();
            this.btnConvert = new System.Windows.Forms.Button();
            this.txtCommands = new System.Windows.Forms.RichTextBox();
            this.lblCommands = new System.Windows.Forms.Label();
            this.lstOperations = new System.Windows.Forms.ListView();
            this.operation_col = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.cmdWriteInput = new System.Windows.Forms.RichTextBox();
            this.lblInput = new System.Windows.Forms.Label();
            this.txtInput = new System.Windows.Forms.RichTextBox();
            this.txtBlockSize = new System.Windows.Forms.RichTextBox();
            this.lblBlock = new System.Windows.Forms.Label();
            this.txtSeekAddress = new System.Windows.Forms.RichTextBox();
            this.lblSeek = new System.Windows.Forms.Label();
            this.txtOutput = new System.Windows.Forms.RichTextBox();
            this.lblOutput = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.btnClear);
            this.splitContainer1.Panel1.Controls.Add(this.btnConvert);
            this.splitContainer1.Panel1.Controls.Add(this.txtCommands);
            this.splitContainer1.Panel1.Controls.Add(this.lblCommands);
            this.splitContainer1.Panel1.Controls.Add(this.lstOperations);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(828, 501);
            this.splitContainer1.SplitterDistance = 203;
            this.splitContainer1.TabIndex = 0;
            // 
            // btnClear
            // 
            this.btnClear.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClear.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClear.Font = new System.Drawing.Font("Consolas", 11.25F);
            this.btnClear.Location = new System.Drawing.Point(2, 467);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(201, 30);
            this.btnClear.TabIndex = 9;
            this.btnClear.Text = "Clear";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // btnConvert
            // 
            this.btnConvert.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnConvert.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnConvert.Font = new System.Drawing.Font("Consolas", 11.25F);
            this.btnConvert.Location = new System.Drawing.Point(2, 436);
            this.btnConvert.Name = "btnConvert";
            this.btnConvert.Size = new System.Drawing.Size(201, 30);
            this.btnConvert.TabIndex = 8;
            this.btnConvert.Text = "Convert";
            this.btnConvert.UseVisualStyleBackColor = true;
            this.btnConvert.Click += new System.EventHandler(this.btnConvert_Click);
            // 
            // txtCommands
            // 
            this.txtCommands.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtCommands.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtCommands.Font = new System.Drawing.Font("Consolas", 11.25F);
            this.txtCommands.Location = new System.Drawing.Point(4, 323);
            this.txtCommands.Name = "txtCommands";
            this.txtCommands.Size = new System.Drawing.Size(199, 110);
            this.txtCommands.TabIndex = 7;
            this.txtCommands.Text = "";
            this.txtCommands.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtCommands_KeyDown);
            // 
            // lblCommands
            // 
            this.lblCommands.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblCommands.AutoSize = true;
            this.lblCommands.Font = new System.Drawing.Font("Consolas", 11.25F, System.Drawing.FontStyle.Bold);
            this.lblCommands.Location = new System.Drawing.Point(2, 302);
            this.lblCommands.Name = "lblCommands";
            this.lblCommands.Size = new System.Drawing.Size(96, 18);
            this.lblCommands.TabIndex = 2;
            this.lblCommands.Text = "r2 commands";
            // 
            // lstOperations
            // 
            this.lstOperations.Activation = System.Windows.Forms.ItemActivation.OneClick;
            this.lstOperations.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstOperations.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lstOperations.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.operation_col});
            this.lstOperations.Font = new System.Drawing.Font("Consolas", 11.25F);
            this.lstOperations.FullRowSelect = true;
            this.lstOperations.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.lstOperations.Location = new System.Drawing.Point(4, -2);
            this.lstOperations.Name = "lstOperations";
            this.lstOperations.Size = new System.Drawing.Size(199, 299);
            this.lstOperations.TabIndex = 0;
            this.lstOperations.UseCompatibleStateImageBehavior = false;
            this.lstOperations.View = System.Windows.Forms.View.Details;
            this.lstOperations.DoubleClick += new System.EventHandler(this.lstOperations_DoubleClick);
            // 
            // operation_col
            // 
            this.operation_col.Text = "Operations";
            this.operation_col.Width = 193;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.cmdWriteInput);
            this.splitContainer2.Panel1.Controls.Add(this.lblInput);
            this.splitContainer2.Panel1.Controls.Add(this.txtInput);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.txtBlockSize);
            this.splitContainer2.Panel2.Controls.Add(this.lblBlock);
            this.splitContainer2.Panel2.Controls.Add(this.txtSeekAddress);
            this.splitContainer2.Panel2.Controls.Add(this.lblSeek);
            this.splitContainer2.Panel2.Controls.Add(this.txtOutput);
            this.splitContainer2.Panel2.Controls.Add(this.lblOutput);
            this.splitContainer2.Size = new System.Drawing.Size(621, 501);
            this.splitContainer2.SplitterDistance = 126;
            this.splitContainer2.TabIndex = 0;
            // 
            // cmdWriteInput
            // 
            this.cmdWriteInput.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdWriteInput.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.cmdWriteInput.Font = new System.Drawing.Font("Consolas", 11.25F);
            this.cmdWriteInput.Location = new System.Drawing.Point(3, 100);
            this.cmdWriteInput.Multiline = false;
            this.cmdWriteInput.Name = "cmdWriteInput";
            this.cmdWriteInput.Size = new System.Drawing.Size(612, 25);
            this.cmdWriteInput.TabIndex = 10;
            this.cmdWriteInput.Text = "";
            // 
            // lblInput
            // 
            this.lblInput.AutoSize = true;
            this.lblInput.Font = new System.Drawing.Font("Consolas", 11.25F, System.Drawing.FontStyle.Bold);
            this.lblInput.Location = new System.Drawing.Point(1, 2);
            this.lblInput.Name = "lblInput";
            this.lblInput.Size = new System.Drawing.Size(48, 18);
            this.lblInput.TabIndex = 3;
            this.lblInput.Text = "Input";
            // 
            // txtInput
            // 
            this.txtInput.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtInput.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtInput.Font = new System.Drawing.Font("Consolas", 11.25F);
            this.txtInput.Location = new System.Drawing.Point(3, 19);
            this.txtInput.Name = "txtInput";
            this.txtInput.Size = new System.Drawing.Size(612, 78);
            this.txtInput.TabIndex = 0;
            this.txtInput.Text = "welcome to r4w gui powered by radare2";
            this.txtInput.TextChanged += new System.EventHandler(this.txtInput_TextChanged);
            // 
            // txtBlockSize
            // 
            this.txtBlockSize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.txtBlockSize.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtBlockSize.Font = new System.Drawing.Font("Consolas", 11.25F);
            this.txtBlockSize.Location = new System.Drawing.Point(553, 344);
            this.txtBlockSize.Multiline = false;
            this.txtBlockSize.Name = "txtBlockSize";
            this.txtBlockSize.Size = new System.Drawing.Size(62, 20);
            this.txtBlockSize.TabIndex = 18;
            this.txtBlockSize.Text = "128";
            this.txtBlockSize.TextChanged += new System.EventHandler(this.txtBlockSize_TextChanged_1);
            // 
            // lblBlock
            // 
            this.lblBlock.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lblBlock.AutoSize = true;
            this.lblBlock.Font = new System.Drawing.Font("Consolas", 11.25F);
            this.lblBlock.Location = new System.Drawing.Point(504, 344);
            this.lblBlock.Name = "lblBlock";
            this.lblBlock.Size = new System.Drawing.Size(48, 18);
            this.lblBlock.TabIndex = 17;
            this.lblBlock.Text = "Block";
            // 
            // txtSeekAddress
            // 
            this.txtSeekAddress.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSeekAddress.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtSeekAddress.Font = new System.Drawing.Font("Consolas", 11.25F);
            this.txtSeekAddress.Location = new System.Drawing.Point(429, 344);
            this.txtSeekAddress.Multiline = false;
            this.txtSeekAddress.Name = "txtSeekAddress";
            this.txtSeekAddress.Size = new System.Drawing.Size(69, 20);
            this.txtSeekAddress.TabIndex = 16;
            this.txtSeekAddress.Text = "0";
            this.txtSeekAddress.TextChanged += new System.EventHandler(this.txtSeekAddress_TextChanged_1);
            // 
            // lblSeek
            // 
            this.lblSeek.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSeek.AutoSize = true;
            this.lblSeek.Font = new System.Drawing.Font("Consolas", 11.25F);
            this.lblSeek.Location = new System.Drawing.Point(383, 344);
            this.lblSeek.Name = "lblSeek";
            this.lblSeek.Size = new System.Drawing.Size(40, 18);
            this.lblSeek.TabIndex = 15;
            this.lblSeek.Text = "Seek";
            this.lblSeek.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // txtOutput
            // 
            this.txtOutput.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtOutput.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtOutput.Font = new System.Drawing.Font("Consolas", 11.25F);
            this.txtOutput.Location = new System.Drawing.Point(3, 22);
            this.txtOutput.Name = "txtOutput";
            this.txtOutput.Size = new System.Drawing.Size(612, 316);
            this.txtOutput.TabIndex = 5;
            this.txtOutput.Text = "";
            this.txtOutput.WordWrap = false;
            // 
            // lblOutput
            // 
            this.lblOutput.AutoSize = true;
            this.lblOutput.Font = new System.Drawing.Font("Consolas", 11.25F, System.Drawing.FontStyle.Bold);
            this.lblOutput.Location = new System.Drawing.Point(1, 1);
            this.lblOutput.Name = "lblOutput";
            this.lblOutput.Size = new System.Drawing.Size(56, 18);
            this.lblOutput.TabIndex = 4;
            this.lblOutput.Text = "Output";
            // 
            // convertFrm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(825, 500);
            this.Controls.Add(this.splitContainer1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "convertFrm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Converter";
            this.Load += new System.EventHandler(this.convertFrm_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel1.PerformLayout();
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.Label lblCommands;
        private System.Windows.Forms.ListView lstOperations;
        private System.Windows.Forms.Label lblInput;
        private System.Windows.Forms.RichTextBox txtInput;
        private System.Windows.Forms.RichTextBox txtOutput;
        private System.Windows.Forms.Label lblOutput;
        private System.Windows.Forms.RichTextBox txtCommands;
        private System.Windows.Forms.Button btnConvert;
        private System.Windows.Forms.RichTextBox cmdWriteInput;
        private System.Windows.Forms.ColumnHeader operation_col;
        private System.Windows.Forms.RichTextBox txtBlockSize;
        private System.Windows.Forms.Label lblBlock;
        private System.Windows.Forms.RichTextBox txtSeekAddress;
        private System.Windows.Forms.Label lblSeek;
        private System.Windows.Forms.Button btnClear;
    }
}