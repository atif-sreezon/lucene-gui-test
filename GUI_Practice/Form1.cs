using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.IO;
using Lucene.Net.Search; // for IndexSearcher

namespace GUI_Practice
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        OpenFileDialog ofd = new OpenFileDialog();
        FolderBrowserDialog fbd = new FolderBrowserDialog();
        LuceneApp myLuceneApp = new LuceneApp();

        #region Button Functions

        private void BtnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void BtnClose_MouseHover(object sender, EventArgs e)
        {
            this.btnClose.BackColor = Color.FromArgb(52,184,214);
        }

        private void BtnClose_MouseLeave(object sender, EventArgs e)
        {
            this.btnClose.BackColor = Color.DimGray;
        }

        private void PanelSide_Paint(object sender, PaintEventArgs e)
        {
            
        }

        private void BtnBrowse_Click(object sender, EventArgs e)
        {
            ofd.Filter = "JSON|*.json";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                tbFName.Text = ofd.SafeFileName;
                ofd.Dispose();
            }
        }

        private void BtnDirectoryBr_Click(object sender, EventArgs e)
        {
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                tbDirectory.Text = fbd.SelectedPath;
                ResultsUpdate("Lucene Index Directory: " + tbDirectory.Text);
                fbd.Dispose();
            }
        }

        private void BtnIndex_Click(object sender, EventArgs e)
        {
            Form1 myForm = new Form1();
            string json = File.ReadAllText(ofd.FileName);
            string curPath = Directory.GetCurrentDirectory();

            List<records> storeRecords = DeserializeJSON(json);

            myLuceneApp.CreateIndex(tbDirectory.Text);

            System.DateTime IndexStart = DateTime.Now;
            for (int x = 0; x < storeRecords.Count; x++)
            {
                ResultsUpdate("Adding record no #"+ (x + 1));
                circularProgressBar1.Value = ((x + 1) / storeRecords.Count) * 100;
                circularProgressBar1.Update();
                for (int y = 0; y < storeRecords[x].passages.Count; y++)
                {
                    //WriteLine("URL: {0}", storeRecords[x].passages[y].url.ToString());
                    //WriteLine("Passage Text: {0}", storeRecords[x].passages[y].passage_text.ToString());
                    myLuceneApp.IndexText(storeRecords[x].passages[y].url, storeRecords[x].passages[y].passage_text);
                    //LuceneApp.IndexText(storeRecords[x].passages[y].passage_text);
                }
            }
            System.DateTime IndexEnd = DateTime.Now;
            TimeSpan span = (IndexEnd - IndexStart);
            ResultsUpdate("All documents added");
            ResultsUpdate("Total Index time: " + span.Minutes + "minute(s) " +
                span.Seconds + "seconds.");
        }

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            ResultsUpdate("Searching for " + tbSearchBar);

            myLuceneApp.CleanUpIndexer();
            myLuceneApp.CreateSearcher();
            myLuceneApp.CreateParser();
            //myLuceneApp.SearchIndex(DisplayTopDoc(tbSearchBar.Text));
            DisplayTopDoc(myLuceneApp.SearchIndex(tbSearchBar.Text));
        }
        #endregion

        const string TEXT_URL = "URL"; //Text Fields. For the column URL column.
        const string TEXT_PASSAGE = "Passage Text"; //Text fields. For the Passage Text column.
        private void DisplayTopDoc(Lucene.Net.Search.TopDocs results)
        {
            int rank = 0;
            var searcher = myLuceneApp.CreateSearcher();
            Lucene.Net.Documents.Document doc = null;

            foreach (ScoreDoc scoreDoc in results.ScoreDocs)
            {
                rank++;
                // retrieve the document from the 'ScoreDoc' object
                doc = searcher.Doc(scoreDoc.Doc);
                string field_URL = doc.Get(TEXT_URL).ToString();
                string field_Text = doc.Get(TEXT_PASSAGE).ToString();
                ResultsUpdate("Rank #" + rank);
                ResultsUpdate("\n");
                ResultsUpdate("Rank #" + rank);
                ResultsUpdate("URL: " + field_URL);
                ResultsUpdate("Passage Text: ");
                ResultsUpdate(field_Text);
                ResultsUpdate("\n");
            }
        }
        #region json functions

        public static List<records> DeserializeJSON(string jsonText)
        {
            Form1 myForm = new Form1();
            try
            {
                var jsonList = JsonConvert.DeserializeObject<List<records>>(jsonText);
                return jsonList;
            }
            catch (Exception ex)
            {
                myForm.ResultsUpdate("ERROR: " + ex.Message.ToString());
                return null;
            }

        }
        #endregion

        #region Display
        public void ResultsUpdate(string stringText)
        {
            try
            {
                System.Diagnostics.Debug.Write(stringText + Environment.NewLine);
                tbResults.Text = tbResults.Text + stringText + Environment.NewLine;
                tbResults.SelectionStart = tbResults.TextLength;
                tbResults.ScrollToCaret();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Write(ex.Message.ToString() + Environment.NewLine);
            }
        }



        #endregion

        #region Lucene Codes


        #endregion

        private void Form1_Load(object sender, EventArgs e)
        {
            circularProgressBar1.Value = 0;
            circularProgressBar1.Minimum = 0;
            circularProgressBar1.Maximum = 100;
        }
    }
}
