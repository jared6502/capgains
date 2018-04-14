using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace generate_8949
{
	public struct LedgerEntry
	{
		public string Security;
		public DateTime Day;
		public Decimal UnitQty;
		public Decimal UnitPrice;
	}

	public partial class MainForm : Form
	{
		public MainForm()
		{
			InitializeComponent();
		}

		private void StartCalc_Click(object sender, EventArgs e)
		{
			if (!File.Exists(SourceDataFileName.Text))
			{
				MessageBox.Show("Input data file doesn't exist!", "Can't open file", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			else
			{
				try
				{
					List<string> SourceData = new List<string>();

					using (StreamReader sr = new StreamReader(new FileStream(SourceDataFileName.Text, FileMode.Open)))
					{
						while (!sr.EndOfStream)
						{
							SourceData.Add(sr.ReadLine());
						}
					}

					ProcessDataset(SourceData.ToArray());
				}
				catch (Exception ex)
				{
					MessageBox.Show("Exception during calculation:\n\n" + ex.Message, "Calc Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
		}

		private void SelectFileButton_Click(object sender, EventArgs e)
		{
			OpenFileDialog ofd = new OpenFileDialog();

			if (ofd.ShowDialog() == DialogResult.OK)
			{
				SourceDataFileName.Text = ofd.FileName;
			}
		}

		private void ProcessDataset(string[] DataSet)
		{
			List<LedgerEntry> Buys = new List<LedgerEntry>();
			List<LedgerEntry> Sells = new List<LedgerEntry>();

			// process data set into a list of buys and sells
			foreach (string row in DataSet)
			{
				string[] cells = row.Split(',');

				LedgerEntry entry = new LedgerEntry();

				if (cells.Length > 3 &&
					DateTime.TryParseExact(cells[1], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out entry.Day) &&
					Decimal.TryParse(cells[2], out entry.UnitQty) &&
					Decimal.TryParse(cells[3], out entry.UnitPrice))
				{
					(entry.UnitPrice >= 0 ? Buys : Sells).Add(entry);
				}
			}

			// sort the orders
			Buys.Sort((x, y) => x.Day.CompareTo(y.Day));
			Sells.Sort((x, y) => x.Day.CompareTo(y.Day));


		}
	}
}
