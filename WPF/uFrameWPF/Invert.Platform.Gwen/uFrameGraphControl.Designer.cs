namespace Invert.Platform.Gwen
{
    partial class uFrameGraphControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.TheContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.Toolbar = new System.Windows.Forms.ToolStrip();
            this.SuspendLayout();
            // 
            // TheContextMenu
            // 
            this.TheContextMenu.Name = "TheContextMenu";
            this.TheContextMenu.Size = new System.Drawing.Size(61, 4);
            // 
            // Toolbar
            // 
            this.Toolbar.Location = new System.Drawing.Point(0, 0);
            this.Toolbar.Name = "Toolbar";
            this.Toolbar.Size = new System.Drawing.Size(150, 25);
            this.Toolbar.TabIndex = 5;
            this.Toolbar.Text = "toolStrip1";
            // 
            // uFrameGraphControl
            // 
            this.Controls.Add(this.Toolbar);
            this.Name = "uFrameGraphControl";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.ContextMenuStrip TheContextMenu;
        public System.Windows.Forms.ToolStrip Toolbar;
    }
}
