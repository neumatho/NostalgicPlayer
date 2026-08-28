namespace Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.Display
{
    partial class PatternControl
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
            if (disposing)
            {
                components?.Dispose();
                renderer?.Dispose();
                contextMenu?.Dispose();
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
            components = new System.ComponentModel.Container();
            patternPanel = new PatternViewerPanel();
            animationTimer = new System.Windows.Forms.Timer(components);
            SuspendLayout();
            //
            // patternPanel
            //
            patternPanel.BackColor = System.Drawing.Color.Black;
            patternPanel.BorderStyle = System.Windows.Forms.BorderStyle.None;
            patternPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            patternPanel.Location = new System.Drawing.Point(8, 8);
            patternPanel.Name = "patternPanel";
            patternPanel.Size = new System.Drawing.Size(784, 584);
            patternPanel.TabIndex = 0;
            patternPanel.Paint += PatternPanel_Paint;
            patternPanel.MouseClick += PatternPanel_MouseClick;
            patternPanel.MouseDown += PatternPanel_MouseDown;
            patternPanel.MouseUp += PatternPanel_MouseUp;
            patternPanel.MouseMove += PatternPanel_MouseMove;
            //
            // animationTimer
            //
            animationTimer.Interval = 20;
            animationTimer.Tick += AnimationTimer_Tick;
            //
            // PatternControl
            //
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.Color.FromArgb(48, 48, 48);
            Controls.Add(patternPanel);
            MinimumSize = new System.Drawing.Size(30 + 4*80 + 20, 400);
            Name = "PatternControl";
            Padding = new System.Windows.Forms.Padding(0);
            Size = new System.Drawing.Size(30 + 8*80 + 20, 600);
            ResumeLayout(false);
        }

        #endregion

        private PatternViewerPanel patternPanel;
        private System.Windows.Forms.Timer animationTimer;
    }
}
