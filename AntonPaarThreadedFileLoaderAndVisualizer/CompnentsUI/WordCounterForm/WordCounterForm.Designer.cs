namespace AntonPaarThreadedFileLoaderAndVisualizer
{
    partial class WordCounterForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            progressBar1 = new ProgressBar();
            button1 = new Button();
            listView1 = new ListView();
            columnHeader1 = new ColumnHeader();
            columnHeader2 = new ColumnHeader();
            SuspendLayout();
            // 
            // progressBar1
            // 
            progressBar1.Location = new Point(12, 404);
            progressBar1.Name = "progressBar1";
            progressBar1.Size = new Size(776, 56);
            progressBar1.TabIndex = 0;
            // 
            // button1
            // 
            button1.Location = new Point(12, 12);
            button1.Name = "button1";
            button1.Size = new Size(776, 56);
            button1.TabIndex = 1;
            button1.Text = "Datei laden und parsen";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // listView1
            // 
            listView1.Columns.AddRange(new ColumnHeader[] { columnHeader1, columnHeader2 });
            listView1.FullRowSelect = true;
            listView1.GridLines = true;
            listView1.ImeMode = ImeMode.On;
            listView1.Location = new Point(12, 74);
            listView1.Name = "listView1";
            listView1.RightToLeft = RightToLeft.Yes;
            listView1.Size = new Size(776, 327);
            listView1.TabIndex = 5;
            listView1.UseCompatibleStateImageBehavior = false;
            listView1.View = View.Details;
            listView1.ColumnClick += listView1_ColumnClick;
            // 
            // columnHeader1
            // 
            columnHeader1.Text = "Wort";
            columnHeader1.Width = 370;
            // 
            // columnHeader2
            // 
            columnHeader2.Text = "Anzahl";
            columnHeader2.TextAlign = HorizontalAlignment.Right;
            columnHeader2.Width = 370;
            // 
            // WordCounterForm
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 473);
            Controls.Add(listView1);
            Controls.Add(button1);
            Controls.Add(progressBar1);
            Name = "WordCounterForm";
            Text = "Word Counter";
            ResumeLayout(false);
        }

        #endregion

        private ProgressBar progressBar1;
        private Button button1;
        private ListView listView1;
        private ColumnHeader columnHeader1;
        private ColumnHeader columnHeader2;
    }
}
