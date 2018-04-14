using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace generate_8949
{
	public class LedgerEntry
	{
		public string Security;
		public DateTime Day;
		public Decimal UnitQty;
		public Decimal UnitPrice;
	}

	public class CapGainEntry
	{
		public string Security;
		public DateTime AcqDate;
		public DateTime SaleDate;
		public Decimal UnitQty;
		public Decimal UnitBuyPrice;
		public Decimal UnitSellPrice;
		public Decimal CostBasis;
		public Decimal Proceeds;
		public Decimal CapitalGain;
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
					entry.Security = cells[0];

					entry.UnitQty = Decimal.Round(entry.UnitQty, 4, MidpointRounding.AwayFromZero);
					entry.UnitPrice = Decimal.Round(entry.UnitPrice, MidpointRounding.AwayFromZero);

					if (entry.UnitQty < 0)
					{
						entry.UnitQty *= -1;
						Sells.Add(entry);
					}
					else
					{
						Buys.Add(entry);
					}
				}
			}

			// sort the orders
			Buys.Sort((x, y) => x.Day.CompareTo(y.Day));
			Sells.Sort((x, y) => x.Day.CompareTo(y.Day));

			List<CapGainEntry> CapitalGains = new List<CapGainEntry>();

			foreach (LedgerEntry sale in Sells)
			{
				while (sale.UnitQty > 0)
				{
					CapGainEntry entry = new CapGainEntry();

					entry.Security = sale.Security;

					entry.UnitBuyPrice = Buys[0].UnitPrice;
					entry.AcqDate = Buys[0].Day;

					entry.UnitSellPrice = sale.UnitPrice;
					entry.SaleDate = sale.Day;

					if (sale.UnitQty > Buys[0].UnitQty)
					{
						sale.UnitQty -= Buys[0].UnitQty;
						entry.UnitQty = Buys[0].UnitQty;
						Buys.RemoveAt(0);
					}
					else
					{
						entry.UnitQty = sale.UnitQty;
						Buys[0].UnitQty -= sale.UnitQty;
						sale.UnitQty = 0;
					}

					entry.CostBasis = entry.UnitQty * entry.UnitBuyPrice;
					entry.Proceeds = entry.UnitQty * entry.UnitSellPrice;
					entry.CapitalGain = entry.Proceeds - entry.CostBasis;

					CapitalGains.Add(entry);
				}
			}

			List<CapGainEntry> ShortTerm = new List<CapGainEntry>();
			List<CapGainEntry> LongTerm = new List<CapGainEntry>();

			foreach (CapGainEntry ce in CapitalGains)
			{
				if (ce.SaleDate.Year > ce.AcqDate.Year + 1 ||
					(ce.SaleDate.Year > ce.AcqDate.Year &&
					ce.SaleDate.Month >= ce.AcqDate.Month &&
					ce.SaleDate.Day >= ce.AcqDate.Day))
				{
					LongTerm.Add(ce);
				}
				else
				{
					ShortTerm.Add(ce);
				}
			}

			List<string> ltg = new List<string>();
			foreach (CapGainEntry ce in LongTerm)
			{
				ltg.Add(CapGainRow(ce));
			}
			SaveTextFile(ltg.ToArray());
			
			List<string> stg = new List<string>();
			foreach (CapGainEntry ce in ShortTerm)
			{
				stg.Add(CapGainRow(ce));
			}
			SaveTextFile(stg.ToArray());

			List<string> remainder = new List<string>();
			foreach (LedgerEntry le in Buys)
			{
				remainder.Add(LedgerRow(le));
			}
			SaveTextFile(remainder.ToArray());
		}

		private void SaveTextFile(string[] text)
		{
			SaveFileDialog sfd = new SaveFileDialog();
			if (sfd.ShowDialog() == DialogResult.OK)
			{
				using (StreamWriter sw = new StreamWriter(new FileStream(sfd.FileName, FileMode.Create)))
				{
					foreach (string s in text)
					{
						sw.WriteLine(s);
					}
				}
			}
		}

		private string CapGainRow(CapGainEntry ce)
		{
			return
				ce.UnitQty.ToString() + " " + ce.Security + "," +
				DateToText(ce.AcqDate) + "," +
				DateToText(ce.SaleDate) + "," +
				ce.Proceeds.ToString() + "," +
				ce.CostBasis.ToString() + "," +
				(ce.CapitalGain >= 0 ? ce.CapitalGain.ToString() : "(" + (ce.CapitalGain * -1).ToString() + ")");
		}

		private string LedgerRow(LedgerEntry le)
		{
			return
				le.Security + "," +
				DateToText(le.Day) + "," +
				le.UnitQty.ToString() + "," +
				le.UnitPrice.ToString();
		}

		private string DateToText(DateTime dt)
		{
			return dt.Year.ToString("0000") + "-" + dt.Month.ToString("00") + "-" + dt.Day.ToString("00");
		}
	}
}
