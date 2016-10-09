namespace GisSmartTools
{
    partial class MapControl
    {
        /// <summary> 
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.picturebox_menu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.开始输入ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.完成输入ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.完成部分ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.重新输入ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.全部重新输入ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.picturebox_menu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // picturebox_menu
            // 
            this.picturebox_menu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.开始输入ToolStripMenuItem,
            this.重新输入ToolStripMenuItem,
            this.完成输入ToolStripMenuItem,
            this.完成部分ToolStripMenuItem});
            this.picturebox_menu.Name = "picturebox_menu";
            this.picturebox_menu.Size = new System.Drawing.Size(153, 114);
            // 
            // 开始输入ToolStripMenuItem
            // 
            this.开始输入ToolStripMenuItem.Name = "开始输入ToolStripMenuItem";
            this.开始输入ToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.开始输入ToolStripMenuItem.Text = "开始输入";
            this.开始输入ToolStripMenuItem.Click += new System.EventHandler(this.开始输入ToolStripMenuItem_Click);
            // 
            // 完成输入ToolStripMenuItem
            // 
            this.完成输入ToolStripMenuItem.Name = "完成输入ToolStripMenuItem";
            this.完成输入ToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.完成输入ToolStripMenuItem.Text = "完成输入";
            this.完成输入ToolStripMenuItem.Click += new System.EventHandler(this.完成输入ToolStripMenuItem_Click);
            // 
            // 完成部分ToolStripMenuItem
            // 
            this.完成部分ToolStripMenuItem.Name = "完成部分ToolStripMenuItem";
            this.完成部分ToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.完成部分ToolStripMenuItem.Text = "完成部分";
            this.完成部分ToolStripMenuItem.Click += new System.EventHandler(this.完成部分ToolStripMenuItem_Click);
            // 
            // 重新输入ToolStripMenuItem
            // 
            this.重新输入ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.全部重新输入ToolStripMenuItem});
            this.重新输入ToolStripMenuItem.Name = "重新输入ToolStripMenuItem";
            this.重新输入ToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.重新输入ToolStripMenuItem.Text = "重新输入";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox1.Location = new System.Drawing.Point(3, 3);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(240, 219);
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.SizeChanged += new System.EventHandler(this.pictureBox1_SizeChanged);
            this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
            this.pictureBox1.DoubleClick += new System.EventHandler(this.pictureBox1_DoubleClick);
            this.pictureBox1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pictureBox1_MouseDown);
            this.pictureBox1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pictureBox1_MouseMove);
            this.pictureBox1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.pictureBox1_MouseUp);
            // 
            // 全部重新输入ToolStripMenuItem
            // 
            this.全部重新输入ToolStripMenuItem.Name = "全部重新输入ToolStripMenuItem";
            this.全部重新输入ToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.全部重新输入ToolStripMenuItem.Text = "全部重新输入";
            this.全部重新输入ToolStripMenuItem.Click += new System.EventHandler(this.全部重新输入ToolStripMenuItem_Click);
            // 
            // MapControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.Controls.Add(this.pictureBox1);
            this.Name = "MapControl";
            this.Size = new System.Drawing.Size(246, 225);
            this.picturebox_menu.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.ContextMenuStrip picturebox_menu;
        private System.Windows.Forms.ToolStripMenuItem 开始输入ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 完成输入ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 完成部分ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 重新输入ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 全部重新输入ToolStripMenuItem;
    }
}
