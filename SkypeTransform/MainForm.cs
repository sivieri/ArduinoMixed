/**
 * Copyright 2011 Alessandro Sivieri <alessandro.sivieri@gmail.com>
 * 
 * SkypeTransform is free software: you can redistribute it and/or modify it
 * under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * any later version.
 * 
 * SkypeTransform is distributed in the hope that it will be useful, but
 * WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
 * General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with SkypeTransform. If not, see http://www.gnu.org/licenses/.
 */

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.IO;
using System.Xml;
using System.Xml.Xsl;
using System.Xml.XPath;
using System.Reflection;
using Saxon.Api;

namespace SkypeTransformer
{
	public class MainForm : System.Windows.Forms.Form
	{
		private System.Windows.Forms.TextBox xmlSource;
		private System.Windows.Forms.TextBox htmlDest;
		private System.Windows.Forms.Label lblSource;
		private System.Windows.Forms.Label lblXslt;
		private System.Windows.Forms.Button generateButton;
        private System.Windows.Forms.Button resetButton;
        private Button openXmlSource;
        private Button saveHtmlDest;
        private Assembly assembly;
        private Stream xsltStream;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public MainForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.xmlSource = new System.Windows.Forms.TextBox();
            this.htmlDest = new System.Windows.Forms.TextBox();
            this.lblSource = new System.Windows.Forms.Label();
            this.lblXslt = new System.Windows.Forms.Label();
            this.generateButton = new System.Windows.Forms.Button();
            this.resetButton = new System.Windows.Forms.Button();
            this.openXmlSource = new System.Windows.Forms.Button();
            this.saveHtmlDest = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // xmlSource
            // 
            this.xmlSource.Location = new System.Drawing.Point(113, 14);
            this.xmlSource.Name = "xmlSource";
            this.xmlSource.ReadOnly = true;
            this.xmlSource.Size = new System.Drawing.Size(114, 20);
            this.xmlSource.TabIndex = 0;
            // 
            // htmlDest
            // 
            this.htmlDest.Location = new System.Drawing.Point(113, 42);
            this.htmlDest.Name = "htmlDest";
            this.htmlDest.ReadOnly = true;
            this.htmlDest.Size = new System.Drawing.Size(114, 20);
            this.htmlDest.TabIndex = 1;
            // 
            // lblSource
            // 
            this.lblSource.Location = new System.Drawing.Point(13, 14);
            this.lblSource.Name = "lblSource";
            this.lblSource.Size = new System.Drawing.Size(84, 20);
            this.lblSource.TabIndex = 2;
            this.lblSource.Text = "Source XML";
            // 
            // lblXslt
            // 
            this.lblXslt.Location = new System.Drawing.Point(13, 42);
            this.lblXslt.Name = "lblXslt";
            this.lblXslt.Size = new System.Drawing.Size(84, 20);
            this.lblXslt.TabIndex = 3;
            this.lblXslt.Text = "Output file";
            // 
            // generateButton
            // 
            this.generateButton.Enabled = false;
            this.generateButton.Location = new System.Drawing.Point(16, 75);
            this.generateButton.Name = "generateButton";
            this.generateButton.Size = new System.Drawing.Size(63, 20);
            this.generateButton.TabIndex = 4;
            this.generateButton.Text = "Generate";
            this.generateButton.Click += new System.EventHandler(this.generateButton_Click);
            // 
            // resetButton
            // 
            this.resetButton.Location = new System.Drawing.Point(113, 75);
            this.resetButton.Name = "resetButton";
            this.resetButton.Size = new System.Drawing.Size(63, 20);
            this.resetButton.TabIndex = 6;
            this.resetButton.Text = "Reset";
            this.resetButton.Click += new System.EventHandler(this.resetButton_Click);
            // 
            // openXmlSource
            // 
            this.openXmlSource.Location = new System.Drawing.Point(244, 12);
            this.openXmlSource.Name = "openXmlSource";
            this.openXmlSource.Size = new System.Drawing.Size(75, 23);
            this.openXmlSource.TabIndex = 7;
            this.openXmlSource.Text = "Open";
            this.openXmlSource.UseVisualStyleBackColor = true;
            this.openXmlSource.Click += new System.EventHandler(this.openXmlSource_Click);
            // 
            // saveHtmlDest
            // 
            this.saveHtmlDest.Location = new System.Drawing.Point(244, 40);
            this.saveHtmlDest.Name = "saveHtmlDest";
            this.saveHtmlDest.Size = new System.Drawing.Size(75, 23);
            this.saveHtmlDest.TabIndex = 8;
            this.saveHtmlDest.Text = "Save";
            this.saveHtmlDest.UseVisualStyleBackColor = true;
            this.saveHtmlDest.Click += new System.EventHandler(this.saveHtmlDest_Click);
            // 
            // MainForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(331, 104);
            this.Controls.Add(this.saveHtmlDest);
            this.Controls.Add(this.openXmlSource);
            this.Controls.Add(this.resetButton);
            this.Controls.Add(this.generateButton);
            this.Controls.Add(this.lblXslt);
            this.Controls.Add(this.lblSource);
            this.Controls.Add(this.htmlDest);
            this.Controls.Add(this.xmlSource);
            this.Name = "MainForm";
            this.Text = "Skype Transformer";
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new MainForm());
		}

		private void MainForm_Load(object sender, System.EventArgs e)
		{
            assembly = Assembly.GetExecutingAssembly();
            xsltStream = assembly.GetManifestResourceStream("SkypeTransformer.skype.xslt");
		}

		private void generateButton_Click(object sender, System.EventArgs e)
		{
            try
            {
                generateButton.Enabled = false;
                Cursor.Current = Cursors.WaitCursor;
                Processor processor = new Processor();
                DocumentBuilder builder = processor.NewDocumentBuilder();
                builder.BaseUri = new Uri(xmlSource.Text);
                XdmNode input = builder.Build(new FileStream(xmlSource.Text, FileMode.Open));
                XsltCompiler compiler = processor.NewXsltCompiler();
                compiler.BaseUri = new Uri(htmlDest.Text);
                XsltTransformer transformer = compiler.Compile(xsltStream).Load();
                transformer.InitialContextNode = input;
                Serializer serializer = new Serializer();
                serializer.SetOutputStream(new FileStream(htmlDest.Text, FileMode.Create, FileAccess.Write));
                transformer.Run(serializer);
                Cursor.Current = Cursors.Default;
                generateButton.Enabled = true;
            }
            catch (Exception ex)
            {
                Cursor.Current = Cursors.Default;
                generateButton.Enabled = true;
                MessageBox.Show(ex.ToString());
            }
		}

        private void openXmlSource_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "XML files (*.xml)|*.xml";
            openFileDialog.RestoreDirectory = true;
            if (openFileDialog.ShowDialog() == DialogResult.OK) {
                xmlSource.Text = openFileDialog.FileName;
                if (!htmlDest.Text.Equals("")) {
                    generateButton.Enabled = true;
                }
            }
        }

        private void saveHtmlDest_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "HTML files (*.html)|*.html";
            saveFileDialog.RestoreDirectory = true;
            if (!xmlSource.Text.Equals("")) {
                String fileName = Path.GetFileNameWithoutExtension(xmlSource.Text);
                saveFileDialog.FileName = fileName + ".html";
            }
            if (saveFileDialog.ShowDialog() == DialogResult.OK) {
                htmlDest.Text = saveFileDialog.FileName;
                if (!xmlSource.Text.Equals("")) {
                    generateButton.Enabled = true;
                }
            }
        }

        private void resetButton_Click(object sender, EventArgs e)
        {
            xmlSource.Text = "";
            htmlDest.Text = "";
        }
	}
}
